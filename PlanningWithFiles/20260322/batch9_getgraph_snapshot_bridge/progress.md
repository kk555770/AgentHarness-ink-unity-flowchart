# Progress

- 2026-03-22：開始 Batch 9 前盤點。
- 2026-03-22：已重讀 `CanonicalGraphApiSpec.md`、`CanonicalGraphJsonContract.md`、`CanonicalGraphJsonRequest.cs`、`CanonicalGraphJsonResponse.cs`、`CanonicalGraphJsonCommandDispatcher.cs`。
- 2026-03-22：已完成兩條唯讀 sidecar：
  - 契約盤點：確認 `GetGraph` 最小輸入只需 `graphId`，且必須 read-only
  - 結構審查：確認最穩切法是 `dispatcher + snapshot mapper`，不動 command service
- 2026-03-22：已新增 `CanonicalGraphJsonGraphSnapshot`、`CanonicalGraphJsonSnapshotMapper`。
- 2026-03-22：已在 `CanonicalGraphJsonCommandDispatcher` 補上 `GetGraph`。
- 2026-03-22：已新增 `CanonicalGraphJsonGetGraphTests`、`CanonicalGraphJsonSnapshotMapperTests`，並更新 `CanonicalGraphJsonContractRoundTripTests` 的 snapshot round-trip 護欄。
- 2026-03-22：Batch 9 新測試第一次失敗，根因不是功能邏輯，而是 `JsonUtility` 會把 null snapshot 欄位序列化成空物件殼；已把測試判準修正為檢查空 snapshot。
- 2026-03-22：已重跑 Batch 9 新測試，結果 `17/17 passed`。
- 2026-03-22：已跑 full EditMode gate，結果 `303 total / 299 passed / 0 failed / 4 skipped`；`OpsidanosInk.EditModeTests.dll` 為 `85/85 passed`。
- 2026-03-22：已跑 PlayMode gate，結果 `221 total / 130 passed / 0 failed / 91 skipped`；`OpsidanosInk.PlayModeTests.dll` 為 `17/17 passed`。
- 2026-03-22：`Assets/OffMeshLinkScene.unity` 的測試副作用已 restore。
