// ===== 變更開始 =====
// 2026/03/19 Opsidanos (修改原因：開始落地 Batch 4 bridge kickoff，建立 canonical graph 的最小 node record 資料盒)
// 預期結果：後續 command service、JSON contract 與 Web-first bridge 有固定的 node 形狀可共用
using System;

namespace OpsidanosInk.CanonicalGraph
{
    [Serializable]
    public sealed class CanonicalGraphNodeRecord
    {
        public string nodeId = string.Empty;
        public string nodeType = string.Empty;
        public string payloadJson = string.Empty;
        public int dialogueActionInputCount;
        public int branchCount;
        public string branchModeToken = string.Empty;
    }
}
// ===== 變更結束 =====
