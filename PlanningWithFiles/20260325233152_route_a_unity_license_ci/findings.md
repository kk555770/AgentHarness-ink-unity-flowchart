# Findings

> 任務：Route A / GitHub-hosted Unity feedback loop

## Findings

- `gh` CLI 可用，且 `gh auth status` 顯示已登入 `kk555770`，token scope 包含 `repo` 與 `workflow`。
- git remote：
  - `origin` = `https://github.com/kk555770/ink-unity-integration.git`
  - `upstream` = `https://github.com/inkle/ink-unity-integration.git`
- 對 fork repo `kk555770/ink-unity-integration`，目前可合理視為具備設定 repository secret 的能力；對 upstream 只有讀取權限。
- 現有 CI 基線已具備：
  - 缺少 `UNITY_LICENSE` 時 fail closed
  - Unity test artifact 與 evidence artifact 會分開上傳
  - docs-garden 會優先抓 `workflow_run` provenance
- 目前 generated test index 已能讀取：
  - run id
  - artifact 類型
  - suite 結果
  - log source
- 目前 evidence manifest 還偏最小，只記 suite、log file、test result path，尚未把 run metadata / sha / branch / workflow run id 直接寫入 artifact 內。
- 依照 GameCI 官方 GitHub 文件，personal license 需要：
  - `UNITY_LICENSE`
  - `UNITY_EMAIL`
  - `UNITY_PASSWORD`
- professional license 則是：
  - `UNITY_SERIAL`
  - `UNITY_EMAIL`
  - `UNITY_PASSWORD`
- 系統層已找到本機 `.ulf`：
  - `/Library/Application Support/Unity/Unity_lic.ulf`
- fork repo 目前 `gh secret list --repo kk555770/ink-unity-integration` 沒有列出任何現有 secret，表示 A 路線的 GitHub 端 secret 仍待建立。
- 本輪已開始把 repo 內改動收斂為：
  - CI / auto-fix 改成檢查完整 Unity secrets 組合
  - auto-fix 會先抓失敗 run 的 artifact
  - local / CI 兩邊都會寫 suite-level evidence manifest
- 本地驗證已通過：
  - `bash Tools/run_tests.sh`
  - `python3 Tools/generate_test_results_index.py --results-root Logs/TestResults`
  - `python3 Tools/doc_garden.py`
  - `python3 Tools/repo_guard.py`
- 目前真正的外部 blocker 不是 repo 內程式碼，而是：
  - 把敏感 Unity secrets 寫到 GitHub repo secrets 需要使用者明確確認
  - repo 端除了 `.ulf` 以外，仍缺 `UNITY_EMAIL` / `UNITY_PASSWORD` 或 `UNITY_SERIAL` 的安全來源
