# Findings：Restore 回歸、範圍偏移、原始設計契約

## 1) 這次錯誤到底錯在哪裡

### A. 流程錯誤（範圍管理）
- 兩次主題設計是 GraphToolkit 主線：
  - `PlanningWithFiles/20260208/graphtoolkit_repeated_issues_root_cause/task_plan.md`
  - `PlanningWithFiles/20260211/project_status_assessment/task_plan.md`
- 但後續為了把 PlayMode 拉綠，直接修到 Runtime 對話檔：
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/InkTagCharacterStatePlayer.cs`
- 結論：這是「跨任務邊界修補」，不是 GraphToolkit 設計本體。

### B. 程式錯誤（Restore + steps 的邏輯落差）
- 舊邏輯在 `BuildTransitionSchedule(...)`：
  - 缺必要動作一律 `Debug.LogError`
  - 缺 `Appear` 一律補到排程最後（`schedule.Add(...)`）
- 在 Restore 特例下，若第一步先 `raiseActors`，actor 尚未被 appear 建立，會先觸發錯誤：
  - `char.transition.steps 缺少必要動作：Appear`
  - `raiseActors 指定 actor=... 但此時畫面上不存在`
- 這正是失敗測試要擋的場景：
  - `Assets/Tests/PlayMode/OpsidanosInkPlayModeTests.cs:205`

### C. 版本錯誤（「說已修」與「實際程式」不一致）
- `git log -- Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/InkTagCharacterStatePlayer.cs` 只看到：`74c26b4`、`6e16382`、`b3d9090`。
- 2026/02/06 那次新增了測試（`896d997`），但該 commit 沒有包含 `InkTagCharacterStatePlayer.cs`。
- `PlanningWithFiles/20260206/phase5_phase6_unified/progress.md:59` 明確記錄：
  - `已還原 InkTagCharacterStatePlayer.cs`
- 結論：當時「修復描述」和「實際提交內容」分離，導致後面回歸。

## 2) 我這次修改到底修了什麼

修改檔案：
- `Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/InkTagCharacterStatePlayer.cs`

修改段落：
- `BuildTransitionSchedule(...)`，約 `1191` 附近。

實際變更：
1. 新增 `isRestore` 判斷。
2. Restore 缺必要動作時，訊息層級從 `Debug.LogError` 改成 `Debug.Log`。
3. Restore 缺 `Appear` 時，從「補在最後」改為「插在最前面（Insert(0, ...)）」。

可驗證結果：
- 目標單測通過：
  - `OpsidanosInk.Tests.OpsidanosInkPlayModeTests.Restore_缺少Appear時_不應產生CharTransitionStepsError`
- Console 觀察到的是一般 Log，不是 Error：
  - `[OpsidanosInk] char.transition.steps 缺少必要動作：Appear（Restore 模式會自動補齊排程）。OutputId=9101`

## 3) 原本的對話設計是什麼（資料鏈）

### A. 輸出來源契約
- 正常推進：`InkStoryEngine` 建立 `StoryOutputSource.Normal`
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Story/InkStoryEngine.cs:172`
- 讀檔/倒帶：`InkSaveSystem` 建立 `StoryOutputSource.Restore`
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Save/InkSaveSystem.cs:395`

### B. 路由契約
- `InkTagEventRouter` 把 `char` tag 分流給 `CharacterTagReceived`
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Story/InkTagEventRouter.cs:156`

### C. 角色演出契約
- `InkTagCharacterStatePlayer` 解析 `char:<json>` 的 `transition.steps`。
- 原始設計（2026/01/25）核心：
  - steps 可控制 `appear/move/disappear`
  - `raise/raiseActors` 只管層級
  - 若缺必要動作，系統會補，但視為錯誤訊號（紅字）
- Restore 設計（2026/02/01）原本聚焦在「層級規則相反」：
  - 動作中：Normal 後做在上；Restore 先做在上
  - 結束重排：Normal 左中右；Restore 右中左
  - `ForceComplete` 也走同規則

### D. 這次修正是否改動原始設計
- 有改動「錯誤等級」契約：Restore 缺必要動作不再是 Error。
- 有改動「排程順序」契約：Restore 缺 `Appear` 不再補尾，而是前插。
- 這屬於「Restore 子規格補強」，不是純修測試。

## 4) 時間線（可追證）

1. 2026/01/25：`transition.steps` 與 `raise/raiseActors` 上線。
2. 2026/02/01：Restore/Normal 層級規則分流上線（但未改缺動作補齊策略）。
3. 2026/02/05：發現快速倒帶時會噴 `missing Appear` / `raiseActors` 錯誤（Planning 記錄）。
4. 2026/02/06：加入保護測試 `Restore_缺少Appear...`（commit `896d997`）。
5. 同一輪記錄有 `已還原 InkTagCharacterStatePlayer.cs`（`phase5_phase6_unified/progress.md:59`）。
6. 2026/02/11：GraphToolkit 驗收時重跑 PlayMode，該測試再次失敗。
7. 2026/02/11：目前工作樹重新補上 Runtime 修正（尚未提交）。

## 5) 風險評估（不要樂觀）

