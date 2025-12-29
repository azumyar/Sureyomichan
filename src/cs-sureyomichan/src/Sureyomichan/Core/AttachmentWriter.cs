using ControlzEx.Standard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Haru.Kei.SureyomiChan.Core; 
class AttachmentWriter {
	private readonly Helpers.SerialRunner downloadRunner;

	private readonly string saveFileNamePng = "tegaki.png";
	private readonly string saveFileNameHtml = "tegaki.html";
	private readonly IConfigProxy config;

	private readonly UiMessageMultiDispatcher uiDispatcher;

	private Dictionary<string, int> currentThreadId = new();
	private object downloadToken = new();

	public AttachmentWriter(IConfigProxy config, UiMessageMultiDispatcher uiDispatcher) {

		static int intervalMiliSec(IConfigProxy config) => config.Get().TegakiRotateTime;

		this.config = config;
		this.uiDispatcher = uiDispatcher;


		this.downloadRunner = new(intervalMiliSec(config));
		this.downloadRunner.Sleep += (_, _) => {
			var token = new object();
			this.downloadToken = token;
			Observable.Return(token)
				.Delay(TimeSpan.FromMilliseconds(intervalMiliSec(config)))
				.Subscribe(x => {
					if (object.ReferenceEquals(x, this.downloadToken)) {
						this.uiDispatcher.Dispatch(x => x.DispatchEndDisplayTegakiPng());

						if (config.Get().TegakiRemoveEnabled) {
							Utils.Logger.Instance.Info("手書きを消去します");
							try {
								var filePath = Path.Combine(this.config.Get().PathDwonloadValue, this.saveFileNamePng);
								var bmp = new RenderTargetBitmap(16, 16, 96.0d, 96.0d, PixelFormats.Pbgra32);

								using var fs = new System.IO.FileStream(filePath, FileMode.OpenOrCreate);
								var enc = new PngBitmapEncoder();
								enc.Frames.Add(BitmapFrame.Create(bmp));
								enc.Save(fs);
							}
							catch (Exception ex) {
								Utils.Logger.Instance.Error(ex);
							}
						}
					}
				});

		};
	}
	public async Task UpdateThreadNo(Models.SureyomiChanResponse response) {
		if(this.currentThreadId.ContainsKey(response.ThreadNoTxt)) {
			if(this.currentThreadId[response.ThreadNoTxt] == response.ThreadNo) {
				return;
			} else {
				this.currentThreadId[response.ThreadNoTxt] = response.ThreadNo;
			}
		} else {
			this.currentThreadId.Add(response.ThreadNoTxt, response.ThreadNo);
		}

		if(this.config.Get().SaveThreadNoEnabled) {
			Utils.Logger.Instance.Info($"{response.ThreadNoTxt}を更新 => {response.ThreadNo}");
			await File.WriteAllBytesAsync(
				Path.Combine(this.config.Get().PathDwonloadValue, response.ThreadNoTxt),
				Encoding.UTF8.GetBytes($"{response.ThreadNo}"));
		} else {
			await Task.Yield();
		}
	}

	public async Task DeadThreadNo(Models.SureyomiChanResponse response) {
		if(this.currentThreadId.ContainsKey(response.ThreadNoTxt)) {
			if(this.currentThreadId[response.ThreadNoTxt] == response.ThreadNo) {
				this.currentThreadId.Remove(response.ThreadNoTxt);
			} else {
				return;
			}
		} else {
			return;
		}

		if(this.config.Get().SaveThreadNoEnabled && this.config.Get().ChangeThreadNoTxtEnabled) {
			Utils.Logger.Instance.Info($"{response.ThreadNoTxt}を更新 => スレ落ち");
			await File.WriteAllBytesAsync(
				Path.Combine(this.config.Get().PathDwonloadValue, response.ThreadNoTxt),
				Encoding.UTF8.GetBytes($"{this.config.Get().ChangeThreadNoTxtText}"));
		} else {
			await Task.Yield();
		}
	}

