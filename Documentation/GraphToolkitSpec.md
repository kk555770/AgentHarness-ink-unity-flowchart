# Unity Graph Toolkit Spec — com.unity.graphtoolkit 0.4.0-exp.2 (Unity 6000.2+)

> 這份文件是「給 AI 參考用」的規範（spec），用於產生/審查 Unity Graph Toolkit（GTK）相關程式碼與工具設計。
> 內容以 Unity 官方 Graph Toolkit 0.4.x 公開 API 與手冊為準；GTK 為 **experimental** 套件，API/行為可能變動。

---

## 0. 規範用語（Normative Keywords）

為降低歧義，本文件使用下列強制程度：

- **MUST / 必須**：不符合就視為錯誤輸出
- **MUST NOT / 禁止**：禁止產生或依賴
- **SHOULD / 建議**：一般情況應遵守；除非有明確理由
- **MAY / 可選**：視需求使用

---

## 1. 套件定位與範圍

### 1.1 Graph Toolkit 是什麼

- Graph Toolkit 是用來在 **Unity Editor** 內建立「節點式（node-based）」工具的框架：提供節點/連線/畫布等 UI 與標準互動，讓你專注在領域邏輯（domain-specific functionality）。
- **MUST** 將 Graph Toolkit 視為「前端 authoring framework」：
  - **MUST NOT** 假設它提供 runtime 執行後端（execution backend）。
  - **MUST NOT** 假設可以在 build 後的 runtime 顯示/操作 GTK 圖形 UI。

### 1.2 重要限制

- Graph Toolkit 為 **experimental**：功能與文件可能在正式驗證前變動。
- 目前文件重點在「編輯器作者體驗」與「工具框架 API」，不包含你圖形工具的「實際運算/編譯/執行」邏輯。

---

## 2. 安裝與版本需求

### 2.1 安裝方式（UPM）

- Graph Toolkit 是 experimental package，通常需手動以 **Package Manager → Add package by name** 安裝：
  - Package name：`com.unity.graphtoolkit`

### 2.2 Unity 版本

- **MUST** 使用 Unity Editor **6000.2 或更高版本**。

---

## 3. 專案結構與組件（Assemblies）

### 3.1 Editor-only

- Graph Toolkit **僅用於 Editor**。
- **MUST** 將 Graph Toolkit 相關程式碼放在：
  - `Editor/` 目錄下，或
  - `asmdef` 的 `includePlatforms: ["Editor"]`

### 3.2 asmdef 參考（官方範例）

- **SHOULD** 在自己的 asmdef 中顯式引用（references）：
  - `Unity.GraphToolkit.Editor`
  - `Unity.GraphToolkit.Common.Editor`
  - `Unity.GraphToolkit.Utility.Editor`
- 若是 Editor 測試組件（`*.EditModeTests.asmdef`）會直接呼叫 Graph Toolkit API，**SHOULD** 同樣加入上述三個 references，避免命名空間找不到。

範例（節錄）：

```json
{
  "name": "MyCompany.MyGraphTool.Editor",
  "rootNamespace": "MyCompany.MyGraphTool.Editor",
  "references": [
    "Unity.GraphToolkit.Editor",
    "Unity.GraphToolkit.Common.Editor",
    "Unity.GraphToolkit.Utility.Editor"
  ],
  "includePlatforms": ["Editor"]
}
```

---

## 4. Graph（圖形資產）規格

### 4.1 Graph 的角色

- `Graph` 是圖形工具的核心：負責生命周期、變更追蹤（graph change tracking）與存取 nodes/variables 等資料結構。

### 4.2 Graph 類別定義規則

建立自訂 Graph 類別時：

1. **MUST** `class MyGraph : Graph`
2. **MUST** 標註 `[Serializable]`（確保資料可序列化持久化）
3. **MUST** 標註 `[Graph(...)]` 以註冊：
   - 檔案副檔名（extension）
   - 可選的圖形能力/行為（`GraphOptions`）

> 注意：官方文件中 extension 有時包含 `.`（如 `.mygraph`），有時不包含（如 `simpleg`）；**SHOULD** 選擇一種形式並在專案內保持一致。

範例：

