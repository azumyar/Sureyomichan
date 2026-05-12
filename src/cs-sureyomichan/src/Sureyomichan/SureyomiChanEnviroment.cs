using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Haru.Kei.SureyomiChan; 

internal static class SureyomiChanEnviroment {
	/// <summary>aimgはMaxRes判定がレスポンスにないようなので定数定義する</summary>
	public static int NijiuraChanMaxRes => 1000;
	public static string YomiageMaxResText => "最大レス数を超過しました。読み上げを停止します。";

	public static string Scheme => "sureyomichan";

	public static string CommandOpen => "open";
	public static string[] SupportCommands => [
		CommandOpen,
	];

	// 後で名前を返る
	public static IEnumerable<SureyomiChanBoardId> SupportBoards__ => [
		SureyomiChanBoardId.FutabaImg,
		SureyomiChanBoardId.NijiuraChanAimg,
	];

	//threadNoTxt

	public static int CopyDataTypeCommandArgs => 1;


	public static string GetStaticString(SureyomiChanStaticItem target) {
		return target switch {
			SureyomiChanStaticItem.ConfigFile => GetPathConfig(),
			SureyomiChanStaticItem.LogFile => GetPathLog(),
			_ => throw new Exception(),
		};
	}

	public static string GetStaticString(SureyomiChanBoardId target) {
		return target switch {
			SureyomiChanBoardId.FutabaImg => "img",
			SureyomiChanBoardId.NijiuraChanAimg => "aimg",
			_ => throw new Exception(),
		};
	}

	public static string GetStaticString(SureyomiChanBoardId target, SureyomiChanBoard__ v) {
		return target switch {
			SureyomiChanBoardId.FutabaImg => v switch {
				SureyomiChanBoard__.Command => "img",
				SureyomiChanBoard__.ThreadNoFileName => "threadno.img.txt",
				_ => throw new Exception(),
			},
			SureyomiChanBoardId.NijiuraChanAimg => v switch {
				SureyomiChanBoard__.Command => "aimg",
				SureyomiChanBoard__.ThreadNoFileName => "threadno.aimg.txt",
				_ => throw new Exception(),
			},
			_ => throw new Exception(),
		};
	}


	public static string GetDynamicString(SureyomiChanDynamicItem target, Models.Config config) {
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
	LogFile,
}

enum SureyomiChanBoardId {
	FutabaImg,
	NijiuraChanAimg
}

enum SureyomiChanBoard__ { // TODO:名前考える
	Command,
	ThreadNoFileName,
}

enum SureyomiChanDynamicItem {
}
