# 進度紀錄

## Session：2026-03-18

### Phase 1：確認剩餘耦合邊界
- **Status:** complete
- **Started:** 2026-03-18 Asia/Taipei
- Actions taken:
  - 重讀 Batch 1 第一刀的 planning 記錄
  - 重新盤點 `InkFlowNodeSchema` 的殘留使用點
  - 確認這一刀鎖定 Editor option schema，不碰 core semantics
- Files created/modified:
  - `PlanningWithFiles/20260318/batch1_option_schema_seam/task_plan.md`（created）
  - `PlanningWithFiles/20260318/batch1_option_schema_seam/findings.md`（created）
  - `PlanningWithFiles/20260318/batch1_option_schema_seam/progress.md`（created）

### Phase 2：切出 Editor-only option schema
- **Status:** complete
- Actions taken:
  - 新增 `InkFlowNodeOptionSchema.cs`
  - 將 `InkFlowChartNodes.cs` 內的 option schema 類別移出
  - 將 `InkFlowChartExporter.cs` 與 `InkFlowChartImporter.cs` 改接 `InkFlowNodeOptionSchema`
  - 用 `rg` 確認程式碼內已無 `InkFlowNodeSchema` 殘留
- Files created/modified:
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowNodeOptionSchema.cs`（created）
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodes.cs`（updated）
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`（updated）
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`（updated）

### Phase 3：補驗證與收斂
- **Status:** complete
- Actions taken:
  - 跑完整 EditMode gate，輸出 `Batch1OptionSchemaGateResults.xml`
  - 確認 `OpsidanosInk.EditModeTests.dll` 為 `37/37 passed`
  - 確認 `InkFlowChartImportTests` 為 `9/9 passed`
  - 確認 `InkFlowChartRoundTripTests` 為 `3/3 passed`
  - 發現完整 EditMode 會留下 `Assets/OffMeshLinkScene.unity` 的 scene fileID 噪音，已 restore
  - 清掉 `Batch1OptionSchemaGateResults.xml` 暫存 XML
- Files created/modified:
  - `PlanningWithFiles/20260318/batch1_option_schema_seam/task_plan.md`（updated）
  - `PlanningWithFiles/20260318/batch1_option_schema_seam/findings.md`（updated）
  - `PlanningWithFiles/20260318/batch1_option_schema_seam/progress.md`（updated）

## 測試結果
| Test | Input | Expected | Actual | Status |
|------|-------|----------|--------|--------|
| 完整 EditMode gate | `Batch1OptionSchemaGateResults.xml` | option schema seam 不影響既有 GraphToolkit 與 core 測試 | `251 passed / 0 failed / 4 skipped`，其中 `OpsidanosInk.EditModeTests.dll` 為 `37/37 passed` | ✓ |

## 錯誤紀錄
| Timestamp | Error | Attempt | Resolution |
|-----------|-------|---------|------------|
| 2026-03-18 | 完整 EditMode gate 會改動 `Assets/OffMeshLinkScene.unity` | 1 | 確認只是外部套件測試造成的 scene fileID 重排，完成驗證後 restore 該檔 |

## 5 題重啟檢查
| Question | Answer |
|----------|--------|
| Where am I? | 本輪 option schema seam 已完成，正在整理可提交結論 |
| Where am I going? | 視需要整理 commit 或繼續下一刀 |
| What's the goal? | 讓 `InkFlowChartNodes.cs` 再薄一層，只留下節點本體與局部 helper |
| What have I learned? | `InkFlowNodeSchema` 的剩餘責任其實已經可以完整收束成單獨的 Editor option schema 檔 |
| What have I done? | 已建立 planning 檔、切出 `InkFlowNodeOptionSchema.cs`、完成三個 GraphToolkit 檔案改接，並用完整 EditMode gate 驗證通過 |
