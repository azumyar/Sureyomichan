using Haru.Kei.SureyomiChan.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Haru.Kei.SureyomiChan.Core;

class FutabaFeature : ISureyomiChanFeature {
	public bool IsSupportThreadOld => true;

	public bool IsSupportThreadDie => true;
}

class FutabaInteraction(string url, Models.__FutabaResData source, Helpers.FutabaApi api, IConfigProxy config) : Models.ISureyomiChanInteraction {
	public string BoardName => "img";
	public bool IsSupportSendDel => true;
	public bool IsSupportDeleteRes => true;

	public async Task<bool> DeleteResAction()
		=> await Utils.Util.AwaitObserver(api.PostDeleteSerial(source.ResNoInt, config.Get().FutabaPasswd), false);

	public async Task<bool> SendDelAction()
		=> await Utils.Util.AwaitObserver(api.PostDelSerial(url, source.ResNoInt), false);

	public async Task<AttachmentObject> DownloadImage() {
		var name = Path.GetFileName(source.FileSource);
		var image = await api.DonwloadImage(source);
		return new AttachmentObject() {
			IsUpdatedTegakiPng = true,
			FileName = name,
			ImageName = name,
			OriginalFileBytes = image,
			ImageFileBytes = image,
		};
	}
}

class NijiuraChanFeature : ISureyomiChanFeature {
	public bool IsSupportThreadOld => false;

	public bool IsSupportThreadDie => false;
}

class NijiuraChanInteraction(string url, Models.NijiuraChanReplyV1  source, Helpers.NijiuraChanApi? api, IConfigProxy config) : Models.ISureyomiChanInteraction {
	public string BoardName => "aimg";
	public bool IsSupportSendDel => false;
	public bool IsSupportDeleteRes => false;

	public async Task<bool> DeleteResAction()
		=> await Task.FromResult(false);

	public async Task<bool> SendDelAction()
		=> await Task.FromResult(false);

	public async Task<AttachmentObject> DownloadImage() {
		var fileName = source.Image ?? "";
		var imageName = source.Image ?? "";
		var orig = default(byte[]);
		var image = default(byte[]);
		if(!string.IsNullOrEmpty(fileName)) {
			var httpClient = Utils.Singleton.Instance.HttpClient;
			var origUrl = $"https://nijiurachan.net/{source.Image}";
			var thumbUrl = $"https://nijiurachan.net/{source.Thumb}";

			using var response1 = await Utils.Util.Http(() => httpClient.GetAsync(origUrl));
			orig = await response1.Content.ReadAsByteArrayAsync();
			if(Path.GetExtension(fileName).ToLower() switch {
				".mp4" => true,
				".webm" => true,
				_ => false,
			}) {
				using var response2 = await Utils.Util.Http(() => httpClient.GetAsync(thumbUrl));
				image = await response2.Content.ReadAsByteArrayAsync();
				imageName = source.Thumb ?? "";
			} else {
				image = orig;
			}
		}

		return await Task.FromResult(new AttachmentObject() {
			IsUpdatedTegakiPng = Path.GetExtension(fileName).ToLower() == ".png",
			FileName = Path.GetFileName(fileName),
			ImageName = Path.GetFileName(imageName),
			OriginalFileBytes = orig,
			ImageFileBytes = image,
		});
	}
}

class SureyomiChanApiLooper : IDisposable {
	interface IWorker {
		IObservable<Models.SureyomiChanResponse> GetThread(int? latestResNo);
	}

	class FutabaApiWorker : IWorker {
		private readonly Helpers.FutabaApi api;
		private readonly IConfigProxy config;
		private readonly string urlString;
		private readonly int threadNo;
		private readonly string threadNoTxt = "threadno.img.txt";

		public FutabaApiWorker(string urlString, int threadNo, IConfigProxy config) {
			this.urlString = urlString;
			this.threadNo = threadNo;
			this.api = Utils.Singleton.Instance.FutabaApi;
			this.config = config;
		}

		public IObservable<Models.SureyomiChanResponse> GetThread(int? latestResNo) {
			static string? stringnull(string s) => s switch {
				{ } v when !string.IsNullOrEmpty(v) => v,
				_ => null,
			};

			SureyomiChanModel toModel(Models.__FutabaResData source) => new(
				threadNo: this.threadNo,
				resIndex: source.ResCount,
				no: source.ResNoInt,
				postTime: DateTime.Now,
				email: source.Email,
				body: source.Comment,
				id: string.IsNullOrEmpty(source.Id) switch {
					true => null,
					_ => source.Id
				},
				deleteType: source.DeleteType,

				imageFileName: source.FileSource switch {
					string v when !string.IsNullOrEmpty(v) => Path.GetFileName(v),
					_ => null,
				},
				imageSource: stringnull(source.FileSource),
				thumbnailSource: stringnull(source.FileThumb),

				token: Utils.Util.Tokenize(source.FormatBody()),
				interaction: new FutabaInteraction(this.urlString, source, this.api, this.config));

			return this.api.GetThreadSerial(this.threadNo, latestResNo)
				.Select(x => new Models.SureyomiChanResponse() {
					ThreadNo = threadNo,
					ThreadNoTxt = threadNoTxt,
					IsAlive = x.NowDateTime < x.DieDateTime,
					CurrentTime = x.NowDateTime,
					DieTime = x.DieDateTime,
					NewReplies = x.Res.Select(x => toModel(x)).ToArray(),
					SupportFeature = new FutabaFeature(),
				});
		}
	}


