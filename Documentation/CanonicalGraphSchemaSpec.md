# Canonical Graph Schema Spec

> 最後更新：2026/03/06  
> 目的：把 canonical graph 的最小結構、節點型別、port/edge 語意、圖規則，以及目前 sidecar / GraphToolkit 的映射正式寫清楚。

## 0. 讀這份文件前先知道

這份文件是 **schema spec**，不是目前 script 的逐行翻譯。

如果 script 現況與更能標準化、可長期維護的 schema 設計衝突，**以本 spec 的規範層為優先**。

但為了不脫離現況，本文件也會保留：

- 現況 projection mapping
- 現況 sidecar DTO 對應
- 過渡命名與 legacy token

也就是說，本文件同時分兩層：

- **規範層（Normative）**
- **現況映射層（Current Implementation Note）**

## 1. 名詞與優先順序

### 1.1 Canonical Graph

Canonical graph 指的是：

> **不依賴 GraphToolkit、不依賴 Unity、也不依賴某一份 sidecar 格式的敘事圖真相。**

### 1.2 Projection

Projection 指的是把 canonical graph 投影到某種工作格式，例如：

- GraphToolkit 視覺圖
- `.flowchart.json`
- `.ink`
- `story.json`

### 1.3 優先順序

當文件或 script 發生衝突時，解讀順序應為：

1. 使用者已確認的架構北極星
2. `Documentation/NarrativeGraphArchitecture.md`
3. `Documentation/CanonicalGraphSchema.md`
4. 本文件的規範層
5. 現況 script / sidecar / GraphToolkit 類別

## 2. Canonical Naming Decision

### 2.1 已確認的 canonical node type 命名

本專案在 canonical schema 層，正式採用：

- `dialogue`

而不是：

- `action`

原因很簡單：

- 目前作者看到的節點就是「對話」
- 真正的動作資料節點已經另外拆成 `stageAction`
- 若 canonical schema 仍沿用 `action`，長期會污染 AI API 與資料模型語意

### 2.2 現況映射註記

Current implementation note：

- 現況 Graph v2 sidecar 仍使用 `type = "action"`
- 其實對應的是目前的 `InkFlowDialogueNode`
- 因此 canonical schema 與現況 sidecar 之間需要一層明確 mapping
- 若 `Documentation/DeveloperModeOutputContract.md` 在現況 projection 說明裡提到 `action`，應解讀成「current sidecar token」，不是 canonical 命名裁決回頭改回 `action`

## 3. Graph 結構

## 3.1 Graph 最小欄位

Canonical `Graph` 至少應包含：

- `graphId`
- `version`
- `nodes`
- `edges`

可選但常見的 graph-level metadata：

- `name`
- `createdAt`
- `updatedAt`
- `authoringHints`

### 3.2 規範層要求

- `graphId` 必須穩定可識別
- `version` 必須屬於 canonical schema 的版本，而不是 projection 專用版本
- `nodes` 與 `edges` 應為 canonical structure 的一級欄位

### 3.3 現況映射註記

Current implementation note：

- 現況 `.flowchart.json` 的 `ExportGraphDto` 只有：
  - `version`
  - `graphName`
  - `startNodeId`
  - `nodes`
- 現況 sidecar 尚未把 `edges` 獨立成一級欄位，而是把 adjacency 內嵌在 `nodes[*].outputs[]`

## 4. Node 結構

### 4.1 Node 最小欄位

Canonical `Node` 至少應包含：

- `nodeId`
- `nodeType`
- `payload`

### 4.2 規範層要求

- `nodeId` 必須在 graph 內唯一
- `nodeType` 必須是穩定語意名稱，不能依賴 UI 顯示名稱
- `payload` 只能描述 node 語意，不描述 GraphToolkit 視窗細節

## 5. Port 與 Edge 結構

### 5.1 Canonical Port 語意分類

Canonical schema 至少需要能區分以下 port 類型：

1. `flow-in`
2. `flow-out`
3. `branch-out[index]`
4. `action-data-out`
5. `dialogue-action-in[index]`

這裡的重點不是字串長什麼樣，而是語意必須能被穩定區分。

### 5.2 Canonical Edge 最小欄位

Canonical `Edge` 至少應包含：

- `fromNodeId`
- `fromPort`
- `toNodeId`
- `toPort`
- `edgeKind`

其中 `edgeKind` 至少應能區分：

- `flow`
- `data`
- `branch`

### 5.3 規範層要求

- edge 必須是第一級語意，不應只存在於某個 projection 的便利欄位裡
- 流程線與資料線必須可明確區分
- 分支線必須保留 branch slot 的順序語意

### 5.4 現況映射註記

Current implementation note：

