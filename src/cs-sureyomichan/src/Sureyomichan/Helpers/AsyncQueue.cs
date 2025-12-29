using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Haru.Kei.SureyomiChan.Helpers;

class AsyncQueue<T> : IDisposable{

	private readonly System.Collections.Concurrent.ConcurrentQueue<T> queue = new();
	private readonly CancellationTokenSource cancel = new();
	private readonly Thread thread;
	public AsyncQueue(Func<T, Task> callBack, int intervalMiliSec = 1000) {
		var ct = this.cancel.Token;
		this.thread = new(async () => {
			SynchronizationContext.SetSynchronizationContext(
				new DispatcherSynchronizationContext());

			while (!cancel.IsCancellationRequested) {
				if (queue.TryDequeue(out var it)) {
					await callBack(it);
				}
				Thread.Sleep(intervalMiliSec);
			}
		});
		this.thread.IsBackground = true;
		this.thread.Start();
	}

	public void Dispose() { 
		this.cancel?.Dispose();
	}

	public void Enqueue(T item) {
		this.queue.Enqueue(item);
	}

	public void Enqueue(IEnumerable<T> item) {
		foreach (var it in item) {
			this.queue.Enqueue(it);
		}
	}
}