	class NijiuraChanApiWorker : IWorker {
		private readonly IConfigProxy config;
		private readonly string urlString;
		private readonly int threadNo;
		private readonly string threadNoTxt = "threadno.aimg.txt";
		private readonly WebView2Proxy webView;

		public NijiuraChanApiWorker(string urlString, int threadNo, IConfigProxy config, WebView2Proxy webView) {
			this.urlString = urlString;
			this.threadNo = threadNo;
			this.config = config;

			this.webView = webView;
		}

		public IObservable<SureyomiChanResponse> GetThread(int? latestResNo) {
			var url = Utils.Singleton.Instance.NijiuraChanUrl.GenApiThread(this.threadNo, latestResNo);
			return Observable.Return(url)
				.ObserveOn(System.Reactive.Concurrency.ThreadPoolScheduler.Instance)
				.Select(x => {
					var nowTime = DateTime.Now;
					var task = this.webView.__RequestApi(url);
					task.Wait();
					var json = task.Result;
					
					IEnumerable<Models.NijiuraChanReplyV1> replies;
					if(latestResNo is { }) {
						var o = JsonSerializer.Deserialize<Models.NijiuraChanResponse<Models.NijiuraChanNewThreadDataV1>>(json);
						replies = o?.Data?.Replies ?? Array.Empty<Models.NijiuraChanReplyV1>();
					} else {
						var o = JsonSerializer.Deserialize<Models.NijiuraChanResponse<Models.NijiuraChanThreadDataV1>>(json);
						replies = o?.Data?.Replies ?? Array.Empty<Models.NijiuraChanReplyV1>();
					}

					SureyomiChanModel toModel(Models.NijiuraChanReplyV1 source) => new(
						threadNo: this.threadNo,
						resIndex: source.Number,
						no: source.Id,
						postTime: source.CreatedAtDateTime,
						email: "",
						body: source.Body,
						id: string.IsNullOrEmpty(source.PosterId) switch {
							true => null,
							_ => source.PosterId
						},
						deleteType: Models.SureyomiChanDeleteType.None,

						imageFileName: source.Image switch {
							string v when !string.IsNullOrEmpty(v) => Path.GetFileName(v),
							_ => null,
						},
						imageSource: source.Image,
						thumbnailSource: source.Thumb,

						token: Utils.Util.Tokenize(source.FormatBody()),
						interaction: new NijiuraChanInteraction(this.urlString, source, null, this.config));

					return new Models.SureyomiChanResponse() {
						ThreadNo = threadNo,
						ThreadNoTxt = threadNoTxt,
						IsAlive = true,
						CurrentTime = nowTime,
						DieTime = nowTime.AddHours(1),
						NewReplies = replies.Select(x => toModel(x)).ToArray() ?? new SureyomiChanModel[0],
						SupportFeature = new NijiuraChanFeature(),
					};
				});
		}
	}

	private readonly UiMessageDispatcher uiMsgDispatcher;
	private readonly IConfigProxy config;
	private readonly CancellationTokenSource cancel;
	private readonly IWorker worker;
	private IDisposable? runSubscriber = null;
	private bool isDisposed = false;

	public SureyomiChanApiLooper(string urlString, Helpers.IApiUrl url, int threadNo, UiMessageDispatcher uiMsgDispatcher, IConfigProxy config, WebView2Proxy webView) {
		this.uiMsgDispatcher = uiMsgDispatcher;
		this.config = config;

		this.cancel = new();
		this.worker = url switch {
			Helpers.FutabaUrl => new FutabaApiWorker(urlString, threadNo, this.config),
			Helpers.NijiuraChanUrl => new NijiuraChanApiWorker(urlString, threadNo, this.config, webView),
			_ => throw new NotSupportedException()
		};
		Utils.Logger.Instance.Info($"ApiLooperの作成完了 => url={url.GetType().Name}, worker={this.worker.GetType().Name}");
	}

	public void Dispose() {
		if(this.isDisposed) {
			return; 
		}

		this.runSubscriber?.Dispose();
		this.cancel?.Cancel();
		this.cancel?.Dispose();
		this.isDisposed = true;
	}


	public void Run(Func<Models.SureyomiChanResponse, bool, Task> callBack, bool skipToLast, int? latestResNo) {
		this.runSubscriber = Observable.Create<int>(async o => {
			await Task.Run(async () => {
				int? latestNo = latestResNo;
				bool skip = skipToLast;
				while (!this.cancel.IsCancellationRequested) {
					Utils.Logger.Instance.Info($"API呼び出しを開始 => worker={this.worker.GetType().Name}, latestNo={latestNo}, skip={skip}");
					uiMsgDispatcher.DispatchBeginGetApi();

					worker.GetThread(latestNo)
						.Subscribe(async x => {
							Utils.Logger.Instance.Info($"API呼び出しが成功");
							uiMsgDispatcher.DispatchEndGetApi(true, x);

							if (x.NewReplies.LastOrDefault() is { } it) {
								latestNo = it.No;
							}

							await callBack(x, skip);
							skip = false;
						}, ex => {
							uiMsgDispatcher.DispatchEndGetApi(false, null);
							Utils.Logger.Instance.Error(ex);
						});
					await Task.Delay(5000);
				}
			}, this.cancel.Token);
		}).Subscribe();
	}
}