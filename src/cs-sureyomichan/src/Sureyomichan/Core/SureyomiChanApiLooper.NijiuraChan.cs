using Haru.Kei.SureyomiChan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json;

namespace Haru.Kei.SureyomiChan.Core; 
partial class SureyomiChanApiLooper {
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

					return new Models.SureyomiChanResponse() {
						ThreadNo = threadNo,
						ThreadNoTxt = threadNoTxt,
						IsAlive = true,
						CurrentTime = nowTime,
						DieTime = nowTime.AddHours(1),
						NewReplies = replies.Select(x => x.ToSureyomiChanModel(this.threadNo, new NijiuraChanInteraction(this.urlString, x, null, this.config))).ToArray() ?? new SureyomiChanModel[0],
						SupportFeature = new NijiuraChanFeature(),
					};
				});
		}
	}


	// 非公開API

	class NijiuraChanInternalApiWorker : IWorker {
		private readonly IConfigProxy config;
		private readonly string urlString;
		private readonly int threadNo;
		private readonly string threadNoTxt = "threadno.aimg.txt";
		private readonly WebView2Proxy webView;

		public NijiuraChanInternalApiWorker(string urlString, int threadNo, IConfigProxy config, WebView2Proxy webView) {
			this.urlString = urlString;
			this.threadNo = threadNo;
			this.config = config;

			this.webView = webView;
		}

		public IObservable<SureyomiChanResponse> GetThread(int? latestResNo) {
			static string getApi(int threadNo, int? latestResNo) {
				return $"https://nijiurachan.net/api/thread/{threadNo}";
			}

			var url = getApi(this.threadNo, latestResNo);
			return Observable.Return(url)
				.ObserveOn(System.Reactive.Concurrency.ThreadPoolScheduler.Instance)
				.Select(x => {
					var nowTime = DateTime.Now;
					var task = this.webView.__RequestApi(url);
					task.Wait();
					var json = task.Result;
					var o = JsonSerializer.Deserialize<Models.NijiuraChanResponse<Models.NijiuraChanThreadInternalData>>(json);

					IEnumerable<Models.NijiuraChanPostInternal> replies;
					if(latestResNo is { } lno) {
						replies = o?.Data?.Posts.Where(x => lno < x.Id).ToArray() ?? Array.Empty<Models.NijiuraChanPostInternal>();
					} else {
						replies = o?.Data?.Posts.Skip(1).ToArray() ?? Array.Empty<Models.NijiuraChanPostInternal>();
					}

					return new Models.SureyomiChanResponse() {
						ThreadNo = threadNo,
						ThreadNoTxt = threadNoTxt,
						IsAlive = !o?.Data?.Thread.IsArchived ?? true,
						CurrentTime = nowTime,
						DieTime = o?.Data?.Thread.ExpiresAtDateTime ?? nowTime.AddHours(1),
						NewReplies = replies.Select(x => x.ToSureyomiChanModel(this.threadNo, new NijiuraChanInternalInteraction(this.urlString, x, null, this.config))).ToArray(),
						SupportFeature = new NijiuraChaninternalFeature(),
					};
				});
		}
	}
}

file class NijiuraChanFeature : ISureyomiChanFeature {
	public bool IsSupportThreadOld => false;

	public bool IsSupportThreadDie => false;
}

file class NijiuraChaninternalFeature : ISureyomiChanFeature {
	public bool IsSupportThreadOld => true;

	public bool IsSupportThreadDie => true;
}
