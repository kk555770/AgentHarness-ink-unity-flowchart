# 進度日誌：專案進度整理、優化方案與下一步

## 2026-02-11

### P-001 初始化
- 建立任務資料夾：`PlanningWithFiles/20260211/project_progress_optimization_audit`
- 建立檔案：`task_plan.md`、`findings.md`、`progress.md`

### P-002 Git 基線
- 指令：`git rev-parse --abbrev-ref HEAD && git rev-parse --short HEAD && git status --short`
- 結果：分支 `arcumit/CodexInk`、HEAD `dc8a443`、工作樹無未提交檔案

### P-003 專案規模盤點
- 指令：`ls -la` 與檔案量統計
- 結果：`Assets` 284 檔、`Packages` 951 檔、`ProjectSettings` 29 檔

### P-004 歷史任務目錄全掃描
- 指令：`find PlanningWithFiles -maxdepth 2 -mindepth 2 -type d`
- 結果：共 23 個任務資料夾（含本輪新建任務）

### P-005 讀取 AGENTS 規範
- 指令：`cat AGENTS.md`
- 結果：已確認修改流程、語言規範、禁止防禦性編碼、UI/Graph 規格參考義務

### P-006 讀取 UI Toolkit 規格
- 指令：`cat UIToolkitSpec.md`
- 結果：已確認 UXML/USS 允許元素、屬性與限制（Unity 6000.3.x）

### P-007 讀取 GraphToolkit 規格
- 指令：`cat GraphToolkitSpec.md`
- 結果：已確認 GraphToolkit 是 Editor-only、experimental，且 asmdef 與 Graph/Node 定義規則有強制條件

### P-008 讀取對話主線早期計畫（第一批）
- 指令：讀取 `20260120`、`20260125`、`20260126`、`20260130` 的 `task_plan.md`
- 結果：已取得對話系統核心規格（點擊兩段式、打字機+Auto、Save/Load/Rollback）

### P-009 讀取對話主線計畫（第二批）
- 指令：讀取 `20260201`、`20260204`、`20260205` 的 `task_plan.md`
- 結果：已補齊 Restore 層級規格與 PlayMode 驗收/自動測試目標

### P-010 讀取 GraphToolkit 與近期狀態計畫
- 指令：讀取 `20260206`、`20260208`、`20260211/project_status_assessment`、`20260211/dialogue_restore_scope_audit` 的 `task_plan.md`
- 結果：已取得主計畫承諾、風險項、S1~S5 收斂敘述與對話稽核範圍

### P-011 提交時間線比對（關鍵三筆）
- 指令：`git show` 與 `git show --stat` 比對 `896d997`、`84cfd67`、`dc8a443`
- 結果：三筆提交的檔案範圍與統計已完整取得

### P-012 模組邊界盤點（manifest + asmdef）
- 指令：讀取 `manifest.json`、全專案 `asmdef`、自有四個組件定義
- 結果：已確認 Runtime/FlowChartEditor/EditModeTests/PlayModeTests 的依賴邊界

### P-013 程式檔版圖盤點
- 指令：列出 Runtime / Editor / PlayMode 測試 `.cs`，統計外部導航測試檔數
- 結果：
  - Runtime 自有 `.cs`：16
  - Editor 自有 `.cs`：19
  - PlayMode 自有測試 `.cs`：2
  - 外部 `com.unity.ai.navigation` 測試 `.cs`：38

### P-014 Unity 場景與 Build Settings 盤點
- 指令：`rg -n "m_Scenes|enabled|path" ProjectSettings/EditorBuildSettings.asset`、`ls -la Assets/*.unity Assets/Scene/*.unity`、`rg -n "OffMeshLinkScene|Test.unity|Basic Demo" -S ProjectSettings Assets`
- 結果：
  - `EditorBuildSettings` 啟用場景為 `Assets/Scene/Test.unity`
  - `Assets/Ink/Demos/Basic Demo/Basic Demo.unity` 在 BuildSettings 中但為 disabled
  - `Assets/OffMeshLinkScene.unity` 存在於專案，但目前不在 BuildSettings 啟用清單
  - 自有 PlayMode 測試訊息明確綁定 `Assets/Scene/Test.unity` 與根物件 `VNPlayer`

