# 任務計畫：strict_harness_alignment

## 目標

- 依據 OpenAI harness engineering 文章與上一輪審核結果，讓目前 repo 的 harness 流程更嚴格對齊目標。
- 修正已知缺口：docs freshness、doc-gardening 回寫、generated test truth、doc-garden 分類統計。
- 完成一輪實作後，重新做完整審核；若仍有缺口，再做下一輪。

## 階段

| 階段 | 狀態 | 說明 |
| --- | --- | --- |
| 1 | 已完成 | 建立本輪 PlanningWithFiles、整理缺口 |
| 2 | 已完成 | 第一輪實作修正 |
| 3 | 已完成 | 驗證、重新生成輸出與同步正式文件 |
| 4 | 已完成 | 重新完整審核 |
| 5 | 已完成 | 依再審核結果完成第二輪優化並再次驗證 |

## 本輪要改的檔案

- `.github/workflows/docs-garden.yml`
- `Tools/repo_guard.py`
- `Tools/repo_guard_rules.json`
- `Tools/doc_garden.py`
- `Tools/generate_test_results_index.py`
- `Documentation/generated/index.md`
- `Documentation/generated/doc_garden_report.md`
- `Documentation/generated/test_results_index.md`
- `Documentation/QUALITY_SCORE.md`
- `Documentation/RELIABILITY.md`
- `Documentation/exec-plans/tech-debt.md`

## 驗證

- `python3 Tools/doc_garden.py`
- `python3 Tools/generate_test_results_index.py`
- `python3 Tools/repo_guard.py`
- `git diff --check`

## 錯誤紀錄

- 本機 Python `urllib` 連 GitHub API 時遇到 SSL 憑證驗證錯誤，已在 `Tools/fetch_ci_test_results.py` 補 `gh api` fallback。
