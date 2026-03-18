// ===== 變更開始 =====
// 2026/03/18 Opsidanos (修改原因：把 current projection 的 legacy dialogue/action 命名抽成獨立 seam，避免 GraphToolkit 腳本繼續自己保管這組對照)
// 預期結果：canonical node kind 與 current projection token 的關係集中在 core，後續 exporter/importer/Web authoring 都能共用同一組 naming 規則
using System;

namespace OpsidanosInk.CanonicalGraph
{
    public static class CurrentFlowProjectionNaming
    {
        public const string DialogueNodeType = "action";
        public const string DialogueActionKindToken = "dialogue";
        public const string StageActionActionKindToken = "action";
        public const string CustomActionKindToken = "custom";
        public const string UnknownNodeType = "unknown";

        public static bool IsDialogueNodeType(string nodeType)
        {
            return string.Equals(nodeType, CanonicalNodeKinds.Dialogue, StringComparison.OrdinalIgnoreCase)
                || string.Equals(nodeType, DialogueNodeType, StringComparison.OrdinalIgnoreCase);
        }

        public static string ToNodeType(string canonicalNodeKind)
        {
            if (string.Equals(canonicalNodeKind, CanonicalNodeKinds.Dialogue, StringComparison.Ordinal))
            {
                return DialogueNodeType;
            }

            if (string.Equals(canonicalNodeKind, CanonicalNodeKinds.Start, StringComparison.Ordinal))
            {
                return CanonicalNodeKinds.Start;
            }

            if (string.Equals(canonicalNodeKind, CanonicalNodeKinds.StageAction, StringComparison.Ordinal))
            {
                return CanonicalNodeKinds.StageAction;
            }

            if (string.Equals(canonicalNodeKind, CanonicalNodeKinds.Comment, StringComparison.Ordinal))
            {
                return CanonicalNodeKinds.Comment;
            }

            if (string.Equals(canonicalNodeKind, CanonicalNodeKinds.Choice, StringComparison.Ordinal))
            {
                return CanonicalNodeKinds.Choice;
            }

            if (string.Equals(canonicalNodeKind, CanonicalNodeKinds.Condition, StringComparison.Ordinal))
            {
                return CanonicalNodeKinds.Condition;
            }

            return UnknownNodeType;
        }
    }
}
// ===== 變更結束 =====
