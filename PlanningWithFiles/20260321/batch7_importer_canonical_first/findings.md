# Findings

- `InkFlowChartImporter.ImportFromFlowchartJson(...)` 目前流程是：
  `validate DTO -> build canonical -> build normalized DTO -> CurrentFlowImportService.BuildPlan(graphDto) -> GraphToolkit rebuild`。
- `CurrentFlowImportService.BuildPlan(ExportGraphDto)` 目前仍是 DTO-first service，負責：
  - dialogue `ActionIn` 最大序號整理
  - choice 文本 / mode 整理
  - condition 文本整理
  - `nextIds` fallback -> flow wire
- `CurrentFlowGraphToolkitImportAdapter.TryPopulateGraph(...)` 目前只處理 GraphToolkit 節點建立、option 寫入與 wire 重建，責任邊界合理，Batch 7 不應讓它變回語意中心。
- `GraphToolkitSpec.md` 明確要求：
  - ports 透過 `OnDefinePorts(...)` 定義
  - port / option 名稱在 node 內必須唯一
  - 可用 `GetInputPortByName(...)` / `GetNodeOptionByName(...)` 查詢
  這代表 import plan 仍應保留「最終 port 名稱」這種 GraphToolkit rebuild 所需資料。
- 高風險回歸點：
  - `startNodeId` 找不到節點
  - `stageAction -> dialogue ActionInN` 序號整理
  - choice / condition 文本與分支順序
  - round-trip 後 `.ink` 可編譯
- `CurrentFlowImportService` 新增 canonical-first 入口後，最小安全作法是：
  - 由 service 內部吸收 `CanonicalGraphDocument -> normalized ExportGraphDto -> CurrentFlowImportPlan`
  - `InkFlowChartImporter` 不再直接顯式呼叫 `TryBuildProjection(...)`
- 新增測試時曾誤把 `[` `]` 當成合法 choice label；實際上 `CurrentFlowProjectionValidator` 明確禁止此字元，因為會破壞匯出的 Ink choice 括號。
