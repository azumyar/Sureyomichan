using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Haru.Kei.SureyomiChan; 

internal static class SureyomiChanEnviroment {
	public static string Scheme => "sureyomichan";

	public static string CommandOpen => "open";
	public static string[] SupportCommands => [
		CommandOpen,
	];

	public static string BoardImg => "img";
	public static string BoardAimg => "aimg";
	public static string[] SupportBoards => [
		BoardImg,
		BoardAimg,
	];

	public static int CopyDataTypeCommandArgs => 1;


	public static string GetStaticPath(SureyomiChanStaticItem target) {
		return target switch {
			SureyomiChanStaticItem.ConfigFile => GetPathConfig(),
			SureyomiChanStaticItem.LogFile => GetPathLog(),
			_ => throw new Exception(),
		};
	}

	public static string GetDynamicPath(SureyomiChanDynamicItem target, Models.Config config) {
		return target switch {
			//SureyomiChanDynamicItem.OriginalSavePath => GetPathConfig(config),
			_ => throw new Exception(),
		};
	}


	private static string GetPathConfig() => Path.Combine(AppContext.BaseDirectory, "config.json");
	private static string GetPathLog() => Path.Combine(AppContext.BaseDirectory, "sureyomichan.log");
}

enum SureyomiChanStaticItem {
	ConfigFile,
	LogFile
}

enum SureyomiChanDynamicItem {
}
