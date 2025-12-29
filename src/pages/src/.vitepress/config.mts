import { defineConfig } from 'vitepress'

// https://vitepress.dev/reference/site-config
export default defineConfig({
  base: "/Sureyomichan/",
  title: "スレ詠みちゃん",
  description: "",
  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    nav: [
      { text: "ホーム", link: "/" },
      { text: "使い方", link: "/usage/" }
    ],

    sidebar: {
      "/usage/": [
        {
          text: "使い方",
          items: [
            { text: "はじめに", link: "/usage/" },
          ],
        }
      ]
    },

    socialLinks: [
      { icon: "github", link: "https://github.com/HARUKei66494739/Sureyomichan" }
    ]
  }
})
