// all.execute.first
// プラグイン処理(全てのプラグインの最初に動く処理、疎通確認、初期処理用)
// スタブ
// Ver2.1.0

export const pluginList = [
  {
    point: "load",
    sort: "001",
    execute: function (param) {
      let result = { isStop: false, isError: false, message: "", resultValue: "" };
      console.log("load all.execute.first", param.from);
      return result;
    }
  },
  {
    point: "start",
    sort: "001",
    execute: function (param) {
      let result = { isStop: false, isError: false, message: "", resultValue: "" };
      // パラメータ名
      const paramName = {
        autoRun: '自動開始',
        keyword: '開始条件',
        autoLive: 'FK自動更新開始',
        overwrite: '透過png上書',
        saveSpan: '表示時間',
        fixWidth: '横幅固定',
        widthPx: '横幅(px)',
        otherSave: '元ファイル別保存',
        otherDir: '元ファイル保存場所',
        customSave: 'デフォサイズ以外別保存',
        customDir: 'デフォサイズ以外保存場所',
        shioSave: 'あぷ系のファイル保存',
        shioDir: 'あぷ系のファイル保存場所',
        threadNo: 'スレッドNotxt保存',
        changeNo: 'スレ落ちtxt変更',
        changeNoTxt: '変更後txt',
        startType: '開始時読上',
        startText: '開始時文章',
        saveType: '保存時読上',
        saveText: '保存時文章',
        alarmType: '落前時読上',
        alarmText: '落前時文章',
        alarmTime: '落前時実行時間',
        endAlarm: '立アラーム',
        endAlarmText: '立アラーム時間',
        endAlarmTime: '立アラーム文章',
        endAlarmChk: '立アラーム済',
        portNumber: 'ポート番号',
        autoDel: 'IDレス自動削除',
        readRes: 'レス読上',
        resTag: 'レス文章特殊タグ',
        readIDRes: 'IDレス飛ばし',
        nonDialog: 'ダイアログ画面非表示',
        plugins: 'プラグイン'
      };
      
      // スライドメニューのコンテンツエリア内に読込済みのプラグインを表示
      const slideMenuContent = document.getElementById("slideMenuContent");


      const storageView = document.createElement("div");
      storageView.id = "StorageView";
      storageView.innerText = '■読込済みStorage値[表示する]';
  
      // テキストエリアを生成
      const storageList = document.createElement("textarea");
      storageList.id = "StorageValue";
      storageList.rows = 40;
      storageList.cols = 40;
      storageList.style.fontSize = 'x-small';
      storageList.style.color = '#800000';
      storageList.style.backgroundColor = '#F0E0D6';
      storageList.style.borderColor = '#800000';
      storageList.style.margin = '5px 5px 5px 5px';
      storageList.style.display = 'none';
      storageList.readOnly = true;
      // storageList.value = tegakiStorage.plugins.map(item => "・" + item).join("\n");
      const list = [];
      for (let key in tegakiStorage) {
        if (key === "plugins") {
          list.push(`${paramName[key]}: [${tegakiStorage[key].join(",")}]`);
        } else {
          list.push(`${paramName[key]}: ${tegakiStorage[key]}`);
        }
      }
      storageList.value = `手書き保存ツール ver${tegaki.version}\n`;
      storageList.value += list.map(item => "・" + item).join("\n");

      storageView.addEventListener('click', () => {
        const sv1 = document.getElementById("StorageView");
        const sv2 = document.getElementById("StorageValue");
        if (sv2.style.display === 'none') {
          sv2.style.display = 'block';
          sv1.innerText = '■読込済みStorage値[隠す]';
        } else {
          sv2.style.display = 'none';
          sv1.innerText = '■読込済みStorage値[表示する]';
        }
      });

      slideMenuContent.appendChild(storageView);
      slideMenuContent.appendChild(storageList);

      console.log("start all.execute.first", param.from);
      return result;
    }
  },
  {
    point: "status404",
    sort: "001",
    execute: function (param) {
      let result = { isStop: false, isError: false, message: "", resultValue: "" };
      console.log("status404 all.execute.first", param.from);
      return result;
    }
  },
  {
    point: "read",
    sort: "001",
    beforeExecute: function (param) {
      let result = { isStop: false, isError: false, message: "", resultValue: "" };
      // console.log("read beforeExecute all.execute.first", param.from);
      return result;
    },
    afterExecute: function (param) {
      let result = { isStop: false, isError: false, message: "", resultValue: "" };
      // console.log("read afterExecute all.execute.first", param.from);
      return result;
    }
  },
  {
    point: "save",
    sort: "001",
    beforeExecute: function (param) {
      let result = { isStop: false, isError: false, message: "", resultValue: "" };
      console.log("save beforeExecute all.execute.first", param.from);
      return result;
    },
    afterExecute: function (param) {
      let result = { isStop: false, isError: false, message: "", resultValue: "" };
      console.log("save afterExecute all.execute.first", param.from);
      return result;
    }
  }
];

