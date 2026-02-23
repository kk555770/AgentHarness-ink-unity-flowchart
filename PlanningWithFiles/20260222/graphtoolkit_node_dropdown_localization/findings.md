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
- `run_tests(mode=EditMode)` 若不加篩選，會連 package 的 Editor 測試一起跑，可能觸發場景建立/切換與大量編譯。
- `Packages/com.unity.ai.navigation/Tests/Editor` 內含會建立/儲存/開啟場景的測試，正是暴走來源之一。
- 目前 `OpsidanosInkTestRunnerMenu.cs` 雖有「只跑我們的」入口，但沒有 GraphToolkit 專用最小範圍入口與硬性 timeout 規則。
- 使用固定篩選（assembly=`OpsidanosInk.EditModeTests` + category=`GraphToolkitFlowSafe` + fixture regex）後，
  可穩定只命中目標 10 個測試，不再觸發 package 場景測試鏈。

## Technical Decisions
| Decision | Rationale |
|----------|-----------|
| 新增 `InkFlowActionKind` enum | 直接提供下拉選單，降低使用者手動輸入負擔 |
| 新增 `ExportNodeDto.actionKind` | 確保匯入再匯出後設定不遺失 |
| 既有 node `type` 不改（仍為 start/action/comment/choice/condition） | 避免破壞既有資料與測試 |
| 節點與欄位顯示名集中在同一個設定類別 | 後續改詞彙時可單點維護 |
| GraphToolkit 驗證改成固定 filter（assembly + category + fixture） | 避免下次誤跑整包測試造成 Unity/MCP 失控 |
| Import/RoundTrip 類別加 `[Timeout(60000)]` | 符合測試 60 秒上限，卡住時能快速失敗 |
| 還原 `Assets/OffMeshLinkScene.unity` | 此場景不屬於本次 GraphToolkit 任務，需回復乾淨狀態 |

## Issues Encountered
| Issue | Resolution |
|-------|------------|
| GraphToolkit 沒有公開「節點顯示名 attribute」給使用者程式集直接套用 | 改為在節點生命週期以集中名稱設定節點標題，並保持 type 不變 |
| MCP 測試介面當前無 Unity session | 改用 Unity CLI 嘗試執行 EditMode 測試 |
| Unity CLI 測試被「專案已被另一 Unity instance 開啟」阻擋 | 停止重試，保留阻擋訊息並回報 |
| MCP `localhost:8080/mcp` 連線失敗（transport send error） | 先完成流程守門改檔，待 Unity/MCP 恢復後再跑驗證 |

## Verification Outcome
- Import fixture：8/8 Passed
- RoundTrip fixture：2/2 Passed
- Unity Console `error`：0 筆

## Resources
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodes.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExportModels.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`
- `Assets/Editor/Tests/InkFlowChartImportTests.cs`
- `Assets/Editor/Tests/InkFlowChartRoundTripTests.cs`
- `Documentation/DeveloperModeOutputContract.md`
- `Assets/Editor/OpsidanosInkTestRunnerMenu.cs`
- `Packages/com.unity.ai.navigation/Tests/Editor/NavMeshModifierVolumeInPrefabTests.cs`
- `Packages/com.unity.ai.navigation/Tests/Editor/Converter/OffMeshLinkConverterTests.cs`
