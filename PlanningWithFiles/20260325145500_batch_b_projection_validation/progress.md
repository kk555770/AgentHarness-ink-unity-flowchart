# Progress：batch_b_projection_validation

## 2026-03-25 14:55

- 確認 `CurrentFlowProjectionService` 的 `InkTarget` 路徑仍停留在字串生成。
- 確認現有 tests 只有 string-level assertion，沒有 compile-level harness。
- 決定不把 `Ink.Compiler` 直接引入 canonical core。

## 2026-03-25 14:58

- 已新增 core probe interface 與 registry。
- 已新增 editor bootstrap，Editor domain 會自動安裝真實 compile probe。
- 已更新 `CurrentFlowProjectionService`，讓 `ValidateProjection(Ink)` / `ProjectGraph(Ink)` 都會先跑 probe。

## 2026-03-25 15:00

- 已新增 `CurrentFlowInkProjectionCompileHarnessTests`。
- 測試已覆蓋：
  - validation 會呼叫 probe
  - project 會回傳可編譯文本
  - compile failure 會回報固定錯誤碼
  - JSON control plane 的 `ProjectGraph(Ink)` 也會走同一條 probe

## 2026-03-25 15:02

- 驗證完成：
  - `python3 Tools/repo_guard.py`
  - `git diff --check`
  - `Tools/run_tests.sh`
- EditMode / PlayMode 都通過。
