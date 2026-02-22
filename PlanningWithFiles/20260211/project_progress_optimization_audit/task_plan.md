# 任務計畫：專案進度整理、優化方案與下一步（重建版）

## 目標
- 完整整理目前專案進度與內容（程式、測試、資產、歷史計畫、提交脈絡）。
- 串清楚「程式 <-> Unity 資料鏈 <-> 測試鏈」的對應關係。
- 提出可執行的優化方案與下一步。

## 方法與原則
- 使用 `planning-with-files`：調查一次就記錄一次。
- 以調查為主；若需修改程式/測試/文件，必須先提案並取得同意。
- 本任務的「主要紀錄」是三檔（`task_plan.md` / `findings.md` / `progress.md`），但允許補上已核准的實作與文件維護結果，避免紀錄與現況矛盾。
- 每個結論獨立成一筆 findings。

## 範圍
- Repository 全域：`Assets`、`Packages`、`ProjectSettings`、`Documentation`、`PlanningWithFiles`。
- 歷史任務全掃描，不只近期。
- Git 提交脈絡（重點關注 `896d997`、`84cfd67`、`dc8a443`）。

## 階段
| 階段 | 狀態 | 說明 |
|---|---|---|
| 1 | complete | 基線盤點（分支、工作樹、目錄與規格檔） |
| 2 | complete | 歷史計畫全掃描與時間線 |
| 3 | complete | 程式模組地圖（Runtime / Editor / 測試） |
| 4 | complete | Unity 資料鏈（場景、UXML/USS、Story、資源） |
| 5 | complete | 測試矩陣與風險缺口 |
| 6 | complete | 專案進度總結與優化方案 |
| 7 | complete | 下一步執行計畫（可停止段落） |

## 錯誤紀錄
| 時間 | 錯誤 | 嘗試 | 結果 |
|---|---|---|---|
| 2026-02-11 | 誤用 `Assets/Tests/EditMode` 搜尋 EditMode 測試 | 立即改用 `rg --files | rg \"EditMode|GraphToolkit\"` 重新定位 | 已確認正確路徑是 `Assets/Editor/Tests`，調查恢復正常 |
