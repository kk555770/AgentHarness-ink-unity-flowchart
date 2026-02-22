# Findings：專案現況與計畫對照整理

日期：2026/01/30

## 快速結論（最後整理）
- 已有玩家模式：可播 `story.json`、顯示選項、回看/Auto/Skip/隱藏 UI/打字機
- 已有演出：Tag（bg/bgm/se/cg/char/shake）+ `resource_map.json`（Editor 用 assetPath、Player build 用 address+Addressables）
- 已完成 Phase 5 最小版：存檔/讀檔/倒帶（1 槽 + Rollback），且 `Assets/Scene/Test.unity` 已接線完成
- 目前最大缺口：Phase 6（Editor Flow Chart）與 Phase 7（完整示範驗收）；另外可選加強：存檔寫入硬碟 + 多槽 UI
- 下一步建議：先用 `Assets/Scene/Test.unity` 進 Play Mode 驗收 Save/Load/Rollback，再決定要做「存檔落地」或直接進 Phase 6

## 專案資料夾概覽（初步）
- `README.md`：主要是原始 ink-unity-integration 的功能介紹（編譯/播放/除錯/Ink Player Window 等）
- `Packages/manifest.json`（依賴重點）：
  - `com.unity.addressables`：`2.7.6`（與先前「Player build 用 Addressables」方向一致）
  - `com.coplaydev.unity-mcp`：透過 GitHub 安裝（用來自動調整 Unity 場景/元件）
  - 也包含 `com.unity.test-framework`、`com.unity.inputsystem` 等常用套件
- `Packages/` 內的主要內容：
  - `Packages/Ink/`：原始 ink-unity-integration（工具與 Ink runtime）
  - `Packages/com.opsidanos.ink/`：本專案自家 VN Runtime（UI Toolkit + Tag 演出 + ResourceMap）
  - `Packages/com.opsidanos.ink/package.json`：依賴 `com.inkle.ink-unity-integration@1.2.1` 與 `com.unity.addressables@2.7.6`，Unity 版本標記為 `6000.3`
- `Assets/`（初步看到的重點資料夾）：
  - `Assets/AddressableAssetsData/`：Addressables 設定資料
  - `Assets/Scene/`：場景（先前 planning 指到 `Test.unity`）
  - `Assets/OpsidanosInk/Demo/`：Demo（先前 planning 指到 `story.ink` / `story.json` / `resource_map.json`）

### Demo（Assets/OpsidanosInk/Demo）
- `story.ink` / `story.json`：Demo 故事與編譯產物
- `resource_map.json`：
  - `version: 2`
  - 每筆都有 `assetPath` 與 `address`
  - 類別包含：`bg`、`bgm`、`se`、`cg`、`character`、`actors`

### 主要 Runtime 程式碼位置（先確認「檔案真的存在」）
- `VNPlayerPresenter`：`Packages/com.opsidanos.ink/Runtime/Scripts/UI/VNPlayerPresenter.cs`
- `InkResourceMap`：`Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/InkResourceMap.cs`
- `InkStoryEngine`：`Packages/com.opsidanos.ink/Runtime/Scripts/Story/InkStoryEngine.cs`
- `InkTagEventRouter`：`Packages/com.opsidanos.ink/Runtime/Scripts/Story/InkTagEventRouter.cs`
- Addressables 相關 Editor 輔助：`Packages/com.opsidanos.ink/Editor/InkResourceMapAddressablesSetup.cs`

### ResourceMap 載入規則（從 `InkResourceMap.cs` 確認）
- Editor（`UNITY_EDITOR`）：
  - 用 `assetPath` → `AssetDatabase.LoadAssetAtPath<T>()` 載入
- Player build（非 Editor）：
  - 用 `address` → `Addressables.LoadAssetAsync<T>()` 載入（`WaitForCompletion()`）
  - `address` 為空會直接 `Debug.LogError` 提示要補 `resource_map.json` 與設 Addressable

### 測試（EditMode Tests）
- `Assets/Editor/Tests/InkResourceMapTests.cs`：ResourceMap JSON 基本檢查
- `Assets/Editor/Tests/InkResourceMapAddressablesTests.cs`：驗證 Demo `resource_map.json` 每筆都有 `address`
- `Assets/Editor/Tests/CharTagJsonTests.cs`：驗證 `char:<json>` 相關解析/規格

