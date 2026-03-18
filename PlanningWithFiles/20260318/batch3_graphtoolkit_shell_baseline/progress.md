# 進度紀錄

## Session：2026-03-18

### Phase 1：確認 Batch 3 最小切點
- **Status:** complete
- **Started:** 2026-03-18 Asia/Taipei
- Actions taken:
  - 建立 Batch 3 的 `PlanningWithFiles` 記錄
  - 啟動平行分析：一條讀 Batch 3 文件，一條盤點 GraphToolkit shell 現況
  - 本地盤點與文件結果對齊，確認 Batch 3 第一刀先薄化 `InkFlowChartGraph.cs`
- Files created/modified:
  - `PlanningWithFiles/20260318/batch3_graphtoolkit_shell_baseline/task_plan.md`（created）
  - `PlanningWithFiles/20260318/batch3_graphtoolkit_shell_baseline/findings.md`（created）
  - `PlanningWithFiles/20260318/batch3_graphtoolkit_shell_baseline/progress.md`（created）

### Phase 2：實作 Batch 3 第一刀
- **Status:** complete
- Actions taken:
  - 新增 `InkFlowChartEditorCommands.cs`，接手 `MenuItem` 與資產路徑 helper
  - 新增 `InkFlowChartGraphShellValidator.cs`，接手 shell 層 start node 驗證橋接
  - 將 `InkFlowChartGraph.cs` 瘦回圖資產殼，只保留 `AssetExtension` 與最薄的 `OnGraphChanged` 入口
- Files created/modified:
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartEditorCommands.cs`（created）
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartGraphShellValidator.cs`（created）
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartGraph.cs`（updated）

### Phase 3：完整驗證與整理
- **Status:** complete
- Actions taken:
  - 跑完整 EditMode gate，確認 shell baseline 第一刀沒有打壞 GraphSmoke / Export / Import / RoundTrip
  - 補跑 PlayMode gate，確認 shell baseline 第一刀沒有側面影響玩家閉環
  - restore `Assets/OffMeshLinkScene.unity` 測試噪音
  - 刪除 `Batch3ShellBaselineGateResults.xml` 暫存 XML
  - 刪除 `Batch3ShellBaselinePlayModeResults.xml` 暫存 XML
- Files created/modified:
  - `PlanningWithFiles/20260318/batch3_graphtoolkit_shell_baseline/task_plan.md`（updated）
  - `PlanningWithFiles/20260318/batch3_graphtoolkit_shell_baseline/findings.md`（updated）
  - `PlanningWithFiles/20260318/batch3_graphtoolkit_shell_baseline/progress.md`（updated）

## 測試結果
| Test | Input | Expected | Actual | Status |
|------|-------|----------|--------|--------|
| 完整 EditMode gate | `Batch3ShellBaselineGateResults.xml` | 薄化 `InkFlowChartGraph` 後，GraphToolkit baseline 仍能守住 GraphSmoke / Export / Import / RoundTrip | `261 passed / 0 failed / 4 skipped`，其中 `OpsidanosInk.EditModeTests.dll` 為 `47/47 passed` | ✓ |
| PlayMode gate | `Batch3ShellBaselinePlayModeResults.xml` | shell baseline 第一刀不影響玩家閉環 | `130 passed / 0 failed / 91 skipped`，其中 `OpsidanosInk.PlayModeTests.dll` 為 `17/17 passed` | ✓ |

## 錯誤紀錄
| Timestamp | Error | Attempt | Resolution |
|-----------|-------|---------|------------|

## 5 題重啟檢查
| Question | Answer |
|----------|--------|
| Where am I? | Batch 3 第一刀已完成，正在整理可提交結論 |
| Where am I going? | 視需要整理成 commit，或繼續往更細的 GraphToolkit baseline seam 前進 |
| What's the goal? | 讓 GraphToolkit 更像 baseline shell，而不是 authoring 真相中心 |
| What have I learned? | `InkFlowChartGraph.cs` 最適合先抽走 MenuItem 與 shell 驗證橋接，讓 Graph 類別回到資產殼 |
| What have I done? | 已完成 Batch 3 第一刀，抽出 editor commands 與 shell validator，並用完整 EditMode gate 驗證通過 |
