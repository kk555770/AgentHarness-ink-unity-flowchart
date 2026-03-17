# 作者工具第一階段 Batch 0 開工清單

> 最後更新：2026/03/17  
> 文件角色：**Batch 0 開工清單 / Batch 0 Kickoff**  
> 目的：把第一階段的第一批工作收成真正可開工的清單，讓後續開始實作時知道先新增哪些檔、先改哪個 asmdef、先補哪幾支測試，並且知道哪些東西現在先不要碰。

## 0. 先講結論

Batch 0 最重要的，不是開始搬邏輯。

Batch 0 最重要的是：

1. 先立一個最小 `CanonicalGraph` 骨架
2. 先把 Editor 測試掛點接到這個骨架
3. 先補最小 core 測試
4. 先確認 GraphToolkit 現有 smoke 線沒有因為新骨架而壞掉

如果用很白話的方式講：

- 這一批像是在工地先畫線、先搭鷹架、先放水平尺
- 不是開始拆牆，也不是開始搬家具

所以這一批做完後，
你不應該看到很多新功能；
你應該看到的是「後面終於有地方可以開始拆」。

## 1. 這份文件在回答什麼

這份文件主要回答：

- Batch 0 具體要改哪些檔
- 哪些檔是新增，哪些檔是修改
- 哪些 placeholder 值得先立
- 哪些東西不該在 Batch 0 先做太多
- 先補哪些測試最合理
- Batch 0 完成後，進入 Batch 1 的門檻是什麼

這份文件**不**負責：

- 開始搬 `InkFlowChartNodes.cs` 裡的語意
- 開始改 exporter / importer
- 開始碰 Runtime 主線
- 開始做 WebView prototype

這些都在後面的批次。

## 2. Batch 0 的工作目標

Batch 0 要達成的目標只有 3 個：

1. repo 裡正式出現 `OpsidanosInk.CanonicalGraph` 組件
2. 現有 Editor 測試可以直接引用這個新組件
3. 新組件至少有最小測試，守住最基本的語意骨架

你可以把它想成：

- 先把新房間的門牌掛好
- 先把電燈接好
- 先確認門打得開

還不用急著把整套家具搬進去。

## 3. Batch 0 要新增哪些檔

### 3.1 必加檔案

- `Packages/com.opsidanos.ink/Core/OpsidanosInk.CanonicalGraph.asmdef`
- `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CanonicalNodeKinds.cs`
- `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CanonicalPortSemantics.cs`
- `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CanonicalGraphInvariant.cs`

### 3.2 建議先補的測試檔

- `Assets/Editor/Tests/CanonicalNodeKindsTests.cs`
- `Assets/Editor/Tests/CanonicalPortSemanticsTests.cs`
- `Assets/Editor/Tests/CanonicalGraphInvariantTests.cs`

## 4. Batch 0 要修改哪些檔

### 4.1 一定要改的檔

- `Assets/Editor/Tests/OpsidanosInk.EditModeTests.asmdef`

現況：

- 這個 asmdef 目前已經能跑 Editor 測試，但還不認得新的 core 組件。

改法：

- 把 `OpsidanosInk.CanonicalGraph` 加進 references。

為何能解：

- 不這樣做，新的 core 測試根本沒有地方可以編進現有 Editor 測試線。

### 4.2 先不要改的檔

