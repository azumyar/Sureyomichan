using Haru.Kei.SureyomiChan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace Haru.Kei.SureyomiChan.Core; 
partial class SureyomiChanApiLooper {
	class FutabaApiWorker : IWorker {
		private readonly Helpers.FutabaApi api;
		private readonly IConfigProxy config;
		private readonly string urlString;
		private readonly int threadNo;
		private readonly string threadNoTxt = "threadno.img.txt";

		public FutabaApiWorker(string urlString, int threadNo, IConfigProxy config) {
			this.urlString = urlString;
			this.threadNo = threadNo;
			this.api = Utils.Singleton.Instance.FutabaApi;
			this.config = config;
		}

		public IObservable<Models.SureyomiChanResponse> GetThread(int? latestResNo) {
			return this.api.GetThreadSerial(this.threadNo, latestResNo)
				.Select(x => new Models.SureyomiChanResponse() {
					ThreadNo = threadNo,
					ThreadNoTxt = threadNoTxt,
					IsAlive = x.NowDateTime < x.DieDateTime,
					CurrentTime = x.NowDateTime,
					DieTime = x.DieDateTime,
					NewReplies = x.Res.Select(x => x.ToSureyomiChanModel(this.threadNo, new FutabaInteraction(this.urlString, x, this.api, this.config))).ToArray(),
					SupportFeature = new FutabaFeature(),
				});
		}
	}
}

file class FutabaFeature : ISureyomiChanFeature {
	public bool IsSupportThreadOld => true;

	public bool IsSupportThreadDie => true;
}