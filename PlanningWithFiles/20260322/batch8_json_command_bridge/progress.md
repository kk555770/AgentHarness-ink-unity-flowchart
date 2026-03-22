# Progress

- 2026-03-22：開始 Batch 8 實作前盤點。
- 2026-03-22：已重讀 `CanonicalGraphApiSpec.md`、`CanonicalGraphJsonContract.md`、`CanonicalGraphCommandService.cs`、`CanonicalGraphOperationResult.cs`、`CanonicalGraphValidationResult.cs`。
- 2026-03-22：已確認 Batch 8 第一版採最小 JSON bridge，不先碰 WebView。
- 2026-03-22：已確認不先把 `Newtonsoft.Json` 拉進 core，而是先用 `JsonUtility + 固定 input shape` 落地第一版。
- 2026-03-22：已新增 `CanonicalGraphJsonRequest`、`CanonicalGraphJsonResponse`、`CanonicalGraphJsonErrorMapper`、`CanonicalGraphJsonCommandDispatcher`。
- 2026-03-22：已新增 `CanonicalGraphJsonCommandDispatcherTests` 與 `CanonicalGraphJsonContractRoundTripTests`。
- 2026-03-22：目前進入本地驗證階段，先跑新測試，再視結果補跑 EditMode / PlayMode gate。
- 2026-03-22：已跑 `CanonicalGraphJsonCommandDispatcherTests` 與 `CanonicalGraphJsonContractRoundTripTests`，結果 `9/9 passed`，`Batch8JsonBridgeTests.xml` 已生成。
- 2026-03-22：已跑 full EditMode gate，結果 `295 total / 291 passed / 0 failed / 4 skipped`；`OpsidanosInk.EditModeTests.dll` 為 `77/77 passed`。
- 2026-03-22：EditMode 後出現 `Assets/OffMeshLinkScene.unity` 與 `Assets/TmpScenes/` 測試副作用，先記錄，待 PlayMode 完成後再統一處理。
- 2026-03-22：reviewer 指出 dispatcher 仍有 `重複 graphId` 與 `graphId trim/lookup 不一致` 兩個語意洞，已在本輪補修並新增對應測試案例，準備重跑新測試確認。
- 2026-03-22：補修後重新跑 Batch 8 新測試，結果 `11/11 passed`。
- 2026-03-22：補修後重新跑 full EditMode gate，結果 `297 total / 293 passed / 0 failed / 4 skipped`；`OpsidanosInk.EditModeTests.dll` 為 `79/79 passed`。
- 2026-03-22：補修後重新跑 PlayMode gate，結果 `221 total / 130 passed / 0 failed / 91 skipped`；`OpsidanosInk.PlayModeTests.dll` 為 `17/17 passed`。
- 2026-03-22：`Assets/OffMeshLinkScene.unity` 已 restore，`Assets/TmpScenes/` 在重跑過程後未再留存。
