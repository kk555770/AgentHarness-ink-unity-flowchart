# Findings：專案進度整理（2026/02/01 更新版）

日期：2026/02/01

## 快速結論（最後整理，會隨 Phase 更新）
- 專案已有：玩家模式可播放 Ink `.json`、UI（回看/Auto/Skip/隱藏 UI/打字機）、Tag 演出（bg/bgm/se/cg/char/shake）、ResourceMap（Editor 用 assetPath；Player 用 Addressables address）
- 已有：存檔/讀檔/倒帶（Rollback）最小版（目前是「記憶體存檔」，停止 Play 會消失）
- 進行中：倒退（Rollback/Load）時 `char` 層級規則相反（需要 Play Mode 肉眼驗收）
- 下一步（建議優先）：先做「Play Mode 驗收清單」把存讀檔/倒帶 + 倒退角色層級全部驗完，再進 Phase 6（Editor Flow Chart）

## 專案「已做功能」摘要（用最短的話）
### 玩家模式（對玩家看的）
- 顯示文字與選項
- Backlog（回看）
- Auto（自動下一句）
- Skip（快速跳）
- 隱藏 UI
- 打字機

### 演出（Tag）
- `bg`：背景
- `cg`：CG
- `bgm` / `se`：音效
- `char-left/center/right`（舊式）與 `char:<json>`（新式狀態化 + steps）
- `shake`：震動

### ResourceMap（資源索引）
- `resource_map.json` 同時包含 `assetPath` 與 `address`
- Editor：用 `assetPath` 直接載入
- Player：用 `address + Addressables` 載入（需要先 Build Addressables）

### 存檔/讀檔/倒帶（Rollback）
- `InkSaveSystem`：1 槽位存檔 + Rollback buffer（預設容量 50）
- 倒退/讀檔時會用 `EmitExternalOutput()` 重新丟出「同一句」的 `StoryOutput`，讓 UI/Tag 重新演出還原
- 注意：目前是記憶體存檔（離開 Play Mode 會消失）

## Unity Demo 場景（Unity MCP 已讀到）
- 場景：`Assets/Scene/Test.unity`
- 根物件：`VNPlayer`（含 `UIDocument`、`InkStoryEngine`、`VNPlayerPresenter`、`InkTagEventRouter`、各 Tag Player、`InkResourceMap`、`InkSaveSystem`）
- `VNPlayerPresenter.saveSystem` 已指到同物件上的 `InkSaveSystem`
- `InkSaveSystem.storyEngine` 已指到同物件上的 `InkStoryEngine`
- `InkStoryEngine.storyJsonAsset`：`Assets/OpsidanosInk/Demo/story.json`

## PlanningWithFiles 任務資料夾對照（待補齊）
- 會把每個資料夾整理成：它做了什麼、對應到哪些檔案、現在完成度、還缺什麼

