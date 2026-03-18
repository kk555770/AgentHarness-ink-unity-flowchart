# 進度紀錄

## Session：2026-03-18

### Phase 1：確認 Batch 2 第一刀範圍
- **Status:** complete
- **Started:** 2026-03-18 Asia/Taipei
- Actions taken:
  - 重讀 Batch 2 提案與邊界文件
  - 盤點 `InkFlowChartExporter.cs`、`InkFlowChartImporter.cs`、`InkFlowChartExportModels.cs`
  - 確認本輪先切 shared DTO 與 validator，不碰 import rebuild adapter
- Files created/modified:
  - `PlanningWithFiles/20260318/batch2_validator_projection_seam/task_plan.md`（created）
  - `PlanningWithFiles/20260318/batch2_validator_projection_seam/findings.md`（created）
  - `PlanningWithFiles/20260318/batch2_validator_projection_seam/progress.md`（created）

### Phase 2：抽出 shared DTO 與 validator
- **Status:** complete
- Actions taken:
  - 新增 `CurrentFlowProjectionModels.cs`
  - 新增 `CurrentFlowProjectionValidator.cs`
  - 刪除舊的 `InkFlowChartExportModels.cs`
  - 將 `InkFlowChartExporter.cs` 改成先呼叫 shared validator，再保留 GraphToolkit 專屬 port 拓樸驗證
  - 將 `InkFlowChartImporter.cs` 改成在建圖前先呼叫 shared validator
  - 更新 `InkFlowChartImportTests.cs` 與 `InkFlowChartRoundTripTests.cs` 以參考 shared DTO
  - 新增 `CurrentFlowProjectionValidatorTests.cs`
- Files created/modified:
  - `Packages/com.opsidanos.ink/Core/Scripts/Projection/CurrentFlow/CurrentFlowProjectionModels.cs`（created）
  - `Packages/com.opsidanos.ink/Core/Scripts/Projection/CurrentFlow/CurrentFlowProjectionValidator.cs`（created）
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExportModels.cs`（deleted）
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`（updated）
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`（updated）
  - `Assets/Editor/Tests/InkFlowChartImportTests.cs`（updated）
  - `Assets/Editor/Tests/InkFlowChartRoundTripTests.cs`（updated）
  - `Assets/Editor/Tests/CurrentFlowProjectionValidatorTests.cs`（created）

### Phase 3：補測試與驗證
- **Status:** complete
- Actions taken:
  - 第一次跑完整 EditMode gate，發現 v2 import / round-trip fixture 因線性 Flow 的空 `toPortName` 被 shared validator 擋下
  - 修正 validator，讓空 `toPortName` 視為 `Flow`
  - 第二次跑完整 EditMode gate，確認全部通過
  - restore `Assets/OffMeshLinkScene.unity` 測試噪音
  - 刪除 `Batch2ValidatorProjectionGateResults.xml` 暫存 XML
- Files created/modified:
  - `PlanningWithFiles/20260318/batch2_validator_projection_seam/task_plan.md`（updated）
  - `PlanningWithFiles/20260318/batch2_validator_projection_seam/findings.md`（updated）
  - `PlanningWithFiles/20260318/batch2_validator_projection_seam/progress.md`（updated）

## 測試結果
| Test | Input | Expected | Actual | Status |
|------|-------|----------|--------|--------|
| 完整 EditMode gate | `Batch2ValidatorProjectionGateResults.xml` | shared DTO / validator seam 不影響 export / import / round-trip 閉環 | `255 passed / 0 failed / 4 skipped`，其中 `OpsidanosInk.EditModeTests.dll` 為 `41/41 passed` | ✓ |
| 完整 EditMode gate | `Batch2ProjectionServiceGateResults.xml` | shared projection service seam 不影響 export / import / round-trip 閉環 | `258 passed / 0 failed / 4 skipped`，其中 `OpsidanosInk.EditModeTests.dll` 為 `44/44 passed` | ✓ |
| 完整 EditMode gate | `Batch2ImportPlanGateResults.xml` | shared import plan service + GraphToolkit import adapter seam 不影響 export / import / round-trip 閉環 | `261 passed / 0 failed / 4 skipped`，其中 `OpsidanosInk.EditModeTests.dll` 為 `47/47 passed` | ✓ |

### Phase 4：開始抽 current projection service
- **Status:** complete
- Actions taken:
  - 新增 `CurrentFlowProjectionService.cs`
  - 將 `InkFlowChartExporter.cs` 改成呼叫 shared `BuildInkContent(...)`
  - 新增 `CurrentFlowProjectionServiceTests.cs`
  - 跑完整 EditMode gate，確認第二刀沒有打壞既有 export / import / round-trip
  - restore `Assets/OffMeshLinkScene.unity` 測試噪音
  - 刪除 `Batch2ProjectionServiceGateResults.xml` 暫存 XML
- Files created/modified:
  - `Packages/com.opsidanos.ink/Core/Scripts/Projection/CurrentFlow/CurrentFlowProjectionService.cs`（created）
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`（updated）
  - `Assets/Editor/Tests/CurrentFlowProjectionServiceTests.cs`（created）

