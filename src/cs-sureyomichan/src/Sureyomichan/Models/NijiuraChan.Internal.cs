using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

// 非公開API定義


namespace Haru.Kei.SureyomiChan.Models;

class NijiuraChanThreadInternalData : JsonObject {
	[JsonPropertyName("thread")]
	[JsonInclude]
	public NijiuraChanThreadInternal Thread { get; private set; } = new();

	[JsonPropertyName("posts")]
	[JsonInclude]
	public IEnumerable<NijiuraChanPostInternal> Posts { get; private set; } = Array.Empty<NijiuraChanPostInternal>();
}

class NijiuraChanThreadInternal : JsonObject {
	[JsonPropertyName("id")]
	[JsonInclude]
	public int Id { get; private set; }
	[JsonPropertyName("board_id")]
	[JsonInclude]
	public string BoardId { get; private set; } = "";
	[JsonPropertyName("title")]
	[JsonInclude]
	public string Title { get; private set; } = "";
	[JsonPropertyName("name")]
	[JsonInclude]
	public string Name { get; private set; } = "";
	[JsonPropertyName("email")]
	[JsonInclude]
	public string Email { get; private set; } = "";
	[JsonPropertyName("body")]
	[JsonInclude]
	public string Body { get; private set; } = "";
	[JsonPropertyName("created_at")]
	[JsonInclude]
	public string CreatedAt { get; private set; } = "";
	[JsonPropertyName("last_post_at")]
	[JsonInclude]
	public string LastPostAt { get; private set; } = "";
	[JsonPropertyName("expires_at")]
	[JsonInclude]
	public string ExpiresAt { get; private set; } = "";
	[JsonPropertyName("is_sage")]
	[JsonInclude]
	public bool IsSage { get; private set; }
	[JsonPropertyName("is_locked")]
	[JsonInclude]
	public bool IsLocked { get; private set; }
	[JsonPropertyName("is_permanent")]
	[JsonInclude]
	public bool IsPermanent { get; private set; }
	[JsonPropertyName("is_admin")]
	[JsonInclude]
	public int IsAdmin { get; private set; }
	[JsonPropertyName("display_id")]
	[JsonInclude]
	public string? DisplayId { get; private set; }
	[JsonPropertyName("op_display_id")]
	[JsonInclude]
	public string? OpDisplayId { get; private set; }
	[JsonPropertyName("poster_id")]
	[JsonInclude]
	public string? PosterId { get; private set; }
	[JsonPropertyName("op_is_warned")]
	[JsonInclude]
	public bool OpIsWarned { get; private set; }
	[JsonPropertyName("op_is_exposed")]
	[JsonInclude]
	public bool OpIsExposed { get; private set; }
	[JsonPropertyName("host")]
	[JsonInclude]
	public string? Host { get; private set; }
	[JsonPropertyName("posts_count")]
	[JsonInclude]
	public int PostsCount { get; private set; }
	[JsonPropertyName("replies_count")]
	[JsonInclude]
	public int RepliesCount { get; private set; }
	[JsonPropertyName("soudane_count")]
	[JsonInclude]
	public int SoudaneCount { get; private set; }
	[JsonPropertyName("op_post_id")]
	[JsonInclude]
	public int OpPostId { get; private set; }
	[JsonPropertyName("is_archived")]
	[JsonInclude]
	public bool IsArchived { get; private set; }
	[JsonPropertyName("archived_at")]
	[JsonInclude]
	public string? ArchivedAt { get; private set; }
	[JsonPropertyName("storage_path")]
	[JsonInclude]
	public string? StoragePath { get; private set; }
	[JsonPropertyName("thumbnail_path")]
	[JsonInclude]
	public string? ThumbnailPath { get; private set; }
	[JsonPropertyName("attachment")]
	[JsonInclude]
	public NijiuraChanAattachmentInternal? Attachment { get; private set; }
}

class NijiuraChanPostInternal {
	[JsonPropertyName("id")]
	[JsonInclude]
	public int Id { get; private set; }
	[JsonPropertyName("thread_id")]
	[JsonInclude]
	public int ThreadId { get; private set; }
	[JsonPropertyName("number_in_thread")]
	[JsonInclude]
	public int NumberInThread { get; private set; }
	[JsonPropertyName("name")]
	[JsonInclude]
	public string Name { get; private set; } = "";
	[JsonPropertyName("email")]
	[JsonInclude]
	public string Email { get; private set; } = "";
	[JsonPropertyName("subject")]
	[JsonInclude]
	public string Subject { get; private set; } = "";
	[JsonPropertyName("body")]
	[JsonInclude]
	public string Body { get; private set; } = "";
	[JsonPropertyName("host")]
	[JsonInclude]
	public string? Host { get; private set; }
	[JsonPropertyName("display_id")]
	[JsonInclude]
	public string? DisplayId { get; private set; }
	[JsonPropertyName("poster_id")]
	[JsonInclude]
	public string? PosterId { get; private set; }
	[JsonPropertyName("is_own_post")]
	[JsonInclude]
	public bool IsOwnPost { get; private set; }
	[JsonPropertyName("is_admin")]
	[JsonInclude]
	public int IsAdmin { get; private set; }
	[JsonPropertyName("show_id")]
	[JsonInclude]
	public bool ShowId { get; private set; }
	[JsonPropertyName("is_public_visible")]
	[JsonInclude]
	public bool IsPublicVisible { get; private set; }
	[JsonPropertyName("is_warned")]
	[JsonInclude]
	public bool IsWarned { get; private set; }
	[JsonPropertyName("is_exposed")]
	[JsonInclude]
	public bool IsExposed { get; private set; }
	[JsonPropertyName("created_at")]
	[JsonInclude]
	public string CreatedAt { get; private set; } = "";
	[JsonPropertyName("soudane_count")]
	[JsonInclude]
	public int SoudaneCount { get; private set; }
	[JsonPropertyName("del_count")]
	[JsonInclude]
	public int DelCount { get; private set; }
	[JsonPropertyName("attachment")]
	[JsonInclude]
	public NijiuraChanAattachmentInternal? Attachment { get; private set; }
}

class NijiuraChanAattachmentInternal {
	[JsonPropertyName("path")]
	[JsonInclude]
	public string Path { get; private set; } = "";
	[JsonPropertyName("thumbnail")]
	[JsonInclude]
	public string Thumbnail { get; private set; } = "";
	[JsonPropertyName("mime_type")]
	[JsonInclude]
	public string MimeType { get; private set; } = "";
	[JsonPropertyName("original_name")]
	[JsonInclude]
	public string? OriginalName { get; private set; }
	[JsonPropertyName("size")]
	[JsonInclude]
	public int Size { get; private set; }
}
