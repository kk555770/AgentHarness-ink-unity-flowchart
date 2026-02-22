# Findings：存檔 / 讀檔 / 倒帶（Rollback）

日期：2026/01/30

## 快速結論（最後整理）
- 已完成最小版 Phase 5：
  - `InkStoryEngine` 可取得/載入 Ink state（JSON），並支援「外部送出輸出」用於讀檔刷新
  - `InkSaveSystem`：支援 1 個存檔槽位 + Rollback（最多 N 句）
  - UI 已新增 `存檔 / 讀檔 / 倒帶` 按鈕（TopBar），並由 `VNPlayerPresenter` 呼叫 `InkSaveSystem`
  - 為了避免讀檔殘留狀態，新增支援：`bg:clear`、`bgm:stop`
  - 已加 EditMode Test（`InkSaveDataTests`）與更新 `Packages/com.opsidanos.ink/README.md`

## Ink State（ToJson / LoadJson）調查
- Ink runtime 內建就有「存檔 JSON」功能：
  - `Ink.Runtime.StoryState.ToJson()`：把目前故事狀態輸出成 JSON
  - `Ink.Runtime.StoryState.LoadJson(string json)`：把 JSON 載回去（會觸發 `onDidLoadState`）
  - 位置：`Packages/Ink/InkLibs/InkRuntime/StoryState.cs`
- 另外 `Ink.Runtime.Story.ToJson()` 是「整個故事內容」的 JSON（不是玩家進度用）
  - 位置：`Packages/Ink/InkLibs/InkRuntime/Story.cs`

## 現有架構的接點（要插在哪）
- `InkStoryEngine`（Runtime）：
  - 已新增存讀 API：`TryGetStoryStateJson()` / `TryLoadStoryStateJson()`
  - 後續 `InkSaveSystem` 可以直接呼叫這兩個方法來存讀 Ink state
  - 已新增 `EmitExternalOutput()`：讀檔/倒帶時可以「不推進故事」就刷新 UI 與 Tag 演出
- Demo 內容同時有兩種角色 Tag：
  - 新式：`char:<json>`（含 `char:clear`）
  - 舊式：`char-left:<id>` / `char-center:<id>` / `char-right:<id>`
- `Assets/Scene/Test.unity` 同時掛了：
  - `InkTagCharacterPlayer`（舊式 char-left/center/right）
  - `InkTagCharacterStatePlayer`（新式 char JSON）
  - 所以「存讀檔的角色狀態」要能同時涵蓋兩種

## 存檔資料格式（草案）
- `InkSaveData`（`Packages/com.opsidanos.ink/Runtime/Scripts/Save/InkSaveData.cs`）：
  - `inkStateJson`：用 `story.state.ToJson()` 取得的玩家進度 JSON
  - `output`：當句 UI 需要的資料（speaker/line/choices）
  - `presentation`：畫面狀態（bg/bgm/cg/char），同時包含：
    - 新式 `char`（`charValue`：JSON 或 clear）
    - 舊式 `char-left/center/right`（三個 id）

## `InkSaveSystem` 作法（重點）
- 監聽 `InkStoryEngine.OutputGenerated`：
  - 每一句出來就用 `TryGetStoryStateJson()` 取得 Ink state
  - 同時從 `StoryOutput.ParsedTags` 記錄「最後一次看到的畫面狀態」（bg/bgm/cg/char…）
  - 產生一份快照推進 rollbackBuffer（最多 N 筆）
- 讀檔/倒帶：
  - 先 `TryLoadStoryStateJson()` 載回 Ink state
  - 再用 `InkStoryEngine.EmitExternalOutput()` 丟出「存檔那一句的 StoryOutput + 還原用 tags」
  - TagEventRouter 會照平常流程把 Tag 分流給各 Tag Player，所以畫面/音訊會一起被還原

### 為了「讀檔回到較早的句子」不殘留狀態
- 新增兩個額外的 Tag 值（只給系統用，不一定要寫在 Ink 裡）：
  - `bg:clear`：清空背景圖
  - `bgm:stop`：停止並清空 BGM

## UI（這次新增的按鈕）
- `VNPlayer.uxml` 的 TopBar 新增三個按鈕：
  - `SaveButton`（存檔）
  - `LoadButton`（讀檔）
  - `RollbackButton`（倒帶）
- `VNPlayerPresenter` 已綁定這三個按鈕的點擊事件：
  - 需要在 Inspector 把 `InkSaveSystem` 指到 `VNPlayerPresenter.saveSystem`

## 測試
- `Assets/Editor/Tests/InkSaveDataTests.cs`：驗證 `InkSaveData` 的 JSON round-trip（ToJson/FromJson）
