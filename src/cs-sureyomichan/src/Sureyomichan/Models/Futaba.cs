using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Haru.Kei.SureyomiChan.Models;


class FutabaResponse : JsonObject {
	[JsonPropertyName("die")]
	[JsonInclude]
	public string die { get; private set; } = "";
	[JsonPropertyName("dielong")]
	[JsonInclude]
	public string dielong { get; private set; } = "";

	[JsonPropertyName("dispname")]
	[JsonInclude]
	public int dispname { get; private set; }

	[JsonPropertyName("dispsod")]
	[JsonInclude]
	public int dispsod { get; private set; }

	[JsonPropertyName("maxres")]
	[JsonInclude]
	public string maxres { get; private set; } = "";

	[JsonPropertyName("nowtime")]
	[JsonInclude]
	public long nowtime { get; private set; }

	[JsonPropertyName("old")]
	[JsonInclude]
	public long old { get; private set; }

	[JsonPropertyName("res")]
	[JsonInclude]
	public Dictionary<string, FutabaResData> __Res { get; private set; } = new();

	[JsonPropertyName("sd")]
	[JsonInclude]
	public object? Soudane { get; private set; }
	/*
	 * @property {Array | Object.<string, number>}  sd
	 */

	[JsonIgnore]
	public IEnumerable<__FutabaResData> Res
		=> this.__Res.Select(x => new __FutabaResData(x.Value, x.Key))
			.OrderBy(x => x.ResNoInt)
			.ToArray();
}

class FutabaResData : JsonObject, IAttachmentData {
	[JsonPropertyName("com")]
	[JsonInclude]
	public string Comment { get; protected set; } = "";
	[JsonPropertyName("del")] // del, del2, selfdel
	[JsonInclude]
	public string Del { get; protected set; } = "";
	[JsonPropertyName("email")]
	[JsonInclude]
	public string Email { get; protected set; } = "";
	[JsonPropertyName("ext")]
	[JsonInclude]
	public string FileExtension { get; protected set; } = "";
	[JsonPropertyName("fsize")]
	[JsonInclude]
	public int FileSize { get; protected set; }
	[JsonPropertyName("h")]
	[JsonInclude]
	public int ThunbHeight { get; protected set; }
	[JsonPropertyName("host")]
	[JsonInclude]
	public string Host { get; protected set; } = "";
	[JsonPropertyName("id")]
	[JsonInclude]
	public string Id { get; protected set; } = "";
	[JsonPropertyName("name")]
	[JsonInclude]
	public string Name { get; protected set; } = "";
	[JsonPropertyName("now")]
	[JsonInclude]
	public string Now { get; protected set; } = "";
	[JsonPropertyName("rsc")]
	[JsonInclude]
	public int ResCount { get; protected set; }
	[JsonPropertyName("src")]
	[JsonInclude]
	public string FileSource { get; protected set; } = "";
	[JsonPropertyName("sub")]
	[JsonInclude]
	public string Subject { get; protected set; } = "";
	[JsonPropertyName("thumb")]
	[JsonInclude]
	public string FileThumb { get; protected set; } = "";
	[JsonPropertyName("tim")]
	[JsonInclude]
	public string Time { get; protected set; } = "";
	[JsonPropertyName("w")]
	[JsonInclude]
	public int FileWidth { get; protected set; }

	[JsonIgnore]
	public string AttachmentImage {
		get {
			var extension = this.FileExtension.ToLower();
			var webp = extension == ".webp";
			var movie = extension switch {
				".mp4" => true,
				".webm" => true,
				_ => false
			};
			if(webp || movie) {
				return this.FileThumb;
			}else {
				return this.FileSource;
			}
		}
	}

}

class __FutabaResData : FutabaResData {
	public __FutabaResData(FutabaResData source, string ResNo) {
		this.Comment = source.Comment;
		this.Del = source.Del;
		this.Email = source.Email;
		this.FileExtension = source.FileExtension;
		this.FileSize = source.FileSize;
		this.ThunbHeight = source.ThunbHeight;
		this.Host = source.Host;
		this.Id = source.Id;
		this.Name = source.Name;
		this.Now = source.Now;
		this.ResCount = source.ResCount;
		this.FileSource = source.FileSource;
		this.Subject = source.Subject;
		this.FileThumb = source.FileThumb;
		this.Time = source.Time;
		this.FileWidth = source.FileWidth;

		this.ResNo = ResNo;
	}

	[JsonPropertyName("resNo")]
	[JsonInclude]
	public string ResNo { get; private set; }

	[JsonIgnore]
	public int ResNoInt {
		get {
			if(field == 0) {
				if(int.TryParse(this.ResNo, out var v)) {
					field = v;
				} else {
					field = -1;
				}
			}
			return field;
		}
	}

	/*
	 * @property {string | undefined} resNo tegaki_save独自、レスNo
	 * @property {boolean | undefined} __tageki_ng tegaki_save独自、NGの場合true
	 * @property {FutabaRes[] | undefined} __tegaki_res tegaki_save独自、プラグインに渡すときレス配列を含める

	 */
}

[JsonConverter(typeof(FutabaResDataJsonConverter))]
class FutabaReplies : JsonObject {
	public IEnumerable<__FutabaResData> Data{get; set;}=new __FutabaResData[0];
}

file class FutabaResDataJsonConverter : JsonConverter<FutabaReplies> {
	public override FutabaReplies Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) {

		if(JsonSerializer.Deserialize<Dictionary<string, FutabaResData>>(reader.GetString()!, options) is { } dic) {
			return new() {
				Data = dic.Select(x => new __FutabaResData(x.Value, x.Key))
				.OrderBy(x => x.ResNoInt)
				.ToArray()
			};
		} else {
			return new();
		}
	}

	public override void Write(
		Utf8JsonWriter writer,
		FutabaReplies resDataValue,
		JsonSerializerOptions options) {

		if(resDataValue.Data.Any()) {
			writer.WriteStartObject();
			foreach(var it in resDataValue.Data) {
				writer.WritePropertyName(it.ResNo);
				writer.WriteRawValue(it.ToString());
			}
			writer.WriteEndObject();
		} else {
			writer.WriteNullValue();
		}
	}
}