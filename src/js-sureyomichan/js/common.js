
function getOpendDialogParam(opend) {
    if(opend) {
        return {
            arrow: ">",
            styleRight: "0px",
        };
    } else {
        return {
            arrow: "<",
            styleRight: "-302px",
        };
    }
}


export function createSureyomichanControl(tegakiStorage, getUrl) {
    const tegakiDiv = document.createElement('div');

    // ボタン部の要素を作成
    const startButton = document.createElement('div');
    startButton.id = 'buttonContent';
    startButton.className = 'tedm';
    startButton.style.position = 'fixed';
    startButton.style.bottom = '20px';
    startButton.style.right = getOpendDialogParam(tegakiStorage.openTegakiContentForStart).styleRight;
    startButton.style.left = 'auto';  // leftをautoにする
    startButton.style.width = '302px';
    startButton.style.height = '80px';
    startButton.style.padding = '0px';
    startButton.style.border = 'none';
    startButton.style.display = 'block';;
    startButton.style.zIndex = '10';   // 開始ボタンエリアはzIndex:10
    startButton.style.transition = 'right 0.3s ease-in-out, width 0.3s ease-in-out';

    // fieldset要素を生成
    const fieldset = document.createElement('fieldset');
    fieldset.className = 'tefieldset';
    // legend要素を生成
    const legend = document.createElement('legend');
    legend.textContent = `スレ詠みちゃん ver.${chrome.runtime.getManifest().version}`;
    fieldset.appendChild(legend);


    // ボタンを追加
    const button1 = document.createElement('button');
    button1.id = 'tegakiButton';
    button1.className = 'tebtn';
    button1.textContent = '開始';
    button1.title = '手書き保存を開始します';
    const button2 = document.createElement('button');
    button2.id = 'tegakiMiddleButton';
    button2.className = 'tebtn';
    button2.textContent = '最終レスから開始';
    button2.title = '読み上げは表示中の最終レス以降から・手書きはすべて保存';
    // ボタンクリック時の動きを追加
    button1.addEventListener('click', () => {
        const url = getUrl();
        if(url) {
            location.href = url;
        }
    });
    button2.addEventListener('click', () => {
        const url = getUrl();
        if(url) {
            location.href = `${url}?latest`;
        }
    });

    // ボタン追加
    fieldset.appendChild(button1);
    fieldset.appendChild(button2);

        // 開始ボタンエリアにボタンを含んだ枠を追加
    startButton.appendChild(fieldset);

    // スライドボタンを追加して、ボタンエリアを表示/非表示できるようにする
    const sliderButton = document.createElement('div');
    sliderButton.className = 'tedt';
    sliderButton.innerText = getOpendDialogParam(tegakiStorage.openTegakiContentForStart).arrow;
    sliderButton.title = '手書き保存ツールボタンエリアを表示/非表示';
    sliderButton.style.position = 'absolute';
    sliderButton.style.left = '-20px';
    sliderButton.style.top = '0px';
    sliderButton.style.width = '20px';
    sliderButton.style.height = '100%';
    sliderButton.style.backgroundColor = '#ea8';
    sliderButton.style.cursor = 'pointer';
    sliderButton.style.display = 'flex';
    sliderButton.style.justifyContent = 'center';
    sliderButton.style.alignItems = 'center';
    sliderButton.style.borderTopLeftRadius = '5px';
    sliderButton.style.borderBottomLeftRadius = '5px';

    // ボタンエリアに追加する
    startButton.appendChild(sliderButton);

    // つまみ要素にクリックイベントを追加
    sliderButton.addEventListener('click', () => {
        const p = getOpendDialogParam(sliderButton.innerText === "<");
        sliderButton.innerText = p.arrow;
        startButton.style.right = p.styleRight;
    });

    // 開始ボタンエリアをbodyに追加
    tegakiDiv.appendChild(startButton);
    return tegakiDiv;
}