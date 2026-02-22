# Findings：Play Mode 驗收（2026/02/04）

日期：2026/02/04  
場景：`Assets/Scene/Test.unity`

## 需求（我這次要確認什麼）
- Play Mode 能正常播放（文字/選項/各 UI 功能）
- Save / Load / Rollback 能還原畫面與狀態
- 倒退/讀檔時 `char` 層級規則相反（含 ForceComplete）
- `git status` 乾淨（忽略 `Samples~` 裡的 `.DS_Store`）

## 驗收結果（勾選/填寫）
| 項目 | 結果 | 備註 |
| --- | --- | --- |
| 基本播放（文字/選項） | 通過 | 2026/02/05 已人工確認：文字可顯示；點擊可前進；選項出現時可點且故事會繼續 |
| Backlog（回看） | 通過 | 2026/02/05 自動測試已驗證可開關；2026/02/06 使用者手動驗收「內容正確」回報通過 |
| Auto | 通過 | 2026/02/05 自動測試已驗證切換；2026/02/06 使用者手動驗收「可自動前進（含 1 秒規則）」回報通過 |
| Skip | 通過 | 2026/02/05 自動測試已驗證切換；2026/02/06 使用者手動驗收「可快速跳」回報通過 |
| 隱藏 UI | 通過 | 2026/02/05 自動測試已驗證可隱藏/顯示；2026/02/06 使用者手動驗收「不影響故事狀態」回報通過 |
| 打字機 + ForceComplete | 通過 | 2026/02/06 使用者手動驗收回報通過 |
| Tag：`bg` | 通過 | 2026/02/06 使用者手動驗收回報通過 |
| Tag：`cg` | 通過 | 2026/02/06 使用者手動驗收回報通過 |
| Tag：`bgm` | 通過 | 2026/02/06 使用者手動驗收回報通過 |
| Tag：`se` | 通過 | 2026/02/06 使用者手動驗收回報通過 |
| Tag：`char`（含 steps） | 通過 | 2026/02/06 已修正 Restore 相關錯誤並通過自動測試；使用者手動驗收 steps 演出回報通過 |
| Tag：`shake` | 通過 | 2026/02/06 使用者手動驗收回報通過 |
| Save → Load 還原 | 通過 | 自動測試已驗證核心邏輯；2026/02/06 使用者手動驗收 UI 操作流程回報通過 |
| Rollback 連按 | 通過 | PlayMode 自動測試已驗證核心邏輯（`Rollback_回到上一句InkState_並送出Restore輸出`）；2026/02/06 已修正快速連按問題並通過高壓測試；使用者手動驗收回報通過 |
| 層級規則：Normal（後做在上） | 通過 | PlayMode 自動測試 `CharLayer_Normal與Restore_固定重排規則相反_且ForceComplete不會破壞` 已驗證 |
| 層級規則：Restore（先做在上） | 通過 | PlayMode 自動測試 `CharLayer_Normal與Restore_固定重排規則相反_且ForceComplete不會破壞` 已驗證 |
| 層級規則：ForceComplete 也一致 | 通過 | PlayMode 自動測試 `CharLayer_Normal與Restore_固定重排規則相反_且ForceComplete不會破壞` 已驗證 |
| git ignore：`Samples~/.DS_Store` 不再出現 | 通過 | 已更新 `.gitignore`，並移除現有 `.DS_Store`；`git status` 不再出現它們 |

## PlayMode 自動測試（Unity Test Runner）
- 測試組件：`Assets/Tests/PlayMode/OpsidanosInk.PlayModeTests.asmdef`
- 測試腳本：`Assets/Tests/PlayMode/OpsidanosInkPlayModeTests.cs`
- 2026/02/04 結果：**3/3 通過**
  - `SaveLoad_還原InkState_並送出Restore輸出`
  - `Rollback_回到上一句InkState_並送出Restore輸出`
  - `CharLayer_Normal與Restore_固定重排規則相反_且ForceComplete不會破壞`
- 2026/02/05 結果：**7/7 通過**（同一組 asmdef；已包含 UI 點擊測試 4 項 + 核心邏輯測試 3 項）
  - UI 點擊：`BacklogButton_可開關Backlog面板`、`AutoButton_可切換文字與樣式`、`SkipButton_可切換文字與樣式`、`HideButton_可隱藏UI_並用ShowUIButton恢復`
  - 核心邏輯：`SaveLoad_還原InkState_並送出Restore輸出`、`Rollback_回到上一句InkState_並送出Restore輸出`、`CharLayer_Normal與Restore_固定重排規則相反_且ForceComplete不會破壞`
- 2026/02/06（再次重跑）結果：**8/8 通過**（新增 `Restore_缺少Appear時_不應產生CharTransitionStepsError` 也通過）

## EditMode 自動測試（Unity Test Runner）
- 2026/02/04 結果：**4/4 通過**
  - `CharTagJsonTests.CharTagJson_DemoStory_SequenceIsParsable`
  - `InkResourceMapAddressablesTests.InkResourceMap_DemoJson_AllEntriesHaveAddress`
  - `InkResourceMapTests.InkResourceMap_DemoJson_AllKindsAreLoadable`
  - `InkSaveDataTests.InkSaveData_RoundTrip_JsonUtility`
