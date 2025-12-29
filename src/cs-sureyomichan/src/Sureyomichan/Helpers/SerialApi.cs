using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Haru.Kei.SureyomiChan.Helpers;

/// <summary>
/// 非同期のDispatch()を一本化して処理します
/// APIの負荷軽減を目的にしています
/// </summary>
class SerialRunner {
	private readonly CountdownEvent dispatchEvent = new(1);
	private readonly ConcurrentQueue<Action> dispatchQueue = new();
	private readonly Thread thread;

	public event EventHandler? StartTask;
	public event EventHandler? EndTask;
	public event EventHandler? Sleep;

	public SerialRunner(int waitTimeMilliSec, string? threadName = null) {
		this.thread = new(() => {
			var lastExecute = DateTime.Now.AddMilliseconds(-waitTimeMilliSec);
			while(true) {
				while(true) {
					if(this.dispatchQueue.TryDequeue(out var it)) {
						var sleepTime = waitTimeMilliSec - (int)(DateTime.Now - lastExecute).TotalMilliseconds;
						if(0 < sleepTime) {
							Thread.Sleep(sleepTime);
						}
						lastExecute = DateTime.Now;

						this.StartTask?.Invoke(this, EventArgs.Empty);
						it.Invoke();
						this.EndTask?.Invoke(this, EventArgs.Empty);
					} else {
						this.Sleep?.Invoke(this, EventArgs.Empty);
						break;
					}
				}
				this.dispatchEvent.Wait();
				this.dispatchEvent.Reset();
			}
		}) {
			Name = threadName,
			IsBackground = true,
		};
		this.thread.Start();
	}

	public IObservable<T> Dispatch<T>(Func<Task<T>> action) {
		return Observable.Create<T>(o => {
			this.dispatchQueue.Enqueue(async () => {
				try {
					o.OnNext(await action.Invoke());
				}
				catch (Exception ex) {
					o.OnError(ex);
				}
				finally {
					o.OnCompleted();
				}
			});
			try {
				this.dispatchEvent.Signal();
			}
			catch (InvalidOperationException) { }

			return System.Reactive.Disposables.Disposable.Empty;
		}).ObserveOn(System.Reactive.Concurrency.ThreadPoolScheduler.Instance);
	}
}

