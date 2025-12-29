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


	#if false
	class NijiuraChanResponseData {
		[JsonPropertyName("thread")]
		public NijiuraChanThread Thread { get; private set; } = new();

		[JsonPropertyName("posts")]
		public IEnumerable<NijiuraChanPost> Posts { get; private set; } = Array.Empty<NijiuraChanPost>();
	}

	class NijiuraChanThread {
		[JsonPropertyName("id")]
		public int Id { get; private set; }
		[JsonPropertyName("board_id")]
		public string? BoardId { get; private set; }
		[JsonPropertyName("title")]
		public string Title { get; private set; } = "";
		[JsonPropertyName("name")]
		public string Name { get; private set; } = "";
		[JsonPropertyName("email")]
		public string Email { get; private set; } = "";
		[JsonPropertyName("body")]
		public string Body { get; private set; } = "";
		[JsonPropertyName("created_at")]
		public string CreatedAt { get; private set; } = "";
		[JsonPropertyName("last_post_at")]
		public string LastPostAt { get; private set; } = "";
		[JsonPropertyName("expires_at")]
		public string ExpiresAt { get; private set; } = "";
		[JsonPropertyName("is_sage")]
		public bool IsSage { get; private set; }
		[JsonPropertyName("is_locked")]
		public bool IsLocked { get; private set; }
		[JsonPropertyName("is_permanent")]
		public bool IsPermanent { get; private set; }
		[JsonPropertyName("is_admin")]
		public int IsAdmin { get; private set; }
		[JsonPropertyName("display_id")]
		public string? DisplayId { get; private set; }
		[JsonPropertyName("op_display_id")]
		public string? OpDisplayId { get; private set; }
		[JsonPropertyName("poster_id")]
		public string? PosterId { get; private set; }
		[JsonPropertyName("op_is_warned")]
		public bool OpIsWarned { get; private set; }
		[JsonPropertyName("op_is_exposed")]
		public bool OpIsExposed { get; private set; }
		[JsonPropertyName("host")]
		public string? Host { get; private set; }
		[JsonPropertyName("posts_count")]
		public int PostsCount { get; private set; }
		[JsonPropertyName("replies_count")]
		public int RepliesCount { get; private set; }
		[JsonPropertyName("soudane_count")]
		public int SoudaneCount { get; private set; }
		[JsonPropertyName("op_post_id")]
		public int OpPostId { get; private set; }
		[JsonPropertyName("is_archived")]
		public bool IsArchived { get; private set; }
		[JsonPropertyName("archived_at")]
		public string? ArchivedAt { get; private set; }
		[JsonPropertyName("storage_path")]
		public string? StoragePath { get; private set; }
		[JsonPropertyName("thumbnail_path")]
		public string? ThumbnailPath { get; private set; }
		[JsonPropertyName("attachment")]
		public NijiuraChanAattachment? Attachment { get; private set; }
	}

	class NijiuraChanPost {
		[JsonPropertyName("id")]
		public int Id { get; private set; }
		[JsonPropertyName("thread_id")]
		public int ThreadId { get; private set; }
		[JsonPropertyName("number_in_thread")]
		public int NumberInThread { get; private set; }
		[JsonPropertyName("name")]
		public string Name { get; private set; } = "";
		[JsonPropertyName("email")]
		public string Email { get; private set; } = "";
		[JsonPropertyName("subject")]
		public string Subject { get; private set; } = "";
		[JsonPropertyName("body")]
		public string Body { get; private set; } = "";
		[JsonPropertyName("host")]
		public string? Host { get; private set; }
		[JsonPropertyName("display_id")]
		public string? DisplayId { get; private set; }
		[JsonPropertyName("poster_id")]
		public string? PosterId { get; private set; }
		[JsonPropertyName("is_own_post")]
		public bool IsOwnPost { get; private set; }
		[JsonPropertyName("is_admin")]
		public int IsAdmin { get; private set; }
		[JsonPropertyName("show_id")]
		public bool ShowId { get; private set; }
		[JsonPropertyName("is_public_visible")]
		public bool IsPublicVisible { get; private set; }
		[JsonPropertyName("is_warned")]
		public bool IsWarned { get; private set; }
		[JsonPropertyName("is_exposed")]
		public bool IsExposed { get; private set; }
		[JsonPropertyName("created_at")]
		public string CreatedAt { get; private set; } = "";
		[JsonPropertyName("soudane_count")]
		public int SoudaneCount { get; private set; }
		[JsonPropertyName("del_count")]
		public int DelCount { get; private set; }
		[JsonPropertyName("attachment")]
		public NijiuraChanAattachment? Attachment { get; private set; }
	}

	class NijiuraChanAattachment {
		[JsonPropertyName("path")]
		public string Path { get; private set; } = "";
		[JsonPropertyName("thumbnail")]
		public string Thumbnail { get; private set; } = "";
		[JsonPropertyName("mime_type")]
		public string MimeType { get; private set; } = "";
		/*
		[JsonPropertyName("original_name")]
		public string OriginalName { get; private set; } = "";
		*/
	[JsonPropertyName("size")]
	public int size { get; private set; }
}
#endif