- 2026/02/05：已新增 asmdef：`Assets/Editor/Tests/OpsidanosInk.EditModeTests.asmdef`，可用 Unity MCP 只跑這組（避免誤跑套件測試）
- 2026/02/05：用 Unity MCP 跑 `OpsidanosInk.EditModeTests` 結果 **4/4 通過**

## 場景接線（Unity MCP）
- 場景：`Assets/Scene/Test.unity`（已載入）
- 物件：`VNPlayer`
  - 主要元件：`UIDocument`、`InkStoryEngine`、`VNPlayerPresenter`、`InkTagEventRouter`、`InkResourceMap`、`InkSaveSystem`、各 Tag Player
  - `VNPlayerPresenter.saveSystem` → 指到同物件 `InkSaveSystem`
  - `InkSaveSystem.storyEngine` → 指到同物件 `InkStoryEngine`
  - `InkStoryEngine.storyJsonAsset`：`Assets/OpsidanosInk/Demo/story.json`

## Play Mode 啟動觀察（Console）
- 目前只做「啟動是否報錯」與「Tag 管線是否有跑」
- 觀察到：
  - `[OpsidanosInk][Tag] OutputId=1 Tags=speaker:旁白, cg:clear`
  - `[OpsidanosInk][CG] clear`

## 重要觀察
- PlayMode 自動測試已能驗證「核心邏輯」（Ink state + Restore 輸出來源 + char 固定重排 + ForceComplete）
- 2026/02/05 已新增「UI 點擊自動測試」，可驗證 Backlog / Auto / Skip / 隱藏 UI 的「按鈕 → UI 狀態切換」
- 目前仍需要人工驗收的重點：基本播放（選項點擊）、打字機 + ForceComplete、各 Tag 演出目視（bg/cg/bgm/se/char/shake）、以及「存檔/讀檔/倒帶」的 UI 按鈕流程

## 2026/02/06 追加調查：快速倒帶的結構性問題
- 這次用 Unity MCP 再看 Console，最近一輪沒有新的 `char.transition.steps` 紅字錯誤；但「快速倒帶」的風險點仍在結構上。
- 目前 `VNPlayerPresenter.OnClickRollback()` 只做一件事：直接呼叫 `saveSystem.RollbackOnce()`，沒有套用既有的 Busy → ForceComplete 規則（`OnClickContinue/OnClickChoice` 有，Rollback 沒有）。
- `InkSaveSystem.RollbackOnce()` 會立刻 `RemoveAt(last)` 然後 `Restore(...)`；如果連點太快，上一個 Restore 的演出還沒收斂，就會開始下一次倒帶，等於「動作未到終點就進下一個動作」。
- 到最前端時，`rollbackBuffer.Count <= 1` 會直接印 Error（`rollbackBuffer 不足`），這在「連按到盡頭」時會持續噴紅字，影響驗收。
- 結論：現在不是「倒帶壞掉」，而是「缺少專門的倒帶節流/排隊機制」；這和你說的判斷一致。

## 2026/02/06 修正完成：讀檔後可回到存檔前 + 快速倒帶排隊
- `InkSaveSystem` 已改成「存檔保存整條 rollback 歷史」：
  - `SaveToSlot()`：保存 `rollbackHistory + activeRollbackIndex`
  - `LoadFromSlot()`：還原完整 rollback 歷史，不再重置成單一快照
- 新增倒帶能力查詢：
  - `AvailableRollbackSteps`
  - `CanRollback`
  - `TryRollbackOnce()`
- `VNPlayerPresenter` 已改成 Rollback 排隊：
  - 連按會排入 `pendingRollbackRequests`
  - `RollbackCoroutine` 逐步執行，每步先處理 Busy（ForceComplete）
  - 到最前句時停止，不再反覆刷紅字錯誤

## 2026/02/06 測試結果（修正後）
- PlayMode（`OpsidanosInk.PlayModeTests`）：**10 / 10 Passed**
  - 包含新增：
    - `LoadFromSlot_仍可倒帶回存檔點之前`
    - `RollbackButton_快速連按_會排隊倒帶到最前句`
- EditMode（`OpsidanosInk.EditModeTests`）：**4 / 4 Passed**

## 2026/02/06 高壓實測（依使用者要求）
- 先查 Console：沒有 `InkSaveSystem 無法倒帶` 新錯誤（僅有一筆「故事已結束」錯誤，屬於推進到結尾）。
- 新增高壓測試：`RollbackButton_高壓連按_讀檔後仍可回到存檔前且無錯誤`
  - 做法：`LoadFromSlot` 後對 `RollbackButton` 連按超過可倒帶步數（`AvailableRollbackSteps + 30`）
  - 驗收：收斂到 `AvailableRollbackSteps == 0`、`CanRollback == false`、且不出現 `InkSaveSystem 無法倒帶` Error
- 實測結果：
  - PlayMode（`OpsidanosInk.PlayModeTests`）：**11 / 11 Passed**（job_id：`a2b6231fe40a4e69a1abadfe9c193ad6`）
  - EditMode（`OpsidanosInk.EditModeTests`）：**4 / 4 Passed**（job_id：`57091b9990294ef0a35768b342030cca`）

## 如果有失敗：最小修正提案清單
- （填：要改哪個檔案/方法 + 驗收方式）
