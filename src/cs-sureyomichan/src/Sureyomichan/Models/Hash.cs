using ControlzEx.Standard;
using Haru.Kei.SureyomiChan.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;
using System.Text;
using GdipBitmap = System.Drawing.Bitmap;
using GdipGraphics = System.Drawing.Graphics;
using GdipImage = System.Drawing.Image;

namespace Haru.Kei.SureyomiChan.Models;

class Hash(ulong hash, string name = "hash") {
	public ulong Value { get; } = hash;

	public static int operator ^(Hash c1, Hash c2) {
		return Distance(c1, c2);
	}

	private static int Distance(Hash h1, Hash h2) {
		static ulong count(ulong bits) {
			bits = (bits & 0x5555555555555555) + (bits >> 1 & 0x5555555555555555);
			bits = (bits & 0x3333333333333333) + (bits >> 2 & 0x3333333333333333);
			bits = (bits & 0x0f0f0f0f0f0f0f0f) + (bits >> 4 & 0x0f0f0f0f0f0f0f0f);
			bits = (bits & 0x00ff00ff00ff00ff) + (bits >> 8 & 0x00ff00ff00ff00ff);
			bits = (bits & 0x0000ffff0000ffff) + (bits >> 16 & 0x0000ffff0000ffff);
			bits = (bits & 0x00000000ffffffff) + (bits >> 32 & 0x00000000ffffffff);
			return bits;
		}
		return (int)count(h1.Value ^ h2.Value);
	}

	public override string ToString() {
		return $"{{{name}:{Value:x}}}";
	}
}

class DifferenceHash : Hash {
	private const int bitmapWidth = 9;
	private const int bitmapHeight = 8;

	public DifferenceHash(ulong hash) : base(hash, "dhash") { }


	public static DifferenceHash? From(string fileName, byte[] image) {
		using var bmp = Utils.ImageUtil.GenBitmap(fileName, image);
		if(bmp is { } b) {
			var data = GenPreImage(b);
			var span = new ReadOnlySpan<byte>(data);
			return new(CalculateHash(ref span, bitmapWidth, bitmapHeight));
		} else {
			return null;
		}
	}

	private static byte[] GenPreImage(GdipBitmap image) {
		using CompositeDisposable d = new CompositeDisposable();
		GdipBitmap bmp;
		if((image.Width == bitmapWidth) && (image.Height == bitmapHeight)) {
			bmp = image;
		} else {
			GdipBitmap input;
			// ベタに小さくすると品質がわるいので
			// 正方形にする
			// 徐々に小さくする
			// の手順を踏む
			// これでもChromeと結果がかなり違う(ハミング距離で20くらい！)
			if(image.Width == image.Height) {
				input = image;
			} else {
				input = new GdipBitmap(bitmapWidth, bitmapHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				using var g = GdipGraphics.FromImage(input);
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bicubic;
				g.DrawImage(image, 0, 0, input.Width, input.Height);
				d.Add(input);
			}

			while(12 < input.Width) {
				var sz = input.Width * 2 / 3;
				var b = new GdipBitmap(sz, sz, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				using var g = GdipGraphics.FromImage(b);
				g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bicubic;
				g.DrawImage(image, 0, 0, b.Width, b.Height);
				d.Add(b);
				input = b;
			}

			{
				bmp = new GdipBitmap(bitmapWidth, bitmapHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				using var g = GdipGraphics.FromImage(bmp);
				g.Clear(System.Drawing.Color.White);
				g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bicubic;
				g.DrawImage(image, 0, 0, bmp.Width, bmp.Height);
				d.Add(bmp);
			}
		}
		//bmp.Save(Path.Combine(AppContext.BaseDirectory, "aaa.png"));
		var bmpData = bmp.LockBits(
			new(0, 0, bmp.Width, bmp.Height),
			System.Drawing.Imaging.ImageLockMode.ReadOnly,
			System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		try {
			var bmpPixels = new byte[bmpData.Stride * bmpData.Height];
			Marshal.Copy(bmpData.Scan0, bmpPixels, 0, bmpPixels.Length);

			return bmpPixels;
		}
		finally {
			bmp.UnlockBits(bmpData);
		}
	}

	private static ulong CalculateHash(ref ReadOnlySpan<byte> data, int width, int height) {
		static double gray(byte r, byte g, byte b) => Math.Floor((r * 0.299) + (g * 0.587) + (b * 0.114));

		var bit = 1UL;
		var hash = 0UL;
		for(var y = 0; y < height; y++) {
			var yy = y * width * 4;
			var prv = gray(data[yy + 2], data[yy + 1], data[yy + 0]);
			for(var x = 1; x < width; x++) {
				var xx = y * width * 4 + x * 4;
				var cur = gray(data[xx + 2], data[xx + 1], data[xx + 0]);
				if(cur < prv) {
					hash |= bit;
				}
				bit = bit << 1;
				prv = cur;
			}
		}
		return hash;
	}
}
