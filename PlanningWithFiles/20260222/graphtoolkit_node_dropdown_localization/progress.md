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
| GraphToolkit Import（Phase 7 重跑） | `run_tests(EditMode, assembly+category+group=InkFlowChartImportTests)` | 只跑 Import fixture | 8/8 Passed | pass |
| GraphToolkit RoundTrip（Phase 7 重跑） | `run_tests(EditMode, assembly+category+group=InkFlowChartRoundTripTests)` | 只跑 RoundTrip fixture | 2/2 Passed | pass |
| Console Error 檢查（Phase 7） | `read_console types=[\"error\"]` | 0 筆 | 0 筆 | pass |
| GraphToolkit Import（Phase 15） | `run_tests(EditMode, assembly=OpsidanosInk.EditModeTests, category=GraphToolkitFlowSafe, group=InkFlowChartImportTests)` | 含 stageAction 匯入案例通過 | 9/9 Passed | pass |
| GraphToolkit RoundTrip（Phase 15） | `run_tests(EditMode, assembly=OpsidanosInk.EditModeTests, category=GraphToolkitFlowSafe, group=InkFlowChartRoundTripTests)` | 含 stageAction round-trip 案例通過 | 3/3 Passed | pass |
| Console Error 檢查（Phase 15） | `read_console types=[\"error\"] count=20` | 0 筆 | 0 筆 | pass |

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

## Session: 2026-02-23（節點標題中文化修正）

### Phase 7: 節點型別中文化與標題修正
- **Status:** complete
- **Started:** 2026-02-23
- Actions taken:
  - 重新盤點 GraphToolkit 標題來源，確認 `UserNodeModelImp.Title` 來自節點類別名稱。
  - 確認先前反射覆寫 `Title` 方案無法反映到畫面。
  - 使用者已同意改為「中文類別名 + 相容遷移」方案，開始實作。
  - `InkFlowChartNodes.cs`：移除 `ApplyNodeTitle` 反射邏輯，新增中文可見節點型別（開始/對話/註解/選項/條件）。
  - `InkFlowChartNodes.cs`：`InkFlowActionKind` 新增 `InspectorName`，下拉顯示改為繁中。
  - `InkFlowChartImporter.cs`：`CreateNodeByType` 改為建立中文可見節點型別，讓匯入後節點名稱也為繁中。
  - `InkFlowChartImportTests.cs`：新增斷言，鎖住匯入建立中文節點型別（開始/對話/註解）。
  - `DeveloperModeOutputContract.md`：補上「節點型別名稱需與白話顯示名一致」規範。
  - 透過 MCP 安全範圍重跑 Import（8/8）與 RoundTrip（2/2），Console error = 0。
- Files created/modified:
  - `PlanningWithFiles/20260222/graphtoolkit_node_dropdown_localization/task_plan.md`
  - `PlanningWithFiles/20260222/graphtoolkit_node_dropdown_localization/findings.md`
  - `PlanningWithFiles/20260222/graphtoolkit_node_dropdown_localization/progress.md`
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodes.cs`
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`
  - `Assets/Editor/Tests/InkFlowChartImportTests.cs`
  - `Documentation/DeveloperModeOutputContract.md`

## Session: 2026-02-24（完整計畫補齊：結構化對話節點）

### Phase 8: 新需求完整盤點與計畫落地
- **Status:** complete
- **Started:** 2026-02-24
- Actions taken:
  - 讀取 `planning-with-files` skill，依流程把新需求落地到三份 planning 檔。
  - 整理使用者新增需求：不用手填 JSON、對話節點改為結構化欄位、角色映射與動作參數動態化。
  - 盤點現況缺口（資料模型/sidecar/匯出匯入/測試）並拆成 Phase 9-14。
  - 補上下一階段驗證標準與風險（包含測試防暴走持續生效條件）。
- Files created/modified:
  - `PlanningWithFiles/20260222/graphtoolkit_node_dropdown_localization/task_plan.md`
  - `PlanningWithFiles/20260222/graphtoolkit_node_dropdown_localization/findings.md`
  - `PlanningWithFiles/20260222/graphtoolkit_node_dropdown_localization/progress.md`

## Next Focus
- 進入 Phase 9：先補齊 `DeveloperModeOutputContract.md` 的結構化對話節點契約，再開始改碼。

## Session: 2026-02-25（動作節點拆分 + 流程線/資料線分離）

