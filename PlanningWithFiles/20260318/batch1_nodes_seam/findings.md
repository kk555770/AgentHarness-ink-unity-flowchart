# 調查發現

## 需求
- 使用者已同意繼續，開始 Batch 1。
- 本輪目標是切出 `InkFlowChartNodes.cs` 的第一批節點語意 seam。

## 研究發現
- `InkFlowNodeSchema` 目前同時裝了：
  - canonical node kind 命名
  - current projection 的 legacy `dialogue/action` 對照
  - port key 與 parse/build helper
  - option key 與顯示名稱
  - choice/condition fallback label
- `InkFlowChartExporter.cs` 與 `InkFlowChartImporter.cs` 已大量依賴 `InkFlowNodeSchema`，代表 Batch 1 不能只改節點檔，還要一起改這兩個 adapter 的呼叫點。
- `GetCurrentProjectionNodeType(INode node)` 依賴 `GraphToolkit` 的 `INode` 與 Editor 節點型別，這種 helper 不適合直接搬進純 core。
- 原本的 `ToLegacyActionKindToken` / `ParseLegacyActionKindToken` 與 `InkFlowActionKind` 其實只剩少數匯出器邏輯在用，這批直接改成吃 core naming token 後，可以整組收掉。
- Batch 0 已經有：
  - `CanonicalNodeKinds`
  - `CanonicalPortSemantics`
  - `CanonicalGraphInvariant`
  代表 Batch 1 應該優先把 `InkFlowNodeSchema` 內與這三者重疊的語意搬走，而不是再做第二套常數。
- `OpsidanosInk.FlowChartEditor.asmdef` 原本沒有參考 `OpsidanosInk.CanonicalGraph`，這會讓 Editor 腳本根本無法正式改接 core；這批已補上參考。
- 這批實作後，新的切法是：
  - core：`CanonicalNodeDisplayNames`、`CurrentFlowProjectionNaming`
  - Editor adapter：`InkFlowCurrentProjectionAdapter`
  - GraphToolkit shell：`InkFlowNodeSchema` 只留 option key / 欄位顯示設定
- 第一輪 `runSynchronously` gate 雖然 `20/20 passed`，但 `InkFlowChartRoundTripTests` 屬於 `UnityTest`，不適合拿這輪當最終驗證；補跑非同步 gate 後，`ImportTests` 與 `RoundTripTests` 已正式各自有方法結果且全部通過。

## 技術判斷
| Decision | Rationale |
|----------|-----------|
| `Flow` / `ActionData` / `ActionIn*` 直接改接 `CanonicalPortSemantics` | Batch 0 已有最小 port seam，這批應開始實際使用 |
| `start/dialogue/stageAction/comment/choice/condition` 直接改接 `CanonicalNodeKinds` | 避免 node type 常數繼續雙軌 |
| current projection 的 `dialogue/action/custom` token 另立小型 naming helper | 這是 projection 命名，不等於 canonical node kind |
| `INode -> nodeType` 對照先留在 Editor 層 | 這會碰到 GraphToolkit 型別，不適合塞回純 core |
| 顯示名稱 seam 先只抽「節點/埠共用語意」 | option 欄位顯示仍偏 GraphToolkit 表單責任，不急著整批搬走 |
| 驗證拆成同步 gate + 非同步 gate | 這樣能同時守住 core/smoke/export 與 `UnityTest` round-trip |

## 遇到的問題
| Issue | Resolution |
|-------|------------|
| `InkFlowNodeSchema` 同時混了 core semantics 與 Editor adapter | 本輪要拆成「core naming / projection naming / editor display」三層，而不是整塊平移 |
| `runSynchronously` 不能代表 `UnityTest` 類 round-trip 真的有跑到方法結果 | 補跑不帶 `-runSynchronously` 的 async gate，並用 `Batch1AsyncGateResults.xml` 確認 12/12 passed |

## 參考資源
- `Documentation/AuthoringPhase1ImplementationProposal.md`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodes.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`
- `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CanonicalNodeKinds.cs`
- `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CanonicalPortSemantics.cs`
- `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CanonicalNodeDisplayNames.cs`
- `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CurrentFlowProjectionNaming.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowCurrentProjectionAdapter.cs`

## 視覺/瀏覽重點
- Batch 1 第一刀不是「把整個 `InkFlowNodeSchema` 搬走」。
- 正確切法比較像把裡面的東西拆成三個抽屜，再決定哪個抽屜放 core、哪個留 Editor。
- 這一刀做完後，`InkFlowNodeSchema` 比較像表單設定抽屜，不再像同時裝著命名真相與 GraphToolkit shell 的大雜燴。
