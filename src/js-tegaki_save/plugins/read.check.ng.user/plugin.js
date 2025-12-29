// @ts-check
// read.check.ng.user
// 読み上げ時のプラグイン[NG機能 ユーザ辞書]
// Ver2.1.0

import {
  MATCH_NONE,
  MATCH_REGEX,
  MATCH_LAMBDA,
  MATCH_CONDITIONS,
  MATCH_AUTO,
  MATCH_EX_IGNORE_CASE,
  MATCH_EX_LINEFEED,
  MATCH_EX_REMOVE_HTML_TAG,
  MATCH_EX_DECODE_SPECIAL_CHARCTER,
  MATCH_REGEX_INDICES,
  MATCH_REGEX_GLOBAL,
  MATCH_REGEX_IGNORE_CASE,
  MATCH_REGEX_MULTILINE,
  MATCH_REGEX_DOT_ALL,
  MATCH_REGEX_UNICODE,
  MATCH_REGEX_UNICODE_SETS,
  MATCH_REGEX_STICY,

  option,
  result,
  run,
} from "../_common.ng/ng.js";

// プラグイン用実行関数（コメント読み上げの前処理）
export const pluginList = [
  {
    point: "read",
    sort: "020",
    beforeExecute: function (param) {
      return checkNg(param);
    }
  }
];


/**
 * プラグイン用実行関数（コメント読み上げの前処理）
 * @param {PluginParam} param 
 * @returns {PluginResult}
 */
