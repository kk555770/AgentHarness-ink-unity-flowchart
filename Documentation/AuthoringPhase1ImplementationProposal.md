# 作者工具第一階段逐批次實作提案

> 最後更新：2026/03/17  
> 文件角色：**第一階段逐批次實作提案 / Phase 1 Batch Proposal**  
> 目的：把第一階段實作計畫再往下切成逐批次施工順序，讓後續真正開工時能照批次推進，不會一口氣把整條作者工具線拆爛。

## 0. 先講結論

如果把第一階段想成要搬家，
那上一份文件是在講：

- 要搬哪些家具
- 哪些房間先不要動

這一份文件講的則是：

- 第一天先搬哪一箱
- 第二天再搬哪一箱
- 每搬完一箱要不要回頭確認門還關得上

所以這份文件最重要的價值不是「方向」，
而是「順序」與「停損點」。

## 1. 這份文件在回答什麼

這份文件主要回答：

- 第一階段應拆成哪幾批
- 每一批先改哪些檔
- 每一批不要碰哪些地方
- 每一批做完後應該看到什麼結果
- 每一批要跑哪些測試當 gate
- 什麼情況下可以往下一批走

這份文件**不**負責：

- 決定 canonical schema 本身
- 決定 Web 前端框架
- 決定 Electron 是否要上
- 重寫 Runtime 主線

這些請看：

- `Documentation/AuthoringToolStrategy.md`
- `Documentation/AuthoringRefactorBoundaries.md`
- `Documentation/AuthoringPhase1ImplementationPlan.md`

如果你要看「Batch 0 第一批真正先改哪些檔」，
請直接讀：

- `Documentation/AuthoringPhase1Batch0Kickoff.md`

## 2. 批次設計原則

這份提案用 4 批來切。

原因是：

- 少於 4 批會太大顆，一壞很難知道是哪裡炸了
- 多於 4 批又會太碎，像把一條走廊切成太多小格，做起來會很拖

每一批都遵守同一個原則：

1. 只切一種責任
2. 盡量不跨 Runtime 主線
3. 每批結束一定要有 gate 測試
4. 測試沒過，就不要進下一批

## 3. 批次總覽

```text
Batch 0
  -> 先立 gate 與新 core 骨架，不搬邏輯

Batch 1
  -> 抽出節點語意 seam

Batch 2
  -> 抽出 validator / projection seam

Batch 3
  -> 讓 GraphToolkit shell 變薄，穩定 baseline
```

最重要的觀念是：

- **Batch 0 先做護欄**
- **Batch 1 與 Batch 2 才開始切骨頭**
- **Batch 3 才收殼**

## 4. Batch 0：先立骨架與 gate

### 要改的檔案

- `Packages/com.opsidanos.ink/Core/OpsidanosInk.CanonicalGraph.asmdef`
- `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/`
- `Assets/Editor/Tests/`

### 先不要動的檔案

- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodes.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`
- `Packages/com.opsidanos.ink/Runtime/Scripts/`

### 現況

- 現在還沒有一個真正獨立的 core seam 可以接住後面要抽出去的語意。
- 測試雖然很多，但大多是在守 GraphToolkit 匯入匯出與 Runtime 閉環，還沒有專門守新 core 的最小護欄。

### 改法

- 先新增最小 `CanonicalGraph` 組件骨架，但先不要急著把大段邏輯搬進去。
- 先新增最小 core 測試空間，至少能放：
  - node kind 對照
  - port semantics
  - graph invariant

### 為何能解

- 先把新房間的地板鋪好，後面搬家具才有地方放。

### 預期效果

- repo 裡開始有一個正式的 core seam。
- 後續 Batch 1 抽出去的東西，有地方可以落。

### Gate 測試

- `Assets/Editor/Tests/InkFlowChartGraphSmokeTests.cs`
- 新增最小 core EditMode 測試

### 可以進下一批的條件

- 新 asmdef 正常編譯
- GraphToolkit smoke test 不退步
- 新 core 測試可以穩定跑

如果要看這一批更細的開工清單，請接著讀：

- `Documentation/AuthoringPhase1Batch0Kickoff.md`

## 5. Batch 1：抽出節點語意 seam

### 要改的檔案

- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodes.cs`
- `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CanonicalNodeKinds.cs`
- `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CanonicalPortSemantics.cs`
- `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CanonicalNodeDisplayNames.cs`

### 先不要動的檔案

- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartGraphStyleBootstrap.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodeFields.uss`

### 現況

- `InkFlowChartNodes.cs` 現在像一個塞太滿的抽屜。
- 裡面同時放了：
  - GraphToolkit node 殼
  - 顯示名稱
  - port 規則
  - branch 規則
  - `dialogue / action` legacy 命名 helper

### 改法

- 把節點種類、埠語意、顯示名稱對照、legacy naming helper 先搬進 core seam。
- GraphToolkit 節點類別只保留：
  - node shell
  - option UI
  - GraphToolkit 專屬綁定

### 為何能解

- 後面 exporter / importer / future bridge 就不用再直接咬住 `InkFlowChartNodes.cs` 的內部語言。

### 預期效果

- `InkFlowChartNodes.cs` 變薄
- 節點語意開始變成「GraphToolkit 之外也能看懂」的東西

### Gate 測試

- `Assets/Editor/Tests/InkFlowChartGraphSmokeTests.cs`
- `Assets/Editor/Tests/InkFlowChartExportTests.cs`
- 新 core node / port 對照測試

### 可以進下一批的條件

- GraphToolkit 還能建立與載入 `.inkfc`
- 匯出基本流程不退步
- 新 core 測試已守住 node kind / port semantics

## 6. Batch 2：抽出 validator / projection seam

### 要改的檔案

- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartGraph.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExportModels.cs`
- `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CanonicalGraphValidator.cs`
- `Packages/com.opsidanos.ink/Core/Scripts/Projection/CurrentFlow/CurrentFlowProjectionService.cs`
- `Packages/com.opsidanos.ink/Core/Scripts/Projection/CurrentFlow/CurrentFlowImportService.cs`

