# 調查發現

## 需求
- 使用者已同意 Batch 3 下一刀：拆 `InkFlowChartNodes.cs` 裡的支援型別與 branch node。
- 這刀要維持現有行為，不主動擴張到樣式殼、projection seam、import seam。

## 研究發現
- `InkFlowActionPayload` 目前只在：
  - `InkFlowChartNodes.cs`
  - `InkFlowDialogueNode` 的 typed input port
  - `InkFlowStageActionNode` 的 typed output port
  之間被引用。
- `InkFlowChoiceMode / InkFlowChoiceNode / InkFlowConditionNode` 目前還被下列檔案使用：
  - `InkFlowChartVisibleNodes.cs`
  - `InkFlowChartExporter.cs`
  - `InkFlowCurrentProjectionAdapter.cs`
  - `CurrentFlowGraphToolkitImportAdapter.cs`
  - `InkFlowChartImportTests.cs`
  - `InkFlowChartExportTests.cs`
- 這代表拆檔的風險主要是：
  - 類型名稱維持不變
  - namespace 維持 `OpsidanosInk.Editor`
  - `VisibleNodes / Exporter / Importer / Tests` 不需要跟著改語意，只要能正常重新編譯與連到同名型別即可。
- `GraphToolkitSpec.md` 這輪已重新讀過；這刀只做 node 類型與支援型別拆檔，不改 Graph asset / Port / Option 契約本身。
- 子代理補充確認：
  - `InkFlowChartBranchNodes.cs` 需要保留 `using OpsidanosInk.CanonicalGraph;` 與 `using Unity.GraphToolkit.Editor;`
  - 最值得守的測試是 `InkFlowChartImportTests / InkFlowChartExportTests / InkFlowChartRoundTripTests / InkFlowChartGraphSmokeTests`
  - 不該順手動 `Exporter / Importer / Adapter / StyleBootstrap / asmdef`
- 這輪已實作：
  - 新增 `InkFlowActionPayload.cs`
  - 新增 `InkFlowChartBranchNodes.cs`
  - 將 `InkFlowChartNodes.cs` 收斂回主線節點集合
- 這輪 gate 結果：
  - EditMode：`265 total / 261 passed / 0 failed / 4 skipped`
  - `OpsidanosInk.EditModeTests.dll`：`47/47 passed`
  - `InkFlowChartGraphSmokeTests`：`1/1 passed`
  - `InkFlowChartExportTests`：`6/6 passed`
  - `InkFlowChartImportTests`：`9/9 passed`
  - `InkFlowChartRoundTripTests`：`3/3 passed`
  - PlayMode：`221 total / 130 passed / 0 failed / 91 skipped`
  - `OpsidanosInk.PlayModeTests.dll`：`17/17 passed`
  - `OpsidanosInkPlayModeTests`：`7/7 passed`
  - `OpsidanosInkPlayModeUiClickTests`：`10/10 passed`
- 測試副作用只有 `Assets/OffMeshLinkScene.unity` 的物件 ID 噪音；已用 `git restore -- Assets/OffMeshLinkScene.unity` 還原。
- `Batch3BranchNodeGateResults.xml` 與 `Batch3BranchNodePlayModeResults.xml` 已刪除，不保留測試暫存垃圾。

## 技術判斷
| Decision | Rationale |
|----------|-----------|
| `InkFlowActionPayload` 應獨立成檔 | 這是 typed wire payload，和節點本體放在同一檔的必要性很低 |
| `InkFlowChoiceMode / InkFlowChoiceNode / InkFlowConditionNode` 應集中到 `InkFlowChartBranchNodes.cs` | 它們同屬分支節點，拆出來後 `InkFlowChartNodes.cs` 會更像主線節點集合 |
| 不改 `Exporter / Importer` | 這輪目標是檔案邊界整理，不應順手重構 Batch 2 已收斂好的 seam |
| 不改 `InkFlowChartGraphStyleBootstrap.cs` | 前面 planning 已明確記錄這批先不要動樣式與 UI 視覺殼 |

## 遇到的問題
| Issue | Resolution |
|-------|------------|
| 一開始的大 patch 沒有對準 `InkFlowChartNodes.cs` 的上下文 | 改成先讀行號，再分成「新增新檔」與「瘦身原檔」兩個小 patch |

## 參考資源
- `GraphToolkitSpec.md`
- `PlanningWithFiles/20260318/batch3_graphtoolkit_shell_baseline/`
- `PlanningWithFiles/20260319/batch3_next_seam_scan/`
