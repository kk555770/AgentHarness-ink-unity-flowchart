// ===== 變更開始 =====
// 2026/02/06 Opsidanos (修改原因：建立 Graph Toolkit 版最小節點集合，先讓拉線與內容欄位可運作)
// 預期結果：開始/流程/註解節點可在 Graph Toolkit 視窗新增、拖曳與連線
using System;
using Unity.GraphToolkit.Editor;
// ===== 變更開始 =====
// 2026/02/23 Opsidanos (修改原因：ActionKind 下拉要顯示繁中名稱，需使用 InspectorName 標註)
// 預期結果：GraphToolkit 的 ActionKind 下拉顯示「對話/動作/自訂」，不再顯示英文 enum 成員名
using UnityEngine;
// ===== 變更結束 =====

namespace OpsidanosInk.Editor
{
    // ===== 變更開始 =====
    // 2026/02/22 Opsidanos (修改原因：集中管理節點顯示名稱與 option key，避免名稱字串散落各處造成維護困難)
    // 預期結果：Editor 顯示名稱與匯出匯入 key 可由同一處調整，後續改名不需多檔同步手改
    public static class InkFlowNodeSchema
    {
        public const string FlowPortName = "Flow";

        public const string NodeTypeStart = "start";
        // ===== 變更開始 =====
        // 2026/03/11 Opsidanos (修改原因：把 canonical dialogue 與 legacy sidecar action 拆成不同常數，避免 exporter/importer 各自散落命名判斷)
        // 預期結果：GraphToolkit 相關程式統一從這裡取得 canonical / current projection 的對話節點命名，不再混用 `dialogue` 與 `action`
        public const string CanonicalNodeTypeDialogue = "dialogue";
        public const string LegacySidecarDialogueNodeType = "action";
        public const string NodeTypeAction = LegacySidecarDialogueNodeType;
        // ===== 變更結束 =====
        public const string NodeTypeStageAction = "stageAction";
        public const string NodeTypeComment = "comment";
        public const string NodeTypeChoice = "choice";
        public const string NodeTypeCondition = "condition";

        public const string ActionKindOptionName = "ActionKind";
        public const string DialogueContentOptionName = "Content";
        public const string DialogueContentOptionDisplayName = "對話內容";
        public const string DialogueActionInputCountOptionName = "ActionInputCount";
        public const string DialogueActionInputCountOptionDisplayName = "動作輸入數量";
        public const int DialogueActionInputCountDefaultValue = 1;
        public const string DialogueActionInputPortNamePrefix = "ActionIn";
        public const string DialogueActionInputDisplayNamePrefix = "動作";

        public const string StageActionContentOptionName = "Content";
        public const string StageActionContentOptionDisplayName = "動作內容";
        public const string StageActionDataOutputPortName = "ActionData";
        public const string StageActionDataOutputPortDisplayName = "動作資料";

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

        // ===== 變更開始 =====
        // 2026/03/11 Opsidanos (修改原因：legacy sidecar 的 actionKind token 仍需相容，但名稱要明確標示它是 legacy mapping)
        // 預期結果：讀 code 時能一眼分辨 canonical node type 與 legacy actionKind token，不再誤會兩者是同一層命名
        private const string LegacyActionKindDialogueToken = "dialogue";
        private const string LegacyActionKindStageActionToken = "action";
        private const string LegacyActionKindCustomToken = "custom";
        // ===== 變更結束 =====

        public static string GetChoicePortFallbackLabel(int order)
        {
            return $"選項{order}";
        }

        public static string GetConditionPortFallbackLabel(int order)
        {
            return $"條件{order}";
        }

        public static string BuildDialogueActionInputPortName(int order)
        {
            return $"{DialogueActionInputPortNamePrefix}{order}";
        }

        public static string GetDialogueActionInputDisplayName(int order)
        {
            return $"{DialogueActionInputDisplayNamePrefix}{order + 1}";
        }

        public static int ParseDialogueActionInputOrder(string toPortName)
        {
            if (string.IsNullOrEmpty(toPortName))
            {
                return int.MaxValue;
            }

            if (!toPortName.StartsWith(DialogueActionInputPortNamePrefix, StringComparison.Ordinal))
            {
                return int.MaxValue;
            }

            string suffix = toPortName.Substring(DialogueActionInputPortNamePrefix.Length);
            if (int.TryParse(suffix, out int order) && order >= 0)
            {
                return order;
            }

            return int.MaxValue;
        }

        // ===== 變更開始 =====
        // 2026/03/11 Opsidanos (修改原因：集中提供 dialogue/action 的 canonical 與 legacy mapping helper，讓匯出匯入不再自己手寫判斷)
        // 預期結果：只要對話節點命名未來再收斂，最多只改這一處 helper，其餘 GraphToolkit 程式不需同步散改
        public static bool IsDialogueNodeType(string nodeType)
        {
            return string.Equals(nodeType, CanonicalNodeTypeDialogue, StringComparison.OrdinalIgnoreCase)
                || string.Equals(nodeType, LegacySidecarDialogueNodeType, StringComparison.OrdinalIgnoreCase);
        }

        public static string GetCurrentProjectionNodeType(INode node)
        {
            if (node is InkFlowStartNode)
            {
                return NodeTypeStart;
            }

            if (node is InkFlowStageActionNode)
            {
                return NodeTypeStageAction;
            }

            if (node is InkFlowDialogueNode)
            {
                return LegacySidecarDialogueNodeType;
            }

            if (node is InkFlowCommentNode)
            {
                return NodeTypeComment;
            }

            if (node is InkFlowChoiceNode)
            {
                return NodeTypeChoice;
            }

            if (node is InkFlowConditionNode)
            {
                return NodeTypeCondition;
            }

            return "unknown";
        }