### Phase 15: 流程線/資料線分離（動作節點拆分）
- **Status:** complete（MCP 安全範圍測試已通過）
- **Started:** 2026-02-25
- Actions taken:
  - `InkFlowChartNodes.cs`：
    - 新增 `NodeTypeStageAction`、`InkFlowActionPayload`。
    - 對話節點新增 `ActionInputCount` option 與 `ActionIn*` typed input ports。
    - 新增動作節點 `InkFlowStageActionNode` + 中文節點 `動作`（`ActionData` typed output）。
    - 保留 `InkFlowActionNode : InkFlowDialogueNode` 相容別名。
  - `InkFlowChartExportModels.cs`：`ExportNodeOutputDto` 新增 `toPortName`。
  - `InkFlowChartExporter.cs`：
    - 輸出 `toPortName`。
    - stageAction 驗證（1 條資料線、只能接對話 `ActionIn*`）。
    - 匯出 Ink 改為「stageAction 內容併入對話 knot」且 stageAction 不單獨輸出 knot。
  - `InkFlowChartImporter.cs`：
    - 連線改讀 `toPortName`（缺值回退 `Flow`）。
    - 支援匯入 `stageAction` 節點。
    - 對話節點匯入時依 stageAction 連線自動還原 `ActionInputCount`。
  - 測試：
    - `InkFlowChartImportTests.cs` 新增 stageAction 資料線案例。
    - `InkFlowChartRoundTripTests.cs` 新增 stageAction round-trip 案例。
    - 移除舊 `actionKind` 下拉斷言（因節點已拆分）。
  - 文件：
    - `DeveloperModeOutputContract.md` 補上流程線/資料線規範、`stageAction` 規則、`toPortName` 欄位。
- Files created/modified:
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodes.cs`
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExportModels.cs`
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`
  - `Assets/Editor/Tests/InkFlowChartImportTests.cs`
  - `Assets/Editor/Tests/InkFlowChartRoundTripTests.cs`
  - `Documentation/DeveloperModeOutputContract.md`
  - `PlanningWithFiles/20260222/graphtoolkit_node_dropdown_localization/task_plan.md`
  - `PlanningWithFiles/20260222/graphtoolkit_node_dropdown_localization/findings.md`
  - `PlanningWithFiles/20260222/graphtoolkit_node_dropdown_localization/progress.md`

## Session: 2026-02-25（節點文字欄位拉長 + 契約先行）

### Phase 16: 文字欄位寬度調整（先契約、後實作）
- **Status:** in_progress（待使用者驗收）
- **Started:** 2026-02-25
- Actions taken:
  - 依使用者指示，先做契約更新，再做最小實作，不提前擴充選項化。
  - `DeveloperModeOutputContract.md` 新增 6.2.1-2「節點文字欄位可讀寬度（作者體驗）」。
  - 新增 `InkFlowChartNodeFields.uss`，把節點欄位 label 寬度壓縮、文字輸入區最小寬度拉長。
  - 新增 `InkFlowChartGraphStyleBootstrap.cs`，僅在 `.inkfc` GraphToolkit 視窗套用樣式。
- Files created/modified:
  - `Documentation/DeveloperModeOutputContract.md`
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodeFields.uss`
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodeFields.uss.meta`
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartGraphStyleBootstrap.cs`
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartGraphStyleBootstrap.cs.meta`
  - `PlanningWithFiles/20260222/graphtoolkit_node_dropdown_localization/task_plan.md`
  - `PlanningWithFiles/20260222/graphtoolkit_node_dropdown_localization/findings.md`
  - `PlanningWithFiles/20260222/graphtoolkit_node_dropdown_localization/progress.md`

## Session: 2026-03-03（規則重新載入與流程鎖定）

### Rule Sync: `/Users/arcumit/.codex/AGENTS.md` + 專案 `AGENTS.md`
- **Status:** complete
- Actions taken:
  - 重新讀取兩份 AGENTS.md，套用最新規範。
  - 依使用者要求，明確鎖定 multi-agent 並行規則與時限規則。
  - 啟用本輪執行約束：可並行工作走 multi-agent；測試任務 60 秒上限；調查/修改不限時。
  - 先將規則同步寫入 `task_plan.md`、`findings.md`、`progress.md`，作為後續實作基準。
- Files created/modified:
  - `PlanningWithFiles/20260222/graphtoolkit_node_dropdown_localization/task_plan.md`
  - `PlanningWithFiles/20260222/graphtoolkit_node_dropdown_localization/findings.md`
  - `PlanningWithFiles/20260222/graphtoolkit_node_dropdown_localization/progress.md`
