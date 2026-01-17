using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Haru.Kei.SureyomiChan.Core;

class SureyomiChanNgProcesser(WebView2Proxy webView2, ConfigProxy config) {

	public async Task<bool> IsNgFromBody(Models.SureyomiChanModel m) {
		var con = config.Get();
		if(m.HasId && con.NonReadId) {
			return await Task.FromResult(true);
		}

		if(m.DeleteType != Models.SureyomiChanDeleteType.None) {
			return await Task.FromResult(true);
		}

		var ret = await webView2.RunPlugin(m);
		return ret?.IsStop ?? false;
	}

	public async Task<bool> IsNgFromImage(Models.DifferenceHash dhash) {
		return await Task.FromResult(false);
	}
}
