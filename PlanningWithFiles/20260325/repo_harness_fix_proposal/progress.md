# 進度紀錄：repo harness 改進提案

## 2026-03-25

- 建立 proposal 專用 `PlanningWithFiles/20260325/repo_harness_fix_proposal/`。
- 已重讀：
  - `AGENTS.md`
  - `.github/workflows/CI.yml`
  - `.github/workflows/codex-auto-fix.yml`
  - `README.md`
  - `Documentation/DocsIndex.md`
  - `Documentation/AuthoringToolStrategy.md`
  - `Documentation/CanonicalGraphApiSpec.md`
  - `PlanningWithFiles/20260321/ci_workflow_fix/*`
  - `PlanningWithFiles/20260322/*`
  - `PlanningWithFiles/20260323/*`
- 初步收斂方向：
  - `codex-auto-fix.yml` 不只要改 prompt，還要改驗證入口與安全前置檢查
  - 正式文件入口應新增「目前里程碑 / 現況進度」文件，而不是硬把大量歷史塞回 `DocsIndex.md`
  - docs / 架構 / 檔案大小 gate 比較適合先做成一支輕量 repo guard 腳本，再掛進 `CI.yml`
- 已進一步確認：
  - Batch 11 / 11A 目前在 repo 中已有程式與測試證據，不必只依賴 `PlanningWithFiles`
  - `Tools/run_tests.sh` 仍指向 `6000.3.2f1`，需與 `ProjectVersion.txt` 的 `6000.3.9f1` 一起對齊
  - asmdef 邊界已足夠明確，architecture guard 第一版可先守 asmdef references，不必一開始做全量 code graph 分析
