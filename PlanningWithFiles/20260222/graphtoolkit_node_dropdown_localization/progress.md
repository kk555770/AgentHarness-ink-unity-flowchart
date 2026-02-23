# Progress Log

## Session: 2026-02-22

### Phase 1: 規範對齊與現況確認
- **Status:** complete
- **Started:** 2026-02-22
- Actions taken:
  - 盤點 GraphToolkit 節點、匯出、匯入、契約與測試檔。
  - 確認目前可下拉欄位僅 `ChoiceMode`，`action` 仍是手動文字。
  - 取得使用者同意後開始實作。
- Files created/modified:
  - `PlanningWithFiles/20260222/graphtoolkit_node_dropdown_localization/task_plan.md`
  - `PlanningWithFiles/20260222/graphtoolkit_node_dropdown_localization/findings.md`
  - `PlanningWithFiles/20260222/graphtoolkit_node_dropdown_localization/progress.md`

### Phase 2: 節點與欄位設計
- **Status:** complete
- Actions taken:
  - 新增 `InkFlowNodeSchema` 集中管理節點標題、type、option key。
  - 新增 `InkFlowActionKind`（`Dialogue` / `StageAction` / `Custom`）。
  - `action` 節點新增 `ActionKind` 下拉欄位。
  - 節點標題改為由 schema 控制（開始/對話/註解/選項/條件）。
- Files created/modified:
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodes.cs`

### Phase 3: 匯出匯入與 sidecar 同步
- **Status:** complete
- Actions taken:
  - `ExportNodeDto` 新增 `actionKind`。
  - 匯出器新增 `GetNodeActionKind`，寫出 `dialogue/action/custom`。
  - 匯入器讀取 `actionKind` 並還原到 `ActionKind` option。
  - 缺少 `actionKind` 時預設 `Dialogue`，保持舊 sidecar 相容。
- Files created/modified:
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExportModels.cs`
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`

### Phase 4: 測試與文件更新
- **Status:** complete
- Actions taken:
  - Import 測試新增 ActionKind 斷言與 fixture `actionKind`。
  - RoundTrip 測試新增 `actionKind` 保留驗證。
  - 契約文件新增「編輯器白話名稱與下拉欄位」與 `actionKind` 規範。
- Files created/modified:
  - `Assets/Editor/Tests/InkFlowChartImportTests.cs`
  - `Assets/Editor/Tests/InkFlowChartRoundTripTests.cs`
  - `Documentation/DeveloperModeOutputContract.md`

### Phase 5: 驗證與交付
- **Status:** complete（環境阻擋已明確記錄）
- Actions taken:
  - 嘗試用 MCP 執行 EditMode 測試（回傳 `no_unity_session`）。
  - 改用 Unity CLI 執行目標測試。
  - Unity 回報專案已被另一個 Unity 執行個體開啟，無法 batch 測試。
  - 執行 `git diff --check`，確認無 whitespace/error patch 問題。
- Files created/modified:
  - 無新增程式檔案（僅驗證）

## Session: 2026-02-23（測試防暴走修正）

### Phase 6: 測試防暴走流程與場景還原
- **Status:** complete
- **Started:** 2026-02-23
- Actions taken:
  - 盤點暴走根因：`run_tests(mode=EditMode)` 無篩選會拉進 package 測試。
  - 確認 package 測試確實會切換/建立場景（`com.unity.ai.navigation`）。
  - 確認目前唯一被動到的非目標場景是 `Assets/OffMeshLinkScene.unity`。
  - `OpsidanosInkTestRunnerMenu` 新增 GraphToolkit 安全入口（assembly + category + fixture 固定篩選）。
  - `InkFlowChartImportTests`、`InkFlowChartRoundTripTests` 新增 `[Category("GraphToolkitFlowSafe")]` + `[Timeout(60000)]`。
  - `DeveloperModeOutputContract.md` 新增 7.4「GraphToolkit 測試防暴走流程」。
  - 已還原 `Assets/OffMeshLinkScene.unity`。
  - 使用者回報「已恢復」後，MCP 連線恢復，已完成固定範圍測試驗證。
- Files created/modified:
  - `PlanningWithFiles/20260222/graphtoolkit_node_dropdown_localization/task_plan.md`
  - `PlanningWithFiles/20260222/graphtoolkit_node_dropdown_localization/findings.md`
  - `PlanningWithFiles/20260222/graphtoolkit_node_dropdown_localization/progress.md`
  - `Assets/Editor/OpsidanosInkTestRunnerMenu.cs`
  - `Assets/Editor/Tests/InkFlowChartImportTests.cs`
  - `Assets/Editor/Tests/InkFlowChartRoundTripTests.cs`
  - `Documentation/DeveloperModeOutputContract.md`

## Test Results
| Test | Input | Expected | Actual | Status |
|------|-------|----------|--------|--------|
| MCP EditMode 測試 | `run_tests(EditMode, ImportTests+RoundTripTests)` | 取得測試 job | 回傳 `no_unity_session` | blocked |
| Unity CLI Import 測試 | `Unity -runTests -testFilter InkFlowChartImportTests` | 執行 EditMode 測試 | 專案被另一 Unity instance 開啟，批次中止 | blocked |
| MCP 連線檢查 | `debug_request_context` | 可取得 session/context | `Transport send error: http://localhost:8080/mcp` | blocked |
| Diff 檢查 | `git diff --check` | 無 patch 格式問題 | 通過 | pass |
| GraphToolkit Import（安全篩選） | `assembly=OpsidanosInk.EditModeTests` + `category=GraphToolkitFlowSafe` + `group=InkFlowChartImportTests` | 只跑 Import fixture | 8/8 Passed | pass |
| GraphToolkit RoundTrip（安全篩選） | `assembly=OpsidanosInk.EditModeTests` + `category=GraphToolkitFlowSafe` + `group=InkFlowChartRoundTripTests` | 只跑 RoundTrip fixture | 2/2 Passed | pass |
| Console Error 檢查 | `read_console types=[\"error\"]` | 0 筆 | 0 筆 | pass |

## Error Log
| Timestamp | Error | Attempt | Resolution |
|-----------|-------|---------|------------|
| 2026-02-22 | Unity MCP: `no_unity_session` | 1 | 改走 Unity CLI |
| 2026-02-22 | Unity CLI: project already open | 1 | 停止重試，記錄並回報 |
| 2026-02-23 | MCP transport error (`localhost:8080/mcp`) | 1 | 先完成流程守門改檔，待 MCP 恢復後重跑固定範圍測試 |

## 5-Question Reboot Check
| Question | Answer |
|----------|--------|
| Where am I? | Phase 6 完成（流程守門 + 驗證完成） |
| Where am I going? | 等使用者決定是否 commit |
| What's the goal? | 防止 GraphToolkit 驗證再次暴走，且還原非目標場景改動（已達成） |
| What have I learned? | 參考 findings.md |
| What have I done? | 已完成安全測試入口 + 分類/超時 + 場景還原，並驗證 10/10 通過 |
