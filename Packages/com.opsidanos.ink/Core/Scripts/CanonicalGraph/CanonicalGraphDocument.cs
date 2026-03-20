// ===== 變更開始 =====
// 2026/03/19 Opsidanos (修改原因：開始落地 Batch 4 bridge kickoff，建立 canonical graph 的最小 document 骨架)
// 預期結果：repo 第一次有可被 command service 與未來 bridge 共用的 graph document 實體
using System;
using System.Collections.Generic;

namespace OpsidanosInk.CanonicalGraph
{
    [Serializable]
    public sealed class CanonicalGraphDocument
    {
        public string graphId = string.Empty;
        public string version = string.Empty;
        public string metadataJson = string.Empty;
        public List<CanonicalGraphNodeRecord> nodes = new List<CanonicalGraphNodeRecord>();
        public List<CanonicalGraphEdgeRecord> edges = new List<CanonicalGraphEdgeRecord>();
    }
}
// ===== 變更結束 =====
