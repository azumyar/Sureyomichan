using ControlzEx.Standard;
using Haru.Kei.SureyomiChan.Core;
using Haru.Kei.SureyomiChan.Models.Bindables;
using MaterialDesignThemes.Wpf;
using Prism.Common;
using Prism.Dialogs;
using Prism.Events;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BindableConfig = Haru.Kei.SureyomiChan.Models.Bindables.BindableConfig;
using BindableSureyomi = Haru.Kei.SureyomiChan.Models.Bindables.BindableSureyomiChanModel;

namespace Haru.Kei.SureyomiChan.ViewModels; 
internal class YomiageDialogViewModel : BindableBase, IDialogAware {
	public record class DialogParams(
		string UrlString,
		Helpers.IApiUrl Url,
		int ThreadNo,
		bool IsLatest,
		Core.IConfigProxy Config,
		Core.UiMessageMultiDispatcher Dispatcher,
		Core.BouyomiChan Bouyomi,
		Core.AttachmentWriter AttachmentWriter,
		Core.TegakiSaveStore Store,
		Core.WebView2Proxy WebView,
		Core.SureyomiChanNgProcesser Ng);
	enum ProcessState {
		Sucess,
		Fail,
		Running
	}
	internal class BaseCommandMessage { }
	internal class ScrollMessage(BindableSureyomi scrollTarget) : BaseCommandMessage {
		public BindableSureyomi ScrollTarget => scrollTarget;
	}

	public string Title {
		get {
			if(field == null) {
				return "";
			}
			return field;
		}
		set {
			this.RaisePropertyChanged(nameof(Title));
			field = value;
		}
	}


	public DialogCloseListener RequestClose { get; }
	public SnackbarMessageQueue SnackbarMessageQueue { get; } = new();


	private ReactivePropertySlim<bool> TegakiSaveRunning { get; } = new(initialValue: false);
	private ReactivePropertySlim<ProcessState> ApiState { get; } = new(initialValue: ProcessState.Sucess);
	private readonly ReactivePropertySlim<Core.SureyomiChanApiLooper?> api = new(initialValue: null);
	public ReadOnlyReactivePropertySlim<bool> IsYomiageRun { get; }

	public ReactiveCollection<BindableSureyomi> Replies { get; } = [];
	public ReactivePropertySlim<string> ThreadDieText { get; } = new(initialValue: "");
	public ReactivePropertySlim<string> Url { get; } = new(initialValue: "");
	public ReactivePropertySlim<bool> ListBoxAutoScroll { get; } = new(initialValue: true);

	public ReadOnlyReactivePropertySlim<Visibility> YomiageStatusStop { get; }
	public ReadOnlyReactivePropertySlim<Visibility> YomiageStatusRunning { get; }
	public ReadOnlyReactivePropertySlim<Visibility> TegakiSaveStatusStop { get; }
	public ReadOnlyReactivePropertySlim<Visibility> TegakiSaveStatusRunning { get; }
	public ReadOnlyReactivePropertySlim<Visibility> ApiStatusSucess { get; }
	public ReadOnlyReactivePropertySlim<Visibility> ApiStatusFail { get; }
	public ReadOnlyReactivePropertySlim<Visibility> ApiStatusRunning { get; }


	public ReactiveCommandSlim<RoutedEventArgs> LoadedCommand { get; } = new();
	public ReactiveCommandSlim ClickYomiageCommand { get; } = new();
	public ReactiveCommandSlim<RoutedEventArgs> SendDelCommand { get; } = new();
	public ReactiveCommandSlim<RoutedEventArgs> DeleteResCommand { get; } = new();

