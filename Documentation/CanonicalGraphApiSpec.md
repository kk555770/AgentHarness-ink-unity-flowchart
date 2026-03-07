# Canonical Graph API Spec

> 最後更新：2026/03/06  
> 目的：定義 AI 與程式要如何操作 canonical graph，讓控制面建立在 schema 語意上，而不是建立在 GraphToolkit 或 Unity Editor 手勢上。

## 0. 先講結論

本文件定義的是：

> **如何操作 canonical graph 的語意 API。**

它不定義：

- GraphToolkit 視窗怎麼拖拉
- Unity Inspector 怎麼點
- 某個 transport（HTTP / RPC / C# method）長什麼樣

換句話說，本文件關心的是：

- 操作名稱代表什麼意思
- 輸入輸出至少要有哪些語意
- 錯誤應怎麼分類
- 哪些行為必須 deterministic
- 哪些操作應該 idempotent

## 1. 本文件和其他文件的關係

本文件只回答：

> **怎麼操作真相。**

它不取代其他文件：

- `Documentation/NarrativeGraphArchitecture.md`
  - 解釋為什麼要 schema-first
- `Documentation/CanonicalGraphSchema.md`
  - 定義 canonical truth 的責任邊界
- `Documentation/CanonicalGraphSchemaSpec.md`
  - 定義 graph / node / port / edge 的結構與規則
- `Documentation/CanonicalGraphJsonContract.md`
  - 定義第一個正式 Plain JSON wire contract，讓語意 API 能被機械呼叫
- `Documentation/DeveloperModeOutputContract.md`
  - 定義 projection 到 runtime 時哪些輸出合法

如果用簡單比喻：

- 架構總覽：為什麼
- schema：真相是什麼
- schema spec：真相長什麼樣
- API spec：怎麼動真相
- JSON contract：怎麼把這些操作包成第一個正式 JSON 介面

## 2. 設計原則

### 2.1 Schema-first

API 操作的對象必須是 canonical graph，不是某個 projection。

因此 API 不應直接以：

- `.inkfc`
- `.flowchart.json`
- Unity Scene
- GraphToolkit View State

作為唯一控制面。

### 2.2 Runtime-agnostic

API 不得依賴 Unity 才成立。

如果某個操作只有在 Unity Editor 裡才講得通，那它應屬於 adapter / tooling API，而不是 canonical graph API。

### 2.3 Deterministic by Default

相同 graph 狀態、相同輸入、相同 projection version，應得到相同的驗證與投影結果。

### 2.4 Retry-friendly

AI 操作難免會重試，因此 API 應盡量定義成可安全重試。

### 2.5 Projection is Derived

Projection 是 API 的結果，不是 API 的唯一入口。

## 3. API 形狀

本 spec 定義的是 **語意操作**，不是特定程式語言簽名。

所有操作都應至少能對應到：

- `input`
- `result`
- `warnings[]`
- `errors[]`

這裡描述的是語意形狀，不是特定 transport 的欄位規範。

目前第一個正式的 JSON 封裝方式，請見：

- `Documentation/CanonicalGraphJsonContract.md`

### 3.1 建議的統一結果形狀

```text
OperationResult<T>
  success: bool
  value: T | null
  warnings: Warning[]
  errors: Error[]
  applied: bool
```

說明：

- `success`
  - 代表操作是否成功完成
- `value`
  - 代表操作回傳的主要結果
- `warnings`
  - 代表不阻斷操作，但需要注意的訊息
- `errors`
  - 代表操作失敗原因
- `applied`
  - 代表這次操作是否真的造成狀態變更

`applied` 很重要，因為 AI 常需要分辨：

- 這次是成功改到了東西
- 還是只是重試同一操作，所以結果沒變

## 4. Graph Lifecycle API

## 4.1 `CreateGraph`

### 語意

建立一份新的 canonical graph。

### 最小輸入

- `graphId`（可由呼叫端提供，或由實作產生）
- `version`
- `metadata`（可空）

### 最小結果

- 新 graph 的 snapshot 或等價識別

### 規範

- 若呼叫端提供 `graphId`，重複時必須失敗
- 若實作自動產生 `graphId`，必須保證唯一

## 4.2 `GetGraph`

### 語意

讀取指定 graph 的 canonical snapshot。

### 規範

- `GetGraph` 必須是 read-only
- 不得因為讀取而改變 graph

## 4.3 `ReplaceGraphMetadata`

