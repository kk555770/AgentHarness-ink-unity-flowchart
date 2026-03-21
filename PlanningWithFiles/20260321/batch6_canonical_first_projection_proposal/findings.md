# Findings

## 2026/03/21

- 目前工作樹**不是乾淨的**，而是停在 Batch 5 未提交狀態：
  - `InkFlowChartExporter.cs`
  - `InkFlowChartImporter.cs`
  - `CurrentFlowCanonicalGraphAdapter.cs`
  - `CurrentFlowCanonicalGraphAdapterTests.cs`
  - `PlanningWithFiles/20260321/`
  - 以及 3 份 XML 測試報表暫存

- `CurrentFlowProjectionService` 目前核心入口仍是：
  - `BuildInkContent(ExportGraphDto exportDto)`
  - `BuildDialogueActionMap(ExportGraphDto exportDto)`
  這代表它還是直接吃 current projection DTO，不是 canonical graph。

- `InkFlowChartExporter.cs` 目前資料流是：
  - GraphToolkit graph
  - `BuildExportDto`
  - `CurrentFlowCanonicalGraphAdapter.TryBuildCanonicalGraph`
  - `CurrentFlowCanonicalGraphAdapter.TryBuildProjection`
  - `CurrentFlowProjectionService.BuildInkContent(exportDto)`
  所以 exporter 雖然已經經過 canonical bridge，
  但最後真正組 `.ink` 時，還是回到 DTO-first。

- `GraphToolkitSpec.md` 對這批工作的限制仍然成立：
  - Graph Toolkit 只能當 Editor authoring framework
  - 不應把 runtime / execution backend 真相放回 Graph Toolkit
  - 這更支持 projection service 往 canonical-first 搬，而不是回頭強化 shell

## 子代理狀態

- 本輪有平行派出兩個讀碼子代理：
  - `ProjectionService` 接點盤點
  - `Exporter` DTO-first 耦合盤點
- 兩者都在合理等待時間內**沒有回傳可用摘要**
- 因此這輪提案**不採用子代理未回傳內容**
- 後續若需要再開新一輪子代理，應改用更小範圍或更短輸出格式

## 收斂判斷

- Batch 6 最值得做的是：
  - 讓 `CurrentFlowProjectionService` 增加 `CanonicalGraphDocument -> Ink` 能力
  - 讓 exporter 正式走 canonical-first projection
- Batch 6 先**不要碰 importer**
  - 因為 importer 仍牽涉 `CurrentFlowImportPlan`
  - 風險會比 projection 這條線高

## 實作後補充

- `CurrentFlowProjectionService` 已新增：
  - `TryBuildProjectionDto(CanonicalGraphDocument, out ExportGraphDto, out string)`
  - `TryBuildInkContent(CanonicalGraphDocument, out string, out string)`
- `InkFlowChartExporter.cs` 已改成：
  - 先把 GraphToolkit graph 組成 `ExportGraphDto`
  - 再建成 `CanonicalGraphDocument`
  - 再透過 canonical-first projection service 產出 normalized DTO 與 `.ink`
- 這表示 exporter 的 `.ink` 投影主線，已不再直接停在 DTO-first

## 驗證結論

- `CurrentFlowCanonicalProjectionServiceTests`
  - `3/3 passed`
- `Unity EditMode gate`
  - `283 total / 279 passed / 0 failed / 4 skipped`
  - `OpsidanosInk.EditModeTests.dll`
    - `65/65 passed`
- `Unity PlayMode gate`
  - `221 total / 130 passed / 0 failed / 91 skipped`
  - `OpsidanosInk.PlayModeTests.dll`
    - `17/17 passed`

## 剩餘觀察

- Batch 5 與 Batch 6 的 XML 測試報表暫存都還在工作樹
- `InkFlowChartImporter.cs` 仍停在 Batch 5 的最小 bridge 改接
- 下一批若繼續往前走，最合理的是：
  - 讓 importer 更進一步共享 canonical-first 路徑
  - 或開始規劃 Web-first bridge 的最小 transport / command surface
