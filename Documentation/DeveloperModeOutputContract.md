# 開發者模式輸出契約（Current Flow Projection → Ink → 玩家模式）

> 最後更新：2026/03/17  
> 文件角色：**現況投影契約層 / Current Projection Contract**  
> 適用範圍：開發者模式（Editor）輸出 `.ink`（含 `.flowchart.json` sidecar）給玩家模式（Runtime）播放與存讀檔

## 0. 這份契約在解決什麼問題
這份文件定義「開發者模式（Editor）輸出 Ink」時，**必須遵守**的契約，目標只有一個：

- 讓玩家模式（Runtime）在以下情境都能「正常演出 + 可存讀檔 + 可倒帶」，而且**不應該出現任何紅字 Error log**：
  - 從頭正常播放
  - 讀檔（Restore）
  - 倒帶（Rollback）
  - 在任意畫面狀態下套用同一筆輸出（例如：進入場景後立刻讀檔）
  - 圖上有分岔時，玩家模式也必須真的能走到分岔（不能只走第一條，其他變成死線）

先講清楚這份文件的定位：

- 它主要描述的是 **current Graph v2 / Flow Chart 工作流** 的 projection contract
- 它不是 canonical truth 本體
- 它也不是作者工具策略文件
- 未來如果作者工具前端改成 WebView / Browser / Electron，只要仍產出相同 projection semantics，這份契約依然適用

如果你想先看「目前這條作者工具工作流怎麼跑」，請先讀：

- `Documentation/CurrentAuthoringWorkflow.md`

## 1. 先講清楚：什麼是「可重播（快照式）」輸出
用 8 歲小孩也懂的例子：

- 你在舞台上演戲，導演每一句台詞旁邊都寫了一張「舞台擺放圖」。
- 不管舞台現在亂成什麼樣，你只要照那張圖擺，最後舞台就會長一樣。

在這個專案裡：

- 「一句輸出」= Ink 編譯後 `story.json` 裡的一筆 `StoryOutput`
- 「快照式」= 這一句的定義必須足夠完整，讓玩家模式可以從任意目前狀態收斂到同一個結果

## 2. 名詞與責任邊界（你要先分清楚兩條線）
- **開發者模式（Editor）**：用 Flow Chart（GraphToolkit）編輯節點與接線，輸出 `.ink`（可再編譯成 `story.json`）
- **玩家模式（Runtime）**：只讀 `story.json`，依 Tag 演出畫面
- **Restore 輸出**：讀檔 / 倒帶時送出的外部輸出（`StoryOutputSource.Restore`）

責任邊界（最重要）：

- 本文件只規定「canonical schema 投影到 Ink / `story.json` / Runtime」時的合法輸出契約；canonical truth 本體請見 `Documentation/CanonicalGraphSchema.md`。
- 開發者模式輸出**必須**符合本文件的契約，讓 Runtime 不需要「補洞」。
- Runtime 內部的紅字 Error log 是「抓錯用」，不是「正常流程」。
- Flow Chart（圖）是目前人類作者最重要的視覺投影：流程結構必須由節點/接線表達並在 Ink 層成立，不得藏在內容文字裡（詳見 §6）。

## 3. Tag 規範總表（哪些是狀態、哪些是事件）
### 3.1 關鍵規則
- Tag `key` **區分大小寫**，必須完全一致（例如只能用 `bg`，不能用 `BG`）。
- Tag `value` 會被 `Trim()`（前後空白會被去掉）。
- **保留值**同樣區分大小寫（例如 `clear` 必須全小寫）。

