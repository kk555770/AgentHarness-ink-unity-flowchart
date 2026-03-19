// ===== 變更開始 =====
// 2026/03/19 Opsidanos (修改原因：繼續薄化 Batch 3，將 choice / condition 分支節點從主節點檔抽成獨立檔)
// 預期結果：`InkFlowChartNodes.cs` 更聚焦主線節點；分支節點則集中在單一 branch node 檔案
using System;
using OpsidanosInk.CanonicalGraph;
using Unity.GraphToolkit.Editor;

namespace OpsidanosInk.Editor
{
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
    // ===== 變更結束 =====
}
// ===== 變更結束 =====
