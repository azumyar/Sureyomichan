using System;
using System.Collections.Generic;
using System.Text;

namespace Haru.Kei.SureyomiChan.Models; 

internal record class NgResult(bool IsNg, string ReplaceBody) {
	public static NgResult Default => new(false, "");
}
