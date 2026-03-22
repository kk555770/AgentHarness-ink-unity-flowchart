# Batch 9 GetGraph snapshot bridge

## 目標

- 補上 `GetGraph` 與 canonical snapshot JSON bridge。
- 讓 Batch 8 的 JSON control plane 從只能下指令，進化成最小可讀可寫回圈。
- 不碰 WebView / GraphToolkit / Runtime 主線。

## 步驟

1. 盤點現有 JSON request / response / dispatcher 與 contract。
2. 設計 snapshot 型別與 mapper。
3. 在 dispatcher 補 `GetGraph`。
4. 補 snapshot / GetGraph 測試。
5. 跑新測試與 EditMode / PlayMode gate。

## 目前狀態

- 已完成步驟 1-4：
  - 補上 snapshot 型別
  - 補上 snapshot mapper
  - dispatcher 已新增 `GetGraph`
  - 已新增 GetGraph / snapshot 測試
- 目前進入步驟 5，準備先跑 Batch 9 新測試，再視結果跑全量 gate。