### 3.2 Tag 清單（目前 Runtime 支援）
| Tag key | 類型 | 值格式 | 保留值 | 讀檔/倒帶（Restore）會自動重播？ | 說明 |
|---|---|---|---|---|---|
| `speaker` | 文字資訊 | `speaker:<文字>` | 無 | 會 | 用來設定當前說話者（Runtime 會解析成 `StoryOutput.Speaker`；讀檔/倒帶會還原當下 speaker）。 |
| `bg` | 狀態 | `bg:<背景Id>` | `clear` | 會 | `bg:clear` 代表清空背景。 |
| `bgm` | 狀態 | `bgm:<音樂Id>` | `stop` | 會 | `bgm:stop` 代表停止並清空 BGM。 |
| `cg` | 狀態 | `cg:<cgId>` | `clear` | 會 | `cg:clear` 代表隱藏 CG。 |
| `char` | 狀態（推薦） | `char:clear` 或 `char:<JSON 物件>` | `clear` | 會 | 角色狀態化快照（見 §4）。 |
| `char-left` | 狀態（舊） | `char-left:<立繪Id>` | `clear` | 會 | 舊的「單槽位換圖」模式。 |
| `char-center` | 狀態（舊） | `char-center:<立繪Id>` | `clear` | 會 | 同上。 |
| `char-right` | 狀態（舊） | `char-right:<立繪Id>` | `clear` | 會 | 同上。 |
| `se` | 事件 | `se:<音效Id>` | 無 | 不會 | 事件不會被存檔還原；讀檔後不應期待它「再響一次」。 |
| `shake` | 事件 | `# shake`（無值） | 無 | 不會 | 事件不會被存檔還原；讀檔後不應期待它「再抖一次」。 |

### 3.3 狀態與事件的差異（為什麼你一定要分開）
用最直白的說法：

- **狀態**：像「背景現在是哪張圖」。你讀檔回到那一刻，背景就應該是那張圖。
- **事件**：像「門打開的音效」。你讀檔回到那一刻，除非你重新觸發事件，否則不該自動再響一次。

因此：

- 開發者模式輸出**不得**用事件 tag 來假裝「狀態」。（例如用 `se` 來代表 BGM 是錯的。）

### 3.4 新增/擴充 Tag 的規範（避免擴充破壞閉環）
只要新增一個 tag（或替既有 tag 增加新用法），就必須同時補齊下列資訊，否則禁止輸出：

- `key`：大小寫必須固定
- 類型：**狀態** 或 **事件**
- 值格式：字串、數字、JSON、無值（以及需要的跳脫規則）
- 保留值：例如 `clear`、`stop`
- Restore 行為：讀檔/倒帶後是否應「自動回到該狀態」
- 測試：至少 1 個「正常播放」+ 1 個「Restore」回歸測試

## 4. 核心契約：`char` 必須用「狀態化 JSON（快照）」輸出
### 4.1 為什麼一定要快照
讀檔 / 倒帶不是「把上一句再播一次」而已，而是：

- 你可能在任何畫面狀態下按下讀檔
- 系統必須把畫面切回「存檔那一刻」的狀態

所以 `char` 不能只講「做一個動作」，必須講「最後應該長怎樣」。

### 4.2 `char` 值允許兩種
- `char:clear`
- `char:<JSON 物件>`（必須以 `{` 開頭）

### 4.3 `char` JSON 最小格式
- `left/center/right`：三個槽位，代表最後的站位
  - 槽位可省略或給 `null`，代表該槽位最後是空的
  - `actor` 必須是非空字串
  - `expr` 可省略（代表預設表情）
- `transition`：轉場參數，可省略（省略就用元件預設秒數與預設順序）

範例（最常用，建議 Flow Chart 預設輸出就長這樣）：
```json
{
  "transition": { "appear": 0.3, "move": 0.3, "disappear": 0.3 },
  "left": { "actor": "bs" },
  "center": { "actor": "alice", "expr": "happy" },
  "right": { "actor": "ss" }
}
```

清空全部（等同 `char:clear`，兩種寫法擇一即可）：
```json
{
  "transition": { "disappear": 0.3 },
  "left": null,
  "center": null,
  "right": null
}
```

### 4.4 禁止事項（角色快照必須閉環）
- 同一個 `actor` **不得**同時出現在兩個槽位（Runtime 會印 Error）。
- `char.mode="patch"` 目前未支援（Runtime 會印 Error），開發者模式輸出**不得**產生。
- `transition.fade` 已改名為 `disappear`（Runtime 會印 Error 提醒），開發者模式輸出**不得**產生。

## 5. `transition.steps` 契約（重點：避免「缺少必要動作」紅字）
`steps` 的用途是「指定先後與同時」，例如：

- 先消失，再移動
- 先置頂某個角色，再開始移動（而且移動時不要自動置頂）

### 5.1 規範 1：只要你有輸出 `steps`，就必須安排三種動作（避免狀態相依）
這是最重要的一條。

- `steps[*].actions` 只允許：`appear` / `move` / `disappear`
- 在整串 `steps` 裡，**必須安排**（至少出現一次）：
  - `appear`
  - `move`
  - `disappear`
- 在整串 `steps` 裡，**不得重複安排**同一種動作（每種動作最多一次）

