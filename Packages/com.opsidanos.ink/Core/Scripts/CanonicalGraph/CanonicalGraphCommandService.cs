// ===== 變更開始 =====
// 2026/03/19 Opsidanos (修改原因：開始落地 Batch 4 bridge kickoff，建立最小 graph command service 骨架)
// 預期結果：repo 第一次具備 CreateGraph / CreateNode / ConnectPorts / ValidateGraph 的可呼叫最小控制面
using System;
using System.Collections.Generic;

namespace OpsidanosInk.CanonicalGraph
{
    public static class CanonicalGraphCommandService
    {
        private const string DefaultGraphVersion = "canonical-1";
        private const string ErrorInvalidGraphId = "INVALID_GRAPH_ID";
        private const string ErrorGraphNotFound = "GRAPH_NOT_FOUND";
        private const string ErrorDuplicateNodeId = "DUPLICATE_NODE_ID";
        private const string ErrorInvalidNodeType = "INVALID_NODE_TYPE";
        private const string ErrorInvalidPayload = "INVALID_PAYLOAD";
        private const string ErrorNodeNotFound = "NODE_NOT_FOUND";
        private const string ErrorInvalidPort = "INVALID_PORT";
        private const string ErrorInvalidEdgeKind = "INVALID_EDGE_KIND";
        private const string ErrorEdgeCardinalityViolation = "EDGE_CARDINALITY_VIOLATION";
        private const string ErrorMissingStartNode = "MISSING_START_NODE";
        private const string ErrorMultipleStartNodes = "MULTIPLE_START_NODES";
        private const string ErrorChoiceBranchInvalid = "CHOICE_BRANCH_INVALID";
        private const string ErrorConditionBranchInvalid = "CONDITION_BRANCH_INVALID";
        private const string ErrorElseBranchMissing = "ELSE_BRANCH_MISSING";

        public static CanonicalGraphOperationResult CreateGraph(string graphId, string version = "", string metadataJson = "")
        {
            if (string.IsNullOrWhiteSpace(graphId))
            {
                return CanonicalGraphOperationResult.Failure(
                    graph: null,
                    code: ErrorInvalidGraphId,
                    message: "graphId 不可為空。");
            }

            CanonicalGraphDocument graph = new CanonicalGraphDocument
            {
                graphId = graphId.Trim(),
                version = string.IsNullOrWhiteSpace(version) ? DefaultGraphVersion : version.Trim(),
                metadataJson = metadataJson ?? string.Empty
            };

            return CanonicalGraphOperationResult.Success(graph, applied: true);
        }

        public static CanonicalGraphOperationResult CreateNode(CanonicalGraphDocument graph, CanonicalGraphNodeRecord node)
        {
            if (!TryGetGraphOrFailure(graph, out CanonicalGraphOperationResult graphFailure))
            {
                return graphFailure;
            }

            if (node == null)
            {
                return CanonicalGraphOperationResult.Failure(graph, ErrorInvalidPayload, "node 不可為空。");
            }

            if (string.IsNullOrWhiteSpace(node.nodeId))
            {
                return CanonicalGraphOperationResult.Failure(graph, ErrorInvalidPayload, "nodeId 不可為空。");
            }

            if (!CanonicalNodeKinds.IsKnown(node.nodeType))
            {
                return CanonicalGraphOperationResult.Failure(graph, ErrorInvalidNodeType, $"未知的 nodeType：{node.nodeType}");
            }

            if (FindNode(graph, node.nodeId) != null)
            {
                return CanonicalGraphOperationResult.Failure(graph, ErrorDuplicateNodeId, $"重複的 nodeId：{node.nodeId}", nodeId: node.nodeId);
            }

            if (!TryValidateNodePayload(node, out CanonicalGraphValidationIssue payloadIssue))
            {
                return CanonicalGraphOperationResult.Failure(
                    graph,
                    payloadIssue.code,
                    payloadIssue.message,
                    nodeId: payloadIssue.nodeId,
                    portName: payloadIssue.portName);
            }

            graph.nodes.Add(CloneNode(node));
            return CanonicalGraphOperationResult.Success(graph, applied: true, nodeId: node.nodeId);
        }

