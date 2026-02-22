# 發現與紀錄：專案進度盤點（2026-01-27）

## 1) 這個專案現在「有哪些東西」？
- 原始 ink-unity-integration（`Packages/Ink/`）：提供 Ink 編譯、Ink Player Window、Inspector 工具等。
- 自家 VN Runtime 套件（`Packages/com.opsidanos.ink/`）：用 **Ink + UI Toolkit** 播放 `.json`，並支援 Tag 演出（BGM/SE/BG/立繪/CG/Shake）。
- 示範資料：
  - 測試場景：`Assets/Scene/Test.unity`
  - Demo Ink：`Assets/OpsidanosInk/Demo/story.ink` → 編譯產物 `Assets/OpsidanosInk/Demo/story.json`
  - ResourceMap：`Assets/OpsidanosInk/Demo/resource_map.json`
- 測試（EditMode）：
  - `Assets/Editor/Tests/InkResourceMapTests.cs`
  - `Assets/Editor/Tests/CharTagJsonTests.cs`

## 2) 目前「可以做到什麼」？（玩家模式 Runtime）
### 2-1) 播放骨架
- `InkStoryEngine`：讀 `story.json` → 產生 `StoryOutput`（含文字、選項、Tag、結束狀態）。
- `VNPlayerPresenter`：用 UI Toolkit 顯示名字/內文/選項，並提供操作：
  - 回看（Backlog）
  - Auto / Skip
  - 隱藏 UI / 顯示 UI
  - 點擊推進「兩段式」：忙碌（打字機/動畫）先強制完成；完成後再點才推進
  - 打字機效果 + Auto「完成後再等 1 秒」規則（不與點擊衝突）

### 2-2) Tag 事件與演出
- `InkTagEventRouter`：把 `StoryOutput.ParsedTags` 轉成事件，並可在 Console 輸出 Tag log。
- 演出元件（訂閱 Tag 事件）：
  - `InkTagAudioPlayer`：`bgm` / `se`
  - `InkTagBackgroundPlayer`：`bg`
  - `InkTagCgPlayer`：`cg` / `cg:clear`
  - `InkTagCharacterPlayer`：舊式 `char-left/center/right:<id>`
  - `InkTagCharacterStatePlayer`：推薦 `char:<json>`（actor/expr 分離 + 轉場）
  - `InkTagShakePlayer`：`# shake`

## 3) 資源映射（ResourceMap）現況
- 目前做法：用 `Assets/OpsidanosInk/Demo/resource_map.json` 集中管理 id → `assetPath`（Editor）+ `address`（Player build）。
- `InkResourceMap`：
  - Editor（Play Mode）用 `AssetDatabase.LoadAssetAtPath` 依 `assetPath` 載入（可用）。
  - Player build 用 Addressables 依 `address` 載入；若 `address` 為空會 `Debug.LogError` 指出需要補欄位與設定 Addressable。
- 專案已安裝 Addressables：`Packages/manifest.json` 內有 `com.unity.addressables`。

## 4) PlanningWithFiles 對應關係與完成度（快速總表）
> 這張表用「功能」當主角，幫你快速知道每個資料夾在做什麼。

| 日期 | 任務資料夾 | 對應功能/內容 | 完成度 | 主要對應位置（範例） |
|---|---|---|---|---|
| 20260120 | `TextAdventureEngine` | v1 主計畫（玩家模式/演出/存檔倒帶/Flow Chart） | 部分過時（計畫本身還留在 Phase 4 in_progress） | `PlanningWithFiles/20260120/TextAdventureEngine/task_plan.md` |
| 20260122 | `OpsidanosInk` | 修正套件錯誤/警告（Samples~、Ink 編譯器誤判資料夾、package.json） | complete | `Packages/com.opsidanos.ink/`、`Packages/Ink/Editor/Core/InkEditorUtils.cs` |
| 20260122 | `TextAdventureEngine` | 承接 v1：補齊玩家模式 MVP（回看/Auto/Skip/隱藏 UI/Tag 管線） | complete | `Packages/com.opsidanos.ink/Runtime/Scripts/Story/*`、`.../UI/VNPlayerPresenter.cs` |
| 20260124 | `vn_layered_ui_refactor` | VN UI 分層（UXML/USS 1~13 層）+ 1920x1080 外觀調整 | complete | `Packages/com.opsidanos.ink/Runtime/UI/UXML/VNPlayer.uxml`、`.../USS/VNPlayer.uss` |
| 20260124 | `project_status_next_steps` | 專案盤點 + Tag 管線實作 + 演出綁定（BGM/SE/BG/立繪/CG/Shake） | complete | `Packages/com.opsidanos.ink/Runtime/Scripts/Story/InkTagEventRouter.cs`、`.../Presentation/*`、`Assets/Scene/Test.unity` |
| 20260125 | `resource_map_json_pipeline` | ResourceMap JSON + `InkResourceMap` + 各 Tag Player 串接 + EditMode Test | complete | `Assets/OpsidanosInk/Demo/resource_map.json`、`Packages/com.opsidanos.ink/.../InkResourceMap.cs`、`Assets/Editor/Tests/InkResourceMapTests.cs` |
| 20260125 | `char_tag_state_json` | 推薦 `char:<json>` 狀態化 + 轉場 steps（含 raise/raiseActors）+ 測試 | complete | `Packages/com.opsidanos.ink/.../InkTagCharacterStatePlayer.cs`、`Assets/Editor/Tests/CharTagJsonTests.cs` |
| 20260125 | `advance_click_skip_animations` | 快速連點修正：Busy/ForceComplete + 點擊冷卻 | complete | `Packages/com.opsidanos.ink/.../UI/VNPlayerPresenter.cs`、`.../IAdvanceBlocker.cs` |
| 20260126 | `typewriter_auto_advance` | 打字機 + Auto 1 秒規則（不與點擊衝突） | complete | `Packages/com.opsidanos.ink/.../UI/VNPlayerPresenter.cs` |

## 5) 目前最明顯的「還沒做完」是什麼？
- （功能面）Phase 5：存檔 / 讀檔 / 倒帶（Rollback buffer）。
- （工具面）Phase 6：開發者模式（Editor）Flow Chart。

## 6) 已完成：Player build 支援 ResourceMap（Addressables）
- 已做完的事（2026-01-27）：
  - `InkResourceMap`：Player build 改用 Addressables 依 `address` 載入（Editor 維持用 `assetPath`）
  - `resource_map.json`：每筆資源補上 `address`，並更新 `version=2`
  - `OpsidanosInk.Runtime.asmdef`：加入 `Unity.Addressables` / `Unity.ResourceManager` 參考
  - `com.opsidanos.ink/package.json`：加入 `com.unity.addressables` 依賴
  - 文件：`Packages/com.opsidanos.ink/README.md` 補上 Player build 的設定步驟
- 你接下來要驗證的點：
  - 先把 Demo 用到的資源設成 Addressable，並 Build Addressables
  - 打包一個 Player，進 `Assets/Scene/Test.unity`，確認 BG/立繪/CG/BGM/SE 都正常，Console 沒有 `InkResourceMap` 的錯誤
