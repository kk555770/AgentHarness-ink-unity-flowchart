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
   - （可選）`InkSaveSystem`（存檔/讀檔/倒帶）
   - （可選）`InkTagEventRouter`（顯示 Tag 除錯輸出用）
5. 設定元件欄位：
   - `UIDocument`
     - `Source Asset`：指定 `Packages/com.opsidanos.ink/Runtime/UI/UXML/VNPlayer.uxml`
   - `InkStoryEngine`
     - `Story Json Asset`：指定匯入樣本後產生的 `story.json`
   - `VNPlayerPresenter`
     - `Ui Document`：拖 `VNPlayer` 自己的 `UIDocument`
     - `Story Engine`：拖 `VNPlayer` 自己的 `InkStoryEngine`
     - `Save System`：拖 `VNPlayer` 自己的 `InkSaveSystem`（若你有加）
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

## 資源映射（推薦：ResourceMap JSON）
如果你不想在每個 Tag Player 都填一堆 bindings（id → Texture/Clip），可以用一個集中式的 `InkResourceMap`：

1. 準備一份 JSON（Demo 已提供）：
   - `Assets/OpsidanosInk/Demo/resource_map.json`
2. 在同一個 GameObject（例如 `VNPlayer`）加上元件：
   - `InkResourceMap`
3. 設定 `InkResourceMap` 欄位：
   - `Resource Map Json`：指定 `resource_map.json`
4. 把各 Tag Player 的 `Resource Map` 指向同一個 `InkResourceMap`：
   - `InkTagAudioPlayer` / `InkTagBackgroundPlayer` / `InkTagCgPlayer` / `InkTagCharacterPlayer` / `InkTagCharacterStatePlayer`
5. 按下 Play：
   - 之後 `bg/bgm/se/cg/char` 只要給 `id`（或 actor/expr），就會從同一份 JSON 取得對應資源

注意：
- `resource_map.json` 的每筆資源現在有兩個欄位：
  - `assetPath`：Editor（Play Mode）用 `AssetDatabase` 載入（不需要 Addressables）
  - `address`：Player build 用 Addressables 載入（一定要填）
- 若要做 Player build，你需要：
  1. 把 `resource_map.json` 用到的資源都設成 Addressable
  2. 資源的 Address（地址）要和 `resource_map.json` 的 `address` 一樣（最簡單：直接用資源路徑當地址）
  3. 先建置 Addressables：`Addressables > Build > New Build > Default Build Script`
  4. （只想設定 Demo）你也可以直接點選 Unity 選單：`OpsidanosInk > Addressables > 套用 Demo ResourceMap（自動勾 Addressable）`

## Tag 演出（最小：畫面抖動）
如果你的 Ink 會用 Tag（例如 `shake`），你可以用 `InkTagShakePlayer` 做最小畫面抖動：

1. 在同一個 GameObject（例如 `VNPlayer`）加上元件：
   - `InkTagEventRouter`
   - `InkTagShakePlayer`
2. 設定 `InkTagShakePlayer` 欄位：
   - `Tag Event Router`：拖同物件的 `InkTagEventRouter`
   - `Ui Document`：拖同物件的 `UIDocument`
   - `Shake Target Element Name`：預設是 `Background`（只會抖背景，不會抖對話框/選項）
   - `Default Duration Seconds` / `Default Strength Pixels`：調整抖動時間與幅度
3. 在 Ink 裡寫：
   - `# shake`
4. 按下 Play：
   - 當 Tag 出現時會看到：`[OpsidanosInk][Shake] OutputId=...`

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
  - `raiseActors`（string[]）：不管有沒有動作，都會先把指定 actor 暫時置頂（如果該 actor 可能需要進場，請把 `raiseActors` 放在包含 `appear` 的那一步）
- 如果你是 Flow Chart/GraphToolkit 匯出（可重播/快照式），只要你有輸出 `steps`，就應該把 `appear/move/disappear` 三種動作都排進去（各最多一次），避免在不同起始狀態（例如讀檔後空畫面）觸發紅字 Error
- 若你漏掉必要動作（例如有角色要進場，但 steps 沒寫 `appear`），系統會印 Error，並把缺的動作補到排程最後面，避免畫面狀態不正確（但紅字代表你的輸出不完整）
- Flow Chart/GraphToolkit 的正式輸出契約請以 `Documentation/DeveloperModeOutputContract.md` 為準

例：先消失，再「移動＋出現」同時
```json
"transition":{
  "appear":0.3,
  "move":0.3,
  "disappear":0.3,
  "steps":[
    {"actions":["disappear"]},
    {"actions":["move","appear"]}
  ]
}
```

例：先把 alice 置頂（無動作），再開始演出，但每個動作都不自動置頂（raise=false）
```json
"transition":{
  "appear":0.3,
  "move":1,
  "disappear":0.3,
  "steps":[
    {"actions":["appear"],"raise":false,"raiseActors":["alice"]},
    {"actions":["move"],"raise":false},
    {"actions":["disappear"],"raise":false}
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

## 存檔 / 讀檔 / 倒帶（Rollback）
如果你想讓玩家模式有最小的存檔功能，可以使用 `InkSaveSystem`：

1. 在同一個 GameObject（例如 `VNPlayer`）加上元件：
   - `InkSaveSystem`
2. 設定 `InkSaveSystem` 欄位：
   - `Story Engine`：拖同物件的 `InkStoryEngine`
   - `Rollback Capacity`：最多能倒帶幾句（預設 50）
3. 設定 `VNPlayerPresenter` 欄位：
   - `Save System`：拖同物件的 `InkSaveSystem`
4. 按下 Play：
   - 右上角 TopBar 會看到：`存檔` / `讀檔` / `倒帶`

注意：
- 目前的「存檔槽位」是存在記憶體裡：停止 Play Mode 後就會消失（之後要做多槽/寫入檔案再擴充）
