
using LibAPNG;
using LibAPNG.WPF;
using LiteDB;
using Lucene.Net.Documents;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;
using WpfAnimatedGif.Decoding;

using GdipBitmap = System.Drawing.Bitmap;
using GdipGraphics = System.Drawing.Graphics;
using GdipImage = System.Drawing.Image;
using ImgObj = Haru.Kei.SureyomiChan.Models.Bindables.ImageObject;

namespace Haru.Kei.SureyomiChan.Utils;

internal class ImageUtil {
	class ImageStoreImpl : Models.IImageStore {
		private const string CacheFile = "image-cache.db";
		private const string CacheTable = "image";
		private string __DbFile => Path.Combine(AppContext.BaseDirectory, CacheFile);

		public ImageStoreImpl() {
			if(File.Exists(this.__DbFile)) {
				try {
					File.Delete(this.__DbFile);
				}
				catch { }
			}
		}

		public byte[]? Get(string board, int threadNo, string imageName) {
			try {
				using var db = new LiteDatabase(__DbFile);
				var ic = db.GetCollection<CacheObject>(CacheTable)
					.FindOne(x => (x.Board == board)
						&& (x.ThreadNo == threadNo)
						&& (x.FileName == imageName));
				return ic?.ImageBytes;
			}
			catch(LiteDB.LiteException e) {
				Logger.Instance.Error(e);
				return null;
			}
		}

		public void Insert(string board, int threadNo, string imageName, byte[] imageBytes) {
			using var db = new LiteDatabase(__DbFile);
			db.BeginTrans();
			try {
				db.GetCollection<CacheObject>(CacheTable)
					.Insert(new CacheObject() {
						Board = board,
						ThreadNo = threadNo,
						Time = DateTime.Now,
						ImageBytes = imageBytes,
						FileName = imageName
					});
				db.Commit();
			}
			catch(LiteDB.LiteException e) {
				Logger.Instance.Error(e);
				db.Rollback();
			}
		}

		public void Remove(string board, int threadNo) {
			using var db = new LiteDatabase(__DbFile);
			db.BeginTrans();
			try {
				db.GetCollection<CacheObject>(CacheTable)
					.DeleteMany(x => (x.Board == board) && (x.ThreadNo == threadNo));
				db.Commit();
			}
			catch(LiteDB.LiteException e) {
				Logger.Instance.Error(e);
				db.Rollback();
			}
		}
	}

	public static Models.IImageStore ImageStore {
		get {
			if(field == null) {
				field = new ImageStoreImpl();
			}
			return field;
		}
	}

	public static ImgObj LoadPng(byte[] image) {
		// 8bitインデックスカラーPNGが読めないので対策する
		static Stream fixIndexdPng(byte[] imageBytes) {
			var stream = new MemoryStream();

			using var ms = new MemoryStream(imageBytes);
			using var bmp = new System.Drawing.Bitmap(ms);
			bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

			stream.Seek(0, SeekOrigin.Begin);
			return stream;
		}

		var p = new LibAPNG.APNG(image);

		var img = BitmapFrame.Create(
			((p.IHDRChunk.ColorType == 3) && p.IsSimplePNG) switch {
				true => fixIndexdPng(image),
				false => p.DefaultImage.GetStream(),
			},
			BitmapCreateOptions.None,
			BitmapCacheOption.OnLoad);
		if(p.IsSimplePNG) {
			return new ImgObj(img);
		} else {
			var animation = p.ToAnimation();
			var storyboard = new Storyboard();
			Storyboard.SetTargetProperty(
				animation,
				new PropertyPath(System.Windows.Controls.Image.SourceProperty));
			storyboard.Children.Add(animation);
			return new ImgObj(img, storyboard);
		}
	}