### 使用方式（從 `Packages/com.opsidanos.ink/README.md` 看到）
- 有「玩家模式」快速開始步驟（要加 `UIDocument`、`InkStoryEngine`、`VNPlayerPresenter` 等）
- 有完整 Tag Player 說明：音訊、背景、CG、角色、Shake
- ResourceMap 明確區分：
  - Editor 用 `assetPath`
  - Player build 用 `address + Addressables`，並要求先 Build Addressables
- 也提供一個 Unity 選單：`OpsidanosInk > Addressables > 套用 Demo ResourceMap（自動勾 Addressable）`

### 場景
- `Assets/Scene/Test.unity`：存在（先前 planning 指到的測試場景）

## PlanningWithFiles 任務清單與對應（逐步整理）
- 目前存在的任務資料夾（依日期）：
  - `20260120/TextAdventureEngine`
  - `20260122/OpsidanosInk`
  - `20260122/TextAdventureEngine`
  - `20260124/project_status_next_steps`
  - `20260124/vn_layered_ui_refactor`
  - `20260125/advance_click_skip_animations`
  - `20260125/char_tag_state_json`
  - `20260125/resource_map_json_pipeline`
  - `20260126/typewriter_auto_advance`
  - `20260127/project_status_refresh`
  - `20260130/project_status_organize`（本次）

### 20260127/project_status_refresh（先前整理成果）
- 這個任務「已經做完」：
  - 已整理 `PlanningWithFiles/` 任務 → 功能 → 主要檔案/資料位置（表格在該任務的 `findings.md`）
  - 已整理專案主要結構：`Packages/Ink` + `Packages/com.opsidanos.ink`
  - 先前選出的「下一步」：Player build 支援 ResourceMap（Addressables）

### 20260124/project_status_next_steps（計畫 + 已實作）
- 這個任務雖然一開始說「只做盤點與提案」，但後面實際把很多東西做完了（Phase 6～10 全部勾選完成）：
  - Tag 事件管線：`InkTagEventRouter` + Tag log
  - 演出綁定：BGM/SE、背景、立繪、CG、Shake
  - ResourceMap（JSON）與各 Tag Player 串接（並列出逐步接線與測試）

### 20260122/OpsidanosInk（套件清理）
- 狀態：complete
- 重點：修正 Console 錯誤/警告（`InkStoryEngine` 命名衝突、Ink 編譯器誤判資料夾、`Samples~`、`package.json`）

### 20260124/vn_layered_ui_refactor（UI 分層）
- 狀態：complete
- 重點：`VNPlayer` UI Toolkit 改成 1～13 分層骨架（不使用 `z-index`），並符合 `UIToolkitSpec.md`

## PlanningWithFiles 完成度總表（更新版）
| 日期 | 任務資料夾 | 重點（用一句話） | 完成度 | 主要對應位置（例） |
| --- | --- | --- | --- | --- |
| 20260120 | `TextAdventureEngine` | v1 主計畫（Runtime + Editor Flow Chart + 存讀檔/倒帶） | Phase 5～7 pending | `PlanningWithFiles/20260120/TextAdventureEngine/task_plan.md` |
| 20260122 | `OpsidanosInk` | 修正套件錯誤/警告（Samples~、編譯器誤判、package.json） | complete | `PlanningWithFiles/20260122/OpsidanosInk/task_plan.md` |
| 20260122 | `TextAdventureEngine` | 補齊 Phase 3：Backlog / AutoSkip / 隱藏 UI / Tag 管線 | complete | `PlanningWithFiles/20260122/TextAdventureEngine/task_plan.md` |
| 20260124 | `vn_layered_ui_refactor` | VNPlayer UI 分層（1～13 層）與 1920x1080 外觀 | complete | `PlanningWithFiles/20260124/vn_layered_ui_refactor/task_plan.md` |
| 20260124 | `project_status_next_steps` | Tag 管線 + 演出（音訊/背景/立繪/CG/Shake）+ ResourceMap 串接 | complete | `PlanningWithFiles/20260124/project_status_next_steps/task_plan.md` |
| 20260125 | `resource_map_json_pipeline` | ResourceMap JSON + `InkResourceMap` + 測試 | complete | `Assets/OpsidanosInk/Demo/resource_map.json` |
| 20260125 | `char_tag_state_json` | `char:<json>` 狀態化 + 轉場 steps + 測試 | complete | `Assets/Editor/Tests/CharTagJsonTests.cs` |
| 20260125 | `advance_click_skip_animations` | 點擊推進：Busy/ForceComplete + 點擊冷卻 | complete | `Packages/com.opsidanos.ink/Runtime/Scripts/UI/VNPlayerPresenter.cs` |
| 20260126 | `typewriter_auto_advance` | 打字機 + Auto（完成後等 1 秒） | complete | `Packages/com.opsidanos.ink/Runtime/Scripts/UI/VNPlayerPresenter.cs` |
| 20260127 | `project_status_refresh` | 專案盤點（含總表）+ Addressables/ResourceMap 驗證方向 | complete | `PlanningWithFiles/20260127/project_status_refresh/findings.md` |
| 20260130 | `save_load_rollback` | Phase 5：存檔/讀檔/倒帶（1 槽 + Rollback） | complete | `PlanningWithFiles/20260130/save_load_rollback/task_plan.md` |
| 20260130 | `project_status_organize` | 本次：更新整理 + 決定下一步 | complete | `PlanningWithFiles/20260130/project_status_organize/` |