## 錯誤紀錄
| Timestamp | Error | Attempt | Resolution |
|-----------|-------|---------|------------|
| 2026-03-18 | shared validator 第一輪把線性 Flow 的空 `toPortName` 視為非法 | 1 | 改成「空 `toPortName` 視為 `Flow`」，第二輪完整 gate 全部通過 |

## 5 題重啟檢查
| Question | Answer |
|----------|--------|
| Where am I? | Batch 2 已完成到 import plan / adapter seam，正在整理可提交結論 |
| Where am I going? | 視需要提交 Batch 2，或繼續往更細的 GraphToolkit shell baseline seam 前進 |
| What's the goal? | 讓 exporter / importer 共用 current projection contract 與 validator，而不是各自握一套 sidecar 規則 |
| What have I learned? | shared validator 要和 importer 既有 contract 對齊，尤其是線性 Flow 空 `toPortName` 要視為 `Flow` |
| What have I done? | 已抽出 shared DTO / validator、DTO -> Ink projection service、DTO -> import plan service，以及 GraphToolkit import adapter，並用完整 EditMode gate 驗證通過 |

## 目前位置補充
- Batch 2 第一刀已完成。
- Batch 2 第二刀也已完成。
- 下一步是把 importer 的 pure DTO 邏輯再抽成 import plan service，讓 importer 更像檔案入口與 GraphToolkit adapter。

### Phase 5：開始抽 current projection import plan seam
- **Status:** complete
- Actions taken:
  - 新增 `CurrentFlowImportModels.cs`
  - 新增 `CurrentFlowImportService.cs`
  - 新增 `CurrentFlowGraphToolkitImportAdapter.cs`
  - 將 `InkFlowChartImporter.cs` 改接 `CurrentFlowImportService` 與 `CurrentFlowGraphToolkitImportAdapter`
  - 新增 `CurrentFlowImportServiceTests.cs`
  - 跑完整 EditMode gate，確認 importer 改接 import plan / adapter 後，既有 export / import / round-trip 仍維持綠燈
  - restore `Assets/OffMeshLinkScene.unity` 測試噪音
  - 刪除 `Batch2ImportPlanGateResults.xml` 暫存 XML
- Files created/modified:
  - `Packages/com.opsidanos.ink/Core/Scripts/Projection/CurrentFlow/CurrentFlowImportModels.cs`（created）
  - `Packages/com.opsidanos.ink/Core/Scripts/Projection/CurrentFlow/CurrentFlowImportService.cs`（created）
  - `Assets/Editor/FlowChart/GraphToolkit/CurrentFlowGraphToolkitImportAdapter.cs`（created）
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`（updated）
  - `Assets/Editor/Tests/CurrentFlowImportServiceTests.cs`（created）
