using Haru.Kei.SureyomiChan.Utils;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Haru.Kei.SureyomiChan.Core;

class WebView2Proxy {
	private bool isInitialized = false;
	private readonly Microsoft.Web.WebView2.Wpf.WebView2 webView2;
	private readonly TegakiSaveStore tegakiSaveStore;
	private readonly string localRoot;
	private readonly bool isOpenWebViewDevTool;

	public event EventHandler? CoreInitialized;

	// WebView2Proxyの設定更新は再起動が必要
	public WebView2Proxy(
		Microsoft.Web.WebView2.Wpf.WebView2 webView2,
		TegakiSaveStore tegakiSaveStore,
		Models.Config currentConfig) {

		this.webView2 = webView2;
		this.tegakiSaveStore = tegakiSaveStore;
		this.localRoot = currentConfig.PathLegacyTegakiSaveValue;
		this.isOpenWebViewDevTool = currentConfig.OpenWebViewDevTool;

		// CoreWebView2初期化
		this.webView2.CoreWebView2InitializationCompleted += this.OnWebViewInitialization;
		Observable.Return(0)
			.Delay(TimeSpan.FromMilliseconds(1))
			.ObserveOn(Reactive.Bindings.UIDispatcherScheduler.Default)
			.Subscribe(async _ => {
				var env = await CoreWebView2Environment.CreateAsync();
				await webView2.EnsureCoreWebView2Async(env);
			});
	}

	private async void OnBootNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e) {
		this.webView2.NavigationCompleted -= this.OnBootNavigationCompleted;
		foreach(var it in Directory.EnumerateFiles(Path.Combine(localRoot, "plugins"), "*.js", SearchOption.AllDirectories)) {
			var plugin_js = it.AsSpan().Slice(localRoot.Length).ToString();
			Utils.Logger.Instance.Info($"互換JavaScriptプラグインの読み込み => {plugin_js}");
			await this.webView2.CoreWebView2.ExecuteScriptAsync(@$"
import('{plugin_js.Replace('\\', '/')}')
  .then(m => {{
    loadModule(m);
    //console.log(tegakiPlugins);
  }})
");
		}

		await this.webView2.CoreWebView2.ExecuteScriptAsync(@$"
function __runNgPlugins(json) {{
	const res = JSON.parse(json);
	const tegaki = JSON.parse(chrome.webview.hostObjects.sync.TegakiSaveObject.GetStore(parseInt(res.resNo)));
	res.__tegaki_res = tegaki.res;
	const pResult = runPlugins({{
		point: 'read',
		execName: 'beforeExecute',
		from: 'tegakiSaveCtrl.read',
		res: res,
	}});
	return JSON.stringify(pResult);
}}");
	}

	private async void OnWebViewInitialization(object? sender, CoreWebView2InitializationCompletedEventArgs e) {
		this.isInitialized = e.IsSuccess;

		Utils.Logger.Instance.Info($"CoreWebView2の初期化が終わりました => IsSuccess={e.IsSuccess}");
		if(this.isInitialized) {
			if(Directory.Exists(localRoot)) {
				this.webView2.CoreWebView2.SetVirtualHostNameToFolderMapping(
					"localmap",
					localRoot,
					Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);

				if(this.isOpenWebViewDevTool) {
					this.webView2.CoreWebView2.OpenDevToolsWindow();
				}
				this.webView2.NavigationCompleted += this.OnBootNavigationCompleted;
				this.webView2.CoreWebView2.Navigate(@"https://localmap/interop/boot.html");
				this.webView2.CoreWebView2.AddHostObjectToScript(
					"TegakiSaveObject",
					new HostObject(tegakiSaveStore));

				CoreInitialized?.Invoke(this, EventArgs.Empty);
			} else {
				Logger.Instance.Error($"tegaki_saveプラグイン設定が不正です。連携を停止します。 => {this.localRoot}");
			}
		}
	}

	public async Task<Models.TegakiSavePluginResult?> RunPlugin(Models.SureyomiChanModel res) {
		var task = await Utils.Util.AwaitObserver(
			Observable.Return(res)
				.ObserveOn(Reactive.Bindings.UIDispatcherScheduler.Default)
				.Select(async x => {
					var r = await this.webView2.CoreWebView2.ExecuteScriptAsync($"__runNgPlugins('{x.ToTegakiSaveModel(isNg: false).ToString(writeIndented: false)}')");
					if(r == "null") {
						return null;
					}

					var s = Regex.Replace(r, @"\\(.)", m => {
						return m.Groups[1].Value switch {
							_ => m.Groups[1].Value,
						};
					});
					return JsonSerializer.Deserialize<Models.TegakiSavePluginResult>(s.Substring(1, s.Length - 2));
				}), default);
		if(task is { }) {
			return await task;
		} else {
			return await Task.FromResult<Models.TegakiSavePluginResult?>(null);
		}
	}

	public async Task<string> __RequestApi(string url) {
		var task = await Utils.Util.AwaitObserver(
			Observable.Return(url)
				.ObserveOn(Reactive.Bindings.UIDispatcherScheduler.Default)
				.Select(async x => {
					var r = await this.webView2.CoreWebView2.ExecuteScriptAsync($@"
{{
    const xmlHttpApi = new XMLHttpRequest();    
    xmlHttpApi.open('GET', '{url}', false);
    xmlHttpApi.send(null);
	xmlHttpApi.responseText;
}}
");
					if(r == "null") {
						return "";
					}

					var s = Regex.Replace(r, @"\\u(....)", m => {
						int charCode16 = Convert.ToInt32(m.Groups[1].Value, 16);
						char c = Convert.ToChar(charCode16);
						return c.ToString();
					});
					s = Regex.Replace(s, @"\\(.)", m => {
						return m.Groups[1].Value switch {
							_ => m.Groups[1].Value,
						};
					});
					return s.Substring(1, s.Length - 2);
				}), default);
		if(task is { }) {
			return await task;
		} else {
			return await Task.FromResult("");
		}

	}
}

[ComVisible(true)]
[Guid("326DF6C0-B080-4C84-99A7-14DBDF6062B3")]
public class HostObject(ITegakiSaveStore tegakiSaveStore) {
	public string GetStore(int resNo) => tegakiSaveStore.GetStore(resNo);
}