# 調查發現

## 初始觀察
- 專案根目錄同時存在 `Packages/com.opsidanos.ink`、`Packages/Ink`、`Assets/Editor/FlowChart`、`UIToolkitSpec.md`、`GraphToolkitSpec.md`。
- `PlanningWithFiles` 下已累積大量歷史任務，顯示專案長期圍繞 Ink Runtime、Graph Toolkit、測試與專案狀態盤點持續演進。

## 第一輪文件收斂
- 根目錄 `README.md` 仍保留原始 `ink-unity-integration` 說明，核心能力是 `.ink -> .json` 編譯、Editor 內播放與偵錯、Inspector 工具。
- `Packages/com.opsidanos.ink/README.md` 顯示此專案真正要交付的是 `com.opsidanos.ink` 套件：用 `Ink + UI Toolkit` 做文字冒險 / 視覺小說框架。
- 目前主資料流已被明確寫成三段：
  1. 開發者模式：Flow Chart / GraphToolkit 輸出 `.ink`
  2. Ink Unity Integration：把 `.ink` 編譯成 `.json`
  3. 玩家模式：Runtime 只讀 `.json` 播放
- `DeveloperModeOutputContract.md` 說明了專案在乎的不只是「能播」，而是「同一句輸出在正常播放、讀檔、倒帶、任意起始畫面」都要收斂成同一結果。
- 2026-02-11 的 `project_status_assessment` 表示：
  - Runtime 玩家主線已成形，核心是 `InkStoryEngine`、`InkTagEventRouter`、`VNPlayerPresenter`、`InkSaveSystem`
  - Editor GraphToolkit 主線仍在進行中，但匯入 / 匯出 MVP 與測試已存在
  - 專案近期重心偏向 GraphToolkit 與 Restore / Rollback 閉環

## 第二輪規格與封裝觀察
- `Packages/com.opsidanos.ink/package.json` 已把套件定位寫死：
  - 套件名：`com.opsidanos.ink`
  - Unity 版本：`6000.3`
  - 依賴：`com.inkle.ink-unity-integration`、`com.unity.addressables`
  - Sample：`玩家模式快速開始`
- 這表示 `OpsidanosInk` 不是取代 Ink，而是站在 Ink 之上再加一層「VN 框架 + 資源映射 + UI + 存讀檔」。
- `UIToolkitSpec.md` 與 `GraphToolkitSpec.md` 不是產品說明文件，而是「AI/協作者在動手前不能踩錯 API 邊界」的規格護欄。
- `Runtime` 目錄結構也很像產品分層：
  - `Story/`：故事與 Tag 解析
  - `Presentation/`：背景、音效、CG、角色、抖動等演出播放器
  - `Save/`：存讀檔與倒帶
  - `UI/`：UI Toolkit Presenter 與可阻擋前進的介面
  - `Runtime/UI/UXML + USS`：實際玩家畫面樣板

## 第三輪程式碼主線觀察
- `InkStoryEngine` 是 Runtime 的故事中心：
  - 啟動時用 `story.json` 建立 `Ink.Runtime.Story`
  - 每次 `Continue` / `ChooseChoice` 後產生 `StoryOutput`
  - `StoryOutput` 內已帶 `speaker`、`rawTags`、`parsedTags`、`choices`、`HasEnded`、`Source`
  - 也提供 `TryGetStoryStateJson`、`TryLoadStoryStateJson`、`EmitExternalOutput` 給存讀檔/倒帶使用
- `InkTagEventRouter` 負責把結構化 Tag 分流成事件：
  - 已知 key 包含 `speaker`、`bg`、`bgm`、`se`、`shake`、`char`、`char-left/center/right`、`cg`
  - 也會印出對開發者友善的 Tag log
- `InkSaveSystem` 不是只有單一存檔，而是已支援：
  - rollback buffer
  - 手動 1~3 槽
  - Auto 槽
  - Restore 時重送外部輸出，讓 UI 與 Tag 演出一起回到那一刻
- `VNPlayer.uxml` 顯示玩家 UI 已有相當具體的 VN 介面：
  - 背景層、角色層、CG 層、對話層、效果層
  - 回看、自動、快轉、隱藏 UI、存讀檔、多槽、自動槽、倒帶
- `InkFlowChartGraph` / `Exporter` / `Importer` 已把 Editor 主線明確化：
  - 建立 `.inkfc` 圖資產
  - 匯出成 `.ink + .flowchart.json`
  - 由 `.flowchart.json + .ink` 匯回 `.inkfc`
  - Graph export/import 會驗證開始節點、分岔規則、port 連線與 sidecar 完整性
- 目前 Editor 重點不是做「即時執行圖」，而是做「可視化 authoring -> 匯出 Ink -> 可逆匯入」閉環

## 第四輪整體收斂
- 專案真正的「產品本體」是 UPM 套件 `com.opsidanos.ink`；根倉庫保留原始 `ink-unity-integration` 地基，外加這個專案自己的 Runtime 與 Editor 工具鏈。
- asmdef 邊界很清楚：
  - `OpsidanosInk.Runtime`：Runtime 播放器主體
  - `OpsidanosInk.FlowChartEditor`：GraphToolkit 作者工具
  - `OpsidanosInk.EditModeTests`
  - `OpsidanosInk.PlayModeTests`
