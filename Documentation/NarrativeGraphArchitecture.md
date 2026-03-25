# 敘事圖架構總覽

> 最後更新：2026/03/17  
> 文件角色：**架構北極星 / Why 層**  
> 目的：把本專案真正的架構北極星寫清楚，避免把過渡期的驗證手段誤認成最終真相來源，並補上目前作者工具策略的方向。

## 0. 先講結論

這個專案不是單純的 Unity VN 工具，也不是圍繞 GraphToolkit 的編輯器專案。

它真正要做的是：

- 以 `Graph JSON / Schema / API` 作為唯一語意真相
- 讓 AI 與人類都能操作同一份敘事圖
- 讓作者工具只是「完整表現真相」的人類可視化外殼，而不是新的真相來源
- 讓 Unity 成為其中一個播放器、驗證器、自動化測試環境

目前策略補充：

- `GraphToolkit` 是**目前仍可工作的作者工具外殼與回歸基準**
- 長期作者工具策略傾向 `Web-first` control surface，讓 AI、自動化與前端演進不綁死在 Unity Editor experimental tooling 上

一句話版本：

> **真相在 Schema，不在 GraphToolkit，不在 Unity，也不在某一份匯出檔。**

## 1. 為什麼要先把這件事寫死

如果沒有先把架構方向講清楚，專案很容易被誤讀成下面幾種樣子：

- 「這是 GraphToolkit-first 的工具」
- 「這是 Unity Runtime-first 的 VN 專案」
- 「`.inkfc` / `.flowchart.json` / `.ink` / `story.json` 都各自像真相」

這些觀察在過渡期看起來都像真的，但都不是最終設計意圖。

本專案的真正目標，是讓敘事圖語意可以：

- 被 AI 用 API 操作
- 被人類用圖形介面編輯
- 被不同 runtime 讀取與執行
- 被自動化測試驗證

如果用 8 歲小孩也懂的比喻：

- `Schema` 像劇本的真正內容
- `GraphToolkit` 像把劇本畫成流程圖的白板
- `Unity` 像把劇本演出來的舞台

白板和舞台都很重要，但它們都不是劇本本身。

## 2. 四層架構

本專案應以四層來理解：

```text
Canonical Schema
  -> Projection
  -> Runtime Adapter
  -> Validation Loop
```

### 2.1 Canonical Schema

`Graph JSON / Schema / API` 是唯一語意真相。

這一層負責定義：

- 有哪些節點型別
- 每種節點允許哪些資料
- 節點之間如何連線
- 哪些規則是合法，哪些是不合法
- 哪些語意必須能被 AI 與人類共同理解

這一層的要求：

- 不能依賴某個編輯器才成立
- 不能依賴某個 runtime 才成立
- 不能把關鍵語意藏在 UI 狀態裡
- 不能把關鍵語意藏在自由文字裡

更具體的責任與最小骨架，請見 `Documentation/CanonicalGraphSchema.md`。

### 2.2 Projection

Projection 是把同一份真相投影到不同工作表面上。

目前最重要的投影有：

- GraphToolkit 視覺圖
- Ink 文字稿
- `story.json` 編譯產物

這一層的核心原則是：

> **Projection 可以改變呈現形式，但不能改變語意。**

其中 `GraphToolkit` 的角色尤其要講清楚：

- 它不是架構中心
- 它不是唯一入口
- 但它必須能完整表現真相

也就是說，GraphToolkit 不是「簡化外殼」，而是 **無資訊遺失的視覺外殼**。

補充一個目前很容易混讀的點：

- 像 `character / castBundle` 這類節點，若目前只存在於 Graph v2 文件、sidecar 設計或作者體驗描述中，較合理的定位應是 projection / authoring data-source layer
- 它們可以很重要，但在真正有 stable schema、匯入匯出邏輯與 round-trip tests 前，不應自動被視為 canonical 最小核心節點

### 2.3 Runtime Adapter

Runtime Adapter 是把同一份敘事圖語意接到某個執行環境上。

目前 repo 裡最明確的 adapter 是 Unity Runtime。

它負責：

