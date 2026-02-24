// @ts-check

/**
 * @typedef {{ point:string, execName:string, from:string, res:any|undefined }} PluginParam
 * @typedef {{ isStop:boolean, isError:boolean, message:string, resultValue:string}} PluginResult
 * 
 * @typedef BackgroundDownloadItem
 * @property {number} tab
 * @property {any} option
 * @property {any} complete
 * 
 * @typedef NgImageStorge
 * @property {number} verison
 * @property {NgImageStorgeItem[]} items
 * 
 * @typedef NgImageStorgeItem
 * @property {string} no
 * @property {number} register
 * @property {string} hash
 * @property {string} memo
 * @property {number} threshold
 * 
 * registerはUnix時間
 * hashはbigIntなので文字列で保存
 * 
 * @typedef TegakiSaveCtrl_speak
 * @property {FutabaRes[]} res
 * @property {FutabaResponse} origin
 */