- Graph v2 sidecar 目前沒有獨立 `edges[]`
- 現況用 `nodes[*].outputs[*]` 近似表達 edge
- 目前最常見的 projection port string 是：
  - `Flow`
  - `Out0`, `Out1`, ...
  - `ActionData`
  - `ActionIn0`, `ActionIn1`, ...

## 6. Canonical Node Types

目前 canonical schema spec 先正式收錄以下 node types：

- `start`
- `dialogue`
- `stageAction`
- `comment`
- `choice`
- `condition`

目前**不納入 canonical 核心節點集合**，但可能存在於 current projection / authoring data layer 的節點：

- `character`
- `castBundle`

原因不是它們不重要，而是：

- 目前正式文件主要在 `Documentation/DeveloperModeOutputContract.md` 描述它們
- 現階段尚未有對應的 canonical payload / port / edge spec
- 也尚未看到和六大核心節點同等級的 GraphToolkit 實作主線與 round-trip 測試護欄

因此在本 spec 中，`character / castBundle` 應先被視為 projection-heavy / authoring data-source nodes，而不是 AI API 預設必須支援的 canonical core types。

## 6.1 `start`

### Payload

`start` 的 canonical payload 預設為空物件。

### Port 規則

- 不應有 `flow-in`
- 應有且僅有 1 個 `flow-out`

### Graph Invariant

- graph 必須且只能有 1 個 `start`

### 現況映射註記

- 現況 sidecar：`type = "start"`
- 現況 GraphToolkit：`InkFlowStartNode`

## 6.2 `dialogue`

### Payload

Canonical `dialogue.payload` 最小欄位：

- `content: string`

### Port 規則

- 應有 1 個 `flow-in`
- 應有至多 1 個 `flow-out`
- 可有 `0..n` 個 `dialogue-action-in[index]`

### Graph Invariant

- `dialogue` 的主流程不能多路分岔；若要分岔必須改用 `choice` 或 `condition`
- `dialogue` 的對話內容不得偷藏流程語法

### 現況映射註記

- 現況 GraphToolkit 類別：`InkFlowDialogueNode`
- legacy 相容類別：`InkFlowActionNode`
- 現況 sidecar：
  - `type = "action"`
  - `content = 對話內容`
  - `actionKind = "dialogue"`
- `ActionInputCount` 是目前 projection / authoring convenience 欄位，不應視為 canonical payload 正式欄位
- `dialogue-action-in[index]` 在現況 projection 以 `ActionIn*` 命名
- `Documentation/DeveloperModeOutputContract.md` 若在 current Graph v2 契約中使用 `action`，應讀成這裡的 legacy sidecar token，而不是 canonical `nodeType`

## 6.3 `stageAction`

### Payload

Canonical `stageAction.payload` 最小欄位：

- `content: string`

### Port 規則

- 不應有 `flow-in`
- 不應有 `flow-out`
- 應有且僅有 1 個 `action-data-out`

### Graph Invariant

- 每個 `stageAction` 必須且只能連到 1 個 `dialogue-action-in[index]`
- `stageAction` 自身不應單獨形成主流程節點

### 現況映射註記

- 現況 sidecar：`type = "stageAction"`
- 現況 sidecar：
  - `content = 動作內容`
  - `actionKind = "action"`
- 現況 projection port：
  - `portName = "ActionData"`
  - `toPortName = "ActionIn*"`

## 6.4 `comment`

### Payload

Canonical `comment.payload` 最小欄位：

- `note: string`

### Port 規則

- 應有 1 個 `flow-in`
- 應有至多 1 個 `flow-out`

### Graph Invariant

- `comment` 不得改變流程結構，只能附著在既有流程上

### 現況映射註記

- 現況 sidecar：`type = "comment"`
- 現況 sidecar `content` 欄位承載註解文字
- canonical payload 名稱可正規化為 `note`

## 6.5 `choice`

### Payload

Canonical `choice.payload` 最小欄位：

- `mode: "once" | "repeatable"`
- `branches[]`

其中每個 branch 至少應包含：

- `index`
- `label`

### Port 規則

- 應有 1 個 `flow-in`
- 應有 `1..n` 個 `branch-out[index]`

### Graph Invariant

- 每個 branch output 必須且只能接 1 條 branch edge
- 每個 branch 必須有非空 `label`
- `label` 不得包含會破壞 projection 的特殊保留字元

### 現況映射註記

- 現況 sidecar：`type = "choice"`
- 現況 sidecar 用：
  - `choiceMode = "*" | "+"`
  - `outputs[*].label`
- 現況 projection port：`Out0`, `Out1`, ...
- `OutputCount` 是目前 GraphToolkit 投影便利欄位，可由 branch 數量導出，不應視為 canonical payload 正式欄位

## 6.6 `condition`

### Payload

Canonical `condition.payload` 最小欄位：

- `branches[]`

其中：

- 非 else branch 至少應包含：
  - `index`
  - `expression`
- else branch 至少應包含：
  - `index`
  - `isElse = true`

