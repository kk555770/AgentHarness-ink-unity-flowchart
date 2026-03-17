// ===== 變更開始 =====
// 2026/03/17 Opsidanos (修改原因：建立 Batch 0 最小 canonical node kind 骨架，讓後續 GraphToolkit 語意能有獨立 core 掛點)
// 預期結果：後續抽離節點語意時，可從這裡取得穩定的 canonical node kind 名稱，不再只能依賴 GraphToolkit 內部常數
using System.Collections.Generic;

namespace OpsidanosInk.CanonicalGraph
{
    public static class CanonicalNodeKinds
    {
        private static readonly string[] OrderedNodeKinds =
        {
            Start,
            Dialogue,
            StageAction,
            Comment,
            Choice,
            Condition
        };

        public const string Start = "start";
        public const string Dialogue = "dialogue";
        public const string StageAction = "stageAction";
        public const string Comment = "comment";
        public const string Choice = "choice";
        public const string Condition = "condition";

        public static IReadOnlyList<string> All => OrderedNodeKinds;

        public static bool IsKnown(string nodeKind)
        {
            return nodeKind == Start
                || nodeKind == Dialogue
                || nodeKind == StageAction
                || nodeKind == Comment
                || nodeKind == Choice
                || nodeKind == Condition;
        }
    }
}
// ===== 變更結束 =====
