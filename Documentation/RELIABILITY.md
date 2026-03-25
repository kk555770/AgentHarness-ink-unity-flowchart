# RELIABILITY

> 文件負責人：harness
> 最後更新：2026/03/25

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

- `repo_guard v1` 已先守 docs 入口、asmdef 依賴方向、`.cs` 檔案大小
- `repo_guard v4` 已開始守 docs freshness / index 內容量 / cross-links / active exec plan / ownership / workflow permissions
- `docs-garden.yml` 已能抓最新成功 CI run 的測試 artifact，並重建 generated docs
- `test_results_index.md` 已升級成 Unity evidence index，會把 XML 摘要、console evidence、log source 與 CI job 狀態收斂在同一頁
- `run_tests.sh` 現在會把 EditMode / PlayMode 的 Unity log 落到 `Logs/UnityEvidence/`
- EditMode / PlayMode / contract 測試已是主要回饋迴路
- current projection contract 已被文件化
- `ImportProjection(flowchart-json)` 已納入正式 system-of-record，且 control-plane dispatcher / shared service / 測試已落地
- `ValidateProjection(Ink)` / `ProjectGraph(Ink)` 現在有 core-level 預設 compile probe，不再只靠 Editor bootstrap 才能驗證
- docs ownership 與 golden principles 已有正式入口，現在不再只靠口頭共識
- `codex-auto-fix.yml` 已改成 Unity-first verify flow，而且缺少 `UNITY_LICENSE` 時會 fail closed
- 目前活躍收斂計畫在 `Documentation/exec-plans/active/harness_system_of_record_v2.md` 與 `Documentation/exec-plans/active/harness_alignment_closure_loop.md`

## 主要缺口

- 缺少更完整的 code graph / asmdef 以外架構 gate
- `ImportProjection` 目前只先支援 `flowchart-json`
- 缺少不依賴 `UNITY_LICENSE` 的完整 Unity 驗證回饋
- 目前仍需要至少一個新版 CI run 產出 Unity 測試 artifact，之後 generated evidence truth 才會從 `Missing` 進入穩定更新
