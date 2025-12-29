using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Haru.Kei.SureyomiChan.Models;

class Token : JsonObject {
	[JsonPropertyName("startOffset")]
	[JsonInclude]
	public required int StartOffset { get; init; }
	[JsonPropertyName("endOffset")]
	[JsonInclude]
	public required int EndOffset { get; init; }
	[JsonPropertyName("term")]
	[JsonInclude]
	public required string Term { get; init; }
	[JsonPropertyName("partOfSpeech")]
	[JsonInclude]
	public required string PartOfSpeech { get; init; }
}