const tegakiStorage = {
    openTegakiContentForStart: true,
};

function getSureyomichanUrl() {
    const r = location.href.match(new RegExp("https?://img.2chan.net/b/res/([0-9]+).htm"));
    if(r) {
        return `sureyomichan://open/img/${r[1]}`;
    } else {
        return null;
    }
}


(async() => {
    const src = chrome.runtime.getURL("js/common.js");
    const m = await import(src);

    const tegakiDiv = m.createSureyomichanControl(tegakiStorage, getSureyomichanUrl);

    const firstChild = document.body.firstChild;
    document.body.insertBefore(tegakiDiv, firstChild);
})()
