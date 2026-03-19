// ===== 變更開始 =====
// 2026/02/06 Opsidanos (修改原因：建立 Graph Toolkit 版最小節點集合，先讓拉線與內容欄位可運作)
// 預期結果：開始/流程/註解節點可在 Graph Toolkit 視窗新增、拖曳與連線
using System;
using OpsidanosInk.CanonicalGraph;
using Unity.GraphToolkit.Editor;
// ===== 變更開始 =====
// 2026/02/23 Opsidanos (修改原因：ActionKind 下拉要顯示繁中名稱，需使用 InspectorName 標註)
// 預期結果：GraphToolkit 的 ActionKind 下拉顯示「對話/動作/自訂」，不再顯示英文 enum 成員名
using UnityEngine;
// ===== 變更結束 =====

namespace OpsidanosInk.Editor
{
    // ===== 變更開始 =====
    // 2026/03/18 Opsidanos (修改原因：把 GraphToolkit option schema 抽到獨立檔，讓節點檔只保留 node 殼與局部 helper)
    // 預期結果：`InkFlowChartNodes.cs` 不再同時承擔 option 常數定義，閱讀時更容易聚焦節點本體
    // `InkFlowNodeOptionSchema` 已移到獨立檔案。
    // ===== 變更結束 =====

    // ===== 變更開始 =====
    // 2026/03/18 Opsidanos (修改原因：開始落地 Batch 3 下一刀，將 graph-specific 可見節點註冊與 node shell 小工具從節點檔抽離)
    // 預期結果：`InkFlowChartNodes.cs` 更專注在節點本體；graph 綁定與 helper 會移到獨立檔案
    // `InkFlowChartVisibleNodes` 與 `InkFlowChartNodeShellUtility` 已移到獨立檔案。
    // ===== 變更結束 =====

    // ===== 變更開始 =====
    // 2026/03/19 Opsidanos (修改原因：繼續薄化 Batch 3，將動作資料 payload 與 branch node 抽到獨立檔)
    // 預期結果：`InkFlowChartNodes.cs` 聚焦主線節點本體；支援型別與分支節點改由獨立檔維護
    // `InkFlowActionPayload` 已移到 `InkFlowActionPayload.cs`。
    // `InkFlowChoiceMode`、`InkFlowChoiceNode`、`InkFlowConditionNode` 已移到 `InkFlowChartBranchNodes.cs`。
    // ===== 變更結束 =====

    [Serializable]
    public abstract class InkFlowBaseNode : Node
    {
        protected const string FlowPortName = CanonicalPortSemantics.Flow;
        // ===== 變更開始 =====
        // 2026/02/23 Opsidanos (修改原因：GraphToolkit 節點標題來源是類別名稱，移除無效的反射覆寫 Title)
        // 預期結果：節點標題由節點類別名稱穩定決定，避免繼續誤以為可用反射改標題
        // ===== 變更結束 =====

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
    public class InkFlowStartNode : InkFlowBaseNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddOutputFlowPort(context);
        }
    }

    [Serializable]
    public class InkFlowDialogueNode : InkFlowBaseNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            // ===== 變更開始 =====
            // 2026/02/25 Opsidanos (修改原因：對話節點需可接多個動作資料輸入，讓「同一句多角色同時動作」可視覺化)
            // 預期結果：作者可用下拉調整動作輸入埠數量，不再把多角色動作塞進同一段文字
            context.AddOption<int>(InkFlowNodeOptionSchema.DialogueActionInputCountOptionName)
                .WithDisplayName(InkFlowNodeOptionSchema.DialogueActionInputCountOptionDisplayName)
                .WithDefaultValue(InkFlowNodeOptionSchema.DialogueActionInputCountDefaultValue)
                .Delayed();
            // ===== 變更結束 =====

            context.AddOption<string>(InkFlowNodeOptionSchema.DialogueContentOptionName)
                .WithDisplayName(InkFlowNodeOptionSchema.DialogueContentOptionDisplayName);
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            // ===== 變更開始 =====
            // 2026/02/25 Opsidanos (修改原因：流程線與資料線分離；對話節點保留 Flow 進出，並新增 typed data input ports)
            // 預期結果：Flow 線只負責主流程，動作節點用資料線接到對話節點，不會誤接成流程分支
            AddInputFlowPort(context);
            AddOutputFlowPort(context);
            // ===== 變更開始 =====
            // 2026/03/18 Opsidanos (修改原因：開始落地 Batch 3 下一刀，把 option 讀值 helper 從節點檔抽成共用 node shell utility)
            // 預期結果：對話節點仍可依 option 建立動作輸入埠，但 `InkFlowChartNodes.cs` 不再自己保存重複的 option 讀值方法
            int actionInputCount = Mathf.Max(0, InkFlowChartNodeShellUtility.GetNodeOptionInt(this, InkFlowNodeOptionSchema.DialogueActionInputCountOptionName));
            // ===== 變更結束 =====
            for (int i = 0; i < actionInputCount; i++)
            {
                string portName = CanonicalPortSemantics.BuildActionInputPortName(i);
                string displayName = CanonicalNodeDisplayNames.BuildDialogueActionInputDisplayName(i);
                context.AddInputPort<InkFlowActionPayload>(portName)
                    .WithDisplayName(displayName)
                    .WithConnectorUI(PortConnectorUI.Circle)
                    .Build();
            }
            // ===== 變更結束 =====
        }
    }

    // ===== 變更開始 =====
    // 2026/02/25 Opsidanos (修改原因：保留舊類別名稱相容，避免既有測試/流程在型別判斷時直接中斷)
    // 預期結果：舊程式碼引用 InkFlowActionNode 仍可運作，語意上對應到「對話節點」
    [Serializable]
    public class InkFlowActionNode : InkFlowDialogueNode
    {
    }
    // ===== 變更結束 =====

    // ===== 變更開始 =====
    // 2026/02/25 Opsidanos (修改原因：新增動作節點，專門輸出動作資料到對話節點，不再混在同一節點)
    // 預期結果：動作節點只走資料輸出線，作者可把多角色動作分開管理
    [Serializable]
    public class InkFlowStageActionNode : InkFlowBaseNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(InkFlowNodeOptionSchema.StageActionContentOptionName)
                .WithDisplayName(InkFlowNodeOptionSchema.StageActionContentOptionDisplayName);
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddOutputPort<InkFlowActionPayload>(CanonicalPortSemantics.ActionData)
                // ===== 變更開始 =====
                // 2026/03/18 Opsidanos (修改原因：動作資料輸出埠顯示名稱改由 core seam 提供，避免 GraphToolkit 自己保管顯示語意)
                // 預期結果：GraphToolkit 與未來作者工具可共用同一個「動作資料」顯示名稱來源
                .WithDisplayName(CanonicalNodeDisplayNames.StageActionDataPort)
                // ===== 變更結束 =====
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
        }
    }
    // ===== 變更結束 =====

    [Serializable]
    public class InkFlowCommentNode : InkFlowBaseNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(InkFlowNodeOptionSchema.CommentNoteOptionName)
                .WithDisplayName(InkFlowNodeOptionSchema.CommentNoteOptionDisplayName);
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddInputFlowPort(context);
            AddOutputFlowPort(context);
        }
    }

}
// ===== 變更結束 =====
