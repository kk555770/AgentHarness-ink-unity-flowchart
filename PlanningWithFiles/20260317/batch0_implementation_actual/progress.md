# 進度紀錄

## Session：2026-03-17

### Phase 1：建立工作記錄與確認範圍
- **Status:** complete
- **Started:** 2026-03-17 Asia/Taipei
- Actions taken:
  - 確認使用者要求開始
  - 建立本輪 `PlanningWithFiles/20260317/batch0_implementation_actual/` 紀錄
  - 確認先做 Batch 0，不跳到 Batch 1
- Files created/modified:
  - `PlanningWithFiles/20260317/batch0_implementation_actual/task_plan.md`（created）
  - `PlanningWithFiles/20260317/batch0_implementation_actual/findings.md`（created）
  - `PlanningWithFiles/20260317/batch0_implementation_actual/progress.md`（created）

### Phase 2：實作 core 骨架
- **Status:** complete
- Actions taken:
  - 確認本機 Unity 版本與 `Packages/com.opsidanos.ink/Core` 現況
  - 對照 Batch 0 kickoff 文件，收斂要立的最小型別
  - 新增 `OpsidanosInk.CanonicalGraph.asmdef`
  - 新增 `CanonicalNodeKinds.cs`
  - 新增 `CanonicalPortSemantics.cs`
  - 新增 `CanonicalGraphInvariant.cs`
- Files created/modified:
  - `PlanningWithFiles/20260317/batch0_implementation_actual/findings.md`（updated）

### Phase 3：實作測試掛點與最小測試
- **Status:** complete
- Actions taken:
  - 更新 `Assets/Editor/Tests/OpsidanosInk.EditModeTests.asmdef`，加入 `OpsidanosInk.CanonicalGraph`
  - 新增 `CanonicalNodeKindsTests.cs`
  - 新增 `CanonicalPortSemanticsTests.cs`
  - 新增 `CanonicalGraphInvariantTests.cs`
- Files created/modified:
  - `Assets/Editor/Tests/OpsidanosInk.EditModeTests.asmdef`（updated）
  - `Assets/Editor/Tests/CanonicalNodeKindsTests.cs`（created）
  - `Assets/Editor/Tests/CanonicalPortSemanticsTests.cs`（created）
  - `Assets/Editor/Tests/CanonicalGraphInvariantTests.cs`（created）

### Phase 4：執行 gate 與回報
- **Status:** complete
- Actions taken:
  - 使用 Unity `6000.3.9f1` 執行 `-runTests`
  - 先確認新 asmdef、測試組譯與檔案 import 成功
  - 追查 `testResults` XML 未落地的根因
  - 確認 Unity Test Framework 1.6.0 的 command line test runner 不應搭配 `-quit`
  - 以不帶 `-quit` 的叫法重跑 `Batch0CoreResults.xml`
  - 以不帶 `-quit` 的叫法重跑 `Batch0GateResults.xml`
  - 確認 Unity 已為新檔案生成 `.meta`
- Files created/modified:
  - `PlanningWithFiles/20260317/batch0_implementation_actual/task_plan.md`（updated）
  - `PlanningWithFiles/20260317/batch0_implementation_actual/findings.md`（updated）
  - `PlanningWithFiles/20260317/batch0_implementation_actual/progress.md`（updated）

## 測試結果
| Test | Input | Expected | Actual | Status |
|------|-------|----------|--------|--------|
| Unity 版本檢查 | `/Applications/Unity/Hub/Editor` | 確認可跑 Batch 0 gate 的版本 | 已確認有 `6000.3.9f1` | ✓ |
| Unity compile/import 驗證 | `-runTests` batchmode 啟動 | 新 asmdef 與測試可被 Unity 編譯 | 已成功完成編譯、domain reload、import 與 `.meta` 生成 | ✓ |
| Canonical core 測試 | `Batch0CoreResults.xml` | 7 個最小 core 測試全部通過 | `7/7 passed` | ✓ |
| Batch 0 gate 測試 | `Batch0GateResults.xml` | canonical core + smoke gate 通過 | `8/8 passed` | ✓ |
| XML 測試報表輸出 | `-testResults` 指定路徑 | 產出正式 XML 結果 | 已確認根因是 `-quit`；移除後 XML 可正常產出 | ✓ |

## 錯誤紀錄
| Timestamp | Error | Attempt | Resolution |
|-----------|-------|---------|------------|
| 2026-03-17 | Unity CLI 未輸出 `testResults` XML | 1 | 追到 Unity Test Framework 1.6.0 command line runner 不應搭配 `-quit`；移除後已正常產生 XML |

## 5 題重啟檢查
| Question | Answer |
|----------|--------|
| Where am I? | Phase 4，已完成 Batch 0 骨架、最小測試與正式 gate 驗證 |
| Where am I going? | 對使用者回報 Batch 0 可交付結果，並準備評估 Batch 1 |
| What's the goal? | 讓 Batch 0 真的落地，而不是只停在文件 |
| What have I learned? | Unity CLI 測試不是不能用，而是不能在 command line runner 上再加 `-quit` |
| What have I done? | 已新增 core 骨架、接上測試 asmdef、補上最小測試，並正式拿到 XML 報表與通過 gate |
