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
    // 2026/02/25 Opsidanos (修改原因：新增「資料連線」型別，讓動作節點與對話節點用型別線區分流程線)
    // 預期結果：GraphToolkit 會把資料連線視為 typed wire，與 Flow 線在視覺與連線規則上分離
    [Serializable]
    public struct InkFlowActionPayload
    {
    }
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

    // ===== 變更開始 =====
    // 2026/02/13 Opsidanos (修改原因：補齊「分岔節點」的正式語意：選項（玩家選）與條件（系統判斷），並允許調整輸出埠數量)
    // 預期結果：Flow Chart 可用專門節點承載多輸出線，後續匯出可對應 Ink 的 `*`/`+` 與 `{ ... }` 條件區塊，避免多行 `->` 死線
    public enum InkFlowChoiceMode
    {
        Once = 0,
        Repeatable = 1
    }

    [Serializable]
    public class InkFlowChoiceNode : InkFlowBaseNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<int>(InkFlowNodeOptionSchema.ChoiceOutputCountOptionName)
                .WithDisplayName(InkFlowNodeOptionSchema.ChoiceOutputCountOptionDisplayName)
                .WithDefaultValue(2)
                .Delayed();

            context.AddOption<InkFlowChoiceMode>(InkFlowNodeOptionSchema.ChoiceModeOptionName)
                .WithDisplayName(InkFlowNodeOptionSchema.ChoiceModeOptionDisplayName);

            context.AddOption<string>(InkFlowNodeOptionSchema.ChoiceTextsOptionName)
                .WithDisplayName(InkFlowNodeOptionSchema.ChoiceTextsOptionDisplayName)
                .WithDefaultValue(InkFlowNodeOptionSchema.ChoiceTextsDefaultValue)
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddInputFlowPort(context);

            // ===== 變更開始 =====
            // 2026/03/18 Opsidanos (修改原因：開始落地 Batch 3 下一刀，把 choice 的 option / branch label 小工具抽成共用 node shell utility)
            // 預期結果：choice 節點的輸出埠命名與顯示規則維持不變，但節點檔不再自己保存重複 helper
            int outputCount = InkFlowChartNodeShellUtility.GetNodeOptionInt(this, InkFlowNodeOptionSchema.ChoiceOutputCountOptionName);
            string choiceTexts = InkFlowChartNodeShellUtility.GetNodeOptionString(this, InkFlowNodeOptionSchema.ChoiceTextsOptionName);
            string[] choiceLines = InkFlowChartNodeShellUtility.SplitLines(choiceTexts);
            // ===== 變更結束 =====

            for (int i = 0; i < outputCount; i++)
            {
                string portName = InkFlowChartNodeShellUtility.BuildOutputPortName(i);
                string displayName = InkFlowChartNodeShellUtility.GetLineOrFallback(choiceLines, i, CanonicalNodeDisplayNames.GetChoicePortFallbackLabel(i + 1));
                context.AddOutputPort(portName)
                    .WithDisplayName(displayName)
                    .WithConnectorUI(PortConnectorUI.Arrowhead)
                    .Build();
            }
        }
    }

    [Serializable]
    public class InkFlowConditionNode : InkFlowBaseNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<int>(InkFlowNodeOptionSchema.ConditionOutputCountOptionName)
                .WithDisplayName(InkFlowNodeOptionSchema.ConditionOutputCountOptionDisplayName)
                .WithDefaultValue(2)
                .Delayed();

            context.AddOption<string>(InkFlowNodeOptionSchema.ConditionTextsOptionName)
                .WithDisplayName(InkFlowNodeOptionSchema.ConditionTextsOptionDisplayName)
                .WithDefaultValue(InkFlowNodeOptionSchema.ConditionTextsDefaultValue)
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddInputFlowPort(context);

            // ===== 變更開始 =====
            // 2026/03/18 Opsidanos (修改原因：開始落地 Batch 3 下一刀，把 condition 的 option / branch label 小工具抽成共用 node shell utility)
            // 預期結果：condition 節點的輸出埠命名與顯示規則維持不變，但節點檔不再自己保存重複 helper
            int outputCount = InkFlowChartNodeShellUtility.GetNodeOptionInt(this, InkFlowNodeOptionSchema.ConditionOutputCountOptionName);
            string conditionTexts = InkFlowChartNodeShellUtility.GetNodeOptionString(this, InkFlowNodeOptionSchema.ConditionTextsOptionName);
            string[] conditionLines = InkFlowChartNodeShellUtility.SplitLines(conditionTexts);
            // ===== 變更結束 =====

            for (int i = 0; i < outputCount; i++)
            {
                string portName = InkFlowChartNodeShellUtility.BuildOutputPortName(i);
                string displayName = i == outputCount - 1
                    // ===== 變更開始 =====
                    // 2026/03/18 Opsidanos (修改原因：condition 的 else 顯示名稱改由 core seam 統一提供)
                    // 預期結果：GraphToolkit 不再自己保管 else 標籤字串，後續其他前端也能共用同一組命名
                    ? CanonicalNodeDisplayNames.ConditionElsePort
                    // ===== 變更結束 =====
                    : InkFlowChartNodeShellUtility.GetLineOrFallback(conditionLines, i, CanonicalNodeDisplayNames.GetConditionPortFallbackLabel(i + 1));
                context.AddOutputPort(portName)
                    .WithDisplayName(displayName)
                    .WithConnectorUI(PortConnectorUI.Arrowhead)
                    .Build();
            }
        }
    }
}
// ===== 變更結束 =====