## PlanningWithFiles 對照表（2026/02/01 更新）
| 日期 | 任務資料夾 | 重點（用一句話） | 完成度 | 主要對應位置（例） |
| --- | --- | --- | --- | --- |
| 20260120 | `TextAdventureEngine` | v1 總計畫（玩家模式/演出/存讀檔倒帶/Flow Chart/驗收） | 部分過時（計畫仍寫 Phase 5 pending） | `PlanningWithFiles/20260120/TextAdventureEngine/task_plan.md` |
| 20260122 | `OpsidanosInk` | 套件清理（Samples~、Ink 編譯器誤判、package.json） | 完成 | `PlanningWithFiles/20260122/OpsidanosInk/task_plan.md` |
| 20260122 | `TextAdventureEngine` | 承接 v1：補齊玩家模式 MVP（回看/Auto/Skip/隱藏 UI/Tag 管線） | 完成 | `PlanningWithFiles/20260122/TextAdventureEngine/task_plan.md` |
| 20260124 | `vn_layered_ui_refactor` | VNPlayer UI 分層（1～13 層）與外觀整理 | 完成 | `PlanningWithFiles/20260124/vn_layered_ui_refactor/task_plan.md` |
| 20260124 | `project_status_next_steps` | Tag 管線 + 演出（音訊/背景/立繪/CG/Shake）+ ResourceMap 串接 | 完成 | `PlanningWithFiles/20260124/project_status_next_steps/task_plan.md` |
| 20260125 | `resource_map_json_pipeline` | ResourceMap JSON + `InkResourceMap` + 測試 | 完成 | `PlanningWithFiles/20260125/resource_map_json_pipeline/task_plan.md` |
| 20260125 | `char_tag_state_json` | `char:<json>` 狀態化 + steps 轉場（含 raise/raiseActors）+ 測試 | 完成 | `PlanningWithFiles/20260125/char_tag_state_json/task_plan.md` |
| 20260125 | `advance_click_skip_animations` | 點擊推進：Busy/ForceComplete + 點擊冷卻 | 完成 | `PlanningWithFiles/20260125/advance_click_skip_animations/task_plan.md` |
| 20260126 | `typewriter_auto_advance` | 打字機 + Auto 延遲規則 | 完成 | `PlanningWithFiles/20260126/typewriter_auto_advance/task_plan.md` |
| 20260127 | `project_status_refresh` | 專案盤點（含對照表）+ Addressables/ResourceMap 下一步 | 完成 | `PlanningWithFiles/20260127/project_status_refresh/findings.md` |
| 20260130 | `save_load_rollback` | 存檔/讀檔/倒帶（1 槽 + Rollback） | 完成 | `PlanningWithFiles/20260130/save_load_rollback/task_plan.md` |
| 20260130 | `project_status_organize` | 專案進度整理（含完成度總表 + 下一步） | 完成 | `PlanningWithFiles/20260130/project_status_organize/findings.md` |
| 20260201 | `rollback_char_layer_reverse` | 倒退（Rollback/Load）時 `char` 層級規則相反 | 進行中（待 PlayMode 驗收） | `PlanningWithFiles/20260201/rollback_char_layer_reverse/task_plan.md` |
| 20260201 | `project_status_update` | 本次：把進度整理更新到最新 | 進行中 | `PlanningWithFiles/20260201/project_status_update/task_plan.md` |

## 目前缺口（Gap）（先列出最明顯的）
- Phase 6：開發者模式（Editor）Flow Chart（還沒開始）
- Phase 7：完整示範與驗收清單（還沒完成）
- Phase 5 延伸（可選）：存檔寫入硬碟 + 多槽 UI（目前是記憶體存檔）
- 進行中：倒退時 `char` 層級規則相反（需要 Play Mode 肉眼驗收）

## 版本控管小整理（Git）
- `Assets/AddressableAssetsData/link.xml*` 已加入 `.gitignore`，並準備移出版控（之後 commit 後就不會再一直跳出變更）

## 下一步（建議優先順序）與「怎麼驗收」
### 第 1 步：先做 Play Mode 驗收（最重要）
目標：把「存檔/讀檔/倒帶」與「倒退角色層級相反」全部用眼睛確認過一遍。

1) 開啟場景：`Assets/Scene/Test.unity` → Play  
2) 往前推進幾句（確保背景/CG/BGM/角色都有變過）  
3) 按一次「存檔」  
4) 再往前推進幾句（讓畫面明顯變更）  
5) 按一次「讀檔」  
   - 驗收：背景/CG/BGM/角色/文字/選項 都回到存檔那一句  
6) 再按幾次「倒帶」  
   - 驗收：每倒一次都能回到上一句，且畫面狀態也跟著回去  
7) 找一段有角色重疊的句子，檢查 `char` 層級：  
   - 正常推進（Normal）：後做的在上面  
   - 倒退/讀檔（Restore）：先做的在上面（含 ForceComplete 連點）  

### 第 2 步：驗收通過後，才開始做大的新功能
目前最大的新功能缺口是 Phase 6：Editor Flow Chart。  
（如果你希望先把存檔變成「真的存到硬碟」，也可以先做 Phase 5 延伸。）