```csharp
using System;
using UnityEditor;
using Unity.GraphToolkit.Editor;

[Graph(AssetExtension)]
[Serializable]
public class MySimpleGraph : Graph
{
    public const string AssetExtension = "simpleg";

    [MenuItem("Assets/Create/My Tools/Simple Graph")]
    static void CreateAssetFile()
    {
        GraphDatabase.PromptInProjectBrowserToCreateNewAsset<MySimpleGraph>();
    }
}
```

### 4.3 GraphOptions（圖形能力/行為旗標）

`GraphOptions` 是 `[Flags]` enum（可組合）。

**已知欄位（0.4.0-exp.2）：**

- `None`：不啟用任何 option
- `Default`：預設行為（包含「同組件內 Node 自動加入 item library」）
- `SupportsSubgraphs`：此 Graph 支援 subgraph
- `DisableAutoInclusionOfNodesFromGraphAssembly`：停用「同組件 Node 自動加入 item library」

**規則：**

- 若需要 subgraph 功能，**MUST** 在 Graph 上啟用 `GraphOptions.SupportsSubgraphs`。
- 若要嚴格控管可用 Node 類型（例如跨專案共用 nodes，但要按 graph 分流），**MAY** 使用 `DisableAutoInclusionOfNodesFromGraphAssembly` + `[UseWithGraph]` 進行相容性標註（見 §5.2）。

---

## 5. Node（節點）規格

### 5.1 Node 基本規則

建立自訂 Node 類別時：

1. **MUST** `class MyNode : Node`
2. **SHOULD** 標註 `[Serializable]`
3. **SHOULD** 將 Node 定義在與 Graph 相同 assembly（若使用預設 `GraphOptions.Default`，可自動出現在 graph item library）
4. Node 的 **class name** 通常會成為 UI 顯示的節點標題（title）

### 5.2 Node 與 Graph 相容性（UseWithGraph）

- `[UseWithGraph]` 用來宣告「該 Node 可用於哪些 Graph 類型」。
- **MUST** 把它視為「可用性/相容性標註」，尤其在你停用自動註冊時很重要。

範例：

```csharp
using System;
using Unity.GraphToolkit.Editor;

[UseWithGraph(typeof(MySimpleGraph))]
[Serializable]
public class MyNodeOnlyForMySimpleGraph : Node
{
}
```

### 5.3 Ports（埠）定義規格

#### 5.3.1 OnDefinePorts

- **MUST** 透過覆寫 `OnDefinePorts(IPortDefinitionContext context)` 定義 ports。
- 使用 `context.AddInputPort...` / `context.AddOutputPort...` 建立，再 `.Build()`。

範例：

```csharp
using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class BasicNodeWithPorts : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        // Connection-only port（不傳遞資料，僅建立連線關係）
        context.AddInputPort("Input").Build();
        context.AddOutputPort("Output").Build();

        // Typed port（傳遞資料型別）
        context.AddInputPort<int>("a").Build();
        context.AddOutputPort<int>("result").Build();
    }
}
```

#### 5.3.2 型別規則（Typed vs Connection-only）

- **Typed ports**：使用泛型 `AddInputPort<T>` / `AddOutputPort<T>`。
- **Connection-only ports**：省略泛型型別參數（例如 `AddInputPort("Input")`）。
- Ports 支援任何 Unity 相容型別（含自訂型別）。

#### 5.3.3 Port builder 常用自訂

- **MAY** 使用 builder 的 fluent API 做 UI 設定，例如：
  - `.WithDisplayName("My Int")`
  - `.WithConnectorUI(PortConnectorUI.Arrowhead)`

```csharp
context.AddInputPort<int>("a")
    .WithDisplayName("My Int")
    .WithConnectorUI(PortConnectorUI.Arrowhead)
    .Build();
```

#### 5.3.4 名稱唯一性（Port / Option）

- **MUST** 保證：
  - **Input port name** 在「該 Node 的 input ports 與 node options」範圍內唯一。
  - **Output port name** 在「該 Node 的 output ports」範圍內唯一。

（原因：Node 提供 `GetInputPortByName(...)`、`GetNodeOptionByName(...)` 等查詢 API。）

#### 5.3.5 常用查詢 API（Node）

- **MAY** 使用 Node 的查詢方法（常見）：
  - `GetInputPorts()` / `GetOutputPorts()`
  - `GetInputPort(int index)` / `GetOutputPort(int index)`
  - `GetInputPortByName(string name)` / `GetOutputPortByName(string name)`
  - `GetNodeOption(int index)` / `GetNodeOptionByName(string name)`

