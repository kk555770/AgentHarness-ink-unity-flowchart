# Findings & Decisions

## Requirements
- 使用者要在 GraphToolkit 節點內用「點擊後下拉」選格式，不靠手動背語法。
- 節點名稱要更白話、繁體中文，且可由程式內集中變數控制。
- 不能破壞既有 Graph v2 規範與閉環（`.inkfc ⇄ .flowchart.json/.ink`）。

## Research Findings
- 目前 `InkFlowActionNode` 只有 `Content` 字串欄位，沒有內容型別下拉。
- 目前 `choice`、`condition` 已有可下拉 enum 欄位（`ChoiceMode`）可作為實作模式。
- 匯出/匯入 DTO 目前沒有 `actionKind`，所以 UI 選擇無法 round-trip 保留。
- 契約文件目前定義節點型別與 sidecar 最小欄位，但未定義 Action 類型下拉。

## Technical Decisions
| Decision | Rationale |
|----------|-----------|
| 新增 `InkFlowActionKind` enum | 直接提供下拉選單，降低使用者手動輸入負擔 |
| 新增 `ExportNodeDto.actionKind` | 確保匯入再匯出後設定不遺失 |
| 既有 node `type` 不改（仍為 start/action/comment/choice/condition） | 避免破壞既有資料與測試 |
| 節點與欄位顯示名集中在同一個設定類別 | 後續改詞彙時可單點維護 |

## Issues Encountered
| Issue | Resolution |
|-------|------------|
| GraphToolkit 沒有公開「節點顯示名 attribute」給使用者程式集直接套用 | 改為在節點生命週期以集中名稱設定節點標題，並保持 type 不變 |
| MCP 測試介面當前無 Unity session | 改用 Unity CLI 嘗試執行 EditMode 測試 |
| Unity CLI 測試被「專案已被另一 Unity instance 開啟」阻擋 | 停止重試，保留阻擋訊息並回報 |

## Resources
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodes.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExportModels.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`
- `Assets/Editor/Tests/InkFlowChartImportTests.cs`
- `Assets/Editor/Tests/InkFlowChartRoundTripTests.cs`
- `Documentation/DeveloperModeOutputContract.md`