1. 目前修正還沒 commit，隨時可能再被清理步驟沖掉。
2. Restore 改成 Log 後，若真有資料品質問題，可能比以前更難從紅字第一時間發現。
3. 測試只保護「char.transition.steps 類錯誤字串」，沒有直接驗證最終畫面角色順序與 actor 存在性。
4. GraphToolkit 任務若再把「全綠測試」當提交條件，仍可能誘發下一次跨範圍修補。

## 6) 最小結論
- 這次不是只有一個 bug。
- 同時有三層問題：
  1) 範圍治理失誤（GraphToolkit 任務混入 Runtime 修補）
  2) Restore 補步驟邏輯與測試契約不一致
  3) 歷史紀錄與 commit 內容脫鉤（說修了，但提交沒有）

## 7) 補查：既定對話流程（歷史計畫逐條，不再用摘要代替）

### A. 玩家模式主流程（v1 主計畫）
- 來源：`PlanningWithFiles/20260120/TextAdventureEngine/task_plan.md`
- 明定內容：
  - Runtime 只讀 `.json` 播放，不依賴 Flow Chart 資產。
  - 玩家模式功能要包含：文字/選項、Backlog、Auto/Skip、隱藏 UI、Tag 管線。
  - 存檔/倒帶要綁在 `PresentationState`，避免狀態分散。

### B. 點擊推進兩段式（Busy -> ForceComplete -> Continue）
- 來源：`PlanningWithFiles/20260125/advance_click_skip_animations/task_plan.md`
- 明定內容：
  - 動畫或打字機未完成時，點擊只做 `ForceComplete`。
  - 完成後再點，才 `Continue`。
  - 進下一句前先清掉上一句殘留動畫。

### C. 打字機 + Auto 1 秒規則（與點擊不衝突）
- 來源：`PlanningWithFiles/20260126/typewriter_auto_advance/task_plan.md`
- 明定內容：
  - Auto 只在「打字機 + 動畫完成」後，再等 1 秒推進。
  - Auto 等待期間若點擊，這一次 Auto 作廢，交由點擊規則接管。
  - 加入同幀只推進一次，避免 Auto/點擊雙觸發。

### D. 存檔/讀檔/倒帶主流程
- 來源：`PlanningWithFiles/20260130/save_load_rollback/task_plan.md`
- 明定內容：
  - Save/Load/Rollback 是對話主線必要能力，不是附屬功能。
  - 讀檔後要回到同句，並還原背景/角色/BGM/CG 等畫面狀態。
  - 最小版先 1 槽，後續再擴多槽 + Auto 槽。

### E. Restore 層級規則（對話演出規格）
- 來源：`PlanningWithFiles/20260201/rollback_char_layer_reverse/task_plan.md`
- 明定內容：
  - Normal：後做在上。
  - Restore：先做在上。
  - `ForceComplete` 也要套同一規則，不能切到另一套排序。

### F. 驗收流程（不是只看測試綠燈）
- 來源：`PlanningWithFiles/20260204/playmode_acceptance_checklist/task_plan.md`
- 明定內容：
  - 要做 PlayMode 清單驗收：基本播放、Tag 演出、Save/Load、Rollback、Restore 層級規則。
  - 驗收場景固定 `Assets/Scene/Test.unity`，並要求用 Unity MCP 查 Console/接線，避免猜測。

### G. 我上一版確實漏查的點
- 我上一版只展開 `Restore_缺少Appear...` 失敗與 runtime 這一段，沒有把 A~F 整條既定流程明列出來。
- 這導致回覆看起來像「只修一個測試」，而不是對照既有對話系統規格在判斷偏移。

## 8) 補查：場景資料鏈實測（Unity MCP）

### A. 目前活動場景
- `Assets/Scene/Test.unity`

### B. `VNPlayer` 元件鏈（實際掛載）
- `UIDocument`
- `InkStoryEngine`
- `VNPlayerPresenter`
- `InkTagEventRouter`
- `InkTagBackgroundPlayer`
- `InkTagAudioPlayer`
- `InkTagCgPlayer`
- `InkTagCharacterPlayer`（舊式）
- `InkTagCharacterStatePlayer`（新式）
- `InkTagShakePlayer`
- `InkResourceMap`
- `InkSaveSystem`

### C. 主要接線（實際屬性）
- `VNPlayerPresenter.storyEngine -> InkStoryEngine`
- `VNPlayerPresenter.saveSystem -> InkSaveSystem`
- `VNPlayerPresenter.advanceBlockers` 包含 `InkTagCharacterStatePlayer`
- `InkTagEventRouter.storyEngine -> InkStoryEngine`
- `InkSaveSystem.storyEngine -> InkStoryEngine`
- `InkStoryEngine.storyJsonAsset -> Assets/OpsidanosInk/Demo/story.json`

### D. 這條資料鏈代表的風險
- 場景同時存在 `InkTagCharacterPlayer` 與 `InkTagCharacterStatePlayer`，如果後續規格未明確限定主線播放器，觀察層級行為時容易判讀混淆。
- 這也是你之前要求「把程式與場景資料鏈串清楚」的核心原因；只看程式碼不夠，必須對照場景實掛載。
