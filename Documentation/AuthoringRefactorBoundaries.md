# 作者工具重製邊界

> 文件負責人：authoring
> 最後更新：2026/03/17  
> 文件角色：**重製邊界層 / Refactor Boundary**  
> 目的：把目前 GraphToolkit 腳本中混在一起的責任切開，整理出哪些應留在 current tooling、哪些應抽成 canonical core、哪些應落到未來 authoring bridge。

## 0. 先講結論

目前 `Assets/Editor/FlowChart/GraphToolkit/` 這組腳本，不是在做同一種事。

它們其實混了 4 層責任：

1. **current GraphToolkit 外殼**
2. **current projection adapter**
3. **canonical-adjacent 規則與命名**
4. **Editor / future authoring bridge 才該負責的資產與互動橋接**

如果後面要重製，最重要的不是先換 UI，
而是先把這 4 層拆開。

## 1. 這份文件在回答什麼

這份文件主要回答：

- 目前 GraphToolkit 腳本各自在扮演什麼角色
- 哪些責任應該留在 current tooling baseline
- 哪些責任應該抽成 canonical core
- 哪些責任應該變成 future authoring bridge
- 第一刀最值得先切哪裡

這份文件**不**負責：

- 定義 canonical schema 內容本身
- 定義 JSON contract
- 定義 current projection 的輸出合法性
- 定義前端框架

這些請看：

- `Documentation/CanonicalGraphSchema.md`
- `Documentation/CanonicalGraphApiSpec.md`
- `Documentation/CanonicalGraphJsonContract.md`
- `Documentation/DeveloperModeOutputContract.md`
- `Documentation/AuthoringToolStrategy.md`

## 2. 目前實際檔案與責任

### 2.1 `Assets/Editor/FlowChart/OpsidanosInk.FlowChartEditor.asmdef`

目前角色：

- current GraphToolkit Editor 組件入口
- 直接依賴 `Unity.GraphToolkit.*`

目前問題：

- 這代表目前整包作者工具邏輯都被綁在 GraphToolkit asmdef 之下
- 若後面要抽 core，這會是第一個需要變薄的邊界

### 2.2 `InkFlowChartGraph.cs`

目前角色：

- `.inkfc` graph asset 類型
- Unity 選單入口
- 建立圖、匯入、匯出
- 基本 graph change validation hook

判讀：

- 它主要屬於 **current GraphToolkit 外殼**
- 但裡面的圖規則驗證，不該永遠留在 GraphToolkit 專屬殼裡

### 2.3 `InkFlowChartNodes.cs`

目前角色：

- GraphToolkit node 類別
- node option / port 定義
- Editor 顯示名稱
- current sidecar token 與 canonical `dialogue` mapping helper
- 部分 port / naming / branch 規則

判讀：

這個檔是目前最混的地方，因為它同時裝了：

- **GraphToolkit node view / node model 殼**
- **current tooling display 細節**
- **canonical-adjacent 命名與語意**
- **legacy sidecar mapping helper**

這表示它不應該原封不動留著當未來中心。

### 2.4 `InkFlowChartExportModels.cs`

目前角色：

- current `.flowchart.json` sidecar DTO

判讀：

- 這明顯屬於 **current projection adapter**
- 它很重要
- 但不應直接升格成 canonical graph document

### 2.5 `InkFlowChartExporter.cs`

目前角色：

- 從 `.inkfc` / GraphToolkit graph 走訪節點
- 建 current sidecar DTO
- 做一部分 graph export validation
- 輸出 `.flowchart.json`
- 輸出 `.ink`

判讀：

這個檔同時混了：

- current projection writer
- 匯出前驗證
- graph traversal 規則
- sidecar mapping
- `.ink` projection

其中有些責任應抽成 canonical core 或 projection service，
而不是長期留在 GraphToolkit exporter 裡。

### 2.6 `InkFlowChartImporter.cs`

目前角色：

- 讀 `.flowchart.json`
- 驗證 sidecar 基本結構
- 建立 GraphToolkit graph
- 重接 ports / wires
- 套回 node options

判讀：

這個檔主要屬於：

- **current projection reader**
- **current GraphToolkit rebuild adapter**

但它裡面的結構合法性判定，
長期也不應全留在 GraphToolkit importer 裡。

### 2.7 `InkFlowChartGraphStyleBootstrap.cs` / `InkFlowChartNodeFields.uss`

