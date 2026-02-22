# 進度紀錄：Graph Toolkit 反覆問題根因調查

## 2026-02-08
- 建立 `task_plan.md`、`findings.md`、`progress.md`。
- 下一步：盤點歷史任務與近期 commit 差異。

- 完成基線盤點：分支狀態、最近 commit、Graph Toolkit 未提交修改清單。
- 觀察：最新 commit `84cfd67` 與 Graph Toolkit 直接相關，優先做 `HEAD~1..HEAD` 差異檢查。
- 完成 `PlanningWithFiles` 全歷史掃描，Graph Toolkit 問題主體集中於 `20260206/phase5_phase6_unified`。
- 已整理 5 類重複問題，下一步進入 `HEAD~1..HEAD` 精確差異比對。
- 追加根因定位：`A new asset is created at the same path...` warning 並非我們路徑重複，而是 Graph Toolkit 在 `ImportAsset` 回呼中先 `LoadGraphObjectAtPath`，讓 `RegisterNewGraphObject()` 必定命中 `existingAsset != null` 的警告條件。
- 完成 `84cfd67` 提交內容與目前工作樹差異比對。
- 已用 Unity MCP 擷取 Console：重現 3 筆同型 `same path as an existing asset` warning，呼叫點定位 `InkFlowChartGraph.CreateGraphAsset` 第 34 行。
- 下一步：跑 EditMode 測試收集目前完整失敗面，再做「反覆問題→根因群組」對齊。
- 已對照 `GraphToolkitSpec.md` 的 GraphDatabase 規格，確認目前行為屬 API 可用範圍，但 warning 來自套件內部實作。
- 已確認 `84cfd67` 與目前工作樹在建圖 API 選擇上不同（Prompt vs CreateGraph）。
- 整組 EditMode 測試目前卡在外部套件測試（editor unfocused）；本輪改以 Console stack 與差異追蹤完成根因定位。
- 已完成根因分群整理（4 大類），可直接對應使用者觀察的「上一個 commit 前後差異」。
- 新增觀察：外部套件測試副作用產生 `Assets/TempLinkUpgrade` 場景檔，已記錄為非 Graph Toolkit 主線變更。
