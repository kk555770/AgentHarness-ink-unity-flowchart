# generated index

> 文件負責人：harness
> 最後更新：2026/03/25

## 角色

- 放生成文件入口
- 這裡只放可重建、可再生的產物
- generated docs 由固定腳本與 workflow 重建，不靠手動改內容

## 固定 generated docs

- `Documentation/generated/doc_garden_report.md`
- `Documentation/generated/test_results_index.md`

## 更新方式

- 本地驗證：`python3 Tools/generate_test_results_index.py`
- `python3 Tools/fetch_ci_test_results.py`
- `python3 Tools/generate_test_results_index.py --results-root Artifacts/CI/TestResults`
- `python3 Tools/doc_garden.py`
- GitHub Actions：`.github/workflows/docs-garden.yml`

## 目前流程

- `docs-garden.yml` 會先抓最新成功 CI run 的 test / evidence artifact
- 接著重建 `test_results_index.md` 與 `doc_garden_report.md`
- 若 generated docs 有變更，workflow 會自動開 PR
- `doc_garden_report.md` 也會列出 owner coverage
- `test_results_index.md` 已升級成最小 Unity evidence index：
  - 測試摘要
  - console error / warning 計數
  - 短 console 摘要
  - log source
  - 本地模式會讀 `Logs/TestResults` 與 `Logs/UnityEvidence`
  - 若 CI 沒有 artifact，則顯示 `Missing` 與對應 job 狀態
