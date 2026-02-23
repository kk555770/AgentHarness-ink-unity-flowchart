// ===== 變更開始 =====
// 2026/02/06 Opsidanos (修改原因：建立 Graph Toolkit 版最小節點集合，先讓拉線與內容欄位可運作)
// 預期結果：開始/流程/註解節點可在 Graph Toolkit 視窗新增、拖曳與連線
using System;
using System.Reflection;
using Unity.GraphToolkit.Editor;

namespace OpsidanosInk.Editor
{
    // ===== 變更開始 =====
    // 2026/02/22 Opsidanos (修改原因：集中管理節點顯示名稱與 option key，避免名稱字串散落各處造成維護困難)
    // 預期結果：Editor 顯示名稱與匯出匯入 key 可由同一處調整，後續改名不需多檔同步手改
    public static class InkFlowNodeSchema
    {
        public const string FlowPortName = "Flow";

        public const string NodeTypeStart = "start";
        public const string NodeTypeAction = "action";
        public const string NodeTypeComment = "comment";
        public const string NodeTypeChoice = "choice";
        public const string NodeTypeCondition = "condition";

        public const string StartNodeTitle = "開始";
        public const string ActionNodeTitle = "對話";
        public const string CommentNodeTitle = "註解";
        public const string ChoiceNodeTitle = "選項";
        public const string ConditionNodeTitle = "條件";

        public const string ActionKindOptionName = "ActionKind";
        public const string ActionContentOptionName = "Content";
        public const string ActionKindOptionDisplayName = "內容類型";
        public const string ActionContentOptionDisplayName = "內容";

        public const string CommentNoteOptionName = "Note";
        public const string CommentNoteOptionDisplayName = "註解";

        public const string ChoiceOutputCountOptionName = "OutputCount";
        public const string ChoiceTextsOptionName = "ChoiceTexts";
        public const string ChoiceModeOptionName = "ChoiceMode";
        public const string ChoiceOutputCountOptionDisplayName = "選項數量";
        public const string ChoiceModeOptionDisplayName = "選項模式";
        public const string ChoiceTextsOptionDisplayName = "選項文字（每行一個）";
        public const string ChoiceTextsDefaultValue = "選項1\n選項2";

        public const string ConditionOutputCountOptionName = "OutputCount";
        public const string ConditionTextsOptionName = "ConditionTexts";
        public const string ConditionOutputCountOptionDisplayName = "分支數量（含否則）";
        public const string ConditionTextsOptionDisplayName = "條件（每行一個；最後一個輸出埠為否則）";
        public const string ConditionTextsDefaultValue = "favor > 7";
        public const string ConditionElsePortDisplayName = "否則";

        private const string ActionKindDialogueToken = "dialogue";
        private const string ActionKindStageActionToken = "action";
        private const string ActionKindCustomToken = "custom";

        public static string GetChoicePortFallbackLabel(int order)
        {
            return $"選項{order}";
        }

        public static string GetConditionPortFallbackLabel(int order)
        {
            return $"條件{order}";
        }

        public static string ToActionKindToken(InkFlowActionKind actionKind)
        {
            switch (actionKind)
            {
                case InkFlowActionKind.StageAction:
                    return ActionKindStageActionToken;
                case InkFlowActionKind.Custom:
                    return ActionKindCustomToken;
                default:
                    return ActionKindDialogueToken;
            }
        }

        public static InkFlowActionKind ParseActionKindToken(string token)
        {
            if (string.Equals(token, ActionKindStageActionToken, StringComparison.OrdinalIgnoreCase))
            {
                return InkFlowActionKind.StageAction;
            }

            if (string.Equals(token, ActionKindCustomToken, StringComparison.OrdinalIgnoreCase))
            {
                return InkFlowActionKind.Custom;
            }

            return InkFlowActionKind.Dialogue;
        }
    }
    // ===== 變更結束 =====

    // ===== 變更開始 =====
    // 2026/02/22 Opsidanos (修改原因：提供 Action 節點可下拉的內容類型，讓一般使用者不用先背格式)
    // 預期結果：Action 節點可直接從下拉選「對話 / 動作 / 自訂」，再填對應內容
    public enum InkFlowActionKind
    {
        Dialogue = 0,
        StageAction = 1,
        Custom = 2
    }
    // ===== 變更結束 =====

