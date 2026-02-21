// ===== 變更開始 =====
// 2026/02/08 Opsidanos (修改原因：建立 Graph Toolkit 匯出 MVP，先打通 `.inkfc` 到 `.ink + .flowchart.json` 主線)
// 預期結果：選到 `.inkfc` 後可穩定輸出最小可驗收雙檔，並在缺少開始節點時明確失敗
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        private const string ActionContentOptionName = "Content";
        private const string CommentNoteOptionName = "Note";

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
                // 2026/02/13 Opsidanos (修改原因：改為支援 Graph v2 分岔節點（choice/condition），但仍禁止在線性節點用多條輸出線冒充分岔)
                // 預期結果：匯出前可驗證圖形是否符合規範：線性節點 0/1、分岔節點每個輸出埠必須完整接線且資料齊全
                if (!TryValidateGraphForExport(exportDto, nodeById, out string graphError))
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

                string inkContent = BuildInkContent(exportDto);
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

                if (exportNode.outputs.Count == 1)
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

            if (typedNode != null && string.Equals(nodeType, "choice", StringComparison.OrdinalIgnoreCase))
            {
                choiceLabels = SplitAndNormalizeLines(GetNodeOptionValue(typedNode, "ChoiceTexts"));
            }

            if (typedNode != null && string.Equals(nodeType, "condition", StringComparison.OrdinalIgnoreCase))
            {
                conditionExpressions = SplitAndNormalizeLines(GetNodeOptionValue(typedNode, "ConditionTexts"));
            }

            var connectedPorts = new List<IPort>();
            List<IPort> outputPorts = node.GetOutputPorts().ToList();
            bool includeAllOutputs = string.Equals(nodeType, "choice", StringComparison.OrdinalIgnoreCase)
                || string.Equals(nodeType, "condition", StringComparison.OrdinalIgnoreCase);
            for (int outputIndex = 0; outputIndex < outputPorts.Count; outputIndex++)
            {
                IPort outputPort = outputPorts[outputIndex];
                connectedPorts.Clear();
                outputPort.GetConnectedPorts(connectedPorts);

                string toNodeId = string.Empty;
                if (connectedPorts.Count > 0)
                {
                    INode nextNode = connectedPorts[0].GetNode();
                    if (nextNode != null && nodeIdByNode.TryGetValue(nextNode, out string nextId))
                    {
                        toNodeId = nextId;
                    }
                }

                var outputDto = new ExportNodeOutputDto
                {
                    portName = outputPort.name,
                    toNodeId = toNodeId
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
            if (node is InkFlowChoiceNode choiceNode)
            {
                INodeOption option = choiceNode.GetNodeOptionByName("ChoiceMode");
                if (option != null && option.TryGetValue(out InkFlowChoiceMode mode) && mode == InkFlowChoiceMode.Repeatable)
                {
                    return "+";
                }

                return "*";
            }

            return string.Empty;
        }

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

        private static bool TryValidateGraphForExport(ExportGraphDto exportDto, Dictionary<string, INode> nodeById, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (exportDto == null || exportDto.nodes == null)
            {
                return true;
            }

            int startNodeCount = exportDto.nodes.Count(node => node != null && string.Equals(node.type, "start", StringComparison.OrdinalIgnoreCase));
            if (startNodeCount != 1)
            {
                errorMessage = $"Graph v2 必須有且只有 1 個 start 節點，但目前有 {startNodeCount} 個。";
                return false;
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

                // ===== 變更開始 =====
                // 2026/02/21 Opsidanos (修改原因：依輸出契約 §6.2.3，action 內容不得藏流程結構，否則「圖不是權威」)
                // 預期結果：action 內容若含 divert/choice/knot/gather 等 Ink 結構語法，匯出必須直接失敗並給明確錯誤訊息
                if (string.Equals(exportNode.type, "action", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryValidateActionContentForExport(exportNode, out errorMessage))
                    {
                        return false;
                    }
                }
                // ===== 變更結束 =====

                List<IPort> outputPorts = node.GetOutputPorts().ToList();
                if (string.Equals(exportNode.type, "start", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(exportNode.type, "action", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(exportNode.type, "comment", StringComparison.OrdinalIgnoreCase))
                {
                    IPort flowOutputPort = node.GetOutputPortByName("Flow");
                    if (flowOutputPort == null)
                    {
                        errorMessage = $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 缺少 Flow 輸出埠。";
                        return false;
                    }

                    connectedPorts.Clear();
                    flowOutputPort.GetConnectedPorts(connectedPorts);

                    if (string.Equals(exportNode.type, "start", StringComparison.OrdinalIgnoreCase))
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
                        if (outputDto == null || string.IsNullOrEmpty(outputDto.toNodeId))
                        {
                            errorMessage = $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 的 Flow 連線未指向可匯出的節點（可能連到非 Flow 節點）。";
                            return false;
                        }
                    }

                    continue;
                }

                if (string.Equals(exportNode.type, "choice", StringComparison.OrdinalIgnoreCase))
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

                    if (exportNode.choiceMode != "*" && exportNode.choiceMode != "+")
                    {
                        errorMessage = $"choice 節點 id=\"{exportNode.id}\" 的 choiceMode 必須是 \"*\" 或 \"+\"。";
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

                        ExportNodeOutputDto outputDto = exportNode.outputs[i];
                        if (string.IsNullOrWhiteSpace(outputDto.label))
                        {
                            errorMessage = $"choice 節點 id=\"{exportNode.id}\" 的輸出埠 name=\"{outputPort.name}\" 缺少選項文字（label）。";
                            return false;
                        }

                        if (outputDto.label.Contains("[") || outputDto.label.Contains("]"))
                        {
                            errorMessage = $"choice 節點 id=\"{exportNode.id}\" 的選項文字（label）不可包含 `[` 或 `]`，否則匯出 Ink 會破壞選項括號。label=\"{outputDto.label}\"";
                            return false;
                        }

                        if (string.IsNullOrEmpty(outputDto.toNodeId))
                        {
                            errorMessage = $"choice 節點 id=\"{exportNode.id}\" 的輸出埠 name=\"{outputPort.name}\" 未連到有效節點。";
                            return false;
                        }
                    }

                    continue;
                }

                if (string.Equals(exportNode.type, "condition", StringComparison.OrdinalIgnoreCase))
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

                        ExportNodeOutputDto outputDto = exportNode.outputs[i];
                        if (string.IsNullOrEmpty(outputDto.toNodeId))
                        {
                            errorMessage = $"condition 節點 id=\"{exportNode.id}\" 的輸出埠 name=\"{outputPort.name}\" 未連到有效節點。";
                            return false;
                        }

                        if (i == elseIndex)
                        {
                            if (!outputDto.isElse)
                            {
                                errorMessage = $"condition 節點 id=\"{exportNode.id}\" 的最後一個輸出埠必須是 else 分支（isElse=true）。";
                                return false;
                            }

                            continue;
                        }

                        if (outputDto.isElse)
                        {
                            errorMessage = $"condition 節點 id=\"{exportNode.id}\" 只有最後一個輸出埠可以是 else 分支。";
                            return false;
                        }

                        if (string.IsNullOrWhiteSpace(outputDto.condition))
                        {
                            errorMessage = $"condition 節點 id=\"{exportNode.id}\" 的輸出埠 name=\"{outputPort.name}\" 缺少條件式（condition）。";
                            return false;
                        }

                        if (outputDto.condition.Contains(":"))
                        {
                            errorMessage = $"condition 節點 id=\"{exportNode.id}\" 的條件式（condition）不需要包含冒號 `:`（匯出時會自動加上）。condition=\"{outputDto.condition}\"";
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

        // ===== 變更開始 =====
        // 2026/02/21 Opsidanos (修改原因：依輸出契約 §6.2.3，禁止在 action 內容偷藏流程結構)
        // 預期結果：作者只能用節點/接線表達流程；文字內容只能描述演出與狀態，不可寫出 `->`、`*`、`==` 等會改變流程的語法
        private static bool TryValidateActionContentForExport(ExportNodeDto exportNode, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (exportNode == null)
            {
                return true;
            }

            string content = exportNode.content ?? string.Empty;
            if (string.IsNullOrEmpty(content))
            {
                return true;
            }

            string[] lines = SplitLinesForInk(content);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i] ?? string.Empty;

                if (ContainsUnescapedSequence(line, "->"))
                {
                    errorMessage =
                        $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 的內容第 {i + 1} 行包含未跳脫的 `->`，Ink 會把它解讀成 divert（跳轉），代表你把流程藏在文字裡。若要分岔/跳轉請用接線或 choice/condition 節點；若只是要顯示符號請寫 `\\\\->`。";
                    return false;
                }

                if (ContainsUnescapedSequence(line, "<-"))
                {
                    errorMessage =
                        $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 的內容第 {i + 1} 行包含未跳脫的 `<-`，Ink 會把它解讀成 thread 語法，代表你把流程藏在文字裡。若只是要顯示符號請寫 `\\\\<-`。";
                    return false;
                }

                string trimmed = line.TrimStart(' ', '\t');
                if (string.IsNullOrEmpty(trimmed))
                {
                    continue;
                }

                if (trimmed == "{")
                {
                    errorMessage =
                        $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 的內容第 {i + 1} 行只有 `{{`，這會開始多行 conditional block（規範禁止在 action 內容建立分岔）。請改用 condition 節點；若只是要顯示符號請寫 `\\\\{{`。";
                    return false;
                }

                if (trimmed == "}")
                {
                    errorMessage =
                        $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 的內容第 {i + 1} 行只有 `}}`，這會結束多行 conditional block（規範禁止在 action 內容建立分岔）。請改用 condition 節點；若只是要顯示符號請寫 `\\\\}}`。";
                    return false;
                }

                if (trimmed.StartsWith("\\", StringComparison.Ordinal))
                {
                    continue;
                }

                if (trimmed.StartsWith("=", StringComparison.Ordinal))
                {
                    errorMessage =
                        $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 的內容第 {i + 1} 行以 `=` 開頭，Ink 會把它當成 knot/stitch 定義，代表你把流程藏在文字裡。請改用節點/接線；若只是要顯示符號請寫 `\\\\=`。";
                    return false;
                }

                if (trimmed.StartsWith("*", StringComparison.Ordinal) || trimmed.StartsWith("+", StringComparison.Ordinal))
                {
                    errorMessage =
                        $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 的內容第 {i + 1} 行以 `*` 或 `+` 開頭，Ink 會把它當成 choice（選項），代表你把流程藏在文字裡。請改用 choice 節點；若只是要顯示符號請寫 `\\\\*` 或 `\\\\+`。";
                    return false;
                }

                if (trimmed.StartsWith("-", StringComparison.Ordinal))
                {
                    errorMessage =
                        $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 的內容第 {i + 1} 行以 `-` 開頭，Ink 會把它當成 weave/gather 或 conditional 分支行，代表你把流程藏在文字裡。請改用接線匯流或 condition 節點；若只是要顯示符號請寫 `\\\\-`。";
                    return false;
                }

                if (trimmed.Equals("INCLUDE", StringComparison.OrdinalIgnoreCase)
                    || trimmed.StartsWith("INCLUDE ", StringComparison.OrdinalIgnoreCase))
                {
                    errorMessage =
                        $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 的內容第 {i + 1} 行以 `INCLUDE` 開頭，但 Graph v2 子集合未定義 INCLUDE 的可逆資料結構。請改用節點/接線與打包流程；若只是要顯示文字請寫 `\\\\INCLUDE`。";
                    return false;
                }
            }

            return true;
        }

        private static bool ContainsUnescapedSequence(string text, string sequence)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(sequence))
            {
                return false;
            }

            for (int i = 0; i <= text.Length - sequence.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < sequence.Length; j++)
                {
                    if (text[i + j] != sequence[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (!match)
                {
                    continue;
                }

                int slashCount = 0;
                int slashIndex = i - 1;
                while (slashIndex >= 0 && text[slashIndex] == '\\')
                {
                    slashCount++;
                    slashIndex--;
                }

                if (slashCount % 2 == 0)
                {
                    return true;
                }

                i += sequence.Length - 1;
            }

            return false;
        }

        private static string[] SplitLinesForInk(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Array.Empty<string>();
            }

            return text
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Split('\n');
        }
        // ===== 變更結束 =====

        private static bool IsFlowNode(INode node)
        {
            return node is InkFlowStartNode
                || node is InkFlowActionNode
                || node is InkFlowCommentNode
                || node is InkFlowChoiceNode
                || node is InkFlowConditionNode;
        }

        private static string GetNodeType(INode node)
        {
            if (node is InkFlowStartNode)
            {
                return "start";
            }

            if (node is InkFlowActionNode)
            {
                return "action";
            }

            if (node is InkFlowCommentNode)
            {
                return "comment";
            }

            if (node is InkFlowChoiceNode)
            {
                return "choice";
            }

            if (node is InkFlowConditionNode)
            {
                return "condition";
            }

            return "unknown";
        }

        private static string GetNodeContent(INode node)
        {
            if (node is InkFlowActionNode actionNode)
            {
                return GetNodeOptionValue(actionNode, ActionContentOptionName);
            }

            if (node is InkFlowCommentNode commentNode)
            {
                return GetNodeOptionValue(commentNode, CommentNoteOptionName);
            }

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

        private static string BuildInkContent(ExportGraphDto exportDto)
        {
            var inkBuilder = new StringBuilder();
            inkBuilder.AppendLine($"-> knot_{exportDto.startNodeId}");
            inkBuilder.AppendLine();

            foreach (ExportNodeDto exportNode in exportDto.nodes)
            {
                inkBuilder.AppendLine($"=== knot_{exportNode.id} ===");

                if (exportNode.type == "start")
                {
                    string nextId = exportNode.outputs != null && exportNode.outputs.Count > 0 ? exportNode.outputs[0].toNodeId : string.Empty;
                    if (!string.IsNullOrEmpty(nextId))
                    {
                        inkBuilder.AppendLine($"-> knot_{nextId}");
                    }
                    else
                    {
                        inkBuilder.AppendLine("-> END");
                    }
                }
                else if (exportNode.type == "action")
                {
                    if (string.IsNullOrEmpty(exportNode.content))
                    {
                        inkBuilder.AppendLine("// 空內容");
                    }
                    else
                    {
                        inkBuilder.AppendLine(exportNode.content);
                    }

                    string nextId = exportNode.outputs != null && exportNode.outputs.Count > 0 ? exportNode.outputs[0].toNodeId : string.Empty;
                    if (!string.IsNullOrEmpty(nextId))
                    {
                        inkBuilder.AppendLine($"-> knot_{nextId}");
                    }
                    else
                    {
                        inkBuilder.AppendLine("-> END");
                    }
                }
                else if (exportNode.type == "comment")
                {
                    // ===== 變更開始 =====
                    // 2026/02/21 Opsidanos (修改原因：comment 內容可能多行；若只在第一行加 //，後續行會變成真正內容而破壞 Ink)
                    // 預期結果：comment 節點的每一行都輸出為 Ink 註解行，確保不影響流程
                    if (string.IsNullOrEmpty(exportNode.content))
                    {
                        inkBuilder.AppendLine("// (空註解)");
                    }
                    else
                    {
                        string[] commentLines = SplitLinesForInk(exportNode.content);
                        for (int i = 0; i < commentLines.Length; i++)
                        {
                            inkBuilder.AppendLine($"// {commentLines[i]}");
                        }
                    }
                    // ===== 變更結束 =====

                    string nextId = exportNode.outputs != null && exportNode.outputs.Count > 0 ? exportNode.outputs[0].toNodeId : string.Empty;
                    if (!string.IsNullOrEmpty(nextId))
                    {
                        inkBuilder.AppendLine($"-> knot_{nextId}");
                    }
                    else
                    {
                        inkBuilder.AppendLine("-> END");
                    }
                }
                else if (exportNode.type == "choice")
                {
                    string bullet = exportNode.choiceMode == "+" ? "+" : "*";
                    foreach (ExportNodeOutputDto output in exportNode.outputs)
                    {
                        inkBuilder.AppendLine($"{bullet} [{output.label}] -> knot_{output.toNodeId}");
                    }
                }
                else if (exportNode.type == "condition")
                {
                    inkBuilder.AppendLine("{");
                    foreach (ExportNodeOutputDto output in exportNode.outputs)
                    {
                        if (output.isElse)
                        {
                            inkBuilder.AppendLine("  - else:");
                        }
                        else
                        {
                            inkBuilder.AppendLine($"  - {output.condition}:");
                        }

                        inkBuilder.AppendLine($"    -> knot_{output.toNodeId}");
                    }
                    inkBuilder.AppendLine("}");
                }
                else
                {
                    inkBuilder.AppendLine("// 不支援的節點型別");
                    inkBuilder.AppendLine("-> END");
                }

                inkBuilder.AppendLine();
            }

            return inkBuilder.ToString();
        }
    }
}
// ===== 變更結束 =====
