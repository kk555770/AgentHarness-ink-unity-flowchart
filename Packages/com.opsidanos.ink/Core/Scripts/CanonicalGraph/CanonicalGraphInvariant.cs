// ===== 變更開始 =====
// 2026/03/17 Opsidanos (修改原因：建立 Batch 0 最小 invariant 入口，先把已知的圖規則名稱與線性節點判定集中到 core)
// 預期結果：後續 validator 開始實作前，至少已有固定的 invariant 名稱與最小判定入口可供測試與文件對位
namespace OpsidanosInk.CanonicalGraph
{
    public static class CanonicalGraphInvariant
    {
        public const string RequiresStartNode = "requiresStartNode";
        public const string SingleFlowOutput = "singleFlowOutput";
        public const string ConditionElseLast = "conditionElseLast";

        public static bool UsesSingleFlowOutput(string nodeKind)
        {
            return nodeKind == CanonicalNodeKinds.Start
                || nodeKind == CanonicalNodeKinds.Dialogue
                || nodeKind == CanonicalNodeKinds.StageAction
                || nodeKind == CanonicalNodeKinds.Comment;
        }
    }
}
// ===== 變更結束 =====
