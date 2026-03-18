# 調查發現

## 需求
- 使用者已同意繼續，開始 Batch 1 下一刀。
- 本輪目標是把 `InkFlowChartNodes.cs` 剩下的 option schema 責任再切薄一層。

## 研究發現
- `InkFlowNodeSchema` 在 Batch 1 第一刀後，已不再承擔 canonical node kind、current projection token 與 port semantics。
- 目前殘留在 `InkFlowNodeSchema` 的內容只剩：
  - option key
  - Inspector 顯示名稱
  - 預設值
- 這些內容的使用點集中在三個檔案：
  - `InkFlowChartNodes.cs`
  - `InkFlowChartExporter.cs`
  - `InkFlowChartImporter.cs`
- 代表這一刀適合切成單獨的 Editor-only schema 檔，不需要再碰 core asmdef 或新增 canonical 型別。
- 實際落地後，新的切法是：
  - `InkFlowNodeOptionSchema.cs`：集中存放 GraphToolkit 表單 schema
  - `InkFlowChartNodes.cs`：只保留節點本體與局部讀值 helper
  - `InkFlowChartExporter.cs` / `InkFlowChartImporter.cs`：共用 `InkFlowNodeOptionSchema`
- `rg` 檢查結果顯示，程式碼內已沒有 `InkFlowNodeSchema` 殘留；剩下的名稱只存在文件與歷史 planning 記錄。
- 完整 EditMode gate 的 XML 結果顯示：
  - `OpsidanosInk.EditModeTests.dll`：`37/37 passed`
  - `InkFlowChartImportTests`：`9/9 passed`
  - `InkFlowChartRoundTripTests`：`3/3 passed`
- 這次不需要再分同步/非同步兩輪，因為完整 EditMode run 已直接產出 `RoundTrip` 的實際 test-case 結果。
- 但完整 EditMode 也會順手跑到外部套件測試，並改動 `Assets/OffMeshLinkScene.unity` 的 scene fileID；這是測試副作用，不是本輪功能變更。

## 技術判斷
| Decision | Rationale |
|----------|-----------|
| 新 schema 檔應留在 `Assets/Editor/FlowChart/GraphToolkit/` | 它仍然是 GraphToolkit 作者表單責任，不屬於 core |
| `InkFlowChartNodes.cs` 應移除 `InkFlowNodeSchema` 類別本體 | 讓檔案更接近純節點定義 |
| 新名稱定為 `InkFlowNodeOptionSchema` | 名字直接說明它是 option/form schema，不再讓人誤會它保管 node semantics |
| 先不新增額外測試檔 | 這一刀主要是搬移 Editor schema 常數，先靠既有 smoke/export/import/round-trip 守回歸 |

## 遇到的問題
| Issue | Resolution |
|-------|------------|
| 完整 EditMode gate 會留下 `Assets/OffMeshLinkScene.unity` 噪音變更 | 驗證 diff 只有 scene fileID 重排後，restore 該檔即可 |

## 參考資源
- `PlanningWithFiles/20260318/batch1_nodes_seam/*`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodes.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`
