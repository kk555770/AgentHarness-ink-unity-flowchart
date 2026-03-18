// ===== 變更開始 =====
// 2026/03/18 Opsidanos (修改原因：開始落地 Batch 2 的 import plan seam，把 current projection DTO 匯入前需要的節點與連線規劃抽成 shared models)
// 預期結果：匯入端可先把 `.flowchart.json` 整理成一份不依賴 GraphToolkit 型別的施工圖，後續不管是 GraphToolkit 或 Web authoring bridge 都能重用
using System.Collections.Generic;

namespace OpsidanosInk.CanonicalGraph
{
    public sealed class CurrentFlowImportPlan
    {
        public string startNodeId { get; set; } = string.Empty;
        public List<CurrentFlowImportNodePlan> nodes { get; } = new List<CurrentFlowImportNodePlan>();
        public List<CurrentFlowImportWirePlan> wires { get; } = new List<CurrentFlowImportWirePlan>();
    }

    public sealed class CurrentFlowImportNodePlan
    {
        public string nodeId { get; set; } = string.Empty;
        public string nodeType { get; set; } = string.Empty;
        public string content { get; set; } = string.Empty;
        public int maxDialogueActionInputOrder { get; set; } = -1;
        public int choiceOutputCount { get; set; }
        public string choiceTexts { get; set; } = string.Empty;
        public string choiceModeToken { get; set; } = "*";
        public int conditionOutputCount { get; set; }
        public string conditionTexts { get; set; } = string.Empty;
    }

    public sealed class CurrentFlowImportWirePlan
    {
        public string fromNodeId { get; set; } = string.Empty;
        public string fromPortName { get; set; } = string.Empty;
        public string toNodeId { get; set; } = string.Empty;
        public string toPortName { get; set; } = string.Empty;
    }
}
// ===== 變更結束 =====
