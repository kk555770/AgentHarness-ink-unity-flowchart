# 進度紀錄

## Session：2026-03-18

### Phase 1：確認切分邊界
- **Status:** complete
- **Started:** 2026-03-18 Asia/Taipei
- Actions taken:
  - 重讀 Batch 1 提案
  - 檢查 `InkFlowChartNodes.cs` 現況
  - 檢查 `InkFlowChartExporter.cs` 與 `InkFlowChartImporter.cs` 的呼叫點
  - 確認 `InkFlowNodeSchema` 是第一刀最值得切的 seam
- Files created/modified:
  - `PlanningWithFiles/20260318/batch1_nodes_seam/task_plan.md`（created）
  - `PlanningWithFiles/20260318/batch1_nodes_seam/findings.md`（created）
  - `PlanningWithFiles/20260318/batch1_nodes_seam/progress.md`（created）

### Phase 2：建立新 seam 型別
- **Status:** complete
- Actions taken:
  - 更新 `OpsidanosInk.FlowChartEditor.asmdef`，正式參考 `OpsidanosInk.CanonicalGraph`
  - 新增 `CanonicalNodeDisplayNames.cs`
  - 新增 `CurrentFlowProjectionNaming.cs`
  - 新增 `InkFlowCurrentProjectionAdapter.cs`
- Files created/modified:
  - `Assets/Editor/FlowChart/OpsidanosInk.FlowChartEditor.asmdef`（updated）
  - `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CanonicalNodeDisplayNames.cs`（created）
  - `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CurrentFlowProjectionNaming.cs`（created）
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowCurrentProjectionAdapter.cs`（created）

### Phase 3：改接現有 GraphToolkit 腳本
- **Status:** complete
- Actions taken:
  - 將 `InkFlowChartNodes.cs` 內的 node/port naming 與 fallback label 改接到 core seam
  - 收掉舊的 `InkFlowActionKind` 與 legacy action token helper
  - 將 `InkFlowChartExporter.cs` 改接 `CanonicalNodeKinds`、`CanonicalPortSemantics`、`CurrentFlowProjectionNaming`
  - 將 `InkFlowChartImporter.cs` 改接同一套 node/port naming seam
- Files created/modified:
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodes.cs`（updated）
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`（updated）
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`（updated）

### Phase 4：補測試與驗證
- **Status:** complete
- Actions taken:
  - 新增 `CurrentFlowProjectionNamingTests.cs`
  - 新增 `CanonicalNodeDisplayNamesTests.cs`
  - 先跑同步 gate，確認 core naming / smoke / export 都過
  - 再跑非同步 gate，正式驗證 import / round-trip 的 `UnityTest`
- Files created/modified:
  - `Assets/Editor/Tests/CurrentFlowProjectionNamingTests.cs`（created）
  - `Assets/Editor/Tests/CanonicalNodeDisplayNamesTests.cs`（created）
  - `PlanningWithFiles/20260318/batch1_nodes_seam/task_plan.md`（updated）
  - `PlanningWithFiles/20260318/batch1_nodes_seam/findings.md`（updated）
  - `PlanningWithFiles/20260318/batch1_nodes_seam/progress.md`（updated）

## 測試結果
| Test | Input | Expected | Actual | Status |
|------|-------|----------|--------|--------|
| Batch 1 同步 gate | `Batch1GateResults.xml` | core naming + smoke + export 通過 | `20/20 passed` | ✓ |
| Batch 1 非同步 gate | `Batch1AsyncGateResults.xml` | import + round-trip 通過 | `12/12 passed` | ✓ |

## 錯誤紀錄
| Timestamp | Error | Attempt | Resolution |
|-----------|-------|---------|------------|
| 2026-03-18 | `runSynchronously` 那輪沒有真的代表 `UnityTest` 已執行 | 1 | 補跑不帶 `-runSynchronously` 的 async gate，正式確認 import / round-trip 12/12 passed |

## 5 題重啟檢查
| Question | Answer |
|----------|--------|
| Where am I? | Phase 4，已完成 Batch 1 第一刀並驗完同步/非同步 gate |
| Where am I going? | 準備整理可交付結論，或評估是否進下一刀 |
| What's the goal? | 讓節點種類與埠語意不再整塊綁在 GraphToolkit 檔案裡 |
| What have I learned? | `runSynchronously` 適合先守同步護欄，但 `UnityTest` 類的 round-trip 需要另外補非同步 gate |
| What have I done? | 已切出 naming seam、改接 GraphToolkit 腳本、補測試，並用兩輪 gate 驗證全部通過 |
