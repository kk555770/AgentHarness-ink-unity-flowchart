# OpsidanosInk

這是一個 UPM 套件（`com.opsidanos.ink`），用 **Ink + UI Toolkit** 做「文字冒險 / 視覺小說」的可重用框架。

## 核心管線（新規定）
1. 開發者模式（Editor）：製作 Flow Chart → 輸出 `.ink`
2. Ink Unity Integration：把 `.ink` 編譯成 `.json`
3. 玩家模式（Runtime）：只讀 `.json` 播放

## 快速開始（玩家模式）
1. 打開 Unity → Package Manager → 選擇 `OpsidanosInk` → Import `玩家模式快速開始`
2. 在專案中找到匯入後的 `story.ink`，確定它已經被編譯出 `story.json`
   - 若你沒有看到 `story.json`：執行 `Assets > Recompile Ink`
3. 建立一個空場景，新增一個 GameObject（例如 `VNPlayer`）
4. 在 `VNPlayer` 加上元件：
   - `UIDocument`
   - `InkStoryEngine`
   - `VNPlayerPresenter`
   - （可選）`InkTagEventRouter`（顯示 Tag 除錯輸出用）
5. 設定元件欄位：
   - `UIDocument`
     - `Source Asset`：指定 `Packages/com.opsidanos.ink/Runtime/UI/UXML/VNPlayer.uxml`
   - `InkStoryEngine`
     - `Story Json Asset`：指定匯入樣本後產生的 `story.json`
   - `VNPlayerPresenter`
     - `Ui Document`：拖 `VNPlayer` 自己的 `UIDocument`
     - `Story Engine`：拖 `VNPlayer` 自己的 `InkStoryEngine`
   - （可選）`InkTagEventRouter`
     - `Story Engine`：拖 `VNPlayer` 自己的 `InkStoryEngine`
6. 按下 Play，你會看到文字與選項

## 目前功能（最小骨架）
- 讀取 Ink `.json` 建立 `Ink.Runtime.Story`
- UI Toolkit 顯示：名字、文字、選項
- Tag 除錯輸出：把 `ParsedTags` 變成 Console 訊息（`InkTagEventRouter`）

## Tag 演出（最小：音訊）
如果你的 Ink 會用 Tag（例如 `bgm` / `se`），你可以用 `InkTagAudioPlayer` 做最小音訊綁定：

1. 在同一個 GameObject（例如 `VNPlayer`）加上元件：
   - `InkTagEventRouter`
   - `InkTagAudioPlayer`
   - `AudioSource`（BGM 用，請把 `Loop` 打開）
   - `AudioSource`（SE 用）
2. 設定 `InkTagAudioPlayer` 欄位：
   - `Tag Event Router`：拖同物件的 `InkTagEventRouter`
   - `Bgm Source`：拖 BGM 的 `AudioSource`
   - `Se Source`：拖 SE 的 `AudioSource`
   - `Bgm Bindings`：新增一筆 `id="opening"`，`clip` 指到你的 BGM
   - `Se Bindings`：新增一筆 `id="open"`，`clip` 指到你的 SE
3. 按下 Play：
   - 你會看到 `[OpsidanosInk][Tag] ...`（Tag log）
   - 當 Tag 出現時，也會看到：
     - `[OpsidanosInk][BGM] opening -> <clipName>`
     - `[OpsidanosInk][SE] open -> <clipName>`

## Tag 演出（最小：背景）
如果你的 Ink 會用 Tag（例如 `bg`），你可以用 `InkTagBackgroundPlayer` 做最小背景切換：

1. 在同一個 GameObject（例如 `VNPlayer`）加上元件：
   - `InkTagEventRouter`
   - `InkTagBackgroundPlayer`
2. 設定 `InkTagBackgroundPlayer` 欄位：
   - `Tag Event Router`：拖同物件的 `InkTagEventRouter`
   - `Ui Document`：拖同物件的 `UIDocument`
   - `Background Element Name`：預設是 `Background`（對應 `VNPlayer.uxml` 的背景圖）
   - `Background Bindings`：新增一筆或多筆 `id` → `sprite` 對照
3. 按下 Play：
   - 當 Tag 出現時會看到：`[OpsidanosInk][BG] <id> -> <spriteName>`
