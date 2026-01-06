using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Haru.Kei.SureyomiChan.Models;

class NijiuraChanResponse<T> : JsonObject {
	[JsonPropertyName("ok")]
	[JsonInclude]
	public required bool Ok { get; init; }

	[JsonPropertyName("data")]
	[JsonInclude]
	public T? Data { get; init; }

	[JsonPropertyName("error")]
	[JsonInclude]
	public string? Error { get; init; }
}

class NijiuraChanThreadDataV1 : JsonObject {
	[JsonPropertyName("thread")]
	[JsonInclude]
	public required NijiuraChanThreadV1 Thread {  get; init; }
	[JsonPropertyName("replies")]
	[JsonInclude]
	public required IEnumerable<NijiuraChanReplyV1> Replies { get; init; }
	[JsonPropertyName("reply_count")]
	[JsonInclude]
	public required int ReplyCount { get; init; }
}


class NijiuraChanNewThreadDataV1 : JsonObject {
	[JsonPropertyName("thread_id")]
	[JsonInclude]
	public required int Thread { get; init; }
	[JsonPropertyName("after")]
	[JsonInclude]
	public required int After { get; init; }
	[JsonPropertyName("replies")]
	[JsonInclude]
	public required IEnumerable<NijiuraChanReplyV1> Replies { get; init; }
	[JsonPropertyName("count")]
	[JsonInclude]
	public required int Count { get; init; }
	[JsonPropertyName("latest_id")]
	[JsonInclude]
	public required int LatestId { get; init; }
	[JsonPropertyName("thread_reply_count")]
	[JsonInclude]
	public required int ThreadReplyCount { get; init; }
}


class NijiuraChanReplyV1 : JsonObject {
	[JsonPropertyName("id")]
	[JsonInclude]
	public int Id { get; private set; }
	[JsonPropertyName("thread_id")]
	[JsonInclude]
	public int ThreadId { get; private set; }
	[JsonPropertyName("number")]
	[JsonInclude]
	public int Number { get; private set; }
	[JsonPropertyName("name")]
	[JsonInclude]
	public string Name { get; private set; } = "";
	[JsonPropertyName("body")]
	[JsonInclude]
	public string Body { get; private set; } = "";
	[JsonPropertyName("image")]
	[JsonInclude]
	public string? Image { get; private set; }
	[JsonPropertyName("thumb")]
	[JsonInclude]
	public string? Thumb { get; private set; }
	[JsonPropertyName("soudane_count")]
	[JsonInclude]
	public int SoudaneCount { get; private set; }
	[JsonPropertyName("del_count")]
	[JsonInclude]
	public int DelCount { get; private set; }
	[JsonPropertyName("created_at")]
	[JsonInclude]
	public string CreatedAt { get; private set; } = "";
	[JsonPropertyName("poster_id")]
	[JsonInclude]
	public string? PosterId { get; private set; }
}


class NijiuraChanThreadV1 : JsonObject {
	[JsonPropertyName("id")]
	[JsonInclude]
	public int Id { get; private set; }
	[JsonPropertyName("name")]
	[JsonInclude]
	public string Name { get; private set; } = "";
	[JsonPropertyName("body")]
	[JsonInclude]
	public string Body { get; private set; } = "";
	[JsonPropertyName("image")]
	[JsonInclude]
	public string? Image { get; private set; }
	[JsonPropertyName("thumb")]
	[JsonInclude]
	public string? Thumb { get; private set; }
	[JsonPropertyName("original_filename")]
	[JsonInclude]
	public string? OriginalFilename { get; private set; }
	[JsonPropertyName("reply_count")]
	[JsonInclude]
	public int ReplyCount { get; private set; }
	[JsonPropertyName("soudane_count")]
	[JsonInclude]
	public int SoudaneCount { get; private set; }
	[JsonPropertyName("created_at")]
	[JsonInclude]
	public string CreatedAt { get; private set; } = "";
	[JsonPropertyName("bumped_at")]
	[JsonInclude]
	public string BumpedAt { get; private set; } = "";
	[JsonPropertyName("show_id")]
	[JsonInclude]
	public bool ShowId { get; private set; }
	[JsonPropertyName("poster_id")]
	[JsonInclude]
	public string? PosterId { get; private set; }
}