目前角色：

- current GraphToolkit UI / 樣式殼

判讀：

- 這塊很乾淨，就是 **current tooling shell**
- 之後不用急著動

## 3. 建議的未來切法

### 3.1 Canonical Core 應負責什麼

這一層應該負責：

- canonical graph document / node / edge / port semantics
- canonical node type 與 graph invariant
- command / validation / projection-neutral operations
- 不依賴 GraphToolkit 的語意層錯誤與 warning

換句話說，這些目前散在：

- `InkFlowNodeSchema` 裡的部分語意命名
- exporter / importer 裡的部分圖規則
- graph change validation 的語意判定

長期都應往這一層收。

### 3.2 Current Projection Adapter 應負責什麼

這一層應該負責：

- `.flowchart.json` sidecar DTO
- current `dialogue <-> action` legacy mapping
- `.ink` projection
- current projection import / export
- projection-specific warning 與 contract 對接

這一層不該負責：

- canonical truth 的本體
- GraphToolkit 視窗互動

### 3.3 Current GraphToolkit Shell 應負責什麼

這一層應該負責：

- `.inkfc` graph asset 殼
- current node 視覺殼 / option UI / style
- Unity Editor 選單入口
- 現況操作體驗

它不該再負責：

- canonical graph 本體
- projection 主要語意裁決
- 長期唯一作者平台假設

### 3.4 Future Authoring Bridge 應負責什麼

這一層是之後 Web-first 作者工具和本地系統之間的橋。

它應該負責：

- 載入 / 儲存 graph 文件
- 呼叫 canonical command / validation API
- 呼叫 projection service
- 管理 Editor 宿主、WebView、Browser、檔案與 AssetDatabase 的橋接
- 把前端操作轉成語意 command

它不該負責：

- 自己定義 node / edge semantics
- 自己定義 canonical validation 規則
- 自己成為新的真相來源

## 4. 第一刀最值得先切哪裡

如果只選一刀，我建議先切：

### **把 `InkFlowChartNodes.cs` 裡的語意層，從 GraphToolkit node 殼裡抽出來**

原因：

- 這個檔目前最混
- 它同時卡住命名、port、mapping、display、legacy 相容
- 不先切它，後面 exporter / importer / Web authoring bridge 都會繼續依賴 GraphToolkit 殼的內部語言

更白話地說：

- 現在像是把「積木規則」和「白板上的貼紙名稱」寫在同一本筆記裡
- 後面要換白板，會很痛

## 5. 第二刀與第三刀

### 第二刀：把 exporter / importer 的語意驗證抽成 projection service / validator

目標：

- exporter / importer 只剩轉換工作
- 語意合法性改由 core / validator 決定

### 第三刀：把 GraphToolkit 殼薄化，讓它退成 baseline shell

目標：

- GraphToolkit 不再承擔真相層責任
- 它保留成 current working baseline / migration baseline

## 6. 建議的未來模組分層

可以先用概念上這樣切：

```text
OpsidanosInk.CanonicalGraph
  - graph document
  - node / edge semantics
  - validator
  - command service

OpsidanosInk.Projection.CurrentFlow
  - .flowchart.json DTO
  - legacy action/dialogue mapping
  - .ink projection
  - import / export adapter

OpsidanosInk.FlowChartEditor
  - GraphToolkit shell
  - .inkfc asset shell
  - node UI / style / menu entry

OpsidanosInk.AuthoringBridge
  - future WebView / Browser / host bridge
  - asset load / save / command dispatch
```

這裡最重要的不是組件名一定長這樣，
而是責任切分要長這樣。

## 7. 這份文件和其他文件的關係

- `AuthoringToolStrategy.md`
  - 負責回答未來方向與平台策略
- `CurrentAuthoringWorkflow.md`
  - 負責回答現在怎麼工作
- `AuthoringPhase1ImplementationPlan.md`
  - 負責回答第一階段實際要先改哪些檔、怎麼驗證
- 本文件
  - 負責回答從「現在怎麼工作」走到「未來方向」時，重製先切哪一刀

## 8. 一句總結

後面真的要重製時，最重要的不是先把 GraphToolkit 換成 WebView。

而是：

> **先把 GraphToolkit 腳本裡混在一起的「語意層、projection 層、tooling shell、bridge 層」拆開，這樣前端換殼才不會變成整套重寫。**
