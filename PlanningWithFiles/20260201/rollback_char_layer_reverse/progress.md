# Progress：2026/02/01 倒退時 char 層級規則相反

## 記錄規則
- 每做完「一小步」（調查或實作），就立刻加一筆

## 工作紀錄
- 2026/02/01：建立本次任務 planning 檔（task_plan/findings/progress）
- 2026/02/01：調查 `InkTagCharacterStatePlayer`：動作中用 `BringToFront()` 置頂，結束後用固定順序重排（左→中→右）；`ForceComplete` 也會走同一個重排
- 2026/02/01：調查 `StoryOutput` 目前沒有「來源」欄位；`InkStoryEngine` 與 `InkSaveSystem` 都會 `new StoryOutput(...)`（因此倒退輸出也會被當成一般輸出）
- 2026/02/01：實作 `StoryOutputSource`（Normal/Restore），並更新 `InkStoryEngine`（Normal）與 `InkSaveSystem`（Restore）建立輸出時帶上來源
- 2026/02/01：實作 `InkTagCharacterStatePlayer` 倒退模式（Restore）動作中置頂相反：新增隱形 anchor，並用 `PlaceInFront(anchor)` 讓先做的動作被後續步驟往上推到最上層
- 2026/02/01：實作倒退模式的「動作結束後固定重排」相反：`SyncActorElementsToSlotOrder` 支援 reverse，且 `ForceComplete` 也一致套用
- 2026/02/01：用 Unity MCP 進 Play Mode 跑 `Assets/Scene/Test.unity`，確認沒有新編譯錯誤/Runtime Error（Console 僅看到 Tag log）
- 2026/02/01：用 Unity MCP 重新編譯/Refresh，Console 沒有新錯誤；EditMode 測試 `InkSaveData_RoundTrip_JsonUtility` Passed（1/1）
- 2026/02/05：已用 PlayMode 自動測試驗證倒退層級規則（含 ForceComplete）
  - 測試：`CharLayer_Normal與Restore_固定重排規則相反_且ForceComplete不會破壞`