        public static string ToLegacyActionKindToken(InkFlowActionKind actionKind)
        {
            switch (actionKind)
            {
                case InkFlowActionKind.StageAction:
                    return LegacyActionKindStageActionToken;
                case InkFlowActionKind.Custom:
                    return LegacyActionKindCustomToken;
                default:
                    return LegacyActionKindDialogueToken;
            }
        }

        public static InkFlowActionKind ParseLegacyActionKindToken(string token)
        {
            if (string.Equals(token, LegacyActionKindStageActionToken, StringComparison.OrdinalIgnoreCase))
            {
                return InkFlowActionKind.StageAction;
            }

            if (string.Equals(token, LegacyActionKindCustomToken, StringComparison.OrdinalIgnoreCase))
            {
                return InkFlowActionKind.Custom;
            }

            return InkFlowActionKind.Dialogue;
        }

        public static string ToActionKindToken(InkFlowActionKind actionKind)
        {
            return ToLegacyActionKindToken(actionKind);
        }

        public static InkFlowActionKind ParseActionKindToken(string token)
        {
            return ParseLegacyActionKindToken(token);
        }
        // ===== 變更結束 =====
    }
    // ===== 變更結束 =====

    // ===== 變更開始 =====
    // 2026/02/25 Opsidanos (修改原因：新增「資料連線」型別，讓動作節點與對話節點用型別線區分流程線)
    // 預期結果：GraphToolkit 會把資料連線視為 typed wire，與 Flow 線在視覺與連線規則上分離
    [Serializable]
    public struct InkFlowActionPayload
    {
    }
    // ===== 變更結束 =====

    // ===== 變更開始 =====
    // 2026/02/22 Opsidanos (修改原因：提供 Action 節點可下拉的內容類型，讓一般使用者不用先背格式)
    // 預期結果：Action 節點可直接從下拉選「對話 / 動作 / 自訂」，再填對應內容
    public enum InkFlowActionKind
    {
        // ===== 變更開始 =====
        // 2026/02/23 Opsidanos (修改原因：將下拉選單值改為繁中顯示，避免作者看到英文)
        // 預期結果：ActionKind 下拉顯示為「對話/動作/自訂」
        [InspectorName("對話")]
        Dialogue = 0,
        [InspectorName("動作")]
        StageAction = 1,
        [InspectorName("自訂")]
        Custom = 2
        // ===== 變更結束 =====
    }
    // ===== 變更結束 =====

    [Serializable]
    public abstract class InkFlowBaseNode : Node
    {
        protected const string FlowPortName = InkFlowNodeSchema.FlowPortName;
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
            context.AddOption<int>(InkFlowNodeSchema.DialogueActionInputCountOptionName)
                .WithDisplayName(InkFlowNodeSchema.DialogueActionInputCountOptionDisplayName)
                .WithDefaultValue(InkFlowNodeSchema.DialogueActionInputCountDefaultValue)
                .Delayed();
            // ===== 變更結束 =====

            context.AddOption<string>(InkFlowNodeSchema.DialogueContentOptionName)
                .WithDisplayName(InkFlowNodeSchema.DialogueContentOptionDisplayName);
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            // ===== 變更開始 =====
            // 2026/02/25 Opsidanos (修改原因：流程線與資料線分離；對話節點保留 Flow 進出，並新增 typed data input ports)
            // 預期結果：Flow 線只負責主流程，動作節點用資料線接到對話節點，不會誤接成流程分支
            AddInputFlowPort(context);
            AddOutputFlowPort(context);

            int actionInputCount = Mathf.Max(0, GetNodeOptionInt(InkFlowNodeSchema.DialogueActionInputCountOptionName));
            for (int i = 0; i < actionInputCount; i++)
            {
                string portName = InkFlowNodeSchema.BuildDialogueActionInputPortName(i);
                string displayName = InkFlowNodeSchema.GetDialogueActionInputDisplayName(i);
                context.AddInputPort<InkFlowActionPayload>(portName)
                    .WithDisplayName(displayName)
                    .WithConnectorUI(PortConnectorUI.Circle)
                    .Build();
            }
            // ===== 變更結束 =====
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
            context.AddOption<string>(InkFlowNodeSchema.StageActionContentOptionName)
                .WithDisplayName(InkFlowNodeSchema.StageActionContentOptionDisplayName);
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddOutputPort<InkFlowActionPayload>(InkFlowNodeSchema.StageActionDataOutputPortName)
                .WithDisplayName(InkFlowNodeSchema.StageActionDataOutputPortDisplayName)
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
    public class InkFlowChoiceNode : InkFlowBaseNode
    {
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
    public class InkFlowConditionNode : InkFlowBaseNode
    {
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
    // ===== 變更開始 =====
    // 2026/02/23 Opsidanos (修改原因：GraphToolkit 以類別名顯示節點標題，新增中文節點型別做為作者可見節點)
    // 預期結果：新增節點時直接顯示「開始/對話/註解/選項/條件」，避免再出現英文類別名稱
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

    // ===== 變更開始 =====
    // 2026/02/25 Opsidanos (修改原因：新增作者可見的「動作」節點，對應資料連線語意)
    // 預期結果：Graph 視圖可直接新增「動作」節點並用資料線接到「對話」節點
    [Serializable]
    [UseWithGraph(typeof(InkFlowChartGraph))]
    public sealed class 動作 : InkFlowStageActionNode
    {
    }
    // ===== 變更結束 =====

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
    // ===== 變更結束 =====
    // ===== 變更結束 =====
}
// ===== 變更結束 =====