### P-015 `Test.unity` 的 `VNPlayer` 資料鏈抽取（場景層）
- 指令：`sed -n '130,700p' Assets/Scene/Test.unity`、`rg -o "guid: [0-9a-f]{32}" Assets/Scene/Test.unity | ...`、GUID 對應 `.meta` 搜尋
- 結果：`VNPlayer` 同一物件上已串接核心元件：
  - `InkStoryEngine` -> `Assets/OpsidanosInk/Demo/story.json`
  - `UIDocument` -> `Packages/com.opsidanos.ink/Runtime/UI/UXML/VNPlayer.uxml`
  - `InkResourceMap` -> `Assets/OpsidanosInk/Demo/resource_map.json`
  - `VNPlayerPresenter` -> `uiDocument/storyEngine/saveSystem/advanceBlockers`
  - `InkSaveSystem` -> `storyEngine`
  - `InkTagEventRouter` -> `storyEngine`
  - 五個 Tag Player（Audio/Background/Cg/Character/Shake/CharacterState）-> `tagEventRouter` 與 `uiDocument/resourceMap`

### P-016 對話推進規則實作核對（`VNPlayerPresenter` + PlayMode 測試）
- 指令：
  - `rg -n "Advance|Back|Rollback|Restore|ForceComplete|cooldown|Auto|typewriter" Packages/com.opsidanos.ink/Runtime/Scripts/UI/VNPlayerPresenter.cs`
  - `sed -n '380,620p' .../VNPlayerPresenter.cs`
  - `sed -n '840,1338p' .../VNPlayerPresenter.cs`
  - `sed -n '1,520p' Assets/Tests/PlayMode/OpsidanosInkPlayModeTests.cs`
  - `sed -n '1,360p' Assets/Tests/PlayMode/OpsidanosInkPlayModeUiClickTests.cs`
- 結果：
  - `OnClickContinue`/`OnClickChoice` 皆是「先壓制 Auto 倒數 -> Busy 時先 ForceComplete + 進冷卻 -> 再點才 Continue/Choose」
  - `TryForceCompleteIfBusy` 把打字機與 `IAdvanceBlocker` 都納入 Busy 判斷
  - `AutoSkipCoroutine` 實作「Busy 結束後再等待 `autoDelaySeconds`（場景值 1 秒）」
  - 目前 PlayMode 測試主力覆蓋 Save/Load/Rollback、Restore 層級、多槽；UI 點擊測試主力覆蓋按鈕接線與狀態切換

### P-017 UI 與故事資源鏈核對（UXML/USS/Story/ResourceMap）
- 指令：`cat VNPlayer.uxml`、`cat VNPlayer.uss`、`cat resource_map.json`、`head -n 160 story.json`
- 結果：
  - `ContinueButton`、`AutoButton`、`SkipButton`、多槽與 `RollbackButton` 都存在於 UXML，名稱與 Presenter 查找一致
  - `story.json` 內含 `bg/bgm/se/cg/char` tags，`resource_map.json` 有對應 id -> assetPath/address
  - 場景的 `InkResourceMap` 與各 Tag Player 綁定一致，形成 `StoryOutput tags -> Router -> Player -> UI/Audio` 連線

### P-018 GraphToolkit 主線現況核對（Editor 組件 + 測試）
- 指令：列檔 `Assets/Editor/FlowChart/GraphToolkit`、讀取 `Assets/Editor/Tests/*.cs`、讀取 asmdef
- 結果：
  - `OpsidanosInk.FlowChartEditor`（Editor-only）與 `OpsidanosInk.EditModeTests`（Editor-only）存在，GraphToolkit 測試檔完整
  - EditMode 測試包含 `InkFlowChartGraphSmokeTests`、`InkFlowChartExportTests`、`InkFlowChartImportTests`
  - 匯入/匯出測試主體是 GraphToolkit 圖資產轉換，不是 Runtime 對話推進

