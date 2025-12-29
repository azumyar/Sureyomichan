using Prism.Events;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Haru.Kei.SureyomiChan.Utils;

class Singleton {
	public HttpClient HttpClient { get; }
	public Helpers.IApiUrl FutabaUrl { get; } = new Helpers.FutabaUrl();
	public Helpers.IApiUrl NijiuraChanUrl { get; } = new Helpers.NijiuraChanUrl();

	public Helpers.FutabaApi FutabaApi { get; }
	public Helpers.NijiuraChanApi NijiuraChanApi { get; }

	public Helpers.StartupSequence StartupSequence { get; } = new();
	public EventAggregator PrismMessenger { get; } = new();

	public Singleton() {
		/*
CookieContainer cookies = new CookieContainer();
HttpClientHandler clientHandler = new HttpClientHandler();
clientHandler.CookieContainer = cookies;
clientHandler.UseCookies = true;
		*/

		this.HttpClient = new(/*clientHandler*/);
		this.HttpClient.DefaultRequestHeaders.Add("User-Agent", "SureyomiChan/v1");
		this.HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");

		this.FutabaApi = new(this.HttpClient, this.FutabaUrl);
		this.NijiuraChanApi = new(this.HttpClient, this.NijiuraChanUrl);
	}

	public static Singleton Instance {
		get {
			if (field == null) {
				field = new();
			}
			return field;
		}
	}
}
