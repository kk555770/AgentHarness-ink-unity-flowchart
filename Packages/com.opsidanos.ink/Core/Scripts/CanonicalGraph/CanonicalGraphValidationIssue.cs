// ===== 變更開始 =====
// 2026/03/19 Opsidanos (修改原因：開始落地 Batch 4 bridge kickoff，建立 graph 驗證與操作共用的 issue 形狀)
// 預期結果：warnings / errors 都有一致的結構，不再用零散字串回報問題
using System;

namespace OpsidanosInk.CanonicalGraph
{
    [Serializable]
    public sealed class CanonicalGraphValidationIssue
    {
        public string code = string.Empty;
        public string message = string.Empty;
        public string nodeId = string.Empty;
        public string edgeId = string.Empty;
        public string portName = string.Empty;
    }
}
// ===== 變更結束 =====
