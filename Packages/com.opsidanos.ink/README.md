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
   - `Background Bindings`：新增一筆或多筆 `id` → `texture2D` 對照
3. 按下 Play：
   - 當 Tag 出現時會看到：`[OpsidanosInk][BG] <id> -> <textureName>`

## Tag 演出（推薦：角色狀態 char JSON）
如果你的 Ink 需要控制立繪，建議使用「狀態化」的 `char` JSON：每一句話用一份 JSON 一次描述 `left/center/right`，避免殘留。

1. 在同一個 GameObject（例如 `VNPlayer`）加上元件：
   - `InkTagEventRouter`
   - `InkTagCharacterStatePlayer`
2. 設定 `InkTagCharacterStatePlayer` 欄位：
   - `Tag Event Router`：拖同物件的 `InkTagEventRouter`
   - `Ui Document`：拖同物件的 `UIDocument`
   - `Bindings`：建立 actor/expr → Texture2D 的對照
     - 例如：`Actor="alice"`、`Expression="normal"`、`IsDefault=true`、`Texture=alice_normal.png`
3. Tag 格式：
   - 顯示 alice 在中間（normal），bs 在左邊：
     - `# char:{"left":{"actor":"bs"},"center":{"actor":"alice","expr":"normal"}}`
   - 三人換位，移動時間 1 秒：
     - `# char:{"transition":{"move":1},"left":{"actor":"alice","expr":"happy"},"center":{"actor":"bs"},"right":{"actor":"ss"}}`
   - 清空全部：
     - `# char:clear`

### 轉場規則（目前版本）
- 預設：`appear=0.3` 秒、`move=0.3` 秒、`disappear=0.3` 秒
- `transition.move=0` 代表瞬間移動（出現/消失仍照各自秒數）
- `transition.fade` 已改名為 `disappear`（目前仍可用，但會印 Error 提醒你改名）
- 預設順序（未指定 steps 時）：出現 → 移動 → 消失

#### steps：自訂先後與同時（推薦給 Flow Chart 輸出）
你可以在 `transition.steps` 指定一串「步驟」，每個步驟可以放 1 個或多個動作：
- `steps` 會照順序一個一個跑
- 同一個步驟內的 `actions` 會同時開始（例如 `move` + `disappear` 同時）
- `actions` 只支援：`appear` / `move` / `disappear`
- 你可以用 `raise` 與 `raiseActors` 控制「本步是否會改變層級」：
  - `raise`（bool，預設 `true`）：`true` 代表「本步有動作的角色」會自動暫時置頂；`false` 代表不自動置頂
  - `raiseActors`（string[]）：不管有沒有動作，都會先把指定 actor 暫時置頂（可用來「只有改層級」）
- 若你漏掉必要動作（例如有角色要消失但 steps 沒寫 `disappear`），系統會印 Error，並把缺的動作補到排程最後面，避免畫面狀態不正確

例：先消失，再「移動＋出現」同時，最後再消失
```json
"transition":{
  "appear":0.3,
  "move":0.3,
  "disappear":0.3,
  "steps":[
    {"actions":["disappear"]},
    {"actions":["move","appear"]},
    {"actions":["disappear"]}
  ]
}
```

例：先把 alice 置頂（無動作），再讓移動發生（且移動這一步不自動置頂）
```json
"transition":{
  "move":1,
  "steps":[
    {"raiseActors":["alice"]},
    {"actions":["move"],"raise":false}
  ]
}
```

#### 動作中置頂（不會蓋過對話框）
- 在每一步開始時，「有動作的角色」會被暫時移到角色層最上面（只在角色層內排序）
- 整串 steps 結束後，角色層順序會重置回基準層級（不影響下一串）

### Ink Tag JSON 注意事項（重要）
- Ink 的 `{ ... }` 會被當成 inline logic，所以你在 `.ink` 裡要把 JSON 的 `{` `}` 用反斜線逃逸：
  - 例：`# char:\{\"left\":\{\"actor\":\"bs\"\}\}`

> 注意：`char.mode="patch"` 目前尚未支援，請先用預設 replace（沒寫到的槽位視為 null）。