- 顯示文字與選項
- 接背景、角色、CG、音效等演出
- 處理存檔、讀檔、倒帶
- 驗證輸出在播放、Restore、Rollback 時是否一致

但最重要的限制是：

> **最終視覺小說執行語意不能依賴 Unity 才成立。**

換句話說：

- Unity 可以是很重要的 adapter
- 但 Unity 不能是唯一的語意宿主

### 2.4 Validation Loop

Validation Loop 是本專案非常重要的一層。

因為本專案不是只做抽象 schema，也不是只做編輯器畫面，而是要保證：

- Schema 沒走樣
- Projection 沒失真
- Runtime 沒誤解
- 回歸測試能抓到偏移

這也是為什麼開發過程會不斷出現：

- 做一段 schema
- 回頭接 GraphToolkit 顯示
- 回頭接 Unity 播放
- 補測試驗證 round-trip、Restore、Rollback

這不是偏離架構，而是架構的一部分。

## 3. 真相與投影的責任邊界

### 3.1 哪些東西是「真相」

- 敘事圖節點語意
- 節點資料結構
- 連線規則
- 驗證規則
- 可被 AI / 人類共同操作的 API

### 3.2 哪些東西不是「真相」

- GraphToolkit 視窗目前怎麼畫
- Unity Inspector 目前怎麼掛元件
- 某個 runtime 目前怎麼呈現角色
- 某個匯出檔現在剛好長什麼樣子

### 3.3 哪些東西是「工作投影」

- `.inkfc`
- `.flowchart.json`（目前 Graph v2 的 sidecar / interchange / round-trip 投影格式）
- `.ink`
- `story.json`
- Unity Runtime 畫面
- 自動化測試輸入與輸出
- `character / castBundle` 這類目前主要服務作者資料搬運的節點設計

這些都很重要，但它們的角色是：

- 編輯
- 交換
- 編譯
- 播放
- 驗證

不是取代 canonical truth。

## 4. 為什麼現在 repo 看起來像「到處都有真相」

這不是因為架構方向錯了，而是因為專案正處在：

> **Schema-first 設計 + 分段式可視化驗證**

的開發狀態。

每做完一段，都需要回頭做幾件事：

- 讓人可以看見
- 讓畫面可以驗
- 讓播放可以驗
- 讓測試可以驗

所以 repo 裡自然會同時有：

- 圖資產
- sidecar
- Ink
- 編譯產物
- Unity 播放鏈
- 測試規格與測試程式

它們看起來都像「很重要的主體」，但更準確的說法是：

- 真相來源只有一個方向：Canonical Schema
- 其他都是用來工作、檢查、驗證、回歸的投影

## 5. 對 AI 控制的直接意義

本專案的長期目標之一，是讓 AI 可以輕易控制敘事圖，像操作 API 與選擇樹一樣進行建立、理解與接線。

這代表 AI 的正確控制面，不應該是：

- 模擬拖滑鼠
- 操作 Unity 視窗手勢
- 記住某個 Editor 的細碎互動流程

AI 應該操作的是：

- 建立節點
- 選擇節點型別
- 設定節點資料
- 決定分支與接線
- 呼叫驗證
- 產生 projection

如果用簡化格式表示：

```text
CreateNode(type, payload)
Connect(fromNodeId, outPort, toNodeId, inPort)
UpdateNode(nodeId, payload)
ValidateGraph()
ExportProjection(target)
```

正式的操作面規格，請見 `Documentation/CanonicalGraphApiSpec.md`。

若要看第一個正式的 JSON 控制格式，請見：

- `Documentation/CanonicalGraphJsonContract.md`

這也是為什麼本專案必須以 schema 為核心，而不能以 GraphToolkit 視窗為核心。

因為 AI 最穩定的操作對象是：

- 結構
- 型別
- 規則
- API

不是滑鼠手勢與 Editor 畫面細節。

## 6. GraphToolkit 的真正角色

GraphToolkit 在本專案裡非常重要，但它的重要性不應被誤解。

它的真正價值是：

- 讓人類更容易看懂流程
- 讓人類更容易編輯敘事圖
- 讓人類可以直觀看見 schema 投影後是否合理

它不是：

- 唯一真相來源
- 最終語意定義者
- 專案的架構中心

