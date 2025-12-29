using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Haru.Kei.SureyomiChan;

static class ModelsExtensions {
	private readonly static TimeZoneInfo jstZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

	private static string Comment2Text(string com) {
		var s1 = Regex.Replace(com, @"<br>", Environment.NewLine,
			RegexOptions.IgnoreCase | RegexOptions.Multiline);
		var s2 = Regex.Replace(s1, @"<[^>]*>", "",
			RegexOptions.IgnoreCase | RegexOptions.Multiline);
		var s3 = System.Net.WebUtility.HtmlDecode(s2);

		return s3;
	}

	private static DateTime NijiuraChanTime2Local(string time) {
		if(DateTime.TryParseExact(time, "yyyy-MM-dd HH:mm:ss",
			null, System.Globalization.DateTimeStyles.None,
			out var result)) {

			// タイムゾーンが設定されていないので念のため設定する
			var d = result.AddHours(-9);
			var jtc = TimeZoneInfo.ConvertTimeFromUtc(
				new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second, DateTimeKind.Utc),
				jstZone);
			return jtc;
		} else {
			// なんか適当な時間かえす
			return new DateTime(1970, 1, 1);
		}
	}

	extension(Models.SureyomiChanModel source) {
		public bool IsId => source.Id is { };
		//public bool HasImage => source.ImageFileName is { };
		/// <summary>画面表示用の日付テキストを取得</summary>
		/// <returns></returns>
		public string FormatDateTime() => Utils.Util.FormatFutabaDateTime(source.PostTime);
		/// <summary><br>を改行に変換し</br>HTMLタグを除去した読み上げ用本文を取得</summary>
		/// <returns></returns>
		public string FormatBody() => Comment2Text(source.Body);

		public Models.TegakiSaveResData ToTegakiSaveModel(bool isNg) {
			static string @string(string? s) => s ?? "";

			var r = new Models.TegakiSaveResData() {
				ResCount = source.ResIndex,
				ResNo = $"{source.No}",
				Del = source.DeleteType.FormatString(),
				Id = @string(source.Id),
				Email = source.Email,
				Comment = source.Body,
				Now = source.FormatDateTime(),
				Time = $"{Utils.Util.ToUnixTimeSeconds(source.PostTime)}",
				TegakiNg = isNg,
				SureyomiTerms = new(source.Token),

				FileSource = @string(source.ImageSource),
				FileThumb = @string(source.ThumbnailSource),
				FileSize = source.ImageFileName switch {
					{ } => 1,
					_ => 0,
				},
				FileExtension = @string(System.IO.Path.GetExtension(source.ImageFileName))
			};

			return r;
		}
	}

	extension(Models.FutabaResponse source) {
		public DateTime NowDateTime => Utils.Util.FromUnixTimeSeconds(source.nowtime);
		public DateTime DieDateTime => DateTime.TryParse(
			source.dielong,
			System.Globalization.CultureInfo.InvariantCulture,
			System.Globalization.DateTimeStyles.None, out var d) switch {
				true => d,
				_ => DateTime.MaxValue
			};
	}

	extension(Models.__FutabaResData source) {
		public DateTime NowDateTime => long.TryParse(source.Time, out var v) switch {
			true => Utils.Util.FromUnixTimeSeconds(v),
			_ => DateTime.Now
		};
		public Models.SureyomiChanDeleteType DeleteType => source.Del.ToLower() switch {
			var v when string.IsNullOrEmpty(v) => Models.SureyomiChanDeleteType.None,
			var v when v == "del" => Models.SureyomiChanDeleteType.DeleteFromOwner,
			var v when v == "del2" => Models.SureyomiChanDeleteType.DeleteFromDel,
			var v when v == "selfdel" => Models.SureyomiChanDeleteType.SelfDelete,
			_ => Models.SureyomiChanDeleteType.None,
		};
		public string FormatBody() => Comment2Text(source.Comment);
	}

	extension(Models.NijiuraChanThreadV1 source) {
		public DateTime CreatedAtDateTime => NijiuraChanTime2Local(source.CreatedAt);
		public DateTime BumpedAtDateTime => NijiuraChanTime2Local(source.BumpedAt);
	}

	extension(Models.NijiuraChanReplyV1 source) {
		public DateTime CreatedAtDateTime => NijiuraChanTime2Local(source.CreatedAt);
		public string FormatBody() => Comment2Text(source.Body);
	}

	extension(Models.SureyomiChanDeleteType source) {
		public string FormatString() => source switch {
			Models.SureyomiChanDeleteType.DeleteFromOwner => "del",
			Models.SureyomiChanDeleteType.DeleteFromDel => "del2",
			Models.SureyomiChanDeleteType.SelfDelete => "selfdel",
			_ => "",
		};
	}
}