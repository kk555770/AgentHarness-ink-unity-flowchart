# Findings

## 2026/03/21

- 目前工作樹乾淨，分支仍是 `arcumit/CodexInk`。
- `Documentation/` 內與這輪最相關的檔案包含：
  - `DocsIndex.md`
  - `AuthoringToolStrategy.md`
  - `NarrativeGraphArchitecture.md`
  - `CanonicalGraphApiSpec.md`
  - `CanonicalGraphJsonContract.md`
  - `CurrentAuthoringWorkflow.md`
  - `AuthoringPhase1ImplementationPlan.md`
  - `AuthoringPhase1ImplementationProposal.md`
- 目前這輪需要同時回答三件事：
  - 文件怎麼定義北極星
  - 真實腳本現在做到哪裡
  - 下一步最合理做什麼

## 文件北極星

- `Documentation/NarrativeGraphArchitecture.md`、`Documentation/AuthoringToolStrategy.md`、`Documentation/DocsIndex.md` 的方向一致：
  - 真相在 `schema / API / JSON contract`
  - `GraphToolkit` 是 current working baseline，不是長期唯一平台
  - 長期作者工具策略是 `Web-first control surface`
- `Documentation/CurrentAuthoringWorkflow.md` 明確把目前工作流定位成：
  - `.inkfc -> GraphToolkit -> .flowchart.json + .ink -> story.json -> Unity Runtime`
  - 這條線很重要，但只是 current working / migration baseline，不是 canonical truth

## 真實腳本現況

- `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/` 已經有最小 canonical core：
  - `CanonicalGraphDocument`
  - `CanonicalGraphCommandService`
  - `CanonicalNodeKinds`
  - `CanonicalPortSemantics`
  - validation / operation result 型別
- `Packages/com.opsidanos.ink/Core/Scripts/Projection/CurrentFlow/` 已經有 current projection seam：
  - `CurrentFlowProjectionModels`
  - `CurrentFlowProjectionValidator`
  - `CurrentFlowProjectionService`
  - `CurrentFlowImportService`
  - `CurrentFlowCanonicalGraphAdapter`
- `Assets/Editor/FlowChart/GraphToolkit/` 已經被薄化成 baseline shell：
  - `InkFlowChartEditorCommands`
  - `InkFlowChartEditorAssetUtility`
  - `InkFlowChartGraphShellValidator`
  - `InkFlowChartNodeShellUtility`
  - `InkFlowChartVisibleNodes`
  - `InkFlowNodeOptionSchema`
- exporter 現在已經會走：
  - `GraphToolkit graph -> ExportGraphDto -> CanonicalGraphDocument -> CurrentFlowProjectionService`
- importer 目前仍停在 Batch 5 型態：
  - `sidecar DTO -> CanonicalGraphDocument -> normalized DTO -> import plan -> GraphToolkit rebuild`

## 差距判斷

- 文件想要的是：
  - canonical graph 為唯一控制面
  - Web-first authoring frontend
- 真實腳本目前做到的是：
  - canonical core 已立起來
  - projection 主線已經開始 canonical-first
  - GraphToolkit shell 已經變薄
- 真正還沒接上的，是：
  - importer 這半邊還沒有 canonical-first / command-first
  - Web-first authoring frontend 還沒開工
  - Plain JSON control plane 仍主要停在文件層，還沒正式接到前端 transport

## 子代理狀態

- 文件向子代理已成功回傳，內容與本地盤點一致
- 真實腳本向子代理在合理等待時間內沒有回傳可用摘要，已關閉
- 這輪結論主要依賴：
  - 本地讀碼
  - 文件盤點
  - 最近 `PlanningWithFiles` 歷史

## 收斂結論

- 目前 repo 最準確的描述是：
  - 文件北極星已經很清楚
  - canonical core 與 projection seam 已經真的落地
  - GraphToolkit 已被成功降格成 current baseline shell
  - 下一步最值得做的，不是再補文件，也不是立刻衝 WebView 成品，而是把 importer / command surface 再往 canonical-first 推一刀