原因（用一句話講完）：

- Runtime 會依「目前畫面狀態」算出這一次**必要動作**（需要進場/移動/退場）；讀檔/倒帶時起始狀態不固定。
- 如果你漏掉必要動作，Runtime 會印紅字 Error，並把缺的動作補到排程最後面（確保畫面不壞）。
- **開發者模式輸出不得依賴這個補齊機制**；契約要保證同一筆輸出在空畫面套用也成立。

### 5.2 規範 2：`raiseActors` 必須保證「當下角色存在」
`raiseActors` 的意思是：「這一步先把某些角色放到角色層最上面」。

- `raiseActors` 只能指定：
  - 已經在畫面上的角色，或
  - 這一步同時安排了 `appear`，讓角色先被建立出來

否則 Runtime 會印出紅字 Error：

- `char.transition.steps[i].raiseActors 指定 actor="..."，但此時畫面上不存在（也不在本步 appear）`

### 5.3 推薦輸出樣板（Flow Chart 預設）
#### A. 不需要特殊先後：不要輸出 `steps`
這是最穩、最不容易出錯的版本（Runtime 會用預設排程：出現 → 移動 → 消失）：

```json
{
  "transition": { "appear": 0.3, "move": 0.3, "disappear": 0.3 },
  "left": { "actor": "bs" },
  "center": { "actor": "alice" }
}
```

#### B. 需要「同時」：用一個 step 放多個 actions（仍然必須包含三種動作）
例：先消失，再「移動＋出現」同時發生：

```json
{
  "transition": {
    "appear": 0.3,
    "move": 0.3,
    "disappear": 0.3,
    "steps": [
      { "actions": ["disappear"] },
      { "actions": ["move", "appear"] }
    ]
  },
  "left": { "actor": "bs" },
  "center": { "actor": "alice" }
}
```

#### C. 需要「先置頂」：把 raiseActors 放在包含 appear 的步驟（仍然必須包含三種動作）
例：先把 alice 置頂，再開始演出，但每個動作都不自動置頂（`raise=false`）：

```json
{
  "transition": {
    "appear": 0.3,
    "move": 1,
    "disappear": 0.3,
    "steps": [
      { "actions": ["appear"], "raise": false, "raiseActors": ["alice"] },
      { "actions": ["move"], "raise": false },
      { "actions": ["disappear"], "raise": false }
    ]
  },
  "left": { "actor": "bs" },
  "center": { "actor": "alice" },
  "right": { "actor": "ss" }
}
```

### 5.4 禁止事項（這些會讓輸出變成狀態相依）
- 不要用「raise-only 在前、appear 在後」的寫法去 raise 一個可能尚未出現的角色（空畫面套用必然噴紅字）。
- 不要把 Runtime 的紅字 Error 當成「正常流程」；它是拿來抓錯的。

## 6. Ink 基準層、Flow Chart 子集合與閉環規則（不可省略）
> 本節的目標不是「用流程圖取代 Ink」，而是把三件事同時寫死，避免矛盾：
>
> 1) 圖上怎麼畫（節點/接線）  
> 2) 匯出後 Ink 長什麼樣（必須可編譯、可多路線）  
> 3) 輸出如何可逆還原回圖（位置可變，但結構與相依關係必須一致）

### 6.1 Ink 基準層（完整索引）
Ink 語法本身允許大量 weave 與遞迴，並且整份 `.ink` 可以被編譯器檢驗後轉成 `story.json`。
因此規範不能只寫「目前專案剛好做得到的部分」，而是必須先列出 Ink 本來就能做到的基準能力，並清楚標示 Graph v2 是否已支援對應。