        public static CanonicalGraphOperationResult ConnectPorts(
            CanonicalGraphDocument graph,
            string fromNodeId,
            string fromPort,
            string toNodeId,
            string toPort)
        {
            if (!TryGetGraphOrFailure(graph, out CanonicalGraphOperationResult graphFailure))
            {
                return graphFailure;
            }

            CanonicalGraphNodeRecord fromNode = FindNode(graph, fromNodeId);
            if (fromNode == null)
            {
                return CanonicalGraphOperationResult.Failure(graph, ErrorNodeNotFound, $"找不到起點節點：{fromNodeId}", nodeId: fromNodeId);
            }

            CanonicalGraphNodeRecord toNode = FindNode(graph, toNodeId);
            if (toNode == null)
            {
                return CanonicalGraphOperationResult.Failure(graph, ErrorNodeNotFound, $"找不到終點節點：{toNodeId}", nodeId: toNodeId);
            }

            PortKind fromPortKind = GetPortKind(fromNode, fromPort, isOutput: true);
            if (fromPortKind == PortKind.Unknown)
            {
                return CanonicalGraphOperationResult.Failure(graph, ErrorInvalidPort, $"不存在的輸出埠：{fromPort}", nodeId: fromNodeId, portName: fromPort);
            }

            PortKind toPortKind = GetPortKind(toNode, toPort, isOutput: false);
            if (toPortKind == PortKind.Unknown)
            {
                return CanonicalGraphOperationResult.Failure(graph, ErrorInvalidPort, $"不存在的輸入埠：{toPort}", nodeId: toNodeId, portName: toPort);
            }

            if (!ArePortsCompatible(fromPortKind, toPortKind))
            {
                return CanonicalGraphOperationResult.Failure(
                    graph,
                    ErrorInvalidEdgeKind,
                    $"不相容的連線：{fromPort} -> {toPort}",
                    nodeId: fromNodeId,
                    portName: fromPort);
            }

            CanonicalGraphEdgeRecord existingEdge = FindEdge(graph, fromNodeId, fromPort, toNodeId, toPort);
            if (existingEdge != null)
            {
                return CanonicalGraphOperationResult.Success(graph, applied: false, edgeId: existingEdge.edgeId);
            }

            if (HasPortConnection(graph, fromNodeId, fromPort, isOutput: true))
            {
                return CanonicalGraphOperationResult.Failure(
                    graph,
                    ErrorEdgeCardinalityViolation,
                    $"輸出埠已經有連線：{fromPort}",
                    nodeId: fromNodeId,
                    portName: fromPort);
            }

            if (HasPortConnection(graph, toNodeId, toPort, isOutput: false))
            {
                return CanonicalGraphOperationResult.Failure(
                    graph,
                    ErrorEdgeCardinalityViolation,
                    $"輸入埠已經有連線：{toPort}",
                    nodeId: toNodeId,
                    portName: toPort);
            }

            CanonicalGraphEdgeRecord edge = new CanonicalGraphEdgeRecord
            {
                edgeId = BuildEdgeId(fromNodeId, fromPort, toNodeId, toPort),
                fromNodeId = fromNodeId,
                fromPort = fromPort,
                toNodeId = toNodeId,
                toPort = toPort
            };

            graph.edges.Add(edge);
            return CanonicalGraphOperationResult.Success(graph, applied: true, edgeId: edge.edgeId);
        }

        public static CanonicalGraphValidationResult ValidateGraph(CanonicalGraphDocument graph)
        {
            if (graph == null)
            {
                return CanonicalGraphValidationResult.Failure(ErrorGraphNotFound, "graph 不可為空。");
            }

            List<CanonicalGraphValidationIssue> warnings = new List<CanonicalGraphValidationIssue>();
            List<CanonicalGraphValidationIssue> errors = new List<CanonicalGraphValidationIssue>();

            int startNodeCount = 0;
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                CanonicalGraphNodeRecord node = graph.nodes[i];
                if (node == null)
                {
                    errors.Add(CreateIssue(ErrorInvalidPayload, "graph 內含空節點。"));
                    continue;
                }

                if (!TryValidateNodePayload(node, out CanonicalGraphValidationIssue payloadIssue))
                {
                    errors.Add(payloadIssue);
                }

                if (node.nodeType == CanonicalNodeKinds.Start)
                {
                    startNodeCount++;
                }
            }

            if (startNodeCount == 0)
            {
                errors.Add(CreateIssue(ErrorMissingStartNode, "graph 缺少 start 節點。"));
            }
            else if (startNodeCount > 1)
            {
                errors.Add(CreateIssue(ErrorMultipleStartNodes, "graph 不可有超過一個 start 節點。"));
            }

