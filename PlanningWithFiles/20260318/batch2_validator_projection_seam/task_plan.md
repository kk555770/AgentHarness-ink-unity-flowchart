# 任務計畫：Batch 2 validator / projection seam

## 目標
開始落地 Batch 2：先把 current projection 的 shared DTO 與 validator 從 `InkFlowChartExporter.cs` 抽出來，讓匯出器先退一步，不再同時握有 DTO 定義與整包語意驗證。

## 當前階段
Phase 5

## 階段

### Phase 1：確認 Batch 2 第一刀範圍
- [x] 重讀 Batch 2 提案與邊界文件
- [x] 盤點 `Exporter / Importer / ExportModels` 的依賴關係
- [x] 決定第一刀只切 shared DTO 與 validator，不碰 import rebuild adapter
- **Status:** complete

### Phase 2：抽出 shared DTO 與 validator
- [x] 建立 current projection shared DTO 檔
- [x] 建立 current projection validator
- [x] 將 `Exporter` 改接 validator 與新 DTO namespace
- [x] 將 `Importer` 與測試改接新 DTO namespace
- **Status:** complete

### Phase 3：補測試與驗證
- [x] 補最小 validator 測試
- [x] 跑 export / import / round-trip gate
- [x] 整理結果與暫存輸出
- **Status:** complete

### Phase 4：抽出 current projection service
- [x] 建立 shared `CurrentFlowProjectionService`
- [x] 將 `Exporter` 改接 shared DTO -> Ink service
- [x] 補最小 projection service 測試
- [x] 跑完整 EditMode gate 並清理暫存輸出
- **Status:** complete

### Phase 5：抽出 current projection import plan seam
- [x] 建立 shared `CurrentFlowImportService`
- [x] 建立 import plan models，集中 DTO -> 匯入規劃
- [x] 將 `Importer` 改接 import plan 與 GraphToolkit adapter
- [x] 補最小 import service 測試
- [x] 跑完整 EditMode gate 並清理暫存輸出
- **Status:** complete

## 關鍵問題
1. `ExportGraphDto` 這組模型應先移到哪一層，才不會再被誤認成 GraphToolkit 私有 DTO？
2. 哪些 validation 可以先只吃 current projection DTO，不必一起搬 GraphToolkit 型別？
3. 這一刀怎麼切，才能讓 `Importer` 也開始共用同一份 shared DTO，但又不一次把 rebuild adapter 全搬走？

## 已做決策
| 決策 | 原因 |
|------|------|
| Batch 2 第一刀先切 validator + shared DTO | 這是最能立刻降低 exporter 職責，又不會一次炸開 import 流程的切法 |
| 先不碰 `InkFlowChartGraph.cs` | 先守住匯入匯出核心，不把 editor shell 一起捲進來 |
| 先不做 WebView prototype | 這輪仍是 seam 重構，不做前端換殼 |
| Batch 2 第二刀改抽 DTO -> Ink projection service | 這段邏輯只吃 current projection DTO，最適合繼續往 shared core 移 |
| `CurrentFlowImportService` 不直接碰 GraphToolkit 型別 | 否則 core 會反過來依賴 editor；正確切法是 core 先產出 import plan，再由 editor adapter 套到 GraphToolkit |

## 錯誤紀錄
| Error | Attempt | Resolution |
|-------|---------|------------|
| shared validator 第一輪把線性 Flow 輸出的空 `toPortName` 視為非法，導致既有 v2 import / round-trip fixture 失敗 | 1 | 將線性 Flow 規則改成「空 `toPortName` 視為 `Flow`」，對齊 importer 既有 contract |

## 備註
- 如果 validator 仍直接依賴 `INode / IPort`，就代表這一刀切得不夠乾淨，需要再把「圖走訪」和「projection 合法性」分開。
- 第二輪完整 EditMode gate 已通過：
  - `OpsidanosInk.EditModeTests.dll`：`41/41 passed`
  - `CurrentFlowProjectionValidatorTests`：`4/4 passed`
  - `InkFlowChartImportTests`：`9/9 passed`
  - `InkFlowChartRoundTripTests`：`3/3 passed`
- Phase 4 已確認 shared projection service 不只自己測得過，也守住既有 `Export / Import / RoundTrip` gate：
  - `OpsidanosInk.EditModeTests.dll`：`44/44 passed`
  - `CurrentFlowProjectionServiceTests`：`3/3 passed`
  - `CurrentFlowProjectionValidatorTests`：`4/4 passed`
  - `InkFlowChartExportTests`：`6/6 passed`
  - `InkFlowChartImportTests`：`9/9 passed`
  - `InkFlowChartRoundTripTests`：`3/3 passed`
- Phase 5 已確認 shared import plan service + GraphToolkit import adapter 的切法也守住既有 gate：
  - `OpsidanosInk.EditModeTests.dll`：`47/47 passed`
  - `CurrentFlowImportServiceTests`：`3/3 passed`
  - `CurrentFlowProjectionServiceTests`：`3/3 passed`
  - `CurrentFlowProjectionValidatorTests`：`4/4 passed`
  - `InkFlowChartExportTests`：`6/6 passed`
  - `InkFlowChartImportTests`：`9/9 passed`
  - `InkFlowChartRoundTripTests`：`3/3 passed`
