using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Haru.Kei.SureyomiChan.Core;

class FutabaInteraction(string url, Models.__FutabaResData source, Helpers.FutabaApi api, IConfigProxy config) : Models.ISureyomiChanInteraction {
	public string BoardName => "img";
	public bool IsSupportSendDel => true;
	public bool IsSupportDeleteRes => true;

	public async Task<bool> DeleteResAction()
		=> await Utils.Util.AwaitObserver(api.PostDeleteSerial(source.ResNoInt, config.Get().FutabaPasswd), false);

	public async Task<bool> SendDelAction()
		=> await Utils.Util.AwaitObserver(api.PostDelSerial(url, source.ResNoInt), false);

	public async Task<Models.AttachmentObject> DownloadImage() {
		var name = Path.GetFileName(source.FileSource);
		var image = await api.DonwloadImage(source);
		return new Models.AttachmentObject() {
			IsUpdatedTegakiPng = true,
			FileName = name,
			ImageName = name,
			OriginalFileBytes = image,
			ImageFileBytes = image,
		};
	}
}

class NijiuraChanInteraction(string url, Models.NijiuraChanReplyV1 source, Helpers.NijiuraChanApi? api, IConfigProxy config) : Models.ISureyomiChanInteraction {
	public string BoardName => "aimg";
	public bool IsSupportSendDel => false;
	public bool IsSupportDeleteRes => false;

	public async Task<bool> DeleteResAction()
		=> await Task.FromResult(false);

	public async Task<bool> SendDelAction()
		=> await Task.FromResult(false);

	public async Task<Models.AttachmentObject> DownloadImage()
		=> await NijiuraChanUtil.DownloadImage(source.Image, source.Thumb);
}

class NijiuraChanInternalInteraction(string url, Models.NijiuraChanPostInternal source, Helpers.NijiuraChanApi? api, IConfigProxy config) : Models.ISureyomiChanInteraction {
	public string BoardName => "aimg";
	public bool IsSupportSendDel => false;
	public bool IsSupportDeleteRes => false;

	public async Task<bool> DeleteResAction()
		=> await Task.FromResult(false);

	public async Task<bool> SendDelAction()
		=> await Task.FromResult(false);

	public async Task<Models.AttachmentObject> DownloadImage()
		=> await NijiuraChanUtil.DownloadImage(source.Attachment?.Path, source.Attachment?.Thumbnail);
}


file static class NijiuraChanUtil {
	public static async Task<Models.AttachmentObject> DownloadImage(string? imagePath, string? thumbnailPath) {
		var fileName = imagePath ?? "";
		var imageName = imagePath ?? "";
		var orig = default(byte[]);
		var image = default(byte[]);
		if(!string.IsNullOrEmpty(fileName)) {
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
		}

		return await Task.FromResult(new Models.AttachmentObject() {
			IsUpdatedTegakiPng = Path.GetExtension(fileName).ToLower() == ".png",
			FileName = Path.GetFileName(fileName),
			ImageName = Path.GetFileName(imageName),
			OriginalFileBytes = orig,
			ImageFileBytes = image,
		});
	}
}
