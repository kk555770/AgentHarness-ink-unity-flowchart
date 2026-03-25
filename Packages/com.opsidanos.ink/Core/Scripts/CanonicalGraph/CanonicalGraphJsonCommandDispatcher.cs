// ===== 變更開始 =====
// 2026/03/22 Opsidanos (修改原因：擴充 Batch 8/9 的 JSON command dispatcher，補上 GetGraph 與最小讀寫回圈)
// 預期結果：repo 的 JSON control plane 不只可下指令，也可穩定讀回 canonical snapshot，成為最小可讀可寫 bridge
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpsidanosInk.CanonicalGraph
{
    public sealed partial class CanonicalGraphJsonCommandDispatcher
    {
        private const string ErrorInvalidRequest = "INVALID_REQUEST";
        private const string ErrorUnsupportedContractVersion = "UNSUPPORTED_CONTRACT_VERSION";
        private const string ErrorUnsupportedOperation = "UNSUPPORTED_OPERATION";
        private const string ErrorDuplicateGraphId = "DUPLICATE_GRAPH_ID";
        // ===== 變更開始 =====
        // 2026/03/23 Opsidanos (修改原因：開始落地 Batch 11A，讓 dispatcher 對 projection-specific failure 有穩定錯誤碼)
        // 預期結果：ValidateProjection / ProjectGraph 可清楚區分 unsupported target 與 projection mapping loss
        private const string ErrorProjectionUnsupported = "PROJECTION_UNSUPPORTED";
        private const string ErrorProjectionMappingLoss = "PROJECTION_MAPPING_LOSS";
        // ===== 變更結束 =====

        [Serializable]
        private sealed class MetadataPayload
        {
            public string graphName = string.Empty;
            public string projectionVersion = string.Empty;
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

        private readonly Dictionary<string, CanonicalGraphDocument> graphById = new Dictionary<string, CanonicalGraphDocument>(StringComparer.Ordinal);

        public bool TryDispatch(string requestJson, out string responseJson)
        {
            CanonicalGraphJsonResponse response = Dispatch(requestJson);
            responseJson = JsonUtility.ToJson(response, true);
            return response.success;
        }

        public CanonicalGraphJsonResponse Dispatch(string requestJson)
        {
            if (string.IsNullOrWhiteSpace(requestJson))
            {
                return CanonicalGraphJsonErrorMapper.BuildFailureResponse(string.Empty, ErrorInvalidRequest, "requestJson 不可為空。");
            }

            CanonicalGraphJsonRequest request;
            try
            {
                request = JsonUtility.FromJson<CanonicalGraphJsonRequest>(requestJson);
            }
            catch (Exception exception)
            {
                return CanonicalGraphJsonErrorMapper.BuildFailureResponse(string.Empty, ErrorInvalidRequest, $"requestJson 無法解析：{exception.Message}");
            }

            return Dispatch(request);
        }

        public CanonicalGraphJsonResponse Dispatch(CanonicalGraphJsonRequest request)
        {
            if (request == null)
            {
                return CanonicalGraphJsonErrorMapper.BuildFailureResponse(string.Empty, ErrorInvalidRequest, "request 不可為空。");
            }

            if (!string.Equals(request.contractVersion, CanonicalGraphJsonRequest.PlainJsonContractVersion, StringComparison.Ordinal))
            {
                return CanonicalGraphJsonErrorMapper.BuildFailureResponse(
                    request.input != null ? request.input.graphId : string.Empty,
                    ErrorUnsupportedContractVersion,
                    $"目前只支援 `{CanonicalGraphJsonRequest.PlainJsonContractVersion}`。");
            }

            if (string.IsNullOrWhiteSpace(request.operation))
            {
                return CanonicalGraphJsonErrorMapper.BuildFailureResponse(
                    request.input != null ? request.input.graphId : string.Empty,
                    ErrorInvalidRequest,
                    "operation 不可為空。");
            }

            CanonicalGraphJsonRequestInput input = request.input ?? new CanonicalGraphJsonRequestInput();

            switch (request.operation)
            {
                case "CreateGraph":
                    return DispatchCreateGraph(input);
                case "CreateNode":
                    return DispatchCreateNode(input);
                case "ConnectPorts":
                    return DispatchConnectPorts(input);
                case "ReplaceNodePayload":
                    return DispatchReplaceNodePayload(input);
                case "DisconnectEdge":
                    return DispatchDisconnectEdge(input);
                case "RemoveNode":
                    return DispatchRemoveNode(input);
                case "GetGraph":
                    return DispatchGetGraph(input);
                case "ValidateGraph":
                    return DispatchValidateGraph(input);
                // ===== 變更開始 =====
                // 2026/03/23 Opsidanos (修改原因：開始落地 Batch 11A，補上 projection-specific control plane operation routing)
                // 預期結果：JSON command dispatcher 可直接處理 ValidateProjection / ProjectGraph / ImportProjection，而不是把它們當 unknown op
                case "ValidateProjection":
                    return DispatchValidateProjection(input);
                case "ProjectGraph":
                    return DispatchProjectGraph(input);
                case "ImportProjection":
                    return DispatchImportProjection(input);
                // ===== 變更結束 =====
                default:
                    return CanonicalGraphJsonErrorMapper.BuildFailureResponse(
                        input.graphId,
                        ErrorUnsupportedOperation,
                        $"不支援的 operation：{request.operation}");
            }
        }

        private CanonicalGraphJsonResponse DispatchCreateGraph(CanonicalGraphJsonRequestInput input)
        {
            string normalizedGraphId = NormalizeGraphId(input != null ? input.graphId : string.Empty);
            if (string.IsNullOrEmpty(normalizedGraphId))
            {
                return CanonicalGraphJsonErrorMapper.BuildFailureResponse(string.Empty, "INVALID_GRAPH_ID", "graphId 不可為空。");
            }

            if (graphById.ContainsKey(normalizedGraphId))
            {
                return CanonicalGraphJsonErrorMapper.BuildFailureResponse(
                    normalizedGraphId,
                    ErrorDuplicateGraphId,
                    $"重複的 graphId：{normalizedGraphId}");
            }

            string metadataJson = BuildMetadataJson(input != null ? input.metadata : null);
            CanonicalGraphOperationResult result = CanonicalGraphCommandService.CreateGraph(
                normalizedGraphId,
                input != null ? input.version : string.Empty,
                metadataJson);

            if (result.success && result.graph != null && result.applied)
            {
                graphById[result.graph.graphId] = result.graph;
            }

            CanonicalGraphJsonResponse response = CanonicalGraphJsonErrorMapper.BuildOperationResponse(
                result,
                result.graph != null ? result.graph.graphId : normalizedGraphId);
            response.result.version = result.graph != null ? result.graph.version ?? string.Empty : string.Empty;
            return response;
        }

        private CanonicalGraphJsonResponse DispatchCreateNode(CanonicalGraphJsonRequestInput input)
        {
            if (!TryGetGraph(input != null ? input.graphId : string.Empty, out CanonicalGraphDocument graph, out CanonicalGraphJsonResponse graphFailure))
            {
                return graphFailure;
            }

            if (input == null || input.node == null)
            {
                return CanonicalGraphJsonErrorMapper.BuildFailureResponse(graph.graphId, ErrorInvalidRequest, "CreateNode 需要 input.node。");
            }

            CanonicalGraphNodeRecord node = BuildNodeRecord(input.node);
            CanonicalGraphOperationResult result = CanonicalGraphCommandService.CreateNode(graph, node);
            return CanonicalGraphJsonErrorMapper.BuildOperationResponse(result, graph.graphId);
        }

        private CanonicalGraphJsonResponse DispatchConnectPorts(CanonicalGraphJsonRequestInput input)
        {
            if (!TryGetGraph(input != null ? input.graphId : string.Empty, out CanonicalGraphDocument graph, out CanonicalGraphJsonResponse graphFailure))
            {
                return graphFailure;
            }

            CanonicalGraphOperationResult result = CanonicalGraphCommandService.ConnectPorts(
                graph,
                input != null ? input.fromNodeId : string.Empty,
                input != null ? input.fromPort : string.Empty,
                input != null ? input.toNodeId : string.Empty,
                input != null ? input.toPort : string.Empty);

            return CanonicalGraphJsonErrorMapper.BuildOperationResponse(result, graph.graphId);
        }

        // ===== 變更開始 =====
        // 2026/03/22 Opsidanos (修改原因：開始落地 Batch 10，補上 JSON mutation dispatcher)
        // 預期結果：plain JSON control plane 可直接支援 ReplaceNodePayload / DisconnectEdge / RemoveNode，形成真正可編輯的作者工具 bridge
        private CanonicalGraphJsonResponse DispatchReplaceNodePayload(CanonicalGraphJsonRequestInput input)
        {
            if (!TryGetGraph(input != null ? input.graphId : string.Empty, out CanonicalGraphDocument graph, out CanonicalGraphJsonResponse graphFailure))
            {
                return graphFailure;
            }

            if (input == null || string.IsNullOrWhiteSpace(input.nodeId))
            {
                return CanonicalGraphJsonErrorMapper.BuildFailureResponse(graph.graphId, ErrorInvalidRequest, "ReplaceNodePayload 需要 input.nodeId。");
            }

            if (input.node == null || input.node.payload == null)
            {
                return CanonicalGraphJsonErrorMapper.BuildFailureResponse(graph.graphId, ErrorInvalidRequest, "ReplaceNodePayload 需要 input.node.payload。");
            }

            CanonicalGraphNodeRecord existingNode = FindNode(graph, input.nodeId);
            if (existingNode == null)
            {
                return CanonicalGraphJsonErrorMapper.BuildFailureResponse(graph.graphId, "NODE_NOT_FOUND", $"找不到節點：{input.nodeId}", nodeId: input.nodeId);
            }

            string payloadJson = BuildPayloadJson(existingNode.nodeType, input.node.payload);
            CanonicalGraphOperationResult result = CanonicalGraphCommandService.ReplaceNodePayload(graph, input.nodeId, payloadJson);
            return CanonicalGraphJsonErrorMapper.BuildOperationResponse(result, graph.graphId);
        }

        private CanonicalGraphJsonResponse DispatchDisconnectEdge(CanonicalGraphJsonRequestInput input)
        {
            if (!TryGetGraph(input != null ? input.graphId : string.Empty, out CanonicalGraphDocument graph, out CanonicalGraphJsonResponse graphFailure))
            {
                return graphFailure;
            }

            CanonicalGraphOperationResult result = CanonicalGraphCommandService.DisconnectEdge(
                graph,
                input != null ? input.edgeId : string.Empty,
                input != null ? input.fromNodeId : string.Empty,
                input != null ? input.fromPort : string.Empty,
                input != null ? input.toNodeId : string.Empty,
                input != null ? input.toPort : string.Empty);

            return CanonicalGraphJsonErrorMapper.BuildOperationResponse(result, graph.graphId);
        }

        private CanonicalGraphJsonResponse DispatchRemoveNode(CanonicalGraphJsonRequestInput input)
        {
            if (!TryGetGraph(input != null ? input.graphId : string.Empty, out CanonicalGraphDocument graph, out CanonicalGraphJsonResponse graphFailure))
            {
                return graphFailure;
            }

            CanonicalGraphOperationResult result = CanonicalGraphCommandService.RemoveNode(graph, input != null ? input.nodeId : string.Empty);
            return CanonicalGraphJsonErrorMapper.BuildOperationResponse(result, graph.graphId);
        }
        // ===== 變更結束 =====

        private CanonicalGraphJsonResponse DispatchGetGraph(CanonicalGraphJsonRequestInput input)
        {
            if (!TryGetGraph(input != null ? input.graphId : string.Empty, out CanonicalGraphDocument graph, out CanonicalGraphJsonResponse graphFailure))
            {
                return graphFailure;
            }

            return CanonicalGraphJsonErrorMapper.BuildSnapshotResponse(graph);
        }

        private CanonicalGraphJsonResponse DispatchValidateGraph(CanonicalGraphJsonRequestInput input)
        {
            if (!TryGetGraph(input != null ? input.graphId : string.Empty, out CanonicalGraphDocument graph, out CanonicalGraphJsonResponse graphFailure))
            {
                return graphFailure;
            }

            CanonicalGraphValidationResult result = CanonicalGraphCommandService.ValidateGraph(graph);
            return CanonicalGraphJsonErrorMapper.BuildValidationResponse(result, graph.graphId);
        }

        // ===== 變更開始 =====
        // 2026/03/23 Opsidanos (修改原因：開始落地 Batch 11A，補上 projection-specific validate / project dispatcher)
        // 預期結果：canonical JSON control plane 可直接驗證並產出 `flowchart-json / ink`，不必再透過 exporter 當唯一入口
        private CanonicalGraphJsonResponse DispatchValidateProjection(CanonicalGraphJsonRequestInput input)
        {
            if (!TryGetGraph(input != null ? input.graphId : string.Empty, out CanonicalGraphDocument graph, out CanonicalGraphJsonResponse graphFailure))
            {
                return graphFailure;
            }

            string target = NormalizeProjectionTarget(input != null ? input.target : string.Empty);
            if (string.IsNullOrEmpty(target))
            {
                return CanonicalGraphJsonErrorMapper.BuildFailureResponse(graph.graphId, ErrorInvalidRequest, "ValidateProjection 需要 input.target。");
            }

            bool success = CurrentFlowProjectionService.TryValidateProjection(
                graph,
                target,
                input != null ? input.projectionVersion : string.Empty,
                out string resolvedProjectionVersion,
                out string errorCode,
                out string errorMessage);

            if (success)
            {
                return CanonicalGraphJsonErrorMapper.BuildProjectionValidationResponse(
                    CanonicalGraphValidationResult.Completed(true, Array.Empty<CanonicalGraphValidationIssue>(), Array.Empty<CanonicalGraphValidationIssue>()),
                    graph.graphId,
                    graph.version,
                    target,
                    resolvedProjectionVersion);
            }

            if (errorCode == ErrorProjectionUnsupported)
            {
                return CanonicalGraphJsonErrorMapper.BuildFailureResponse(graph.graphId, errorCode, errorMessage);
            }

            var validationIssue = new CanonicalGraphValidationIssue
            {
                code = string.IsNullOrEmpty(errorCode) ? ErrorProjectionMappingLoss : errorCode,
                message = string.IsNullOrEmpty(errorMessage) ? "projection 驗證失敗。" : errorMessage
            };

            return CanonicalGraphJsonErrorMapper.BuildProjectionValidationResponse(
                CanonicalGraphValidationResult.Completed(false, Array.Empty<CanonicalGraphValidationIssue>(), new[] { validationIssue }),
                graph.graphId,
                graph.version,
                target,
                resolvedProjectionVersion);
        }

        private CanonicalGraphJsonResponse DispatchProjectGraph(CanonicalGraphJsonRequestInput input)
        {
            if (!TryGetGraph(input != null ? input.graphId : string.Empty, out CanonicalGraphDocument graph, out CanonicalGraphJsonResponse graphFailure))
            {
                return graphFailure;
            }

            string target = NormalizeProjectionTarget(input != null ? input.target : string.Empty);
            if (string.IsNullOrEmpty(target))
            {
                return CanonicalGraphJsonErrorMapper.BuildFailureResponse(graph.graphId, ErrorInvalidRequest, "ProjectGraph 需要 input.target。");
            }

            bool success = CurrentFlowProjectionService.TryProjectGraph(
                graph,
                target,
                input != null ? input.projectionVersion : string.Empty,
                out string resolvedProjectionVersion,
                out string projectionText,
                out string projectionJson,
                out string errorCode,
                out string errorMessage);

            if (!success)
            {
                return CanonicalGraphJsonErrorMapper.BuildFailureResponse(
                    graph.graphId,
                    string.IsNullOrEmpty(errorCode) ? ErrorProjectionMappingLoss : errorCode,
                    string.IsNullOrEmpty(errorMessage) ? "projection 失敗。" : errorMessage);
            }

            return CanonicalGraphJsonErrorMapper.BuildProjectionResponse(
                graph.graphId,
                graph.version,
                target,
                resolvedProjectionVersion,
                projectionText,
                projectionJson);
        }
        // ===== 變更結束 =====

        private bool TryGetGraph(string graphId, out CanonicalGraphDocument graph, out CanonicalGraphJsonResponse failureResponse)
        {
            graph = null;
            failureResponse = null;
            string normalizedGraphId = NormalizeGraphId(graphId);

            if (string.IsNullOrEmpty(normalizedGraphId) || !graphById.TryGetValue(normalizedGraphId, out graph) || graph == null)
            {
                failureResponse = CanonicalGraphJsonErrorMapper.BuildFailureResponse(
                    normalizedGraphId,
                    "GRAPH_NOT_FOUND",
                    $"找不到 graph：{normalizedGraphId}");
                return false;
            }

            return true;
        }

        private static string NormalizeGraphId(string graphId)
        {
            return string.IsNullOrWhiteSpace(graphId) ? string.Empty : graphId.Trim();
        }

        // ===== 變更開始 =====
        // 2026/03/23 Opsidanos (修改原因：開始落地 Batch 11A，讓 projection target 與 graphId 一樣先做最小正規化)
        // 預期結果：dispatcher 在 ValidateProjection / ProjectGraph 時，不會因 target 前後空白而把 request 誤判成不同操作
        private static string NormalizeProjectionTarget(string target)
        {
            return string.IsNullOrWhiteSpace(target) ? string.Empty : target.Trim();
        }
        // ===== 變更結束 =====

        // ===== 變更開始 =====
        // 2026/03/22 Opsidanos (修改原因：開始落地 Batch 10，讓 dispatcher 可在 replace payload 前先查到既有 nodeType)
        // 預期結果：ReplaceNodePayload 只替換 payload，不會被 request 端偷偷改 nodeType
        private static CanonicalGraphNodeRecord FindNode(CanonicalGraphDocument graph, string nodeId)
        {
            if (graph == null || string.IsNullOrWhiteSpace(nodeId))
            {
                return null;
            }

            string normalizedNodeId = nodeId.Trim();
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                CanonicalGraphNodeRecord node = graph.nodes[i];
                if (node != null && node.nodeId == normalizedNodeId)
                {
                    return node;
                }
            }

            return null;
        }
        // ===== 變更結束 =====

        private static CanonicalGraphNodeRecord BuildNodeRecord(CanonicalGraphJsonNodeInput input)
        {
            CanonicalGraphJsonNodePayload payload = input.payload ?? new CanonicalGraphJsonNodePayload();
            string nodeType = input.nodeType ?? string.Empty;

            var node = new CanonicalGraphNodeRecord
            {
                nodeId = input.nodeId ?? string.Empty,
                nodeType = nodeType,
                payloadJson = BuildPayloadJson(nodeType, payload),
                dialogueActionInputCount = input.dialogueActionInputCount,
                branchCount = BuildBranchCount(input, payload),
                branchModeToken = input.branchModeToken ?? string.Empty
            };

            return node;
        }

        private static int BuildBranchCount(CanonicalGraphJsonNodeInput input, CanonicalGraphJsonNodePayload payload)
        {
            if (input.branchCount > 0)
            {
                return input.branchCount;
            }

            if (string.Equals(input.nodeType, CanonicalNodeKinds.Choice, StringComparison.OrdinalIgnoreCase))
            {
                return payload.labels != null ? payload.labels.Count : 0;
            }

            if (string.Equals(input.nodeType, CanonicalNodeKinds.Condition, StringComparison.OrdinalIgnoreCase))
            {
                int conditionCount = payload.conditions != null ? payload.conditions.Count : 0;
                return conditionCount > 0 ? conditionCount + 1 : 0;
            }

            return 0;
        }

        private static string BuildMetadataJson(CanonicalGraphJsonMetadataInput metadata)
        {
            if (metadata == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(metadata.rawJson))
            {
                return metadata.rawJson;
            }

            if (string.IsNullOrEmpty(metadata.graphName) && string.IsNullOrEmpty(metadata.projectionVersion))
            {
                return string.Empty;
            }

            return JsonUtility.ToJson(new MetadataPayload
            {
                graphName = metadata.graphName ?? string.Empty,
                projectionVersion = metadata.projectionVersion ?? string.Empty
            });
        }

        private static string BuildPayloadJson(string nodeType, CanonicalGraphJsonNodePayload payload)
        {
            if (string.Equals(nodeType, CanonicalNodeKinds.Dialogue, StringComparison.OrdinalIgnoreCase)
                || string.Equals(nodeType, CanonicalNodeKinds.StageAction, StringComparison.OrdinalIgnoreCase)
                || string.Equals(nodeType, CanonicalNodeKinds.Comment, StringComparison.OrdinalIgnoreCase))
            {
                return JsonUtility.ToJson(new TextPayload
                {
                    content = payload != null ? payload.content ?? string.Empty : string.Empty
                });
            }

            if (string.Equals(nodeType, CanonicalNodeKinds.Choice, StringComparison.OrdinalIgnoreCase))
            {
                return JsonUtility.ToJson(new ChoicePayload
                {
                    labels = payload != null && payload.labels != null
                        ? new List<string>(payload.labels)
                        : new List<string>()
                });
            }

            if (string.Equals(nodeType, CanonicalNodeKinds.Condition, StringComparison.OrdinalIgnoreCase))
            {
                return JsonUtility.ToJson(new ConditionPayload
                {
                    conditions = payload != null && payload.conditions != null
                        ? new List<string>(payload.conditions)
                        : new List<string>()
                });
            }

            return string.Empty;
        }
    }
}
// ===== 變更結束 =====
