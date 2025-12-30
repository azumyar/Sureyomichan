---
# https://vitepress.dev/reference/default-theme-home-page
layout: home

hero:
  name: "スレ詠みちゃん"
  actions:
      - theme: brand
        text: ダウンロード
        link: https://github.com/HARUKei66494739/Sureyomichan/releases

#features:
#  - title: Feature A
#    details: Lorem ipsum dolor sit amet, consectetur adipiscing elit
#  - title: Feature B
#    details: Lorem ipsum dolor sit amet, consectetur adipiscing elit
#  - title: Feature C
#    details: Lorem ipsum dolor sit amet, consectetur adipiscing elit
---

<script setup lang="ts">
  // いい感じの方法ある気がするけどYAMLにv-bindする方法が見当たらないので強引に書き換える
  import { onMounted } from 'vue';
  onMounted(() => {
    const account = "HARUKei66494739";
    const repository = "Sureyomichan";
    fetch(`https://api.github.com/repos/${account}/${repository}/releases`)
      .then(function (res) {
        return res.json();
      }).then(function (json) {
        for(const release of json) {
          if(!release.draft && !release.prerelease) {
            for(const asset of release.assets) {
              console.log(asset.name);
              if(asset.name.match(/^sureyomichan-v.+\.zip$/)) {
                const a = document.querySelector(".actions .action a");
                if(a != null) {
                  a.innerText = `ダウンロード(${release.tag_name})`;
                  a.href = asset.browser_download_url;
                }
                return;
              }
            }
          }
        }
     });
  });
</script>