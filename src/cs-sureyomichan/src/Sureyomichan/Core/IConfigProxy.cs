using System;
using System.Collections.Generic;
using System.Text;

namespace Haru.Kei.SureyomiChan.Core;

interface IConfigProxy {
	public Models.Config Get();
}


class ConfigProxy : IConfigProxy {
	private Models.Config? config;

	public void Update(Models.Config? config) { 
		this.config = config;
	}

	public Models.Config Get() {
		return this.config ?? Models.Config.DefaultConfig;
	}
}