## 缺口（Gap）與下一步候選清單
- 目前最明顯還沒做完的部分（以 v1 計畫看）：
  - Phase 6：開發者模式（Editor）Flow Chart
  - Phase 7：示範與驗收（含「開發者模式匯出 .ink → 編譯 .json → 玩家模式可播放」的完整管線）
- 另外（可選加強）：
  - Phase 5 延伸：存檔寫入硬碟（目前最小版存檔只在記憶體，停止 Play 會消失）
  - Phase 5 延伸：多槽位 UI（目前先做 1 槽 + Rollback）

### 先前任務狀態（用來判斷「是不是已經有人做過」）
- `20260120/TextAdventureEngine`
  - Phase 1～4：complete
  - Phase 5～7：pending（核心缺口）
- `20260122/TextAdventureEngine`
  - 只針對 Phase 3（玩家模式 MVP）補齊，狀態：complete

### 目前程式碼是否已經有「存讀檔/倒帶」？
- 已有（最小可用版）：
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Save/InkSaveData.cs`
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Save/InkSaveSystem.cs`
  - `InkStoryEngine` 新增：`TryGetStoryStateJson()` / `TryLoadStoryStateJson()` / `EmitExternalOutput()`
  - UI：`VNPlayer.uxml` TopBar 新增 `存檔/讀檔/倒帶`，由 `VNPlayerPresenter` 呼叫
- 注意：目前存檔在記憶體，停止 Play Mode 會消失（還沒寫入硬碟）

### 存檔/讀檔目前接在哪裡？
- `InkStoryEngine`：
  - 已提供「取出/載入 Ink state」API（JSON）
  - 已提供 `EmitExternalOutput()`：讀檔/倒帶時用來刷新 UI 與 Tag 演出
- `InkSaveSystem`：
  - 監聽 `InkStoryEngine.OutputGenerated`，每句產生一份快照（Ink state + output + presentation snapshot）
  - 提供 `SaveToSlot()` / `LoadFromSlot()` / `RollbackOnce()`
- `VNPlayerPresenter`：
  - TopBar 按鈕呼叫 `InkSaveSystem`
  - 需要 Inspector 引用：`saveSystem`（`Assets/Scene/Test.unity` 已接好）

## 下一步建議（更新：2026/01/31）
### 優先 1：Phase 7「示範與驗收」
- 目標：確認 Save/Load/Rollback 在 Play Mode 真的可用（文字、選項、背景、立繪、CG、BGM 都會回到對的狀態）
- 驗證方式：Play Mode 跑 `Assets/Scene/Test.unity`，中途按「存檔」→ 再推進幾句 → 按「讀檔」；再按「倒帶」

### 優先 2（可選）：Phase 5 延伸「存檔寫入硬碟 + 多槽」
- 目前：存檔只在記憶體（停止 Play 會消失），只有 1 槽
- 若需要：再做寫檔 + 多槽 UI

### 之後：Phase 6「Editor Flow Chart」
- 這個工程大，需要再定規格（節點集合、匯入匯出、版面還原規則）