### Port 規則

- 應有 1 個 `flow-in`
- 應有至少 2 個 `branch-out[index]`

### Graph Invariant

- 最後一個 branch 必須是 else
- 只有最後一個 branch 可以是 else
- 非 else branch 必須有非空 expression
- 每個 branch output 必須且只能接 1 條 branch edge

### 現況映射註記

- 現況 sidecar：`type = "condition"`
- 現況 sidecar 用：
  - `outputs[*].condition`
  - `outputs[*].isElse`
- 現況 projection port：`Out0`, `Out1`, ...
- `OutputCount` 是目前 GraphToolkit 投影便利欄位，可由 branch 數量導出，不應視為 canonical payload 正式欄位

## 7. Graph Invariants

以下規則屬於 canonical schema 應維持的圖規則：

1. graph 內必須且只能有 1 個 `start`
2. 線性流程節點不得以多條流程線冒充分岔
3. `stageAction` 只能透過資料線連到 `dialogue`
4. `choice` 的每個 branch 都必須完整接線
5. `condition` 的每個 branch 都必須完整接線，且 else 規則正確
6. 主流程結構不得藏在自由文字裡

## 8. Text Payload Constraint

`dialogue` / `stageAction` / `comment` 的文字 payload 在 schema 層應遵守：

- 不得用自由文字偷藏流程結構
- 分支、跳轉、條件、匯流應由節點與 edge 表達

### 現況映射註記

Current implementation note：

- 現況 exporter 已主動拒絕多種未跳脫 Ink 結構語法，例如：
  - `->`
  - `<-`
  - `=`
  - `*`
  - `+`
  - `-`
  - `INCLUDE`

這些規則目前以 projection / export validation 形式存在，但語意上屬於 canonical schema 應維持的約束。

## 9. Canonical Schema 與 Current Sidecar Mapping

### 9.1 Graph 層

- canonical `graphId`
  - 現況 sidecar：未正式存在
- canonical `version`
  - 現況 sidecar：`ExportGraphDto.version`
- canonical `name`
  - 現況 sidecar：`graphName`
- canonical `edges[]`
  - 現況 sidecar：投影在 `nodes[*].outputs[]`

### 9.2 Node Type 層

- canonical `start`
  - 現況 sidecar `type = "start"`
- canonical `dialogue`
  - 現況 sidecar `type = "action"` + `actionKind = "dialogue"`
- canonical `stageAction`
  - 現況 sidecar `type = "stageAction"` + `actionKind = "action"`
- canonical `comment`
  - 現況 sidecar `type = "comment"`
- canonical `choice`
  - 現況 sidecar `type = "choice"`
- canonical `condition`
  - 現況 sidecar `type = "condition"`

### 9.3 Port / Edge 層

- canonical `flow-out` / `flow-in`
  - 現況 sidecar：`Flow`
- canonical `branch-out[index]`
  - 現況 sidecar：`Out{index}`
- canonical `action-data-out`
  - 現況 sidecar：`ActionData`
- canonical `dialogue-action-in[index]`
  - 現況 sidecar：`ActionIn{index}`

## 10. Projection-Only Fields

以下欄位目前屬於 projection / current implementation convenience，不應直接升格成 canonical truth：

- `nextIds`
  - v1 相容欄位
- `ActionInputCount`
  - GraphToolkit authoring convenience
- `ChoiceOutputCount` / `ConditionOutputCount`
  - GraphToolkit authoring convenience
- `actionKind`
  - 在節點型別尚未完全正規化前的過渡欄位

以下節點類型目前也更接近 projection / authoring data-source 設計，而不是 canonical core types：

- `character`
- `castBundle`

若未來要升格它們，至少應先補齊：

- stable payload
- stable port / edge semantics
- importer / exporter mapping
- round-trip tests

## 11. AI API 含義

只要 canonical schema 清楚，AI 的穩定控制面就應直接對應到 schema，而不是對應到 GraphToolkit 視窗動作。

例如：

```text
CreateNode(type="dialogue", payload={ content: "..." })
CreateNode(type="stageAction", payload={ content: "# action:alice" })
Connect(stageActionA.action-data-out, dialogue1.dialogue-action-in[0])
Connect(dialogue1.flow-out, choice1.flow-in)
```

如果 AI 要操作的是這種語意層，未來不管 projection 或 runtime 換掉，控制面都不需要跟著壞掉。

更完整的操作命令、錯誤模型、deterministic / idempotent 規則，請見：

- `Documentation/CanonicalGraphApiSpec.md`

## 12. 一句總結

本 spec 的核心裁決只有一句：

> **canonical schema 應以 `dialogue`、`stageAction`、`choice`、`condition` 等穩定語意為核心；現況 sidecar 的 `action`、`nextIds`、`ActionInputCount` 等欄位，只能視為 projection / legacy mapping。**
