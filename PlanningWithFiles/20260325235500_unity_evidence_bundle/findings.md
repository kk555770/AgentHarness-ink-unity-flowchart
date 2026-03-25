# Findings：unity_evidence_bundle

## 初始前提

- 上一輪已完成 docs ownership / core beliefs。
- 上一輪 parallel audit 已指出：下一輪最值得做的是 `Unity log-query / evidence bundle`。
- 目前 repo 已有：
  - `CI.yml`
  - `Tools/run_tests.sh`
  - `Documentation/generated/test_results_index.md`
  - `docs-garden.yml`

## 本輪判斷標準

- 先做最小但真的可重建的 evidence，不做空殼。
- 先補 logs / console / test summary 的查詢入口，不直接衝完整 metrics / traces。

## 目前已確認

- `Documentation/generated/test_results_index.md` 應直接升級成 evidence index，不再新增平行的 `unity_evidence_index.md`。
- `Tools/run_tests.sh` 現況只會落 XML，不會落可索引的 Unity log。
- 最新成功 CI run `23545345565` 沒有任何 Unity test artifact；原因不是抓取失敗，而是該 run 的 `unity-tests` job 被 skipped，`unity-tests-disabled` job 成功輸出說明。
- 本地 `Logs/TestResults/*.xml` 已經是可重建的真實證據，但 `Documentation/generated/test_results_index.md` 現在還綁在 `Artifacts/CI/TestResults`，所以需要升級成能同時描述本地與 CI artifact 缺失原因的 evidence 頁。

## 最終方案

- 不新增新的 generated evidence 檔案，直接把 `Documentation/generated/test_results_index.md` 升級成最小 Unity evidence index。
- `Tools/run_tests.sh` 補 `Logs/UnityEvidence/`，讓本地可重建 evidence 不再只有 XML。
- `CI.yml` 補 `unity-evidence-editmode` / `unity-evidence-playmode` artifact upload。
- `Tools/fetch_ci_test_results.py` 補 `unity-evidence-*` 抓取與 CI job metadata。
- `docs-garden.yml` 保持單一路徑：`fetch_ci_test_results.py` + `generate_test_results_index.py` + `doc_garden.py`，不額外用第二套 inline 腳本再拼 evidence。

## 最終再審核

- `Tools/run_tests.sh` 已成功落本地 XML 與 `Logs/UnityEvidence/*.log`
- `python3 Tools/generate_test_results_index.py` 可在本地模式正確產生 evidence index
- `python3 Tools/generate_test_results_index.py --results-root Artifacts/CI/TestResults` 可在 CI mode 正確顯示：
  - `artifact 類型 none`
  - `Unity 測試（Edit → Play，只跑我們的） skipped`
  - `Missing console source`
- `python3 Tools/doc_garden.py` 通過
- `python3 Tools/repo_guard.py` 通過
- `git diff --check` 通過
- `ruby -e 'require "yaml"; YAML.load_file(...)'` 通過

## 再審核後修掉的真問題

- `generate_test_results_index.py` 一開始會把 stack trace / 方法名誤算成 console error，已改成排除 stack trace 並收斂判準。
- `generate_test_results_index.py` 一開始在 CI mode 會誤吃本地 `Logs/UnityEvidence`，已改成只有本地 `Logs/TestResults` 模式才會自動補本地 evidence root。
- `docs-garden.yml` 一開始被加進第二套 ad-hoc evidence 彙整腳本，已收斂回單一路徑，避免 workflow 與 `Tools/*` 重複實作。
