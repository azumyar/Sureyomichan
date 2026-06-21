using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haru.Kei.SureyomiChan.Core;

class FutabaInteraction(string url, Models.__FutabaResData source, Helpers.FutabaApi api, IConfigProxy config) : Models.ISureyomiChanInteraction {
	public SureyomiChanBoardId BoardId => SureyomiChanBoardId.FutabaImg;
	public bool IsSupportSendDel => true;
	public bool IsSupportDeleteRes => true;

	public async Task<bool> DeleteResAction()
		=> await Utils.Util.AwaitObserver(api.PostDeleteSerial(source.ResNoInt, config.Get().FutabaPasswd), false);

	public async Task<bool> SendDelAction()
		=> await Utils.Util.AwaitObserver(api.PostDelSerial(url, source.ResNoInt), false);

	public async Task<IEnumerable<Models.AttachmentObject>> DownloadImages() {
		var name = Path.GetFileName(source.FileSource);
		if(string.IsNullOrEmpty(name)) {
			await Task.Yield();
			return [];
		}

		var image = await api.DonwloadImage(source);
		if(0 == image.Count()) {
			return [];
		}

		return [new Models.AttachmentObject() {
			IsUpdatedTegakiPng = true,
			FileName = name,
			ImageName = name,
			OriginalFileBytes = image,
			ImageFileBytes = image,
			Hash = Models.DifferenceHash.From(name, image),
		}];
	}
}

class NijiuraChanInteraction(string url, Models.NijiuraChanReplyV1 source, Helpers.NijiuraChanApi? api, IConfigProxy config) : Models.ISureyomiChanInteraction {
	public SureyomiChanBoardId BoardId => SureyomiChanBoardId.NijiuraChanAimg;
	public bool IsSupportSendDel => false;
	public bool IsSupportDeleteRes => false;

	public async Task<bool> DeleteResAction()
		=> await Task.FromResult(false);

	public async Task<bool> SendDelAction()
		=> await Task.FromResult(false);

	public async Task<IEnumerable<Models.AttachmentObject>> DownloadImages()
		=> await NijiuraChanUtil.DownloadImages(source.Image, source.Thumb, null);
}

class NijiuraChanInternalInteraction(string url, Models.NijiuraChanPostInternal source, Helpers.NijiuraChanApi? api, IConfigProxy config) : Models.ISureyomiChanInteraction {
	public SureyomiChanBoardId BoardId => SureyomiChanBoardId.NijiuraChanAimg;
	public bool IsSupportSendDel => false;
	public bool IsSupportDeleteRes => false;

	public async Task<bool> DeleteResAction()
		=> await Task.FromResult(false);

	public async Task<bool> SendDelAction()
		=> await Task.FromResult(false);

	public async Task<IEnumerable<Models.AttachmentObject>> DownloadImages()
		=> await NijiuraChanUtil.DownloadImages(source.Attachment?.Path, source.Attachment?.Thumbnail, source.Attachment?.IsOekaki);
}


file static class NijiuraChanUtil {
	public static async Task<IEnumerable<Models.AttachmentObject>> DownloadImages(string? imagePath, string? thumbnailPath, bool? isOekaki) {
		var fileName = imagePath ?? "";
		var imageName = imagePath ?? "";
		var orig = default(byte[]);
		var image = default(byte[]);

		if(string.IsNullOrEmpty(fileName)) {
			await Task.Yield();
			return [];
		}

		var httpClient = Utils.Singleton.Instance.HttpClient;
		var origUrl = $"https://nijiurachan.net/{imagePath}";
		var thumbUrl = $"https://nijiurachan.net/{thumbnailPath}";

		using var response1 = await Utils.Util.Http(() => httpClient.GetAsync(origUrl));
		orig = await response1.Content.ReadAsByteArrayAsync();
		if(Path.GetExtension(fileName).ToLower() switch {
			".mp4" => true,
			".webm" => true,
			_ => false,
		}) {
			using var response2 = await Utils.Util.Http(() => httpClient.GetAsync(thumbUrl));
			image = await response2.Content.ReadAsByteArrayAsync();
			imageName = thumbnailPath ?? "";
		} else {
			image = orig;
		}
		if(image is null || (0 == image.Count())) {
			return [];
		}

		return await Task.FromResult<IEnumerable<Models.AttachmentObject>>(
			[
				new Models.AttachmentObject() {
					IsUpdatedTegakiPng = isOekaki ?? Path.GetExtension(fileName).ToLower() == ".png",
					FileName = Path.GetFileName(fileName),
					ImageName = Path.GetFileName(imageName),
					OriginalFileBytes = orig,
					ImageFileBytes = image,
					Hash = image switch {
						{ } => Models.DifferenceHash.From(imageName, image),
						_ => default,
					}
				},
			]);
	}
}
