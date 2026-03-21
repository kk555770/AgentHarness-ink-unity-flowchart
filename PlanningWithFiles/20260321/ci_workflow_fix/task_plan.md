# 任務計畫：CI workflow 啟動修復

## 任務目標

修復 GitHub Actions `CI.yml` 在 job 建立前就失敗的問題，
讓 workflow 至少能正常建立 job，並對齊專案實際 Unity 版本。

## 任務範圍

- 盤點 `CI.yml` 的 job-level `if` 與 Unity 版本設定
- 修改 workflow，避免直接在 job-level `if` 使用 `secrets`
- 對齊 `ProjectVersion.txt` 的 Unity 版本
- 做最小只讀驗證並記錄結果

## 階段

- [x] 重讀全域與 repo `AGENTS.md`
- [x] 確認目前工作樹乾淨
- [x] 盤點 `CI.yml` 與 `ProjectVersion.txt`
- [x] 修改 `CI.yml`
- [x] 做只讀驗證
- [x] 更新 findings / progress

## 目前判斷

1. 最新 GitHub Actions run 是 `0s` 失敗，且 `jobs` 為空，符合 workflow file issue 特徵。
2. `CI.yml` 目前直接在 job-level `if:` 使用 `secrets.UNITY_LICENSE`，這是高風險寫法。
3. `CI.yml` 仍指定 `6000.3.2f1`，但專案實際版本是 `6000.3.9f1`。
