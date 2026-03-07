# Canonical Graph JSON Contract

> 最後更新：2026/03/06  
> 目的：定義 canonical graph 的第一個正式 JSON wire contract，讓 AI 與程式可以用機械可讀的方式呼叫語意 API，同時不把 transport 外殼誤當成真相本體。

## 0. 先講結論

本文件定義的是：

> **canonical graph 語意 API 的第一個正式 Plain JSON contract。**

它的定位是：

- 第一個正式的 machine-callable JSON 介面
- 薄 envelope 的預設控制格式
- 對 canonical API 的直接 JSON 映射

它不是：

- canonical truth
- projection format
- GraphToolkit sidecar
- JSON-RPC

如果用 8 歲小孩也懂的方式講：

- `CanonicalGraphApiSpec` 像在講「你可以做哪些事」
- 本文件像在講「如果要把這些事寫成 JSON 指令，格式長什麼樣」

## 1. 本文件和其他文件的關係

本文件只回答：

> **canonical graph 語意操作，落成第一個正式 JSON wire format 時要長什麼樣。**

它不取代其他文件：

- `Documentation/NarrativeGraphArchitecture.md`
  - 解釋為什麼 schema-first 與 transport 分層很重要
- `Documentation/CanonicalGraphSchema.md`
  - 定義 canonical truth 的責任邊界
- `Documentation/CanonicalGraphSchemaSpec.md`
  - 定義 graph / node / port / edge 的結構與 invariant
- `Documentation/CanonicalGraphApiSpec.md`
  - 定義操作名稱、語意責任、錯誤分類與 deterministic / idempotent 規則
- `Documentation/DeveloperModeOutputContract.md`
  - 定義投影到 Ink / Runtime 的輸出契約

這代表：

- 本文件不重新發明 `CreateNode`
- 本文件也不重新定義 `dialogue` / `choice` / `condition`
- 本文件只把既有語意 API 包成固定 JSON 形狀

## 2. 設計原則

### 2.1 Thin Envelope

本 contract 採用薄 envelope。

意思是：

- 只保留最少必要欄位
- 不預設帶入 RPC 框架語意
- 不預設帶入 batch / notification / protocol negotiation

### 2.2 Schema-first

JSON 欄位只能包裝 canonical API，不能反過來改寫 canonical schema。

因此：

- JSON contract 不能自行新增節點語意
- JSON contract 不能把 projection 欄位升格成真相
- JSON contract 不能把 `.flowchart.json` sidecar 當作 request / response 格式

### 2.3 Transport-light

這份 contract 的目的，是先把控制面定穩。

所以它優先處理：

- 操作名稱
- 輸入欄位
- 回傳欄位
- warning / error
- idempotent / deterministic 對結果形狀的影響

而不是先處理：

- RPC id
- notification
- batch
- server capability negotiation

### 2.4 JSON-RPC 可後加

未來若要導入 JSON-RPC，應該做成：

> **包裝本 contract 的 adapter。**

而不是回頭改寫 canonical API 或 schema。

## 3. Request Envelope

每個 request 至少應包含：

- `contractVersion`
- `operation`
- `input`

### 3.1 最小 request 形狀

```json
{
  "contractVersion": "plain-json-1",
  "operation": "CreateNode",
  "input": {
    "graphId": "chapter-01",
    "node": {
      "nodeId": "N002",
      "nodeType": "dialogue",
      "payload": {
        "content": "你好。"
      }
    }
  }
}
```

### 3.2 欄位說明

- `contractVersion`
  - 代表這份 JSON wire contract 的版本
  - 不是 canonical schema version
  - 不是 projection version
- `operation`
  - 必須對應 `CanonicalGraphApiSpec.md` 中的語意操作名稱
- `input`
  - 必須符合該操作的最小輸入要求

### 3.3 不建議預設加入的欄位

第一版不建議把下列欄位列為必填：

- `requestId`
- `traceId`
- `sessionId`
- `batch`
- `notification`

原因很簡單：

- 它們屬於傳輸或服務治理問題
- 不是現在最需要先立穩的敘事圖控制面

若某個實作真的需要 request correlation，可在實作層額外加可選欄位，但不應回頭污染 canonical API。

## 4. Response Envelope

每個 response 至少應包含：

- `contractVersion`
- `success`
- `result`
- `warnings`
- `errors`
- `applied`

### 4.1 最小 response 形狀

```json
{
  "contractVersion": "plain-json-1",
  "success": true,
  "result": {
    "nodeId": "N002"
  },
  "warnings": [],
  "errors": [],
  "applied": true
}
```

### 4.2 欄位說明

- `contractVersion`
  - 回應端實際採用的 JSON contract 版本
- `success`
  - 此次操作是否成功完成
- `result`
  - 該次操作的主要回傳值
- `warnings`
  - 不阻斷操作，但呼叫端應注意的訊息
- `errors`
  - 導致操作失敗的結構化錯誤
- `applied`
  - 這次操作是否真的造成狀態變更

## 5. Warning / Error 結構

### 5.1 Warning 最小形狀

```json
{
  "code": "LEGACY_MAPPING_APPLIED",
  "message": "已套用 legacy action -> dialogue 對映。",
  "details": {
    "legacyNodeType": "action",
    "canonicalNodeType": "dialogue"
  }
}
```

### 5.2 Error 最小形狀

```json
{
  "code": "INVALID_PORT",
  "message": "目標輸入埠不存在。",
  "details": {
    "graphId": "chapter-01",
    "nodeId": "N002",
    "toPort": "flow-in-typo"
  }
}
```

### 5.3 規範

