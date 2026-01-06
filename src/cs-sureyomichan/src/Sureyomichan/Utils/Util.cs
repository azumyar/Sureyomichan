using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Ja;
using Lucene.Net.Analysis.Ja.Dict;
using Lucene.Net.Analysis.Ja.TokenAttributes;
using Lucene.Net.Analysis.TokenAttributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace Haru.Kei.SureyomiChan.Utils;
//
// 雑多なユーティリティクラス
//

static class Util {
	/// <summary>UNIX時間(秒)から変換</summary>
	/// <param name="t"></param>
	/// <returns></returns>
	public static DateTime FromUnixTimeSeconds(long t)
		=> new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
			.AddSeconds(t)
			.ToLocalTime();

	/// <summary>UNIX時間(ミリ秒)から変換</summary>
	/// <param name="t"></param>
	/// <returns></returns>
	public static DateTime FromUnixTimeMiliSeconds(long t)
		=> new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
			.AddMilliseconds(t)
			.ToLocalTime();

	/// <summary>UNIX時間(秒)に変換</summary>
	/// <param name="d"></param>
	/// <returns></returns>
	public static long ToUnixTimeSeconds(DateTime d) {
		var offest = TimeZoneInfo.Local.GetUtcOffset(d);
		return new DateTimeOffset(d).ToUnixTimeMilliseconds()
				- (((offest.Hours * 3600) + (offest.Minutes * 60) + offest.Seconds));
	}

	public static string FormatFutabaDateTime(DateTime d) {
		var date = d.ToString("yy/MM/dd");
		var dw = d.DayOfWeek switch {
			DayOfWeek.Sunday => "日",
			DayOfWeek.Monday => "月",
			DayOfWeek.Tuesday => "火",
			DayOfWeek.Wednesday => "水",
			DayOfWeek.Thursday => "木",
			DayOfWeek.Friday => "金",
			DayOfWeek.Saturday => "土",
			_ => "？"
		};
		var time = d.ToString("HH:mm:ss");
		return $"{date}({dw}){time}";
	}

	/// <summary>
	/// HTTP API呼び出しの定型処理
	/// </summary>
	/// <param name="http"></param>
	/// <returns></returns>
	/// <exception cref="Exceptions.ApiHttpErrorException"></exception>
	/// <exception cref="Exceptions.ApiHttpConnectionException"></exception>
	// TODO: あとで名前考える
	public static async Task<HttpResponseMessage> Http(Func<Task<HttpResponseMessage>> http) {
		var url = "--";
		try {
			var r = await http();
			url = r.RequestMessage?.RequestUri?.ToString() ?? url;
			r.EnsureSuccessStatusCode();
			return r;
		}
		catch (HttpRequestException ex) {
			throw new Exceptions.ApiHttpErrorException(url, ex);
		}
		catch (Exception ex) when (ex is SocketException || ex is TimeoutException) {
			throw new Exceptions.ApiHttpConnectionException(ex);
		}
	}

	public static async Task<T> AwaitObserver<T>(IObservable<T> o, T defaultValue)
			=> await Task.Run(async () => {
				T result = defaultValue;
				var ev = new AutoResetEvent(false);
				o.Subscribe(
					x => {
						result = x;
						ev.Set();
					}, ex => {
						throw ex;
					});
				ev.WaitOne();
				return result;
			});

	public static Task<int> AddCustomUrlScheme(string name, string exePath, nint hwnd)
		=> DoCustomUrlScheme(
			@$"/c reg add ""HKEY_CLASSES_ROOT\{name}"" /v ""URL Protocol"" /t ""REG_SZ"" /f  & reg add ""HKEY_CLASSES_ROOT\{name}\shell\open\command"" /t ""REG_SZ"" /d ""{exePath} %1"" /f",
			hwnd);

	public static Task<int> RemoveCustomUrlScheme(string name, nint hwnd)
		=> DoCustomUrlScheme(
			@$"/c reg delete ""HKEY_CLASSES_ROOT\{name}"" /f",
			hwnd);

