# 調查發現

## 需求
- 使用者已同意繼續進行下一輪文件翻修。
- 本輪持續只改文件，不改正式腳本。
- 目標是補上 current GraphToolkit 腳本與 future canonical core / bridge 的責任邊界。

## 研究發現
- 目前文件已經有：
  - 北極星
  - 作者工具策略
  - current authoring workflow
  - current projection contract
- 但還缺一份專門回答「重製時哪塊先抽、哪塊先留」的邊界文件。
- `Assets/Editor/FlowChart/OpsidanosInk.FlowChartEditor.asmdef` 直接依賴 `Unity.GraphToolkit.*`，代表目前作者工具主線仍綁在 GraphToolkit Editor 殼下。
- `InkFlowChartNodes.cs` 是目前最混的檔案，裡面同時有 GraphToolkit node 殼、Editor 顯示命名、port 規則、canonical-adjacent 語意與 legacy mapping helper。
- `InkFlowChartExportModels.cs` 比較乾淨，定位明確屬於 current `.flowchart.json` projection DTO。
- `InkFlowChartExporter.cs` 與 `InkFlowChartImporter.cs` 各自都混了 traversal、validation、projection、GraphToolkit rebuild adapter 等多種責任。
- `InkFlowChartGraphStyleBootstrap.cs` 與 USS 樣式檔屬於 current tooling shell，短期不用先動。

## 技術判斷
| Decision | Rationale |
|----------|-----------|
| 新增重製邊界文件 | 補上從文件到實作之間的中介層，讓後面重構更有抓手 |
| 盤點 asmdef 與 GraphToolkit 腳本 | 讓邊界文件不是空話，而是能對到實際檔案 |
| 第一刀先切 `InkFlowChartNodes.cs` 的語意層 | 這裡最混，若不先抽離，後面 exporter / importer / Web authoring bridge 會一直依賴 GraphToolkit 內部語言 |
| 匯入匯出器先降格成 adapter | 長期應讓 validation 與語意裁決回到 canonical core / projection service，而不是繼續堆在 GraphToolkit exporter/importer 裡 |

## 遇到的問題
| Issue | Resolution |
|-------|------------|
| 文件缺少實際檔案對位的邊界說明 | 新增 `Documentation/AuthoringRefactorBoundaries.md`，用實際檔案逐一對位責任 |

## 參考資源
- `Documentation/AuthoringToolStrategy.md`
- `Documentation/CurrentAuthoringWorkflow.md`
- `Documentation/DocsIndex.md`
- `Assets/Editor/FlowChart/GraphToolkit/`
- `Assets/Editor/FlowChart/OpsidanosInk.FlowChartEditor.asmdef`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartGraph.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodes.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExportModels.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`

## 視覺/瀏覽重點
- 缺的不是更多方向，而是把「重構先切哪一刀」講清楚。
- 這一輪的重點不是設計新模組名稱，而是先把 current shell、projection adapter、canonical-adjacent semantics、future bridge 四層責任切開。