	public static ImgObj LoadGif(byte[] image) {
		using var stream = new MemoryStream(image);
		if(BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad) is GifBitmapDecoder decoder) {
			try {
				if(decoder?.Metadata != null) {
					if(!decoder.Frames.Skip(1).Any()) {
						return new(decoder.Frames.First());
					}

					stream.Seek(0, SeekOrigin.Begin);
					var gifFile = GifFile.ReadGifFile(stream, true);
					var width = gifFile.Header.LogicalScreenDescriptor.Width;
					var height = gifFile.Header.LogicalScreenDescriptor.Height;
					int index = 0;
					var keyFrames = new ObjectKeyFrameCollection();
					var totalDuration = TimeSpan.Zero;
					var baseFrame = default(BitmapSource);
					foreach(var rawFrame in decoder.Frames) {
						static bool isFull(GifImageDescriptor metadata, int w, int h) {
							return metadata.Left == 0
								   && metadata.Top == 0
								   && metadata.Width == w
								   && metadata.Height == h;
						}
						static BitmapSource clear(BitmapSource frame, GifImageDescriptor metadata) {
							DrawingVisual visual = new DrawingVisual();
							using(var context = visual.RenderOpen()) {
								var fullRect = new Rect(0, 0, frame.PixelWidth, frame.PixelHeight);
								var clearRect = new Rect(metadata.Left, metadata.Top, metadata.Width, metadata.Height);
								var clip = Geometry.Combine(
									new RectangleGeometry(fullRect),
									new RectangleGeometry(clearRect),
									GeometryCombineMode.Exclude,
									null);
								context.PushClip(clip);
								context.DrawImage(frame, fullRect);
							}

							var bitmap = new RenderTargetBitmap(
									frame.PixelWidth, frame.PixelHeight,
									frame.DpiX, frame.DpiY,
									PixelFormats.Pbgra32);
							bitmap.Render(visual);

							var result = new WriteableBitmap(bitmap);

							if(result.CanFreeze && !result.IsFrozen)
								result.Freeze();
							return result;
						}

						var ggce = gifFile.Frames[index].Extensions.OfType<GifGraphicControlExtension>().FirstOrDefault();
						var metadata = new {
							Descriptor = gifFile.Frames[index].Descriptor,
							Left = gifFile.Frames[index].Descriptor.Left,
							Top = gifFile.Frames[index].Descriptor.Top,
							Width = gifFile.Frames[index].Descriptor.Width,
							Height = gifFile.Frames[index].Descriptor.Height,
							Delay = ggce switch {
								GifGraphicControlExtension v when v.Delay != 0 => TimeSpan.FromMilliseconds(v.Delay),
								_ => TimeSpan.FromMilliseconds(100),
							},
							DisposalMethod = ggce switch {
								GifGraphicControlExtension v => (ImageBehavior.FrameDisposalMethod)v.DisposalMethod,
								_ => ImageBehavior.FrameDisposalMethod.None
							}
						};
						BitmapSource frame;
						if((baseFrame == null) && isFull(metadata.Descriptor, width, height)) {
							frame = rawFrame;
						} else {
							var visual = new DrawingVisual();
							using(var context = visual.RenderOpen()) {
								if(baseFrame != null) {
									context.DrawImage(baseFrame, new Rect(0, 0, width, height));
								}

								var rect = new Rect(metadata.Left, metadata.Top, metadata.Width, metadata.Height);
								context.DrawImage(rawFrame, rect);
							}
							var bitmap = new RenderTargetBitmap(
								width, height,
								96, 96,
								PixelFormats.Pbgra32);
							bitmap.Render(visual);

							frame = new WriteableBitmap(bitmap);
							if(frame.CanFreeze && !frame.IsFrozen) {
								frame.Freeze();
							}
						}

						keyFrames.Add(new DiscreteObjectKeyFrame(frame, totalDuration));
						totalDuration += metadata.Delay;
						baseFrame = metadata switch {
							var v when(v.DisposalMethod == ImageBehavior.FrameDisposalMethod.None) => frame,
							var v when(v.DisposalMethod == ImageBehavior.FrameDisposalMethod.DoNotDispose) => frame,
							var v when(v.DisposalMethod == ImageBehavior.FrameDisposalMethod.RestoreBackground) && isFull(v.Descriptor, width, height) => null,
							var v when(v.DisposalMethod == ImageBehavior.FrameDisposalMethod.RestoreBackground) => clear(frame, v.Descriptor),
							var v when(v.DisposalMethod == ImageBehavior.FrameDisposalMethod.RestorePrevious) => baseFrame,
							var v when((4 <= (int)v.DisposalMethod) && ((int)v.DisposalMethod <= 7)) => frame, // 4-7は仕様で未定義なのでとりあえずnoneとしてふるまう
							_ => throw new ArgumentException(),
						};
						index++;
					}
					// 無限に固定
					//var repeatCount = gifFile.RepeatCount;
					return new(decoder.Frames.First(), new ObjectAnimationUsingKeyFrames {
						KeyFrames = keyFrames,
						Duration = totalDuration,
						RepeatBehavior = RepeatBehavior.Forever,
						SpeedRatio = 1.0
					});
				}
			}
			catch { }
		}
		throw new Exceptions.ImageNotSupportException();
	}

	public static ImgObj LoadWebp(byte[] image) {
		static IEnumerable<int> keyframe(byte[] image) {
			List<int> delay = new List<int>();

			// WEBP
			if(System.Text.Encoding.ASCII.GetString(image, 0, 4).Equals("RIFF")) {
				for(int i = 12; i < image.Length - 8; i += 8) {
					string chname = System.Text.Encoding.ASCII.GetString(image, i, 4);
					int chsize = BitConverter.ToInt32(image, i + 4);

					if(chname.Equals("ANMF")) {
						int offset = i + 8 + 12;
						byte[] dbuff = { image[offset], image[offset + 1], image[offset + 2], 0 };
						delay.Add(BitConverter.ToInt32(dbuff, 0));
					}
					i += chsize;
				}
			}

			return delay.AsReadOnly();
		}

		using var ms = new MemoryStream(image);
		var d = BitmapDecoder.Create(
			ms,
			BitmapCreateOptions.None,
			BitmapCacheOption.OnLoad);
		var img = d.Frames[0];
		if(d.Frames.Count == 1) {
			return new ImgObj(img);
		} else {
			var kf = keyframe(image);
			if(kf.Count() != d.Frames.Count) {
				Utils.Logger.Instance.Error("フレーム数が一致しません");
			}
			var animation = new ObjectAnimationUsingKeyFrames();
			animation.RepeatBehavior = RepeatBehavior.Forever;
			var keyTime = TimeSpan.Zero;
			for(var i = 0; i < d.Frames.Count; i++) {
				animation.KeyFrames.Add(new DiscreteObjectKeyFrame() {
					KeyTime = keyTime,
					Value = d.Frames[i],
				});
				keyTime += TimeSpan.FromMilliseconds(kf.ElementAt(i));
			}
			animation.Duration = keyTime;

			var storyboard = new Storyboard();
			Storyboard.SetTargetProperty(
				animation,
				new PropertyPath(System.Windows.Controls.Image.SourceProperty));
			storyboard.Children.Add(animation);
			return new ImgObj(img, storyboard);
		}
	}


	public static GdipBitmap? GenBitmap(string fileName, byte[] image) {
		static GdipImage? gen(string fileName, byte[] image)
			=> Path.GetExtension(fileName)?.ToLower() switch {
				".png" => GdipBitmap.FromStream(new MemoryStream(image)),
				".jpg" => GdipBitmap.FromStream(new MemoryStream(image)),
				".jpeg" => GdipBitmap.FromStream(new MemoryStream(image)),
				".gif" => GdipBitmap.FromStream(new MemoryStream(image)),

				_ => null
			};
		return gen(fileName, image) switch {
			{ } v => new GdipBitmap(v),
			_ => null
		};
	}
}
file class CacheObject {
	public string Board { get; set; } = "";
	public int ThreadNo { get; set; }
	public DateTime Time { get; set; }
	public string FileName { get; set; } = "";
	public byte[] ImageBytes { get; set; } = new byte[0];
}