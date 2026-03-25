# 發現紀錄：repo harness 改進提案

## codex-auto-fix 現況

- `.github/workflows/codex-auto-fix.yml` 目前把 repo 當成 `Node.js monorepo with Jest tests`。
- 驗證步驟跑的是 `npm test --silent`，與此 repo 的 Unity 測試入口不相符。
- repo 內已存在較正確的本地測試入口：
  - `.github/workflows/CI.yml`
  - `Tools/run_tests.sh`

## 正式文件入口現況

- `README.md` 與 `Documentation/DocsIndex.md` 已經有「北極星 / 控制面 / 現況工作流」的導讀。
- 但入口仍主要停在 `Phase 1 / Batch 0~3`。
- Batch 8~11A 的最新里程碑、目前缺口、下一步優先順序，主要仍住在 `PlanningWithFiles/20260322-*`、`PlanningWithFiles/20260323-*`。

## 機械式 gate 現況

- repo 內目前沒有明顯的 docs / architecture / file-size guard 腳本。
- 也沒有 `.editorconfig` 或 analyzer 類型的 repo 級靜態規範入口。
- 現有 CI 主要是 Unity EditMode / PlayMode 測試，尚未覆蓋：
  - docs 入口是否同步
  - 架構文件要求與 repo 結構是否同步
  - 超大檔案是否持續擴散

## 目前可重用的基礎

- `CI.yml` 已有前置 gate job 模式，可延伸新的非 Unity guard job。
- `Documentation/CanonicalGraphApiSpec.md`、`Documentation/CanonicalGraphJsonContract.md` 已正式寫出 `ValidateProjection / ProjectGraph / ImportProjection`。
- `Tools/run_tests.sh` 已經明確只跑：
  - `OpsidanosInk.EditModeTests`
  - `OpsidanosInk.PlayModeTests`

## 補充收斂

### 正式文件入口不應再只停在 Phase 1 / Batch 0~3

- `README.md` 與 `Documentation/DocsIndex.md` 目前都把「現在做到哪裡」混在第一階段文件導讀裡。
- 比較穩的解法不是把所有 batch 歷史硬塞回兩份入口檔，而是新增一份穩定的：
  - `Documentation/CurrentMilestones.md`
- 這份文件只做 3 件事：
  - 用 1 頁摘要交代 Batch 8~11 的已交付內容
  - 說清楚 Batch 11A 是目前收斂中的 contract / host bridge 護欄，不要誤讀成 WebView 已開工
  - 把「已完成」「目前 focus」「尚未做」切開

### Batch 8~11A 在 repo 內已有足夠正式證據，可收斂回文件

- Batch 8：
  - `CreateGraph / CreateNode / ConnectPorts / ValidateGraph`
  - 證據：`PlanningWithFiles/20260322/batch8_json_command_bridge/findings.md`
- Batch 9：
  - `GetGraph` snapshot read loop
  - 證據：`PlanningWithFiles/20260322/batch9_getgraph_snapshot_bridge/findings.md`
- Batch 10：
  - `ReplaceNodePayload / DisconnectEdge / RemoveNode`
  - 證據：`PlanningWithFiles/20260322/batch10_mutation_bridge/findings.md`
- Batch 11 / 11A：
  - repo 內實作與測試已可見 `ValidateProjection / ProjectGraph`
  - 證據在：
    - `Packages/com.opsidanos.ink/Core/Scripts/Projection/CurrentFlow/CurrentFlowProjectionService.cs`
    - `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CanonicalGraphJsonCommandDispatcher.cs`
    - `Assets/Editor/Tests/CanonicalGraphJsonCommandDispatcherTests.cs`
    - `Assets/Editor/Tests/CanonicalGraphJsonContractRoundTripTests.cs`
- 因此正式文件可以用：
  - `Batch 8~10 = 已交付 control plane 基礎`
  - `Batch 11 = projection-specific control plane 已落地（`flowchart-json / ink`）`
  - `Batch 11A = contract / host bridge 護欄與切點收斂`
  這種寫法，不需要等所有後續 host 完成才更新。

### 機械式 gate 最穩的落點是 repo-local script + CI job

- 若直接把規則散寫在 GitHub Actions YAML，可維護性差，也不利於 agent 本地重跑。
- 最穩做法是：
  - 新增 `Tools/repo_guard.py`
  - 新增 `Tools/repo_guard_rules.json`
  - `CI.yml` 加一個 `repo-guards` job 呼叫它
- 這支 guard 先做最小但高價值的三類檢查：
  1. docs 入口同步
  2. asmdef 依賴方向
  3. `.cs` 檔案大小預算

### 架構 gate 可以先守 asmdef，而不是一開始就做整個 code graph

- 目前 repo 已有明確 asmdef 邊界：
  - `OpsidanosInk.CanonicalGraph`
  - `OpsidanosInk.Runtime`
  - `OpsidanosInk.FlowChartEditor`
  - `OpsidanosInk.EditModeTests`
  - `OpsidanosInk.PlayModeTests`
- 因此第一版 architecture guard 可先驗證：
  - `OpsidanosInk.CanonicalGraph.asmdef` 不得新增 references
  - `OpsidanosInk.FlowChartEditor.asmdef` 只能依賴 canonical graph 與 GraphToolkit editor assemblies
  - `OpsidanosInk.Runtime.asmdef` 不得反向依賴 editor asmdef
  - `OpsidanosInk.PlayModeTests.asmdef` 只依賴 runtime
- 這樣可以先把最容易漂移的層級逆流卡住。

### 檔案大小 gate 需要 baseline allowlist，而不是一刀切

- 現況已有多個大檔：
  - `Assets/Editor/SelectionExporter.cs`
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/InkTagCharacterStatePlayer.cs`
  - `Packages/com.opsidanos.ink/Runtime/Scripts/UI/VNPlayerPresenter.cs`
  - `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CanonicalGraphCommandService.cs`
- 所以第一版不應直接用單一上限把現況打爆。
- 比較穩的是：
  - 新檔預設上限較小（例如 500 行）
  - 既有超大檔用 allowlist + 冻結上限（只允許極小增幅，或先完全禁止再長）

### `codex-auto-fix.yml` 與 repo-local test script 目前也有版本不一致

- `ProjectSettings/ProjectVersion.txt` 是 `6000.3.9f1`
- `Tools/run_tests.sh` 的 `UNITY_BIN_DEFAULT` 仍是 `6000.3.2f1`
- 因此若要把 auto-fix prompt 引到 repo-local test entry，最好同一輪把 `Tools/run_tests.sh` 的預設版本也對齊，不然 repo 會同時存在兩套 Unity 版本敘事。
