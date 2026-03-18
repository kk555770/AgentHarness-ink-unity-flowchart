// ===== 變更開始 =====
// 2026/03/18 Opsidanos (修改原因：開始落地 Batch 3 下一刀，將 node option 讀值與 branch port 顯示小工具從 InkFlowChartNodes 抽成獨立 helper)
// 預期結果：InkFlowChartNodes 不再自己保存重複的小工具方法；node shell 可共用同一組 option/branch label helper
using System;
using Unity.GraphToolkit.Editor;

namespace OpsidanosInk.Editor
{
    public static class InkFlowChartNodeShellUtility
    {
        public static int GetNodeOptionInt(Node node, string optionName)
        {
            INodeOption option = node.GetNodeOptionByName(optionName);
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

        public static string GetNodeOptionString(Node node, string optionName)
        {
            INodeOption option = node.GetNodeOptionByName(optionName);
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

        public static string BuildOutputPortName(int index)
        {
            return $"Out{index}";
        }

        public static string[] SplitLines(string text)
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

        public static string GetLineOrFallback(string[] lines, int index, string fallback)
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
}
// ===== 變更結束 =====