- 這代表 Flow Chart / GraphToolkit 雖然很重要，但目前仍比較像此 repo 內的開發者模式工具鏈，不是已經封裝進對外套件的穩定 API。
- 目前可直接數到的測試量：
  - EditMode：24
  - PlayMode：17
  - 合計：41
- 測試重心證明了專案現況：
  - Runtime 已經在驗證存讀檔、倒帶、多槽、自動槽、UI 點擊節奏、Restore 閉環，不再只是 Demo
  - GraphToolkit 正在驗證匯入、匯出、round-trip、v1/v2 規則、條件/選項/資料線與流程線分離，顯示它仍在快速收斂
- 最重要的總結不是「它能播 Ink」，而是：
  - 用圖編故事
  - 匯出成 Ink 與 sidecar
  - 編譯成 `story.json`
  - Runtime 播放、讀檔、倒帶後仍收斂到同一狀態
  這條閉環才是整個專案的真正核心

## 第五輪：使用者澄清後的真正架構北極星
- 使用者明確澄清：真正想要的不是「圍繞 GraphToolkit 的 Unity 工具」，而是「以 Graph JSON / Schema / API 為唯一語意真相」的敘事圖系統。
- 這代表先前從 repo 現況推測出的「GraphToolkit 看起來像主稿」只適用於目前過渡期實作觀察，不代表最終設計方向。

### Canonical Schema / Projection / Runtime Adapter / Validation Loop
1. Canonical Schema
 - `Graph JSON / Schema / API` 才是唯一真相來源。
 - 真相必須能被 AI 與人類共同操作，不能綁死某個編輯器或某個 runtime。
2. Projection
 - `GraphToolkit Editor` 的角色是「完整表現真相的人類可編輯視覺投影」。
 - 關鍵不是把 GraphToolkit 降成簡化外殼，而是它必須是 **無資訊遺失** 的視覺外殼。
 - `Ink` / `story.json` 也是 projection / compilation output，不是最終真相來源。
3. Runtime Adapter
 - 最終視覺小說執行設計不能依賴 Unity 才成立。
 - Unity Runtime 只是其中一個 adapter / renderer / validator，不應成為唯一執行語意宿主。
4. Validation Loop
 - 每完成一段 schema 或流程規則，就回頭讓 GraphToolkit 可視化、讓 Unity 可播放、讓測試可自動驗證。
 - 這些回頭補投影與測試的動作，不是架構偏移，而是為了建立「可看見、可驗證、可回歸」的閉環。

### 為什麼 repo 現在看起來像「到處都有真相」
- 不是因為北極星混亂，而是因為專案處在「Schema-first 設計 + 階段性投影驗證」的過渡態。
- 目前同時存在：
 - `.inkfc` 圖資產
 - `.flowchart.json`
 - `.ink`
 - `story.json`
 - Unity Runtime 呈現與測試
- 這些更準確的定位不是多個平行真相來源，而是：
 - 一個想被建立的 canonical truth
 - 多個為了作者體驗、播放驗證、自動化回歸而存在的工作投影

### 對本專案更準確的重新命名
- 這個專案不是單純的 Unity VN 工具，也不是 GraphToolkit 專案。
- 更準確地說，它是在做：
 - 一個以敘事圖 schema 為核心的系統
 - AI 與人類都能操作同一份語意真相
 - GraphToolkit 負責人類高效率編輯
 - Unity 負責其中一個視覺化播放器與驗證環境
 - 匯出到 Ink 與 `story.json` 是為了相容既有敘事與測試管線

### 對目前觀察結果的修正
- 「GraphToolkit 看起來像主稿」這句話只描述了目前 repo 的工作節奏，不應上升為架構結論。
- 更合理的判讀是：
 - 架構意圖：`Schema-first`
 - 當前實作：仍含大量為了視覺檢驗與自動化驗證而存在的投影耦合
 - 專案現況：正在把可視化、播放、匯出、測試逐步對齊到 canonical schema

## 第六輪：正式文件落點觀察
- `Documentation/` 目前只有：
  - `DeveloperModeOutputContract.md`
  - 兩份 PDF
  - `InkPlayerWindow.md`
- 根目錄沒有現成的 Architecture 文件。
- 目前最合理的正式落點，是在 `Documentation/` 新增一份架構說明文件，與 `DeveloperModeOutputContract.md` 同層。
- 這份文件的角色不應取代契約文件，而應補上：
  - canonical schema 的定位
  - projection / adapter / validation loop 的關係
  - 為什麼 repo 現況看起來像多重真相

## 第七輪：正式架構文件已建立
- 已新增 `Documentation/NarrativeGraphArchitecture.md`。
- 這份文件的定位是「架構總覽」，不是輸出契約，也不是 API spec。
- 它現在負責回答三件事：
  - 真相來源在哪裡
  - GraphToolkit / Ink / Unity / Tests 各自是什麼角色
  - 為什麼目前 repo 的多重投影現象是開發驗證策略，不是架構混亂
