using Haru.Kei.SureyomiChan.Models;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Haru.Kei.SureyomiChan.Helpers;

interface IApiUrl {
	public string Name { get; }
	public string GenUrlThread(int threadNo);
	public bool IsValidUrl(string url) => this.ParseThreadNo(url) != null;
	public int? ParseThreadNo(string url);
	public string GenApiThread(int threadNo, int? latestNo);
	public string GenApiDelete();
	public string GenApiDel();
	public string GenImage(Models.IAttachmentData model);
}

class FutabaUrl : IApiUrl {
	private readonly Random random = new();
	private readonly string domain;
	private readonly string boardName;
	private string FutabaEndPoint => $"https://{domain}.2chan.net/{boardName}/futaba.php";
	private string FutabaDelEndPoint => $"https://{domain}.2chan.net/del.php";

	public FutabaUrl() : this("img", "b") { }
	public FutabaUrl(string domain, string boardName) {
		this.domain = domain;
		this.boardName = boardName;
	}

	// 他に影響するのでimgで固定
	public string Name => SureyomiChanEnviroment.BoardImg;
	public string GenUrlThread(int threadNo) => $"https://{domain}.2chan.net/{boardName}/res/{threadNo}.htm";
	public int? ParseThreadNo(string url) {
		var m = Regex.Match(url, @$"https://{domain}\.2chan\.net/{boardName}/res/([0-9]+)\.htm");
		if (!m.Success) {
			return null;
		}

		if (!int.TryParse(m.Groups[1].Value, out var no)) {
			return null;
		}

		return no;
	}

	public string GenApiThread(int threadNo, int? latestNo) {
		return latestNo switch {
			int v => $"https://{domain}.2chan.net/{boardName}/futaba.php?mode=json&res={threadNo}&start={latestNo + 1}&{random.NextDouble()}",
			_ => $"https://{domain}.2chan.net/{boardName}/futaba.php?mode=json&res={threadNo}&start={threadNo + 1}&{random.NextDouble()}"
		};
	}

	public string GenApiDelete() => $"{FutabaEndPoint}?guid=on";
	public string GenApiDel() => $"{FutabaDelEndPoint}";

	public string GenImage(IAttachmentData data) => $"https://{domain}.2chan.net/{data.AttachmentImage}";
}

class NijiuraChanUrl : IApiUrl {
	public string Name => SureyomiChanEnviroment.BoardAimg;
	public string GenUrlThread(int threadNo) => $"https://nijiurachan.net/pc/thread.php?id={threadNo}";
	public int? ParseThreadNo(string url) {
		var m = Regex.Match(url, @$"https://nijiurachan\.net/pc/thread\.php\?id=([0-9]+)");
		if (!m.Success) {
			return null;
		}

		if (!int.TryParse(m.Groups[1].Value, out var no)) {
			return null;
		}

		return no;
	}

	public string GenApiThread(int threadNo, int? latestNo) {
		return latestNo switch {
			int v => $"https://nijiurachan.net/api/v1/thread/{threadNo}/new?after={v}",
			_ => $"https://nijiurachan.net/api/v1/thread/{threadNo}"
		};
	}

	public string GenApiDelete() => "";
	public string GenApiDel() => "";
	public string GenImage(IAttachmentData data) => $"";
}