| Ink 能力（基準） | 常見寫法（例） | Flow Chart 對應（Graph v2） | 匯出 `.ink` 要求 | `.ink + sidecar` 可逆？ | 備註 |
|---|---|---|---|---|---|
| 註解 | `// ...`、`/* ... */` | `comment` 節點 | 匯出成 Ink 註解行 | 是 | 不影響玩家模式流程 |
| Tag（含自訂擴充） | `# bg:...`、`# char:...`、`# shake` | `dialogue` 內容（現況 sidecar token：`action`） | 原樣輸出；值需符合 §3-§5 | 是 | 只要是 `#` tag 就屬 Ink 基準能力 |
| Knot（段落） | `== knot ==`（`=` 至少 2 個） | 每個節點 1 個 knot | 匯出 `=== knot_<nodeId> ===` | 是 | 本子集合固定「每節點一 knot」 |
| Stitch（子段落） | `= stitch`（`=` 只有 1 個） | （保留） | （保留） | （保留） | 需要新增節點與資料結構才可逆 |
| Divert（跳轉） | `-> 目標` | 接線（edge） | 由接線產生 divert | 是 | **禁止用多行 `->` 假裝分岔** |
| 結束 | `-> END` | 線性節點無輸出線 | 匯出 `-> END` | 是 | 代表流程結束 |
| Choice（選項） | `*` / `+` | `choice` 節點 | 每輸出埠 1 行 choice | 是 | `*` 一次性、`+` 可重複 |
| Choice 顯示條件 | `* {expr} [文字] ...` | （保留） | （保留） | （保留） | 這不是 `condition` 節點；需 per-choice 欄位 |
| Conditional（條件分岔） | `{ - cond: ... - else: ... }` | `condition` 節點 | 匯出多行 conditional block | 是 | `- else:` 必須最後且必須存在 |
| Gather（匯流） | 行首 `-`（weave 結構） | 多條線連到同一節點 | 本子集合不輸出 gather 行 | 是 | 用「多個 divert 指向同一 knot」匯流 |
| Weave / 遞迴 | choice/gather 混用、遞迴 divert | （保留） | （保留） | （保留） | Ink 允許；Graph v2 先不視覺化 |
| Tunnel onwards | `->->` | （保留） | （保留） | （保留） | Ink 允許；未納入 Graph v2 |
| Thread | `<- 目標` | （保留） | （保留） | （保留） | Ink 允許；未納入 Graph v2 |
| 邏輯行 | `~ temp x = 1` | `dialogue` 內容（現況 sidecar token：`action`） | 允許原樣輸出 | 是 | 不得用來偷改「流程結構」 |
| 變數宣告 | `VAR x = 0`、`CONST x = 1` | `dialogue` 內容（現況 sidecar token：`action`） | 允許原樣輸出 | 是 | 建議集中在靠近 start 的節點 |
| INCLUDE | `INCLUDE a.ink` | （保留） | （保留） | （保留） | 若要支援需新增節點＋打包規則 |

> 說明：表中「保留」代表 Ink 有能力，但 Graph v2 尚未定義對應節點與可逆資料結構；在補齊前，開發者模式不應輸出這種流程結構，否則圖就不再是權威。
>
> 注意：行首 `-` 在 Ink 有兩個常見意思：
> - 在 knot 內容中：gather（weave 的匯流）
> - 在 `{ ... }` 區塊中：conditional 的分支行
> Graph v2 的 `condition` 節點只會輸出後者；匯流則用「多個 divert 指向同一個 knot」來表達。

### 6.2 Flow Chart 子集合（GraphToolkit）節點規範（Graph v2）
Flow Chart 的目的不是限制 Ink，而是建立「所見即所得」且可逆的資料結構。

#### 6.2.0 命名裁決與 legacy mapping
- 本文件在語意層，一律把「主流程對話節點」稱為 `dialogue`。
- 但目前 Graph v2 sidecar / current implementation 仍保留 legacy token：`type = "action"`。
- 因此本文件若提到：
  - `dialogue`：代表 canonical / 語意層的對話節點
  - `action`：若未特別註明，僅代表 current sidecar token，不代表 canonical 命名回頭改回 `action`
  - `stageAction`：代表真正的動作資料節點，不能和 legacy `action` 混讀
- 若要看 canonical 裁決本體，請以 `Documentation/CanonicalGraphSchemaSpec.md` 為準；本文件處理的是 Graph v2 projection 到 Runtime 的合法輸出契約。

#### 6.2.1 節點型別
- `start`：開始節點（必須存在且只能 1 個）
- `dialogue`：對話節點（canonical 名稱；現況 sidecar token = `action`；主流程節點，承載對話文字與接收動作資料）
- `stageAction`：動作節點（資料節點，不參與主流程，輸出動作資料給對話節點）
- `character`：角色資料節點（資料節點，維護角色ID/名稱/表情圖片清單）
- `castBundle`：登場角色集束節點（資料節點，聚合本段可用角色清單）
- `comment`：註解節點（只影響作者閱讀，不影響播放）
- `choice`：選項節點（玩家選擇，會有 1 條以上輸出線）
- `condition`：條件節點（系統判斷，會有 2 條以上輸出線，且必須含「否則」）

