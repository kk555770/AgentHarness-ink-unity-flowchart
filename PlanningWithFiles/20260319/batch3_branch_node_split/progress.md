# 進度紀錄

## Session：2026-03-19

### Phase 1：確認拆檔邊界
- **Status:** completed
- **Started:** 2026-03-19 Asia/Taipei
- Actions taken:
  - 建立 `batch3_branch_node_split` 的 `PlanningWithFiles` 記錄
  - 盤點 `InkFlowActionPayload / InkFlowChoiceMode / InkFlowChoiceNode / InkFlowConditionNode` 的引用點
  - 重讀 `GraphToolkitSpec.md`，確認這刀只做拆檔，不改 GTK 契約
  - 綜合本地盤點與子代理分析，確認這刀不需要動 `Exporter / Importer / Adapter / StyleBootstrap`
- Files created/modified:
  - `PlanningWithFiles/20260319/batch3_branch_node_split/task_plan.md`（created）
  - `PlanningWithFiles/20260319/batch3_branch_node_split/findings.md`（created）
  - `PlanningWithFiles/20260319/batch3_branch_node_split/progress.md`（created）

### Phase 2：落地拆檔
- **Status:** completed
- **Started:** 2026-03-19 Asia/Taipei
- Actions taken:
  - 新增 `InkFlowActionPayload.cs`，承接動作資料 typed wire payload
  - 新增 `InkFlowChartBranchNodes.cs`，承接 `InkFlowChoiceMode / InkFlowChoiceNode / InkFlowConditionNode`
  - 更新 `InkFlowChartNodes.cs`，讓它收斂回主線節點集合
- Files created/modified:
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowActionPayload.cs`（created）
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartBranchNodes.cs`（created）
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodes.cs`（modified）
  - `PlanningWithFiles/20260319/batch3_branch_node_split/task_plan.md`（modified）
  - `PlanningWithFiles/20260319/batch3_branch_node_split/findings.md`（modified）
  - `PlanningWithFiles/20260319/batch3_branch_node_split/progress.md`（modified）

### Phase 3：完整驗證與整理
- **Status:** completed
- **Started:** 2026-03-19 Asia/Taipei
- Actions taken:
  - 執行 Unity EditMode gate，取得 `265 total / 261 passed / 0 failed / 4 skipped`
  - 確認 `OpsidanosInk.EditModeTests.dll`、`InkFlowChartGraphSmokeTests`、`InkFlowChartExportTests`、`InkFlowChartImportTests`、`InkFlowChartRoundTripTests` 全數通過
  - 執行 Unity PlayMode gate，取得 `221 total / 130 passed / 0 failed / 91 skipped`
  - 確認 `OpsidanosInk.PlayModeTests.dll`、`OpsidanosInkPlayModeTests`、`OpsidanosInkPlayModeUiClickTests` 全數通過
  - 還原 `Assets/OffMeshLinkScene.unity` 測試噪音
  - 刪除 `Batch3BranchNodeGateResults.xml` 與 `Batch3BranchNodePlayModeResults.xml` 暫存輸出
- Files created/modified:
  - `PlanningWithFiles/20260319/batch3_branch_node_split/task_plan.md`（modified）
  - `PlanningWithFiles/20260319/batch3_branch_node_split/findings.md`（modified）
  - `PlanningWithFiles/20260319/batch3_branch_node_split/progress.md`（modified）

## 測試結果
| Test | Input | Expected | Actual | Status |
|------|-------|----------|--------|--------|
| Unity EditMode gate | branch node 拆檔後的完整 EditMode 測試 | 無回歸 | `265 total / 261 passed / 0 failed / 4 skipped` | Pass |
| `OpsidanosInk.EditModeTests.dll` | Editor shell / graph / projection 相關 EditMode tests | 全通過 | `47/47 passed` | Pass |
| `InkFlowChartGraphSmokeTests` | `.inkfc` 建立與載入 smoke | 全通過 | `1/1 passed` | Pass |
| `InkFlowChartExportTests` | 匯出整合回歸 | 全通過 | `6/6 passed` | Pass |
| `InkFlowChartImportTests` | 匯入整合回歸 | 全通過 | `9/9 passed` | Pass |
| `InkFlowChartRoundTripTests` | round-trip 回歸 | 全通過 | `3/3 passed` | Pass |
| Unity PlayMode gate | 玩家閉環 PlayMode 測試 | 無回歸 | `221 total / 130 passed / 0 failed / 91 skipped` | Pass |
| `OpsidanosInk.PlayModeTests.dll` | Runtime PlayMode tests | 全通過 | `17/17 passed` | Pass |
| `OpsidanosInkPlayModeTests` | 玩家模式流程 | 全通過 | `7/7 passed` | Pass |
| `OpsidanosInkPlayModeUiClickTests` | UI 點擊流程 | 全通過 | `10/10 passed` | Pass |

## 錯誤紀錄
| Timestamp | Error | Attempt | Resolution |
|-----------|-------|---------|------------|
| 2026-03-19 | 大 patch 沒有對準 `InkFlowChartNodes.cs` 上下文 | 先把新增檔與原檔刪減一起打一個 patch | 改成先讀行號，再分成兩個小 patch |
| 2026-03-19 | Unity 測試讓 `Assets/OffMeshLinkScene.unity` 出現物件 ID 噪音 | 先看 `git diff` 確認是否只是測試副作用 | 確認無語意差異後還原 |

## 5 題重啟檢查
| Question | Answer |
|----------|--------|
| Where am I? | 已完成 branch node 拆檔與完整 gate，正在整理可回報結果 |
| Where am I going? | 回報這刀成果，並判讀 Batch 3 下一個最值得切的 seam |
| What's the goal? | 讓 `InkFlowChartNodes.cs` 更接近主線節點集合，繼續薄化 `GraphToolkit shell` |
| What have I learned? | 這刀可以用純拆檔完成，並且既有 Editor / PlayMode gate 都能守住；剩餘候選 seam 會更集中在 style shell 或更細的 node 主題拆分 |
| What have I done? | 已建立新的 planning、完成引用盤點、拆出 `InkFlowActionPayload.cs` 與 `InkFlowChartBranchNodes.cs`，並跑完完整 gate |
