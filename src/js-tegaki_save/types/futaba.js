// @ts-check

/**
 * @typedef FutabaResponse
 * @property {string} die
 * @property {string} dielong
 * @property {number} dispname
 * @property {number} dispsod
 * @property {string} maxres
 * @property {number} nowtime
 * @property {number} old
 * @property {Object.<string, FutabaRes>[] | undefined} res
 * @property {Array | Object.<string, number>}  sd
 * 
 * @typedef FutabaRes
 * @property {string} com
 * @property {string} del
 * @property {string} email
 * @property {string} ext
 * @property {number} fsize
 * @property {number} h
 * @property {string} host
 * @property {string} id
 * @property {string} name
 * @property {string} now
 * @property {number} rsc
 * @property {string} src
 * @property {string} sub
 * @property {string} thumb
 * @property {string} tim
 * @property {number} w
 * @property {string | undefined} resNo tegaki_save独自、レスNo
 * @property {boolean | undefined} __tageki_ng tegaki_save独自、NGの場合true
 * @property {FutabaRes[] | undefined} __tegaki_res tegaki_save独自、プラグインに渡すときレス配列を含める
 */