#### 6.2.1-0 `character / castBundle` 的目前定位
- 本文件會詳細描述 `character / castBundle`，是因為它們屬於目前 Graph v2 projection / authoring workflow 的資料來源設計。
- 但這不代表它們已經正式升格為 canonical schema 的核心節點。
- 到目前為止，更穩定的角色資料主線仍是 Runtime / ResourceMap：
  - `resource_map.json`
  - `InkResourceMap`
  - `char` JSON / actor-expr 映射
- 因此閱讀本文件時，應把 `character / castBundle` 解讀成：
  - current Graph v2 的 projection-heavy / authoring data-source nodes
  - 不是 canonical truth 已定案的最小核心節點

#### 6.2.1-1 編輯器白話名稱與下拉欄位（作者體驗）
- 節點標題（顯示給作者看）應使用白話繁中：
  - `start` 顯示「開始」
  - `dialogue` 顯示「對話」
  - `stageAction` 顯示「動作」
  - `character` 顯示「角色」
  - `castBundle` 顯示「登場角色」
  - `comment` 顯示「註解」
  - `choice` 顯示「選項」
  - `condition` 顯示「條件」
- GraphToolkit 實作要求：節點型別名稱本身要用同一組繁中詞（開始/對話/動作/角色/登場角色/註解/選項/條件），避免標題與類別名不一致。

#### 6.2.1-2 節點文字欄位可讀寬度（作者體驗）
- 目標：讓作者在節點內直接看清楚內容，不需要先打開外部編輯器。
- GraphToolkit 實作要求：
  - 需提供專案層級樣式覆寫，把節點內文字欄位調整為「可直接閱讀中等長句子」的寬度。
  - 欄位寬度調整只影響編輯器顯示，不得改變匯出/匯入資料語意。
  - 若有長標籤，需避免標籤寬度過大擠壓文字輸入區。
- 適用範圍（目前 Graph v2）：
  - `dialogue` 的對話內容欄位（現況 sidecar token = `action`）
  - `stageAction` 的動作內容欄位
  - `character` 的角色ID/名稱/表情名稱欄位
  - `comment` 的註解欄位
  - `choice` 的選項文字欄位
  - `condition` 的條件文字欄位

#### 6.2.2 接線規則（最重要）
- 接線分三種（必須分離）：
  - **流程線（Flow）**：主流程走向（start/dialogue/comment/choice/condition）
  - **資料線（ActionData）**：動作節點 → 對話節點的資料輸入（ActionIn*）
  - **資料線（CharacterData / CastData）**：
    - `character` → `castBundle`（CharacterIn*）
    - `castBundle` → `dialogue/stageAction`（CastIn）
  - 編輯器上應讓流程線與資料線可視覺區分（至少型別/顏色其中之一必須可辨識）
- `start/dialogue/comment`（流程線）：
  - 輸出線數量只能是 `0` 或 `1`
  - `0`：代表該 knot 結束（匯出 `-> END`）
  - `1`：代表下一個節點（匯出 `-> knot_<nextId>`）
  - `> 1`：匯出必須失敗，並提示「請改用 choice/condition」
- `choice/condition`（流程線）：
  - 節點必須允許調整「輸出埠數量」
  - 每個輸出埠只能接 `1` 條線（每個選項/分支只能去 1 個目標）
  - 任一輸出埠沒接線：匯出必須失敗（因為會造成不可逆，也無法在圖上說清楚要去哪裡）
- `stageAction`（資料線）：
  - 每個動作節點必須且只能有 `1` 條資料輸出線
  - 只能接到 `dialogue` 節點的 `ActionIn*` 輸入埠（現況 sidecar token = `action`）
  - 不允許接到 `Flow` 輸入埠
- `character`（資料線）：
  - 可輸出到 `castBundle` 的 `CharacterIn*`
  - 不允許直接接到 `dialogue/stageAction` 或 `Flow` 輸入埠
- `castBundle`（資料線）：
  - 輸入埠由連線自動增減（`CharacterIn*`）
  - 輸出埠由連線自動增減（`CastOut*`）
  - 每個輸出埠只可接 `dialogue/stageAction` 的 `CastIn`
  - 至少要有 `1` 條角色輸入線與 `1` 條角色輸出線，否則匯出必須失敗
