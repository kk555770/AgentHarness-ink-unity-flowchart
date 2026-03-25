# Canonical Graph Schema

> 文件負責人：architecture
> 最後更新：2026/03/17  
> 文件角色：**真相層 / What 層**  
> 目的：定義本專案的唯一語意真相應該長什麼樣，並把它和 sidecar、GraphToolkit、Web 作者工具、Unity runtime 的責任邊界切開。

## 0. 先講結論

本專案的唯一語意真相，不應是：

- `.inkfc`
- `.flowchart.json`
- `.ink`
- `story.json`
- Unity 場景裡目前掛了哪些元件

真正的唯一語意真相，應該是：

> **一份獨立於編輯器與 runtime 的敘事圖 schema。**

這份 schema 必須能同時被：

- AI 用 API 操作
- 人類用圖形工具編輯
- 匯出器投影到 Ink / sidecar
- runtime adapter 載入並執行

重點是：

- 人類用的圖形工具可以是 GraphToolkit
- 也可以是未來的 WebView / Browser / Electron 作者介面
- 但不管前端殼長什麼樣，它們都不應升格成 canonical truth 本體

## 1. 這份文件和其他文件的關係

本文件只回答一件事：

> **canonical truth 應該包含哪些語意，以及哪些東西不能混進來。**

它不取代其他文件：

- `Documentation/DocsIndex.md`
  - 負責給新讀者一張文件地圖
- `Documentation/AuthoringToolStrategy.md`
  - 負責回答作者工具為什麼會往 Web-first 收斂，以及 current GraphToolkit 的過渡定位

- `Documentation/NarrativeGraphArchitecture.md`
  - 負責回答整體北極星、四層架構與責任邊界
- `Documentation/DeveloperModeOutputContract.md`
  - 負責回答 projection 到 Ink / runtime 時哪些輸出才算合法
- `Documentation/GraphToolkitSpec.md`
  - 負責回答 GraphToolkit API 與 Editor 工具開發規範
- `Documentation/UIToolkitSpec.md`
  - 負責回答 UI Toolkit API 與 UI 結構規範

如果用小朋友也懂的方式講：

- `NarrativeGraphArchitecture` 像在講「為什麼要有這套規則」
- `CanonicalGraphSchema` 像在講「真正的積木長什麼樣」
- `DeveloperModeOutputContract` 像在講「積木拼好後，送去舞台演出要符合什麼規矩」

若要看更具體的 node / port / edge / invariant / sidecar mapping，請直接讀：

- `Documentation/CanonicalGraphSchemaSpec.md`

若要看更具體的 AI / 程式控制操作，請直接讀：

- `Documentation/CanonicalGraphApiSpec.md`

## 2. Canonical Schema 的責任

Canonical schema 應只負責定義以下內容：

- 圖本身有哪些節點
- 節點之間怎麼連
- 每種節點可以帶哪些資料
- 哪些資料是必要的
- 哪些組合是合法的
- 哪些規則 AI 與人類都必須共同遵守

Canonical schema **不應**直接包含：

- GraphToolkit 視窗位置、折疊狀態、選取狀態
- Web 前端框選狀態、面板開關、縮放比例
- Unity 專用 component 引用
- 某個 runtime 才有的臨時狀態
- 純粹為了投影或編譯方便才出現的中繼格式技巧

## 3. 核心實體

Canonical schema 至少應由四種核心實體組成：

### 3.1 Graph

`Graph` 代表整份敘事圖。

它至少應有：

- `graphId` 或等價識別
- `version`
- `nodes`
- `edges`
- 必要的 graph-level metadata

`Graph` 的責任是保存整體語意結構，不是保存某個 Editor 視窗長什麼樣。

### 3.2 Node

`Node` 代表一個有明確語意的敘事單位。

每個 node 至少應有：

- `nodeId`
- `nodeType`
- `payload`

重要原則：

- `nodeType` 必須穩定，不能依賴 GraphToolkit 畫面上的中文顯示字串
- `payload` 必須只描述語意，不描述某個投影工具的 UI 細節

### 3.3 Port

`Port` 是 node 可連線的語意接點。

port 的存在意義不是為了畫線而已，而是為了表達：

- 流程往哪裡走
- 哪些是資料線
- 哪些是分支線
- 哪些連線合法，哪些不合法

port 名稱與型別應屬於 schema 的一部分，而不是 Editor 私有細節。

### 3.4 Edge

`Edge` 代表兩個 port 之間的語意關係。

edge 至少要能表達：

- 從哪個 node / port 出發
- 連到哪個 node / port
- 這條線的語意是否屬於流程、資料或分支

如果某種連線在不同投影裡畫法不同，edge 仍必須維持同一個語意。

## 4. 節點型別的原則

Canonical schema 需要穩定的節點型別集合。

以目前專案已出現的方向來看，至少包含：

- `start`
- `dialogue`
- `stageAction`
- `comment`
- `choice`
- `condition`

目前**不建議直接升格為 canonical 核心節點**，但可能存在於 projection / authoring data layer 的節點類型，至少包含：

- `character`
- `castBundle`

它們目前更接近：

