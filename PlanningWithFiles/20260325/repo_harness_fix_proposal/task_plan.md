# 任務計畫：repo harness 改進提案

## 目標

- 針對使用者指定的三件事提出可執行的修改方案：
  1. 修正 `codex-auto-fix.yml`，讓自動修復流程真的理解這是 Unity repo。
  2. 把 Batch 8~11A 的最新里程碑收斂進正式文件入口。
  3. 補上 docs / 架構 / 檔案大小的機械式 lint 或 CI gate。

## 限制

- 這一輪只做提案，不修改產品碼或 workflow。
- 可更新 `PlanningWithFiles` 工作記錄，保留可追溯性。

## 階段

| 階段 | 狀態 | 說明 |
| --- | --- | --- |
| 1 | 進行中 | 重讀相關 workflow、docs、planning 歷史，確認現況缺口 |
| 2 | 未開始 | 收斂每條提案的檔案清單、改法、原因、效果、驗證 |
| 3 | 未開始 | 輸出正式提案 |

## 關鍵問題

1. `codex-auto-fix.yml` 最小修法應該只改 prompt，還是要一起補測試入口與 guard？
2. Batch 8~11A 應該收斂到既有 `README.md` / `DocsIndex.md`，還是新增一份正式里程碑文件？
3. docs / 架構 / 檔案大小的 gate 應放進現有 `CI.yml`，還是拆成獨立 workflow？

## 錯誤紀錄

| 錯誤 | 嘗試 | 處理 |
| --- | --- | --- |
| 無 | 1 | 目前仍在唯讀盤點與提案收斂 |
