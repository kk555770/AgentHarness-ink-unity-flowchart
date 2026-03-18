// ===== 變更開始 =====
// 2026/02/08 Opsidanos (修改原因：建立 Graph Toolkit 匯出 MVP，先打通 `.inkfc` 到 `.ink + .flowchart.json` 主線)
// 預期結果：選到 `.inkfc` 後可穩定輸出最小可驗收雙檔，並在缺少開始節點時明確失敗
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpsidanosInk.CanonicalGraph;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace OpsidanosInk.Editor
{
    public readonly struct InkFlowChartExportResult
    {
        public bool success { get; }
        public string graphAssetPath { get; }
        public string inkOutputPath { get; }
        public string jsonOutputPath { get; }
        public string errorMessage { get; }

        private InkFlowChartExportResult(bool success, string graphAssetPath, string inkOutputPath, string jsonOutputPath, string errorMessage)
        {
            this.success = success;
            this.graphAssetPath = graphAssetPath ?? string.Empty;
            this.inkOutputPath = inkOutputPath ?? string.Empty;
            this.jsonOutputPath = jsonOutputPath ?? string.Empty;
            this.errorMessage = errorMessage ?? string.Empty;
        }

        public static InkFlowChartExportResult Success(string graphAssetPath, string inkOutputPath, string jsonOutputPath)
        {
            return new InkFlowChartExportResult(true, graphAssetPath, inkOutputPath, jsonOutputPath, string.Empty);
        }

        public static InkFlowChartExportResult Failure(string graphAssetPath, string errorMessage)
        {
            return new InkFlowChartExportResult(false, graphAssetPath, string.Empty, string.Empty, errorMessage);
        }
    }

    public static class InkFlowChartExporter
    {
        public static InkFlowChartExportResult ExportGraphAsset(string graphAssetPath)
        {
            try
            {
                InkFlowChartGraph graph = GraphDatabase.LoadGraphForImporter<InkFlowChartGraph>(graphAssetPath);
                // ===== 變更開始 =====
                // 2026/02/08 Opsidanos (修改原因：部分測試時序下 importer 讀不到剛建立的圖資產，補上記憶體路徑讀取避免誤判不存在)
                // 預期結果：匯出流程優先讀磁碟乾淨版，若暫時不可得時可回退到一般載入路徑，避免「找不到 Graph 資產」誤判
                if (graph == null)
                {
                    graph = GraphDatabase.LoadGraph<InkFlowChartGraph>(graphAssetPath);
                }
                // ===== 變更結束 =====
                if (graph == null)
                {
                    string error = $"找不到 Graph 資產：{graphAssetPath}";
                    return InkFlowChartExportResult.Failure(graphAssetPath, error);
                }

                BuildExportDtoResult buildResult = BuildExportDto(graph);
                ExportGraphDto exportDto = buildResult.exportDto;
                Dictionary<string, INode> nodeById = buildResult.nodeById;
                if (string.IsNullOrEmpty(exportDto.startNodeId))
                {
                    string error = $"匯出失敗：找不到開始節點（{graphAssetPath}）。";
                    return InkFlowChartExportResult.Failure(graphAssetPath, error);
                }
                // ===== 變更開始 =====
                // 2026/03/18 Opsidanos (修改原因：開始落地 Batch 2，先把 current projection 的純 DTO 驗證抽到 shared validator，再把 GraphToolkit 特有的 port 拓樸檢查留在 adapter)
                // 預期結果：匯出器不再自己保管整包 sidecar 合法性；未來 importer 與其他 bridge 也可共用同一套 DTO validator
                if (!CurrentFlowProjectionValidator.TryValidate(exportDto, out string graphError))
                {
                    string error = $"匯出失敗：{graphError}（{graphAssetPath}）。";
                    return InkFlowChartExportResult.Failure(graphAssetPath, error);
                }

                if (!TryValidateGraphPortLayoutForExport(exportDto, nodeById, out graphError))
                {
                    string error = $"匯出失敗：{graphError}（{graphAssetPath}）。";
                    return InkFlowChartExportResult.Failure(graphAssetPath, error);
                }
                // ===== 變更結束 =====

                string directoryPath = Path.GetDirectoryName(graphAssetPath) ?? "Assets";
                string graphName = Path.GetFileNameWithoutExtension(graphAssetPath);
                string jsonOutputPath = $"{directoryPath}/{graphName}.flowchart.json";
                string inkOutputPath = $"{directoryPath}/{graphName}.ink";

                string jsonContent = JsonUtility.ToJson(exportDto, true);
                File.WriteAllText(jsonOutputPath, jsonContent, new UTF8Encoding(false));

                // ===== 變更開始 =====
                // 2026/03/18 Opsidanos (修改原因：開始落地 Batch 2 第二刀，把 current projection DTO -> Ink 的組裝規則抽到 shared projection service)
                // 預期結果：exporter 退回檔案入口與 GraphToolkit adapter；Ink 文字輸出則由 shared service 統一提供
                string inkContent = CurrentFlowProjectionService.BuildInkContent(exportDto);
                // ===== 變更結束 =====
                File.WriteAllText(inkOutputPath, inkContent, new UTF8Encoding(false));

                return InkFlowChartExportResult.Success(graphAssetPath, inkOutputPath, jsonOutputPath);
            }
            catch (Exception exception)
            {
                string error = $"匯出失敗（{graphAssetPath}）：{exception.Message}";
                return InkFlowChartExportResult.Failure(graphAssetPath, error);
            }
        }

        private readonly struct BuildExportDtoResult
        {
            public ExportGraphDto exportDto { get; }
            public Dictionary<string, INode> nodeById { get; }

            public BuildExportDtoResult(ExportGraphDto exportDto, Dictionary<string, INode> nodeById)
            {
                this.exportDto = exportDto;
                this.nodeById = nodeById;
            }
        }

        private static BuildExportDtoResult BuildExportDto(InkFlowChartGraph graph)
        {
            var exportDto = new ExportGraphDto
            {
                graphName = graph.name
            };

            List<INode> orderedNodes = graph
                .GetNodes()
                .Where(IsFlowNode)
                .ToList();

            var nodeIdByNode = new Dictionary<INode, string>();
            var nodeById = new Dictionary<string, INode>(StringComparer.Ordinal);
            for (int nodeIndex = 0; nodeIndex < orderedNodes.Count; nodeIndex++)
            {
                INode node = orderedNodes[nodeIndex];
                string nodeId = $"N{nodeIndex:000}";
                nodeIdByNode[node] = nodeId;
                nodeById[nodeId] = node;

                var exportNode = new ExportNodeDto
                {
                    id = nodeId,
                    type = GetNodeType(node),
                    content = GetNodeContent(node),
                    // ===== 變更開始 =====
                    // 2026/02/22 Opsidanos (修改原因：Action 節點新增內容類型下拉，匯出時需保留到 sidecar)
                    // 預期結果：匯出 `.flowchart.json` 時可帶出 actionKind，供匯入與 round-trip 保持一致
                    actionKind = GetNodeActionKind(node),
                    // ===== 變更結束 =====
                    choiceMode = GetNodeChoiceMode(node)
                };
                exportDto.nodes.Add(exportNode);

                if (string.IsNullOrEmpty(exportDto.startNodeId) && node is InkFlowStartNode)
                {
                    exportDto.startNodeId = nodeId;
                }
            }

            foreach (ExportNodeDto exportNode in exportDto.nodes)
            {
                if (!nodeById.TryGetValue(exportNode.id, out INode node) || node == null)
                {
                    continue;
                }

                exportNode.outputs = BuildNodeOutputs(node, exportNode.type, nodeIdByNode);

                if (exportNode.outputs.Count == 1
                    && IsLinearNodeType(exportNode.type)
                    && string.Equals(exportNode.outputs[0].portName, CanonicalPortSemantics.Flow, StringComparison.Ordinal))
                {
                    ExportNodeOutputDto output = exportNode.outputs[0];
                    if (!string.IsNullOrEmpty(output.toNodeId))
                    {
                        exportNode.nextIds = new List<string> { output.toNodeId };
                    }
                }
            }

            return new BuildExportDtoResult(exportDto, nodeById);
        }

        private static List<ExportNodeOutputDto> BuildNodeOutputs(INode node, string nodeType, Dictionary<INode, string> nodeIdByNode)
        {
            var outputDtos = new List<ExportNodeOutputDto>();
            Node typedNode = node as Node;
            List<string> choiceLabels = null;
            List<string> conditionExpressions = null;

            // ===== 變更開始 =====
            // 2026/02/22 Opsidanos (修改原因：choice/condition 的 option key 與 type 判斷改由 schema 常數管理)
            // 預期結果：BuildNodeOutputs 不再硬編碼字串，避免 key 或 type 拼字漂移
            if (typedNode != null && string.Equals(nodeType, CanonicalNodeKinds.Choice, StringComparison.OrdinalIgnoreCase))
            {
                choiceLabels = SplitAndNormalizeLines(GetNodeOptionValue(typedNode, InkFlowNodeOptionSchema.ChoiceTextsOptionName));
            }

            if (typedNode != null && string.Equals(nodeType, CanonicalNodeKinds.Condition, StringComparison.OrdinalIgnoreCase))
            {
                conditionExpressions = SplitAndNormalizeLines(GetNodeOptionValue(typedNode, InkFlowNodeOptionSchema.ConditionTextsOptionName));
            }

            var connectedPorts = new List<IPort>();
            List<IPort> outputPorts = node.GetOutputPorts().ToList();
            bool includeAllOutputs = string.Equals(nodeType, CanonicalNodeKinds.Choice, StringComparison.OrdinalIgnoreCase)
                || string.Equals(nodeType, CanonicalNodeKinds.Condition, StringComparison.OrdinalIgnoreCase);
            // ===== 變更結束 =====
            for (int outputIndex = 0; outputIndex < outputPorts.Count; outputIndex++)
            {
                IPort outputPort = outputPorts[outputIndex];
                connectedPorts.Clear();
                outputPort.GetConnectedPorts(connectedPorts);

                string toNodeId = string.Empty;
                string toPortName = string.Empty;
                if (connectedPorts.Count > 0)
                {
                    IPort connectedInputPort = connectedPorts[0];
                    if (connectedInputPort != null)
                    {
                        toPortName = connectedInputPort.name ?? string.Empty;
                    }

                    INode nextNode = connectedInputPort != null ? connectedInputPort.GetNode() : null;
                    if (nextNode != null && nodeIdByNode.TryGetValue(nextNode, out string nextId))
                    {
                        toNodeId = nextId;
                    }
                }

                var outputDto = new ExportNodeOutputDto
                {
                    portName = outputPort.name,
                    toNodeId = toNodeId,
                    toPortName = toPortName
                };

                if (choiceLabels != null)
                {
                    outputDto.label = outputIndex >= 0 && outputIndex < choiceLabels.Count ? choiceLabels[outputIndex] : string.Empty;
                }

                if (conditionExpressions != null)
                {
                    bool isElse = outputIndex == outputPorts.Count - 1;
                    outputDto.isElse = isElse;
                    if (!isElse)
                    {
                        outputDto.condition = outputIndex >= 0 && outputIndex < conditionExpressions.Count ? conditionExpressions[outputIndex] : string.Empty;
                    }
                }

                if (includeAllOutputs || !string.IsNullOrEmpty(outputDto.toNodeId))
                {
                    outputDtos.Add(outputDto);
                }
            }

            return outputDtos;
        }

        private static string GetNodeChoiceMode(INode node)
        {
            // ===== 變更開始 =====
            // 2026/02/22 Opsidanos (修改原因：ChoiceMode option key 改由 schema 常數提供，避免硬編碼字串)
            // 預期結果：choiceMode 匯出讀值與節點定義共用同一個 key
            if (node is InkFlowChoiceNode choiceNode)
            {
                INodeOption option = choiceNode.GetNodeOptionByName(InkFlowNodeOptionSchema.ChoiceModeOptionName);
                if (option != null && option.TryGetValue(out InkFlowChoiceMode mode) && mode == InkFlowChoiceMode.Repeatable)
                {
                    return "+";
                }

                return "*";
            }

            // ===== 變更結束 =====
            return string.Empty;
        }

        // ===== 變更開始 =====
        // 2026/02/22 Opsidanos (修改原因：Action 節點新增內容類型下拉，匯出時需要對應 sidecar token)
        // 預期結果：Action 節點可穩定輸出 `dialogue/action/custom`，舊資料缺值時預設為 dialogue
        private static string GetNodeActionKind(INode node)
        {
            // ===== 變更開始 =====
            // 2026/02/25 Opsidanos (修改原因：對話/動作節點已拆分；actionKind 改由節點型別推導以保持舊欄位相容)
            // 預期結果：舊 sidecar 讀者仍可從 actionKind 判斷語意，且不依賴已移除的下拉 option
            // ===== 變更開始 =====
            // 2026/03/11 Opsidanos (修改原因：legacy actionKind token 改由 schema helper 明確提供，避免匯出器自己握有 mapping 細節)
            // 預期結果：匯出器只表達「這是對話或動作」，實際 legacy token 由單一 helper 產生
            if (node is InkFlowStageActionNode)
            {
                return CurrentFlowProjectionNaming.StageActionActionKindToken;
            }

            if (node is InkFlowDialogueNode)
            {
                return CurrentFlowProjectionNaming.DialogueActionKindToken;
            }
            // ===== 變更結束 =====

            return string.Empty;
            // ===== 變更結束 =====
        }
        // ===== 變更結束 =====

        private static List<string> SplitAndNormalizeLines(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new List<string>();
            }

            string[] rawLines = text
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Split('\n');

            var result = new List<string>(rawLines.Length);
            for (int i = 0; i < rawLines.Length; i++)
            {
                result.Add(rawLines[i]?.Trim() ?? string.Empty);
            }

            for (int i = result.Count - 1; i >= 0; i--)
            {
                if (!string.IsNullOrEmpty(result[i]))
                {
                    break;
                }

                result.RemoveAt(i);
            }

            return result;
        }

        private static bool TryValidateGraphPortLayoutForExport(ExportGraphDto exportDto, Dictionary<string, INode> nodeById, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (exportDto == null || exportDto.nodes == null)
            {
                return true;
            }

            if (nodeById == null)
            {
                return true;
            }

            var connectedPorts = new List<IPort>();
            foreach (ExportNodeDto exportNode in exportDto.nodes)
            {
                if (exportNode == null)
                {
                    continue;
                }

                if (!nodeById.TryGetValue(exportNode.id, out INode node) || node == null)
                {
                    continue;
                }

                List<IPort> outputPorts = node.GetOutputPorts().ToList();
                if (string.Equals(exportNode.type, CanonicalNodeKinds.Start, StringComparison.OrdinalIgnoreCase)
                    || CurrentFlowProjectionNaming.IsDialogueNodeType(exportNode.type)
                    || string.Equals(exportNode.type, CanonicalNodeKinds.Comment, StringComparison.OrdinalIgnoreCase))
                {
                    IPort flowOutputPort = node.GetOutputPortByName(CanonicalPortSemantics.Flow);
                    if (flowOutputPort == null)
                    {
                        errorMessage = $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 缺少 Flow 輸出埠。";
                        return false;
                    }

                    connectedPorts.Clear();
                    flowOutputPort.GetConnectedPorts(connectedPorts);

                    if (string.Equals(exportNode.type, CanonicalNodeKinds.Start, StringComparison.OrdinalIgnoreCase))
                    {
                        if (connectedPorts.Count != 1)
                        {
                            errorMessage = $"start 節點 id=\"{exportNode.id}\" 必須且只能接 1 條下一步連線，但目前是 {connectedPorts.Count} 條。";
                            return false;
                        }
                    }
                    else
                    {
                        if (connectedPorts.Count > 1)
                        {
                            errorMessage = $"線性節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 只允許 0 或 1 條輸出線；若要分岔請改用 choice/condition 節點。";
                            return false;
                        }
                    }

                    if (connectedPorts.Count == 1)
                    {
                        ExportNodeOutputDto outputDto = exportNode.outputs != null && exportNode.outputs.Count > 0 ? exportNode.outputs[0] : null;
                        if (outputDto == null
                            || string.IsNullOrEmpty(outputDto.toNodeId)
                            || !string.Equals(outputDto.toPortName, CanonicalPortSemantics.Flow, StringComparison.Ordinal))
                        {
                            errorMessage = $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 的 Flow 連線未指向可匯出的節點（可能連到非 Flow 節點）。";
                            return false;
                        }
                    }

                    continue;
                }

                if (string.Equals(exportNode.type, CanonicalNodeKinds.StageAction, StringComparison.OrdinalIgnoreCase))
                {
                    IPort dataOutputPort = node.GetOutputPortByName(CanonicalPortSemantics.ActionData);
                    if (dataOutputPort == null)
                    {
                        errorMessage = $"動作節點 id=\"{exportNode.id}\" 缺少資料輸出埠 `{CanonicalPortSemantics.ActionData}`。";
                        return false;
                    }

                    connectedPorts.Clear();
                    dataOutputPort.GetConnectedPorts(connectedPorts);
                    if (connectedPorts.Count != 1)
                    {
                        errorMessage = $"動作節點 id=\"{exportNode.id}\" 必須且只能有 1 條資料線連到對話節點，目前是 {connectedPorts.Count} 條。";
                        return false;
                    }

                    continue;
                }

                if (string.Equals(exportNode.type, CanonicalNodeKinds.Choice, StringComparison.OrdinalIgnoreCase))
                {
                    if (outputPorts.Count < 1)
                    {
                        errorMessage = $"choice 節點 id=\"{exportNode.id}\" 至少需要 1 個輸出埠。";
                        return false;
                    }

                    if (exportNode.outputs == null || exportNode.outputs.Count != outputPorts.Count)
                    {
                        errorMessage = $"choice 節點 id=\"{exportNode.id}\" 的 outputs 資料與輸出埠數量不一致。";
                        return false;
                    }

                    for (int i = 0; i < outputPorts.Count; i++)
                    {
                        IPort outputPort = outputPorts[i];
                        connectedPorts.Clear();
                        outputPort.GetConnectedPorts(connectedPorts);
                        if (connectedPorts.Count != 1)
                        {
                            errorMessage = $"choice 節點 id=\"{exportNode.id}\" 的輸出埠 name=\"{outputPort.name}\" display=\"{outputPort.displayName}\" 必須且只能接 1 條線，但目前是 {connectedPorts.Count} 條。";
                            return false;
                        }

                    }

                    continue;
                }

                if (string.Equals(exportNode.type, CanonicalNodeKinds.Condition, StringComparison.OrdinalIgnoreCase))
                {
                    if (outputPorts.Count < 2)
                    {
                        errorMessage = $"condition 節點 id=\"{exportNode.id}\" 至少需要 2 個輸出埠（含否則）。";
                        return false;
                    }

                    if (exportNode.outputs == null || exportNode.outputs.Count != outputPorts.Count)
                    {
                        errorMessage = $"condition 節點 id=\"{exportNode.id}\" 的 outputs 資料與輸出埠數量不一致。";
                        return false;
                    }

                    int elseIndex = outputPorts.Count - 1;
                    for (int i = 0; i < outputPorts.Count; i++)
                    {
                        IPort outputPort = outputPorts[i];
                        connectedPorts.Clear();
                        outputPort.GetConnectedPorts(connectedPorts);
                        if (connectedPorts.Count != 1)
                        {
                            errorMessage = $"condition 節點 id=\"{exportNode.id}\" 的輸出埠 name=\"{outputPort.name}\" display=\"{outputPort.displayName}\" 必須且只能接 1 條線，但目前是 {connectedPorts.Count} 條。";
                            return false;
                        }

                    }

                    continue;
                }

                errorMessage = $"不支援的節點型別：id=\"{exportNode.id}\" type=\"{exportNode.type}\"。";
                return false;
            }

            return true;
        }

        private static bool IsFlowNode(INode node)
        {
            return node is InkFlowStartNode
                || node is InkFlowDialogueNode
                || node is InkFlowStageActionNode
                || node is InkFlowCommentNode
                || node is InkFlowChoiceNode
                || node is InkFlowConditionNode;
        }

        // ===== 變更開始 =====
        // 2026/02/25 Opsidanos (修改原因：nextIds 僅適用線性流程節點，避免把資料線誤當 next)
        // 預期結果：stageAction 的資料輸出不會污染 v1 相容欄位 nextIds
        private static bool IsLinearNodeType(string nodeType)
        {
            return string.Equals(nodeType, CanonicalNodeKinds.Start, StringComparison.OrdinalIgnoreCase)
                || CurrentFlowProjectionNaming.IsDialogueNodeType(nodeType)
                || string.Equals(nodeType, CanonicalNodeKinds.Comment, StringComparison.OrdinalIgnoreCase);
        }
        // ===== 變更結束 =====

        private static string GetNodeType(INode node)
        {
            // ===== 變更開始 =====
            // 2026/02/22 Opsidanos (修改原因：節點 type 字串改由共用 schema 常數輸出，避免匯出/匯入打字不一致)
            // 預期結果：節點 type 來源單一化（start/action/comment/choice/condition）
            // ===== 變更開始 =====
            // 2026/03/11 Opsidanos (修改原因：把 dialogue 的 current projection type 與 legacy token 集中到 schema helper，避免匯出器自行散落對應規則)
            // 預期結果：匯出 sidecar 時，所有節點 type 都由同一個 helper 決定；對話節點固定輸出 current projection token
            return InkFlowCurrentProjectionAdapter.GetCurrentProjectionNodeType(node);
            // ===== 變更結束 =====
            // ===== 變更結束 =====
        }

        private static string GetNodeContent(INode node)
        {
            // ===== 變更開始 =====
            // 2026/02/22 Opsidanos (修改原因：option key 改由共用 schema 常數提供，避免 key 字串散落)
            // 預期結果：Action/Comment 內容欄位匯出時使用同一組 key 定義
            if (node is InkFlowDialogueNode dialogueNode)
            {
                return GetNodeOptionValue(dialogueNode, InkFlowNodeOptionSchema.DialogueContentOptionName);
            }

            if (node is InkFlowStageActionNode stageActionNode)
            {
                return GetNodeOptionValue(stageActionNode, InkFlowNodeOptionSchema.StageActionContentOptionName);
            }

            if (node is InkFlowCommentNode commentNode)
            {
                return GetNodeOptionValue(commentNode, InkFlowNodeOptionSchema.CommentNoteOptionName);
            }
            // ===== 變更結束 =====

            return string.Empty;
        }

        private static string GetNodeOptionValue(Node node, string optionName)
        {
            INodeOption option = node.GetNodeOptionByName(optionName);
            if (option == null)
            {
                return string.Empty;
            }

            if (option.TryGetValue(out string value))
            {
                return value ?? string.Empty;
            }

            return string.Empty;
        }

    }
}
// ===== 變更結束 =====
