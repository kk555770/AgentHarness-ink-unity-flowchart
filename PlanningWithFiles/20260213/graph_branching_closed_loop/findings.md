# Findings：Flow Chart 分岔節點規範與匯出/匯入閉環

## F-001 目前 GraphToolkit 節點只有線性 Flow 埠
- 範圍：`Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodes.cs`
- 現況：只有 `InkFlowStartNode / InkFlowActionNode / InkFlowCommentNode`，每個節點都是單一輸入埠 `Flow`，單一輸出埠 `Flow`。
- 影響：圖上雖可用同一個輸出埠接多條線，但「每條線的語意/資料」無法被區分與保存（例如選項文字、條件式）。

## F-002 匯出目前用「多行 `->`」表示多條 next，Ink 實際只會走第一條
- 範圍：`Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`
- 現況：`BuildInkContent` 會對 `nextIds` 逐一輸出 `-> knot_xxx`。
- 影響：當 `nextIds.Count > 1` 時，後續 divert 會變成死碼，導致「圖上看似分岔，輸出實際不分岔」，閉環破裂。

## F-003 目前匯出/匯入用「線性流程」硬性拒絕多 next
- 範圍：
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`
- 現況：有 `TryValidateLinearNextIds` 與匯入時 `nextIds.Count > 1` 直接失敗。
- 影響：一旦要支援「選項 / 條件 / 結果」節點，多輸出線會被整體封死，與預期不一致。

## F-004 Ink 的「選項」是 `*`（一次性）與 `+`（可重複），不是多行 `->`
- 範圍：`Packages/Ink/InkLibs/InkCompiler/InkParser/InkParser_Choices.cs`
- 現況：Ink 解析 choice 時只接受 `*` 或 `+` 作為 bullet；`*` 代表 once-only，`+` 代表可重複。
- 推論：流程圖若要表達「玩家選擇」，匯出必須輸出 choice 行（`*`/`+`），不能用多行 `->` 假裝分岔。

## F-005 Ink 的「條件分岔」可用 `{ ... }` 多行 conditional，分支必須用 `-` 開頭，且 `else` 只能放最後
- 範圍：`Packages/Ink/InkLibs/InkCompiler/InkParser/InkParser_Conditional.cs`
- 現況：
  - 多行 conditional 的每個分支需以 `-` 開頭。
  - 條件式後面必須有 `:`（例如 `- favor > 7:`）。
  - `- else:` 只能出現一次，且必須是最後一個分支。
- 推論：流程圖若要表達「系統判斷」分岔，匯出應以 conditional block 產生，不應用多行 `->`。
