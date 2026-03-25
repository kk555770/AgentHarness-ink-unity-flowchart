# 目前作者工具工作流

> 文件負責人：authoring
> 最後更新：2026/03/17  
> 文件角色：**現況作者工具工作流 / Current Authoring Workflow**  
> 目的：把目前 repo 裡實際可用的 GraphToolkit 作者工具工作流講清楚，避免讀者把它誤認成 canonical truth 或未來唯一長期平台。

## 0. 先講結論

目前 repo 裡真正在運作的作者工具工作流，大致是這樣：

```text
.inkfc
  -> current GraphToolkit 編輯
  -> 匯出 .flowchart.json + .ink
  -> Ink 編譯成 story.json
  -> Unity Runtime 播放 / Save / Load / Rollback 驗證
```

這條線很重要，因為它是：

- 現在最可工作的作者工作流
- current projection 的回歸基準
- 匯入 / 匯出 / round-trip 測試的主要落點

但它不是：

- canonical truth 本體
- AI 最終該直接操作的控制面
- 未來唯一長期作者平台

## 1. 這份文件在回答什麼

這份文件主要回答：

- 目前 GraphToolkit 作者工具實際怎麼運作
- `.inkfc`、`.flowchart.json`、`.ink`、`story.json` 各自扮演什麼角色
- current GraphToolkit workflow 與 current projection contract 的關係
- 為什麼這條線值得保留，但不該升格成長期真相來源

這份文件**不**負責：

- 定義 canonical schema
- 定義 canonical API / JSON contract
- 定義未來 Web-first 策略本身

這些請看：

- `Documentation/CanonicalGraphSchema.md`
- `Documentation/CanonicalGraphApiSpec.md`
- `Documentation/CanonicalGraphJsonContract.md`
- `Documentation/AuthoringToolStrategy.md`

如果你接著想看「這條 current workflow 之後應怎麼拆分責任」，請讀：

- `Documentation/AuthoringRefactorBoundaries.md`

## 2. 目前工作流的 4 個主要載體

### 2.1 `.inkfc`

這是目前 GraphToolkit 使用的圖資產。

它比較像：

- Editor 內可打開、可拉線、可編輯的工作檔

你可以把它想成：

- 目前借來畫圖的白板檔案

### 2.2 `.flowchart.json`

這是目前最重要的 sidecar / interchange / round-trip 載體。

它比較像：

- current Graph v2 的投影格式
- 匯入器與匯出器之間的中繼資料載體

重要提醒：

- 它很重要
- 但它是 current projection format
- 不應被誤讀成 canonical truth 本體

### 2.3 `.ink`

這是目前作者工具投影出的文字稿。

它比較像：

- 可編譯成 `story.json` 的文字投影
- current authoring workflow 與 Runtime 播放之間的重要橋樑

### 2.4 `story.json`

這是目前 Unity Runtime 真正讀進去播放的產物。

它比較像：

- 播放器吃的編譯結果

它不是作者工具本體，也不是 canonical graph。

## 3. 目前 GraphToolkit 真正在做的事

目前 GraphToolkit 這條線，主要負責：

- 建立與編輯流程節點
- 管理目前的節點 / port / wire 互動
- 輸出 current sidecar
- 輸出 `.ink`
- 支援 `.flowchart.json + .ink -> .inkfc` 的回匯
- 作為 current authoring workflow 的回歸基準

這代表它的價值很真實：

- 它不是空殼
- 它不是只有 demo
- 它已經承擔 current working authoring shell 的角色

但這也代表：

- 它的責任不應再膨脹成 canonical truth 本體

## 4. 目前工作流和 current projection contract 的關係

你可以把兩者想成：

- **CurrentAuthoringWorkflow**：在講「現在實際怎麼操作、怎麼流動」
- **DeveloperModeOutputContract**：在講「這條線輸出到 Runtime 時，哪些結果才算合法」

也就是說：

- 本文件偏 **工作流描述**
- `DeveloperModeOutputContract.md` 偏 **輸出合法性契約**

如果用小朋友也懂的比喻：

- 本文件像在講「現在大家怎麼把積木搬到舞台邊」
- 輸出契約像在講「搬到舞台上的積木擺法要符合哪些規矩」

## 5. 目前這條線為什麼值得保留

即使長期方向是 Web-first，這條 current GraphToolkit workflow 仍值得保留，因為它現在有：

- round-trip 經驗
- sidecar 投影經驗
- 匯入 / 匯出測試護欄
- 和 Unity Runtime 真實閉環接上的驗證價值

所以比較健康的理解不是：

- 「直接把它整條丟掉」

而是：

- 「把它保留成 current working baseline / migration baseline」

## 6. 目前這條線不能被誤讀成什麼

### 6.1 不能被誤讀成 canonical truth

因為 canonical truth 應該獨立於 GraphToolkit、`.flowchart.json`、`.ink` 與 `story.json`。

### 6.2 不能被誤讀成未來唯一作者平台

因為作者工具長期策略已傾向 Web-first control surface。

### 6.3 不能被誤讀成 AI 最終控制面

AI 長期應操作的是：

- canonical graph
- command / validation / projection API

而不是直接依賴 GraphToolkit 視窗手勢。

## 7. 和未來 Web-first 策略怎麼接

比較健康的接法應該是：

```text
Canonical Graph Core
  -> Command / Validation / Projection API
  -> current GraphToolkit workflow（現況基準）
  -> future Web authoring frontend（長期方向）
  -> Unity Runtime
```

這代表：

- current GraphToolkit workflow 可以繼續當回歸基準
- future Web authoring frontend 可以逐步接手人類互動層
- 真相與控制面不需要因為前端換殼而一起重寫

## 8. 建議閱讀順序

如果你要理解作者工具這一塊，建議這樣讀：

1. `Documentation/NarrativeGraphArchitecture.md`
2. `Documentation/AuthoringToolStrategy.md`
3. `Documentation/CurrentAuthoringWorkflow.md`
4. `Documentation/DeveloperModeOutputContract.md`

## 9. 一句總結

目前 GraphToolkit 工作流的正確定位不是「最終真相」。

而是：

> **目前仍可工作的作者工具外殼、current projection 的工作流載體，以及未來 Web-first 作者工具重製時的重要對照基準。**