	public async Task Save(Models.SureyomiChanModel model, Models.AttachmentObject attachment) {
		this.downloadToken = new();
		this.uiDispatcher.Dispatch(x => x.DispatchStartDisplayTegakiPng());

		var orig = attachment.OriginalFileBytes;
		var image = attachment.ImageFileBytes;

		if(orig is { } && image is { }) {
			if(attachment.IsUpdatedTegakiPng) {
				var tegakiPngPath = Path.Combine(
					this.config.Get().PathDwonloadValue,
					this.saveFileNamePng);
				downloadRunner.Dispatch(async () => {
					static Task savePng(string path, byte[] or, byte[] im) {
						return File.WriteAllBytesAsync(path, im);
					}
					static Task saveJpgGif(string path, byte[] or, byte[] im) {
						return Task.Run(() => {
							using var m = new MemoryStream(im);
							using var bmp = System.Drawing.Bitmap.FromStream(m);
							bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
						});
					}
					static Task saveWebP(string path, byte[] or, byte[] im) {
						return Task.Run(() => {

						});
					}
					static Task saveMovie(string path, byte[] or, byte[] im) {
						return Task.Run(() => {

						});
					}
					static Task unknown(string fileNmae) {
						return Task.Run(() => {
							Utils.Logger.Instance.Error($"不明な添付ファイル => {fileNmae}");
						});
					}

					Utils.Logger.Instance.Info($"手書きを保存します => {attachment.FileName}");
					try {
						var task = Path.GetExtension(attachment.FileName).ToLower() switch {
							string v when v == ".png" => savePng(tegakiPngPath, orig, image),
							string v when v == ".jpg" => saveJpgGif(tegakiPngPath, orig, image),
							string v when v == ".jpeg" => saveJpgGif(tegakiPngPath, orig, image),
							string v when v == ".gif" => saveJpgGif(tegakiPngPath, orig, image),
							string v when v == ".webp" => saveWebP(tegakiPngPath, orig, image),
							string v when v == ".mp4" => saveMovie(tegakiPngPath, orig, image),
							string v when v == ".webm" => saveMovie(tegakiPngPath, orig, image),
							_ => unknown(attachment.FileName),
						};
						await task;
						//await this.SaveHtml((model, attachment));
					}
					catch(Exception ex) when(ex is System.IO.IOException) {
						Utils.Logger.Instance.Error(ex);
					}
					return 0;
				}).Subscribe();
			}
			if(config.Get().IsEnabledAttacmentFile) {
				await Task.Run(async () => {
					Utils.Logger.Instance.Info($"オリジナルを保存します => {attachment.FileName}");
					string saveRoot = GetSaveDirectoryWithCreate(model);

					// 作成出来ている場合保存
					if(Directory.Exists(saveRoot)) {
						try {
							await File.WriteAllBytesAsync(Path.Combine(saveRoot, attachment.FileName), orig);
						}
						catch(Exception ex) when(ex is System.IO.IOException) {
							Utils.Logger.Instance.Error(ex);
						}
					} else {
						Utils.Logger.Instance.Error("保存フォルダが存在しません");
					}
				});
			}
		}
	}


