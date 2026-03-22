// ===== 變更開始 =====
// 2026/03/22 Opsidanos (修改原因：為 Batch 9 補上 canonical graph 的最小 JSON snapshot 資料盒)
// 預期結果：GetGraph 可回傳固定且可序列化的 canonical snapshot，不必把 GraphToolkit 或 projection DTO 當讀取結果
using System;
using System.Collections.Generic;

namespace OpsidanosInk.CanonicalGraph
{
    [Serializable]
    public sealed class CanonicalGraphJsonGraphSnapshot
    {
        public string graphId = string.Empty;
        public string version = string.Empty;
        public string metadataJson = string.Empty;
        public List<CanonicalGraphNodeRecord> nodes = new List<CanonicalGraphNodeRecord>();
        public List<CanonicalGraphEdgeRecord> edges = new List<CanonicalGraphEdgeRecord>();
    }
}
// ===== 變更結束 =====
