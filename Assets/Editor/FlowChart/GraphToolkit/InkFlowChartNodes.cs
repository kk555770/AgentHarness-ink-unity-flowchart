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
        private const string OutputCountOptionName = "OutputCount";
        private const string ChoiceTextsOptionName = "ChoiceTexts";
        private const string ChoiceModeOptionName = "ChoiceMode";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<int>(OutputCountOptionName)
                .WithDisplayName("選項數量")
                .WithDefaultValue(2)
                .Delayed();

            context.AddOption<InkFlowChoiceMode>(ChoiceModeOptionName)
                .WithDisplayName("選項模式");

            context.AddOption<string>(ChoiceTextsOptionName)
                .WithDisplayName("選項文字（每行一個）")
                .WithDefaultValue("選項1\n選項2")
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddInputFlowPort(context);

            int outputCount = GetNodeOptionInt(OutputCountOptionName);
            string choiceTexts = GetNodeOptionString(ChoiceTextsOptionName);
            string[] choiceLines = SplitLines(choiceTexts);

            for (int i = 0; i < outputCount; i++)
            {
                string portName = BuildOutputPortName(i);
                string displayName = GetLineOrFallback(choiceLines, i, $"選項{i + 1}");
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
        private const string OutputCountOptionName = "OutputCount";
        private const string ConditionTextsOptionName = "ConditionTexts";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<int>(OutputCountOptionName)
                .WithDisplayName("分支數量（含否則）")
                .WithDefaultValue(2)
                .Delayed();

            context.AddOption<string>(ConditionTextsOptionName)
                .WithDisplayName("條件（每行一個；最後一個輸出埠為否則）")
                .WithDefaultValue("favor > 7")
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddInputFlowPort(context);

            int outputCount = GetNodeOptionInt(OutputCountOptionName);
            string conditionTexts = GetNodeOptionString(ConditionTextsOptionName);
            string[] conditionLines = SplitLines(conditionTexts);

            for (int i = 0; i < outputCount; i++)
            {
                string portName = BuildOutputPortName(i);
                string displayName = i == outputCount - 1
                    ? "否則"
                    : GetLineOrFallback(conditionLines, i, $"條件{i + 1}");
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
