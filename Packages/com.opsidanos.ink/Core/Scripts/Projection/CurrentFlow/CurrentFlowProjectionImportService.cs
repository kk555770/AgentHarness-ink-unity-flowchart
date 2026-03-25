// ===== 變更開始 =====
// 2026/03/25 Opsidanos (修改原因：補上 target-based projection import service，讓 control plane 不只可 project，也能把 projection 還原成 canonical graph)
// 預期結果：`ImportProjection(flowchart-json)` 可透過 shared bridge 直接產生 canonical graph，unsupported target 則回固定錯誤碼
using System;
using UnityEngine;

namespace OpsidanosInk.CanonicalGraph
{
    public static class CurrentFlowProjectionImportService
    {
        private const string InvalidRequestError = "INVALID_REQUEST";
        private const string ProjectionImportFailedError = "PROJECTION_IMPORT_FAILED";
        private const string ProjectionUnsupportedError = "PROJECTION_UNSUPPORTED";
        private const string DefaultProjectionVersion = "2.0";

        public static bool TryImportProjection(
            string target,
            string projectionVersion,
            string projectionText,
            string projectionJson,
            out string resolvedProjectionVersion,
            out CanonicalGraphDocument graph,
            out string errorCode,
            out string errorMessage)
        {
            resolvedProjectionVersion = ResolveProjectionVersion(projectionVersion);
            graph = null;
            errorCode = string.Empty;
            errorMessage = string.Empty;

            switch (NormalizeTarget(target))
            {
                case CurrentFlowProjectionService.FlowchartJsonTarget:
                    return TryImportFlowchartJson(
                        projectionVersion,
                        projectionJson,
                        out resolvedProjectionVersion,
                        out graph,
                        out errorCode,
                        out errorMessage);

                case CurrentFlowProjectionService.InkTarget:
                    errorCode = ProjectionUnsupportedError;
                    errorMessage = "目前尚未支援從 Ink 文字直接還原 canonical graph。";
                    return false;

                default:
                    errorCode = ProjectionUnsupportedError;
                    errorMessage = $"不支援的 projection target：{target}";
                    return false;
            }
        }

        private static bool TryImportFlowchartJson(
            string projectionVersion,
            string projectionJson,
            out string resolvedProjectionVersion,
            out CanonicalGraphDocument graph,
            out string errorCode,
            out string errorMessage)
        {
            resolvedProjectionVersion = ResolveProjectionVersion(projectionVersion);
            graph = null;
            errorCode = string.Empty;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(projectionJson))
            {
                errorCode = InvalidRequestError;
                errorMessage = "ImportProjection(flowchart-json) 需要 input.projectionJson。";
                return false;
            }

            ExportGraphDto exportDto;
            try
            {
                exportDto = JsonUtility.FromJson<ExportGraphDto>(projectionJson);
            }
            catch (Exception exception)
            {
                errorCode = InvalidRequestError;
                errorMessage = $"projectionJson 無法解析：{exception.Message}";
                return false;
            }

            if (exportDto == null)
            {
                errorCode = InvalidRequestError;
                errorMessage = "projectionJson 無法解析成 current projection DTO。";
                return false;
            }

            if (!CurrentFlowCanonicalGraphAdapter.TryBuildCanonicalGraph(exportDto, out graph, out errorMessage))
            {
                errorCode = ProjectionImportFailedError;
                graph = null;
                return false;
            }

            resolvedProjectionVersion = ResolveProjectionVersion(
                !string.IsNullOrWhiteSpace(exportDto.version)
                    ? exportDto.version
                    : projectionVersion);
            return true;
        }

        private static string NormalizeTarget(string target)
        {
            return string.IsNullOrWhiteSpace(target) ? string.Empty : target.Trim();
        }

        private static string ResolveProjectionVersion(string projectionVersion)
        {
            return string.IsNullOrWhiteSpace(projectionVersion)
                ? DefaultProjectionVersion
                : projectionVersion.Trim();
        }
    }
}
// ===== 變更結束 =====