- `dialogue/stageAction`（角色來源）：
  - 角色下拉來源只能是 `CastIn` 連到的 `castBundle`
  - 若節點沒有 `CastIn` 連線，角色下拉必須顯示空集合（不可偷讀全域角色）
- 匯流（多路線收束）：
  - 允許多條線連到同一個節點（這是文字小說最常見的「收束」手法）

> 以上規則的目的，是避免「圖上看起來分岔，匯出卻只是把多行 `->` 排在一起」這種隱性錯誤。Ink 不會同時走多個 divert，多出來的只會變死線。

#### 6.2.3 `dialogue/stageAction/comment` 內容規範（圖是權威，所以不能藏流程結構）
`dialogue/stageAction/comment` 的內容允許做「演出」與「狀態更新」，但不得偷偷改變流程結構。

- 允許（例）：一般文字、tag（`# ...`）、邏輯行（`~ ...`）、變數宣告（`VAR/CONST ...`）、行內邏輯（文字中出現 `{expr}`）
- 禁止（只要出現就代表你把流程藏在文字裡）：
  - Knot/Stitch 定義（行首 `==`、`===`、或行首 `=`）
  - Divert / Tunnel / Thread（行首 `->`、`->->`、`<-`）
  - Choice（行首 `*` 或 `+`）
  - Gather（行首 `-`，在 knot 內容中會變成 weave 的 gather）
  - 多行 conditional block（用 `{` 開一個區塊，底下用 `- cond:` / `- else:` 做分支那種）

如果你只是要「顯示符號」而不是語法，必須跳脫（特別是行首）：
- 要顯示 `->`：寫 `\->`（在 `-` 前加 `\`）
- 要顯示 `==`：寫 `\==`（在第一個 `=` 前加 `\`）
- 要顯示行首 `-`：寫 `\-`
- 要顯示 `*` 或 `+`：寫 `\*`、`\+`
- 要顯示 `{` `}`：寫 `\{`、`\}`
- 要顯示 `#`：寫 `\#`

#### 6.2.4 角色資料來源與 Addressable 規則（Graph v2）
- 本節描述的是 current Graph v2 authoring / sidecar contract。
- `character / castBundle` 在這裡的規則，目的是讓 projection 能保存角色候選來源、Addressable key 與作者資料，而不是宣告它們已成為 canonical 核心節點。
- `character` 節點欄位：
  - `characterId`：角色唯一ID（下拉主鍵）
  - `characterName`：預設顯示名稱
  - `expressions[*]`：每列為「表情名稱 + 圖片資源」
- 圖片欄位操作規則：
  - 作者可直接把 Project 視窗圖片拖到 `character` 節點圖片欄位
  - 拖入當下必須自動檢查 Addressable
  - 若 `Project_Resources` Group 不存在，必須自動建立
  - 若圖片尚未加入 Addressable，必須自動加入 `Project_Resources`
- Addressable Key 規則：
  - 最終格式固定：`char/{characterId}/{expressionName}`
  - 若先拖圖、後填 `characterId` 或 `expressionName`，欄位變更時必須即時重算並更新 Key
- 刷新規則（非每幀）：
  - 圖接線變更（連線/斷線）
  - `character` 內容變更（ID/名稱/表情名稱/圖片）
  - 匯入 sidecar 後、開啟圖時
- 名稱覆寫規則：
  - `dialogue/stageAction` 可覆寫當下顯示名稱
  - 覆寫只作用在當前節點，不得回寫 `character` 主資料
- `castBundle` 多輸出語意（第一版）：
  - 輸出埠可依連線自動增減
  - 每個輸出埠都代表同一份角色名單（不做分組語意）

### 6.3 Flow Chart → Ink 的對應輸出（Graph v2）
#### 6.3.1 檔案開頭（入口）
- 檔案第一行必須是：`-> knot_<startNodeId>`

#### 6.3.2 每個節點對應一個 knot
- 每個**主流程節點**輸出一個 knot：`=== knot_<nodeId> ===`
- `stageAction` 不單獨輸出 knot；它會在目標 `dialogue` 節點輸出時合併
- `character/castBundle` 也不單獨輸出 knot；它們只提供角色資料來源，不參與主流程文本

#### 6.3.3 `start/dialogue/comment`（線性主流程）
- 節點內容照原樣輸出（`comment` 以 `//` 註解輸出）
- 若有下一個節點：輸出 `-> knot_<nextId>`
- 若沒有下一個節點：輸出 `-> END`

