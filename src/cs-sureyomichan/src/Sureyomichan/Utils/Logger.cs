using Haru.Kei.SureyomiChan.Exceptions;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

using Proc = System.Diagnostics.Process;
using Thread = System.Threading.Thread;

namespace Haru.Kei.SureyomiChan.Utils;

class Logger {
	public static Logger Instance { get; } = new();
	private System.Reactive.Concurrency.EventLoopScheduler LogScheduler { get; } = new();
	private System.IO.FileStream? logWriter;
	private bool inited = false;

	public Logger() {
		try {
			this.logWriter = new System.IO.FileStream(
				SureyomiChanEnviroment.GetStaticPath(SureyomiChanStaticItem.LogFile),
				System.IO.FileMode.OpenOrCreate,
				System.IO.FileAccess.ReadWrite,
				System.IO.FileShare.ReadWrite);
		}
		catch(Exception e) when (
			(e is System.IO.FileNotFoundException)
			|| (e is System.IO.IOException)) { }
	}

	public void Init() {
		this.inited = true;
		this.logWriter?.SetLength(0);
	}

	public void Info(string i) => this.Write("i", i);

	public void Error(string e) => this.Write("i", e);
	
	public void Error(Exception e) {
		// 内部エラーのパースをする
		static string eApiInvalidJsonException(Exceptions.ApiInvalidJsonException ex) => $"予想しないJSONがAPIから返却されました\r\n{ex.Json}";
		static string eApiHttpErrorException(Exceptions.ApiHttpErrorException ex) => $"HTTPサーバがエラーコードを返却しました({(int)(ex.HttpRequestException.StatusCode ?? 0)}) => {ex.Url}";
		static string eApiHttpConnectionException(Exceptions.ApiHttpConnectionException ex) => $"HTTPサーバとの通信に失敗しました\r\n{ex.ConnectionException}";
		static string eImageNotSupportException(Exceptions.ImageNotSupportException _) => $"画像の読み込みに失敗しました";

		var m = e switch {
			Exceptions.ApiInvalidJsonException v => eApiInvalidJsonException(v),
			Exceptions.ApiHttpErrorException v => eApiHttpErrorException(v),
			Exceptions.ApiHttpConnectionException v => eApiHttpConnectionException(v),
			Exceptions.ImageNotSupportException v => eImageNotSupportException(v),
			//Exceptions.TegakiSaveException v => v.ToString(), 
			_ => e.ToString()
		};

		this.Write("e", m);
	}

	private void Write(string prefix, string message) {
		static string genDate() => DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

		var p = Proc.GetCurrentProcess();
		var pName = p.ProcessName;
		var pId = p.Id;
		var tId = Thread.CurrentThread.ManagedThreadId;

		Observable.Return($"[{genDate()}][{pName}][{pId}][{tId}][{prefix}]{message}")
			.ObserveOn(LogScheduler)
			.Subscribe(x => {
				Console.WriteLine(x);
				if(this.inited) {
					this.logWriter?.Write(Encoding.UTF8.GetBytes($"{x}\r\n"));
					this.logWriter?.Flush();
				}
			});
	}

}
