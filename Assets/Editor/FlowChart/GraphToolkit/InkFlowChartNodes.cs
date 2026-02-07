// ===== 變更開始 =====
// 2026/02/06 Opsidanos (修改原因：建立 Graph Toolkit 版最小節點集合，先讓拉線與內容欄位可運作)
// 預期結果：開始/流程/註解節點可在 Graph Toolkit 視窗新增、拖曳與連線
using System;
using Unity.GraphToolkit.Editor;

namespace OpsidanosInk.Editor
{
    [Serializable]
    public abstract class InkFlowBaseNode : Node
    {
        protected const string FlowPortName = "Flow";

        protected void AddInputFlowPort(IPortDefinitionContext context)
        {
            context.AddInputPort(FlowPortName)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }

        protected void AddOutputFlowPort(IPortDefinitionContext context)
        {
            context.AddOutputPort(FlowPortName)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }

    [Serializable]
    [UseWithGraph(typeof(InkFlowChartGraph))]
    public sealed class InkFlowStartNode : InkFlowBaseNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddOutputFlowPort(context);
        }
    }

    [Serializable]
    [UseWithGraph(typeof(InkFlowChartGraph))]
    public sealed class InkFlowActionNode : InkFlowBaseNode
    {
        private const string ContentOptionName = "Content";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(ContentOptionName)
                .WithDisplayName("內容");
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddInputFlowPort(context);
            AddOutputFlowPort(context);
        }
    }

    [Serializable]
    [UseWithGraph(typeof(InkFlowChartGraph))]
    public sealed class InkFlowCommentNode : InkFlowBaseNode
    {
        private const string NoteOptionName = "Note";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(NoteOptionName)
                .WithDisplayName("註解");
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddInputFlowPort(context);
            AddOutputFlowPort(context);
        }
    }
}
// ===== 變更結束 =====
