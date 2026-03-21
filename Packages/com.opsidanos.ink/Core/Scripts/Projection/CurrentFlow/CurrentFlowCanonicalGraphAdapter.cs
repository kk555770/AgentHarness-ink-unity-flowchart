// ===== 變更開始 =====
// 2026/03/21 Opsidanos (修改原因：開始落地 Batch 5，補上 canonical document 與 current projection DTO 之間的 shared bridge)
// 預期結果：current projection 可先經過 canonical graph 再回到 DTO，後續 importer/exporter/Web-first bridge 都能共用同一條資料橋
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpsidanosInk.CanonicalGraph
{
    public static class CurrentFlowCanonicalGraphAdapter
    {
        private const string CurrentProjectionVersion = "2.0";
        private const string CanonicalGraphVersion = "canonical-1";

        [Serializable]
        private sealed class CurrentFlowGraphMetadata
        {
            public string graphName = string.Empty;
            public string projectionVersion = CurrentProjectionVersion;
        }

        [Serializable]
        private sealed class TextPayload
        {
            public string content = string.Empty;
        }

        [Serializable]
        private sealed class ChoicePayload
        {
            public List<string> labels = new List<string>();
        }

        [Serializable]
        private sealed class ConditionPayload
        {
            public List<string> conditions = new List<string>();
        }

        public static bool TryBuildCanonicalGraph(ExportGraphDto exportDto, out CanonicalGraphDocument graph, out string errorMessage)
        {
            graph = null;
            errorMessage = string.Empty;

            // ===== 變更開始 =====
            // 2026/03/21 Opsidanos (修改原因：shared bridge 入口先統一守 current projection 契約，再轉成 canonical document)
            // 預期結果：不合法的 `.flowchart.json` 不會被悄悄轉進 canonical graph，而是直接用明確錯誤停下來
            if (!CurrentFlowProjectionValidator.TryValidate(exportDto, out errorMessage))
            {
                return false;
            }
            // ===== 變更結束 =====

            CurrentFlowGraphMetadata metadata = new CurrentFlowGraphMetadata
            {
                graphName = exportDto?.graphName ?? string.Empty,
                projectionVersion = string.IsNullOrEmpty(exportDto?.version) ? CurrentProjectionVersion : exportDto.version
            };

            CanonicalGraphOperationResult createGraphResult = CanonicalGraphCommandService.CreateGraph(
                BuildGraphId(exportDto),
                CanonicalGraphVersion,
                JsonUtility.ToJson(metadata));

            if (!createGraphResult.success || createGraphResult.graph == null)
            {
                errorMessage = BuildOperationErrorMessage("建立 canonical graph 失敗", createGraphResult.errors);
                return false;
            }

            graph = createGraphResult.graph;

            List<ExportNodeDto> exportNodes = exportDto?.nodes ?? new List<ExportNodeDto>();
            for (int i = 0; i < exportNodes.Count; i++)
            {
                ExportNodeDto exportNode = exportNodes[i];
                if (exportNode == null)
                {
                    continue;
                }

                CanonicalGraphNodeRecord nodeRecord = BuildCanonicalNodeRecord(exportDto, exportNode);
                CanonicalGraphOperationResult createNodeResult = CanonicalGraphCommandService.CreateNode(graph, nodeRecord);
                if (!createNodeResult.success)
                {
                    errorMessage = BuildOperationErrorMessage($"建立節點失敗（{exportNode.id}）", createNodeResult.errors);
                    return false;
                }
            }

            for (int i = 0; i < exportNodes.Count; i++)
            {
                ExportNodeDto exportNode = exportNodes[i];
                if (exportNode == null)
                {
                    continue;
                }

                List<ExportNodeOutputDto> outputs = GetEffectiveOutputs(exportNode);
                for (int outputIndex = 0; outputIndex < outputs.Count; outputIndex++)
                {
                    ExportNodeOutputDto output = outputs[outputIndex];
                    if (output == null || string.IsNullOrEmpty(output.toNodeId))
                    {
                        continue;
                    }

                    CanonicalGraphOperationResult connectResult = CanonicalGraphCommandService.ConnectPorts(
                        graph,
                        exportNode.id ?? string.Empty,
                        string.IsNullOrEmpty(output.portName) ? CanonicalPortSemantics.Flow : output.portName,
                        output.toNodeId ?? string.Empty,
                        string.IsNullOrEmpty(output.toPortName) ? CanonicalPortSemantics.Flow : output.toPortName);

                    if (!connectResult.success)
                    {
                        errorMessage = BuildOperationErrorMessage(
                            $"建立連線失敗（{exportNode.id}:{output.portName} -> {output.toNodeId}:{output.toPortName}）",
                            connectResult.errors);
                        return false;
                    }
                }
            }

            CanonicalGraphValidationResult validationResult = CanonicalGraphCommandService.ValidateGraph(graph);
            if (!validationResult.success || !validationResult.isValid)
            {
                errorMessage = BuildValidationErrorMessage("canonical graph 驗證失敗", validationResult.errors);
                return false;
            }

            return true;
        }

        public static bool TryBuildProjection(CanonicalGraphDocument graph, out ExportGraphDto exportDto, out string errorMessage)
        {
            exportDto = null;
            errorMessage = string.Empty;

            CanonicalGraphValidationResult validationResult = CanonicalGraphCommandService.ValidateGraph(graph);
            if (!validationResult.success || !validationResult.isValid)
            {
                errorMessage = BuildValidationErrorMessage("canonical graph 驗證失敗", validationResult.errors);
                return false;
            }

            bool hasMetadata = TryReadMetadata(graph?.metadataJson, out CurrentFlowGraphMetadata metadata);
            exportDto = new ExportGraphDto
            {
                version = hasMetadata && !string.IsNullOrEmpty(metadata.projectionVersion)
                    ? metadata.projectionVersion
                    : CurrentProjectionVersion,
                graphName = hasMetadata ? metadata.graphName ?? string.Empty : (graph?.graphId ?? string.Empty),
                startNodeId = string.Empty
            };

            List<CanonicalGraphNodeRecord> nodes = graph?.nodes ?? new List<CanonicalGraphNodeRecord>();
            for (int i = 0; i < nodes.Count; i++)
            {
                CanonicalGraphNodeRecord node = nodes[i];
                if (node == null)
                {
                    continue;
                }

                if (!TryBuildExportNode(graph, node, out ExportNodeDto exportNode, out errorMessage))
                {
                    exportDto = null;
                    return false;
                }

                exportDto.nodes.Add(exportNode);
                if (node.nodeType == CanonicalNodeKinds.Start)
                {
                    exportDto.startNodeId = node.nodeId ?? string.Empty;
                }
            }

            if (!CurrentFlowProjectionValidator.TryValidate(exportDto, out errorMessage))
            {
                exportDto = null;
                return false;
            }

            return true;
        }

        private static CanonicalGraphNodeRecord BuildCanonicalNodeRecord(ExportGraphDto graphDto, ExportNodeDto exportNode)
        {
            string canonicalNodeType = ToCanonicalNodeType(exportNode.type);
            var nodeRecord = new CanonicalGraphNodeRecord
            {
                nodeId = exportNode.id ?? string.Empty,
                nodeType = canonicalNodeType,
                payloadJson = BuildPayloadJson(canonicalNodeType, exportNode),
                branchModeToken = string.Empty
            };

            if (canonicalNodeType == CanonicalNodeKinds.Dialogue)
            {
                int maxActionInputOrder = GetMaxDialogueActionInputOrder(graphDto, exportNode.id);
                nodeRecord.dialogueActionInputCount = maxActionInputOrder >= 0 ? maxActionInputOrder + 1 : 0;
                return nodeRecord;
            }

            if (canonicalNodeType == CanonicalNodeKinds.Choice)
            {
                nodeRecord.branchCount = exportNode.outputs?.Count ?? 0;
                nodeRecord.branchModeToken = exportNode.choiceMode == "+" ? "+" : "*";
                return nodeRecord;
            }

            if (canonicalNodeType == CanonicalNodeKinds.Condition)
            {
                nodeRecord.branchCount = exportNode.outputs?.Count ?? 0;
                return nodeRecord;
            }

            return nodeRecord;
        }

        private static bool TryBuildExportNode(
            CanonicalGraphDocument graph,
            CanonicalGraphNodeRecord node,
            out ExportNodeDto exportNode,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            exportNode = new ExportNodeDto
            {
                id = node.nodeId ?? string.Empty,
                type = CurrentFlowProjectionNaming.ToNodeType(node.nodeType),
                outputs = new List<ExportNodeOutputDto>(),
                nextIds = new List<string>()
            };

            if (exportNode.type == CurrentFlowProjectionNaming.UnknownNodeType)
            {
                errorMessage = $"不支援的 canonical nodeType：{node.nodeType}";
                return false;
            }

            if (node.nodeType == CanonicalNodeKinds.Dialogue)
            {
                exportNode.content = ReadTextPayload(node.payloadJson);
                exportNode.actionKind = CurrentFlowProjectionNaming.DialogueActionKindToken;
                if (!TryBuildLinearOutputs(graph, node.nodeId, CanonicalPortSemantics.Flow, exportNode.outputs, out errorMessage))
                {
                    return false;
                }

                SyncNextIds(exportNode);
                return true;
            }

            if (node.nodeType == CanonicalNodeKinds.StageAction)
            {
                exportNode.content = ReadTextPayload(node.payloadJson);
                exportNode.actionKind = CurrentFlowProjectionNaming.StageActionActionKindToken;
                if (!TryBuildLinearOutputs(graph, node.nodeId, CanonicalPortSemantics.ActionData, exportNode.outputs, out errorMessage))
                {
                    return false;
                }

                return true;
            }

            if (node.nodeType == CanonicalNodeKinds.Comment)
            {
                exportNode.content = ReadTextPayload(node.payloadJson);
                if (!TryBuildLinearOutputs(graph, node.nodeId, CanonicalPortSemantics.Flow, exportNode.outputs, out errorMessage))
                {
                    return false;
                }

                SyncNextIds(exportNode);
                return true;
            }

            if (node.nodeType == CanonicalNodeKinds.Start)
            {
                if (!TryBuildLinearOutputs(graph, node.nodeId, CanonicalPortSemantics.Flow, exportNode.outputs, out errorMessage))
                {
                    return false;
                }

                SyncNextIds(exportNode);
                return true;
            }

            if (node.nodeType == CanonicalNodeKinds.Choice)
            {
                ChoicePayload payload = ReadChoicePayload(node.payloadJson);
                exportNode.choiceMode = node.branchModeToken == "+" ? "+" : "*";
                for (int i = 0; i < node.branchCount; i++)
                {
                    string portName = BuildBranchOutputPortName(i);
                    string label = i < payload.labels.Count ? payload.labels[i] ?? string.Empty : string.Empty;
                    if (!TryBuildBranchOutput(graph, node.nodeId, portName, out ExportNodeOutputDto output, out errorMessage))
                    {
                        return false;
                    }

                    output.label = label;
                    exportNode.outputs.Add(output);
                }

                return true;
            }

            if (node.nodeType == CanonicalNodeKinds.Condition)
            {
                ConditionPayload payload = ReadConditionPayload(node.payloadJson);
                for (int i = 0; i < node.branchCount; i++)
                {
                    string portName = BuildBranchOutputPortName(i);
                    if (!TryBuildBranchOutput(graph, node.nodeId, portName, out ExportNodeOutputDto output, out errorMessage))
                    {
                        return false;
                    }

                    bool isElse = i == node.branchCount - 1;
                    output.isElse = isElse;
                    output.condition = isElse
                        ? string.Empty
                        : (i < payload.conditions.Count ? payload.conditions[i] ?? string.Empty : string.Empty);
                    exportNode.outputs.Add(output);
                }

                return true;
            }

            errorMessage = $"不支援的 canonical nodeType：{node.nodeType}";
            return false;
        }

        private static bool TryBuildLinearOutputs(
            CanonicalGraphDocument graph,
            string nodeId,
            string portName,
            List<ExportNodeOutputDto> outputs,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            outputs.Clear();

            if (!TryGetSingleOutgoingEdge(graph, nodeId, portName, out CanonicalGraphEdgeRecord edge, out errorMessage))
            {
                return false;
            }

            if (edge == null)
            {
                return true;
            }

            outputs.Add(new ExportNodeOutputDto
            {
                portName = portName,
                toNodeId = edge.toNodeId ?? string.Empty,
                toPortName = edge.toPort ?? string.Empty
            });
            return true;
        }

        private static bool TryBuildBranchOutput(
            CanonicalGraphDocument graph,
            string nodeId,
            string portName,
            out ExportNodeOutputDto output,
            out string errorMessage)
        {
            output = new ExportNodeOutputDto
            {
                portName = portName
            };

            if (!TryGetSingleOutgoingEdge(graph, nodeId, portName, out CanonicalGraphEdgeRecord edge, out errorMessage))
            {
                return false;
            }

            if (edge == null)
            {
                return true;
            }

            output.toNodeId = edge.toNodeId ?? string.Empty;
            output.toPortName = edge.toPort ?? string.Empty;
            return true;
        }

        private static bool TryGetSingleOutgoingEdge(
            CanonicalGraphDocument graph,
            string nodeId,
            string portName,
            out CanonicalGraphEdgeRecord edge,
            out string errorMessage)
        {
            edge = null;
            errorMessage = string.Empty;
            List<CanonicalGraphEdgeRecord> edges = graph?.edges ?? new List<CanonicalGraphEdgeRecord>();
            for (int i = 0; i < edges.Count; i++)
            {
                CanonicalGraphEdgeRecord candidate = edges[i];
                if (candidate == null
                    || !string.Equals(candidate.fromNodeId, nodeId, StringComparison.Ordinal)
                    || !string.Equals(candidate.fromPort, portName, StringComparison.Ordinal))
                {
                    continue;
                }

                if (edge != null)
                {
                    errorMessage = $"projection 失敗：埠 `{nodeId}:{portName}` 出現多條輸出連線。";
                    return false;
                }

                edge = candidate;
            }

            return true;
        }

        private static void SyncNextIds(ExportNodeDto exportNode)
        {
            exportNode.nextIds.Clear();
            if (exportNode.outputs != null
                && exportNode.outputs.Count == 1
                && string.Equals(exportNode.outputs[0].portName, CanonicalPortSemantics.Flow, StringComparison.Ordinal)
                && !string.IsNullOrEmpty(exportNode.outputs[0].toNodeId))
            {
                exportNode.nextIds.Add(exportNode.outputs[0].toNodeId);
            }
        }

        private static List<ExportNodeOutputDto> GetEffectiveOutputs(ExportNodeDto exportNode)
        {
            if (exportNode.outputs != null && exportNode.outputs.Count > 0)
            {
                return exportNode.outputs;
            }

            var outputs = new List<ExportNodeOutputDto>();
            List<string> nextIds = exportNode.nextIds ?? new List<string>();
            for (int i = 0; i < nextIds.Count; i++)
            {
                string nextId = nextIds[i] ?? string.Empty;
                if (string.IsNullOrEmpty(nextId))
                {
                    continue;
                }

                outputs.Add(new ExportNodeOutputDto
                {
                    portName = CanonicalPortSemantics.Flow,
                    toNodeId = nextId,
                    toPortName = CanonicalPortSemantics.Flow
                });
            }

            return outputs;
        }

        private static string BuildPayloadJson(string canonicalNodeType, ExportNodeDto exportNode)
        {
            if (canonicalNodeType == CanonicalNodeKinds.Dialogue
                || canonicalNodeType == CanonicalNodeKinds.StageAction
                || canonicalNodeType == CanonicalNodeKinds.Comment)
            {
                return JsonUtility.ToJson(new TextPayload
                {
                    content = exportNode.content ?? string.Empty
                });
            }

            if (canonicalNodeType == CanonicalNodeKinds.Choice)
            {
                ChoicePayload payload = new ChoicePayload();
                List<ExportNodeOutputDto> outputs = exportNode.outputs ?? new List<ExportNodeOutputDto>();
                for (int i = 0; i < outputs.Count; i++)
                {
                    payload.labels.Add(outputs[i] != null ? outputs[i].label ?? string.Empty : string.Empty);
                }

                return JsonUtility.ToJson(payload);
            }

            if (canonicalNodeType == CanonicalNodeKinds.Condition)
            {
                ConditionPayload payload = new ConditionPayload();
                List<ExportNodeOutputDto> outputs = exportNode.outputs ?? new List<ExportNodeOutputDto>();
                for (int i = 0; i < outputs.Count; i++)
                {
                    ExportNodeOutputDto output = outputs[i];
                    if (output != null && output.isElse)
                    {
                        break;
                    }

                    payload.conditions.Add(output != null ? output.condition ?? string.Empty : string.Empty);
                }

                return JsonUtility.ToJson(payload);
            }

            return string.Empty;
        }

        private static string ToCanonicalNodeType(string exportNodeType)
        {
            if (CurrentFlowProjectionNaming.IsDialogueNodeType(exportNodeType))
            {
                return CanonicalNodeKinds.Dialogue;
            }

            if (string.Equals(exportNodeType, CanonicalNodeKinds.Start, StringComparison.OrdinalIgnoreCase))
            {
                return CanonicalNodeKinds.Start;
            }

            if (string.Equals(exportNodeType, CanonicalNodeKinds.StageAction, StringComparison.OrdinalIgnoreCase))
            {
                return CanonicalNodeKinds.StageAction;
            }

            if (string.Equals(exportNodeType, CanonicalNodeKinds.Comment, StringComparison.OrdinalIgnoreCase))
            {
                return CanonicalNodeKinds.Comment;
            }

            if (string.Equals(exportNodeType, CanonicalNodeKinds.Choice, StringComparison.OrdinalIgnoreCase))
            {
                return CanonicalNodeKinds.Choice;
            }

            if (string.Equals(exportNodeType, CanonicalNodeKinds.Condition, StringComparison.OrdinalIgnoreCase))
            {
                return CanonicalNodeKinds.Condition;
            }

            return exportNodeType ?? string.Empty;
        }

        private static int GetMaxDialogueActionInputOrder(ExportGraphDto graphDto, string dialogueNodeId)
        {
            if (graphDto == null || graphDto.nodes == null || string.IsNullOrEmpty(dialogueNodeId))
            {
                return -1;
            }

            int maxOrder = -1;
            for (int i = 0; i < graphDto.nodes.Count; i++)
            {
                ExportNodeDto node = graphDto.nodes[i];
                if (node == null
                    || !string.Equals(node.type, CanonicalNodeKinds.StageAction, StringComparison.OrdinalIgnoreCase)
                    || node.outputs == null)
                {
                    continue;
                }

                for (int outputIndex = 0; outputIndex < node.outputs.Count; outputIndex++)
                {
                    ExportNodeOutputDto output = node.outputs[outputIndex];
                    if (output == null || !string.Equals(output.toNodeId, dialogueNodeId, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (CanonicalPortSemantics.TryParseActionInputOrder(output.toPortName, out int order) && order > maxOrder)
                    {
                        maxOrder = order;
                    }
                }
            }

            return maxOrder;
        }

        private static bool TryReadMetadata(string metadataJson, out CurrentFlowGraphMetadata metadata)
        {
            metadata = null;
            if (string.IsNullOrEmpty(metadataJson))
            {
                return false;
            }

            metadata = JsonUtility.FromJson<CurrentFlowGraphMetadata>(metadataJson);
            return metadata != null;
        }

        private static string ReadTextPayload(string payloadJson)
        {
            TextPayload payload = string.IsNullOrEmpty(payloadJson)
                ? new TextPayload()
                : JsonUtility.FromJson<TextPayload>(payloadJson) ?? new TextPayload();
            return payload.content ?? string.Empty;
        }

        private static ChoicePayload ReadChoicePayload(string payloadJson)
        {
            ChoicePayload payload = string.IsNullOrEmpty(payloadJson)
                ? new ChoicePayload()
                : JsonUtility.FromJson<ChoicePayload>(payloadJson) ?? new ChoicePayload();
            payload.labels ??= new List<string>();
            return payload;
        }

        private static ConditionPayload ReadConditionPayload(string payloadJson)
        {
            ConditionPayload payload = string.IsNullOrEmpty(payloadJson)
                ? new ConditionPayload()
                : JsonUtility.FromJson<ConditionPayload>(payloadJson) ?? new ConditionPayload();
            payload.conditions ??= new List<string>();
            return payload;
        }

        private static string BuildGraphId(ExportGraphDto exportDto)
        {
            if (!string.IsNullOrWhiteSpace(exportDto?.graphName))
            {
                return exportDto.graphName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(exportDto?.startNodeId))
            {
                return $"graph_{exportDto.startNodeId.Trim()}";
            }

            return "graph";
        }

        private static string BuildBranchOutputPortName(int index)
        {
            return $"Out{index}";
        }

        private static string BuildOperationErrorMessage(string prefix, CanonicalGraphValidationIssue[] errors)
        {
            if (errors == null || errors.Length == 0 || errors[0] == null)
            {
                return $"{prefix}。";
            }

            return $"{prefix}：{errors[0].code} {errors[0].message}";
        }

        private static string BuildValidationErrorMessage(string prefix, CanonicalGraphValidationIssue[] errors)
        {
            if (errors == null || errors.Length == 0 || errors[0] == null)
            {
                return $"{prefix}。";
            }

            return $"{prefix}：{errors[0].code} {errors[0].message}";
        }
    }
}
// ===== 變更結束 =====
