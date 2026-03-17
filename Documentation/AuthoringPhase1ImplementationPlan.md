# 作者工具第一階段實作計畫

> 最後更新：2026/03/17  
> 文件角色：**第一階段實作計畫 / Phase 1 Implementation Plan**  
> 目的：把作者工具重製的第一階段，整理成可執行的工程清單，說清楚先改哪些檔、先不動哪些檔、為什麼這樣切，以及要怎麼驗證。

## 0. 先講結論

第一階段真正該做的，不是先做一個很漂亮的 WebView。

第一階段真正該做的是：

1. 先把 GraphToolkit 腳本裡混在一起的語意層抽出來
2. 先把 current projection 的驗證與轉換責任切乾淨
3. 先讓 GraphToolkit 退成 baseline shell
4. 先保住現在已經能跑的 round-trip 與 Runtime 驗證線

如果用很白話的方式講：

- 現在像是把「積木規則」、「白板上的貼紙」、「搬運流程」都寫在同一本小本子裡
- 第一階段不是先換新白板
- 而是先把這三件事拆成不同本小本子

這樣後面不管是做 WebView、Browser，還是 Electron，
才不會一換殼就整套重寫。

## 1. 這份文件在回答什麼

這份文件主要回答：

- 第一階段到底做什麼，不做什麼
- 先改哪些檔案
- 哪些檔案暫時不要動
- 每一步為什麼能解現在的問題
- 做完後應該看到什麼結果
- 要跑哪些測試來確認沒有把現有閉環打壞

這份文件**不**負責：

- 決定前端框架
- 決定 Electron 要不要上
- 重寫 Unity Runtime
- 定義 canonical schema 內容本身

這些請看：

- `Documentation/AuthoringToolStrategy.md`
- `Documentation/AuthoringRefactorBoundaries.md`
- `Documentation/AuthoringPhase1ImplementationProposal.md`
- `Documentation/CanonicalGraphSchema.md`
- `Documentation/CanonicalGraphApiSpec.md`

如果你要看「第一階段應拆成哪幾批做」，
請直接讀：

- `Documentation/AuthoringPhase1ImplementationProposal.md`

## 2. 第一階段的範圍

### 2.1 這一階段要做什麼

第一階段要做的是：

- 抽出 **canonical-adjacent semantics seam**
- 抽出 **validator / projection service seam**
- 保留 current GraphToolkit workflow 當 baseline
- 讓未來 Web-first 作者工具有地方可以接

### 2.2 這一階段不要做什麼

第一階段**不要**做：

- 直接把 GraphToolkit 全部換掉
- 直接上 Electron
- 重寫 `InkStoryEngine` 與目前 Runtime 主線
- 重寫 Save / Load / Rollback
- 直接廢掉 `.flowchart.json`
- 一開始就把所有資料搬去前端

原因很簡單：

- 現在已經有一條可跑的作者工具與播放閉環
- 第一階段的目的是把「真相層與工具殼」切開
- 不是把所有現況成果一起推倒

## 3. 第一階段建議先動哪些檔

下面這份清單是第一階段最值得先動的檔案。

### 3.1 第一刀：先切 `InkFlowChartNodes.cs`

優先修改：

- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodes.cs`

建議新增：

- `Packages/com.opsidanos.ink/Core/OpsidanosInk.CanonicalGraph.asmdef`
- `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CanonicalNodeKinds.cs`
- `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CanonicalPortSemantics.cs`
- `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CanonicalNodeDisplayNames.cs`

現況：

- `InkFlowChartNodes.cs` 同時有 GraphToolkit node 殼、Editor 顯示、port 規則、canonical-adjacent 語意與 legacy mapping helper。

改法：

- 把 node kind、port kind、branch 規則、display name 對照、`dialogue / action` 這類 naming helper 往新的 core 語意層收。
- `InkFlowChartNodes.cs` 自己只保留 GraphToolkit node shell 與 UI 綁定。

為何能解：

- 這樣 exporter、importer、未來 Web authoring bridge，就不用再去偷用 GraphToolkit 內部語言。

預期效果：

- GraphToolkit 仍能開圖、編圖。
- 但節點語意不再只有 GraphToolkit 知道。

### 3.2 第二刀：把 validation 與 projection seam 從 exporter / importer 抽出來

優先修改：

- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartGraph.cs`

建議新增：

- `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CanonicalGraphValidator.cs`
- `Packages/com.opsidanos.ink/Core/Scripts/Projection/CurrentFlow/CurrentFlowProjectionService.cs`
- `Packages/com.opsidanos.ink/Core/Scripts/Projection/CurrentFlow/CurrentFlowImportService.cs`
- `Packages/com.opsidanos.ink/Core/Scripts/Projection/CurrentFlow/CurrentFlowWarnings.cs`

現況：

- exporter / importer 目前同時在做 traversal、validation、projection、Graph rebuild。

改法：

- 把「這張圖語意上可不可以這樣接」交給 validator。
- 把「怎麼投影成 `.flowchart.json` / `.ink`」交給 projection service。
- exporter / importer 自己只保留 GraphToolkit graph 讀取、GraphToolkit graph 回建、檔案 I/O 這些 adapter 工作。

