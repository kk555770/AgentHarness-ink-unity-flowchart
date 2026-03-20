# 調查發現

## 需求
- 使用者要求「往下一步做」。
- 依 repo `AGENTS.md`，新的程式碼修改前必須先提案。

## 研究發現
- `Documentation/AuthoringPhase1ImplementationPlan.md` 與 `Documentation/AuthoringPhase1ImplementationProposal.md` 都明寫：
  - `Batch 3` 的目的是讓 `GraphToolkit shell` 退成 baseline
  - `InkFlowChartGraphStyleBootstrap.cs` 與 `InkFlowChartNodeFields.uss` 暫時保留
- 目前已完成：
  - `InkFlowChartGraph.cs` 薄化
  - `InkFlowChartEditorCommands.cs` 與 `InkFlowChartEditorAssetUtility.cs`
  - `InkFlowChartVisibleNodes.cs`
  - `InkFlowChartNodeShellUtility.cs`
  - `InkFlowActionPayload.cs`
  - `InkFlowChartBranchNodes.cs`
  - `InkFlowChartNodes.cs` 收斂回主線節點集合
- `OpsidanosInk.FlowChartEditor.asmdef` 現在已只依賴：
  - `OpsidanosInk.CanonicalGraph`
  - GraphToolkit editor assemblies
- `Batch 3` 的成功條件看起來已經大致達成：
  - GraphToolkit baseline 還能工作
  - current projection 閉環沒壞
  - Runtime 播放與 PlayMode 閉環沒退步
  - shell seam 已經夠薄，後續可以開始接 Web-first bridge
- 若硬要再切 `InkFlowChartGraphStyleBootstrap.cs`，雖然風險低，但會和文件裡「樣式殼先不要動」的批次界線打架。
- Core 目前已經有：
  - canonical 命名與 invariant 常數
  - current projection models / validator / project / import service
- Core 目前還沒有：
  - 正式的 canonical graph document model
  - 正式的 graph mutation / validation / projection command service
  - 可直接對應 `CanonicalGraphApiSpec` / `CanonicalGraphJsonContract` 的最小操作骨架

## 技術判斷
| Decision | Rationale |
|----------|-----------|
| 下一步比較像 `Batch 4`，不是再切一刀 `Batch 3` | 文件原本就把 `StyleBootstrap` 定義成先保留；目前更大的缺口已轉到 bridge/control plane |
| 最小 bridge kickoff 應先做 canonical graph document 與 command surface | Web-first / AI 接入的真正接點不是再薄一個 GraphToolkit 檔，而是開始有可呼叫的 graph 操作骨架 |

## 遇到的問題
| Issue | Resolution |
|-------|------------|

## 參考資源
- `Documentation/AuthoringPhase1ImplementationPlan.md`
- `Documentation/AuthoringPhase1ImplementationProposal.md`
- `Documentation/CanonicalGraphApiSpec.md`
- `Documentation/CanonicalGraphJsonContract.md`
