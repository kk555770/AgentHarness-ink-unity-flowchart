// ===== 變更開始 =====
// 2026/03/19 Opsidanos (修改原因：開始落地 Batch 4 bridge kickoff，建立 graph mutation 操作共用 result 骨架)
// 預期結果：CreateGraph / CreateNode / ConnectPorts 可一致回傳 success、applied、warnings、errors 與最小識別結果
using System;

namespace OpsidanosInk.CanonicalGraph
{
    public readonly struct CanonicalGraphOperationResult
    {
        public readonly bool success;
        public readonly bool applied;
        public readonly CanonicalGraphDocument graph;
        public readonly string nodeId;
        public readonly string edgeId;
        public readonly CanonicalGraphValidationIssue[] warnings;
        public readonly CanonicalGraphValidationIssue[] errors;

        private CanonicalGraphOperationResult(
            bool success,
            bool applied,
            CanonicalGraphDocument graph,
            string nodeId,
            string edgeId,
            CanonicalGraphValidationIssue[] warnings,
            CanonicalGraphValidationIssue[] errors)
        {
            this.success = success;
            this.applied = applied;
            this.graph = graph;
            this.nodeId = nodeId ?? string.Empty;
            this.edgeId = edgeId ?? string.Empty;
            this.warnings = warnings ?? Array.Empty<CanonicalGraphValidationIssue>();
            this.errors = errors ?? Array.Empty<CanonicalGraphValidationIssue>();
        }

        public static CanonicalGraphOperationResult Success(
            CanonicalGraphDocument graph,
            bool applied,
            string nodeId = "",
            string edgeId = "",
            CanonicalGraphValidationIssue[] warnings = null)
        {
            return new CanonicalGraphOperationResult(
                success: true,
                applied: applied,
                graph: graph,
                nodeId: nodeId,
                edgeId: edgeId,
                warnings: warnings,
                errors: Array.Empty<CanonicalGraphValidationIssue>());
        }

        public static CanonicalGraphOperationResult Failure(
            CanonicalGraphDocument graph,
            string code,
            string message,
            string nodeId = "",
            string edgeId = "",
            string portName = "")
        {
            CanonicalGraphValidationIssue error = new CanonicalGraphValidationIssue
            {
                code = code ?? string.Empty,
                message = message ?? string.Empty,
                nodeId = nodeId ?? string.Empty,
                edgeId = edgeId ?? string.Empty,
                portName = portName ?? string.Empty
            };

            return new CanonicalGraphOperationResult(
                success: false,
                applied: false,
                graph: graph,
                nodeId: nodeId,
                edgeId: edgeId,
                warnings: Array.Empty<CanonicalGraphValidationIssue>(),
                errors: new[] { error });
        }
    }
}
// ===== 變更結束 =====
