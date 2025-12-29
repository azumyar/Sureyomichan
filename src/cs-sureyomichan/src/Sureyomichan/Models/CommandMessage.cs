using System;
using System.Collections.Generic;
using System.Text;

namespace Haru.Kei.SureyomiChan.Models;

internal class BaseCommandMessage(object token) {
	public object Token => token;
}

internal class WindowShowMessage(object token) : BaseCommandMessage(token) { }

internal class WindowMinimizeMessage(object token) : BaseCommandMessage(token) { }

internal class ScrollMessage(object token, Bindables.BindableSureyomiChanModel scrollTarget) : BaseCommandMessage(token) {
	public Bindables.BindableSureyomiChanModel ScrollTarget => scrollTarget;
}