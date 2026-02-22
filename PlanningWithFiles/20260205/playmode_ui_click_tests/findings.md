# Findings：PlayMode UI 點擊自動測試 + 一鍵跑完

日期：2026/02/05

## 目前狀態
- 已完成新測試實作
- 已用 Unity MCP 跑完測試（全部通過）
- 已用 `.gitignore` 忽略 `Assets/_Recovery/`（Unity 自動救援暫存場景）
- 已將 `UserSettings/EditorUserSettings.asset` 從版控移除並忽略（它是個人「最近開過哪些場景」清單，很容易一直變）
- 已將 `ProjectSettings/EditorSettings.asset` 從版控移除並忽略（跑 PlayMode 測試會改到它，暫時先當成個人檔）

## UI 元件（從 `VNPlayer.uxml` 盤點）
- `VNRoot`（用來檢查 `vn-ui-hidden`）
- `BacklogButton` / `BacklogCloseButton` / `BacklogPanel`
- `AutoButton` / `SkipButton`
- `HideButton` / `ShowUIButton`

## 新增內容
- PlayMode：`OpsidanosInkPlayModeUiClickTests`
- Editor Menu：`OpsidanosInk/測試/跑全部（Edit → Play，只跑我們的）`
- 終端機一鍵跑：`Tools/run_tests.sh`
  - 會跑：`OpsidanosInk.EditModeTests` → `OpsidanosInk.PlayModeTests`
  - 結果輸出：`Logs/TestResults/OpsidanosInk_EditMode.xml`、`Logs/TestResults/OpsidanosInk_PlayMode.xml`
- GitHub Actions CI：`.github/workflows/CI.yml`
  - 工作流程名稱：`CI`
  - 只跑：`OpsidanosInk.EditModeTests`、`OpsidanosInk.PlayModeTests`
  - 需要設定 `UNITY_LICENSE` secret 才會真的跑 Unity 測試（沒設就先顯示提示並通過）

## 測試結果（Unity MCP）
- EditMode（`OpsidanosInk.EditModeTests`）：4 / 4 Passed（job_id：`f1d9794b6777462db836316d7b5eb343`）
- PlayMode（`OpsidanosInk.PlayModeTests`）：7 / 7 Passed（job_id：`f28e0c3a67534c819335e5ef97560db2`）
  - UI 點擊：Backlog / Auto / Skip / Hide / Show ✓
  - 既有測試：Save / Load / Rollback / CharLayer ✓
- 再次驗證（忽略 `EditorSettings.asset` 後）：跑完測試 `git status` 仍保持乾淨
  - EditMode：4 / 4 Passed（job_id：`89731d96022e44aaaf331f9c5734cd7d`）
  - PlayMode：7 / 7 Passed（job_id：`55e9f48a14d247cd96e0370c66aee014`）
- 2026/02/06 擴充後結果：
  - PlayMode（`OpsidanosInk.PlayModeTests`）：10 / 10 Passed（job_id：`84ba8fc47f3d4ee48f9f109a9cbcf2c1`）
    - 新增通過：`LoadFromSlot_仍可倒帶回存檔點之前`
    - 新增通過：`RollbackButton_快速連按_會排隊倒帶到最前句`
  - EditMode（`OpsidanosInk.EditModeTests`）：4 / 4 Passed（job_id：`bb7b25f5d11e4d27b3a5718b4a216e8a`）

## 2026/02/06 高壓測試（本次新增）
- 新增測試：`RollbackButton_高壓連按_讀檔後仍可回到存檔前且無錯誤`
  - 先推進到存檔點並 `SaveToSlot`
  - `LoadFromSlot` 後，對 `RollbackButton` 做超量點擊（`AvailableRollbackSteps + 30`）
  - 驗收重點：
    - 收斂到 `AvailableRollbackSteps == 0`
    - `CanRollback == false`
    - Console 無 `InkSaveSystem 無法倒帶` Error
- 最終結果：通過
  - PlayMode（`OpsidanosInk.PlayModeTests`）：11 / 11 Passed（job_id：`a2b6231fe40a4e69a1abadfe9c193ad6`）
  - EditMode（`OpsidanosInk.EditModeTests`）：4 / 4 Passed（job_id：`57091b9990294ef0a35768b342030cca`）

## 觀察
- 用 Unity MCP 跑測試時，即使 Unity 沒有在前景（`editor_is_focused=false`），仍可跑完（這是 MCP 內建的「No Throttling」在幫忙）
- PlayMode 測試會把 `ProjectSettings/EditorSettings.asset` 的 `m_EnterPlayModeOptions` 暫時改成 1（DisableDomainReload）
  - 為了避免版控一直變髒，本任務已把它從版控移除並忽略（暫時當成個人檔）

## 補充：為什麼 `EditorSettings.asset` / `EditorUserSettings.asset` 會被改到？
### 舊紀錄（之前踩過同一個雷）
- `PlanningWithFiles/20260125/advance_click_skip_animations/findings.md` 有記錄：
  - 跑錯測試（跑到別人的 package 測試）會讓 Unity 亂閃、產生不該存在的測試場景
  - 同時把 `ProjectSettings/EditorSettings.asset`、`UserSettings/EditorUserSettings.asset` 改掉

### 這次的明確來源（Unity MCP 會動設定）
- Unity MCP 的套件（`com.coplaydev.unity-mcp`）在跑 PlayMode 測試時，會「暫時」把：
  - `EditorSettings.enterPlayModeOptionsEnabled = true`
  - `EditorSettings.enterPlayModeOptions` 加上 `DisableDomainReload`
- 目的很單純：如果 PlayMode 觸發 domain reload，MCP 連線會斷掉，測試結果回不來。
- 正常情況下它會在 `finally` 還原設定；但如果那次測試卡住/中斷，就可能留下變更。
- 位置（可查證）：`Library/PackageCache/com.coplaydev.unity-mcp@6e9594da7766/Editor/Services/TestRunnerService.cs`
  - `EnsurePlayModeRunsWithoutDomainReload(...)`
  - `RestoreEnterPlayModeOptions(...)`

### `EditorUserSettings.asset` 為什麼常變？
- 它裡面的 `RecentlyUsedSceneGuid-*` 是「最近開過的場景清單」，Unity 一更新清單就會寫入，屬於很容易變的個人紀錄。
