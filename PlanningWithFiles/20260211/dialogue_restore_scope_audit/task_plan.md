# 任務計畫：對話系統 Restore 回歸與範圍偏移稽核

## 目標
釐清三件事：
1. 這次真正的錯誤發生在哪裡（程式邏輯 / 流程決策 / 版本歷史）。
2. 我這次改動到底修了什麼、改了哪個契約。
3. 專案原本的對話設計資料鏈是什麼，哪裡被混進 GraphToolkit 任務。

## 關鍵問題
1. `Restore_缺少Appear時_不應產生CharTransitionStepsError` 為何會失敗？
2. 2026/02/06 曾記錄「已修復」為何在目前 `HEAD` 看不到修正？
3. 目前未提交修正是否符合「原始對話系統設計」還是屬於設計變更？

## 階段規劃
| 階段 | 狀態 | 說明 |
|---|---|---|
| 1. 範圍與版本基線 | complete | 盤點分支、工作樹、最近提交、受影響檔案 |
| 2. 失敗測試與改動對照 | complete | 逐行比對失敗測試與 `InkTagCharacterStatePlayer` 差異 |
| 3. 歷史計畫與提交追因 | complete | 對照 `PlanningWithFiles` 歷史與 git 歷史，定位回歸時間點 |
| 4. 原始對話設計資料鏈重建 | complete | 串起 `StoryOutput -> Router -> CharacterStatePlayer -> Save/Restore` |
| 5. 結論與風險評估 | complete | 輸出「錯誤點、修復點、設計契約、後續決策點」，並補查既定對話流程條文與場景資料鏈 |

## 已知限制
- 本輪以調查與結論為主，不新增功能改動。
- 若需進一步調整程式碼，需另外提案並取得同意。

## 錯誤紀錄
| 時間 | 錯誤 | 嘗試 | 結果 |
|---|---|---|---|
| 2026-02-11 | `git show dc8a443 -- <runtime-file>` 無輸出 | 改用 `git log -- <file>` + `git show <commit>:<file>` | 成功確認 `dc8a443` 未修改該檔 |
