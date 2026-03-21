# Progress

## 2026/03/21

### 已完成

- 重讀：
  - `~/.codex/AGENTS.md`
  - repo `AGENTS.md`
- 確認工作樹乾淨
- 盤點：
  - `.github/workflows/CI.yml`
  - `ProjectSettings/ProjectVersion.txt`
  - 最新 GitHub Actions run 狀態

### 關鍵結論

- 現在的 CI failure 比較像 workflow file issue，不像測試 fail。
- `CI.yml` 需要把 `UNITY_LICENSE` 的判斷從 job-level secrets if 改成安全的 gate 輸出。
- `unityVersion` 也需要對齊到 `6000.3.9f1`。

### 已完成修改

- 在 `.github/workflows/CI.yml` 新增 `ci-gate` job
- 將 `UNITY_LICENSE` 的判斷改成：
  - 先由 gate step 寫入 `GITHUB_OUTPUT`
  - 再由 `needs.ci-gate.outputs.has-unity-license` 控制後續 job
- 將兩個 `unityVersion` 都從 `6000.3.2f1` 改成 `6000.3.9f1`

### 只讀驗證結果

- `ruby -e 'require \"yaml\"; YAML.load_file(...)'`
  - `yaml-ok`
- `git diff --check`
  - 無格式問題
- `git diff --stat`
  - 只改到 `.github/workflows/CI.yml`

### 目前狀態

- CI 修正已落地
- 尚未 commit
- 下一個真正驗證點會是 push 後的新 GitHub Actions run
