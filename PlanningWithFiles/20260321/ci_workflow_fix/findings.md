# Findings

## 2026/03/21

- GitHub Actions 最新失敗 run：
  - run id：`23371534013`
  - branch：`arcumit/CodexInk`
  - commit：`2914dad`
- `gh run view` 顯示：
  - `This run likely failed because of a workflow file issue.`
- `gh run view --json ...` 回傳：
  - `jobs: []`
- 這表示失敗很可能發生在 workflow 解析或 job 條件判斷階段，不像是測試真的跑到一半失敗。

- `CI.yml` 目前有兩段 job-level `if`：
  - `if: ${{ secrets.UNITY_LICENSE != '' }}`
  - `if: ${{ secrets.UNITY_LICENSE == '' }}`
- `CI.yml` 目前指定：
  - `unityVersion: 6000.3.2f1`
- 專案實際版本：
  - `ProjectVersion.txt` 是 `6000.3.9f1`

## 收斂判斷

- 第一刀應先修 workflow 啟動條件，不碰產品碼。
- 最穩的做法是先加一個前置 gate job，把 `UNITY_LICENSE` 轉成一般輸出值，再讓後續 job 根據 outputs 判斷。
- Unity 版本應同一輪直接對齊到 `6000.3.9f1`。

## 實作後補充

- `CI.yml` 已新增 `ci-gate` job：
  - 用 step `unity-license` 讀 `UNITY_LICENSE`
  - 轉成 `has-unity-license=true/false`
  - 再由後續 job 使用 `needs.ci-gate.outputs.has-unity-license`
- `unity-tests` 與 `unity-tests-disabled` 已不再直接在 job-level `if:` 使用 `secrets`
- `unityVersion` 已從 `6000.3.2f1` 對齊到 `6000.3.9f1`

## 驗證結論

- `ruby` YAML 解析：
  - `yaml-ok`
- `git diff --check`
  - 無格式錯誤
- `git diff --stat`
  - 僅修改 `.github/workflows/CI.yml`

## 剩餘風險

- 這輪只能做本地 workflow 檔驗證，還無法在本地直接模擬 GitHub Actions 的完整 job 建立流程
- 真正確認 `0s workflow file issue` 是否消失，仍需要 push 後再看新的 GitHub run
