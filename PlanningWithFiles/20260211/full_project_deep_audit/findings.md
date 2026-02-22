# Findings：全專案深度調查（不使用 Unity MCP）

## F-001 初始化
- 已建立本輪調查專用計畫檔。
- 調查原則：只讀取、逐步記錄、不做程式或資產修改。

## F-002 Git 基線
- 分支：`arcumit/CodexInk`
- `HEAD`：`dc8a443`
- 工作樹未提交檔案有 2 個：
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/InkTagCharacterStatePlayer.cs`
  - `ProjectSettings/EditorBuildSettings.asset`
- 近期提交主軸（近 25 筆）顯示：Graph Toolkit 匯入/匯出與測試穩定性是最近主線。
- 風險：目前存在跨領域未提交變更（Runtime + ProjectSettings），後續判讀測試結果時必須區分是否受未提交檔案影響。

## F-003 專案結構盤點
- 檔案量概況：
  - `Assets`：284 檔
  - `Packages`：951 檔
  - `ProjectSettings`：29 檔
- `Assets` 主要區塊：
  - `Scene`、`Tests`、`Editor`、`OpsidanosInk`、`UI Toolkit`、`FlowCharts`、`AddressableAssetsData`
  - 額外場景：`Assets/OffMeshLinkScene.unity`
- `Packages` 主要區塊：
  - `Packages/com.opsidanos.ink`
  - `Packages/com.unity.ai.navigation`
  - `Packages/Ink`
- `Documentation` 目前有 3 份主要文件：
  - `Documentation/InkPlayerWindow.md`
  - `Documentation/Ink for Unity Package Documentation.pdf`
  - `Documentation/API Documentation.pdf`
- 初步判讀：專案同時包含對話系統與 Navigation 套件測試，後續調查必須區分「自有模組」與「第三方套件測試」。

## F-004 規格與約束矩陣（第一輪）
- `AGENTS.md` 核心契約：
  - 先提案、同意後才能改一般檔案。
  - `PlanningWithFiles` 三檔可直接改，但要求「調查一步、記錄一步」。
  - 禁止防禦性編碼，問題應用 `Debug.Log` 暴露。
  - 動到 UI/Graph 必須對照 `UIToolkitSpec.md`、`GraphToolkitSpec.md`。
- `UIToolkitSpec.md` 核心限制：
  - 僅允許 UI Toolkit 元件與支援的 USS 屬性。
  - 明確區分 Runtime 與 Editor-only 控件。
- `GraphToolkitSpec.md` 核心限制（目前讀到章節）：
  - Graph Toolkit 為 Editor-only（不得假設 runtime 執行）。
  - 需 Unity 6000.2+，目前專案使用 6000.3.2f1 相容。
  - Graph/Node/Port/Option 的宣告方式與唯一性有強制規則。
- `README.md` 屬上游 ink-unity-integration 通用說明，與本專案自訂 Runtime 規格並不等價。
- 初步風險：專案同時存在「上游通用說明」與「本地 AGENTS/Spec」，若引用錯來源會造成設計判斷偏移。

## F-005 套件與組件邊界
- `Packages/manifest.json` 顯示核心依賴同時包含：
  - 對話系統：`com.inkle.ink-unity-integration`（由 `com.opsidanos.ink` 轉依賴）
  - 視覺節點：`com.unity.graphtoolkit` `0.4.0-exp.2`
  - 導航測試：`com.unity.ai.navigation` `2.0.9`
- 自有組件（asmdef）邊界：
  - `OpsidanosInk.Runtime`：依賴 `Ink-Libraries`、Addressables。
  - `OpsidanosInk.FlowChartEditor`：Editor-only，依賴 Graph Toolkit 三個 Editor 組件。
  - `OpsidanosInk.EditModeTests`：同時覆蓋 Runtime 與 FlowChartEditor（含 Graph Toolkit 依賴）。
  - `OpsidanosInk.PlayModeTests`：僅依賴 `OpsidanosInk.Runtime`。
- 關鍵結論：
  - GraphToolkit 問題理應先落在 `OpsidanosInk.FlowChartEditor` / `EditModeTests` 線。
  - 對話播放問題理應先落在 `OpsidanosInk.Runtime` / `PlayModeTests` 線。
  - 兩線混判會直接導致範圍偏移。

## F-006 新增需求：上一個提交誤解風險稽核
- 使用者新增要求：必須調查上一個 commit（`dc8a443`）是否在誤解情況下做出錯誤修改。
- `dc8a443` 提交摘要：
  - 訊息：`feat: 收斂 Graph Toolkit 匯入 MVP 與測試穩定性`
  - 檔案數：9 檔（770 insertions / 8 deletions）
  - 變更範圍：
    - `Assets/Editor/FlowChart/GraphToolkit/*`（Exporter/Graph/Importer）
    - `Assets/Editor/Tests/*`（Export/Smoke/Import tests）
    - `.gitignore`
- 初步判讀：此提交範圍集中在 GraphToolkit Editor 線，尚未看到 Runtime 對話系統檔案變更。
- 下一步：逐檔審查 `dc8a443` 的修改內容與對應測試意圖，判定是否存在需求誤解。

## F-007 `dc8a443` 逐檔稽核（事實層）
- `InkFlowChartGraph.cs`：
  - 新增選單 `建立新圖 / 匯出選中圖 / 匯入選中匯出檔`。
  - 建圖 API 使用 `GraphDatabase.CreateGraph` 直接建立 `.inkfc`。
  - 新增選取資產副檔名判斷共用函式。
- `InkFlowChartExporter.cs`：
  - `LoadGraphForImporter` 失敗時 fallback `LoadGraph`。
- `InkFlowChartImporter.cs`（新增）：
  - 提供 `.flowchart.json + .ink -> .inkfc` 匯入。
  - 使用暫存圖路徑建立後 `MoveAsset` 到正式路徑。
  - 透過反射拿 `Graph.m_Implementation` 與 `CreateNodeModel/CreateWire`。
  - 建圖時以 `Debug.unityLogger.logEnabled=false` 包住 `CreateGraph`。
- `InkFlowChartExportTests.cs`：
  - fixture 圖由 `CopyAsset` 改成「每次建圖 + 寫節點」。
  - 測試內也有 `CreateGraphWithMutedLogger`（關閉 logger）。
- `InkFlowChartGraphSmokeTests.cs`：
  - 建圖流程改為短暫關閉 logger。
- `InkFlowChartImportTests.cs`（新增）：
  - 新增 3 組匯入測試（成功、缺 startNodeId、缺 .ink）。
  - 清理策略改為 `UnityTearDown` + 反射檢查 InkCompiler 是否仍在編譯。
- `.gitignore`：
  - 新增 GraphToolkit 測試暫存忽略路徑。

## F-008 程式模組地圖（第一輪）
- 自有 Runtime 主線（`Packages/com.opsidanos.ink/Runtime/Scripts`）共 16 個 `.cs`：
  - `Story`：`InkStoryEngine`、`InkTagParser`、`InkTagEventRouter`、`StoryOutput`
  - `UI`：`VNPlayerPresenter`、`IAdvanceBlocker`
  - `Presentation`：背景/CG/音效/角色/震動播放器 + `InkResourceMap`
  - `Save`：`InkSaveData`、`InkSaveSystem`
- 自有 Editor 主線（`Assets/Editor`）共 19 個 `.cs`：
  - GraphToolkit：`InkFlowChartGraph`、`InkFlowChartExporter`、`InkFlowChartImporter`、`InkFlowChartNodes`
  - 測試：`InkFlowChart*Tests`、`InkResourceMap*Tests`、`InkSaveDataTests`、`CharTagJsonTests`
- 自有 PlayMode 測試檔：2 個（`OpsidanosInkPlayModeTests.cs`、`OpsidanosInkPlayModeUiClickTests.cs`）。
- 外部套件 `com.unity.ai.navigation` 測試檔：38 個（會影響整組測試時間與噪音）。
- 影響面重點：
  - 對話系統（Runtime）與 GraphToolkit（Editor）已在 asmdef 層切開。
  - 測試執行若跑全組，會混入外部套件測試，容易遮蔽自有模組問題。

## F-009 流程偏差（使用者即時指正）
- 問題：本輪有「一次彙整多個調查結果才寫入」的行為，未完全符合「調查一步、記錄一步」。
- 立即修正規則（自本條起生效）：
  1. 每執行 1 次調查動作（一次指令或一次檔案讀取），立刻追加 1 筆 `progress.md`。
  2. 每產生 1 個新結論，立刻追加 1 筆 `findings.md`。
  3. 不再等到階段結束才補記錄。

## F-010 場景腳本 GUID 盤點（中間結果）
- `Assets/Scene/Test.unity` 目前包含 27 個唯一 GUID 參考。
- 其中 `000...e000`、`000...f000` 為 Unity 內建程序集 GUID；其餘需對照專案 `.meta` 才能還原元件來源。
