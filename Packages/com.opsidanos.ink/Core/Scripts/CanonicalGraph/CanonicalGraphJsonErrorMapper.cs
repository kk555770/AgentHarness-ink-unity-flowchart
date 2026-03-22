// ===== 變更開始 =====
// 2026/03/22 Opsidanos (修改原因：擴充 Batch 8/9 的 JSON contract mapper，補上 GetGraph snapshot response 組裝)
// 預期結果：dispatcher 的寫入與讀取路徑都由單一 mapper 組 response，`snapshot / warning / error / applied` 會維持穩定形狀
using System.Collections.Generic;

namespace OpsidanosInk.CanonicalGraph
{
    public static class CanonicalGraphJsonErrorMapper
    {
        public static CanonicalGraphJsonResponse BuildOperationResponse(CanonicalGraphOperationResult result, string graphId)
        {
            var response = new CanonicalGraphJsonResponse
            {
                success = result.success,
                applied = result.applied
            };

            response.result.graphId = !string.IsNullOrEmpty(graphId)
                ? graphId
                : result.graph != null ? result.graph.graphId ?? string.Empty : string.Empty;
            response.result.version = result.graph != null ? result.graph.version ?? string.Empty : string.Empty;
            response.result.nodeId = result.nodeId ?? string.Empty;
            response.result.edgeId = result.edgeId ?? string.Empty;
            response.warnings = MapIssues(result.warnings, response.result.graphId);
            response.errors = MapIssues(result.errors, response.result.graphId);
            return response;
        }

        public static CanonicalGraphJsonResponse BuildValidationResponse(CanonicalGraphValidationResult result, string graphId)
        {
            var response = new CanonicalGraphJsonResponse
            {
                success = result.success,
                applied = false
            };

            response.result.graphId = graphId ?? string.Empty;
            response.result.isValid = result.isValid;
            response.warnings = MapIssues(result.warnings, response.result.graphId);
            response.errors = MapIssues(result.errors, response.result.graphId);
            return response;
        }

        public static CanonicalGraphJsonResponse BuildSnapshotResponse(CanonicalGraphDocument graph)
        {
            var response = new CanonicalGraphJsonResponse
            {
                success = graph != null,
                applied = false
            };

            response.result.graphId = graph != null ? graph.graphId ?? string.Empty : string.Empty;
            response.result.version = graph != null ? graph.version ?? string.Empty : string.Empty;
            response.snapshot = CanonicalGraphJsonSnapshotMapper.ToSnapshot(graph);
            return response;
        }

        public static CanonicalGraphJsonResponse BuildFailureResponse(
            string graphId,
            string code,
            string message,
            string nodeId = "",
            string edgeId = "",
            string portName = "")
        {
            var response = new CanonicalGraphJsonResponse
            {
                success = false,
                applied = false
            };

            response.result.graphId = graphId ?? string.Empty;
            response.errors.Add(new CanonicalGraphJsonIssue
            {
                code = code ?? string.Empty,
                message = message ?? string.Empty,
                details = new CanonicalGraphJsonIssueDetails
                {
                    graphId = graphId ?? string.Empty,
                    nodeId = nodeId ?? string.Empty,
                    edgeId = edgeId ?? string.Empty,
                    portName = portName ?? string.Empty
                }
            });

            return response;
        }

        private static List<CanonicalGraphJsonIssue> MapIssues(CanonicalGraphValidationIssue[] issues, string graphId)
        {
            var mappedIssues = new List<CanonicalGraphJsonIssue>();
            if (issues == null)
            {
                return mappedIssues;
            }

            for (int i = 0; i < issues.Length; i++)
            {
                CanonicalGraphValidationIssue issue = issues[i];
                if (issue == null)
                {
                    continue;
                }

                mappedIssues.Add(new CanonicalGraphJsonIssue
                {
                    code = issue.code ?? string.Empty,
                    message = issue.message ?? string.Empty,
                    details = new CanonicalGraphJsonIssueDetails
                    {
                        graphId = graphId ?? string.Empty,
                        nodeId = issue.nodeId ?? string.Empty,
                        edgeId = issue.edgeId ?? string.Empty,
                        portName = issue.portName ?? string.Empty
                    }
                });
            }

            return mappedIssues;
        }
    }
}
// ===== 變更結束 =====
