# Batch 8 plain JSON command bridge

## 目標

- 把 `CanonicalGraphApiSpec` 與 `CanonicalGraphJsonContract` 落成第一版可呼叫程式。
- 建立最小 JSON request / response / dispatcher / error mapper。
- 先支援：
  - `CreateGraph`
  - `CreateNode`
  - `ConnectPorts`
  - `ValidateGraph`
- 不碰 WebView / Browser / Electron 殼。

## 步驟

1. 盤點 API spec、JSON contract 與現有 `CanonicalGraphCommandService` 可直接重用的欄位。
2. 設計最小 JSON request / response 型別與 graph store。
3. 實作 `CanonicalGraphJsonCommandDispatcher` 與 `CanonicalGraphJsonErrorMapper`。
4. 補 dispatcher / contract 測試。
5. 跑 EditMode / PlayMode gate 驗證。

## 目前狀態

- 已完成步驟 1-4：
  - 新增 JSON request / response 型別
  - 新增 dispatcher 與 error mapper
  - 新增 dispatcher / contract 測試
- 目前進入步驟 5，準備先跑新測試，再跑 EditMode / PlayMode gate。