	private readonly Core.UiMessageDispatcher uiMsgDispatcher;
	private object viewToken = new();
	private DialogParams? param;
	public YomiageDialogViewModel() {
		this.uiMsgDispatcher = new() {
			OnBeginApi = () => this.ApiState.Value = ProcessState.Running,
			OnEndApi = (sucessed) => this.ApiState.Value = sucessed switch {
				true => ProcessState.Sucess,
				_ => ProcessState.Fail
			},
			OnNewReplies = (x) => {
				if(x.Any()) {
					this.Replies.AddRange(x);
					if(this.ListBoxAutoScroll.Value) {
						Utils.Singleton.Instance.PrismMessenger.GetEvent<PubSubEvent<Models.ScrollMessage>>()
							.Publish(new(this.viewToken, x.Last()));
					}
				}
			},
			OnUpdateDieTime = (c, d) => {
				var ts = d - c;
				var tt = DateTime.Now.Add(ts); // 消滅時間表示はPCの時計を使用
				this.ThreadDieText.Value = ts switch {
					TimeSpan y when y.TotalSeconds < 0 => $"スレ消滅：{Math.Abs(ts.TotalSeconds):00}秒経過(消滅時間を過ぎました)",
					TimeSpan y when 0 < y.Days => $"スレ消滅：{tt.ToString("MM/dd")}(あと{ts.ToString(@"dd\日hh\時\間")})",
					TimeSpan y when 0 < y.Hours => $"スレ消滅：{tt.ToString("HH:mm")}(あと{ts.ToString(@"hh\時\間mm\分")})",
					TimeSpan y when 0 < y.Minutes => $"スレ消滅：{tt.ToString("HH:mm")}(あと{ts.ToString(@"mm\分ss\秒")})",
					_ => $"スレ消滅：{tt.ToString("HH:mm")}(あと{ts.ToString(@"ss\秒")})",
				};
			},
			OnThreadDied = () => {
				this.ThreadDieText.Value = "スレッドが落ちました";
				StopYomiage();
			},
			OnBouyomiChanNotFound = () => {
				if(this.api.Value != null) {
					this.EnqueueErrorMessage("棒読みちゃんが見つかりません。読み上げを停止します");
					StopYomiage();
				}
			},
			OnSartDisplayTegaki = () => this.TegakiSaveRunning.Value = true,
			OnEndDisplayTegaki = () => this.TegakiSaveRunning.Value = false,
			OnErrorTegaki = (m) => { },
		};
		this.TegakiSaveStatusStop = this.TegakiSaveRunning.Select(x => x switch {
			false => Visibility.Visible,
			_ => Visibility.Collapsed,
		}).ToReadOnlyReactivePropertySlim();
		this.TegakiSaveStatusRunning = this.TegakiSaveRunning.Select(x => x switch {
			true => Visibility.Visible,
			_ => Visibility.Collapsed,
		}).ToReadOnlyReactivePropertySlim();

		this.ApiStatusSucess = this.ApiState.Select(x => x switch {
			ProcessState.Sucess => Visibility.Visible,
			_ => Visibility.Collapsed,
		}).ToReadOnlyReactivePropertySlim();
		this.ApiStatusFail = this.ApiState.Select(x => x switch {
			ProcessState.Fail => Visibility.Visible,
			_ => Visibility.Collapsed,
		}).ToReadOnlyReactivePropertySlim();
		this.ApiStatusRunning = this.ApiState.Select(x => x switch {
			ProcessState.Running => Visibility.Visible,
			_ => Visibility.Collapsed,
		}).ToReadOnlyReactivePropertySlim();
		this.IsYomiageRun = this.api.Select(x => x switch {
			{ } => true,
			_ => false,
		}).ObserveOn(UIDispatcherScheduler.Default)
		.ToReadOnlyReactivePropertySlim();
		this.YomiageStatusStop = this.IsYomiageRun.Select(x => x switch {
			false => Visibility.Visible,
			_ => Visibility.Collapsed,
		}).ToReadOnlyReactivePropertySlim();
		this.YomiageStatusRunning = this.IsYomiageRun.Select(x => x switch {
			true => Visibility.Visible,
			_ => Visibility.Collapsed,
		}).ToReadOnlyReactivePropertySlim();

		this.LoadedCommand.Subscribe(x => this.OnLoaded(x));
		this.SendDelCommand.Subscribe(x => this.OnDendDel(x));
		this.DeleteResCommand.Subscribe(x => this.OnDeleteRes(x));
		this.ClickYomiageCommand.Subscribe(_ => this.OnYomiage());
	}

	public bool CanCloseDialog() {
		return true;
	}

	public void OnDialogOpened(IDialogParameters parameters) {
		this.param = parameters.GetValue<DialogParams>(nameof(DialogParams));
		this.param.Dispatcher.Register(this.uiMsgDispatcher);

		this.Title = this.Url.Value = this.param.UrlString;
		this.StartYomiage(this.param.IsLatest);
	}

	public void OnDialogClosed() {
		if(this.api.Value != null) {
			this.StopYomiage();
			if(this.param is { }) {
				this.param.Store.Clear(this.param.ThreadNo);
			}
		}
	}