    [Serializable]
    public abstract class InkFlowBaseNode : Node
    {
        protected const string FlowPortName = InkFlowNodeSchema.FlowPortName;
        private static readonly FieldInfo NodeImplementationField = typeof(Node).GetField("m_Implementation", BindingFlags.Instance | BindingFlags.NonPublic);

        protected abstract string NodeTitle { get; }

        public override void OnEnable()
        {
            base.OnEnable();
            ApplyNodeTitle();
        }

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

        // ===== 變更開始 =====
        // 2026/02/22 Opsidanos (修改原因：節點 class 名稱是英文，作者在圖上會看到難懂標題；改由集中設定覆寫節點標題)
        // 預期結果：節點標題顯示為白話繁中（開始/對話/註解/選項/條件），且可在同一個設定類別調整
        private void ApplyNodeTitle()
        {
            string nodeTitle = NodeTitle;
            if (string.IsNullOrWhiteSpace(nodeTitle))
            {
                return;
            }

            object nodeImplementation = NodeImplementationField?.GetValue(this);
            if (nodeImplementation == null)
            {
                return;
            }

            PropertyInfo titleProperty = nodeImplementation
                .GetType()
                .GetProperty("Title", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (titleProperty == null || !titleProperty.CanWrite)
            {
                return;
            }

            string currentTitle = titleProperty.GetValue(nodeImplementation) as string;
            if (string.Equals(currentTitle, nodeTitle, StringComparison.Ordinal))
            {
                return;
            }

            titleProperty.SetValue(nodeImplementation, nodeTitle);
        }
        // ===== 變更結束 =====
    }

    [Serializable]
    [UseWithGraph(typeof(InkFlowChartGraph))]
    public sealed class InkFlowStartNode : InkFlowBaseNode
    {
        protected override string NodeTitle => InkFlowNodeSchema.StartNodeTitle;

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddOutputFlowPort(context);
        }
    }

    [Serializable]
    [UseWithGraph(typeof(InkFlowChartGraph))]
    public sealed class InkFlowActionNode : InkFlowBaseNode
    {
        protected override string NodeTitle => InkFlowNodeSchema.ActionNodeTitle;

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            // ===== 變更開始 =====
            // 2026/02/22 Opsidanos (修改原因：先選內容類型再填內容，降低手動輸入失誤)
            // 預期結果：Action 節點可透過下拉快速切換「對話 / 動作 / 自訂」
            context.AddOption<InkFlowActionKind>(InkFlowNodeSchema.ActionKindOptionName)
                .WithDisplayName(InkFlowNodeSchema.ActionKindOptionDisplayName)
                .WithDefaultValue(InkFlowActionKind.Dialogue);
            // ===== 變更結束 =====

            context.AddOption<string>(InkFlowNodeSchema.ActionContentOptionName)
                .WithDisplayName(InkFlowNodeSchema.ActionContentOptionDisplayName);
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
        protected override string NodeTitle => InkFlowNodeSchema.CommentNodeTitle;

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(InkFlowNodeSchema.CommentNoteOptionName)
                .WithDisplayName(InkFlowNodeSchema.CommentNoteOptionDisplayName);
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
    [UseWithGraph(typeof(InkFlowChartGraph))]
    public sealed class InkFlowChoiceNode : InkFlowBaseNode
    {
        protected override string NodeTitle => InkFlowNodeSchema.ChoiceNodeTitle;

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<int>(InkFlowNodeSchema.ChoiceOutputCountOptionName)
                .WithDisplayName(InkFlowNodeSchema.ChoiceOutputCountOptionDisplayName)
                .WithDefaultValue(2)
                .Delayed();

            context.AddOption<InkFlowChoiceMode>(InkFlowNodeSchema.ChoiceModeOptionName)
                .WithDisplayName(InkFlowNodeSchema.ChoiceModeOptionDisplayName);

            context.AddOption<string>(InkFlowNodeSchema.ChoiceTextsOptionName)
                .WithDisplayName(InkFlowNodeSchema.ChoiceTextsOptionDisplayName)
                .WithDefaultValue(InkFlowNodeSchema.ChoiceTextsDefaultValue)
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddInputFlowPort(context);

