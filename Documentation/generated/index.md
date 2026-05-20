# generated index

> 文件負責人：harness
> 最後更新：2026/05/20

## 角色

- 放生成文件入口
- 這裡只放可重建、可再生的產物
- generated docs 由固定腳本重建；閒置期間不靠背景 workflow 自動開 PR

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

- 專案目前閒置，`docs-garden.yml` 不再由 `workflow_run` 或排程自動啟動
- 若使用者明確手動啟動並輸入確認字，workflow 會抓最新成功 CI run 的
  test / evidence artifact
- 接著重建 `test_results_index.md` 與 `doc_garden_report.md`
- 若 generated docs 有變更，只有在手動確認啟動時才會建立 PR
- `doc_garden_report.md` 也會列出 owner coverage
- `ImportProjection(flowchart-json)` 已納入正式 system-of-record 與 control-plane；generated docs 仍應只反映實際驗證證據，不負責替未跑過的 target 背書
- `test_results_index.md` 已升級成最小 Unity evidence index：
  - 測試摘要
  - console error / warning 計數
  - 短 console 摘要
  - log source
  - CI / local 兩邊都會寫 suite manifest，保留最少必要 provenance
  - 本地模式會讀 `Logs/TestResults` 與 `Logs/UnityEvidence`
  - 若 CI 沒有 artifact，則顯示 `Missing` 與對應 job 狀態
