// ===== 變更開始 =====
// 2026/03/22 Opsidanos (修改原因：開始落地 Batch 8，建立 canonical graph 的最小 JSON command request 形狀)
// 預期結果：AI、程式與未來 authoring bridge 可用固定 JSON envelope 呼叫 `CreateGraph / CreateNode / ConnectPorts / ValidateGraph`
using System;
using System.Collections.Generic;

namespace OpsidanosInk.CanonicalGraph
{
    [Serializable]
    public sealed class CanonicalGraphJsonRequest
    {
        public const string PlainJsonContractVersion = "plain-json-1";

        public string contractVersion = PlainJsonContractVersion;
        public string operation = string.Empty;
        public CanonicalGraphJsonRequestInput input = new CanonicalGraphJsonRequestInput();
    }

    [Serializable]
    public sealed class CanonicalGraphJsonRequestInput
    {
        public string graphId = string.Empty;
        public string version = string.Empty;
        public CanonicalGraphJsonMetadataInput metadata = new CanonicalGraphJsonMetadataInput();
        // ===== 變更開始 =====
        // 2026/03/22 Opsidanos (修改原因：開始落地 Batch 10，補上 mutation bridge 需要的 node / edge selector 欄位)
        // 預期結果：`ReplaceNodePayload / RemoveNode / DisconnectEdge` 可用固定且簡單的 JSON shape 指向目標 node 或 edge
        public string nodeId = string.Empty;
        public string edgeId = string.Empty;
        // ===== 變更結束 =====
        // ===== 變更開始 =====
        // 2026/03/23 Opsidanos (修改原因：開始落地 Batch 11A，補上 projection-specific control plane 需要的 target 與 version 欄位)
        // 預期結果：`ValidateProjection / ProjectGraph` 可用固定 JSON shape 指定投影目標與 projection version
        public string target = string.Empty;
        public string projectionVersion = string.Empty;
        // ===== 變更結束 =====
        // ===== 變更開始 =====
        // 2026/03/25 Opsidanos (修改原因：補上 ImportProjection 的輸入載體，讓 control plane 可直接吃 projection 文字或 JSON)
        // 預期結果：`ImportProjection` 可用與 projection response 對稱的固定 JSON shape 接收 `projectionText / projectionJson`
        public string projectionText = string.Empty;
        public string projectionJson = string.Empty;
        // ===== 變更結束 =====
        public CanonicalGraphJsonNodeInput node = new CanonicalGraphJsonNodeInput();
        public string fromNodeId = string.Empty;
        public string fromPort = string.Empty;
        public string toNodeId = string.Empty;
        public string toPort = string.Empty;
    }

    [Serializable]
    public sealed class CanonicalGraphJsonMetadataInput
    {
        public string graphName = string.Empty;
        public string projectionVersion = string.Empty;
        public string rawJson = string.Empty;
    }

    [Serializable]
    public sealed class CanonicalGraphJsonNodeInput
    {
        public string nodeId = string.Empty;
        public string nodeType = string.Empty;
        public CanonicalGraphJsonNodePayload payload = new CanonicalGraphJsonNodePayload();
        public int dialogueActionInputCount;
        public int branchCount;
        public string branchModeToken = string.Empty;
    }

    [Serializable]
    public sealed class CanonicalGraphJsonNodePayload
    {
        public string content = string.Empty;
        public List<string> labels = new List<string>();
        public List<string> conditions = new List<string>();
    }
}
// ===== 變更結束 =====