---

## 6. Node Options（節點選項）規格

### 6.1 目的

- Node options 用來「同一種 Node 類型」在 UI 上提供可調設定，進而改變：
  - port 數量
  - port 型別
  - 其他節點結構/行為（取決於你的工具設計）

### 6.2 定義方式（OnDefineOptions）

- **MUST** 透過覆寫 `OnDefineOptions(IOptionDefinitionContext context)` 宣告 options。
- **SHOULD** 用 `const string` 定義 option 名稱，避免 magic string。

範例：用 option 控制輸入 port 數量

```csharp
using System;
using UnityEngine;
using Unity.GraphToolkit.Editor;

[Serializable]
public class NodeWithDynamicPorts : Node
{
    const string k_PortCountName = "PortCount";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        context.AddOption<int>(k_PortCountName)
            .WithDisplayName("Port Count")
            .WithDefaultValue(2)
            .Delayed(); // 延遲更新直到使用者完成輸入
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        var option = GetNodeOptionByName(k_PortCountName);
        option.TryGetValue<int>(out var portCount);

        for (var i = 0; i < portCount; i++)
            context.AddInputPort<Vector2>($"{i}").Build();

        context.AddOutputPort<Vector2>("result").Build();
    }
}
```

### 6.3 讀取方式（GetNodeOptionByName + TryGetValue）

- `GetNodeOptionByName(name)` 回傳 `INodeOption`（找不到可能為 `null`）。
- `TryGetValue<T>(out T value)` 取得目前設定值。

### 6.4 `Delayed()` 的使用規則

- 若 option 的更新會導致拓樸變動（例如新增/移除 ports），且你希望在使用者輸入完成後才更新，**SHOULD** 使用 `Delayed()`。
- 若 option 變動需要「即時」反映（例如 enum 切換 port 型別），**MAY** 不使用 `Delayed()`。

---

## 7. ContextNode / BlockNode（情境節點與區塊節點）

### 7.1 概念

- **ContextNode**：容器節點，內含一組可重排的 BlockNodes。
- **BlockNode**：只能存在於 ContextNode 內部的特殊 Node，不能獨立放在 graph canvas。

UI 結構（概念）：

- ContextNode：Header、Options panel（可選）、Ports（可選）、Blocks container（含 Add block）
- BlockNode：Header、Options panel（可選）、Ports（可選）、Etch（視覺化執行序指示）

> Graph Toolkit 不強制特定執行順序，但會以「etch」顯示 block 之間的視覺序列。

### 7.2 Implement a Context Node

```csharp
using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class MyContextNode : ContextNode
{
    // 可照一般 Node 方式覆寫 OnDefinePorts / OnDefineOptions 等
}
```

### 7.3 Implement a Block Node（UseWithContext）

- **MUST** 讓 BlockNode 繼承 `BlockNode`
- **MUST** 用 `[UseWithContext(typeof(...))]` 指定可容納它的 ContextNode 類型

```csharp
using System;
using Unity.GraphToolkit.Editor;

[UseWithContext(typeof(MyContextNode))]
[Serializable]
public class MyBlockNode : BlockNode
{
}
```

- 若可用於多個 contexts，可在 attribute 內列多個型別：

```csharp
[UseWithContext(typeof(MyContextNode), typeof(MyOtherContextNode))]
public class MyBlockNodeWithMultipleContexts : BlockNode {}
```

### 7.4 在 Editor 內的操作規格（Instantiate / Manage）

- 新增 ContextNode：
  - 右鍵 canvas → `Create node...` → `Contexts` 類別 → double-click 加入。
- 新增 BlockNode：
  - 點 ContextNode 內的 `Add a Block` → item library 會只顯示「相容 blocks」→ double-click 加入。
- 重新排序/移動：
  - **MAY** drag block 在同一 context 內調整順序。
  - **MAY** drag block 到另一個相容的 context。
  - 若拖到不相容的 context：
    - context 顯示紅色外框
    - 放開時 block 會回到原位（不可落地）

### 7.5 何時用 Context Nodes（設計指引）

Context nodes 在下列情況特別有價值：

