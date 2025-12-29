// @ts-check
/**
 * @typedef {Object} NgTemplate
 * @property {number|string} matchType
 * @property {string|RegExp|TestFunction|NgTemplate[]|null} value
 * @property {"and"|"or"} [comparison]
 * 
 * @callback TestFunction
 * @param {NgTemplate} template
 * @param {string} comment
 * @param {string} commentFormat
 * @param {FutabaRes} res
 * @param {PluginResult} result
 * @returns {boolean}
 */

/** 検索タイプ-通常*/ export const MATCH_NONE = 0;
/** 検索タイプ-正規表現*/ export const MATCH_REGEX = 1;
/** 検索タイプ-ラムダ式*/ export const MATCH_LAMBDA = 2;
/** 検索タイプ-複数条件*/ export const MATCH_CONDITIONS  = 3;
/** 検索タイプ-自動認識*/ export const MATCH_AUTO = 0xff;

/** 拡張オプション-大文字小文字を無視*/ export const MATCH_EX_IGNORE_CASE = 0x100;
/** 拡張オプション-改行を付加する*/ export const MATCH_EX_LINEFEED = 0x200;
/** 拡張オプション-HTMLタグを取り除く*/ export const MATCH_EX_REMOVE_HTML_TAG  = 0x800; // 初出時間違えてたので順番が違う
/** 拡張オプション-HTML特殊文字を復元する*/ export const MATCH_EX_DECODE_SPECIAL_CHARCTER  = 0x400;

/** 拡張オプション-正規表現オプション-indeces*/ export const MATCH_REGEX_INDICES = 0x010000;
/** 拡張オプション-正規表現オプション-global*/ export const MATCH_REGEX_GLOBAL = 0x020000;
/** 拡張オプション-正規表現オプション-igoore case*/ export const MATCH_REGEX_IGNORE_CASE = 0x100000; // 初出時間違えてたので順番が違う
/** 拡張オプション-正規表現オプション-muliti line*/ export const MATCH_REGEX_MULTILINE = 0x40000;
/** 拡張オプション-正規表現オプション-dot ALL*/ export const MATCH_REGEX_DOT_ALL= 0x200000; // 初出時間違えてたので順番が違う
/** 拡張オプション-正規表現オプション-unicode*/ export const MATCH_REGEX_UNICODE = 0x400000; // 初出時間違えてたので順番が違う
/** 拡張オプション-正規表現オプション-unicode sets*/ export const MATCH_REGEX_UNICODE_SETS = 0x800000; // 初出時間違えてたので順番が違う
/** 拡張オプション-正規表現オプション-sticy*/ export const MATCH_REGEX_STICY = 0x80000;


/**
 * matchTypeから正規表現フラグ文字列に対応する変換テーブル
 */
const regexOptTable = [
    { option: MATCH_REGEX_INDICES, flag: "d" },
    { option: MATCH_REGEX_GLOBAL, flag: "g" },
    { option: MATCH_REGEX_IGNORE_CASE, flag: "i" },
    { option: MATCH_REGEX_MULTILINE, flag: "m" },
    { option: MATCH_REGEX_DOT_ALL, flag: "s" },
    { option: MATCH_REGEX_UNICODE, flag: "u" },
    { option: MATCH_REGEX_UNICODE_SETS, flag: "v" },
    { option: MATCH_REGEX_STICY, flag: "y" },
  ];
  
/**
 * matchTypeを生成します
 * @param {number} type 
 * @param {number[] | undefined} [ex] 
 * @return {number}
 */
export function option(type, ex) {
  let r = type;
  if(ex) {
    for(const v of ex) {
      r |= v;
    }
  }
  return r;
}

/**
 * NG処理本体
 * @param {PluginParam} param 
 * @param {NgTemplate[]} ng
 * @param {string[]} [resComment]
 * @returns {{isStop:boolean, isError:boolean, message:string, resultValue:string}}
 */
export function run(param, ng, resComment) {
    /**
   * コメントから改行タグを処理
   * @param {string} com
   * @returns {string}
   */
  function parseComment(com) {
    return com.replace(/<br>|\r\n|\r|\n/g, "");
  }

  // 処理結果返り値
  // isStop：処理の中断判定…返した側で処理を中断すべきかどうかの判定値（true：処理を中断させる、false：処理続行）
  // isError：エラーの有無 （true：エラー発生、エラーの種類によっては処理を続行しない 返した側で判断する）
  // message：中断やエラーのメッセージ
  // resultValue：呼び出し元に返したい値
  /** @type {PluginResult} */
  const ret = {isStop : false, isError : false, message : "", resultValue : ""};
  let prevValue = ret.resultValue;
  let comment = parseComment(param.res.com);
  if(comment === "") {
    // 空の場合、処理を行わない
    return ret;
  }
  for(const it of ng.map((x, i) => {return {template: x, index: i};}).filter(x => x.template.value && x.template.value !== "")) {
    const test = parseOption(it.template);
    let commentFormat = prevValue.length ? prevValue : param.res.com;
    const opt = parseIntSafe(it.template.matchType);
    if((opt & MATCH_EX_LINEFEED) == MATCH_EX_LINEFEED) {
      commentFormat = commentFormat.replace(/<br>|\r\n|\r/g, "\n");
    }
    if((opt & MATCH_EX_REMOVE_HTML_TAG) == MATCH_EX_REMOVE_HTML_TAG) {
      commentFormat = commentFormat.replace(/<[^>]*>/g, "");
    }
    if((opt & MATCH_EX_DECODE_SPECIAL_CHARCTER) == MATCH_EX_DECODE_SPECIAL_CHARCTER) {
      commentFormat = commentFormat.replace(
        /&([^;]+);/g,
        (_, p1) => {

        if(p1[0] == '#') {
          return String.fromCodePoint(parseInt(`0${p1.substring(1)}`));
        }

        switch(p1.toLowerCase()) {
          case "gt": return ">";
          case "lt": return "<";
          case "amp": return "&";
          case "quot": return "\"";
        }

        return "";
      });
    }
    if(test && test(it.template, comment, commentFormat, param.res, ret)) {
      console.log(`NG機能 共通NG辞書 beforeExecute read.check.ng.public ${it.index + 1} 番目のNG設定にヒット 対象コメント: ${comment}`);
      if(ret.isStop) {
          break;
      }
      if(ret.resultValue !== prevValue) {
          prevValue = ret.resultValue;
          comment = parseComment(ret.resultValue);
      }
    }
  }

  // ヒットしている場合
  if(ret.isStop && resComment) {
    const speakText = resComment.filter((x) => x && x.length);
    if(speakText.length){
      bgSendMessage({case: "speak", text: speakText, port: tegakiStorage.portNumber});
    }
  }
  return ret;
}