- 作者工具的資料來源節點
- projection / sidecar 為了維持角色候選來源而存在的搬運節點
- runtime 資源映射（例如 `resource_map.json` / `InkResourceMap`）的上游作者資料設計

而不是已被 canonical schema、匯入匯出邏輯與 round-trip 測試共同驗證過的最小核心語意。

後續就算擴充更多型別，也應遵守同一原則：

- 型別名稱描述語意，不描述 UI 呈現
- 節點資料由 payload 表達，不靠自由文字偷藏規則
- 分支、條件、流程結構不能退回到「從內文猜」
- 若某節點目前只有 projection 契約，沒有穩定 payload / port / edge / round-trip 驗證，應先留在 projection-heavy 層，而不是搶先升格成 canonical core type

## 5. Graph Invariants

Canonical schema 應能定義一組不依賴投影工具的圖規則。

例如：

- 必須且只能有一個開始節點
- 線性節點不能偷偷接出多條流程線
- `choice` / `condition` 的輸出埠規則必須完整
- 流程線與資料線不能混成同一件事
- 匯出到 Ink 時，不得靠自由文字偷藏 `->`、`*`、`+`、`==` 來改變流程結構

這些規則應該先存在於 schema 層，再由：

- GraphToolkit 驗證器
- 匯出器
- 匯入器
- 測試

去共同驗證，而不是把規則散落在每個投影工具裡。

## 6. API 形狀的最小原則

AI 要穩定操作本專案，控制面應靠 schema / API，而不是靠 Editor 手勢。

最小 API 形狀可以先想成：

```text
CreateGraph(metadata)
CreateNode(type, payload)
UpdateNode(nodeId, payload)
RemoveNode(nodeId)
Connect(fromNodeId, outPort, toNodeId, inPort)
Disconnect(edgeId)
ValidateGraph()
ProjectGraph(target)
```

這裡最重要的不是函式名稱，而是它們代表的責任：

- 建立語意
- 修改語意
- 連接語意
- 驗證語意
- 投影語意

也就是說，Projection 應該是 API 的結果，不應該反過來變成 API 的唯一入口。

更完整的操作規格、錯誤模型、idempotency 與 projection API，請見 `Documentation/CanonicalGraphApiSpec.md`。

## 7. Projection 的邊界

Canonical schema 與 projection 的關係應該是：

```text
canonical graph
  -> GraphToolkit 視覺圖
  -> .flowchart.json sidecar
  -> .ink
  -> story.json
```

其中：

- `GraphToolkit` 是人類可編輯視覺投影
- `.flowchart.json` 是目前 Graph v2 的 sidecar / interchange / round-trip 投影格式
- `.ink` 是文本投影
- `story.json` 是編譯後執行投影
- `character / castBundle` 若目前存在於 Graph v2 文件與 sidecar 設計，較合理的定位也是 projection / authoring data-source nodes，而不是 canonical 最小核心節點

關鍵原則：

- projection 可以有自己的格式
- projection 可以有為了搬運資料而存在的中繼欄位
- projection 可以暫時保留 legacy token（例如 current sidecar 用 `type = "action"` 代表 canonical `dialogue`）
- 但 projection 不應凌駕於 canonical schema 之上，變成新的真相來源

## 8. Runtime Adapter 的邊界

runtime adapter 的工作，是把 canonical graph 的語意接到某個執行環境。

以目前 repo 來說，Unity runtime 是最重要的一個 adapter。

但 schema 層應避免出現：

- 只有 Unity 才看得懂的語意
- 只有某個 MonoBehaviour 才能解釋的規則
- 只有目前這個 UI 版型才成立的故事結構

如果某個資訊離開 Unity 就失效，那它比較可能屬於 adapter 細節，而不是 canonical schema。

## 9. 和目前 repo 的關係

以目前實作來看，最接近 canonical schema 的東西，還不是獨立命名清楚的一套 domain model。

目前最接近的是：

- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExportModels.cs`
- `Documentation/DeveloperModeOutputContract.md`
- GraphToolkit node / exporter / importer 中分散的節點型別、port 規則、round-trip 規則

這代表目前 repo 仍在過渡期：

- 架構意圖已經是 `Schema-first`
- 但實作上還有一部分 schema 語意分散在 sidecar DTO、GraphToolkit 工具與契約文件裡

這不是方向錯誤，但它提醒我們：

> **之後的收斂方向，應該是讓 schema 本體越來越清楚，而不是讓 sidecar 或 Editor 類別越來越像真相本身。**

## 10. 本文件的實務用途

後續當團隊在討論某個新需求時，應先問：

1. 這個需求新增的是 canonical schema，還是某個 projection？
2. 這個資訊是否必須被 AI 與人類共同理解？
3. 如果要 round-trip，GraphToolkit 和 sidecar 是否都能無資訊遺失地表現？
4. 這個規則是否被錯放到 Unity runtime 或 GraphToolkit UI 細節裡？

只要先問這四句，就比較不容易把短期驗證手段誤寫成長期真相來源。

## 11. 一句總結

本文件要保護的核心原則只有一句：

> **敘事圖的真相應先存在於 schema，之後才投影到 Editor、sidecar、Ink、runtime 與測試。**
