using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Haru.Kei.SureyomiChan.Models.Bindables; 

class BindableSureyomiChanModel : INotifyPropertyChanged {

	public event PropertyChangedEventHandler? PropertyChanged;

	public IReadOnlyReactiveProperty<int> ResIndex { get; }
	public IReadOnlyReactiveProperty<string> No { get; }
	public IReadOnlyReactiveProperty<string> PostTime { get; }
	public IReadOnlyReactiveProperty<Visibility> EmailVisibility { get; }
	public IReadOnlyReactiveProperty<string> Email { get; }
	public IReadOnlyReactiveProperty<string> Body { get; }
	public IReadOnlyReactiveProperty<string?> Id { get; }
	public IReadOnlyReactiveProperty<bool> IsId { get; }

	public IReadOnlyReactiveProperty<Visibility> ImageErrorVisibility { get; }
	public IReadOnlyReactiveProperty<string?> ImageName { get; }
	public ReactivePropertySlim<ImageObject?> Image { get; }

	public SureyomiChanModel Model { get; }

	public ReactivePropertySlim<int> DeleteProgress { get; } = new(initialValue: 0);
	public ReactivePropertySlim<bool> IndeterminateDeleteProgress { get; } = new(initialValue: false);

	public IReadOnlyReactiveProperty<Visibility> ResVisibility { get; }
	public IReadOnlyReactiveProperty<Visibility> NgVisibility { get; }
	public IReadOnlyReactiveProperty<string> NgText { get; }

	public IReadOnlyReactiveProperty<Visibility> SendDelVisibility { get; }
	public IReadOnlyReactiveProperty<Visibility> DeleteResVisibility { get; }


	private IDisposable? deleteSubscriber = null;
	private double __deleteProgress = 0d;
	private const int DeleteGraceTimeMiliSec = 2000;
	private const int DeleteIntervalTimeMiliSec = 10;
	private ReactivePropertySlim<bool> IsNg { get; }
	private ReactivePropertySlim<Models.SureyomiChanDeleteType> DeleteType { get; }

	public BindableSureyomiChanModel(
		SureyomiChanModel model,
		(bool IsSucessed, AttachmentObject? Attachment)? attachment,
		ulong? dHash,
		bool isNg
		) {

		this.ResIndex = new ReactivePropertySlim<int>(initialValue: model.ResIndex);
		this.No = new ReactivePropertySlim<string>(initialValue: FormatNo(model));
		this.PostTime = new ReactivePropertySlim<string>(initialValue: model.FormatDateTime());
		this.EmailVisibility = new ReactivePropertySlim<Visibility>(initialValue: FormatEmailVisibility(model));
		this.Email = new ReactivePropertySlim<string>(initialValue: FormatEmail(model));
		this.Body = new ReactivePropertySlim<string>(initialValue: FormatBody(model));
		this.Id = new ReactivePropertySlim<string?>(initialValue: model.Id);
		this.ImageName = new ReactivePropertySlim<string?>(initialValue: attachment switch {
			{ } => model.ImageFileName,
			_ => "",
		});
		this.ImageErrorVisibility = new ReactivePropertySlim<Visibility>(initialValue: attachment switch {
			{ } v => v.IsSucessed switch {
				true => Visibility.Collapsed,
				_ => Visibility.Visible,
			},
			_ => Visibility.Collapsed,
		});
		this.Image = LoadImage(attachment?.Attachment, isNg);
		this.IsNg = new(initialValue: isNg);
		this.DeleteType = new(initialValue: model.DeleteType);

		this.SendDelVisibility = new ReactivePropertySlim<Visibility>(initialValue: model.Interaction.IsSupportSendDel switch {
			true => Visibility.Visible,
			_ => Visibility.Collapsed
		});
		this.DeleteResVisibility = new ReactivePropertySlim<Visibility>(initialValue: model.Interaction.IsSupportDeleteRes switch {
			true => Visibility.Visible,
			_ => Visibility.Collapsed
		});

		this.ResVisibility = this.DeleteType
			.Select(x => x switch {
				Models.SureyomiChanDeleteType.None => Visibility.Visible,
				_ => Visibility.Collapsed,
			}).ToReadOnlyReactivePropertySlim();
		this.NgVisibility = this.DeleteType
			.Select(x => x switch {
				Models.SureyomiChanDeleteType.None => Visibility.Collapsed,
				_ => Visibility.Visible,
			}).ToReadOnlyReactivePropertySlim();
		this.NgText = this.DeleteType
			.CombineLatest(this.IsNg, (x, ng) => (x, ng) switch {
				{ } v when v.ng => "NGレス",
				{ } v when v.x == Models.SureyomiChanDeleteType.DeleteFromOwner => "スレッドを立てた人によって削除されました",
				{ } v when v.x == Models.SureyomiChanDeleteType.DeleteFromDel => "削除依頼によって隔離されました",
				{ } v when v.x == Models.SureyomiChanDeleteType.SelfDelete => model.Body,
				_ => "",
			}).ToReadOnlyReactivePropertySlim<string>();
		this.IsId = this.Id.Select(x => x is { }).ToReadOnlyReactivePropertySlim();

		this.Model = model;
	}

