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
    // 2026/02/22 Opsidanos (修改原因：集中管理節點顯示名稱與 option key，避免名稱字串散落各處造成維護困難)
    // 預期結果：Editor 顯示名稱與匯出匯入 key 可由同一處調整，後續改名不需多檔同步手改
    public static class InkFlowNodeSchema
    {
        // ===== 變更開始 =====
        // 2026/03/18 Opsidanos (修改原因：開始落地 Batch 1，把 node type 與 port naming 的真相來源改接到 core seam，讓 GraphToolkit schema 退成 adapter)
        // 預期結果：`InkFlowNodeSchema` 只保留 GraphToolkit option key 與顯示設定；canonical/current projection 命名改由 core 統一提供
        public const string DialogueContentOptionName = "Content";
        public const string DialogueContentOptionDisplayName = "對話內容";
        public const string DialogueActionInputCountOptionName = "ActionInputCount";
        public const string DialogueActionInputCountOptionDisplayName = "動作輸入數量";
        public const int DialogueActionInputCountDefaultValue = 1;

        public const string StageActionContentOptionName = "Content";
        public const string StageActionContentOptionDisplayName = "動作內容";

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
                string portName = CanonicalPortSemantics.BuildActionInputPortName(i);
                string displayName = CanonicalNodeDisplayNames.BuildDialogueActionInputDisplayName(i);
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
                string displayName = GetLineOrFallback(choiceLines, i, CanonicalNodeDisplayNames.GetChoicePortFallbackLabel(i + 1));
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
                    // ===== 變更開始 =====
                    // 2026/03/18 Opsidanos (修改原因：condition 的 else 顯示名稱改由 core seam 統一提供)
                    // 預期結果：GraphToolkit 不再自己保管 else 標籤字串，後續其他前端也能共用同一組命名
                    ? CanonicalNodeDisplayNames.ConditionElsePort
                    // ===== 變更結束 =====
                    : GetLineOrFallback(conditionLines, i, CanonicalNodeDisplayNames.GetConditionPortFallbackLabel(i + 1));
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
