# 閒置專案 CI/CD 停用計畫

## 目標

把目前閒置專案中會在使用者未明確意識到時自動執行的 CI/CD 或 PR
相關流程關掉。保留可以人工啟動的檢查入口，避免之後無意識消耗資源。

## 原則

- 不建立 PR。
- 不碰既有未追蹤工作資料夾。
- 先找出真正會自動觸發的來源，再做最小變更。
- 若有 GitHub Actions，優先改成 `workflow_dispatch` 人工觸發。
- 若沒有 CI/CD 設定，記錄證據並確認是否有外部 automation。

## 階段

| 階段 | 狀態 | 內容 |
| --- | --- | --- |
| 1 | complete | 盤點 GitHub Actions、CI 設定、排程與 PR 自動化入口 |
| 2 | complete | 修改自動觸發設定，改成人工明確啟動 |
| 3 | complete | 檢查文件是否需要同步標註「專案閒置」 |
| 4 | complete | 驗證變更，不啟動遠端 CI，不建立 PR |

## 決策紀錄

- 2026-05-20：使用者明確要求專案閒置，停用閒置 CI/CD，
  並避免在未意識到時一直跑 PR。
- 2026-05-20：遠端 GitHub workflow 已全部設為 `disabled_manually`。
- 2026-05-20：9 個由 GitHub Actions bot 產生的 docs-garden PR 已關閉，
  並刪除對應 `codex/docs-garden-*` branch。
- 2026-05-20：驗證通過；目前沒有 open PR、queued run、in-progress run。

## 錯誤紀錄

| 錯誤 | 處理 |
| --- | --- |
- 無 | 無 |
