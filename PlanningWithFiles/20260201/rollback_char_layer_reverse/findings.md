# Findings：倒退（Rollback/Load）時 char 層級規則相反

日期：2026/02/01

## 快速結論（最後整理）
- 已新增 `StoryOutputSource`（Normal/Restore）：一般推進由 `InkStoryEngine` 標 Normal，倒退/讀檔由 `InkSaveSystem` 標 Restore
- `InkTagCharacterStatePlayer` 已完整覆蓋兩條路徑：
  - 動作中置頂：Normal 用 `BringToFront()`；Restore 用隱形 anchor + `PlaceInFront(anchor)`，讓先做的動作留在上面
  - 動作結束後固定重排：Normal 左→中→右；Restore 右→中→左
  - `ForceComplete` 也依同樣的 Restore/Normal 規則重排，避免點擊後順序跳回去

## 需要被「完整覆蓋」的規則點
- 動作中置頂（steps 每一步的 raise / raiseActors）
- 動作結束後的固定重排（目前是左→中→右）
- `ForceComplete`（使用者點擊強制完成動畫）也要一致

## 目前程式碼位置（已確認）
- `StoryOutput`：`Packages/com.opsidanos.ink/Runtime/Scripts/Story/StoryOutput.cs`
- 一般輸出建立：`Packages/com.opsidanos.ink/Runtime/Scripts/Story/InkStoryEngine.cs`（`new StoryOutput(...)`）
- 倒退/讀檔輸出建立：`Packages/com.opsidanos.ink/Runtime/Scripts/Save/InkSaveSystem.cs`（`new StoryOutput(...)`）
- char 層級規則本體：`Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/InkTagCharacterStatePlayer.cs`
  - 動作中置頂：`BringActorsToFrontPreserveOrder()` → `VisualElement.BringToFront()`
  - 結束固定重排：`SyncActorElementsToSlotOrder()`（左→中→右）
  - 強制完成：`ForceCompleteToEnd()` → `ApplyFinalState()` → `SyncActorElementsToSlotOrder()`

## 初步設計
- 在 `StoryOutput` 加 `Source`（Normal / Restore）
- Restore 模式下：
  - 建立一個「隱形 anchor」插在角色層裡當分隔點
  - 每一步要 raise 的角色都插到 anchor 後面（靠近 anchor 的在下面），讓「先做的」被後面的步驟往上推到最上層
  - 同一個角色如果在更後面的步驟又被 raise，應該不要被往下拖（避免破壞「先做的在上面」）

## 驗收清單（Play Mode）
- 正常推進（Normal）：後做的動作在上面
- 倒退/讀檔（Restore）：先做的動作在上面
- ForceComplete（連點強制完成）：
  - Normal 不變
  - Restore 不變（不會因為強制完成就跳回 Normal 的重排）

## 如果驗收失敗（優先排查）
- 倒退/讀檔是否真的走到 `StoryOutputSource.Restore`
- 場景是否同時存在「舊式角色播放器」與「新式角色播放器」造成你以為是同一套在排序
- `PlaceInFront(anchor)` 的 anchor 是否存在於角色層，且沒有被其他重排邏輯移走
