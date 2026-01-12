using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Haru.Kei.SureyomiChan.Models;

class SureyomiChanResponse {
	public required int ThreadNo { get; init; }
	public required string ThreadNoTxt { get; init; }
	public required bool IsAlive { get; init; }
	public required DateTime CurrentTime { get; init; }
	public required DateTime DieTime { get; init; }
	public required IEnumerable<SureyomiChanModel> NewReplies { get; init; }
	public required ISureyomiChanFeature SupportFeature { get; init; }
}

class SureyomiChanModel(
	int threadNo,
	int resIndex,
	int no,
	DateTime postTime,
	string email,
	string body,
	SureyomiChanDeleteType deleteType,

	// 画像関係いれる
	string? imageFileName,
	string? imageSource,
	string? thumbnailSource,

	string? id,
	IEnumerable<Models.Token> token,
	ISureyomiChanInteraction interaction) {

	public int ThreadNo { get; } = threadNo;
	public int ResIndex { get; } = resIndex;
	public int No { get; } = no;
	public DateTime PostTime { get; } = postTime;
	public string Email { get; } = email;
	public string Body { get; } = body;
	public string? Id { get; } = id;
	public bool HasId { get; } = id is not null;

	public SureyomiChanDeleteType DeleteType { get; } = deleteType;

	public string? ImageFileName { get; } = imageFileName;
	public string? ImageSource{ get; } = imageSource;
	public string? ThumbnailSource { get; } = thumbnailSource;

	public IEnumerable<Models.Token> Token { get; } = token;
	public ISureyomiChanInteraction Interaction { get; } = interaction;
}

interface ISureyomiChanFeature {
	public bool IsSupportThreadOld { get; }
	public bool IsSupportThreadDie { get; }
}

interface ISureyomiChanInteraction {
	SureyomiChanBoardId BoardId { get; }
	bool IsSupportSendDel { get; }
	bool IsSupportDeleteRes { get; }

	Task<bool> SendDelAction();
	Task<bool> DeleteResAction();

	Task<AttachmentObject> DownloadImage();
}

enum SureyomiChanDeleteType {
	None,
	/// <summary>スレッドを立てた人によって削除されました</summary>
	DeleteFromOwner,
	/// <summary>削除依頼によって隔離されました</summary>
	DeleteFromDel,
	/// <summary>本人が削除</summary>
	SelfDelete,
}