### 語意

替換 graph-level metadata，不改動 nodes / edges。

### 規範

- metadata 更新不得偷偷改變圖結構

## 5. Node Mutation API

## 5.1 `CreateNode`

### 語意

在 graph 內建立一個新 node。

### 最小輸入

- `nodeId`
- `nodeType`
- `payload`

### 規範

- `nodeType` 必須是 canonical node type
- `payload` 必須符合該 node type 的 schema
- 重複 `nodeId` 必須失敗

### 現況映射註記

Current implementation note：

- 現況 GraphToolkit 建 node 仍偏向 editor internal model
- 現況 sidecar 還在使用 legacy `type = "action"` 表示 canonical `dialogue`

## 5.2 `ReplaceNodePayload`

### 語意

以完整 payload 替換某個 node 的 payload。

### 為什麼採用 Replace，而不是預設 Patch

預設採 `ReplaceNodePayload`，而不是先定義模糊的 patch API，原因是：

- replace 語意比較單純
- 容易 deterministic
- 不容易因 partial merge 規則不同而產生歧義

### 規範

- 若 node 不存在，必須失敗
- 若新 payload 不合法，必須失敗
- replace 不得偷偷修改其他 node 或 edge

## 5.3 `RemoveNode`

### 語意

刪除某個 node，並處理與該 node 相連的 edges。

### 規範

- 預設應連同相關 edges 一起刪除
- 不得留下指向不存在 node 的 dangling edge

### 高影響注意

`RemoveNode` 會直接影響圖結構，因此實作應搭配 validation 或 transaction 機制。

## 6. Edge Mutation API

## 6.1 `ConnectPorts`

### 語意

在兩個合法 port 之間建立一條 edge。

### 最小輸入

- `fromNodeId`
- `fromPort`
- `toNodeId`
- `toPort`

### 規範

- 連線前必須驗證 port 是否存在
- 連線前必須驗證 edge kind 是否合法
- 連線前必須驗證 cardinality 是否被允許

### Idempotency

若完全相同的 edge 已存在，`ConnectPorts` 應視為：

- `success = true`
- `applied = false`

而不是重複建立第二條相同 edge。

這樣比較適合 AI 重試。

## 6.2 `DisconnectEdge`

### 語意

移除一條既有 edge。

### 最小輸入

- `edgeId`
  或
- `(fromNodeId, fromPort, toNodeId, toPort)` 的完整組合

### Idempotency

若目標 edge 不存在，建議語意為：

- `success = true`
- `applied = false`
- 並回傳 warning 或空結果

而不是把「已經不存在」視為硬錯誤。

原因：

- 這對 AI / 自動化重試更友善
- 也比較符合「想要的狀態已經成立」的語意

## 7. Validation API

## 7.1 `ValidateGraph`

### 語意

驗證 graph 是否符合 canonical schema 與 graph invariants。

### 最小結果

- `isValid`
- `errors[]`
- `warnings[]`

### 規範

- `ValidateGraph` 必須是 pure read-only operation
- 不得偷偷修圖
- 不得依賴 UI 狀態

### 至少應覆蓋的驗證類別

- graph-level invariants
- node payload validity
- port existence
- edge legality
- branch completeness
- text payload constraint

### 現況映射註記

Current implementation note：

- 現況 exporter 已承擔大量 validation 責任
- 長期來看，validation 應越來越往 canonical graph API 收斂，而不是只卡在 exporter

## 7.2 `ValidateProjection`

### 語意

驗證 graph 在特定 projection target 下是否可合法投影。

### 用途

例如：

- 檢查能否投影到 Graph v2 sidecar
- 檢查能否投影到 Ink
- 檢查能否進一步編譯成 `story.json`

### 規範

- `ValidateProjection(target)` 不應改 graph
- 它補的是 projection-specific rule，不是取代 `ValidateGraph`

## 8. Projection API

## 8.1 `ProjectGraph`

### 語意

把 canonical graph 投影到指定 target format。

### 最小輸入

- `graphId` 或 graph snapshot
- `target`
- `projectionVersion`（可選，但建議明確）

### 核心 target 類型

本 spec 建議至少區分：

- `flowchart-json`
- `ink`
- `story-json`
- `graphtoolkit-model`

### 規範

- 投影不得改 canonical graph
- 相同 graph + 相同 target + 相同 projection version，應產生語意等價輸出

### 現況映射註記

Current implementation note：

