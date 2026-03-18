// ===== 變更開始 =====
// 2026/03/18 Opsidanos (修改原因：開始落地 Batch 3，將 GraphToolkit shell 的圖層驗證入口從 InkFlowChartGraph 抽成獨立 helper)
// 預期結果：InkFlowChartGraph 只保留最薄的 OnGraphChanged 入口；shell 層的 start node 規則橋接則集中在單一 validator helper
using System.Linq;
using Unity.GraphToolkit.Editor;

namespace OpsidanosInk.Editor
{
    public static class InkFlowChartGraphShellValidator
    {
        public static void Validate(InkFlowChartGraph graph, GraphLogger graphLogger)
        {
            int startNodeCount = graph.GetNodes().Count(node => node is InkFlowStartNode);
            if (startNodeCount == 0)
            {
                graphLogger.LogError("Flow Chart 需要 1 個開始節點。", graph);
                return;
            }

            if (startNodeCount > 1)
            {
                graphLogger.LogError("Flow Chart 只允許 1 個開始節點。", graph);
            }
        }
    }
}
// ===== 變更結束 =====
