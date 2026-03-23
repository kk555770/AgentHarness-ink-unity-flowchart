# Batch 10 mutation bridge

## 目標

- 補上最小 editable mutation set：`ReplaceNodePayload`、`DisconnectEdge`、`RemoveNode`。
- 讓 canonical JSON control plane 從可讀可寫最小回圈，進化成真正可編輯的 authoring control plane。
- 不碰 WebView / GraphToolkit / Runtime 主線。

## 步驟

1. [完成] 盤點 mutation API spec、JSON contract 與現有 command service / dispatcher 切點。
2. [完成] 在 command service 補 mutation 操作。
3. [完成] 在 JSON dispatcher / request 補對應操作。
4. [完成] 補 mutation 測試。
5. [完成] 跑 targeted tests 與 EditMode / PlayMode gate。
