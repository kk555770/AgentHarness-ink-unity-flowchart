# Progress：unity_evidence_bundle

## 2026-03-25 23:55

- 重新讀取 repo `AGENTS.md`
- 重新讀取 `planning-with-files` skill
- 確認工作區乾淨
- 準備盤點 `run_tests.sh`、`CI.yml`、現有 generated docs 與可用 log evidence

## 2026-03-26 00:12

- 盤點 `Tools/run_tests.sh`、`Tools/fetch_ci_test_results.py`、`Tools/generate_test_results_index.py`、`Tools/doc_garden.py`、`Tools/repo_guard.py`
- 盤點 `Documentation/generated/index.md`、`Documentation/generated/test_results_index.md`、`Documentation/QUALITY_SCORE.md`、`Documentation/RELIABILITY.md`
- 確認本地 `Logs/` 目前只有：
  - `Logs/TestResults/OpsidanosInk_EditMode.xml`
  - `Logs/TestResults/OpsidanosInk_PlayMode.xml`
  - `Logs/Packages-Update.log`
- 用 `gh auth status` 確認可讀 GitHub Actions
- 用 `Tools/fetch_ci_test_results.py` 抓最新成功 CI run，確認 `run 23545345565` 沒有 Unity artifact，只留下 `_source.json`
- 用 `GH_REPO=kk555770/ink-unity-integration gh run view 23545345565 --json jobs,...` 確認缺 artifact 的原因是：
  - `Repo Guards` 成功
  - `CI 前置檢查` 成功
  - `Unity 測試（Edit → Play，只跑我們的）` skipped
  - `Unity 測試（尚未啟用）` 成功
- `evidence_doc_shape` sub-agent 回報：不新增新 generated doc，直接升級 `Documentation/generated/test_results_index.md`
- `local_unity_evidence_audit` sub-agent 回報：本地 evidence 應以 `Logs/TestResults/*.xml` 為主，再補 Editor / Unity log 入口

## 2026-03-26 00:18

- 修改 `Tools/run_tests.sh`
- `run_tests.sh` 現在除了 `Logs/TestResults/*.xml` 之外，也會把 EditMode / PlayMode 的 Unity log 寫到 `Logs/UnityEvidence/`
- 保留終端輸出，同時用 `tee` 落盤，避免本地 evidence 只有 XML 沒有 log

## 2026-03-26 00:24

- 更新 `Documentation/generated/index.md`
- 更新 `Documentation/QUALITY_SCORE.md`
- 更新 `Documentation/RELIABILITY.md`
- 更新 `Documentation/exec-plans/active/harness_system_of_record_v2.md`
- 更新 `Tools/repo_guard_rules.json`
- 正式文件與 guard 已開始把 `test_results_index.md` 描述為最小 Unity evidence index，而不是單純的 XML 清單

## 2026-03-26 00:40

- `evidence_tools_impl` sub-agent 完成：
  - `Tools/fetch_ci_test_results.py` 會抓 `unity-test-results-*` 與 `unity-evidence-*`
  - `_source.json` 現在會寫入 run / jobs / artifact type / roots
  - `Tools/generate_test_results_index.py` 已升級成 evidence index
- `evidence_workflow_impl` sub-agent 完成：
  - `CI.yml` 已補 evidence artifact upload
  - `docs-garden.yml` 一度被加上重複邏輯，主線已再收斂回「fetch script + generate script」單一路徑
- 本地驗證：
  - `Tools/run_tests.sh` 已成功跑完 EditMode / PlayMode
  - `Logs/UnityEvidence/OpsidanosInk_EditMode.log`
  - `Logs/UnityEvidence/OpsidanosInk_PlayMode.log`
  - `python3 Tools/generate_test_results_index.py` 已能正確顯示本地 XML + log evidence
  - 期間抓到兩個真 bug：
    - console error 判準太寬，會把 stack trace / 方法名算進去
    - CI mode 會誤吃本地 `Logs/UnityEvidence`
  - 以上兩點都已修正

## 2026-03-26 00:52

- 完成最後一輪完整再審核
- 串列重建 CI mode generated docs：
  - `python3 Tools/generate_test_results_index.py --results-root Artifacts/CI/TestResults`
  - `python3 Tools/doc_garden.py`
- 驗證：
  - `python3 Tools/repo_guard.py`
  - `git diff --check`
  - `ruby -e 'require "yaml"; YAML.load_file(...)'`
- 最終確認：
  - `Documentation/generated/test_results_index.md` 目前保留的是 CI truth，不是本地暫時驗證版本
  - `Documentation/generated/doc_garden_report.md` owner coverage / freshness 都正常
  - 這輪 evidence bundle 沒有再出現新的阻塞缺口
