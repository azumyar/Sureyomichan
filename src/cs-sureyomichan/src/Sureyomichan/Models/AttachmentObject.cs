using System;
using System.Collections.Generic;
using System.Text;

namespace Haru.Kei.SureyomiChan.Models;

/// <summary>img/aimgの添付ファイルを抽象化する</summary>
class AttachmentObject {
	public required bool IsUpdatedTegakiPng { get; init; }
	public required string FileName { get; init; }
	public required string ImageName { get; init; }
	public required byte[]? OriginalFileBytes { get; init; }
	public required byte[]? ImageFileBytes { get; init; }
}