            for (int i = 0; i < graph.edges.Count; i++)
            {
                CanonicalGraphEdgeRecord edge = graph.edges[i];
                if (edge == null)
                {
                    errors.Add(CreateIssue(ErrorInvalidPayload, "graph 內含空連線。"));
                    continue;
                }

                CanonicalGraphNodeRecord fromNode = FindNode(graph, edge.fromNodeId);
                CanonicalGraphNodeRecord toNode = FindNode(graph, edge.toNodeId);
                if (fromNode == null)
                {
                    errors.Add(CreateIssue(ErrorNodeNotFound, $"連線起點不存在：{edge.fromNodeId}", edgeId: edge.edgeId, nodeId: edge.fromNodeId));
                    continue;
                }

                if (toNode == null)
                {
                    errors.Add(CreateIssue(ErrorNodeNotFound, $"連線終點不存在：{edge.toNodeId}", edgeId: edge.edgeId, nodeId: edge.toNodeId));
                    continue;
                }

                PortKind fromPortKind = GetPortKind(fromNode, edge.fromPort, isOutput: true);
                if (fromPortKind == PortKind.Unknown)
                {
                    errors.Add(CreateIssue(ErrorInvalidPort, $"不存在的輸出埠：{edge.fromPort}", edge.edgeId, edge.fromNodeId, edge.fromPort));
                    continue;
                }

                PortKind toPortKind = GetPortKind(toNode, edge.toPort, isOutput: false);
                if (toPortKind == PortKind.Unknown)
                {
                    errors.Add(CreateIssue(ErrorInvalidPort, $"不存在的輸入埠：{edge.toPort}", edge.edgeId, edge.toNodeId, edge.toPort));
                    continue;
                }

                if (!ArePortsCompatible(fromPortKind, toPortKind))
                {
                    errors.Add(CreateIssue(ErrorInvalidEdgeKind, $"不相容的連線：{edge.fromPort} -> {edge.toPort}", edge.edgeId, edge.fromNodeId, edge.fromPort));
                }
            }

            return CanonicalGraphValidationResult.Completed(
                isValid: errors.Count == 0,
                warnings: warnings.ToArray(),
                errors: errors.ToArray());
        }

        private static bool TryGetGraphOrFailure(CanonicalGraphDocument graph, out CanonicalGraphOperationResult failure)
        {
            if (graph == null)
            {
                failure = CanonicalGraphOperationResult.Failure(null, ErrorGraphNotFound, "graph 不可為空。");
                return false;
            }

            failure = default;
            return true;
        }

        private static CanonicalGraphNodeRecord FindNode(CanonicalGraphDocument graph, string nodeId)
        {
            if (graph == null || string.IsNullOrEmpty(nodeId))
            {
                return null;
            }

            for (int i = 0; i < graph.nodes.Count; i++)
            {
                CanonicalGraphNodeRecord node = graph.nodes[i];
                if (node != null && node.nodeId == nodeId)
                {
                    return node;
                }
            }

            return null;
        }

        private static CanonicalGraphEdgeRecord FindEdge(
            CanonicalGraphDocument graph,
            string fromNodeId,
            string fromPort,
            string toNodeId,
            string toPort)
        {
            if (graph == null)
            {
                return null;
            }

            for (int i = 0; i < graph.edges.Count; i++)
            {
                CanonicalGraphEdgeRecord edge = graph.edges[i];
                if (edge != null
                    && edge.fromNodeId == fromNodeId
                    && edge.fromPort == fromPort
                    && edge.toNodeId == toNodeId
                    && edge.toPort == toPort)
                {
                    return edge;
                }
            }

            return null;
        }

        private static bool HasPortConnection(CanonicalGraphDocument graph, string nodeId, string portName, bool isOutput)
        {
            for (int i = 0; i < graph.edges.Count; i++)
            {
                CanonicalGraphEdgeRecord edge = graph.edges[i];
                if (edge == null)
                {
                    continue;
                }

                if (isOutput && edge.fromNodeId == nodeId && edge.fromPort == portName)
                {
                    return true;
                }

                if (!isOutput && edge.toNodeId == nodeId && edge.toPort == portName)
                {
                    return true;
                }
            }

            return false;
        }

        private static CanonicalGraphNodeRecord CloneNode(CanonicalGraphNodeRecord source)
        {
            return new CanonicalGraphNodeRecord
            {
                nodeId = source.nodeId ?? string.Empty,
                nodeType = source.nodeType ?? string.Empty,
                payloadJson = source.payloadJson ?? string.Empty,
                dialogueActionInputCount = source.dialogueActionInputCount,
                branchCount = source.branchCount,
                branchModeToken = source.branchModeToken ?? string.Empty
            };
        }