	private static async Task<int> DoCustomUrlScheme(string arg, nint hwnd) {
		var psi = new System.Diagnostics.ProcessStartInfo() {
			UseShellExecute = true,
			FileName = "cmd.exe",
			Verb = "runas",
			Arguments = arg,

			ErrorDialog = true,
			ErrorDialogParentHandle = hwnd,
		};

		try {
			return await Task.Run(() => {
				using var p = System.Diagnostics.Process.Start(psi);
				p?.WaitForExit();
				return p?.ExitCode ?? 255;
			});
		}
		catch(System.ComponentModel.Win32Exception e) {
			Logger.Instance.Error(e);
			return await Task.FromResult(255);
		}
	}

	public static (string Board, int ThreadNo, bool IsLatest)? ParseCommandLine(string cmd) {
		var span = cmd.AsSpan();
		// argv[0]を削除
		if(span[0] == '\"') {
			span = span.Slice(1);
			span = span.Slice(span.IndexOf('\"') + 1);
		} else {
			span = span.IndexOf(' ') switch {
				int v when 0 < v => span.Slice(v + 1),
				_ => span.Slice(span.Length),
			};
		}

		// argv[1]を取り出し
		span = span.IndexOf(' ') switch {
			int v when 0 < v => span.Slice(0, v),
			_ => span
		};
		if(span.Length == 0) {
			return null;
		}

		Logger.Instance.Info($"コマンドラインを解析します => {span}");
		var uri = new Uri(span.ToString());
		if(uri.Scheme == SureyomiChanEnviroment.Scheme) {
			if(!SureyomiChanEnviroment.SupportCommands.Where(x => x == uri.Host).Any()) {
				goto error;
			}

			if(uri.Host == SureyomiChanEnviroment.CommandOpen) {
				var p = uri.LocalPath.Split("/");
				if(p.Length == 3) {
					var board = "";
					var no = 0;
					var latest = false;
					if(!SureyomiChanEnviroment.SupportBoards.Where(x => x == p[1]).Any()) {
						goto error;
					}
					board = p[1];

					if(!uint.TryParse(p[2], out var uno)) {
						goto error;
					}
					no = (int)uno;
					if(1 < uri.Query.Length) {
						foreach(var it in uri.Query.Substring(1).Split('&')) {
							if($"{it}" == "latest") {
								latest = true;
							}
						}
					}
					Logger.Instance.Info($"コマンドラインを解析しました => {board}, {no}, {latest}");
					return (board, no, latest);
				}
			}
		}
	error:
		Logger.Instance.Info($"コマンドラインは不正でした => {span}");
		return null;
	}

	public static IEnumerable<Models.Token> Tokenize(string text) {
		var s_userDictionary = new UserDictionary(new StringReader("日本経済新聞,日本 経済 新聞,ニホン ケイザイ シンブン,カスタム名詞"));
		using var reader = new StringReader(text);
		using var tokenizer = new JapaneseTokenizer(reader, s_userDictionary, true, JapaneseTokenizerMode.SEARCH);

		using var ts = new TokenStreamComponents(tokenizer, tokenizer).TokenStream;
		ts.Reset();
		var r = new List<Models.Token>();
		while(ts.IncrementToken()) {
			var startOffset = 0;
			var endOffset = 0;
			var term = "";
			var partOfSpeech = "";

			if(ts.HasAttribute<IOffsetAttribute>()
				&& ts.GetAttribute<IOffsetAttribute>() is { } offsetAtt) {

				startOffset = offsetAtt.StartOffset;
				endOffset = offsetAtt.EndOffset;
			}

			if(ts.HasAttribute<IPartOfSpeechAttribute>() && ts.GetAttribute<IPartOfSpeechAttribute>() is { } prtAtt) {
				partOfSpeech = prtAtt.GetPartOfSpeech();
			}

			term = ts.GetAttribute<ICharTermAttribute>().ToString();
			r.Add(new() {
				Term = term,
				PartOfSpeech = partOfSpeech,
				StartOffset = startOffset,
				EndOffset = endOffset,
			});
		}
		return r.AsReadOnly();
	}
}

