# Progress

## 2026/03/21

### 已完成

- 重讀：
  - `~/.codex/AGENTS.md`
  - repo `AGENTS.md`
- 確認目前工作樹乾淨
- 盤點：
  - `CanonicalGraphCommandService`
  - `CanonicalGraphApiSpec.md`
  - `AuthoringRefactorBoundaries.md`
  - `CurrentFlowProjectionService`
  - `InkFlowChartExporter`
  - `InkFlowChartImporter`
  - `CurrentFlowGraphToolkitImportAdapter`
  - `InkFlowChartExportTests`
  - `InkFlowChartRoundTripTests`

### 關鍵結論

- 現在的 shared bridge 還停在 `ExportGraphDto`
- canonical document 已存在，但 current working line 尚未經過它
- 下一步若要符合 Web-first / AI control plane 方向，
  最該先補的是 `canonical document <-> current projection` 的橋

### 下一步提案草案

- 新增 current projection 與 canonical document 的雙向 adapter
- 新增 adapter 純資料測試
- 第二步才讓 exporter / importer 漸進改接

## 2026/03/21 實作進度

### 已完成

- 新增 `CurrentFlowCanonicalGraphAdapter.cs`
- 新增 `CurrentFlowCanonicalGraphAdapterTests.cs`
- 將 `InkFlowChartExporter.cs` 最小改接：
  - `ExportGraphDto -> CanonicalGraphDocument`
  - `CanonicalGraphDocument -> ExportGraphDto`
- 將 `InkFlowChartImporter.cs` 最小改接：
  - sidecar 驗證後先經過 canonical bridge
  - 再回到 normalized `ExportGraphDto` 給既有 import plan 使用

### 目前狀態

- adapter 與 importer/exporter 改接已落地
- 正在跑第一輪最小 EditMode 測試：
  - `CurrentFlowCanonicalGraphAdapterTests`

### 測試回報

- `CurrentFlowCanonicalGraphAdapterTests`
  - `4/4 passed`
  - 已確認：
    - `ExportGraphDto -> CanonicalGraphDocument`
    - `CanonicalGraphDocument -> ExportGraphDto`
    - `choice / condition / stageAction / dialogue` 的關鍵 mapping
- 已升級到完整 `EditMode gate`

- `Unity EditMode gate`
  - `276 total / 276 passed / 0 failed / 4 skipped`
  - `OpsidanosInk.EditModeTests.dll`
    - `62/62 passed`
  - `CurrentFlowCanonicalGraphAdapterTests`
    - `4/4 passed`
  - `InkFlowChartExportTests`
    - 無回歸
  - `InkFlowChartImportTests`
    - 無回歸
  - `InkFlowChartRoundTripTests`
    - 無回歸

### 下一步

- 跑 `Unity PlayMode gate`
- 清理 `OffMeshLinkScene.unity` 與 XML 暫存

### 最終結果

- `Unity PlayMode gate`
  - `221 total / 130 passed / 0 failed / 91 skipped`
  - `OpsidanosInk.PlayModeTests.dll`
    - `17/17 passed`
  - `OpsidanosInkPlayModeTests`
    - `7/7 passed`
  - `OpsidanosInkPlayModeUiClickTests`
    - `10/10 passed`

- `OffMeshLinkScene.unity`
  - 已用 `git restore` 還原測試噪音

- 工作樹目前仍保留：
  - Batch 5 程式碼變更
  - `PlanningWithFiles/20260321/`
  - `Batch5AdapterTests.xml`
  - `Batch5EditModeResults.xml`
  - `Batch5PlayModeResults.xml`

### 收斂判斷

- Batch 5 已成功把 `canonical graph` 接到 current projection 工作流的一小段
- GraphToolkit baseline、import/export、round-trip、玩家模式都沒有回歸