	private bool StartYomiage(bool isLatest) {
		Task<bool> sageIsNgFromBody(Models.SureyomiChanModel it) => this.param.Ng?.IsNgFromBody(it)!;
		Task<bool> safeIsNgFromImage(Models.DifferenceHash? dhash) => dhash switch {
			{ } v => this.param.Ng?.IsNgFromImage(v)!,
			_ => Task.FromResult(false),
		};
		if(this.param == null) {
			Utils.Logger.Instance.Error($"！！整合性エラー読み上げパラメータが初期化されていません！！");
			return false;
		}
		if(this.api.Value != null) {
			Utils.Logger.Instance.Error($"！！整合性エラー読み上げ開始にapiがnullではありません！！");
			return false;
		}

		Utils.Logger.Instance.Info($"読み上げを開始します => {this.param.Url.Name}, {this.param.ThreadNo}");
		var yomiage = new Core.Yomiage(this.param.Bouyomi, this.param.Config);
		this.ThreadDieText.Value = "";
		try {
			api.Value = new Core.SureyomiChanApiLooper(
				this.param.UrlString,
				this.param.Url,
				this.param.ThreadNo,
				this.uiMsgDispatcher,
				this.param.Config,
				this.param.WebView);
			yomiage.SpeakStarted();

			api.Value.Run(
				callBack: async (x, skip) => {
					void yomiSpeak(Models.SureyomiChanModel m) {
						if(!skip) {
							yomiage.EnqueueSpeak(m);
						}
					}
					void yomiImage() {
						if(!skip) {
							yomiage.SaveImage();
						}
					}
					bool isOld() => (x.DieTime - x.CurrentTime).TotalMilliseconds < this.param.Config.Get().YomiageOldTime;

					var speak = new List<Models.SureyomiChanModel>();
					var disp = new List<BindableSureyomi>();
					var images = new List<byte[]>();
					foreach(var it in x.NewReplies) {
						var attachment = default((bool IsSucessed, Models.AttachmentObject? Attachment)?);
						var dHash = default(Models.DifferenceHash?);
						var isNg = await sageIsNgFromBody(it);
						if(!isNg) {
							if(it.ImageFileName is { }) {
								try {
									var di = await it.Interaction.DownloadImage();
									if(di.ImageFileBytes is { }) {
										dHash = Models.DifferenceHash.From(it.ImageFileName, di.ImageFileBytes);
										isNg = await safeIsNgFromImage(dHash);
									}
									attachment = (true, di);
								}
								catch(Exceptions.SureyomiChanException ex) {
									Utils.Logger.Instance.Error(ex);
									attachment = (false, null);
								}
								await Task.Delay(500);
							}
						}

						if(!isNg) {
							yomiSpeak(it);
							if(attachment?.Attachment is { } att) {
								yomiImage();
								await this.param.AttachmentWriter.Save(it, att);
							}
							if(this.param.Config.Get().IsEnabledUpFile) {
								var _ = this.param.AttachmentWriter.DownloadShio(it);
							}
						}
						disp.Add(new(it, attachment, dHash?.Value, isNg));
						this.param.Store.Add(this.param.ThreadNo, it, isNg);
					}

					await this.param.AttachmentWriter.UpdateThreadNo(x);
					if(x.SupportFeature.IsSupportThreadOld && isOld()) {
						yomiage.SpeakOld();
					}
					if(x.SupportFeature.IsSupportThreadDie && !x.IsAlive) {
						yomiage.SpeakDead();
						await this.param.AttachmentWriter.DeadThreadNo(x);
					}

					uiMsgDispatcher.DispatchNewRiplies(disp);
				},
				skipToLast: isLatest,
				latestResNo: this.Replies.LastOrDefault()?.Model.No);
			return true;
		}
		catch(NotSupportedException e) {
			// URLが不正
			this.EnqueueErrorMessage($"[{this.param.Url.Name}]はサポートされていない読み上げURLです");
			Utils.Logger.Instance.Error(e);

			return false;
		}
	}

	private void StopYomiage() {
		if(this.api.Value == null) {
			Utils.Logger.Instance.Error($"！！整合性エラー読み上げを停止しようとしましたがapiがnullです！！");
			return;
		}

		Utils.Logger.Instance.Info($"読み上げを停止します");

		api.Value.Dispose();
		api.Value = null;
	}

	private void EnqueueErrorMessage(string message) => this.SnackbarMessageQueue.Enqueue(message);

	private void OnLoaded(RoutedEventArgs e) {
		this.viewToken = e.Source;
	}

	private async void OnDendDel(RoutedEventArgs e) {
		if(e.Source is FrameworkElement el && el.DataContext is BindableSureyomi m) {
			try {
				Utils.Logger.Instance.Info("delを送信");
				var r = await m.Model.Interaction.SendDelAction();
				Utils.Logger.Instance.Info($"delを送信しました => {r}");
			}
			catch(Exceptions.SureyomiChanException ex) {
				Utils.Logger.Instance.Error(ex);
			}
		}
	}

	private void OnDeleteRes(RoutedEventArgs e) {
		if(this.param == null) {
			Utils.Logger.Instance.Error($"！！整合性エラー読み上げパラメータが初期化されていません！！");
			return;
		}

		if(e.Source is FrameworkElement el && el.DataContext is BindableSureyomi m) {
			m.BeginDelete(async () => {
				var r = false;
				try {
					this.param.Store.MarkNg(m.Model.No);

					Utils.Logger.Instance.Info("レス削除を呼び出します");
					r = await m.Model.Interaction.DeleteResAction();
					Utils.Logger.Instance.Info($"レス削除を呼び出しました => {r}");
					if(!r) {
						this.EnqueueErrorMessage($"レス[{m.Model.No}]の削除に失敗しました");
					}
				}
				catch(Exceptions.SureyomiChanException ex) {
					Utils.Logger.Instance.Error(ex);
				}
				return r;
			});
		}
	}

	private void OnYomiage() {
		if(api.Value == null) {
			this.StartYomiage(false);
		} else {
			this.StopYomiage();
		}
	}
}
