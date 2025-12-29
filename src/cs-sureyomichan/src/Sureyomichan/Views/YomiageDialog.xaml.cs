using Prism.Events;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Haru.Kei.SureyomiChan.Views {
	/// <summary>
	/// YomiageDialog.xaml の相互作用ロジック
	/// </summary>
	public partial class YomiageDialog : UserControl {
		public YomiageDialog() {
			InitializeComponent();

			Utils.Singleton.Instance.PrismMessenger.GetEvent<PubSubEvent<Models.ScrollMessage>>()
				.Subscribe(async x => {
					// タイミングによってはまだListBoxの追加が適用されてないので一度UIスレッドを進める
					await Task.Delay(1);
					if(object.ReferenceEquals(this, x.Token)) {
						this.ReplyListBox.ScrollIntoView(x.ScrollTarget);
					}
				});

			this.Loaded += (_, e) => {
				// XAMLでバインドできない？
				if(this.DataContext is ViewModels.YomiageDialogViewModel viewModel) {
					viewModel.LoadedCommand.Execute(e);
				}
			};
		}
	}
}
