using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reflection.Metadata;
using System.Security.Policy;
using System.Threading.Tasks;

namespace Haru.Kei.SureyomiChan.Core;

class BouyomiChan {
	private readonly HttpClient httpClient;
	private readonly IConfigProxy config;
	private readonly Helpers.SerialRunner bouyomiRunner;
	private readonly UiMessageMultiDispatcher uiMsgMultiDispatcher;

	public BouyomiChan(HttpClient httpClient, UiMessageMultiDispatcher uiMsgMultiDispatcher, IConfigProxy config) {
		this.httpClient = httpClient;
		this.uiMsgMultiDispatcher = uiMsgMultiDispatcher;
		this.config = config;
		this.bouyomiRunner = new(500);
	}

	public void EnqueueSpeak(params string[] text) {
		foreach(var it in text) {
			bouyomiRunner.Dispatch(async () => {
				try {
					return await this.SpeakAsync(it);
				}
				catch(Exceptions.SureyomiChanException ex) {
					Utils.Logger.Instance.Error(ex);
					if(ex is Exceptions.ApiHttpErrorException exx && exx.HttpRequestException.StatusCode is null) {
						this.uiMsgMultiDispatcher.Dispatch(x => x.DispatchBouyomiChanNotFound());
					}
					if(ex is Exceptions.ApiHttpConnectionException) {
						this.uiMsgMultiDispatcher.Dispatch(x => x.DispatchBouyomiChanNotFound());
					}
					return false;
				}
			}).Subscribe();
		}
	}

	private async Task<bool> SpeakAsync(string speak) {
		Utils.Logger.Instance.Info($"棒読みちゃんの呼び出しを開始 => text={speak}");
		string genUrl() => $"http://localhost:{this.config.Get().BouyomiChanPort}/talk?text={Uri.EscapeDataString(speak)}";

		using var _ = await Utils.Util.Http(() => httpClient.GetAsync(genUrl()));
		Utils.Logger.Instance.Info($"棒読みちゃんの呼び出しが成功 => text={speak}");
		return true;
	}
}

