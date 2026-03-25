// ===== 變更開始 =====
// 2026/03/22 Opsidanos (修改原因：擴充 Batch 8/9 的 JSON command response 形狀，補上 GetGraph snapshot 回傳能力)
// 預期結果：command bridge 除了 `success / result / warnings / errors / applied` 外，也能在 GetGraph 時穩定回傳 canonical snapshot
using System;
using System.Collections.Generic;

namespace OpsidanosInk.CanonicalGraph
{
    [Serializable]
    public sealed class CanonicalGraphJsonResponse
    {
        public string contractVersion = CanonicalGraphJsonRequest.PlainJsonContractVersion;
        public bool success;
        public CanonicalGraphJsonResult result = new CanonicalGraphJsonResult();
        public CanonicalGraphJsonGraphSnapshot snapshot;
        public List<CanonicalGraphJsonIssue> warnings = new List<CanonicalGraphJsonIssue>();
        public List<CanonicalGraphJsonIssue> errors = new List<CanonicalGraphJsonIssue>();
        public bool applied;
    }

    [Serializable]
    public sealed class CanonicalGraphJsonResult
    {
        public string graphId = string.Empty;
        public string version = string.Empty;
        public string nodeId = string.Empty;
        public string edgeId = string.Empty;
        public bool isValid;
        // ===== 變更開始 =====
        // 2026/03/23 Opsidanos (修改原因：開始落地 Batch 11A，補上 projection-specific response 欄位)
        // 預期結果：ValidateProjection / ProjectGraph 除了 graph 狀態外，也能穩定回傳 target、projection version 與 target-native 產物
        public string target = string.Empty;
        public string projectionVersion = string.Empty;
        public string projectionText = string.Empty;
        public string projectionJson = string.Empty;
        // ===== 變更結束 =====
    }

    [Serializable]
    public sealed class CanonicalGraphJsonIssue
    {
        public string code = string.Empty;
        public string message = string.Empty;
        public CanonicalGraphJsonIssueDetails details = new CanonicalGraphJsonIssueDetails();
    }

    [Serializable]
    public sealed class CanonicalGraphJsonIssueDetails
    {
        public string graphId = string.Empty;
        public string nodeId = string.Empty;
        public string edgeId = string.Empty;
        public string portName = string.Empty;
    }
}
// ===== 變更結束 =====
