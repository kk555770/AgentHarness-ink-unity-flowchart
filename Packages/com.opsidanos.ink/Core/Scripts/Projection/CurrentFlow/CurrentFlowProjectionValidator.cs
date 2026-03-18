// ===== 變更開始 =====
// 2026/03/18 Opsidanos (修改原因：抽出 current projection 的純 DTO 驗證規則，讓 exporter/importer 不再各自保管同一包 sidecar 合法性)
// 預期結果：只要拿到 `.flowchart.json` 對應 DTO，就能先驗證 choice/condition/stageAction/dialogue 內容是否合法，不必先依賴 GraphToolkit graph 物件
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpsidanosInk.CanonicalGraph
{
    public static class CurrentFlowProjectionValidator
    {
        public static bool TryValidate(ExportGraphDto exportDto, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (exportDto == null || exportDto.nodes == null)
            {
                return true;
            }

            int startNodeCount = exportDto.nodes.Count(node =>
                node != null && string.Equals(node.type, CanonicalNodeKinds.Start, StringComparison.OrdinalIgnoreCase));
            if (startNodeCount != 1)
            {
                errorMessage = $"Graph v2 必須有且只有 1 個 start 節點，但目前有 {startNodeCount} 個。";
                return false;
            }

            Dictionary<string, ExportNodeDto> nodeById = BuildNodeMap(exportDto.nodes);
            foreach (ExportNodeDto exportNode in exportDto.nodes)
            {
                if (exportNode == null)
                {
                    continue;
                }

                if (CurrentFlowProjectionNaming.IsDialogueNodeType(exportNode.type))
                {
                    if (!TryValidateActionContent(exportNode, out errorMessage))
                    {
                        return false;
                    }
                }

                if (IsLinearNodeType(exportNode.type))
                {
                    if (!TryValidateLinearNode(exportNode, nodeById, out errorMessage))
                    {
                        return false;
                    }

                    continue;
                }

                if (string.Equals(exportNode.type, CanonicalNodeKinds.StageAction, StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryValidateStageActionNode(exportNode, nodeById, out errorMessage))
                    {
                        return false;
                    }

                    continue;
                }

                if (string.Equals(exportNode.type, CanonicalNodeKinds.Choice, StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryValidateChoiceNode(exportNode, out errorMessage))
                    {
                        return false;
                    }

                    continue;
                }

                if (string.Equals(exportNode.type, CanonicalNodeKinds.Condition, StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryValidateConditionNode(exportNode, out errorMessage))
                    {
                        return false;
                    }

                    continue;
                }

                errorMessage = $"不支援的節點型別：id=\"{exportNode.id}\" type=\"{exportNode.type}\"。";
                return false;
            }

            return true;
        }

        private static bool TryValidateLinearNode(ExportNodeDto exportNode, Dictionary<string, ExportNodeDto> nodeById, out string errorMessage)
        {
            errorMessage = string.Empty;

            List<ExportNodeOutputDto> outputs = exportNode.outputs ?? new List<ExportNodeOutputDto>();
            List<string> nextIds = exportNode.nextIds ?? new List<string>();

            if (outputs.Count > 1)
            {
                errorMessage = $"線性節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 只允許 0 或 1 條輸出線；若要分岔請改用 choice/condition 節點。";
                return false;
            }

            if (outputs.Count == 0 && nextIds.Count > 1)
            {
                errorMessage = $"Graph v1（線性流程）只允許每個節點有 0 或 1 條 next。節點 `{exportNode.id}`（type={exportNode.type}）有 {nextIds.Count} 條：{string.Join(", ", nextIds)}。";
                return false;
            }

            int effectiveNextCount = outputs.Count > 0 ? outputs.Count : nextIds.Count;
            if (string.Equals(exportNode.type, CanonicalNodeKinds.Start, StringComparison.OrdinalIgnoreCase) && effectiveNextCount != 1)
            {
                errorMessage = $"start 節點 id=\"{exportNode.id}\" 必須且只能接 1 條下一步連線，但目前是 {effectiveNextCount} 條。";
                return false;
            }

            if (outputs.Count == 0)
            {
                if (nextIds.Count == 1 && !string.IsNullOrEmpty(nextIds[0]) && !nodeById.ContainsKey(nextIds[0]))
                {
                    errorMessage = $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 指向不存在節點 `{nextIds[0]}`。";
                    return false;
                }

                return true;
            }

            ExportNodeOutputDto outputDto = outputs[0];
            if (outputDto == null
                || string.IsNullOrEmpty(outputDto.toNodeId)
                || (!string.IsNullOrEmpty(outputDto.toPortName)
                    && !string.Equals(outputDto.toPortName, CanonicalPortSemantics.Flow, StringComparison.Ordinal)))
            {
                errorMessage = $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 的 Flow 連線未指向可匯出的節點（可能連到非 Flow 節點）。";
                return false;
            }

            if (!nodeById.ContainsKey(outputDto.toNodeId))
            {
                errorMessage = $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 指向不存在節點 `{outputDto.toNodeId}`。";
                return false;
            }

            return true;
        }

        private static bool TryValidateStageActionNode(ExportNodeDto exportNode, Dictionary<string, ExportNodeDto> nodeById, out string errorMessage)
        {
            errorMessage = string.Empty;

            List<ExportNodeOutputDto> outputs = exportNode.outputs ?? new List<ExportNodeOutputDto>();
            if (outputs.Count != 1)
            {
                errorMessage = $"動作節點 id=\"{exportNode.id}\" 必須且只能有 1 條資料線連到對話節點，目前是 {outputs.Count} 條。";
                return false;
            }

            ExportNodeOutputDto outputDto = outputs[0];
            if (outputDto == null || string.IsNullOrEmpty(outputDto.toNodeId))
            {
                errorMessage = $"動作節點 id=\"{exportNode.id}\" 的資料線未連到有效節點。";
                return false;
            }

            if (!nodeById.TryGetValue(outputDto.toNodeId, out ExportNodeDto targetNode) || targetNode == null)
            {
                errorMessage = $"動作節點 id=\"{exportNode.id}\" 指向不存在節點 `{outputDto.toNodeId}`。";
                return false;
            }

            if (!CurrentFlowProjectionNaming.IsDialogueNodeType(targetNode.type))
            {
                errorMessage = $"動作節點 id=\"{exportNode.id}\" 的資料線只能接到對話節點，目前目標型別不是對話。";
                return false;
            }

            if (!CanonicalPortSemantics.TryParseActionInputOrder(outputDto.toPortName, out _))
            {
                errorMessage = $"動作節點 id=\"{exportNode.id}\" 連到的目標輸入埠 `{outputDto.toPortName}` 不是合法對話動作輸入埠（ActionIn*）。";
                return false;
            }

            return true;
        }

        private static bool TryValidateChoiceNode(ExportNodeDto exportNode, out string errorMessage)
        {
            errorMessage = string.Empty;

            List<ExportNodeOutputDto> outputs = exportNode.outputs ?? new List<ExportNodeOutputDto>();
            if (outputs.Count < 1)
            {
                errorMessage = $"choice 節點 id=\"{exportNode.id}\" 至少需要 1 個輸出埠。";
                return false;
            }

            if (exportNode.choiceMode != "*" && exportNode.choiceMode != "+")
            {
                errorMessage = $"choice 節點 id=\"{exportNode.id}\" 的 choiceMode 必須是 \"*\" 或 \"+\"。";
                return false;
            }

            for (int i = 0; i < outputs.Count; i++)
            {
                ExportNodeOutputDto outputDto = outputs[i];
                string portName = outputDto != null ? outputDto.portName : string.Empty;
                if (outputDto == null || string.IsNullOrEmpty(outputDto.toNodeId))
                {
                    errorMessage = $"choice 節點 id=\"{exportNode.id}\" 的輸出埠 name=\"{portName}\" 未連到有效節點。";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(outputDto.label))
                {
                    errorMessage = $"choice 節點 id=\"{exportNode.id}\" 的輸出埠 name=\"{portName}\" 缺少選項文字（label）。";
                    return false;
                }

                if (outputDto.label.Contains("[") || outputDto.label.Contains("]"))
                {
                    errorMessage = $"choice 節點 id=\"{exportNode.id}\" 的選項文字（label）不可包含 `[` 或 `]`，否則匯出 Ink 會破壞選項括號。label=\"{outputDto.label}\"";
                    return false;
                }
            }

            return true;
        }

        private static bool TryValidateConditionNode(ExportNodeDto exportNode, out string errorMessage)
        {
            errorMessage = string.Empty;

            List<ExportNodeOutputDto> outputs = exportNode.outputs ?? new List<ExportNodeOutputDto>();
            if (outputs.Count < 2)
            {
                errorMessage = $"condition 節點 id=\"{exportNode.id}\" 至少需要 2 個 outputs（含 else）。";
                return false;
            }

            int elseIndex = outputs.Count - 1;
            for (int i = 0; i < outputs.Count; i++)
            {
                ExportNodeOutputDto outputDto = outputs[i];
                string portName = outputDto != null ? outputDto.portName : string.Empty;
                if (outputDto == null || string.IsNullOrEmpty(outputDto.toNodeId))
                {
                    errorMessage = $"condition 節點 id=\"{exportNode.id}\" 的輸出埠 name=\"{portName}\" 未連到有效節點。";
                    return false;
                }

                if (i == elseIndex)
                {
                    if (!outputDto.isElse)
                    {
                        errorMessage = $"condition 節點 id=\"{exportNode.id}\" 的最後一個 outputs 必須是 else 分支（isElse=true）。";
                        return false;
                    }

                    continue;
                }

                if (outputDto.isElse)
                {
                    errorMessage = $"condition 節點 id=\"{exportNode.id}\" 只有最後一個輸出埠可以是 else 分支。";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(outputDto.condition))
                {
                    errorMessage = $"condition 節點 id=\"{exportNode.id}\" 的輸出埠 name=\"{portName}\" 缺少條件式（condition）。";
                    return false;
                }

                if (outputDto.condition.Contains(":"))
                {
                    errorMessage = $"condition 節點 id=\"{exportNode.id}\" 的條件式（condition）不需要包含冒號 `:`（匯出時會自動加上）。condition=\"{outputDto.condition}\"";
                    return false;
                }
            }

            return true;
        }

        private static Dictionary<string, ExportNodeDto> BuildNodeMap(List<ExportNodeDto> nodes)
        {
            var map = new Dictionary<string, ExportNodeDto>(StringComparer.Ordinal);
            if (nodes == null)
            {
                return map;
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                ExportNodeDto node = nodes[i];
                if (node == null || string.IsNullOrEmpty(node.id))
                {
                    continue;
                }

                if (!map.ContainsKey(node.id))
                {
                    map.Add(node.id, node);
                }
            }

            return map;
        }

        private static bool IsLinearNodeType(string nodeType)
        {
            return string.Equals(nodeType, CanonicalNodeKinds.Start, StringComparison.OrdinalIgnoreCase)
                || CurrentFlowProjectionNaming.IsDialogueNodeType(nodeType)
                || string.Equals(nodeType, CanonicalNodeKinds.Comment, StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryValidateActionContent(ExportNodeDto exportNode, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (exportNode == null)
            {
                return true;
            }

            string content = exportNode.content ?? string.Empty;
            if (string.IsNullOrEmpty(content))
            {
                return true;
            }

            string[] lines = SplitLines(content);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i] ?? string.Empty;

                if (ContainsUnescapedSequence(line, "->"))
                {
                    errorMessage =
                        $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 的內容第 {i + 1} 行包含未跳脫的 `->`，Ink 會把它解讀成 divert（跳轉），代表你把流程藏在文字裡。若要分岔/跳轉請用接線或 choice/condition 節點；若只是要顯示符號請寫 `\\\\->`。";
                    return false;
                }

                if (ContainsUnescapedSequence(line, "<-"))
                {
                    errorMessage =
                        $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 的內容第 {i + 1} 行包含未跳脫的 `<-`，Ink 會把它解讀成 thread 語法，代表你把流程藏在文字裡。若只是要顯示符號請寫 `\\\\<-`。";
                    return false;
                }

                string trimmed = line.TrimStart(' ', '\t');
                if (string.IsNullOrEmpty(trimmed))
                {
                    continue;
                }

                if (trimmed == "{")
                {
                    errorMessage =
                        $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 的內容第 {i + 1} 行只有 `{{`，這會開始多行 conditional block（規範禁止在 action 內容建立分岔）。請改用 condition 節點；若只是要顯示符號請寫 `\\\\{{`。";
                    return false;
                }

                if (trimmed == "}")
                {
                    errorMessage =
                        $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 的內容第 {i + 1} 行只有 `}}`，這會結束多行 conditional block（規範禁止在 action 內容建立分岔）。請改用 condition 節點；若只是要顯示符號請寫 `\\\\}}`。";
                    return false;
                }

                if (trimmed.StartsWith("\\", StringComparison.Ordinal))
                {
                    continue;
                }

                if (trimmed.StartsWith("=", StringComparison.Ordinal))
                {
                    errorMessage =
                        $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 的內容第 {i + 1} 行以 `=` 開頭，Ink 會把它當成 knot/stitch 定義，代表你把流程藏在文字裡。請改用節點/接線；若只是要顯示符號請寫 `\\\\=`。";
                    return false;
                }

                if (trimmed.StartsWith("*", StringComparison.Ordinal) || trimmed.StartsWith("+", StringComparison.Ordinal))
                {
                    errorMessage =
                        $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 的內容第 {i + 1} 行以 `*` 或 `+` 開頭，Ink 會把它當成 choice（選項），代表你把流程藏在文字裡。請改用 choice 節點；若只是要顯示符號請寫 `\\\\*` 或 `\\\\+`。";
                    return false;
                }

                if (trimmed.StartsWith("-", StringComparison.Ordinal))
                {
                    errorMessage =
                        $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 的內容第 {i + 1} 行以 `-` 開頭，Ink 會把它當成 weave/gather 或 conditional 分支行，代表你把流程藏在文字裡。請改用接線匯流或 condition 節點；若只是要顯示符號請寫 `\\\\-`。";
                    return false;
                }

                if (trimmed.Equals("INCLUDE", StringComparison.OrdinalIgnoreCase)
                    || trimmed.StartsWith("INCLUDE ", StringComparison.OrdinalIgnoreCase))
                {
                    errorMessage =
                        $"節點 id=\"{exportNode.id}\" type=\"{exportNode.type}\" 的內容第 {i + 1} 行以 `INCLUDE` 開頭，但 Graph v2 子集合未定義 INCLUDE 的可逆資料結構。請改用節點/接線與打包流程；若只是要顯示文字請寫 `\\\\INCLUDE`。";
                    return false;
                }
            }

            return true;
        }

        private static bool ContainsUnescapedSequence(string text, string sequence)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(sequence))
            {
                return false;
            }

            for (int i = 0; i <= text.Length - sequence.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < sequence.Length; j++)
                {
                    if (text[i + j] != sequence[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (!match)
                {
                    continue;
                }

                int slashCount = 0;
                int slashIndex = i - 1;
                while (slashIndex >= 0 && text[slashIndex] == '\\')
                {
                    slashCount++;
                    slashIndex--;
                }

                if (slashCount % 2 == 0)
                {
                    return true;
                }

                i += sequence.Length - 1;
            }

            return false;
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
    }
}
// ===== 變更結束 =====
