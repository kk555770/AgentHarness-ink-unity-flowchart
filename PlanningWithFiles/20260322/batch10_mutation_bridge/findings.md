# Findings

- Batch 10 最值得先補的 editable mutation set 是：
  - `ReplaceNodePayload`
  - `DisconnectEdge`
  - `RemoveNode`
- `ReplaceGraphMetadata` 可以做，但不應擠進前三優先順序。
- 若沒有這三個操作，前端 / AI 會卡在：
  - 節點內容只能重建不能改
  - 接錯線不能拆
  - 建錯節點不能刪
- 最小實作順序建議：
  - `ReplaceNodePayload`
  - `DisconnectEdge`
  - `RemoveNode`
- `DisconnectEdge` 應維持 idempotent：
  - 目標 edge 已不存在時，`success = true`
  - `applied = false`
- `DisconnectEdge` 的最小輸入可採：
  - `edgeId`
  - 或完整 `(fromNodeId, fromPort, toNodeId, toPort)`
- `RemoveNode` 刪除時必須一併清掉相關 edges，不可留下 dangling edge。
- `RemoveNode` 這一批先採 strict 語意：
  - node 不存在時直接失敗
  - 不把「不存在」視為 no-op
- `ReplaceNodePayload` 應採完整 replace，而不是 patch。
- `ReplaceNodePayload` 若作用在 `choice / condition`，需要依新 payload 重新推導 `branchCount`，
  否則 payload 與 branch 結構會漂移。
- 這一批仍不應直接做 WebView，因為目前最缺的不是 host，而是 canonical control plane 的可編輯寫入面。