為何能解：

- 這樣之後換前端時，不需要把 validation 和 projection 再重寫一份。

預期效果：

- current GraphToolkit workflow 還是能匯入匯出。
- 但語意裁決中心不再綁在 exporter / importer 裡。

### 3.3 第三刀：讓 GraphToolkit shell 變薄

優先修改：

- `Assets/Editor/FlowChart/OpsidanosInk.FlowChartEditor.asmdef`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartGraph.cs`

暫時保留：

- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartGraphStyleBootstrap.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodeFields.uss`

現況：

- 目前作者工具主線直接掛在 GraphToolkit asmdef 下面。

改法：

- 讓 GraphToolkit editor 組件只依賴 core 與 projection service。
- 讓 `.inkfc` graph asset 類型留在 editor shell，但不要繼續承擔真相層責任。

為何能解：

- 這一步做完，GraphToolkit 就比較像「現在還在用的白板」，不是整個規則系統。

預期效果：

- current editor 還能繼續當 baseline。
- 但之後 Bridge / Web editor 有更清楚的接點。

## 4. 第一階段先不要動哪些檔

下面這些檔案，第一階段應該盡量不動，避免一次把太多閉環打開：

- `Packages/com.opsidanos.ink/Runtime/Scripts/Story/InkStoryEngine.cs`
- `Packages/com.opsidanos.ink/Runtime/Scripts/Story/StoryOutput.cs`
- `Packages/com.opsidanos.ink/Runtime/Scripts/UI/VNPlayerPresenter.cs`
- `Packages/com.opsidanos.ink/Runtime/Scripts/Save/InkSaveSystem.cs`
- `Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/InkTagCharacterStatePlayer.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartGraphStyleBootstrap.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodeFields.uss`

原因：

- 這些檔案現在承擔的是 Runtime 閉環、UI 演出閉環、樣式殼，和第一階段要切的 seam 不是同一件事。

## 5. 第一階段的成功條件

第一階段做完後，至少要滿足這幾件事：

1. GraphToolkit 仍能建立、編輯、匯出、匯入 `.inkfc` graph。
2. `InkFlowChartNodes.cs` 不再是唯一持有節點語意的地方。
3. exporter / importer 不再是主要語意裁決中心。
4. current `.flowchart.json` 與 `.ink` projection 還能正常產出。
5. Unity Runtime 的播放、Save / Load / Rollback 閉環不退步。
6. 未來 Web-first bridge 已有明確可以接的 core seam。

## 6. 第一階段的驗證方式

第一階段不是只看「能不能編譯」。

至少要驗證這幾組：

### 6.1 GraphToolkit 基本煙霧測試

- `Assets/Editor/Tests/InkFlowChartGraphSmokeTests.cs`

確認：

- 建圖、基本 asset 建立、基本操作沒有直接壞掉

### 6.2 匯出測試

- `Assets/Editor/Tests/InkFlowChartExportTests.cs`

確認：

- `.inkfc -> .flowchart.json + .ink` 仍能成立

### 6.3 匯入測試

- `Assets/Editor/Tests/InkFlowChartImportTests.cs`

確認：

- `.flowchart.json + .ink -> .inkfc` 仍能成立

### 6.4 Round-trip 測試

- `Assets/Editor/Tests/InkFlowChartRoundTripTests.cs`

確認：

- 匯出再匯入後，主要節點與 wire 規則仍一致

### 6.5 Runtime / PlayMode 驗證

- `Assets/Tests/PlayMode/OpsidanosInkPlayModeTests.cs`
- `Assets/Tests/PlayMode/OpsidanosInkPlayModeUiClickTests.cs`

確認：

- 匯出的內容接回目前 Unity Runtime 後，不會把玩家模式節奏、選項互動、Save / Load / Rollback 之類的閉環打壞

### 6.6 新增 core 測試

第一階段建議補上新的 EditMode 測試，專門守：

- node kind 對照
- port semantics
- graph invariant
- validator warning / error
- projection service 的 deterministic output

這樣之後就算前端殼換掉，語意層也還有自己的護欄。

## 7. 建議執行順序

建議照這個順序做：

1. 先新增 core 語意層最小骨架
2. 先把 `InkFlowChartNodes.cs` 的語意 helper 搬出去
3. 再把 exporter / importer 的 validation 與 projection seam 抽出去
4. 最後才把 GraphToolkit shell 變薄
5. 等這一切穩了，再開始做最小 Web authoring prototype

這個順序的重點是：

- 先換骨頭
- 再換接頭
- 最後才換殼

## 8. 和其他文件的關係

- `AuthoringToolStrategy.md`
  - 講的是未來方向
- `AuthoringRefactorBoundaries.md`
  - 講的是要先切哪幾刀
- 本文件
  - 講的是第一階段實際要怎麼動手

## 9. 一句總結

第一階段最重要的，不是先做 WebView。

而是：

> **先把 GraphToolkit 裡混在一起的語意層與 adapter 層拆開，保住現有 round-trip 與 Runtime 驗證，讓後面的 Web-first 作者工具可以接在穩定的 core 上。**
