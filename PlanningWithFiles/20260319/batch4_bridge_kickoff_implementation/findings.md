# 調查發現

## 需求
- 使用者已同意 `Batch 4 bridge kickoff`。
- 這一批只做最小 control plane 骨架，不做 WebView、不改 GraphToolkit adapter。

## 研究發現
- 目前 core 風格分兩類：
  - `Canonical*`：真相層常數 / 規則 / naming
  - `CurrentFlow*`：current projection DTO / validator / service
- 現有資料型別風格：
  - DTO：`[Serializable] sealed class` + 公開欄位或自動屬性 + 預設初始化集合
  - Result：`readonly struct` + 私有建構子 + `Success / Failure`
  - Service：`public static class`
- 文件與 sub agent 對最小操作集合收斂一致：
  - `CreateGraph`
  - `CreateNode`
  - `ConnectPorts`
  - `ValidateGraph`
- 這批最重要的是：
  - `success` 和 `applied` 要分開
  - `ValidateGraph` 的 `success=true` 只代表驗證流程正常跑完，不代表圖一定合法
  - duplicate edge 應回 `success=true, applied=false`
- 這輪已實作：
  - `CanonicalGraphDocument / NodeRecord / EdgeRecord`
  - `CanonicalGraphValidationIssue / OperationResult / ValidationResult`
  - `CanonicalGraphCommandService`
  - `CanonicalGraphDocumentTests / CanonicalGraphCommandServiceTests`
- 這輪 gate 結果：
  - EditMode：`276 total / 272 passed / 0 failed / 4 skipped`
  - `OpsidanosInk.EditModeTests.dll`：`58/58 passed`
  - `CanonicalGraphDocumentTests`：`3/3 passed`
  - `CanonicalGraphCommandServiceTests`：`8/8 passed`
  - `InkFlowChartGraphSmokeTests`：`1/1 passed`
  - `InkFlowChartExportTests`：`6/6 passed`
  - `InkFlowChartImportTests`：`9/9 passed`
  - `InkFlowChartRoundTripTests`：`3/3 passed`
  - PlayMode：`221 total / 130 passed / 0 failed / 91 skipped`
  - `OpsidanosInk.PlayModeTests.dll`：`17/17 passed`
  - `OpsidanosInkPlayModeTests`：`7/7 passed`
  - `OpsidanosInkPlayModeUiClickTests`：`10/10 passed`
- 測試副作用只有 `Assets/OffMeshLinkScene.unity` 的物件 ID 噪音；已用 `git restore -- Assets/OffMeshLinkScene.unity` 還原。
- `Batch4BridgeKickoffEditModeResults.xml` 與 `Batch4BridgeKickoffPlayModeResults.xml` 已刪除，不保留測試暫存垃圾。

## 技術判斷
| Decision | Rationale |
|----------|-----------|
| `CanonicalGraphDocument`、`NodeRecord`、`EdgeRecord` 用薄 DTO | 先給 Web-first bridge 與未來 JSON contract 一個可穩定對接的 graph 形狀 |
| `CanonicalGraphCommandService` 先只做最小四個操作 | 先立控制面骨架，不把 projection / transport 一次混進來 |
| payload 第一版先走 `payloadJson` + 少量必要輔助欄位 | 避免一開始就把所有 payload 型別打滿，讓這批能低風險落地 |

## 遇到的問題
| Issue | Resolution |
|-------|------------|
| `Condition 缺 else` 的 ValidateGraph 測試一開始誤用了 `CreateNode` | 改成兩層各守一件事：`CreateNode` 直接擋壞 payload，`ValidateGraph` 則驗手動塞入的壞 graph |

## 參考資源
- `Documentation/CanonicalGraphApiSpec.md`
- `Documentation/CanonicalGraphJsonContract.md`
- `Documentation/CanonicalGraphSchemaSpec.md`
- `Packages/com.opsidanos.ink/Core/Scripts/Projection/CurrentFlow/CurrentFlowProjectionModels.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`