### P-019 調查錯誤與修正（路徑判斷）
- 錯誤：誤用 `Assets/Tests/EditMode` 搜尋，實際目錄不存在
- 修正：改以 `Assets/Editor/Tests` 與 `rg --files | rg "EditMode|GraphToolkit"` 重新定位
- 結果：已完成正確路徑核對，後續調查基於正確目錄

### P-020 關鍵提交與範圍再核對（`896d997` / `84cfd67` / `dc8a443`）
- 指令：`git show --name-status --stat --oneline <commit>`
- 結果：
  - `dc8a443`：只動 GraphToolkit 與 Editor 測試（另含 `.gitignore`），沒有 Runtime 對話程式修改
  - `84cfd67`：GraphToolkit 匯出 MVP 與 Editor 測試
  - `896d997`：同時動 Runtime 對話/PlayMode 測試與舊 FlowChart Editor 匯出匯入，屬混合提交

### P-021 OffMeshLinkScene 影響範圍核對
- 指令：`rg -n "OffMeshLinkScene|OffMesh|NavMesh" -S Assets Packages/com.opsidanos.ink ProjectSettings`
- 結果：
  - `Assets/OffMeshLinkScene.unity` 只在場景資產層存在，未被 BuildSettings 啟用
  - 自有 Runtime/PlayMode 測試未綁定 `OffMeshLinkScene`
  - 目前對話主驗收鏈仍是 `Assets/Scene/Test.unity`

### P-022 測試矩陣盤點（數量與類型）
- 指令：列舉 `Assets/Editor/Tests/*.cs` 與 `Assets/Tests/PlayMode/*.cs` 的 `[Test]/[UnityTest]`
- 結果：
  - Editor 測試：`CharTagJson`、`ResourceMap`、`SaveData`、`GraphToolkit 匯入匯出/Smoke`
  - PlayMode 測試：`OpsidanosInkPlayModeTests` 6 項、`OpsidanosInkPlayModeUiClickTests` 7 項
  - 缺口待補：`ContinueButton` 兩段式（第一次 ForceComplete + 冷卻 + 第二次前進）尚無直接 UI 回歸測試

### P-023 優化方案與下一步草案整理
- 動作：根據 F-016 ~ F-024 產出優化方案草案與分段執行順序
- 結果：草案聚焦三個主軸
  1) 對話兩段式點擊節奏測試補齊（PlayMode）
  2) GraphToolkit 與 Runtime 驗收分流（任務與測試對齊）
  3) `VNPlayer` 場景單點耦合風險降低（拆可驗證節點）

## 2026-02-12

### P-024 Git 追證：關鍵提交統計與範圍
- 指令：`git show --stat --oneline 896d997`、`git show --stat --oneline 84cfd67`、`git show --stat --oneline dc8a443`
- 結果：提交統計數字與 `findings.md` 的 F-011 完全一致；且 `dc8a443` 不含 Runtime 變更（只有 GraphToolkit + Editor 測試 + `.gitignore`）。

### P-025 場景資料鏈追證：`VNPlayer` 綁定的 GUID 對應資產路徑
- 指令：以 `rg` 定位 `Assets/Scene/Test.unity` 內 `VNPlayer` 的 `storyJsonAsset/resourceMapJson/sourceAsset` GUID，並用對應 `.meta` 檔確認路徑。
- 結果：`VNPlayer` 的 `story.json/resource_map.json/VNPlayer.uxml` 綁定與本輪整理一致。

### P-026 數量口徑追證：Editor `.cs` 與外部導航測試
- 指令：`find Assets/Editor -name '*.cs' | wc -l`、`find Assets/Editor -name '*.cs' -not -path 'Assets/Editor/Tests/*' | wc -l`、`find Packages/com.unity.ai.navigation/Tests -name '*.cs' | wc -l`
- 結果：`Assets/Editor` 的 `.cs` 共 19（含 `Assets/Editor/Tests`）；非測試腳本為 12；`com.unity.ai.navigation` 測試檔數為 38。

