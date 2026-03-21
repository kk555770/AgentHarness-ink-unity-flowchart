# Progress

## 2026/03/21

### 已完成

- 重讀：
  - `~/.codex/AGENTS.md`
  - repo `AGENTS.md`
- 確認目前工作樹真相：
  - Batch 5 還未提交
  - XML 測試報表暫存仍在
- 重讀：
  - `GraphToolkitSpec.md`
- 盤點：
  - `CurrentFlowProjectionService.cs`
  - `InkFlowChartExporter.cs`
  - `CurrentFlowCanonicalGraphAdapter.cs`
  - `PlanningWithFiles/20260321/batch5_canonical_projection_bridge_proposal/*`

### 子代理執行狀況

- 已平行派出 2 個 explorer 子代理做讀碼摘要
- 兩者在本輪等待時間內沒有回傳可整合內容
- 已把這件事記錄為「不可當決策依據」

### 關鍵結論

- Batch 5 已有 bridge，但 projection 主線還沒有 canonical-first
- exporter 仍是：
  - `DTO -> canonical -> DTO -> Ink`
- 下一步最合理的是：
  - 把 `CurrentFlowProjectionService` 搬成 canonical-first
  - 讓 exporter 真的走 `canonical -> Ink`

### 下一步提案草案

- 修改 `CurrentFlowProjectionService.cs`
- 視需要小改 `CurrentFlowCanonicalGraphAdapter.cs`
- 修改 `InkFlowChartExporter.cs`
- 新增 `CurrentFlowCanonicalProjectionServiceTests.cs`

## 2026/03/21 實作進度

### 已完成

- 在 `CurrentFlowProjectionService.cs` 新增：
  - `TryBuildProjectionDto(CanonicalGraphDocument, out ExportGraphDto, out string)`
  - `TryBuildInkContent(CanonicalGraphDocument, out string, out string)`
- 在 `InkFlowChartExporter.cs` 改成：
  - 先走 `TryBuildCanonicalGraph`
  - 再走 `CurrentFlowProjectionService.TryBuildProjectionDto`
  - 最後走 `CurrentFlowProjectionService.TryBuildInkContent`
- 新增 `CurrentFlowCanonicalProjectionServiceTests.cs`

### 最小測試回報

- `CurrentFlowCanonicalProjectionServiceTests`
  - `3/3 passed`
  - 已確認：
    - `CanonicalGraphDocument -> ExportGraphDto`
    - `CanonicalGraphDocument -> .ink`
    - `choice / condition / stageAction / dialogue` 的關鍵 projection 不漂移

### 進行中

- 準備升級到完整 `Unity EditMode gate`

### 完整 EditMode 回報

- `Unity EditMode gate`
  - `283 total / 279 passed / 0 failed / 4 skipped`
  - `OpsidanosInk.EditModeTests.dll`
    - `65/65 passed`
  - `CurrentFlowCanonicalProjectionServiceTests`
    - `3/3 passed`
  - `CurrentFlowCanonicalGraphAdapterTests`
    - `4/4 passed`
  - `CurrentFlowProjectionServiceTests`
    - `3/3 passed`
  - `InkFlowChartExportTests`
    - `6/6 passed`
  - `InkFlowChartImportTests`
    - 無回歸
  - `InkFlowChartRoundTripTests`
    - 無回歸

### 目前狀態

- Batch 6 的 canonical-first projection service 與 exporter 改接已通過完整 EditMode gate
- `Assets/OffMeshLinkScene.unity` 因測試產生噪音，待 PlayMode 結束後一併還原
- 下一步：升級到完整 `Unity PlayMode gate`

### 完整 PlayMode 回報

- `Unity PlayMode gate`
  - `221 total / 130 passed / 0 failed / 91 skipped`
  - `OpsidanosInk.PlayModeTests.dll`
    - `17/17 passed`
  - `OpsidanosInkPlayModeTests`
    - `7/7 passed`
  - `OpsidanosInkPlayModeUiClickTests`
    - `10/10 passed`

### 待收尾

- 還原 `Assets/OffMeshLinkScene.unity` 的測試噪音
- 視需要決定是否保留或清掉：
  - `Batch6ProjectionServiceTests.xml`
  - `Batch6EditModeResults.xml`
  - `Batch6PlayModeResults.xml`
