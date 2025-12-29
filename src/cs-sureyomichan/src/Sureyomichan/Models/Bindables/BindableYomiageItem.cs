using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Haru.Kei.SureyomiChan.Models.Bindables; 
internal class BindableYomiageItem(string url) : System.ComponentModel.INotifyPropertyChanged {
	public event PropertyChangedEventHandler? PropertyChanged;

	public IReadOnlyReactiveProperty<string> Url { get; } = new ReactivePropertySlim<string>(initialValue: url);
}