- 多個操作邏輯上屬於同一群組
- 操作需要遵循特定順序（至少在視覺/使用者理解上）
- 多個操作共享相同參數
- 需要限制使用者能插入的操作集合（強制相容性）
- 複雜邏輯需要拆成可管理的片段

---

## 8. Subgraph（子圖）規格

### 8.1 啟用條件

- **MUST** 在 Graph 上啟用 `GraphOptions.SupportsSubgraphs` 才表示支援 subgraph。

### 8.2 SubgraphAttribute

- `[Subgraph]` 用來宣告「subgraph 類型」與「main graph 類型」之間的連結關係。

### 8.3 功能與公開 API 可用性（0.4）

根據官方 feature matrix（摘要）：

- **Creation（從 selection 建 subgraph）**：public API **Complete**
- 下列功能在 editor UI 可用，但對 public API 仍標記為 *Not available*：
  - breadcrumbs 導覽
  - local / asset subgraphs 的建立與轉換
  - item library path 設定
  - extract 成 placemat、unpack 等

**規則：**

- **MUST NOT** 產生依賴「public API 尚未提供」的 subgraph 操作程式碼。
- 需要自動化 subgraph 工作流時，**SHOULD** 限縮在目前 API 標記為 Complete 的範圍內。

---

## 9. GraphDatabase（圖形資產資料庫）規格

### 9.1 定位

- `GraphDatabase` 類似 Unity 的 `AssetDatabase`，但專為 graph assets 設計：用於建立、載入、儲存 graph 與相關資產操作。

### 9.2 常用方法與規則

#### 9.2.1 建立資產（UI 互動）

- `GraphDatabase.PromptInProjectBrowserToCreateNewAsset<T>(string defaultName = "New Graph")`
  - **用途**：在 Project Browser 建立新 graph asset 並讓使用者立刻命名。

#### 9.2.2 建立資產（程式化）

- `GraphDatabase.CreateGraph<T>(string assetPath)`
  - **MUST** 提供專案相對路徑，例如：`"Assets/Graphs/MyGraph.mygraph"`
  - **MUST** 包含 importer 可辨識的副檔名（即你 GraphAttribute 註冊的 extension）

#### 9.2.3 載入

- `GraphDatabase.LoadGraph<T>(string assetPath)`
  - 類似 `AssetDatabase.LoadAssetAtPath`：可能回傳記憶體中的 instance。
- `GraphDatabase.LoadGraphForImporter<T>(string assetPath)`
  - **用途**：import pipeline 需要 deterministic 的「磁碟乾淨版本」。

#### 9.2.4 儲存

- `GraphDatabase.SaveGraphIfDirty(Graph graph)`
  - 類似 `SaveAssetIfDirty`：只有 dirty 才寫入。

#### 9.2.5 取得資產識別

- `GraphDatabase.GetGraphAssetPath(Graph graph)`
  - **MUST** 用它取得路徑；**MUST NOT** 對 graph 物件使用 `AssetDatabase.GetAssetPath`（可能不正確）。
- `GraphDatabase.GetGraphAssetGUID(Graph graph)`

---

## 10. Graph processing / Errors（圖形處理與錯誤顯示）

### 10.1 變更追蹤入口：Graph.OnGraphChanged(GraphLogger)

- **MAY** 覆寫 `Graph.OnGraphChanged(GraphLogger logger)` 作為：
  - graph 結構/資料變動後的分析掛點（validation、compilation、cache rebuild…）
  - 在 graph editor 上顯示錯誤/警告標記（error markers）的入口

> 官方文件將「Graph processing hooks / error markers」標記為 **Partial**（部分 API 可用）。

### 10.2 GraphLogger 規格

- `GraphLogger` 會與 Unity Console 整合：
  - 若提供 `context`（通常是 node），訊息也會在 graph editor 內以 marker 表現（info/error…）。
  - **重要**：只有當該 graph 的 editor 視窗開啟時，Console 才會顯示這些訊息。

已知方法（0.4）：

- `Log(object message, object context = null)`：資訊訊息
- `LogError(object message, object context = null)`：錯誤訊息  
  （警告方法存在與否請以實際 API 文件為準；GraphLogger 類別描述包含 warnings。）

### 10.3 IErrorsAndWarnings

- `IErrorsAndWarnings` 用於承載與 graph 或 graph elements 相關的 errors / warnings / info messages。

---

## 11. Editor 功能矩陣（Public API 可用性摘要）