        private static bool TryValidateNodePayload(CanonicalGraphNodeRecord node, out CanonicalGraphValidationIssue issue)
        {
            issue = null;

            if (string.IsNullOrWhiteSpace(node.nodeId))
            {
                issue = CreateIssue(ErrorInvalidPayload, "nodeId 不可為空。", nodeId: node.nodeId);
                return false;
            }

            if (!CanonicalNodeKinds.IsKnown(node.nodeType))
            {
                issue = CreateIssue(ErrorInvalidNodeType, $"未知的 nodeType：{node.nodeType}", nodeId: node.nodeId);
                return false;
            }

            if (node.nodeType == CanonicalNodeKinds.Dialogue && node.dialogueActionInputCount < 0)
            {
                issue = CreateIssue(ErrorInvalidPayload, "dialogueActionInputCount 不可小於 0。", nodeId: node.nodeId);
                return false;
            }

            if (node.nodeType == CanonicalNodeKinds.Choice && node.branchCount <= 0)
            {
                issue = CreateIssue(ErrorChoiceBranchInvalid, "choice 節點至少要有一個分支。", nodeId: node.nodeId);
                return false;
            }

            if (node.nodeType == CanonicalNodeKinds.Condition && node.branchCount <= 0)
            {
                issue = CreateIssue(ErrorConditionBranchInvalid, "condition 節點至少要有一個條件分支。", nodeId: node.nodeId);
                return false;
            }

            if (node.nodeType == CanonicalNodeKinds.Condition && node.branchCount < 2)
            {
                issue = CreateIssue(ErrorElseBranchMissing, "condition 節點至少要保留一條 else 分支。", nodeId: node.nodeId);
                return false;
            }

            return true;
        }

        private static PortKind GetPortKind(CanonicalGraphNodeRecord node, string portName, bool isOutput)
        {
            if (node == null || string.IsNullOrWhiteSpace(portName))
            {
                return PortKind.Unknown;
            }

            switch (node.nodeType)
            {
                case var _ when node.nodeType == CanonicalNodeKinds.Start:
                    return isOutput && portName == CanonicalPortSemantics.Flow ? PortKind.Flow : PortKind.Unknown;

                case var _ when node.nodeType == CanonicalNodeKinds.Dialogue:
                    if (isOutput)
                    {
                        return portName == CanonicalPortSemantics.Flow ? PortKind.Flow : PortKind.Unknown;
                    }

                    if (portName == CanonicalPortSemantics.Flow)
                    {
                        return PortKind.Flow;
                    }

                    for (int i = 0; i < node.dialogueActionInputCount; i++)
                    {
                        if (portName == CanonicalPortSemantics.BuildActionInputPortName(i))
                        {
                            return PortKind.ActionInput;
                        }
                    }

                    return PortKind.Unknown;

                case var _ when node.nodeType == CanonicalNodeKinds.StageAction:
                    return isOutput && portName == CanonicalPortSemantics.ActionData ? PortKind.ActionData : PortKind.Unknown;

                case var _ when node.nodeType == CanonicalNodeKinds.Comment:
                    return portName == CanonicalPortSemantics.Flow ? PortKind.Flow : PortKind.Unknown;

                case var _ when node.nodeType == CanonicalNodeKinds.Choice:
                case var _ when node.nodeType == CanonicalNodeKinds.Condition:
                    if (!isOutput)
                    {
                        return portName == CanonicalPortSemantics.Flow ? PortKind.Flow : PortKind.Unknown;
                    }

                    for (int i = 0; i < node.branchCount; i++)
                    {
                        if (portName == BuildBranchOutputPortName(i))
                        {
                            return PortKind.Flow;
                        }
                    }

                    return PortKind.Unknown;
            }

            return PortKind.Unknown;
        }

        private static bool ArePortsCompatible(PortKind fromPortKind, PortKind toPortKind)
        {
            return (fromPortKind == PortKind.Flow && toPortKind == PortKind.Flow)
                || (fromPortKind == PortKind.ActionData && toPortKind == PortKind.ActionInput);
        }

        private static string BuildEdgeId(string fromNodeId, string fromPort, string toNodeId, string toPort)
        {
            return $"{fromNodeId}:{fromPort}->{toNodeId}:{toPort}";
        }

        private static string BuildBranchOutputPortName(int index)
        {
            return $"Out{index}";
        }

        private static CanonicalGraphValidationIssue CreateIssue(
            string code,
            string message,
            string edgeId = "",
            string nodeId = "",
            string portName = "")
        {
            return new CanonicalGraphValidationIssue
            {
                code = code ?? string.Empty,
                message = message ?? string.Empty,
                edgeId = edgeId ?? string.Empty,
                nodeId = nodeId ?? string.Empty,
                portName = portName ?? string.Empty
            };
        }

        private enum PortKind
        {
            Unknown = 0,
            Flow = 1,
            ActionData = 2,
            ActionInput = 3
        }
    }
}
// ===== 變更結束 =====
