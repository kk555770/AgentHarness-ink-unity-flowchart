# 任務計畫：全專案深度調查（不使用 Unity MCP）

## 目標
在不修改程式與資產內容的前提下，完整調查專案現況、資料鏈、測試鏈、歷史計畫與風險，避免遺漏關鍵依賴與跨模組誤判。

## 範圍與限制
- 範圍：整個儲存庫（`Assets`、`Packages`、`ProjectSettings`、`Documentation`、`PlanningWithFiles`、根目錄規格檔）。
- 限制：本輪禁止使用 Unity MCP；只進行檔案與 Git 層級調查。
- 規則：每完成一個調查步驟，立即更新 `findings.md` 與 `progress.md`。

## 關鍵問題
1. 專案目前的模組結構、資料流、測試流是否一致。
2. 哪些模組存在高風險耦合或規格偏移。
3. 歷史計畫與目前工作樹是否有斷層。
4. 上一個提交（`dc8a443`）是否在誤解需求下做出錯誤修改。
5. 下一步應先收斂哪一段，才能形成可停止的穩定段落。
6. 整體專案的優化方案與未來規劃如何分期落地。

## 階段規劃
| 階段 | 狀態 | 說明 |
|---|---|---|
| 1. 基線盤點 | complete | 已盤點分支、工作樹、頂層目錄、主要規格檔 |
| 2. 規格與約束矩陣 | in_progress | 彙整 AGENTS / UI Toolkit / Graph Toolkit / README 契約 |
| 3. 程式模組地圖 | in_progress | 盤點 Runtime 與 Editor 模組、核心入口與依賴方向 |
| 4. 資料與資產鏈 | pending | 盤點場景、Prefab、Story 資料、UXML/USS、Scriptable 資產 |
| 5. 測試矩陣 | pending | 盤點 PlayMode / Editor 測試覆蓋範圍與缺口 |
| 6. 歷史計畫與提交時間線 | pending | 全量掃描 PlanningWithFiles 與 Git 主要變更脈絡 |
| 7. `dc8a443` 提交誤解風險稽核 | in_progress | 逐檔比對提交改動、測試意圖、規格契約與可能誤解 |
| 8. 整體優化方案與未來規劃 | pending | 形成跨模組優化藍圖、里程碑、驗收基準 |
| 9. 風險與下一步提案 | pending | 形成優先級、影響面與可停止段落建議 |

## 調查檢查清單
- [ ] 目錄與關鍵檔案全覽
- [ ] 規格文件契約摘要
- [ ] 程式入口與呼叫鏈
- [ ] Save/Load/Rollback/Restore 資料鏈
- [ ] UI 點擊/Auto/Skip 行為鏈
- [ ] Story tag 路由與播放器鏈
- [ ] 測試對應到實作檔的關係
- [ ] 歷史計畫對應到 commit 的一致性
- [ ] `dc8a443` 逐檔誤解風險審查
- [ ] 專案優化與未來規劃草案
- [ ] 未提交改動風險

## 錯誤紀錄
| 時間 | 錯誤 | 嘗試 | 結果 |
|---|---|---|---|
