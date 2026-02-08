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
                if (graph == null)
                {
                    string error = $"找不到 Graph 資產：{graphAssetPath}";
                    return InkFlowChartExportResult.Failure(graphAssetPath, error);
                }

                ExportGraphDto exportDto = BuildExportDto(graph);
                if (string.IsNullOrEmpty(exportDto.startNodeId))
                {
                    string error = $"匯出失敗：找不到開始節點（{graphAssetPath}）。";
                    return InkFlowChartExportResult.Failure(graphAssetPath, error);
                }

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

        private static ExportGraphDto BuildExportDto(InkFlowChartGraph graph)
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
            for (int nodeIndex = 0; nodeIndex < orderedNodes.Count; nodeIndex++)
            {
                INode node = orderedNodes[nodeIndex];
                string nodeId = $"N{nodeIndex:000}";
                nodeIdByNode[node] = nodeId;

                var exportNode = new ExportNodeDto
                {
                    id = nodeId,
                    type = GetNodeType(node),
                    content = GetNodeContent(node)
                };
                exportDto.nodes.Add(exportNode);

                if (string.IsNullOrEmpty(exportDto.startNodeId) && node is InkFlowStartNode)
                {
                    exportDto.startNodeId = nodeId;
                }
            }

            foreach (ExportNodeDto exportNode in exportDto.nodes)
            {
                INode node = orderedNodes.First(candidate => nodeIdByNode[candidate] == exportNode.id);
                exportNode.nextIds = BuildNextIds(node, nodeIdByNode);
            }

            return exportDto;
        }

        private static List<string> BuildNextIds(INode node, Dictionary<INode, string> nodeIdByNode)
        {
            var nextIds = new List<string>();
            var connectedPorts = new List<IPort>();
            foreach (IPort outputPort in node.GetOutputPorts())
            {
                connectedPorts.Clear();
                outputPort.GetConnectedPorts(connectedPorts);
                foreach (IPort connectedPort in connectedPorts)
                {
                    INode nextNode = connectedPort.GetNode();
                    if (nextNode != null && nodeIdByNode.TryGetValue(nextNode, out string nextId) && !nextIds.Contains(nextId))
                    {
                        nextIds.Add(nextId);
                    }
                }
            }

            return nextIds;
        }

        private static bool IsFlowNode(INode node)
        {
            return node is InkFlowStartNode || node is InkFlowActionNode || node is InkFlowCommentNode;
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

                if (exportNode.type == "action")
                {
                    if (string.IsNullOrEmpty(exportNode.content))
                    {
                        inkBuilder.AppendLine("// 空內容");
                    }
                    else
                    {
                        inkBuilder.AppendLine(exportNode.content);
                    }
                }
                else if (exportNode.type == "comment")
                {
                    inkBuilder.AppendLine($"// {exportNode.content}");
                }

                if (exportNode.nextIds.Count > 0)
                {
                    foreach (string nextId in exportNode.nextIds)
                    {
                        inkBuilder.AppendLine($"-> knot_{nextId}");
                    }
                }
                else
                {
                    inkBuilder.AppendLine("-> END");
                }

                inkBuilder.AppendLine();
            }

            return inkBuilder.ToString();
        }
    }
}
// ===== 變更結束 =====
