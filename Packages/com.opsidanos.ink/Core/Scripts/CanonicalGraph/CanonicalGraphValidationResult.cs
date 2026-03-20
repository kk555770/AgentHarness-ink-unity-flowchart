// ===== 變更開始 =====
// 2026/03/19 Opsidanos (修改原因：開始落地 Batch 4 bridge kickoff，建立 ValidateGraph 專用 result 骨架)
// 預期結果：ValidateGraph 可區分「驗證流程有沒有成功跑完」與「圖本身是否合法」
using System;

namespace OpsidanosInk.CanonicalGraph
{
    public readonly struct CanonicalGraphValidationResult
    {
        public readonly bool success;
        public readonly bool isValid;
        public readonly CanonicalGraphValidationIssue[] warnings;
        public readonly CanonicalGraphValidationIssue[] errors;

        private CanonicalGraphValidationResult(
            bool success,
            bool isValid,
            CanonicalGraphValidationIssue[] warnings,
            CanonicalGraphValidationIssue[] errors)
        {
            this.success = success;
            this.isValid = isValid;
            this.warnings = warnings ?? Array.Empty<CanonicalGraphValidationIssue>();
            this.errors = errors ?? Array.Empty<CanonicalGraphValidationIssue>();
        }

        public static CanonicalGraphValidationResult Completed(
            bool isValid,
            CanonicalGraphValidationIssue[] warnings,
            CanonicalGraphValidationIssue[] errors)
        {
            return new CanonicalGraphValidationResult(
                success: true,
                isValid: isValid,
                warnings: warnings,
                errors: errors);
        }

        public static CanonicalGraphValidationResult Failure(string code, string message)
        {
            CanonicalGraphValidationIssue error = new CanonicalGraphValidationIssue
            {
                code = code ?? string.Empty,
                message = message ?? string.Empty
            };

            return new CanonicalGraphValidationResult(
                success: false,
                isValid: false,
                warnings: Array.Empty<CanonicalGraphValidationIssue>(),
                errors: new[] { error });
        }
    }
}
// ===== 變更結束 =====
