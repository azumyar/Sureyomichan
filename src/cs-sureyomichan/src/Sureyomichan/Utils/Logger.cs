using Haru.Kei.SureyomiChan.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Text;

using Proc = System.Diagnostics.Process;
using Thread = System.Threading.Thread;

namespace Haru.Kei.SureyomiChan.Utils;

class Logger {
	public static Logger Instance { get; } = new();
	private System.Reactive.Concurrency.EventLoopScheduler LogScheduler { get; } = new();
	private System.IO.FileStream? logWriter;
	private bool inited = false;
	private readonly MemoryStream beforeLog = new();
	private readonly object lockObj = new();

	public Logger() {
		try {
			this.logWriter = new System.IO.FileStream(
				SureyomiChanEnviroment.GetStaticString(SureyomiChanStaticItem.LogFile),
				System.IO.FileMode.OpenOrCreate,
				System.IO.FileAccess.ReadWrite,
				System.IO.FileShare.ReadWrite);
		}
		catch(Exception e) when(
			(e is System.IO.FileNotFoundException)
			|| (e is System.IO.IOException)) { }
	}

	public void Init() {
		this.inited = true;
		this.logWriter?.SetLength(0);
		if(0 < this.beforeLog.Length) {
			Observable.Return(this.beforeLog.ToArray())
				.ObserveOn(LogScheduler)
				.Subscribe(x => {
					if(this.inited) {
						this.logWriter?.Write(x);
						this.logWriter?.Flush();
					}
				});
			this.beforeLog.SetLength(0);
		}
	}

	public void Info(string i) => this.WriteAsync("i", i);

	public void Error(string e) => this.WriteAsync("i", e);

	public void Error(Exception e) => this.WriteAsync("e", this.FormatException(e));

	public void WriteFromUnhandledException(string message, Exception? e) {
		var sb = new StringBuilder();
		if(e is { }) {
			sb.AppendLine(message)
				.Append(this.FormatException(e));
		} else {
			sb.Append(message);
		}

		this.WriteSync("e", sb.ToString());
	}

	private string FormatErrorMessage(string prefix, string message) {
		static string genDate() => DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

		var p = Proc.GetCurrentProcess();
		var pName = p.ProcessName;
		var pId = p.Id;
		var tId = Thread.CurrentThread.ManagedThreadId;

		return $"[{genDate()}][{pName}][{pId}][{tId}][{prefix}]{message}";
	}

	private string FormatException(Exception e) {
		// 内部エラーのパースをする
		static string eApiInvalidJsonException(Exceptions.ApiInvalidJsonException ex) => $"予想しないJSONがAPIから返却されました\r\n{ex.Json}";
		static string eApiHttpErrorException(Exceptions.ApiHttpErrorException ex) => $"HTTPサーバがエラーコードを返却しました({(int)(ex.HttpRequestException.StatusCode ?? 0)}) => {ex.Url}";
		static string eApiHttpConnectionException(Exceptions.ApiHttpConnectionException ex) => $"HTTPサーバとの通信に失敗しました\r\n{ex.ConnectionException}";
		static string eImageNotSupportException(Exceptions.ImageNotSupportException _) => $"画像の読み込みに失敗しました";

		return e switch {
			Exceptions.ApiInvalidJsonException v => eApiInvalidJsonException(v),
			Exceptions.ApiHttpErrorException v => eApiHttpErrorException(v),
			Exceptions.ApiHttpConnectionException v => eApiHttpConnectionException(v),
			Exceptions.ImageNotSupportException v => eImageNotSupportException(v),
			//Exceptions.TegakiSaveException v => v.ToString(), 
			_ => e.ToString()
		};
	}

	private void WriteAsync(string prefix, string message)
		=> Observable.Return(this.FormatErrorMessage(prefix, message))
			.ObserveOn(LogScheduler)
			.Subscribe(x => {
				this.Write(x);
			});

	private void WriteSync(string prefix, string message)
		=> this.Write(this.FormatErrorMessage(prefix, message));

	private void Write(string x) {
		lock(lockObj) {
			Console.WriteLine(x);
			Stream? stream = this.inited switch {
				true => this.logWriter,
				_ => this.beforeLog
			};
			if(stream is { }) {
				stream.Write(Encoding.UTF8.GetBytes($"{x}\r\n"));
				stream.Flush();
			}
		}
	}
}