function checkNg(param) {
  // ============================================================================================================
  // 編集箇所（以下の範囲内をユーザNG辞書として編集してください）
  // ============================================================================================================
  /*
  NGワードは下記書式で指定します(旧型式とも互換性を担保しています)
  list[list.length] = {matchType : オプション, value : 文字列　or 式};

  オプションは下記を指定できます
  通常検索                  : option(MATCH_NONE)
  正規表現検索              : option(MATCH_REGEX)
  ラムダ式(わかる人向け)    : option(MATCH_LAMBDA)
  複数条件指定(わかる人向け): option(MATCH_CONDITIONS)

  例)
  aaa bbb cccというレスに対して
  bbbが含まれる場合NGにしたい場合次のように指定します
  list[list.length] = {matchType : option(MATCH_NONE), value : "bbb"};

  bが複数含まれる場合正規表現でNGにしたい場合次のように指定します
  list[list.length] = {matchType : option(MATCH_REGEX), value : "b+"};

  ◆matchType拡張
  optionに拡張オプションを指定することで検索条件を拡張できます。
  拡張オプションは複数指定できます。
  option(検索方式指定, [拡張1, 拡張2, ...])

  拡張オプションは下記種類があります。
  MATCH_EX_IGNORE_CASE             :大文字小文字を無視します。この拡張は現時点でMATCH_NONEしか機能しません。
  MATCH_EX_LINEFEED                :通常レスの改行を除去しますが、\nで改行した文字列を対象に検索します
  MATCH_EX_REMOVE_HTML_TAG         :HTMLタグを除去します
  MATCH_EX_DECODE_SPECIAL_CHARCTER :HTML特殊タグ(&[文字コード];&gt;&lt;&amp;&quot;)を復元します
  MATCH_REGEX_INDICES              :正規表現フラグ,INDICES
  MATCH_REGEX_GLOBAL               :正規表現フラグ,GLOBAL
  MATCH_REGEX_IGNORE_CASE          :正規表現フラグ,IGNORE_CASE
  MATCH_REGEX_MULTILINE            :正規表現フラグ,MULTILINE
  MATCH_REGEX_DOT_ALL              :正規表現フラグ,DOT_ALL
  MATCH_REGEX_UNICODE              :正規表現フラグ,UNICODE
  MATCH_REGEX_UNICODE_SETS         :正規表現フラグ,UNICODE_SETS
  MATCH_REGEX_STICY                :正規表現フラグ,STICY

  例)
  行端末尾w(ようするに草)を単純にNGしたい場合は次のようにします
  list[list.length] = {matchType : option(MATCH_REGEX, [MATCH_EX_LINEFEED, MATCH_REGEX_IGNORE_CASE, MATCH_REGEX_MULTILINE]), value : "w$"};

  ◆ラムダ式とresult関数
  option(MATCH_LAMBDA)で指定するラムダ式は次のシグネチャで指定します
  (
    template,      // ラムダ式が定義されているNGテンプレート
    comment,       // コメント(改行なし)
    commentFormat, // コメント(改行あり)
    res,           // ふたばres JSONオブジェクト
    ret            // 戻り値連想配列
  ) => true or false; // 処理をした場合true

  retを処理するresultヘルパ関数を用意しています
  result(
    test, : boolean型、NG評価の結果、NGの場合trueを指定
    ret,  : 戻り値連想配列
  ) => testの値

  サンプル
  list[list.length] = {matchType : option(MATCH_LAMBDA), value : (
    template,
    comment,
    commentFormat,
    res,
    ret) => {

    const test = () => {
      // NG判定処理
      // NGだとreturn true
    };
    
    return result(test(), ret);
  }


  ◆MATCH_CONDITIONS
  valueにNG条件の配列を定義します
  配列の条件をすべて評価しNGにするか決定します
  NG条件にcomparison("and", "or")を定義することで後続条件をAND評価するかOR評価するか決定します
  comparisonが定義されていない場合ANDとして扱います

  例)
  list[list.length] = {matchType : option(MATCH_CONDITIONS) , value : [
    {matchType : option(MATCH_NONE), value: "AAA", comparison: "or"},
    {matchType : option(MATCH_NONE), value: "BBB"},
  ]};
  AAA or BBBが含まれる場合NGにします

  list[list.length] = {matchType : option(MATCH_CONDITIONS) , value : [
    {matchType : option(MATCH_NONE), value: "ABC", comparison: "or"},
    {matchType : option(MATCH_CONDITIONS) , value : [
      {matchType : option(MATCH_NONE), value: "AAA", comparison: "or"},
      {matchType : option(MATCH_NONE), value: "BBB"},
    ]}
  ]}; 
  MATCH_CONDITIONSを入れ子にすることもできます
  ABC or (AAA or BBB)

  */

  // ユーザ登録のNGワードの配列
  /** @type {import("../_common.ng/ng.js").NgTemplate[]} */
  const list = new Array();
  list[list.length] = {matchType : option(MATCH_NONE), value : "におスレ"};    // 本文内に含む場合(サンプルとして追加してあるワード)
  list[list.length] = {matchType : option(MATCH_REGEX), value : "^(?=.*シャンカー)(?=.*チェンソーマン).*"}; 
  list[list.length] = {matchType : option(MATCH_LAMBDA), value : (_, __, ___, /** @type {FutabaRes}*/res, /** @type {PluginResult}*/ret) => {
    const test = () => {
      if(/ｷﾀ━+(ﾟ∀ﾟ)━+\s!+/.test(res.com) && !res.fsize) {
        return true;
      } else {
        return false;
      }
    }
    return result(test(), ret);
  }};
  list[list.length] = {matchType : option(MATCH_LAMBDA), value : (_, c, __, /** @type {FutabaRes}*/res, /** @type {PluginResult}*/ret) => {
    const test = () => {
      if(/^<a\s.*https?:\/\/img.2chan.net\/b\/res\/[0-9]+.htm<\/a>$/.test(c)) {
        return true;
      }
      if(/"^https?:\/\/img.2chan.net\/b\/res\/[0-9]+.htm$"}; /.test(res.email)) {
        return true;
      }
      return false;
    }
    return result(test(), ret);

  }};
  list[list.length] = {matchType : option(MATCH_LAMBDA), value : (_, __, ___, /** @type {FutabaRes}*/res, /** @type {PluginResult}*/ret) => {
    const test = () => {
      if(/ｷﾀ━+\(ﾟ∀ﾟ\)━+\s!+/.test(res.com) && !res.fsize) {
        return true;
      } else {
        return false;
      }
    }
    return result(test(), ret);
  }};
  list[list.length] = {matchType : option(MATCH_NONE) , value : ""}; 
  list[list.length] = {matchType : option(MATCH_NONE) , value : ""}; 

  /** @type {string[]} */
  const resComment = new Array();
  resComment[resComment.length] = "";  // NGにヒットした時に読み上げたいワード
  resComment[resComment.length] = "";  // NGにヒットした時に読み上げたいワード
  resComment[resComment.length] = "";  // NGにヒットした時に読み上げたいワード
  resComment[resComment.length] = "";  // NGにヒットした時に読み上げたいワード
  resComment[resComment.length] = "";  // NGにヒットした時に読み上げたいワード

  // ============================================================================================================

  return run(param, list, resComment);
}
