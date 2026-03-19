# 調查發現

## 需求
- 使用者要求繼續往下找 Batch 3 的下一個最小切點。
- 若切點夠清楚且風險低，直接實作，不停在分析。

## 研究發現
- `GraphToolkit` 目錄目前主要剩下：
  - `InkFlowChartEditorCommands.cs`
  - `InkFlowChartGraph.cs`
  - `InkFlowChartGraphShellValidator.cs`
  - `InkFlowChartVisibleNodes.cs`
  - `InkFlowChartNodeShellUtility.cs`
  - `InkFlowChartNodes.cs`
  - `InkFlowNodeOptionSchema.cs`
  - `InkFlowCurrentProjectionAdapter.cs`
  - `CurrentFlowGraphToolkitImportAdapter.cs`
  - `InkFlowChartExporter.cs`
  - `InkFlowChartImporter.cs`
- 其中 `Exporter / Importer / CurrentFlowGraphToolkitImportAdapter / InkFlowCurrentProjectionAdapter` 已經很明確是 projection 或 import adapter 角色，不像 Batch 3 再優先下刀的地方。
- `InkFlowChartGraph.cs`、`InkFlowChartGraphShellValidator.cs`、`InkFlowChartVisibleNodes.cs` 也都已經很薄，像是單一職責殼。
- `InkFlowNodeOptionSchema.cs` 目前是純 Editor-only schema 常數表。
- 本地盤點下來，真正還比較混的地方比較像：
  - `InkFlowChartEditorCommands.cs` 同時握有 `MenuItem`、資產建立、選取資產判斷、資料夾路徑決策
  - `InkFlowChartNodes.cs` 仍同時放多個 node type、支援型別 `InkFlowActionPayload`、以及 `InkFlowChoiceMode`
- 這代表 Batch 3 下一刀若還要再切，最可能的方向是：
  - 繼續薄化 `InkFlowChartEditorCommands.cs` 的資產路徑/建立 helper
  - 或把 `InkFlowChartNodes.cs` 剩下的支援型別與多 node 類別再拆成更細檔案
- `GraphToolkitSpec.md` 這輪已重新閱讀；目前這刀只動到 Editor-only command shell 與資產建立路徑 helper，沒有改 Graph asset、Node、Port、Option 的 GTK 契約。
- 子代理分析補充了一個更低風險但更小收益的備選切點：`InkFlowChartGraphStyleBootstrap.cs`。這輪先不走那刀，因為 `InkFlowChartEditorCommands.cs` 仍明顯混有選單入口與資產路徑策略，切完的結構收益更直接。
- 這輪已實作：
  - 新增 `InkFlowChartEditorAssetUtility.cs`
  - 將 `InkFlowChartEditorCommands.cs` 的建立新圖路徑、選取 `.inkfc` / `.flowchart.json`、資料夾解析 helper 全部轉接到新 utility
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
- 兩個 XML 暫存 `Batch3EditorCommandGateResults.xml`、`Batch3EditorCommandPlayModeResults.xml` 已刪除，工作樹不保留測試垃圾。

## 技術判斷
| Decision | Rationale |
|----------|-----------|
| `InkFlowChartEditorAssetUtility.cs` 值得獨立成檔 | 這批 helper 都是 Editor shell 的資產路徑決策，和 MenuItem 行為入口不是同一種責任 |
| 先不碰 `Exporter / Importer / Adapter` | 這些在 Batch 2 已是明確 adapter seam，現在再動風險高於收益 |

## 遇到的問題
| Issue | Resolution |
|-------|------------|
| Unity 測試會把 `Assets/OffMeshLinkScene.unity` 弄髒 | 確認只是物件 ID 重排後，於驗證完成後直接 restore |

## 參考資源
- `Documentation/AuthoringPhase1ImplementationProposal.md`
- `Documentation/AuthoringPhase1ImplementationPlan.md`
- `Documentation/AuthoringRefactorBoundaries.md`
- `PlanningWithFiles/20260318/batch3_graphtoolkit_shell_baseline/task_plan.md`
- `PlanningWithFiles/20260318/batch3_graphtoolkit_shell_baseline/findings.md`
- `PlanningWithFiles/20260318/batch3_graphtoolkit_shell_baseline/progress.md`