	/// <summary>アップローダからファイルをダウンロード</summary>
	/// <param name="model"></param>
	/// <returns></returns>
	public async Task DownloadShio(Models.SureyomiChanModel model) {
		static string? url(string bottle, string fileName) =>  bottle switch {
			"fu" => $"https://dec.2chan.net/up2/src/{fileName}",
			"f" => $"https://dec.2chan.net/up/src/{fileName}",
			_ => null
		};

		// ファイルリストの作成
		var matches = new List<Match>();
		foreach(var line in model.FormatBody().Split("\n")) {
			if((0 < line.Length) && (line[0] != '>')) {
				matches.AddRange(Regex.Matches(line, @"(fu|f)[0-9]+\.([a-zA-Z0-9]{1,4})").Cast<Match>());
			}
		}
		if(matches.Count == 0) {
			await Task.Yield();
			return;
		}

		string saveRoot = GetSaveDirectoryWithCreate(model);

		// 作成出来ている場合保存
		if(Directory.Exists(saveRoot)) {
			foreach(var m in matches) {
				var fileName = m.Value;
				var filePath = Path.Combine(saveRoot, m.Value);
				var u = url(m.Groups[1].Value, fileName);

				if(u == null) {
					Utils.Logger.Instance.Info($"{fileName}からURLが復元できませんでした。スキップします。");
					continue;
				}

				if(File.Exists(filePath)) {
					Utils.Logger.Instance.Info($"すでに{fileName}は存在します。ダウンロードをスキップします。");
					continue;
				}

				Utils.Logger.Instance.Info($"ファイルをダウンロードします => {u}");
				try {
					using var r = await Utils.Util.Http(() => Utils.Singleton.Instance.HttpClient.GetAsync(u));
					using var rs = await r.Content.ReadAsStreamAsync();
					using var ws = new FileStream(filePath, FileMode.OpenOrCreate);
					var b = new byte[1024];
					while((await rs.ReadAsync(b, 0, b.Length)) is int size && (0 < size)) {
						await ws.WriteAsync(b, 0, size);
					}
					await ws.FlushAsync();
					Utils.Logger.Instance.Info($"ファイルをダウンロードしました => {u}");
					await Task.Delay(500);
				}
				catch(Exception ex) when(ex is System.IO.IOException) {
					Utils.Logger.Instance.Error($"ファイルの書き込みに失敗しました => {fileName}");
					Utils.Logger.Instance.Error(ex);
				}
				catch(Exception ex) when((ex is Exceptions.ApiHttpConnectionException)
					|| (ex is Exceptions.ApiHttpConnectionException)) {

					Utils.Logger.Instance.Error(ex);
				}
			}
		} else {
			Utils.Logger.Instance.Error("保存フォルダが存在しません");
		}
	}
	private string GetSaveDirectoryWithCreate(Models.SureyomiChanModel model) {
		string saveRoot = this.config.Get().PathDwonloadValue;
		var fd = this.GetSaveDirectoryName(model);
		if(!string.IsNullOrWhiteSpace(fd)) {
			saveRoot = Path.Combine(saveRoot, fd);
		}

		// 存在しない場合フォルダを作る
		if(!Directory.Exists(saveRoot)) {
			Utils.Logger.Instance.Info($"保存フォルダを作成します => {fd}");
			try {
				Directory.CreateDirectory(saveRoot);
			}
			catch(Exception) { }
		}
		return saveRoot;
	}

	private async Task SaveHtml((Models.SureyomiChanModel Model, Models.AttachmentObject Attachment)? arg) {
		// OBS用のtegaki.htmlを出力
		// 検討中
		string? src() {
			if(arg is { } a) {
				var dir = this.GetSaveDirectoryName(a.Model);
				return string.IsNullOrEmpty(dir) switch {
					true => a.Attachment.FileName,
					false => $"{dir}/{a.Attachment.FileName}",
				};
			} else {
				return null;
			}
		}

		var tegakiHtmlPath = Path.Combine(
			this.config.Get().PathDwonloadValue,
			this.saveFileNameHtml);

		await File.WriteAllBytesAsync(
			tegakiHtmlPath,
			Encoding.UTF8.GetBytes(this.ToHtml(src())));
	}

	private string GetSaveDirectoryName(Models.SureyomiChanModel model) {
		var sb = new StringBuilder(this.config.Get().SaveSubFolderName);
		sb.Replace("$Board", $"{model.Interaction.BoardName}");
		sb.Replace("$Thread", $"{model.ThreadNo}");
		return sb.ToString();
	}


	private string ToHtml(string? src) {
		static string nopHtml() => $@"<!doctype html>
<html>
<head>
<meta http-equiv=""refresh"" content=""3"">
</head>
<body>
</body>
</html>";
		static string imageHtml(string src) => $@" <!doctype html>
<html>
<head>
<meta http-equiv=""refresh"" content=""3"">
<meta charset=""utf-8"">
<style type=""text/css"">
* {{
  margin: 0;
  padding: 0;
}}

.view {{
  width: 100vw;
  height: 100vh;
}}
.view img {{
  object-fit: contain;
  width: 100%;
  height: 100%;
}}
</style>
</head>
<body>
<div class=""view""><img src=""{src}""></div>
</body>
</html>";

		return src switch {
			{ } => imageHtml(src),
			_ => nopHtml(),
		};
	}

}