#### 6.3.3-1 `stageAction`（動作資料）
- `stageAction` 節點內容在匯出時，按 `ActionIn*` 順序插入對應 `dialogue` knot 內容區塊
- 同一個 `dialogue` 若接多條資料線，順序規則為：
  1. `ActionIn0`, `ActionIn1`, `ActionIn2`...
  2. 同一輸入埠衝突時以節點 id 排序（保證 deterministic）

#### 6.3.3-2 `character/castBundle`（角色資料）
- 這兩種節點不直接輸出 Ink 行。
- 它們只決定 `dialogue/stageAction` 的角色候選來源與顯示資訊。

#### 6.3.4 `choice`（玩家選擇）
- 每個輸出埠匯出成一行 choice：
  - `* [選項文字] -> knot_<nextId>`（一次性）
  - `+ [選項文字] -> knot_<nextId>`（可重複）
- choice 節點允許只有 1 個選項（常用於「讓玩家明確按一下」的演出）

#### 6.3.5 `condition`（系統判斷）
- 必須匯出成多行 conditional block：
```ink
{
  - <條件1>:
    -> knot_<nextIdA>
  - else:
    -> knot_<nextIdElse>
}
```
- `- else:` 必須存在且必須是最後一個分支（避免條件都不符合時掉到未知狀態）

### 6.4 `.flowchart.json`（sidecar）是目前 Graph v2 可逆閉環的權威投影格式
- `.ink` 是玩家模式要用的流程文本。
- `.flowchart.json` 保存「圖的結構投影」：節點型別、分岔埠、分支文字/條件、連線目標。
- 對目前 Graph v2 匯入器而言，匯入時必須以 sidecar 還原，不解析 `.ink` 來猜圖形結構。
- 但 `.flowchart.json` 仍屬 sidecar / interchange / round-trip projection，不應被誤讀為凌駕於 canonical schema 之上的最終真相來源；canonical truth 本體請見 `Documentation/CanonicalGraphSchema.md`。

#### 6.4.1 Graph v2 sidecar 最小欄位
- `version`：格式版本（Graph v2 建議使用 `2.x`）
- `graphName`
- `startNodeId`
- `nodes[*].id/type/content`
- `nodes[*].outputs[*].portName/toNodeId/toPortName`
- `dialogue` 額外需要（現況 sidecar：`type = "action"`）：
  - `nodes[*].outputs[*].toPortName` 預期為 `Flow`
  - `nodes[*].actorId`（可空）
  - `nodes[*].actorName`（可空，代表當下節點覆寫名）
- `stageAction` 額外需要：
  - `nodes[*].outputs[*].portName` 預期為 `ActionData`
  - `nodes[*].outputs[*].toPortName` 預期為 `ActionIn*`
  - `nodes[*].actorId`（可空）
  - `nodes[*].actorName`（可空，代表當下節點覆寫名）
- `character` 額外需要：
  - `nodes[*].characterId`
  - `nodes[*].characterName`
  - `nodes[*].expressions[*].name`
  - `nodes[*].expressions[*].textureGuid`
  - `nodes[*].expressions[*].addressableKey`
  - `nodes[*].outputs[*].portName` 預期為 `CharacterData`
  - `nodes[*].outputs[*].toPortName` 預期為 `CharacterIn*`
- `castBundle` 額外需要：
  - `nodes[*].castInputCount`（可由匯入時重建，但 sidecar 建議保留）
  - `nodes[*].outputs[*].portName` 預期為 `CastData` 或 `CastOut*`
  - `nodes[*].outputs[*].toPortName` 預期為 `CastIn`
- `choice` 額外需要：
  - `nodes[*].choiceMode`（`*` 或 `+`）
  - `nodes[*].outputs[*].label`
- `condition` 額外需要：
  - `nodes[*].outputs[*].condition`（非 else 分支）
  - `nodes[*].outputs[*].isElse`（else 分支，且必須在最後）

#### 6.4.2 Graph v2 匯入相容策略（角色資料）
- 這一節仍屬 current projection / importer contract。
- 若未來 `character / castBundle` 要升格為 canonical node，需另外補 canonical payload / edge semantics / tests，不能直接把這裡的 sidecar 欄位原封不動升格。
- 舊 sidecar 若沒有 `character/castBundle` 欄位，匯入器不得猜測補圖；應明確記錄 warning 並維持可開啟。
- 若有 `textureGuid` 但缺 `addressableKey`，匯入後在刷新階段按 `char/{id}/{expr}` 重算 Key。
- 若 `dialogue/stageAction` 有 `actorId`，但 `CastIn` 沒有連到來源，匯入後角色下拉視為空集合，並提示修正接線。