- `code`
  - 必須是 stable error / warning code
  - 不得只靠自然語言訊息辨識
- `message`
  - 給人讀的短訊息
- `details`
  - 給機械或除錯看的結構化資料

錯誤碼本體應以 `Documentation/CanonicalGraphApiSpec.md` 為準。

## 6. Canonical Operation Mapping

本 contract 的 `operation` 必須直接對應 canonical API。

目前第一版至少包含：

- `CreateGraph`
- `GetGraph`
- `ReplaceGraphMetadata`
- `CreateNode`
- `ReplaceNodePayload`
- `RemoveNode`
- `ConnectPorts`
- `DisconnectEdge`
- `ValidateGraph`
- `ValidateProjection`
- `ProjectGraph`
- `ImportProjection`

這些名稱的語意、最小輸入與規範，正式以 `Documentation/CanonicalGraphApiSpec.md` 為準。

## 7. JSON 形狀範例

### 7.1 `CreateGraph`

```json
{
  "contractVersion": "plain-json-1",
  "operation": "CreateGraph",
  "input": {
    "graphId": "chapter-01",
    "version": "canon-1",
    "metadata": {
      "title": "第一章"
    }
  }
}
```

```json
{
  "contractVersion": "plain-json-1",
  "success": true,
  "result": {
    "graphId": "chapter-01"
  },
  "warnings": [],
  "errors": [],
  "applied": true
}
```

### 7.2 `CreateNode`

```json
{
  "contractVersion": "plain-json-1",
  "operation": "CreateNode",
  "input": {
    "graphId": "chapter-01",
    "node": {
      "nodeId": "N002",
      "nodeType": "dialogue",
      "payload": {
        "content": "我們終於到了。"
      }
    }
  }
}
```

### 7.3 `ConnectPorts`

```json
{
  "contractVersion": "plain-json-1",
  "operation": "ConnectPorts",
  "input": {
    "graphId": "chapter-01",
    "fromNodeId": "N001",
    "fromPort": "flow-out",
    "toNodeId": "N002",
    "toPort": "flow-in"
  }
}
```

### 7.4 `DisconnectEdge`

```json
{
  "contractVersion": "plain-json-1",
  "operation": "DisconnectEdge",
  "input": {
    "graphId": "chapter-01",
    "fromNodeId": "N001",
    "fromPort": "flow-out",
    "toNodeId": "N002",
    "toPort": "flow-in"
  }
}
```

若目標 edge 已不存在，建議回應：

```json
{
  "contractVersion": "plain-json-1",
  "success": true,
  "result": null,
  "warnings": [],
  "errors": [],
  "applied": false
}
```

### 7.5 `ValidateGraph`

```json
{
  "contractVersion": "plain-json-1",
  "operation": "ValidateGraph",
  "input": {
    "graphId": "chapter-01"
  }
}
```

```json
{
  "contractVersion": "plain-json-1",
  "success": true,
  "result": {
    "isValid": true
  },
  "warnings": [],
  "errors": [],
  "applied": false
}
```

### 7.6 `ProjectGraph`

```json
{
  "contractVersion": "plain-json-1",
  "operation": "ProjectGraph",
  "input": {
    "graphId": "chapter-01",
    "target": "flowchart-json",
    "projectionVersion": "graph-v2"
  }
}
```

## 8. Idempotent / Deterministic 對 JSON Contract 的影響

### 8.1 Idempotent 操作

對建議 idempotent 的操作：

- `ConnectPorts`
- `DisconnectEdge`

若請求重送後，目標狀態本來就已成立，建議回應：

- `success = true`
- `applied = false`

這樣 AI 比較容易判斷：

- 這次沒有失敗
- 只是沒有新的狀態變更

### 8.2 Deterministic 操作

對下列操作：

- `GetGraph`
- `ValidateGraph`
- `ValidateProjection`
- `ProjectGraph`

相同 graph 狀態與相同輸入，應得到語意等價的 `result`。

## 9. 這份 JSON Contract 不包含什麼

為了避免把外層協議過早做重，第一版明確不包含：

- JSON-RPC 的 `jsonrpc` / `id` / `method` / `params`
- HTTP 路徑設計
- CLI 參數格式
- streaming
- notification
- batch
- capability negotiation

這些都可以在未來加，但應該作為 adapter，而不是回頭污染 canonical API。

## 10. 和 `.flowchart.json` 的差別

這是非常重要的邊界。

`CanonicalGraphJsonContract` 和 `.flowchart.json` 完全不是同一層東西。

- 本文件
  - 是 AI / 程式控制 canonical API 的 JSON wire contract
- `.flowchart.json`
  - 是 Graph v2 的 sidecar / interchange / round-trip projection format

一個是在講：

- 「怎麼叫系統做事」

另一個是在講：

- 「圖投影出來之後長什麼樣」

兩者都可能是 JSON，  
但它們不是同一種 JSON。

## 11. 未來與 JSON-RPC 的關係

若未來要導入 JSON-RPC，建議做成：

```text
JSON-RPC request
  -> 轉成 Plain JSON contract
  -> 對應 canonical operation
  -> 取得標準 response
  -> 再包回 JSON-RPC response
```

也就是：

- Plain JSON contract 先定義控制面
- JSON-RPC 再負責包裝 transport 標準

這樣後續要同時支援：

- 本地直接呼叫
- CLI
- agent bridge
- 遠端服務

都比較穩。

## 12. 一句總結

本文件要保護的核心原則只有一句：

> **先把 canonical graph 的控制面用薄而硬的 Plain JSON contract 定穩，再考慮用 JSON-RPC 等外層協議去包它。**
