# 任務計畫：unity_evidence_bundle

## 目標

- 依 OpenAI harness engineering 文章，落地這個 repo 的下一個可重建 evidence 層。
- 把 Unity 測試摘要、log / console 證據整理成 generated docs 與 CI artifact。
- 完成後驗證這些證據不是口頭保證，而是可重建、可索引、可再審核。

## 階段

| 階段 | 狀態 | 說明 |
| --- | --- | --- |
| 1 | 已完成 | 建立 PlanningWithFiles、盤點 run_tests / CI / log evidence 現況 |
| 2 | 已完成 | 確定最小 evidence bundle 方案 |
| 3 | 已完成 | 實作腳本、workflow、generated docs、正式文件 |
| 4 | 已完成 | 驗證與再審核 |

## 候選產物

- `Documentation/generated/unity_evidence_index.md`
- `Logs/` 或 `Artifacts/CI/` 下的 Unity log summary
- CI artifact upload / fetch / generated docs 連動

## 驗證

- `python3 Tools/repo_guard.py`
- `git diff --check`
- 本輪 evidence 腳本驗證
- `Tools/run_tests.sh`
- `ruby -e 'require "yaml"; YAML.load_file(...)'`
