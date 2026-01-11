using System;
using System.Collections.Generic;
using System.Text;

namespace Haru.Kei.SureyomiChan.Models; 

interface IAttachmentData {
	public string AttachmentImage { get; }
}

internal interface IImageStore {
	public byte[]? Get(string board, int threadNo, string imageName);
	public void Insert(string board, int threadNo, string imageName, byte[] imageBytes);
	public void Remove(string board, int threadNo);
}