	public void BeginDelete(Func<Task<bool>> callback) {
		void init() {
			this.deleteSubscriber?.Dispose();
			this.deleteSubscriber = null;
			this.__deleteProgress = 0;
			this.DeleteProgress.Value = 0;
		}

		if(this.deleteSubscriber != null) {
			init();
		} else {
			this.deleteSubscriber = Observable.Interval(TimeSpan.FromMilliseconds(DeleteIntervalTimeMiliSec))
				.ObserveOn(Reactive.Bindings.UIDispatcherScheduler.Default)
				.Subscribe(async _ => {
					this.__deleteProgress += ((double)DeleteIntervalTimeMiliSec / DeleteGraceTimeMiliSec);
					this.DeleteProgress.Value = (int)(Math.Min(1d, this.__deleteProgress) * 100);
					if(1d <= this.__deleteProgress) {
						init();

						this.IndeterminateDeleteProgress.Value = true;
						await callback.Invoke();
						this.IndeterminateDeleteProgress.Value = false;
					}
				});
		}
	}

	private static string FormatNo(SureyomiChanModel model) => $"No.{model.No}";
	private static Visibility FormatEmailVisibility(SureyomiChanModel model) => string.IsNullOrEmpty(model.Email) switch {
		false => Visibility.Visible,
		_ => Visibility.Collapsed,
	};

	private static string FormatEmail(SureyomiChanModel model) => model.Email switch {
		string v when !string.IsNullOrEmpty(v) => $"No.{v}",
		_ => "",
	};

	private static string FormatBody(SureyomiChanModel model) {
		var s1 = Regex.Replace(model.Body, @"<br>", Environment.NewLine,
			RegexOptions.IgnoreCase | RegexOptions.Multiline);
		var s2 = Regex.Replace(s1, @"<[^>]*>", "",
			RegexOptions.IgnoreCase | RegexOptions.Multiline);
		var s3 = System.Net.WebUtility.HtmlDecode(s2);

		return s3;
	}

	private static ReactivePropertySlim<ImageObject?> LoadImage(AttachmentObject? image, bool isNg) {
		var r = new ReactivePropertySlim<ImageObject?>(initialValue: null);
		if(!isNg && image?.ImageFileBytes != null) {
			Observable.Return(0)
				.ObserveOn(Reactive.Bindings.UIDispatcherScheduler.Default)
				//.ObserveOn(System.Reactive.Concurrency.ThreadPoolScheduler.Instance)
				.Select(_ => {
						return Path.GetExtension(image.ImageName).ToLower() switch {
							".png" => Utils.ImageUtil.LoadPng(image.ImageFileBytes),
							".webp" => Utils.ImageUtil.LoadWebp(image.ImageFileBytes),
							".gif" => Utils.ImageUtil.LoadGif(image.ImageFileBytes),
							_ => new ImageObject(BitmapFrame.Create(new MemoryStream(image.ImageFileBytes)))
						};
				})
				//.ObserveOn(Reactive.Bindings.UIDispatcherScheduler.Default)
				.Subscribe(
					x => r.Value = x,
					ex => {
						Utils.Logger.Instance.Error(ex);
					});
		}
		return r;
	}
}

// TODO: 場所をかえる
class ImageObject : INotifyPropertyChanged {
	public event PropertyChangedEventHandler? PropertyChanged;

	public BitmapSource ImageSource { get; }
	public Timeline? AnimationSource { get; }

	public ImageObject(BitmapSource image, Timeline? animation = null) {
		this.ImageSource = image;
		this.AnimationSource = animation;
	}
}