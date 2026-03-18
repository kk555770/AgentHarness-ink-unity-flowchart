// ===== 變更開始 =====
// 2026/03/18 Opsidanos (修改原因：開始落地 Batch 2 的 projection seam，把 current projection DTO -> Ink 文字輸出的規則從 exporter 抽成 shared service)
// 預期結果：只要拿到 current projection DTO，就能在 GraphToolkit 之外重用同一套 Ink 組裝邏輯；exporter 則退回成檔案入口與 adapter
using System;
using System.Collections.Generic;
using System.Text;

namespace OpsidanosInk.CanonicalGraph
{
    public static class CurrentFlowProjectionService
    {
        private readonly struct DialogueBoundAction
        {
            public string dialogueNodeId { get; }
            public string stageActionNodeId { get; }
            public string content { get; }
            public int inputOrder { get; }

            public DialogueBoundAction(string dialogueNodeId, string stageActionNodeId, string content, int inputOrder)
            {
                this.dialogueNodeId = dialogueNodeId ?? string.Empty;
                this.stageActionNodeId = stageActionNodeId ?? string.Empty;
                this.content = content ?? string.Empty;
                this.inputOrder = inputOrder;
            }
        }

        public static string BuildInkContent(ExportGraphDto exportDto)
        {
            var inkBuilder = new StringBuilder();
            inkBuilder.AppendLine($"-> knot_{exportDto.startNodeId}");
            inkBuilder.AppendLine();
            Dictionary<string, List<DialogueBoundAction>> dialogueActionMap = BuildDialogueActionMap(exportDto);

            foreach (ExportNodeDto exportNode in exportDto.nodes)
            {
                if (string.Equals(exportNode.type, CanonicalNodeKinds.StageAction, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                inkBuilder.AppendLine($"=== knot_{exportNode.id} ===");

                if (string.Equals(exportNode.type, CanonicalNodeKinds.Start, StringComparison.OrdinalIgnoreCase))
                {
                    string nextId = exportNode.outputs != null && exportNode.outputs.Count > 0 ? exportNode.outputs[0].toNodeId : string.Empty;
                    if (!string.IsNullOrEmpty(nextId))
                    {
                        inkBuilder.AppendLine($"-> knot_{nextId}");
                    }
                    else
                    {
                        inkBuilder.AppendLine("-> END");
                    }
                }
                else if (CurrentFlowProjectionNaming.IsDialogueNodeType(exportNode.type))
                {
                    if (dialogueActionMap.TryGetValue(exportNode.id, out List<DialogueBoundAction> actions))
                    {
                        for (int i = 0; i < actions.Count; i++)
                        {
                            string actionContent = actions[i].content ?? string.Empty;
                            if (string.IsNullOrEmpty(actionContent))
                            {
                                continue;
                            }

                            inkBuilder.AppendLine(actionContent);
                        }
                    }

                    if (string.IsNullOrEmpty(exportNode.content))
                    {
                        inkBuilder.AppendLine("// 空內容");
                    }
                    else
                    {
                        inkBuilder.AppendLine(exportNode.content);
                    }

                    string nextId = exportNode.outputs != null && exportNode.outputs.Count > 0 ? exportNode.outputs[0].toNodeId : string.Empty;
                    if (!string.IsNullOrEmpty(nextId))
                    {
                        inkBuilder.AppendLine($"-> knot_{nextId}");
                    }
                    else
                    {
                        inkBuilder.AppendLine("-> END");
                    }
                }
                else if (string.Equals(exportNode.type, CanonicalNodeKinds.Comment, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(exportNode.content))
                    {
                        inkBuilder.AppendLine("// (空註解)");
                    }
                    else
                    {
                        string[] commentLines = SplitLines(exportNode.content);
                        for (int i = 0; i < commentLines.Length; i++)
                        {
                            inkBuilder.AppendLine($"// {commentLines[i]}");
                        }
                    }

                    string nextId = exportNode.outputs != null && exportNode.outputs.Count > 0 ? exportNode.outputs[0].toNodeId : string.Empty;
                    if (!string.IsNullOrEmpty(nextId))
                    {
                        inkBuilder.AppendLine($"-> knot_{nextId}");
                    }
                    else
                    {
                        inkBuilder.AppendLine("-> END");
                    }
                }
                else if (string.Equals(exportNode.type, CanonicalNodeKinds.Choice, StringComparison.OrdinalIgnoreCase))
                {
                    string bullet = exportNode.choiceMode == "+" ? "+" : "*";
                    foreach (ExportNodeOutputDto output in exportNode.outputs)
                    {
                        inkBuilder.AppendLine($"{bullet} [{output.label}] -> knot_{output.toNodeId}");
                    }
                }
                else if (string.Equals(exportNode.type, CanonicalNodeKinds.Condition, StringComparison.OrdinalIgnoreCase))
                {
                    inkBuilder.AppendLine("{");
                    foreach (ExportNodeOutputDto output in exportNode.outputs)
                    {
                        if (output.isElse)
                        {
                            inkBuilder.AppendLine("  - else:");
                        }
                        else
                        {
                            inkBuilder.AppendLine($"  - {output.condition}:");
                        }

                        inkBuilder.AppendLine($"    -> knot_{output.toNodeId}");
                    }
                    inkBuilder.AppendLine("}");
                }
                else
                {
                    inkBuilder.AppendLine("// 不支援的節點型別");
                    inkBuilder.AppendLine("-> END");
                }

                inkBuilder.AppendLine();
            }

            return inkBuilder.ToString();
        }

        private static Dictionary<string, List<DialogueBoundAction>> BuildDialogueActionMap(ExportGraphDto exportDto)
        {
            var map = new Dictionary<string, List<DialogueBoundAction>>(StringComparer.Ordinal);
            if (exportDto == null || exportDto.nodes == null)
            {
                return map;
            }

            foreach (ExportNodeDto node in exportDto.nodes)
            {
                if (node == null || !string.Equals(node.type, CanonicalNodeKinds.StageAction, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (node.outputs == null || node.outputs.Count == 0)
                {
                    continue;
                }

                for (int i = 0; i < node.outputs.Count; i++)
                {
                    ExportNodeOutputDto output = node.outputs[i];
                    if (output == null || string.IsNullOrEmpty(output.toNodeId))
                    {
                        continue;
                    }

                    int inputOrder = CanonicalPortSemantics.TryParseActionInputOrder(output.toPortName, out int parsedOrder)
                        ? parsedOrder
                        : int.MaxValue;
                    var action = new DialogueBoundAction(output.toNodeId, node.id, node.content, inputOrder);

                    if (!map.TryGetValue(output.toNodeId, out List<DialogueBoundAction> list))
                    {
                        list = new List<DialogueBoundAction>();
                        map[output.toNodeId] = list;
                    }

                    list.Add(action);
                }
            }

            foreach (KeyValuePair<string, List<DialogueBoundAction>> pair in map)
            {
                pair.Value.Sort((left, right) =>
                {
                    int orderCompare = left.inputOrder.CompareTo(right.inputOrder);
                    if (orderCompare != 0)
                    {
                        return orderCompare;
                    }

                    return string.CompareOrdinal(left.stageActionNodeId, right.stageActionNodeId);
                });
            }

            return map;
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