- `Assets/Editor/FlowChart/OpsidanosInk.FlowChartEditor.asmdef`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodes.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`
- `Packages/com.opsidanos.ink/Runtime/OpsidanosInk.Runtime.asmdef`
- `Packages/com.opsidanos.ink/Runtime/Scripts/`

原因：

- Batch 0 的任務是「先立骨架與掛點」。
- 不是開始重排 GraphToolkit 依賴，也不是開始搬 Runtime。

## 5. Batch 0 的 placeholder 應該長什麼樣

這一批應該先立「小而真」的型別，
不要一開始就立很多空殼大類別。

### 5.1 建議先立的型別

- `CanonicalNodeKinds`
  - 先定義最小節點種類名稱
- `CanonicalPortSemantics`
  - 先定義最小埠語意名稱
- `CanonicalGraphInvariant`
  - 先定義最小 invariant 名稱或檢查入口

### 5.2 先不要急著立的型別

- `CanonicalGraphValidator`
- `CurrentFlowProjectionService`
- `CurrentFlowImportService`
- `AuthoringBridge`

原因：

- 這些東西屬於後面批次的責任。
- 如果 Batch 0 先把它們立一堆空殼，很容易讓人誤以為邏輯已經開始搬了。

## 6. Batch 0 建議的實作順序

建議照這個順序做：

1. 先建立 `Core/` 目錄與 `OpsidanosInk.CanonicalGraph.asmdef`
2. 先新增最小語意型別
3. 再更新 `OpsidanosInk.EditModeTests.asmdef`
4. 再新增最小 core 測試檔
5. 最後才跑 Batch 0 gate

這個順序的好處是：

- 先讓編譯路徑成形
- 再讓測試接上
- 最後才檢查護欄

## 7. Batch 0 的最小測試內容

### `CanonicalNodeKindsTests.cs`

至少先守：

- 是否包含目前第一階段會用到的最小節點種類
- 名稱是否穩定

### `CanonicalPortSemanticsTests.cs`

至少先守：

- 最小 Flow / ActionData 之類埠語意名稱是否穩定
- 不同 node kind 對應的埠語意表是否有最小一致性

### `CanonicalGraphInvariantTests.cs`

至少先守：

- start 節點的最小存在規則
- 線性流程節點不可多條 next 這類已知 invariant 名稱是否有固定入口

這一批的測試重點不是「功能很多」，
而是「名字、入口、最小規則開始固定」。

## 8. Batch 0 的 gate

Batch 0 建議至少跑這些：

- `Assets/Editor/Tests/InkFlowChartGraphSmokeTests.cs`
- `Assets/Editor/Tests/CanonicalNodeKindsTests.cs`
- `Assets/Editor/Tests/CanonicalPortSemanticsTests.cs`
- `Assets/Editor/Tests/CanonicalGraphInvariantTests.cs`

如果你想再多一層安心，可以補跑：

- `Assets/Editor/Tests/InkFlowChartExportTests.cs`

但這不是 Batch 0 的必跑主 gate，
因為這一批還沒開始搬 exporter 行為。

## 9. Batch 0 完成條件

下面這幾件事都成立，才算 Batch 0 完成：

1. `OpsidanosInk.CanonicalGraph.asmdef` 可正常編譯
2. `OpsidanosInk.EditModeTests.asmdef` 已能引用新 core
3. 新 core 測試可正常編譯並通過
4. `InkFlowChartGraphSmokeTests` 不退步
5. GraphToolkit 現況工作流沒有因為新增骨架而直接被弄壞

## 10. Batch 0 不要做的事

這一批不要順手做下面這些事：

- 順手把 `InkFlowChartNodes.cs` 裡的 helper 搬掉
- 順手把 validator 空類別全部建好
- 順手改 exporter / importer
- 順手改 `OpsidanosInk.FlowChartEditor.asmdef`
- 順手碰 `InkStoryEngine.cs`

因為這些都不是「搭鷹架」，
而是已經開始拆牆了。

## 11. 和其他文件的關係

- `AuthoringPhase1ImplementationPlan.md`
  - 講第一階段整體範圍
- `AuthoringPhase1ImplementationProposal.md`
  - 講第一階段應分幾批
- 本文件
  - 講第一批真正要先怎麼開工

## 12. 一句總結

Batch 0 最重要的，不是改出新功能。

而是：

> **先把 `OpsidanosInk.CanonicalGraph`、Editor 測試掛點、最小 core 測試三件事立起來，確認地板鋪平了，後面才開始進 Batch 1 抽 `InkFlowChartNodes.cs` 的語意 seam。**
