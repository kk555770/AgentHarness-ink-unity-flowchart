// ===== 變更開始 =====
// 2026/03/18 Opsidanos (修改原因：把依賴 GraphToolkit 節點型別的 current projection 對照從 core naming seam 分離，避免純命名層又被 Editor 型別綁回去)
// 預期結果：core 只保管命名字串，GraphToolkit 端只負責把實際節點型別轉成 current projection node type
using OpsidanosInk.CanonicalGraph;
using Unity.GraphToolkit.Editor;

namespace OpsidanosInk.Editor
{
    public static class InkFlowCurrentProjectionAdapter
    {
        public static string GetCurrentProjectionNodeType(INode node)
        {
            if (node is InkFlowStartNode)
            {
                return CurrentFlowProjectionNaming.ToNodeType(CanonicalNodeKinds.Start);
            }

            if (node is InkFlowStageActionNode)
            {
                return CurrentFlowProjectionNaming.ToNodeType(CanonicalNodeKinds.StageAction);
            }

            if (node is InkFlowDialogueNode)
            {
                return CurrentFlowProjectionNaming.ToNodeType(CanonicalNodeKinds.Dialogue);
            }

            if (node is InkFlowCommentNode)
            {
                return CurrentFlowProjectionNaming.ToNodeType(CanonicalNodeKinds.Comment);
            }

            if (node is InkFlowChoiceNode)
            {
                return CurrentFlowProjectionNaming.ToNodeType(CanonicalNodeKinds.Choice);
            }

            if (node is InkFlowConditionNode)
            {
                return CurrentFlowProjectionNaming.ToNodeType(CanonicalNodeKinds.Condition);
            }

            return CurrentFlowProjectionNaming.UnknownNodeType;
        }
    }
}
// ===== 變更結束 =====
