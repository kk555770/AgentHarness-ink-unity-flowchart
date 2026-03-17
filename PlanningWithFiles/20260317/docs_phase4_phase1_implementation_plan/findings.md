# 調查發現

## 需求
- 使用者已同意繼續下一輪文件翻修。
- 本輪持續只改文件，不改正式腳本。
- 目標是把前面幾份策略 / 邊界文件，收斂成第一階段可執行的實作計畫。

## 研究發現
- `Documentation/AuthoringToolStrategy.md` 已有高層級的「第一階段建議」，但還偏策略層，不是工程清單。
- `Documentation/AuthoringRefactorBoundaries.md` 已清楚指出第一刀、第二刀、第三刀，但還沒落成「先改哪些檔、怎麼驗證」。
- repo 已有明確的 Graph 匯入匯出與 round-trip 測試：
  - `Assets/Editor/Tests/InkFlowChartGraphSmokeTests.cs`
  - `Assets/Editor/Tests/InkFlowChartImportTests.cs`
  - `Assets/Editor/Tests/InkFlowChartExportTests.cs`
  - `Assets/Editor/Tests/InkFlowChartRoundTripTests.cs`
- repo 也已有 Runtime / UI / PlayMode 驗證線：
  - `Assets/Tests/PlayMode/OpsidanosInkPlayModeTests.cs`
  - `Assets/Tests/PlayMode/OpsidanosInkPlayModeUiClickTests.cs`
- 最合理的新文件角色是「implementation plan」，而不是再新增一份 strategy 或 architecture。
- 第一階段最需要被寫死的不是 UI 選型，而是：
  - 先動哪些 current GraphToolkit 檔
  - 先不動哪些 Runtime 檔
  - 驗證必須直接對應現有測試護欄

## 技術判斷
| Decision | Rationale |
|----------|-----------|
| 新增 implementation plan 文件 | 避免 strategy 與 refactor boundary 被拿來硬扛工程清單 |
| 驗證段直接引用現有測試檔 | 讓計畫和 repo 目前的護欄接上，而不是空泛的「未來要測」 |
| 把第一階段聚焦在 `InkFlowChartNodes.cs`、exporter/importer、Graph shell | 這最符合前一輪邊界文件的第一刀、第二刀、第三刀 |
| Runtime 與樣式殼列入暫不動清單 | 降低一次打開太多閉環的風險 |

## 遇到的問題
| Issue | Resolution |
|-------|------------|
| 第一階段說法分散在不同文件 | 以 implementation plan 集中收斂，其他文件只保留連結與角色說明 |
| 容易把「先做 WebView」誤認成第一階段 | 在新文件開頭明確寫死第一階段先切 core seam，不先換前端殼 |

## 參考資源
- `Documentation/AuthoringToolStrategy.md`
- `Documentation/AuthoringRefactorBoundaries.md`
- `Documentation/CurrentAuthoringWorkflow.md`
- `Documentation/DocsIndex.md`
- `Assets/Editor/Tests/InkFlowChartGraphSmokeTests.cs`
- `Assets/Editor/Tests/InkFlowChartImportTests.cs`
- `Assets/Editor/Tests/InkFlowChartExportTests.cs`
- `Assets/Editor/Tests/InkFlowChartRoundTripTests.cs`
- `Assets/Tests/PlayMode/OpsidanosInkPlayModeTests.cs`
- `Assets/Tests/PlayMode/OpsidanosInkPlayModeUiClickTests.cs`

## 視覺/瀏覽重點
- 這輪不是新增新方向，而是把已經講過的方向變成可執行順序。
- 這輪的核心交付是：讓讀者知道第一階段應先換骨頭，不是先換外殼。