> 這裡把官方 feature pages 的「API Access」標記整理成規範，用來限制 AI 產出「不能靠 public API 實作」的功能。

| 功能區 | Editor UI 可用 | Public API 可用性（0.4） | 規範建議 |
|---|---:|---|---|
| Nodes（自訂 Node 類別、ports、options） | ✅ | ✅（核心 API） | 可產生程式碼 |
| Context/Block nodes | ✅ | ✅/Partial | 可產生（注意相容性 attribute） |
| Ports 視覺（色彩/圖示、plugged ports 行為） | ✅ | Not available | **禁止** 依賴 API 控制視覺規則 |
| Wires 互動（連線、wire 插入 node、從 wire 建 node） | ✅ | Not available | **禁止** 依賴 API 直接驅動 |
| Blackboard / Variables / Inspector / Minimap / Toolbar / Item Library | ✅ | Not available | **禁止** 期待 public API 直接操作面板 |
| Subgraph（完整工作流） | ✅ | 部分（Creation Complete，其它 Not available） | 僅用已公開的部分 |
| Alignment / Selection / Deletion / Pan&Zoom / Display Optimization / Missing Items | ✅ | Not available | 視為 editor 內建互動；不輸出不存在的 API |
| Error / Graph processing | ✅ | Partial | 僅使用 `OnGraphChanged` + `GraphLogger` 等公開 API |

---

## 12. 互動（右鍵選單 / 快捷鍵）規格（0.4 重要變更）

### 12.1 新增預設快捷鍵（0.4）

- Create Local Subgraph from Selection：
  - Windows：`Ctrl + Shift + L`
  - macOS：`Cmd + Shift + L`
- Pan：
  - `Right-click + drag`（macOS 不適用）

### 12.2 Context menus（右鍵）

0.4 增加多處右鍵選單（黑板、context/block、canvas、nodes、ports、wires、sticky notes…），且改為只顯示「所選項目共通命令」。

### 12.3 修改快捷鍵

- 使用 Unity 的 **Shortcuts** 系統在 Editor 內管理/自訂（Edit > Shortcuts / Unity > Shortcuts）。

---

## 13. 已知公開 API 面（Unity.GraphToolkit.Editor）

> 本節列出 GTK 0.4 `Unity.GraphToolkit.Editor` namespace 中「文件可見」的主要類型，用來約束 AI 不要胡亂引用不存在的型別。

### 13.1 Classes（主要）

- `Graph`, `Node`
- `ContextNode`, `BlockNode`
- `GraphDatabase`, `GraphLogger`
- Attributes：
  - `GraphAttribute`
  - `UseWithGraphAttribute`
  - `UseWithContextAttribute`
  - `SubgraphAttribute`
- `INodeExtensions`（node/port 相關 extension methods）

### 13.2 Interfaces（節錄）

- `INode`, `IPort`, `INodeOption`
- Port builders：
  - `IInputPortBuilder`, `IInputPortBuilder<TData>`
  - `IOutputPortBuilder`, `IOutputPortBuilder<TData>`
  - `IPortBuilder<T>`
  - `ITypedInputPortBuilder`, `ITypedOutputPortBuilder`
- Options：
  - `IOptionBuilder`, `IOptionBuilder<TData>`
  - `Node.IOptionDefinitionContext`
- Subgraph / variables：
  - `ISubgraphNode`
  - `IVariable`, `IVariableNode`
- Diagnostics：
  - `IErrorsAndWarnings`
- 常數節點：
  - `IConstantNode`

> 注意：以上為「文件可見」集合的摘要，實際還可能包含更多型別/成員；以實際 API 文件為準。

---

## 14. AI 產出檢核清單（最常見錯誤）

1. **Editor-only 違反**
   - 把 GTK 程式碼放到 runtime assembly / build 會報錯。
2. **缺少 `[Serializable]`**
   - Graph/Node 不可正確持久化時，圖形存檔後重開可能丟資料。
3. **Graph extension 重複**
   - 多個 Graph 使用同 extension 會讓 importer 選擇不明確。
4. **Port / Option 命名衝突**
   - input ports 與 node options 的 name 衝突會讓 `Get...ByName` 行為不可靠。
5. **依賴 Not available 的功能**
   - 不要產生「用 public API 去控制黑板、minimap、wires互動」之類的程式碼。
