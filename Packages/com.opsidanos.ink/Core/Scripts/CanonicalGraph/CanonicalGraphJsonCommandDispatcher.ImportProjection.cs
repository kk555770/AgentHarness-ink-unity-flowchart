// ===== 變更開始 =====
// 2026/03/25 Opsidanos (修改原因：把 ImportProjection 的 dispatcher 邏輯拆到 partial，避免再把主 dispatcher 檔案堆更大)
// 預期結果：JSON control plane 可直接把 `flowchart-json` 匯回 canonical graph，並把結果寫入 graph store
using System;

namespace OpsidanosInk.CanonicalGraph
{
    public sealed partial class CanonicalGraphJsonCommandDispatcher
    {
        private const string ErrorProjectionImportFailed = "PROJECTION_IMPORT_FAILED";

        private CanonicalGraphJsonResponse DispatchImportProjection(CanonicalGraphJsonRequestInput input)
        {
            string requestedGraphId = NormalizeGraphId(input != null ? input.graphId : string.Empty);
            string target = NormalizeProjectionTarget(input != null ? input.target : string.Empty);
            if (string.IsNullOrEmpty(target))
            {
                return CanonicalGraphJsonErrorMapper.BuildFailureResponse(requestedGraphId, ErrorInvalidRequest, "ImportProjection 需要 input.target。");
            }

            bool success = CurrentFlowProjectionImportService.TryImportProjection(
                target,
                input != null ? input.projectionVersion : string.Empty,
                input != null ? input.projectionText : string.Empty,
                input != null ? input.projectionJson : string.Empty,
                out string resolvedProjectionVersion,
                out CanonicalGraphDocument importedGraph,
                out string errorCode,
                out string errorMessage);

            string responseGraphId = !string.IsNullOrEmpty(requestedGraphId)
                ? requestedGraphId
                : importedGraph != null ? NormalizeGraphId(importedGraph.graphId) : string.Empty;

            if (!success)
            {
                return CanonicalGraphJsonErrorMapper.BuildFailureResponse(
                    responseGraphId,
                    string.IsNullOrEmpty(errorCode) ? ErrorProjectionImportFailed : errorCode,
                    string.IsNullOrEmpty(errorMessage) ? "projection 匯入失敗。" : errorMessage);
            }

            if (importedGraph == null)
            {
                return CanonicalGraphJsonErrorMapper.BuildFailureResponse(
                    responseGraphId,
                    ErrorProjectionImportFailed,
                    "projection 匯入失敗：沒有產生 canonical graph。");
            }

            string normalizedGraphId = !string.IsNullOrEmpty(requestedGraphId)
                ? requestedGraphId
                : NormalizeGraphId(importedGraph.graphId);
            if (string.IsNullOrEmpty(normalizedGraphId))
            {
                return CanonicalGraphJsonErrorMapper.BuildFailureResponse(
                    string.Empty,
                    ErrorProjectionImportFailed,
                    "projection 匯入失敗：canonical graphId 為空。");
            }

            if (graphById.ContainsKey(normalizedGraphId))
            {
                return CanonicalGraphJsonErrorMapper.BuildFailureResponse(
                    normalizedGraphId,
                    ErrorDuplicateGraphId,
                    $"重複的 graphId：{normalizedGraphId}");
            }

            if (!string.Equals(importedGraph.graphId, normalizedGraphId, StringComparison.Ordinal))
            {
                importedGraph.graphId = normalizedGraphId;
            }

            graphById[normalizedGraphId] = importedGraph;
            return CanonicalGraphJsonErrorMapper.BuildImportProjectionResponse(
                importedGraph,
                target,
                resolvedProjectionVersion);
        }
    }
}
// ===== 變更結束 =====
