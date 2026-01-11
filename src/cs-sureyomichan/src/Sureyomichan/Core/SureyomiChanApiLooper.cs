using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Haru.Kei.SureyomiChan.Core;
partial class SureyomiChanApiLooper : IDisposable {
	interface IWorker {
		IObservable<Models.SureyomiChanResponse> GetThread(int? latestResNo);
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
			Helpers.NijiuraChanUrl => new NijiuraChanInternalApiWorker(urlString, threadNo, this.config, webView),
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