# Progress：2026/01/30 專案進度整理工作紀錄

## 記錄規則
- 每做完一段調查/整理，就加一筆
- 每次有「決定」或「發現重要線索」，要同步寫到 `findings.md`

## 工作紀錄
- 2026/01/30：建立本次 planning-with-files 三個檔案（task_plan/findings/progress）
- 2026/01/30：盤點 `PlanningWithFiles/` 目前任務資料夾清單；先讀到 `20260127/project_status_refresh/task_plan.md`，確認先前已做過「對應關係表」與「下一步（Addressables）」的提案方向
- 2026/01/30：閱讀 `20260127/project_status_refresh/findings.md` 與 `20260124/project_status_next_steps/task_plan.md`，確認先前已把「功能總表」與「Tag/演出/ResourceMap」一大段做到 complete
- 2026/01/30：閱讀 `20260120/TextAdventureEngine/task_plan.md` 與 `20260122/TextAdventureEngine/task_plan.md`，確認 v1 計畫目前主要還缺：Phase 5（存讀檔/倒帶）、Phase 6（Editor Flow Chart）、Phase 7（完整示範驗收）
- 2026/01/30：閱讀 `README.md` 與 `Packages/manifest.json`；確認專案已安裝 Addressables（2.7.6）與 Unity MCP 套件，與既有 planning 記錄一致
- 2026/01/30：確認 `Packages/` 內有 `Ink` 與 `com.opsidanos.ink`；`com.opsidanos.ink/package.json` 已宣告依賴 Ink integration（1.2.1）與 Addressables（2.7.6）
- 2026/01/30：初步盤點 `Assets/`：有 `AddressableAssetsData/`、`Scene/`、`OpsidanosInk/Demo/` 等資料夾，符合先前 planning 記錄
- 2026/01/30：確認 `Assets/OpsidanosInk/Demo/resource_map.json` 已是 `version: 2`，且每筆都有 `address` 欄位（與 2026-01-27 的 Addressables 記錄一致）
- 2026/01/30：用搜尋確認主要類別存在：`InkResourceMap`、`VNPlayerPresenter`，並找到 Addressables 相關 Editor 腳本 `InkResourceMapAddressablesSetup`
- 2026/01/30：閱讀 `InkResourceMap.cs`：確認 Editor 用 `assetPath + AssetDatabase`；Player build 用 `address + Addressables`（缺 address 會直接報錯提示）
- 2026/01/30：用搜尋確認故事播放與 Tag 路由核心類別存在：`InkStoryEngine`、`InkTagEventRouter`
- 2026/01/30：盤點 `Assets/Editor/Tests/`：有 ResourceMap、Addressables address、CharTag JSON 的 EditMode Tests
- 2026/01/30：閱讀 `Packages/com.opsidanos.ink/README.md`：確認已寫好玩家模式快速開始、Tag Player、ResourceMap（含 Player build 的 Addressables 步驟）；並確認 `Assets/Scene/Test.unity` 存在
- 2026/01/30：補讀 `20260122/OpsidanosInk/task_plan.md` 與 `20260124/vn_layered_ui_refactor/task_plan.md`：兩者狀態皆為 complete（套件清理、UI 分層）
- 2026/01/30：用搜尋確認 `Packages/com.opsidanos.ink/Runtime/Scripts/` 目前沒有存檔/讀檔/倒帶（Rollback）相關實作（缺口仍在）
- 2026/01/30：閱讀 `InkTagEventRouter.cs` 與 `VNPlayerPresenter.cs`：Tag 已有事件分流；Presenter 有 Backlog/Auto/Skip/隱藏 UI/打字機，但目前沒有存讀檔 API
- 2026/01/30：完成缺口整理，並在 `findings.md` 寫出「下一步提案 A：存檔/讀檔/倒帶（Rollback）」的可執行計畫
- 2026/01/31：確認 Phase 5「存檔/讀檔/倒帶」已完成程式碼實作，並用 Unity MCP 把 `Assets/Scene/Test.unity` 的引用接好（`VNPlayerPresenter.saveSystem`、`InkSaveSystem.storyEngine`）
- 2026/01/31：把 `Assets/Scene/Test.unity` 存檔，確保場景接線不會丟失
- 2026/01/31：用 Unity MCP 跑 EditMode 測試 `InkSaveData_RoundTrip_JsonUtility`，結果 Passed（1/1）
- 2026/01/31：把 `Assets/AddressableAssetsData/link.xml` 補回來（避免打包時 Addressables/資源被裁掉）

## 2026/02/06（狀態同步）
- 本任務屬於歷史盤點與提案，已完成並結案。
- 本任務提出的提案 A（存檔/讀檔/倒帶）已在後續任務完成實作並驗收。