            int outputCount = GetNodeOptionInt(InkFlowNodeSchema.ChoiceOutputCountOptionName);
            string choiceTexts = GetNodeOptionString(InkFlowNodeSchema.ChoiceTextsOptionName);
            string[] choiceLines = SplitLines(choiceTexts);

            for (int i = 0; i < outputCount; i++)
            {
                string portName = BuildOutputPortName(i);
                string displayName = GetLineOrFallback(choiceLines, i, InkFlowNodeSchema.GetChoicePortFallbackLabel(i + 1));
                context.AddOutputPort(portName)
                    .WithDisplayName(displayName)
                    .WithConnectorUI(PortConnectorUI.Arrowhead)
                    .Build();
            }
        }

        private static string BuildOutputPortName(int index)
        {
            return $"Out{index}";
        }

        private string GetNodeOptionString(string optionName)
        {
            INodeOption option = GetNodeOptionByName(optionName);
            if (option == null)
            {
                return string.Empty;
            }

            if (option.TryGetValue(out string value))
            {
                return value ?? string.Empty;
            }

            return string.Empty;
        }

        private int GetNodeOptionInt(string optionName)
        {
            INodeOption option = GetNodeOptionByName(optionName);
            if (option == null)
            {
                return 0;
            }

            if (option.TryGetValue(out int value))
            {
                return value;
            }

            return 0;
        }

        private static string[] SplitLines(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Array.Empty<string>();
            }

            return text
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Split('\n');
        }

        private static string GetLineOrFallback(string[] lines, int index, string fallback)
        {
            if (lines != null && index >= 0 && index < lines.Length)
            {
                string value = lines[index];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return fallback;
        }
    }

    [Serializable]
    [UseWithGraph(typeof(InkFlowChartGraph))]
    public sealed class InkFlowConditionNode : InkFlowBaseNode
    {
        protected override string NodeTitle => InkFlowNodeSchema.ConditionNodeTitle;

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<int>(InkFlowNodeSchema.ConditionOutputCountOptionName)
                .WithDisplayName(InkFlowNodeSchema.ConditionOutputCountOptionDisplayName)
                .WithDefaultValue(2)
                .Delayed();

            context.AddOption<string>(InkFlowNodeSchema.ConditionTextsOptionName)
                .WithDisplayName(InkFlowNodeSchema.ConditionTextsOptionDisplayName)
                .WithDefaultValue(InkFlowNodeSchema.ConditionTextsDefaultValue)
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddInputFlowPort(context);

            int outputCount = GetNodeOptionInt(InkFlowNodeSchema.ConditionOutputCountOptionName);
            string conditionTexts = GetNodeOptionString(InkFlowNodeSchema.ConditionTextsOptionName);
            string[] conditionLines = SplitLines(conditionTexts);

            for (int i = 0; i < outputCount; i++)
            {
                string portName = BuildOutputPortName(i);
                string displayName = i == outputCount - 1
                    ? InkFlowNodeSchema.ConditionElsePortDisplayName
                    : GetLineOrFallback(conditionLines, i, InkFlowNodeSchema.GetConditionPortFallbackLabel(i + 1));
                context.AddOutputPort(portName)
                    .WithDisplayName(displayName)
                    .WithConnectorUI(PortConnectorUI.Arrowhead)
                    .Build();
            }
        }

        private static string BuildOutputPortName(int index)
        {
            return $"Out{index}";
        }

        private string GetNodeOptionString(string optionName)
        {
            INodeOption option = GetNodeOptionByName(optionName);
            if (option == null)
            {
                return string.Empty;
            }

            if (option.TryGetValue(out string value))
            {
                return value ?? string.Empty;
            }

            return string.Empty;
        }

        private int GetNodeOptionInt(string optionName)
        {
            INodeOption option = GetNodeOptionByName(optionName);
            if (option == null)
            {
                return 0;
            }

            if (option.TryGetValue(out int value))
            {
                return value;
            }

            return 0;
        }

        private static string[] SplitLines(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Array.Empty<string>();
            }

            return text
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Split('\n');
        }

        private static string GetLineOrFallback(string[] lines, int index, string fallback)
        {
            if (lines != null && index >= 0 && index < lines.Length)
            {
                string value = lines[index];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return fallback;
        }
    }
    // ===== 變更結束 =====
}
// ===== 變更結束 =====
