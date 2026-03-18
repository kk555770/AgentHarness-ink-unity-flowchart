# 調查發現

## 需求
- 使用者已同意繼續往下做。
- 本輪目標是開始 Batch 2，先切 `validator / projection seam`。

## 研究發現
- `InkFlowChartExporter.cs` 現在同時負責：
  - 建立 current projection DTO
  - 驗證 current projection 合法性
  - 建立 `.ink`
  - 寫出 `.flowchart.json`
- `TryValidateGraphForExport(...)` 裡的大部分規則，其實已經是在驗 current projection DTO，而不是 GraphToolkit UI 本身。
- `TryValidateActionContentForExport(...)` 完全只吃 `ExportNodeDto` 與字串內容，適合第一批直接抽走。
- `InkFlowChartExportModels.cs` 目前還放在 GraphToolkit Editor 目錄下，容易讓人誤會它是 GraphToolkit 私有模型，但實際上：
  - `Exporter` 用它
  - `Importer` 用它
  - `RoundTrip / Import / Export` 測試也都直接建它
- 代表這組 DTO 更像 current projection shared contract，不像純 editor shell 私有資料。
- Batch 2 第一刀實作後，新的切法是：
  - `CurrentFlowProjectionModels.cs`：shared current projection DTO
  - `CurrentFlowProjectionValidator.cs`：純 DTO 驗證規則
  - `InkFlowChartExporter.cs`：保留 GraphToolkit graph traversal 與 port 拓樸驗證
  - `InkFlowChartImporter.cs`：在建圖前先共用 shared validator
- 第一輪 gate 失敗的根因不是方向錯，而是 shared validator 把線性節點的空 `toPortName` 判成非法；但既有 importer contract 本來就把空值視為 `Flow`。修正後全部 gate 恢復綠燈。
- `BuildInkContent(...)`、`BuildDialogueActionMap(...)` 與註解分行邏輯只吃 current projection DTO，不依賴 GraphToolkit 型別；這代表 Batch 2 第二刀可以繼續把它們抽成 shared `CurrentFlowProjectionService`。
- 第二刀的風險點不是 graph traversal，而是抽走後有沒有漏掉原本 exporter 的排序、空內容註解與 choice / condition 語法細節；所以 gate 一定要重跑完整 EditMode，而不是只跑新單測。
- 第二刀完成後，`InkFlowChartExporter.cs` 已不再自己保管 `BuildInkContent(...)`、`BuildDialogueActionMap(...)` 與 comment 分行 helper；它現在比較像：
  - GraphToolkit graph traversal
  - current projection DTO 建立
  - GraphToolkit 專屬 port 拓樸驗證
  - 檔案寫出入口
- 第二輪完整 gate 顯示 `CurrentFlowProjectionServiceTests` `3/3 passed`，而且既有 `Export / Import / RoundTrip` 也都維持綠燈，表示 shared projection service 的抽離沒有改壞 current workflow。
- 文件裡雖然把 `CurrentFlowImportService` 寫在 core 路徑，但如果它直接依賴 `INode / Node / INodeOption / UnityEditor`，就會把 core 又拉回 GraphToolkit editor 耦合。
- 所以下一刀更合理的切法是：
  - `CurrentFlowImportService`：只吃 `ExportGraphDto`，產出 normalized import plan
  - `CurrentFlowGraphToolkitImportAdapter`：把 import plan 套到 GraphToolkit node / option / wire
- Phase 5 完成後，`InkFlowChartImporter.cs` 已不再自己保存：
  - choice / condition 文本整理
  - dialogue 動作輸入埠數量推導
  - outputs / nextIds -> wire fallback 正規化
  - GraphToolkit node / option / wire rebuild 細節
- 這些責任現在改成：
  - `CurrentFlowImportService.cs`：純 DTO -> import plan
  - `CurrentFlowGraphToolkitImportAdapter.cs`：import plan -> GraphToolkit graph
- 完整 gate 顯示 `CurrentFlowImportServiceTests` `3/3 passed`，而且 `Import / RoundTrip / Export` 全部維持綠燈，代表 importer 的骨頭已經比原本乾淨很多，但 current workflow 沒壞。

## 技術判斷
| Decision | Rationale |
|----------|-----------|
| 先把 DTO 往 shared seam 挪 | validator 與 importer/exporter 才能站在同一份 current projection contract 上 |
| 先抽 DTO-based validator，不先抽 graph traversal | 這樣第一刀風險最低，也最容易用既有測試守回歸 |
| 先保留 `Importer` 的 Graph rebuild 與 option writeback 在 Editor 層 | 這段仍直接碰 GraphToolkit 型別，不適合現在硬搬 |

## 遇到的問題
| Issue | Resolution |
|-------|------------|
| shared validator 第一輪把線性 Flow 的空 `toPortName` 視為非法 | 改成「空 `toPortName` 視為 `Flow`」，與 importer 既有 fallback 對齊 |

## 參考資源
- `Documentation/AuthoringPhase1ImplementationProposal.md`
- `Documentation/AuthoringPhase1ImplementationPlan.md`
- `Documentation/AuthoringRefactorBoundaries.md`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExportModels.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`
