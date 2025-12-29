using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Haru.Kei.SureyomiChan.Core;

/// <summary>tegaki_saveのtegakiグローバル変数をWebViewとやりとりするためのinterface</summary>
public interface ITegakiSaveStore {
	/// <summary>tegakiグローバル変数を表すjson</summary>
	/// <returns></returns>
	public string GetStore(int resNo);
}

class TegakiSaveStore : ITegakiSaveStore {
	/// <summary>tegakiグローバル変数定義</summary>
	class StoreObject : Models.JsonObject {
		[JsonPropertyName("res")]
		[JsonInclude]
		public required List<Models.TegakiSaveResData> TegakiData { get; init; }
	}

	private readonly object lockObj = new();
	private Dictionary<int, List<Models.TegakiSaveResData>> TegakiData { get; } = new();


	string ITegakiSaveStore.GetStore(int resNo) {
		return new StoreObject() {
			TegakiData = ToTegakiSaveModels(resNo),
		}.ToString();
	}

	public List<Models.TegakiSaveResData> ToTegakiSaveModels(int resNo) {
		lock(this.lockObj) {
			foreach(var it in this.TegakiData.Values) {
				if(it.Where(x => x.ResNo == $"resNo").Any()) {
					return it.ToList();

				}
			}
			return new();
		}
	}


	public void Add(int threadNo, Models.SureyomiChanModel m, bool isNg) {
		lock(this.lockObj) {
			var model = m.ToTegakiSaveModel(isNg: isNg);
			if(this.TegakiData.TryGetValue(threadNo, out var lt)) {
				lt.Add(model);
			} else {
				this.TegakiData.Add(threadNo, new() { model });
			}
		}
	}

	public void MarkNg(int resNo) {
		lock(this.lockObj) {
			foreach(var it in this.TegakiData.Values) {
				if(it.Where(x => x.ResNo == $"{resNo}")
					.FirstOrDefault() is { } target) {

					target.TegakiNg = true;
					return;
				}
			}
		}
	}

	public void Clear(int resNo) {
		lock(this.lockObj) {
			this.TegakiData.Remove(resNo);
		}
	}
}
