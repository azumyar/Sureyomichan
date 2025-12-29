using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Haru.Kei.SureyomiChan.Models;

public class JsonObject {
	public override string ToString() => this.ToString(
		writeIndented: true);

	public string ToString(
		bool writeIndented) => JsonSerializer.Serialize(
			this,
			this.GetType(),
			new JsonSerializerOptions {
				Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
				WriteIndented = writeIndented
			});

}
