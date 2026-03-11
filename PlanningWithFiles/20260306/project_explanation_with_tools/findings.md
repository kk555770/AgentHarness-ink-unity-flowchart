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

## 第八輪：下一步文件工作的關鍵矛盾
- `Documentation/DeveloperModeOutputContract.md:410` 目前明寫：`.flowchart.json`（sidecar）是可逆閉環的權威來源。
- 新增的 `Documentation/NarrativeGraphArchitecture.md` 則已寫明：唯一語意真相應是 `Graph JSON / Schema / API` 的 canonical schema，而不是某一份匯出檔。
- 這代表目前正式文件之間存在「過渡期觀察」與「最終架構北極星」的語意衝突。
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExportModels.cs` 目前只有 `ExportGraphDto / ExportNodeDto / ExportNodeOutputDto`，比較像 sidecar DTO，還不是獨立命名與責任都清楚的 canonical schema 文件。
- 因此下一步最合理的文件工作，不是立刻擴寫很多新 spec，而是先把三層東西切開：
  1. canonical schema / graph API 的正式定位
  2. `.flowchart.json` 作為目前 sidecar / projection / interchange format 的定位
  3. `DeveloperModeOutputContract` 作為 projection 到 runtime 的合法契約

## 第九輪：文件收斂已完成
- 已新增 `Documentation/CanonicalGraphSchema.md`，專門定義 canonical truth 應包含哪些圖語意、哪些責任不能混進 projection 或 runtime。
- 已修改 `Documentation/NarrativeGraphArchitecture.md`：
  - 補上對 `CanonicalGraphSchema.md` 的引用
  - 明確把 `.flowchart.json` 定位成目前 Graph v2 的 sidecar / interchange / round-trip 投影格式
  - 補齊架構總覽、schema 文件、輸出契約三者的分工
- 已修改 `Documentation/DeveloperModeOutputContract.md`：
  - 明確說明本文件只處理 canonical schema 投影到 runtime 的合法輸出契約
  - 把 `.flowchart.json` 從「最終權威來源」收斂為「目前 Graph v2 的權威投影格式」
  - 保留目前匯入器仍以 sidecar 還原的實務前提，避免和現行實作脫節

## 第十輪：下一步最適合補的是 schema spec，不是再一份架構稿
- `CanonicalGraphSchema.md` 目前已經把責任邊界寫清楚，但仍停在「原則層」。
- `InkFlowChartNodes.cs` 已經實際存在一批可作為 schema seed 的正式元素：
  - node types：`start / action / stageAction / comment / choice / condition`
  - flow port：`Flow`
  - dialogue action input ports：`ActionIn*`
  - stageAction data output：`ActionData`
  - choice / condition 的 output count 與文字規則
- `InkFlowChartExportModels.cs` 也已經存在目前 sidecar 的最小 DTO：
  - `ExportGraphDto`
  - `ExportNodeDto`
  - `ExportNodeOutputDto`
- 因此下一步最合理的文件工作，是把「目前 code 裡已存在的節點 / port / edge / payload / invariant」正式整理成 schema spec，而不是再多寫一份抽象願景文件。
- 這份 schema spec 應該回答：
  1. 每個 canonical node type 的最小欄位
  2. 哪些 port 屬於流程線，哪些屬於資料線
  3. edge 最少要保存哪些欄位
  4. sidecar DTO 與 canonical schema 的對應關係
  5. 哪些規則屬於 schema invariant，哪些只是目前 projection 限制

## 第十一輪：使用者補充的裁決規則
- 使用者明確要求：實際腳本可能有缺陷或尚未改正；若 script 現況與標準化方向衝突，不能盲目以現況 script 為準。
- 接下來撰寫 schema spec 的裁決順序應是：
  1. 使用者已確認的架構北極星
  2. 更能標準化、可長期維護的 schema 設計
  3. 現況 script 作為「目前實作映射」與「兼容註記」
- 若衝突牽涉高影響決策，應主動向使用者詢問，並同時說明：
  - 傾向方案
  - 優點
  - 缺點
  - 未來展望
  - 可能後果
- 這代表下一份 schema spec 不應只是 code dump，而應明確區分：
  - 規範層（normative）
  - 現況映射層（current implementation note）

## 第十二輪：schema spec 前的高影響歧義
- 在 `InkFlowChartNodes.cs` 中，實際節點型別常數目前是：
  - `start`
  - `action`
  - `stageAction`
  - `comment`
  - `choice`
  - `condition`
- 但從語意上看，`action` 目前其實對應的是「對話節點」：
  - 類別名是 `InkFlowDialogueNode`
  - 顯示名稱是「對話」
  - payload 代表的是對話內容
  - `stageAction` 才是專門的動作資料節點
- 這代表 canonical schema spec 目前遇到第一個高影響命名衝突：
  - 要沿用現況 sidecar / code token `action`
  - 還是把 canonical node type 正規化成語意更準的 `dialogue`
- 這個決策會影響：
  - 未來 AI API 的 node type 命名
  - canonical schema 與 sidecar 的 mapping 複雜度
  - 歷史資料與現況 exporter/importer 的相容策略

## 第十三輪：使用者已確認 `dialogue` 為 canonical node type
- 使用者已選擇：
  - canonical schema 採用 `dialogue`
  - 現況 `action` 僅保留為 legacy sidecar / implementation mapping
- 這個決策已正式落入 schema spec，避免把歷史命名錯誤升格成長期規範。

## 第十四輪：schema spec 已建立
- 已新增 `Documentation/CanonicalGraphSchemaSpec.md`。
- 新 spec 已正式寫入：
  - graph / node / edge 最小欄位
  - canonical port 類型：`flow-in`、`flow-out`、`branch-out[index]`、`action-data-out`、`dialogue-action-in[index]`
  - canonical node types：`start`、`dialogue`、`stageAction`、`comment`、`choice`、`condition`
  - graph invariants
  - text payload 不得偷藏流程結構
  - canonical schema 與目前 sidecar / GraphToolkit 的 mapping

## 第十五輪：重新完整盤點的起點
- 今天現存的 `PlanningWithFiles/20260306/project_explanation_with_tools` 已包含：
  - 架構總覽
  - canonical schema 原則
  - canonical schema spec
  - canonical graph API spec
  - plain JSON contract 後續待提案
- 根目錄 `README.md` 仍明顯保留上游 `ink-unity-integration` 定位，尚未反映 repo 已演變成 `com.opsidanos.ink + Graph authoring + runtime validation` 的較大系統。
- `ProjectSettings/ProjectVersion.txt` 顯示目前 Unity 版本仍是 `6000.3.9f1`，與專案 AGENTS 規範一致。
- `Packages/manifest.json` 顯示目前技術底座仍穩定圍繞：
  - `com.unity.graphtoolkit`
  - `com.unity.addressables`
  - `com.unity.ai.navigation`
  - `com.unity.inputsystem`
  - `com.unity.test-framework`
  - `com.coplaydev.unity-mcp`
- 這說明 repo 的作者工具、Runtime 播放、測試自動化與 Unity-MCP 檢查鏈都仍在同一個工作倉庫內，沒有拆成多 repo。

## 第十六輪：文件與目錄地圖已再次確認
- 根目錄結構目前可分成 6 個主要區塊：
  - `Documentation/`：正式設計與契約文件
  - `Packages/com.opsidanos.ink/`：真正產品套件
  - `Assets/Editor/FlowChart/`：GraphToolkit 作者工具
  - `Assets/Editor/Tests/`：EditMode 測試
  - `Assets/Tests/PlayMode/`：PlayMode 測試
  - `PlanningWithFiles/`：長期規劃與調查記憶
- `Documentation/` 現在已形成清楚的文件分層，不再只是零散筆記：
  - `NarrativeGraphArchitecture.md`：架構總覽
  - `CanonicalGraphSchema.md`：schema 原則邊界
  - `CanonicalGraphSchemaSpec.md`：schema 具體規格
  - `CanonicalGraphApiSpec.md`：canonical 操作 API
  - `CanonicalGraphJsonContract.md`：plain JSON wire contract
  - `DeveloperModeOutputContract.md`：projection 到 runtime 的輸出契約
- `DeveloperModeOutputContract.md` 仍明確保留目前實作落點：
  - Graph v2
  - Flow Chart 節點規範
  - `.flowchart.json` 作為目前可逆閉環的權威投影格式
  這表示 canonical truth 與 current sidecar 已有分層，但 sidecar 在現況工作流中仍是核心操作面之一。
- `Packages/com.opsidanos.ink/README.md` 已把使用方式寫成實作導向手冊：
  - 如何掛 `InkStoryEngine`
  - 如何掛 `VNPlayerPresenter`
  - 如何加 `InkSaveSystem`
  - 如何把不同 Tag Player 與 `InkResourceMap` 接起來
  這份 README 比根 README 更接近現在真正產品的「使用者入口」。
- `Packages/com.opsidanos.ink/Runtime/OpsidanosInk.Runtime.asmdef` 搭配 `Assets/Editor/FlowChart/OpsidanosInk.FlowChartEditor.asmdef`、`Assets/Editor/Tests/OpsidanosInk.EditModeTests.asmdef`、`Assets/Tests/PlayMode/OpsidanosInk.PlayModeTests.asmdef`，已形成明確的產品 / 作者工具 / 測試邊界。
- `Packages/com.opsidanos.ink/Runtime/` 的檔案樹再次驗證 Runtime 目前分四層：
  - `Story/`
  - `Presentation/`
  - `Save/`
  - `UI/`
  再加上 `UI/UXML`、`UI/USS` 作為實際玩家介面樣板。
- `Assets/Editor/FlowChart/GraphToolkit/` 目前核心檔案集中在：
  - `InkFlowChartGraph.cs`
  - `InkFlowChartNodes.cs`
  - `InkFlowChartExporter.cs`
  - `InkFlowChartImporter.cs`
  - `InkFlowChartExportModels.cs`
  這說明作者工具目前主線仍然是「圖資產 + 節點定義 + 匯出/匯入 + sidecar DTO」。
- 測試樹再次驗證：
  - EditMode 偏重 Graph 匯入匯出、round-trip、resource map、save data
  - PlayMode 偏重實際播放與 UI 點擊節奏
  這與先前對 repo 成熟度的判讀一致，尚未看到方向反轉。

## 第十七輪：Runtime 串接主線已再次確認
- `InkStoryEngine.cs` 仍是 Runtime 第一個核心中樞：
  - `Start()` 時讀 `story.json`
  - `Continue()` / `ChooseChoice()` 都走 `EmitNext()`
  - 產出統一的 `StoryOutput`
  - 也提供 `TryGetStoryStateJson()`、`TryLoadStoryStateJson()`、`EmitExternalOutput()` 給存讀檔與 restore 使用
- `StoryOutput.cs` 的 shape 很關鍵，因為它把 Runtime 後續每一層都串在一起：
  - `OutputId`
  - `Speaker`
  - `LineText`
  - `HasEnded`
  - `Tags`
  - `ParsedTags`
  - `Choices`
  - `Source`
- `InkTagParser.cs` 與 `InkTag.cs` 表示 Tag 已不是單純字串 log，而是先轉成結構化 `InkTag(Key, Value)`，後面所有 Tag Player 都吃這個格式。
- `InkTagEventRouter.cs` 是第二個中樞：
  - 訂閱 `InkStoryEngine.OutputGenerated`
  - 逐個 `RouteTag`
  - 對 `speaker/bg/bgm/se/shake/char/char-left/center/right/cg` 分別丟事件
  - 未知 tag 也保留獨立事件與 log
  這代表 Tag 分流是事件匯流排設計，不是把所有演出硬寫死在 story engine 裡。
- `InkSaveSystem.cs` 是第三個中樞，而且成熟度比「最小存檔」高很多：
  - 有 rollback buffer
  - 有手動 1~3 槽
  - 有 auto 槽
  - Save 時不只存 ink state，還會一起存 `PresentationSnapshot`
  - Restore 時會先 `TryLoadStoryStateJson()`，再重建 `StoryOutput`，最後套回 presentation 狀態
- `InkSaveData.cs` 明確顯示目前快照模型已含三層：
  - `inkStateJson`
  - `StoryOutputSnapshot`
  - `PresentationSnapshot`
  其中 presentation 已含 `bgId`、`bgmId`、`cgValue`、`charValue`、`charLeftId/CenterId/RightId`，說明存檔目標不是只把文字捲回去，而是要讓畫面也一起收斂。
- `VNPlayerPresenter.cs` 是第四個中樞：
  - 直接綁 `UIDocument`
  - 直接訂閱 `InkStoryEngine`
  - 直接呼叫 `InkSaveSystem`
  - 內建 continue、choice、backlog、auto、skip、hide UI、manual slot、auto slot、rollback
  - 也有 typewriter、click cooldown、auto countdown、busy blocker
  這已經不是最小 demo presenter，而是完整玩家控制器。
- `IAdvanceBlocker.cs` 搭配 `VNPlayerPresenter` 與 `InkTagCharacterStatePlayer`，表示「某些演出未播完時不能前進」已被抽成可擴充介面，不是單寫死在字機效果。
- `VNPlayer.uxml` 與 `VNPlayer.uss` 再次證明 UI 已經是完整 VN 版型，不只是名字＋文字＋選項：
  - 背景層
  - 角色層
  - CG 層
  - 對話層
  - 系統按鈕列
  - Backlog 面板
  - 顯示 UI 按鈕
  - Click effect / Minigame layer 預留層
- Presentation 層目前最明顯是「Tag Player + ResourceMap」設計：
  - `InkTagAudioPlayer`
  - `InkTagBackgroundPlayer`
  - `InkTagCgPlayer`
  - `InkTagCharacterPlayer`
  - `InkTagCharacterStatePlayer`
  - `InkTagShakePlayer`
  - `InkResourceMap`
  這說明故事語意與具體畫面/音訊資源已被拆開。
- `InkResourceMap.cs` 顯示資源載入策略已清楚區分：
  - Editor / PlayMode 用 `AssetDatabase` 的 `assetPath`
  - Player build 用 Addressables `address`
  這很像典型 adapter，目的不是把 resource id 綁死在單一執行環境。
- `InkTagCharacterStatePlayer.cs` 是目前 Presentation 層最複雜、也最成熟的演出播放器之一：
  - 吃 `char` JSON
  - 有 `transition.steps`
  - 支援 `appear/move/disappear`
  - 支援 `raiseActors`
  - 支援 rollback / restore 時相反排序規則
  - 自己實作 `IAdvanceBlocker`
  這直接呼應 `DeveloperModeOutputContract.md` 那條「可重播快照式輸出」主線。

## 第十八輪：測試重心再次驗證成熟度
- EditMode 測試主線沒有偏離：
  - `InkFlowChartExportTests.cs` 驗證匯出成功與非法內容失敗
  - `InkFlowChartImportTests.cs` 驗證 sidecar 匯回圖資產與錯誤條件
  - `InkFlowChartRoundTripTests.cs` 驗證 Graph v2 choice / condition / dialogue+stageAction 的 round-trip
  - `InkResourceMapTests.cs` / `InkResourceMapAddressablesTests.cs` 驗證資源映射可載入且 address 完整
  - `InkSaveDataTests.cs` 驗證 save data / save bank JSON round-trip
- PlayMode 測試也不是只驗 happy path：
  - `OpsidanosInkPlayModeTests.cs` 驗證 save/load/rollback/multi-slot/auto-slot
  - 同時驗證 `char` restore 與 transition steps 收斂
  - `OpsidanosInkPlayModeUiClickTests.cs` 驗證 backlog/auto/skip/hide UI/button cooldown/rollback 連按防暴走
- 這批測試名稱本身就透露目前專案最在乎的是：
  - restore 後是否真的回到同一狀態
  - UI 點擊節奏是否一致
  - round-trip 是否保真
  而不是只有「故事能不能往下播」。

## 第十九輪：文件責任邊界與過渡區再次驗證
- `NarrativeGraphArchitecture.md` 現在已把控制面分層講得很清楚：
  - `Canonical Schema`
  - `Canonical Graph API`
  - `Plain JSON Contract`
  - `JSON-RPC / HTTP / CLI Adapter`
  這再次證明 JSON-RPC 在目前設計裡是 transport adapter，不是 canonical truth。
- `CanonicalGraphJsonContract.md` 也再次明寫：
  - 自己不是 canonical truth
  - 不是 projection format
  - 不是 GraphToolkit sidecar
  這和 `NarrativeGraphArchitecture.md` 的 transport 分層一致。
- `CanonicalGraphSchemaSpec.md` 已正式把 canonical node type 定為：
  - `start`
  - `dialogue`
  - `stageAction`
  - `comment`
  - `choice`
  - `condition`
  同時把現況 `type = "action"` 明確定位成 legacy sidecar / current mapping。
- `DeveloperModeOutputContract.md` 與 schema spec 之間目前最容易讓人誤會的點仍然存在：
  - schema spec 講的是 canonical `dialogue`
  - output contract 的 Graph v2 區塊仍大量用 `action/stageAction/comment`
  這不代表文件互相打架，而是代表它們分別站在「canonical 層」與「current Graph v2 projection 層」說話。
- `DeveloperModeOutputContract.md` 已明白補上新的邊界註記：
  - `.flowchart.json` 是「目前 Graph v2 可逆閉環的權威投影格式」
  - 但不應凌駕於 canonical schema 之上
  這表示目前正式文件已經知道並主動處理這個歧義。
- `DeveloperModeOutputContract.md` 仍收錄 `character` / `castBundle` 一整套規格：
  - 資料線
  - `AddressableKey`
  - sidecar 欄位
  - `CastIn` / `CharacterIn*`
  但 `CanonicalGraphSchemaSpec.md` 的正式 canonical node types 目前沒有把它們列進核心集合。
- 因此目前最像「仍在過渡中」的，不是 Runtime 主線，而是：
  - canonical schema 與 current Graph v2 projection 的覆蓋範圍尚未完全重疊
  - 特別是 `character/castBundle` 這類資料節點仍較像 projection / authoring layer 的設計，還沒有被正式升格成 canonical schema 核心節點。

## 第二十輪：Presentation 層的新舊路線共存現況
- `InkTagCharacterPlayer.cs` 仍存在，負責舊式：
  - `char-left`
  - `char-center`
  - `char-right`
  直接把貼圖塞到三個固定槽位。
- `InkTagCharacterStatePlayer.cs` 則是新版主線：
  - 吃 `char` JSON
  - 用 state snapshot + transition steps 驅動角色層
  - 支援 rollback / restore 對齊
  - 實作 `IAdvanceBlocker`
- `InkTagCgPlayer.cs`、`InkTagBackgroundPlayer.cs`、`InkTagAudioPlayer.cs`、`InkTagShakePlayer.cs` 都走一致模式：
  - 訂閱 `InkTagEventRouter`
  - 優先吃 `InkResourceMap`
  - 找 UI element / AudioSource 套演出
- 這代表 Presentation 層已經有一致的 adapter 風格，但角色系統目前仍可看見：
  - 舊三槽 tag 路線
  - 新快照式 `char` JSON 路線
  也就是一個明顯可見的過渡痕跡。

## 第十九輪：正式文件語意邊界再次驗證
- `NarrativeGraphArchitecture.md` 的核心立場仍然很鮮明：
  - 真相在 `Graph JSON / Schema / API`
  - GraphToolkit 是 projection
  - Unity 是 runtime adapter
  - 驗證閉環是架構的一部分，不是附屬品
- `CanonicalGraphJsonContract.md` 已清楚把自己定位成：
  - canonical graph 語意 API 的第一個 plain JSON wire contract
  - 薄 envelope
  - transport-light
  - 可被 JSON-RPC 後續包裝
  也就是說它已明確避開把 transport 殼誤升格成 canonical truth。
- `DeveloperModeOutputContract.md` 現在也有明講：
  - 本文件只處理 canonical schema 投影到 Ink / `story.json` / Runtime 的合法輸出
  - Flow Chart 是目前人類作者最重要的視覺投影
  - Runtime 不應靠「補洞」容錯來掩蓋輸出問題
- 三份文件放在一起看，邊界目前已比前幾輪更清楚：
  - `Architecture`：世界觀與角色分工
  - `Canonical*`：真相本體與控制面
  - `DeveloperModeOutputContract`：投影到玩家模式時不能踩線的輸出契約
- 但仍有一個容易讓新讀者誤會的地方：
  - `Architecture` 把 canonical schema 放在最高層
  - `DeveloperModeOutputContract` 裡又保留大量 Graph v2 / `.flowchart.json` / `action` 舊命名等現況細節
  如果只讀後者，仍可能把目前 sidecar / editor workflow 誤看成最終 canonical 定義。

## 第二十輪：場景與 Demo 資產再次證明 repo 不只是文件
- `Assets/Scene/Test.unity` 內已實際掛上：
  - `UIDocument`
  - `InkStoryEngine`
  - `VNPlayerPresenter`
  - `InkTagEventRouter`
  - `InkTagAudioPlayer`
  - `InkTagBackgroundPlayer`
  - `InkTagCharacterStatePlayer`
  - `InkTagShakePlayer`
  - `InkSaveSystem`
  這表示 Runtime 主線不只是「套件理論上可接」，而是 repo 裡有實際示範場景把整串接起來。
- `Assets/OpsidanosInk/Demo/story.ink` 不只是最小 hello world，而是刻意拿來驗：
  - `char` JSON
  - `transition.steps`
  - `raiseActors`
  - `bg/bgm/se/cg/shake`
  這和 `DeveloperModeOutputContract.md` 與 PlayMode 測試的主線完全對得上。
- `Assets/OpsidanosInk/Demo/resource_map.json` 已示範同一份映射同時保存：
  - `assetPath`
  - `address`
  - 背景 / BGM / SE / CG / character / actor expressions
  這再次證明 `InkResourceMap` 不是概念稿，而是已實際被 Demo 餵資料。
- `Packages/com.opsidanos.ink/Samples~/PlayerModeSample/story.ink` 則仍是較小的 UPM sample 入口：
  - 有最小故事
  - 但沒有完整 `story.json` 與 demo resource map 打包在 sample 內
  這表示 sample 偏向教你「如何開始」，而 repo 內 `Assets/OpsidanosInk/Demo/` 才更像完整驗證場。
  - 哪些欄位只是 projection / legacy convenience，例如 `nextIds`、`ActionInputCount`、`actionKind`
- `Documentation/CanonicalGraphSchema.md` 也已補上對 `CanonicalGraphSchemaSpec.md` 的引用，讓原則層與規格層分工更清楚。

## 第十五輪：AI 控制 API spec 已建立
- 已新增 `Documentation/CanonicalGraphApiSpec.md`。
- 新文件已正式定義：
  - graph lifecycle API
  - node mutation API
  - edge mutation API
  - validation API
  - projection API
  - stable error model
  - deterministic / idempotent 規則
- 這份文件明確選擇：
  - API 操作 canonical graph，而不是 GraphToolkit / Unity Editor 手勢
  - `ConnectPorts` / `DisconnectEdge` 對 AI 重試友善，採 idempotent 語意
  - `ValidateGraph` / `ProjectGraph` 應是 pure read-only operation
- 已同步修改：
  - `Documentation/NarrativeGraphArchitecture.md`
  - `Documentation/CanonicalGraphSchema.md`
  - `Documentation/CanonicalGraphSchemaSpec.md`
  讓架構、schema、schema spec、API spec 四層文件互相可追溯

## 第十六輪：重新盤點設計規範後，JSON-RPC 的正確落點
- 已重新閱讀：
  - `AGENTS.md`
  - `GraphToolkitSpec.md`
  - `UIToolkitSpec.md`
  - `Documentation/NarrativeGraphArchitecture.md`
  - `Documentation/CanonicalGraphSchema.md`
  - `Documentation/CanonicalGraphSchemaSpec.md`
  - `Documentation/CanonicalGraphApiSpec.md`
  - `Documentation/DeveloperModeOutputContract.md`
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExportModels.cs`
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodes.cs`
- 重新盤點後，現有正式文件對 transport 的立場其實已經很明確：
  - `CanonicalGraphApiSpec.md` 明寫「不定義某個 transport（HTTP / RPC / C# method）長什麼樣」。
  - `NarrativeGraphArchitecture.md` 明寫 AI 應操作 canonical graph 的語意 API，而不是 Editor 手勢。
  - `GraphToolkitSpec.md` 明寫 Graph Toolkit 是 Editor authoring framework，**不是 runtime execution backend**。
- 這代表如果「先試 JSON-RPC」，目前最不衝突的落點不是把 JSON-RPC 升格成 canonical API 本體，而是：
  - **在 transport / adapter 層加一份 JSON-RPC wire contract**
  - 由 JSON-RPC method 去包 canonical API operation
- 目前 repo 內尚未存在任何正式 JSON-RPC 規格或實作線索；全文搜尋 `JSON-RPC` / `jsonrpc` 沒有命中正式文件或程式碼。
- 現有 `.flowchart.json` sidecar 仍明確屬於 projection / interchange / round-trip format：
  - `ExportGraphDto.version`
  - `graphName`
  - `startNodeId`
  - `nodes[*].outputs[]`
  這些都是 Graph v2 投影格式，不是 transport contract。
- 因此若下一步改成「先試 JSON-RPC」，比較標準化的解法不是改寫 `CanonicalGraphApiSpec.md` 的核心語意，而是新增一份類似：
  - `CanonicalGraphJsonRpcAdapterSpec.md`
  - 或 `CanonicalGraphJsonContract.md`（但需明確切成 JSON-RPC adapter，而非 canonical truth）
- 目前最重要的語意邊界可重新收斂成：
  1. canonical truth：`Graph JSON / Schema / API`
  2. projection：`.flowchart.json` / `.ink` / `story.json` / GraphToolkit 畫面
  3. runtime adapter：Unity runtime
  4. transport adapter：未來可新增 `Plain JSON` 或 `JSON-RPC`
- 若採 JSON-RPC，method 名稱、params/result/error envelope 屬於 transport 規格；
  `CreateNode` / `ConnectPorts` / `ValidateGraph` / `ProjectGraph` 這些語意本體仍應留在 canonical API spec。

## 第十七輪：Plain JSON contract 已落成正式文件
- 已新增 `Documentation/CanonicalGraphJsonContract.md`。
- 新文件已正式把「第一個可機械呼叫的 JSON 控制格式」定為：
  - `contractVersion`
  - `operation`
  - `input`
  - `success`
  - `result`
  - `warnings`
  - `errors`
  - `applied`
- 新文件明確切開三種容易混淆的 JSON：
  1. canonical control JSON contract
  2. `.flowchart.json` sidecar / projection
  3. 未來可選的 JSON-RPC adapter
- 已同步修改 `Documentation/CanonicalGraphApiSpec.md`：
  - 補上對 `CanonicalGraphJsonContract.md` 的引用
  - 明確說明 API spec 描述的是語意形狀，不是 transport 欄位規範
  - 補強「Plain JSON / JSON-RPC / HTTP / CLI 都只是外層包裝」的邊界
- 已同步修改 `Documentation/NarrativeGraphArchitecture.md`：
  - 在 AI 控制那一段補上 `CanonicalGraphJsonContract.md`
  - 補出控制面正確分層：
    - `Canonical Schema`
    - `Canonical Graph API`
    - `Plain JSON Contract`
    - `可選的 JSON-RPC / HTTP / CLI Adapter`
- 重新確認後，目前專案文件的最穩路線可收斂成：
  - 先把 `Plain JSON Contract` 當作第一個正式 wire contract
  - 後續若需要，再在外層加 `JSON-RPC adapter spec`
  - 不應反過來讓 JSON-RPC 改寫 canonical API 或 sidecar / projection 角色

## 第十八輪：下一步應補的是 payload-level JSON spec
- `Documentation/CanonicalGraphJsonContract.md` 目前已經把 envelope 定下來：
  - `contractVersion`
  - `operation`
  - `input`
  - `success`
  - `result`
  - `warnings`
  - `errors`
  - `applied`
- 但對機械呼叫者來說，還缺三塊真正會影響互通性的細節：
  1. `graph snapshot` 的正式 JSON 形狀
  2. 每個 operation 的 `result payload` 最小欄位
  3. 每種 error / warning 的 `details` 最小欄位
- 如果不補這一層，雖然 envelope 已清楚，但 AI / tool / future adapter 還是得自己猜：
  - `result` 裡到底回 `graphId`、`graphSnapshot`、`nodeRef` 還是 `edgeRef`
  - `errors[*].details` 應該固定有哪些欄位
  - `GetGraph` / `ProjectGraph` / `ImportProjection` 的 payload 是否共用同一組 snapshot/ref 規格
- 因此下一步最合理的文件工作，不是立刻做 JSON-RPC adapter，而是先把 Plain JSON contract 的 payload-level spec 補完整。

## 第十九輪：重新完整盤點後的全貌驗證
- 重新看完整批文件、程式碼、樣板、Demo、asmdef 與測試後，repo 的主線沒有翻盤，仍可穩定收斂成兩條閉環：
  1. 作者工具閉環：`.inkfc <-> .flowchart.json + .ink`
  2. 玩家模式閉環：`story.json -> Runtime -> Save/Load/Rollback -> Restore`
- 這兩條閉環都不是孤立存在，而是共同服務同一個更大的方向：
  - 長期真相：canonical narrative graph
  - 現況工作投影：GraphToolkit / `.flowchart.json` / `.ink` / `story.json`
  - 目前最成熟的 runtime adapter：Unity Runtime

## 第二十輪：Runtime 再次驗證後的成熟度判讀
- `InkStoryEngine.cs`、`StoryOutput.cs`、`InkTagEventRouter.cs` 再次證明 Runtime 主幹已穩定：
  - 用統一 `StoryOutput` 串故事、Tag、選項、結束狀態
  - 用 `Source = Normal / Restore` 區分正常推進與回放
  - 用事件分流把故事引擎和演出播放器拆開
- `InkResourceMap.cs` 與 `resource_map.json` 再次證明資源綁定已進入集中式資料管線：
  - Editor 與 Player build 的資源定位都被納進同一份映射
  - 不是零散 demo 欄位
- `InkTagCharacterStatePlayer.cs`、`InkSaveSystem.cs`、`VNPlayerPresenter.cs` 再次說明目前最成熟的能力其實不是單純播放，而是：
  - `char` JSON 快照
  - `transition.steps`
  - `ForceComplete`
  - `IAdvanceBlocker`
  - 多槽 / Auto / rollback / restore
  - Busy click 節奏控制
- 這些能力與 PlayMode 測試名稱完全對得上，代表 Runtime 已不是「看起來能播」，而是「被測試守住可重播與可回復」。

## 第二十一輪：Editor / Graph 再次驗證後的成熟度判讀
- `InkFlowChartGraph.cs`、`InkFlowChartExporter.cs`、`InkFlowChartImporter.cs` 再次證明作者工具主線也已成形：
  - 可建圖
  - 可匯出 `.ink + .flowchart.json`
  - 可由 sidecar 匯回圖
  - 有 round-trip 測試
- `InkFlowChartExportModels.cs` 與測試名稱再次說明 Graph v2 已是目前正式工作版本：
  - `outputs / label / condition / isElse / toPortName`
  - `choice`
  - `condition`
  - `stageAction -> dialogue` 資料線
- 因此作者工具目前最像「收斂中的正式主線」，而不是「隨手做的原型」。

## 第二十二輪：仍在過渡中的語意接縫
- `dialogue` vs `action`
  - canonical 已裁定 `dialogue`
  - current code / sidecar / 契約文件仍大量保留 `action`
- `.flowchart.json` 的邊界
  - 它已經非常接近正式 schema
  - 但目前正確定位仍是「Graph v2 的權威投影格式」，不是最終 truth
- `character / castBundle`
  - `DeveloperModeOutputContract.md` 已寫得很完整
  - 但它們尚未像六大核心節點那樣完整進入 canonical schema spec
  - 這代表它們目前更像 projection-heavy 節點或資料層節點
- 根目錄 `README.md`
  - 仍偏上游 `ink-unity-integration`
  - 容易讓第一次接觸 repo 的人低估 `OpsidanosInk + Graph authoring + restore/rollback validation` 已成為真正主體

## 第二十三輪：這次重新閱讀後最準確的專案描述
- 這個 repo 不是單純的「Unity Ink 套件」。
- 也不是單純的「GraphToolkit 故事編輯器」。
- 更準確的描述仍是：
  - 一個以敘事圖 schema 為長期真相方向的系統
  - 人類可透過 GraphToolkit 做可視化 authoring
  - 系統可投影成 `.ink` 與 `story.json`
  - Unity Runtime 目前負責最成熟的播放、演出、存讀檔、倒帶與驗證閉環
- 因此今天重新完整盤點後，先前的理解不但沒有被推翻，反而被文件、測試、Demo 與核心程式碼再次互相印證。

## 第二十四輪：第一版修改提案的建議排序
- 第一優先仍建議先改「文件語意接縫」，不是先改 Runtime 功能。
- 目前最值得先動的第一刀是：
  - 在 `DeveloperModeOutputContract.md` 明文切開：
    - canonical `dialogue`
    - legacy sidecar token `action`
    - 真正的 `stageAction`
- 這一刀的價值最大，因為它同時影響：
  - schema spec 的可讀性
  - exporter / importer 的命名收斂
  - 未來 AI API 與 JSON contract 的語意穩定性
- 第一版提案的推薦順序可收斂成：
  1. 文件：`dialogue` / legacy `action` 對照與邊界
  2. 文件：`character / castBundle` 是否屬 canonical 的正式裁決
  3. 程式碼：把 `dialogue <-> action` 的 mapping 集中到單一 helper / 常數層
  4. 文件入口：更新根 `README.md` 的目前 repo 定位與閱讀順序

## 第二十五輪：第一版文件收斂已完成
- 已修改 `Documentation/DeveloperModeOutputContract.md`：
  - 新增 `6.2.0 命名裁決與 legacy mapping`
  - 明文切開：
    - canonical `dialogue`
    - current sidecar token `action`
    - 真正的 `stageAction`
  - 原本會把對話節點寫成 `action` 的段落，已改成 `dialogue`，並在需要處保留 `action` 作為 legacy token 註記
- 已修改 `Documentation/CanonicalGraphSchemaSpec.md`：
  - 補上對輸出契約文件的解讀註記
  - 明確說明輸出契約若提到 `action`，應讀成 current sidecar token，而不是 canonical `nodeType`
- 已修改 `Documentation/CanonicalGraphSchema.md`：
  - 補上 projection 可以暫時保留 legacy token 的原則說明
- 這一刀沒有改 sidecar 格式、沒有改 exporter/importer、沒有改 Runtime。
- 收斂後的效果是：
  - `dialogue` / `action` / `stageAction` 三者的邊界在正式文件裡更容易一眼看懂
  - 後續若要補 `character / castBundle` 或再往下收斂 code mapping，起點會乾淨很多

## 第二十六輪：`character / castBundle` 的現況落點
- 目前 `character / castBundle` 的主要存在位置幾乎都在 `Documentation/DeveloperModeOutputContract.md`：
  - 節點定義
  - 資料線規則
  - Addressable 規則
  - sidecar 欄位
  - 匯入相容策略
- 但重新全文搜尋後，`Assets/Editor/FlowChart/GraphToolkit/` 與相關 EditMode 測試中，並沒有對應的 `character / castBundle` 節點程式主線、匯入匯出邏輯或 round-trip 測試。
- 目前實際存在、且被程式與測試支撐的角色資料主線，反而是 Runtime / ResourceMap：
  - `InkResourceMap`
  - `InkTagCharacterPlayer`
  - `InkTagCharacterStatePlayer`
  - `resource_map.json`
- 這代表 `character / castBundle` 目前更像：
  - Graph v2 projection / authoring 設想
  - 或尚未落地完成的資料層節點提案
  而不是已被實作與測試收斂過的 canonical 核心節點。

## 第二十七輪：第二版提案的傾向結論
- 第二版若要低風險高報酬，最合理的方向不是立刻把 `character / castBundle` 升格為 canonical node。
- 更合理的做法是：
  - 先把它們正式標記為「目前屬 projection-heavy / authoring data-source nodes」
  - 明確寫出「尚未正式進入 canonical 核心節點集合」
  - 待未來真的有 GraphToolkit 節點實作、sidecar 穩定格式、匯入匯出邏輯與 round-trip 測試後，再考慮升格

## 第二十八輪：第二版文件收斂已完成
- 已修改 `Documentation/CanonicalGraphSchema.md`：
  - 正式把 `character / castBundle` 排除在目前 canonical 核心節點集合之外
  - 補上「若某節點只有 projection 契約、沒有 stable payload / port / edge / round-trip 驗證，就不應搶先升格 canonical core」的原則
- 已修改 `Documentation/CanonicalGraphSchemaSpec.md`：
  - 明確列出 `character / castBundle` 目前不納入 canonical core types
  - 補上未來若要升格所需的門檻：
    - stable payload
    - stable port / edge semantics
    - importer / exporter mapping
    - round-trip tests
- 已修改 `Documentation/DeveloperModeOutputContract.md`：
  - 明文把 `character / castBundle` 定位成 current Graph v2 的 projection-heavy / authoring data-source nodes
  - 保留目前 Graph v2 sidecar / importer contract 細節，但避免被誤讀成 canonical 裁決
- 已修改 `Documentation/NarrativeGraphArchitecture.md`：
  - 在 Projection 與工作投影段落補上 `character / castBundle` 的總體定位
- 這一輪的實際效果是：
  - `character / castBundle` 不再看起來像「文件寫很多，所以已經是 canonical truth」
  - 後續若真的要升格，文件上已經先明確列出升格門檻

## 第二十九輪：`dialogue <-> legacy action` 的程式碼 mapping 已收斂
- 已修改 `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodes.cs`：
  - 新增明確常數：
    - `CanonicalNodeTypeDialogue = "dialogue"`
    - `LegacySidecarDialogueNodeType = "action"`
  - 保留 `NodeTypeAction` 作為 legacy alias，避免現有 code 與測試全面破裂
  - 新增集中 helper：
    - `IsDialogueNodeType(...)`
    - `GetCurrentProjectionNodeType(...)`
    - `ToLegacyActionKindToken(...)`
    - `ParseLegacyActionKindToken(...)`
- 已修改 `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`：
  - 不再自己硬編碼 `type == "action"` 來判斷對話節點
  - 改由 `InkFlowNodeSchema.IsDialogueNodeType(...)` 統一判斷
  - 匯出 sidecar type 時，改由 `InkFlowNodeSchema.GetCurrentProjectionNodeType(...)` 決定
  - `actionKind` 的 legacy token 也改由 schema helper 提供
- 已修改 `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`：
  - 匯入時改由 `InkFlowNodeSchema.IsDialogueNodeType(...)` 同時接受 canonical `dialogue` 與 legacy `action`
- 這一輪的效果是：
  - canonical / projection / legacy token 的 mapping 終於不再散落在 exporter / importer 多處
  - 若未來 sidecar 命名再收斂，主要只需要改 `InkFlowNodeSchema`

## 第三十輪：程式碼收斂驗證結果
- 已跑 Unity EditMode 測試：
  - 類別篩選：`GraphToolkitFlowSafe`
  - 結果：12 / 12 通過
- 驗證涵蓋：
  - `InkFlowChartImportTests`
  - `InkFlowChartRoundTripTests`
- 這代表這次 mapping 收斂沒有打壞目前最重要的 GraphToolkit 匯入 / 匯出 / round-trip 主線。

## 第三十一輪：根 README 入口整理已完成
- 已修改根 `README.md`，在最前面補上：
  - repo 現況定位
  - 建議閱讀順序
  - 目前最重要的幾件事
  - 與下方上游說明的關係
- 這次調整的重點不是重寫整份 README，而是：
  - 保留上游 `ink-unity-integration` 說明作為參考
  - 但先把新讀者導到真正的當前入口：
    - `Documentation/NarrativeGraphArchitecture.md`
    - `Documentation/CanonicalGraphSchema.md`
    - `Documentation/CanonicalGraphSchemaSpec.md`
    - `Documentation/DeveloperModeOutputContract.md`
    - `Packages/com.opsidanos.ink/README.md`
- 這一輪的效果是：
  - 新讀者不會再只看上游段落就把整個 repo 誤判成單純的 Ink Unity Integration 倉庫
  - README 與目前文件分層、Runtime 主線、GraphToolkit 主線的說法已基本對齊

## 第三十二輪：README 入口整理後的下一步判斷
- README 現在已適合扮演：
  - repo 現況公告
  - 建議閱讀順序
  - 文件入口索引
- 若再把更多解釋硬塞進 README，容易又回到「入口文件過重」的問題。
- 因此若要再往下改善 onboarding，較合理的下一步是：
  - 新增一份短版專案導讀文件
  - README 只負責把人導過去
