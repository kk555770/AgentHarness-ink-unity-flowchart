# RELIABILITY

> 文件負責人：harness
> 最後更新：2026/05/20

## 現有可靠性入口

- `.github/workflows/CI.yml`
- `.github/workflows/docs-garden.yml`
- `Tools/repo_guard.py`
- `Tools/doc_garden.py`
- `Tools/generate_test_results_index.py`
- `Tools/run_tests.sh`
- `Assets/Editor/Tests/`
- `Assets/Tests/PlayMode/`
- `Documentation/DeveloperModeOutputContract.md`
- `Documentation/DocsOwnership.md`
- `Documentation/design-docs/core-beliefs.md`

## 現況

- 2026/05/20 起，專案進入閒置狀態：
  - 遠端 `CI`、`Update-UPM-Branch`、`Codex Auto-Fix on Failure`、
    `Docs Garden` workflow 都已設為 `disabled_manually`
  - `CI.yml` 改為只能人工啟動
  - `docs-garden.yml` 取消 `workflow_run` 與每週排程，且需要確認字才會開 PR
  - `codex-auto-fix.yml` 取消 CI failure 自動觸發，job 在閒置期間不執行
  - `Update-UPM-Branch.yml` 取消 tag push 觸發，改為手動指定 tag
- `repo_guard v1` 已先守 docs 入口、asmdef 依賴方向、`.cs` 檔案大小
- `repo_guard v4` 已開始守 docs freshness / index 內容量 / cross-links / active exec plan / ownership / workflow permissions
- `docs-garden.yml` 保留抓最新成功 CI run 測試 artifact 與重建 generated docs
  的能力，但閒置期間不再自動排程或自動跟著 CI 執行
- `test_results_index.md` 已升級成 Unity evidence index，會把 XML 摘要、console evidence、log source 與 CI job 狀態收斂在同一頁
- `run_tests.sh` 現在會把 EditMode / PlayMode 的 Unity log 落到 `Logs/UnityEvidence/`
- EditMode / PlayMode / contract 測試已是主要回饋迴路
- current projection contract 已被文件化
- `ImportProjection(flowchart-json)` 已納入正式 system-of-record，且 control-plane dispatcher / shared service / 測試已落地
- `ValidateProjection(Ink)` / `ProjectGraph(Ink)` 現在有 core-level 預設 compile probe，不再只靠 Editor bootstrap 才能驗證
- docs ownership 與 golden principles 已有正式入口，現在不再只靠口頭共識
- `CI.yml` 與 `codex-auto-fix.yml` 現在都會檢查完整 Unity 驗證 secrets：
  - personal：`UNITY_LICENSE` + `UNITY_EMAIL` + `UNITY_PASSWORD`
  - professional：`UNITY_SERIAL` + `UNITY_EMAIL` + `UNITY_PASSWORD`
- Unity 測試 workflow 會把 suite manifest 寫進 evidence artifact，並帶上 run id / branch / sha / auth mode 等 provenance
- 目前活躍收斂計畫在 `Documentation/exec-plans/active/harness_system_of_record_v2.md` 與 `Documentation/exec-plans/active/harness_alignment_closure_loop.md`

## 主要缺口

- 閒置期間 generated evidence 不會自動刷新；需要使用者明確恢復 CI/CD 後才會更新。
- 缺少更完整的 code graph / asmdef 以外架構 gate
- `ImportProjection` 目前只先支援 `flowchart-json`
- 完整 Unity 驗證回饋仍依賴 GitHub 上有有效的 Unity secrets 組合，以及至少一個成功 CI run 產生 artifact
- 目前仍需要至少一個新版 CI run 產出 Unity 測試 artifact，之後 generated evidence truth 才會從 `Missing` 進入穩定更新