### 6.5 JSON 在 `.ink` 內的跳脫規則（特別是 `char`）
Ink 會把 `{ ... }` 當成 inline logic，所以你在 `.ink` 內要把 JSON 的大括號跳脫：

- 必須把 `{` `}` 寫成 `\{` `\}`
- 建議把 JSON 裡的 `"` 一律寫成 `\"`（避免被誤讀）

例（`.ink` 檔內寫法）：
```text
# char:\{\"left\":\{\"actor\":\"bs\"\},\"center\":\{\"actor\":\"alice\"\}\}
```

例（玩家模式最終收到的 tag value 概念上等同）：
```json
{"left":{"actor":"bs"},"center":{"actor":"alice"}}
```

## 7. 可檢驗性（避免「看起來能跑，其實流程已壞」）
- 匯出的 `.ink` 必須能被 Ink 編譯器編譯成 `story.json`（否則代表流程語法不成立）
- 匯出的 `.ink + .flowchart.json` 必須可匯入還原成圖（結構與相依關係一致）
- 本契約最容易出錯的部份（例如 `char.transition.steps` 的閉環要求）必須有回歸測試鎖定（目前 PlayMode 測試在：`OpsidanosInkPlayModeTests.cs`、`OpsidanosInkPlayModeUiClickTests.cs`）

### 7.1 玩家輸入節奏不變式（Continue / Choice / Rollback）
- 玩家模式三種輸入（`Continue`、`Choice`、`Rollback`）的點擊節奏必須一致：
  1. Busy（打字機或角色演出尚未完成）時，第一次點擊只能 `ForceComplete`，不能直接前進或倒帶。
  2. 觸發 `ForceComplete` 後必須進入點擊冷卻；冷卻期間點擊不得觸發前進/選擇/倒帶。
  3. 冷卻結束後，下一次有效點擊只能執行 1 次流程動作（前進一步、選擇一次、或倒帶一步）。
- 禁止把 `Rollback` 設計成「連按排隊連續倒帶」語義；若要倒帶多步，必須由多次有效點擊觸發。

### 7.2 Rollback 測試治理（caseId 注入）
- `OpsidanosInkPlayModeUiClickTests.cs` 的 Rollback 節奏驗證採「固定流程 + caseId 注入」。
- 目前最低保留案例：
  - `RBK_001`：Busy 首點只補完，冷卻後第二次才倒帶一步。
  - `RBK_002`：快速連按不排隊，一波連按最多倒帶一步。
- 規格變更時，先改 case 參數（如 `requiredRollbackSteps`、各階段 `clickCount`），不要直接重寫整段測試流程；只有流程邏輯本身改變時，才改共用 runner。

### 7.3 節奏相關改動的最小驗收
- 任何變更只要碰到 `OnClickContinue` / `OnClickChoice` / `OnClickRollback` / 點擊冷卻，都至少要通過：
  - `RollbackButton_Busy時第一次只補完不倒帶_冷卻後第二次才倒帶`
  - `RollbackButton_快速連按_不會排隊連續倒帶`
  - `OpsidanosInkPlayModeUiClickTests` 整包
- 驗收時 Console `error` 必須是 0 筆。

### 7.4 GraphToolkit 測試防暴走流程（固定範圍）
- GraphToolkit 匯入/匯出閉環驗證，禁止用「整包 EditMode」直接跑。
- 允許的固定範圍：
  - Assembly：`OpsidanosInk.EditModeTests`
  - Category：`GraphToolkitFlowSafe`
  - Fixture：`OpsidanosInk.Tests.EditMode.InkFlowChartImportTests`、`OpsidanosInk.Tests.EditMode.InkFlowChartRoundTripTests`
- 兩個 fixture 必須維持 `[Timeout(60000)]`，避免測試卡住拖垮 Unity/MCP。
- Unity Editor 建議入口：`Tools/OpsidanosInk/測試/GraphToolkit/安全跑 Import+RoundTrip（60秒）`
- 若執行紀錄出現 package 測試名稱（例如 `com.unity.ai.navigation`），判定為範圍外執行，必須中止並改回固定篩選流程。
