const tegakiStorage = {
    openTegakiContentForStart: true,
};

function getSureyomichanUrl() {
    const r = location.href.match(new RegExp("https://nijiurachan.net/pc/thread.php\\?id=([0-9]+)"));
    console.log(location.href);
    if(r) {
        return `sureyomichan://open/aimg/${r[1]}`;
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