### 先不要動的檔案

- `Packages/com.opsidanos.ink/Runtime/Scripts/Story/InkStoryEngine.cs`
- `Packages/com.opsidanos.ink/Runtime/Scripts/UI/VNPlayerPresenter.cs`
- `Packages/com.opsidanos.ink/Runtime/Scripts/Save/InkSaveSystem.cs`

### 現況

- exporter / importer 現在不只是在搬資料。
- 它們還同時在做：
  - traversal
  - validation
  - projection
  - rebuild adapter

### 改法

- 把「圖規則合不合法」集中到 `CanonicalGraphValidator`。
- 把「怎麼投影成 `.flowchart.json` / `.ink`」集中到 `CurrentFlowProjectionService`。
- 把「怎麼從 current projection 回建 GraphToolkit graph」集中到 `CurrentFlowImportService`。
- exporter / importer 剩下 adapter 與檔案入口責任。

### 為何能解

- 這樣之後就算前端從 GraphToolkit 換成 Web authoring，
  也不用把 validation 與 projection 再寫一套。

### 預期效果

- exporter / importer 不再是語意裁決中心
- `.flowchart.json` 仍保留為 current projection format
- `.ink` 匯出與回匯邏輯還能維持

### Gate 測試

- `Assets/Editor/Tests/InkFlowChartExportTests.cs`
- `Assets/Editor/Tests/InkFlowChartImportTests.cs`
- `Assets/Editor/Tests/InkFlowChartRoundTripTests.cs`
- 新 validator / projection EditMode 測試

### 可以進下一批的條件

- 匯出測試仍能守住 `.inkfc -> .ink + .flowchart.json`
- 匯入測試仍能守住 `.flowchart.json + .ink -> .inkfc`
- Round-trip 測試仍能守住 Graph v2 的可逆閉環

## 7. Batch 3：讓 GraphToolkit shell 退成 baseline

### 要改的檔案

- `Assets/Editor/FlowChart/OpsidanosInk.FlowChartEditor.asmdef`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartGraph.cs`

### 先不要動的檔案

- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartGraphStyleBootstrap.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodeFields.uss`
- `Packages/com.opsidanos.ink/Runtime/Scripts/`

### 現況

- GraphToolkit editor 殼目前還像總指揮台。
- 但第一階段結束時，它應該比較像已經變成「現在還在用的白板」。

### 改法

- 調整 asmdef 與 editor 依賴方向，讓 GraphToolkit shell 改成依賴 core / projection service。
- 把 `.inkfc` asset 留在 editor shell，但不再讓它扛真相層。

### 為何能解

- 後續如果要接 `AuthoringBridge`、WebView、Browser，
  就不會還是被 GraphToolkit 主線反向綁住。

### 預期效果

- GraphToolkit 還能用
- 但角色降成 current baseline / migration baseline

### Gate 測試

- `Assets/Editor/Tests/InkFlowChartGraphSmokeTests.cs`
- `Assets/Editor/Tests/InkFlowChartExportTests.cs`
- `Assets/Editor/Tests/InkFlowChartImportTests.cs`
- `Assets/Editor/Tests/InkFlowChartRoundTripTests.cs`
- `Assets/Tests/PlayMode/OpsidanosInkPlayModeTests.cs`
- `Assets/Tests/PlayMode/OpsidanosInkPlayModeUiClickTests.cs`

### 可以宣告第一階段完成的條件

- GraphToolkit baseline 還能工作
- current projection 閉環沒壞
- Runtime 播放、Save / Load / Rollback 沒退步
- core seam 已足夠讓下一階段開始接 Web-first bridge

## 8. 建議不要跨批次做的事

下面這些事情，看起來很順手，但其實很容易讓批次失焦：

- 在 Batch 1 就順手重寫 exporter
- 在 Batch 2 就順手做 WebView prototype
- 在 Batch 3 就順手碰 Runtime tag 播放器
- 一邊抽 core，一邊改 UI 樣式

這些都很像搬家時順手開始裝潢。

結果通常是：

- 不知道是哪一批把東西弄壞
- 測試過不了時，很難回頭找原因

## 9. 建議的停損規則

如果遇到下面情況，建議當批停下，不要硬往下推：

1. `InkFlowChartGraphSmokeTests` 先壞掉
2. 匯出測試開始出現 `.ink` 不可編譯
3. 匯入測試開始回不出 `.inkfc`
4. Round-trip 測試開始讓 Graph v2 資料線或 outputs 變形
5. PlayMode 測試開始讓 Save / Load / Rollback 退步

因為這代表你不是在做「局部切 seam」，
而是已經碰到現有閉環的主血管了。

## 10. 和其他文件的關係

- `AuthoringToolStrategy.md`
  - 講未來方向
- `AuthoringRefactorBoundaries.md`
  - 講先切哪幾刀
- `AuthoringPhase1ImplementationPlan.md`
  - 講第一階段的整體範圍
- 本文件
  - 講第一階段要分成哪幾批施工

## 11. 一句總結

第一階段最健康的做法，不是大爆改。

而是：

> **先用 Batch 0 到 Batch 3，把 core seam、validator / projection seam、GraphToolkit baseline 一批一批切開；每切一批，就用現有測試確認房子還站著。**
