using Haru.Kei.SureyomiChan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reactive.Linq;
using System.Runtime;
using System.Text;
using System.Text.Json;

namespace Haru.Kei.SureyomiChan.Helpers;

using NjiuraChanThreadResponse = Models.NijiuraChanResponse<Models.NijiuraChanThreadDataV1>;

class NijiuraChanApi {
	private readonly HttpClient httpClient;

	public NijiuraChanApi(HttpClient httpClient, IApiUrl apiUrl) {

		// 現時点でAPIが素直につながらないので保留

		CookieContainer cookies = new CookieContainer();
		HttpClientHandler clientHandler = new HttpClientHandler();
		clientHandler.CookieContainer = cookies;
		clientHandler.UseCookies = true;


		this.httpClient = new(clientHandler);
		this.httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100");
		this.httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
		this.httpClient.DefaultRequestVersion = new Version(2, 0);
	}

	public IObservable<NjiuraChanThreadResponse> GetThread(int threadId, int? latestResId = null) {
		return Observable.Create<NjiuraChanThreadResponse>(async o => {
			string genUrl() => latestResId switch {
				int v => $"https://nijiurachan.net/api/v1/thread/{threadId}/new?after={v}",
				_ => $"https://nijiurachan.net/api/v1/thread/{threadId}"
			};

			var url = genUrl();
			try {
				var r = await httpClient.GetAsync(url);
				r.EnsureSuccessStatusCode();
				var json = await r.Content.ReadAsStringAsync();
				if (JsonSerializer.Deserialize<NjiuraChanThreadResponse>(json) is { } obj) {
					o.OnNext(obj);
				} else {
					o.OnError(new Exceptions.ApiInvalidJsonException(json));
				}
			}
			catch (HttpRequestException ex) {
				o.OnError(new Exceptions.ApiHttpErrorException(url, ex));
			}
		});
	}
}