但它也不能只是隨便畫一下的 UI。

它必須遵守：

- 能完整表現 schema
- 不能偷藏只有 Editor 才知道的語意
- 不能遺失 AI 建圖時已存在的資訊
- 不能讓 round-trip 後語意變形

## 7. Unity 的真正角色

Unity 在目前 repo 裡很重，但它的正確定位是：

- 一個重要的 runtime adapter
- 一個重要的視覺驗證環境
- 一個重要的自動化測試宿主

Unity 不是：

- 唯一執行語意來源
- 敘事圖定義本身

因此後續設計應始終避免：

- 只有 Unity component graph 才看得懂的語意
- 只有 Unity 播放器才知道的節點規則
- 只有 Unity 內部狀態才成立的故事真相

## 7.5 控制面 Transport Adapter 的正確位置

除了內容本身的 Projection 與 Runtime Adapter 之外，控制面還有一條容易被混淆的線：

```text
Canonical Schema
  -> Canonical Graph API
    -> Plain JSON Contract
      -> 可選的 JSON-RPC / HTTP / CLI Adapter
```

這條線的重點是：

- `Canonical Graph API`
  - 決定「可以做哪些事」
- `Plain JSON Contract`
  - 決定「第一個正式 JSON 指令長什麼樣」
- `JSON-RPC / HTTP / CLI Adapter`
  - 決定「外部系統怎麼呼叫這些指令」

因此：

- JSON 不是自動就等於 projection
- JSON 也不是自動就等於 canonical truth
- JSON-RPC 若未來加入，應是 transport adapter，不應升格成真相來源

## 8. 對目前 repo 的實務解讀

以目前 repo 狀態來說，更準確的說法是：

- 架構北極星是 `Schema-first`
- 實作上仍存在一些為了視覺檢驗、播放驗證、回歸測試而形成的投影耦合
- 這些耦合不必然代表方向錯誤，但需要持續收斂

因此判斷某個設計是否正確時，應先問：

1. 這個資訊是不是屬於 canonical schema？
2. 如果是，GraphToolkit 是否能完整表現？
3. 如果是，Unity runtime 是否只是 adapter，而不是額外新增語意？
4. round-trip 與測試是否能驗證這件事沒有走樣？

## 9. 本文件與其他文件的關係

本文件是架構總覽。

它負責回答：

- 真相在哪裡
- 各層扮演什麼角色
- 為什麼現在 repo 看起來像多重真相
- 為什麼作者工具策略不應長期綁死在 GraphToolkit

它不取代下列文件：

- `Documentation/DocsIndex.md`
  - 負責給新讀者一張文件地圖
- `Documentation/AuthoringToolStrategy.md`
  - 負責把目前作者工具策略、Web-first 方向與 GraphToolkit 過渡定位寫清楚
- `Documentation/CurrentAuthoringWorkflow.md`
  - 負責把目前 GraphToolkit / sidecar / ink round-trip 工作流集中說清楚

- `Documentation/CanonicalGraphSchema.md`
  - 負責定義 canonical truth 應包含哪些圖語意與 API 邊界
- `Documentation/CanonicalGraphApiSpec.md`
  - 負責定義 AI / 程式如何操作 canonical graph
- `Documentation/CanonicalGraphJsonContract.md`
  - 負責定義第一個正式 Plain JSON wire contract
- `Documentation/DeveloperModeOutputContract.md`
  - 負責規定 projection 輸出到玩家模式時的正式契約
- `Documentation/GraphToolkitSpec.md`
  - 負責 GraphToolkit API 與工具開發規範
- `Documentation/UIToolkitSpec.md`
  - 負責 UI Toolkit API 與 UI 輸出規範

也就是說：

- 本文件回答「為什麼要這樣設計」
- schema / API / 契約 / spec 文件回答「具體要怎麼做才算合法」

## 10. 一句總結

本專案的最終目標不是做一個只能在 Unity 裡拖拉的編輯器。

而是做一個：

> **以敘事圖 schema 為核心，能被 AI 與人類共同操作，並能投影到 current GraphToolkit、未來 Web 作者工具、Ink、Unity 與測試系統的敘事圖協定。**
