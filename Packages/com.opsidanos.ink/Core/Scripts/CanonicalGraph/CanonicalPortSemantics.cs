// ===== 變更開始 =====
// 2026/03/17 Opsidanos (修改原因：建立 Batch 0 最小 canonical port semantics 骨架，先把最常用的 Flow / ActionData / ActionIn 命名集中到 core)
// 預期結果：後續 GraphToolkit、validator、projection service 可共用同一組埠語意名稱，不再各自手寫字串
namespace OpsidanosInk.CanonicalGraph
{
    public static class CanonicalPortSemantics
    {
        public const string Flow = "Flow";
        public const string ActionData = "ActionData";
        public const string ActionInputPrefix = "ActionIn";

        public static string BuildActionInputPortName(int order)
        {
            return $"{ActionInputPrefix}{order}";
        }

        public static bool TryParseActionInputOrder(string portName, out int order)
        {
            order = -1;
            if (string.IsNullOrEmpty(portName) || !portName.StartsWith(ActionInputPrefix, System.StringComparison.Ordinal))
            {
                return false;
            }

            string suffix = portName.Substring(ActionInputPrefix.Length);
            if (!int.TryParse(suffix, out order))
            {
                order = -1;
                return false;
            }

            if (order < 0)
            {
                order = -1;
                return false;
            }

            return true;
        }
    }
}
// ===== 變更結束 =====
