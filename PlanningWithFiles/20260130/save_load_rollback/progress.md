# Progress：2026/01/30 Phase 5 存讀檔/倒帶 實作紀錄

## 記錄規則
- 每做完「一小步」（調查或實作），就立刻加一筆
- 有重要發現就同步寫到 `findings.md`

## 工作紀錄
- 2026/01/30：建立本次任務 planning 檔（task_plan/findings/progress）
- 2026/01/30：調查 Ink runtime：確認 `Ink.Runtime.StoryState` 內建 `ToJson()` / `LoadJson()` 可用來存讀玩家進度（`Packages/Ink/InkLibs/InkRuntime/StoryState.cs`）
- 2026/01/30：修改 `InkStoryEngine`：新增 `TryGetStoryStateJson()` / `TryLoadStoryStateJson()`，讓外部能存讀 Ink state（`Packages/com.opsidanos.ink/Runtime/Scripts/Story/InkStoryEngine.cs`）
- 2026/01/30：調查 Demo Ink：同時使用 `char:<json>` 與 `char-left/center/right:<id>`；`Assets/Scene/Test.unity` 也同時有兩種角色播放器，所以存讀檔要涵蓋兩套角色狀態
- 2026/01/30：新增存檔資料格式 `InkSaveData`（含 Ink state / 當句輸出 / 畫面狀態）（`Packages/com.opsidanos.ink/Runtime/Scripts/Save/InkSaveData.cs`）
- 2026/01/30：追加 `InkStoryEngine.EmitExternalOutput()`：讀檔/倒帶時可不推進故事就重新送出 `StoryOutput` 來刷新 UI 與 Tag（`Packages/com.opsidanos.ink/Runtime/Scripts/Story/InkStoryEngine.cs`）
- 2026/01/30：新增 `InkSaveSystem`：用 `StoryOutput` + `InkStoryEngine` state 產生快照，支援「存檔槽位 / 讀檔 / 倒帶」並用 `EmitExternalOutput` 還原（`Packages/com.opsidanos.ink/Runtime/Scripts/Save/InkSaveSystem.cs`）
- 2026/01/30：為了讀檔能「清空殘留狀態」，新增支援：`bg:clear`（背景清空）、`bgm:stop`（停止 BGM）
- 2026/01/30：更新 UI：`VNPlayer.uxml` 新增「存檔/讀檔/倒帶」按鈕，`VNPlayerPresenter` 也已綁定到 `InkSaveSystem`（需要在 Inspector 指定）
- 2026/01/30：新增 EditMode Test：`InkSaveDataTests`（驗證 `InkSaveData` 可被 JsonUtility 正確序列化/反序列化）
- 2026/01/30：更新文件 `Packages/com.opsidanos.ink/README.md`：補上 `InkSaveSystem` 的使用步驟與注意事項（記憶體存檔、Rollback）
- 2026/01/30：補齊新檔案的 `.meta`（Save 資料夾、Save scripts、測試腳本），避免 Unity 重新產生 guid
- 2026/01/30：準備用 Unity MCP 自動接線，但目前 MCP 回報 `instances=0`（沒有 Unity Editor 連線），因此暫時無法用 MCP 修改 `Assets/Scene/Test.unity`，需要先啟動 Unity 專案
- 2026/01/30：再次查 Unity MCP：已偵測到 1 個 Unity instance（Unity 6000.3.2f1），準備開始自動接線
- 2026/01/30：Unity MCP 已切換 active instance：`ink-unity-integration@2d7dbc696b9837d4`
- 2026/01/30：Unity MCP 狀態一開始顯示 stale（暫時不可用），已呼叫 `refresh_unity(wait_for_ready=true)`，Unity 端回報已 ready
- 2026/01/31：Unity Console 出現編譯錯誤：`InkTagBackgroundPlayer.cs` 變數命名衝突（CS0136），已修正（bg:clear 區塊內改用 `background` 變數名）
- 2026/01/31：已請 Unity `refresh_unity(compile=request)` 重新編譯，並再呼叫一次 `refresh_unity(wait_for_ready=true)` 讓 MCP 恢復可用狀態
- 2026/01/31：用 Unity MCP `batch_execute` 成功把場景接線完成：`InkSaveSystem.storyEngine` → `InkStoryEngine`、`VNPlayerPresenter.saveSystem` → `InkSaveSystem`（都掛在 `VNPlayer`）
- 2026/01/31：用 Unity MCP 存檔場景：`Assets/Scene/Test.unity`
- 2026/01/31：用 Unity MCP 跑 EditMode 測試 `OpsidanosInk.Tests.InkSaveDataTests.InkSaveData_RoundTrip_JsonUtility`，結果 Passed（1/1）

## 2026/02/06（狀態同步）
- 本任務（最小版存讀檔/倒帶）已完成並結案。
- 後續延伸已在其他任務落地：讀檔後保留 rollback 歷史、快速連按 rollback 排隊、高壓測試通過。