### P-027 實作：ContinueButton 兩段式點擊節奏回歸測試（PlayMode）
- 動作：在 `Assets/Tests/PlayMode/OpsidanosInkPlayModeUiClickTests.cs` 新增 2 個測試（Normal/Restore），並補齊建立 Busy 與禁止前進的 helper。
- 結果：可直接驗證「Busy 第一次點擊只 ForceComplete 不前進、冷卻內不前進、冷卻後第二次點擊才前進」。

### P-028 MCP 測試：只跑 UI 點擊群組（PlayMode）
- 動作：透過 Unity MCP 以 `group_names=OpsidanosInk.Tests.OpsidanosInkPlayModeUiClickTests` 跑 PlayMode 測試。
- 結果：共 9 項全數通過（包含新補的 2 個 ContinueButton 節奏測試）。

### P-029 MCP 重跑：定位 `OpsidanosInk.PlayModeTests` 的失敗項
- 動作：透過 Unity MCP 單跑測試 `OpsidanosInk.Tests.OpsidanosInkPlayModeTests.Restore_缺少Appear時_不應產生CharTransitionStepsError`。
- 結果：測試失敗，原因是出現未處理的 Error log：
  - `[OpsidanosInk] char.transition.steps 缺少必要動作：Appear（我會補到排程最後面，確保畫面狀態正確）。OutputId=9101`
  - Unity Test Framework 要求必須用 `LogAssert.Expect` 明確宣告「我就是要看到這筆 Error」，否則會判定測試失敗。

### P-030 追證：失敗測試用的 `steps` 其實來自 Demo story.ink 的「狀態相依」範例
- 動作：搜尋 `Assets/OpsidanosInk/Demo/story.ink` 的 `# char:` 範例。
- 結果：
  - 範例存在 `steps=[{raiseActors:["alice"]},{actions:["move"],raise:false}]` 這種「不含 appear」的寫法。
  - 這種寫法在「上一句 alice 已存在」時合理，但若在「空畫面」直接套用，就必然觸發 `requiredActions=Appear` 的 Error log。

### P-031 修正：將 Restore 的 steps 測試改為「可重播」輸出
- 動作：修改 PlayMode 測試，把 `Restore_缺少Appear...` 改為 `Restore_可重播CharJson...`，並把測試用 `charJson.steps` 補齊 `appear`（避免空畫面 Restore 缺必要動作）。
- 結果：MCP 單跑該測試通過；再跑整個 `OpsidanosInk.PlayModeTests` 15/15 全數通過。

### P-032 文件化：建立「開發者模式輸出契約（可重播/快照式）」文件
- 動作：新增文件 `Documentation/DeveloperModeOutputContract.md`，明確定義 Flow Chart / GraphToolkit 輸出 Ink 時 `char.transition.steps` 的最小契約。
- 結果：把「不要依賴 Runtime 補齊缺少動作」落為文件規範，並把 Ink 基準層對照、Graph 子集合與「圖是權威、不可藏流程」寫死，避免規範依現有進度縮水。

### P-033 測試鎖定：新增 PlayMode 契約測試
- 動作：在 `OpsidanosInkPlayModeTests` 新增 `CharTransitionSteps_可重播契約_可從任意狀態收斂`，同時覆蓋：
  - 空畫面套用（必須能 appear）
  - 非空畫面套用（必須能 move + disappear）
- 結果：MCP 重跑 `OpsidanosInk.PlayModeTests` 16/16 全數通過。

## 2026-02-13

### P-034 文件維護：刪除易誤解段落並修正用詞
- 動作：更新 `Documentation/DeveloperModeOutputContract.md`，精簡重複/易誤解敘述（特別是 `transition.steps` 與 JSON 跳脫說明）。
- 結果：文件規則不變，但更難被誤讀成「Runtime 固定要求」或「raiseActors 可以取代必要動作」。
