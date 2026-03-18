// ===== 變更開始 =====
// 2026/03/18 Opsidanos (修改原因：開始落地 Batch 3 下一刀，將 graph-specific 的可見節點註冊從 InkFlowChartNodes 抽成獨立檔)
// 預期結果：InkFlowChartNodes 只保留節點本體；哪些節點要綁到 InkFlowChartGraph 顯示，則集中在單一 graph registration 檔
using System;
using Unity.GraphToolkit.Editor;

namespace OpsidanosInk.Editor
{
    [Serializable]
    [UseWithGraph(typeof(InkFlowChartGraph))]
    public sealed class 開始 : InkFlowStartNode
    {
    }

    [Serializable]
    [UseWithGraph(typeof(InkFlowChartGraph))]
    public sealed class 對話 : InkFlowActionNode
    {
    }

    [Serializable]
    [UseWithGraph(typeof(InkFlowChartGraph))]
    public sealed class 動作 : InkFlowStageActionNode
    {
    }

    [Serializable]
    [UseWithGraph(typeof(InkFlowChartGraph))]
    public sealed class 註解 : InkFlowCommentNode
    {
    }

    [Serializable]
    [UseWithGraph(typeof(InkFlowChartGraph))]
    public sealed class 選項 : InkFlowChoiceNode
    {
    }

    [Serializable]
    [UseWithGraph(typeof(InkFlowChartGraph))]
    public sealed class 條件 : InkFlowConditionNode
    {
    }
}
// ===== 變更結束 =====