- 現況 Graph v2 sidecar 是最成熟的 round-trip projection
- `.ink` 是目前最重要的文本 projection
- `story.json` 仍依賴 Ink 編譯器 adapter

## 8.2 `ImportProjection`

### 語意

把某個 projection format 還原成 canonical graph。

### 規範

- `ImportProjection` 的結果應是 canonical graph，不是直接回某個 editor internal object
- 若 projection 含 legacy mapping，應回傳 warnings

### 現況映射註記

Current implementation note：

- 現況 importer 主要是 `.flowchart.json + .ink -> .inkfc`
- 長期理想方向應是 `.projection -> canonical graph -> 視覺投影`

## 9. Error Model

至少應有 stable error code，而不是只有自然語言字串。

### 9.1 建議錯誤分類

- `GRAPH_NOT_FOUND`
- `DUPLICATE_GRAPH_ID`
- `DUPLICATE_NODE_ID`
- `NODE_NOT_FOUND`
- `INVALID_NODE_TYPE`
- `INVALID_PAYLOAD`
- `INVALID_PORT`
- `INVALID_EDGE_KIND`
- `EDGE_CARDINALITY_VIOLATION`
- `MISSING_START_NODE`
- `MULTIPLE_START_NODES`
- `CHOICE_BRANCH_INVALID`
- `CONDITION_BRANCH_INVALID`
- `ELSE_BRANCH_MISSING`
- `ELSE_BRANCH_NOT_LAST`
- `FLOW_HIDDEN_IN_TEXT`
- `PROJECTION_UNSUPPORTED`
- `PROJECTION_MAPPING_LOSS`
- `LEGACY_MAPPING_APPLIED`
- `COMPILER_ADAPTER_FAILURE`

### 9.2 Warning 的用途

warning 適合用來表達：

- 仍可成功，但採用了 legacy mapping
- projection 可以完成，但不是最佳形式
- 某些欄位被忽略或自動對映

## 10. Deterministic / Idempotent Rules

### 10.1 必須 deterministic 的操作

- `GetGraph`
- `ValidateGraph`
- `ValidateProjection`
- `ProjectGraph`

### 10.2 建議 idempotent 的操作

- `ConnectPorts`（相同 edge 已存在）
- `DisconnectEdge`（目標 edge 已不存在）

### 10.3 不應被定義成 silent auto-fix 的操作

- `CreateNode`
- `ReplaceNodePayload`
- `ConnectPorts`

如果輸入不合法，應明確失敗，不應偷偷補洞。

## 11. Canonical API 與 Current Implementation 的關係

目前 repo 中最接近 API 行為的東西，仍分散在：

- GraphToolkit 建圖流程
- exporter / importer
- 測試 fixture

這些可以視為 **current implementation hints**，但不應直接升格成 canonical API。

例如：

- 現況 `GraphDatabase.CreateGraph<InkFlowChartGraph>` 是 Editor 工具流程
- 但 canonical API 的語意應是 `CreateGraph`
- 現況 exporter 的 `TryValidateGraphForExport` 是 projection 驗證
- 但 canonical API 的語意應拆成 `ValidateGraph` 與 `ValidateProjection`

## 12. AI 使用範例

以下示意的，不是 transport 規格，而是語意流程：

```text
CreateGraph(graphId="chapter-01", version="canon-1")
CreateNode(nodeId="N001", type="start", payload={})
CreateNode(nodeId="N002", type="dialogue", payload={ content: "你好。" })
CreateNode(nodeId="N003", type="choice", payload={
  mode: "once",
  branches: [
    { index: 0, label: "去 A" },
    { index: 1, label: "去 B" }
  ]
})

ConnectPorts(N001, "flow-out", N002, "flow-in")
ConnectPorts(N002, "flow-out", N003, "flow-in")
ValidateGraph()
ProjectGraph(target="flowchart-json")
ProjectGraph(target="ink")
```

如果 AI 控制面真的是這種語意層，未來 projection 或 runtime 換掉時，控制邏輯才不會全部重寫。

## 13. 一句總結

本文件要保護的核心原則只有一句：

> **AI 與程式應操作 canonical graph 的語意 API，而不是操作 GraphToolkit 或 Unity Editor 的手勢 API。**

補一句 transport 分層版本：

> **Plain JSON、JSON-RPC、HTTP 或 CLI 都只能是外層包裝；不能反過來改寫 canonical graph 的語意 API。**
