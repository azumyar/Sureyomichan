using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Haru.Kei.SureyomiChan.Core;

class SureyomiChanNgProcesser(WebView2Proxy webView2, ConfigProxy config) {

	public async Task<(bool, string)> IsNgFromBody(Models.SureyomiChanModel m, Models.DifferenceHash? dhash) {
		var con = config.Get();
		if(m.HasId && con.NonReadId) {
			return await Task.FromResult(ToResult(true));
		}

		if(m.DeleteType != Models.SureyomiChanDeleteType.None) {
			return await Task.FromResult(ToResult(true));
		}

		System.Console.WriteLine(m.Body);
		var ret = await webView2.RunPlugin(m, dhash?.Value);
		System.Console.WriteLine(ret);
		return ToResult(ret);
	}

	public async Task<(bool, string)> IsNgFromImage(Models.DifferenceHash dhash) {
		return await Task.FromResult(ToResult(false));
	}

	private static (bool, string) ToResult(bool b) => (b, "");
	private static (bool, string) ToResult(Models.TegakiSavePluginResult? r) => r switch {
		{ } v => (v.IsStop, v.ResultValue),
		_ => ToResult(false),
	};
		
}