/**
 * templateからTestFunctionを返します
 * @param {NgTemplate} template
 * @return {TestFunction|null}
 */
function parseOption(template) {
  if((template.value === "") || (!template.value)) {
    return null;
  }

  const opt = parseIntSafe(template.matchType);
  if((opt & 0xff) === 0) {
    return test;
  } else if((opt & 0xff) === MATCH_REGEX) {
    return testRegex;
  } else if((opt & 0xff) === MATCH_LAMBDA) {
    if(template.value.constructor.name === "Function") {
      return /** @type {TestFunction} */ (template.value);
    }
  } else if((opt & 0xff) === MATCH_CONDITIONS) {
    if(Array.isArray(template.value)) {
      return testMultiConditions;
    }
  } else if((opt & 0xff) == MATCH_AUTO) {
    if(template.value.constructor.name === "String") {
      return test;
    } else if(template.value.constructor.name === "RegExp") {
      return testRegex;
    } else if(template.value.constructor.name === "Function") {
      return /** @type {TestFunction} */ (template.value);
    } else if(Array.isArray(template.value)) {
      return testMultiConditions;
    }
  }
  return null;
}

/**
 * よく使うtest()の戻り値パターン
 * @param {boolean} test
 * @param {PluginResult} ret
 * @returns {boolean}
 */
export function result(test, ret) {
  if(test) {
    ret.isStop = true;
    return true;
  } else {
    return false;
  }
}

/**
 * 通常検索のtest関数
 * @param {NgTemplate} template
 * @param {string} comment
 * @param {string} commentFormat,
 * @param {FutabaRes} _
 * @param {PluginResult} ret
 * @returns {boolean}
 */
function test(template, comment, commentFormat, _, ret) {
  const opt = parseIntSafe(template.matchType);
  const c = ((opt & MATCH_EX_LINEFEED) === MATCH_EX_LINEFEED) ? commentFormat : comment;
  const test = () => {
    if(typeof template.value !== "string") {
      throw TypeError("invalid tyoe template.value");
    }
    if((opt & MATCH_EX_IGNORE_CASE) === MATCH_EX_IGNORE_CASE) {
      return 0 <= c.toLowerCase().indexOf(template.value.toLowerCase());
    }
    return 0 <= c.indexOf(template.value);
  };

  return result(test(), ret);
}

/**
 * 正規表現検索のtest関数
 * @param {NgTemplate} template
 * @param {string} comment
 * @param {string} commentFormat
 * @param {FutabaRes} _
 * @param {PluginResult} ret
 * @returns {boolean}
 */
function testRegex(template, comment, commentFormat, _, ret) {
  const opt = parseIntSafe(template.matchType);
  const c = ((opt & MATCH_EX_LINEFEED) === MATCH_EX_LINEFEED) ? commentFormat : comment;
  const rxp = () => {
    if(template.value && template.value.constructor.name === "RegExp") {
      return /** @type {RegExp} */ (template.value);
    }
    if(typeof template.value === "string") {
      if((opt & MATCH_EX_IGNORE_CASE) === MATCH_EX_IGNORE_CASE) {
        // 使わない
      }
      return RegExp(
        template.value,
        regexOptTable
          .map((x) => (opt & x.option) === x.option ? x.flag : "")
          .join("")
        );
    }
    throw TypeError("invalid type template.value");
  };

  return result(rxp().test(c), ret);
}

/**
 * -
 * @param {NgTemplate} template
 * @param {string} comment
 * @param {string} commentFormat
 * @param {FutabaRes} res
 * @param {PluginResult} ret
 * @returns {boolean}
 */
function testMultiConditions(template, comment, commentFormat, res, ret) {
  if(!(Array.isArray(template.value) && template.value.length)) {
    throw TypeError("invalid type template.value");
  }

  /**
   * -
   * @param {NgTemplate} template_
   * @returns {TestFunction}
   */
  function parse(template_) {
    const f = parseOption(template_);
    if(!f) {
      throw TypeError("invalid option template.matchType");
    }
    return f;
  }

  let prv = template.value[0];
  let ng = parse(template.value[0])(template.value[0], comment, commentFormat, res, ret);
  for(const it of template.value.slice(1)) {
    const cur = parse(it)(it, comment, commentFormat, res, ret);
    switch(prv.comparison) {
    case "and":
      ng = ng && cur;
      break;
    case "or":
      ng = ng || cur;
      break;
    default:
      ng = ng && cur;
      break;
    }
    prv = it
  }
  return result(ng, ret);
}

 /**
 * parseIntをタイプセーフにする
 * @param {string | number} v
 * @returns {number}
 */
 function parseIntSafe(v) {
   if(typeof  v === "number") {
     return v;
   } else {
     return Number.parseInt(v);
   }
}