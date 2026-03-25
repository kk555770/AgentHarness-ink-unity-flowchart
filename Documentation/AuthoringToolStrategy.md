# 作者工具策略

> 文件負責人：authoring
> 最後更新：2026/03/17  
> 目的：把本專案的作者工具策略寫清楚，避免把 current GraphToolkit 工作流誤認成長期唯一方向，也避免後續 Web-first 重製缺少共同語言。

## 0. 先講結論

本專案的作者工具策略，現在應這樣理解：

- **canonical graph** 才是唯一真相
- 作者工具只是操作真相的外殼
- `GraphToolkit` 是**目前仍可工作的作者工具外殼與回歸基準**
- 長期方向傾向 **Web-first control surface**

如果用很白話的方式講：

- 真正的故事規則像積木
- GraphToolkit 像目前借來畫圖的白板
- 未來 Web 作者工具像自己做的白板

白板很重要，但白板不是積木本身。

## 1. 這份文件在回答什麼

這份文件主要回答：

- 為什麼作者工具不應長期綁死在 GraphToolkit
- 為什麼 Web-first 比較符合 AI 接入方向
- WebView / Browser / Electron 各自應放在什麼位置
- 目前 GraphToolkit 要怎麼定位，才不會和 canonical truth 搶角色

如果你要看「目前這條 GraphToolkit / sidecar / ink round-trip 實際怎麼跑」，
請直接讀：

- `Documentation/CurrentAuthoringWorkflow.md`

如果你要看「之後重製應先切哪一刀」，
請直接讀：

- `Documentation/AuthoringRefactorBoundaries.md`

如果你要看「第一階段實際該先改哪些檔、怎麼驗證」，
請直接讀：

- `Documentation/AuthoringPhase1ImplementationPlan.md`

這份文件**不**負責定義：

- canonical graph 長什麼樣
- JSON contract 長什麼樣
- Runtime 輸出契約長什麼樣
- current GraphToolkit workflow 的逐步操作說明

這些請看：

- `Documentation/CanonicalGraphSchema.md`
- `Documentation/CanonicalGraphApiSpec.md`
- `Documentation/CanonicalGraphJsonContract.md`
- `Documentation/DeveloperModeOutputContract.md`

## 2. 為什麼要補這份文件

如果沒有把作者工具策略先寫清楚，後面很容易出現 3 種誤讀：

1. 因為 GraphToolkit 現在能用，就誤以為它是長期核心平台
2. 因為想做 WebView，就誤以為 canonical truth 要搬去前端
3. 因為 AI 擅長操作 web UI，就誤以為前端互動可以直接取代語意控制面

這三種都不對。

正確順序是：

```text
Canonical Graph Core
  -> Command / Validation / Projection API
  -> Authoring Surface
```

作者工具的前端可以換，
但真相層與控制面不能跟著飄。

## 3. GraphToolkit 的現況定位

目前 GraphToolkit 在這個 repo 裡有真實價值：

- 它已經承載 current Flow Chart 工作流
- `.inkfc <-> .flowchart.json + .ink` 已經有 round-trip 與測試護欄
- 它是目前最成熟的作者工具回歸基準

但它不應被升格成：

- canonical truth 本體
- 長期唯一作者平台
- AI 直接操作的最終控制面

更準確的定位是：

- **current tooling baseline**
- **legacy / migration-friendly authoring shell**
- **驗證目前 projection contract 的工作外殼**

如果要看這個 baseline 在 repo 裡實際怎麼流動，請讀：

- `Documentation/CurrentAuthoringWorkflow.md`

如果要看這個 baseline 之後應怎麼拆，請讀：

- `Documentation/AuthoringRefactorBoundaries.md`

## 4. 為什麼策略會往 Web-first 收斂

### 4.1 AI 比較擅長操作 web control surface

這不是因為「web 比較潮」。

而是因為：

- DOM / 可觀察狀態 / 明確事件，比較適合 AI 與自動化
- Playwright、瀏覽器自動化、互動式 UI 測試在 web 世界更成熟
- Web 前端比較容易做搜尋、批次編輯、diff、面板、AI 輔助區塊

### 4.2 不想把未來綁死在 Unity experimental tooling

目前 GraphToolkit 是 experimental 套件。

這代表：

- API 可能變
- 支援承諾相對弱
- 不適合直接當成 3 到 5 年的唯一長期平台假設

所以比較健康的做法是：

- 把 GraphToolkit 視為 current working tooling
- 把真正長期穩定的核心放在 canonical graph 與 command API

### 4.3 作者工具不應只存在於 Unity Editor 裡

如果作者工具長期要接 AI，
也可能要接：

- Browser
- Unity 內嵌 WebView
- Electron
- 其他本地或遠端前端

那就不應把工具本體設計成只能存在於 Unity Editor 自家元件裡。

## 5. WebView、Browser、Electron 各自是什麼

這三個名詞很容易混在一起，所以先切開：

### Web 前端

最核心的是「web app 本身」。

也就是：

- 節點畫布
- 面板
- 表單
- 驗證訊息
- AI 輔助互動

這一層才是作者工具的前端本體。

### WebView

WebView 是：

- 把 web 前端嵌進某個宿主裡的方式

例如：

- Unity Editor 視窗裡嵌一個 WebView

所以 WebView 不是策略本體，
它只是前端載入方式之一。

### Browser

Browser 是：

- 直接讓作者工具跑在外部瀏覽器裡

這通常最簡單，也最容易做 AI / Playwright 自動化。

### Electron

Electron 是：

- 把 web app 包成獨立桌面殼

它的價值是桌面封裝，
不是作者工具語意本體。

所以 Electron 不該是第一個要先決定的問題。

## 6. 策略上的責任切分

未來比較健康的切法應該是：

```text
Canonical Graph Core
  -> Command / Validation / Projection API
  -> Web Authoring Frontend
  -> Unity Runtime Adapter
```

這代表：

- 真相在 canonical graph
- 規則在 core / validator / projection service
- current GraphToolkit workflow 保留為 migration / regression baseline
- Web 前端負責人類互動與 AI-friendly control surface
- Unity Runtime 負責播放與驗證閉環

## 7. 這不代表什麼

這份策略文件**不代表**：

- 已經決定立刻丟掉 GraphToolkit
- 已經決定第一版一定要用 Electron
- 已經決定前端框架
- 已經決定所有資料都搬去前端

目前比較合理的理解是：

- 先把文件與 core 語言統一
- 再把 control plane 抽乾淨
- 然後再做 Web-first 作者工具原型

## 8. 第一階段建議

如果後面真的要實作，第一階段最合理的是：

1. 先穩定 canonical graph core 與 command API
2. 先做最小 Web editor
3. 先保留 GraphToolkit 當對照組與回歸基準
4. 等最小閉環穩了，再評估是否抽成 Unity WebView 或 Electron

這樣做的好處是：

- 不會一開始就把桌面封裝成本扛上身
- 不會把換前端誤當成換真相層
- 可以保留現在已經存在的 round-trip 驗證價值

如果要看更具體的工程清單，請接著讀：

- `Documentation/AuthoringPhase1ImplementationPlan.md`

## 9. 一句總結

本專案的作者工具策略，不是「把 GraphToolkit 全部推倒再換一個新 UI」。

而是：

> **把 canonical graph 與 control plane 立穩，然後讓作者工具前端長期往 Web-first 收斂；GraphToolkit 保留為 current working baseline，而不是未來唯一平台。**
