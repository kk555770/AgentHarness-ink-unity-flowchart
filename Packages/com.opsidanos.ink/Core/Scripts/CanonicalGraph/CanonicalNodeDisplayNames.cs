// ===== 變更開始 =====
// 2026/03/18 Opsidanos (修改原因：開始落地 Batch 1，先把 GraphToolkit 與未來作者工具都會共用的節點顯示名稱集中到 core seam)
// 預期結果：動作輸入顯示名稱、選項/條件 fallback label 與 else 標籤不再只能綁在 GraphToolkit 節點檔內
namespace OpsidanosInk.CanonicalGraph
{
    public static class CanonicalNodeDisplayNames
    {
        public const string Choice = "選項";
        public const string Condition = "條件";
        public const string DialogueActionInputPrefix = "動作";
        public const string StageActionDataPort = "動作資料";
        public const string ConditionElsePort = "否則";

        public static string BuildDialogueActionInputDisplayName(int order)
        {
            return $"{DialogueActionInputPrefix}{order + 1}";
        }

        public static string GetChoicePortFallbackLabel(int order)
        {
            return $"{Choice}{order}";
        }

        public static string GetConditionPortFallbackLabel(int order)
        {
            return $"{Condition}{order}";
        }
    }
}
// ===== 變更結束 =====
