# Batch 7 importer canonical-first 收斂

## 目標

- 讓 `InkFlowChartImporter` 以 `CanonicalGraphDocument` 當主入口，避免 importer 自己顯式編排 `canonical -> normalized DTO -> import plan`。
- 在 `CurrentFlowImportService` 補 canonical-first 入口，讓 importer 與 exporter 的主線更對稱。
- 維持 `CurrentFlowGraphToolkitImportAdapter` 為 shell-only 的 GraphToolkit rebuild 層。

## 步驟

1. 盤點 importer、import service、GraphToolkit rebuild 與既有測試接點。
2. 在 `CurrentFlowImportService` 新增 canonical-first import plan 入口。
3. 將 `InkFlowChartImporter` 改接 canonical-first import service。
4. 視需要小幅調整 `CurrentFlowCanonicalGraphAdapter` 共用 helper。
5. 新增 canonical-first import service 測試。
6. 跑 EditMode / PlayMode gate 驗證。

## 完成狀態

- [x] 盤點 importer、import service、GraphToolkit rebuild 與既有測試接點
- [x] 在 `CurrentFlowImportService` 新增 canonical-first import plan 入口
- [x] 將 `InkFlowChartImporter` 改接 canonical-first import service
- [x] 新增 `CurrentFlowCanonicalImportServiceTests`
- [x] 跑 EditMode / PlayMode 驗證
