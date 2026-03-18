// ===== 變更開始 =====
// 2026/03/18 Opsidanos (修改原因：開始落地 Batch 2 的 import plan seam，把 current projection DTO -> 匯入施工圖 的規則從 importer 抽成 shared service)
// 預期結果：匯入器不再自己整理 choice/condition 文本、dialogue action input 數量與 wire fallback；這些純 DTO 規則改由 shared service 集中提供
using System;
using System.Collections.Generic;

namespace OpsidanosInk.CanonicalGraph
{
    public static class CurrentFlowImportService
    {
        public static CurrentFlowImportPlan BuildPlan(ExportGraphDto graphDto)
        {
            var plan = new CurrentFlowImportPlan
            {
                startNodeId = graphDto?.startNodeId ?? string.Empty
            };

            if (graphDto == null || graphDto.nodes == null)
            {
                return plan;
            }

            for (int i = 0; i < graphDto.nodes.Count; i++)
            {
                ExportNodeDto exportNode = graphDto.nodes[i];
                if (exportNode == null)
                {
                    continue;
                }

                plan.nodes.Add(BuildNodePlan(graphDto, exportNode));
            }

            for (int i = 0; i < graphDto.nodes.Count; i++)
            {
                ExportNodeDto exportNode = graphDto.nodes[i];
                if (exportNode == null || string.IsNullOrEmpty(exportNode.id))
                {
                    continue;
                }

                BuildWirePlans(plan, exportNode);
            }

            return plan;
        }

        private static CurrentFlowImportNodePlan BuildNodePlan(ExportGraphDto graphDto, ExportNodeDto exportNode)
        {
            var nodePlan = new CurrentFlowImportNodePlan
            {
                nodeId = exportNode.id ?? string.Empty,
                nodeType = exportNode.type ?? string.Empty,
                content = exportNode.content ?? string.Empty
            };

            if (CurrentFlowProjectionNaming.IsDialogueNodeType(exportNode.type))
            {
                nodePlan.maxDialogueActionInputOrder = GetMaxDialogueActionInputOrder(graphDto, exportNode.id);
                return nodePlan;
            }

            if (string.Equals(exportNode.type, CanonicalNodeKinds.Choice, StringComparison.OrdinalIgnoreCase))
            {
                nodePlan.choiceOutputCount = exportNode.outputs?.Count ?? 0;
                nodePlan.choiceTexts = BuildJoinedLabels(exportNode.outputs);
                nodePlan.choiceModeToken = exportNode.choiceMode == "+" ? "+" : "*";
                return nodePlan;
            }

            if (string.Equals(exportNode.type, CanonicalNodeKinds.Condition, StringComparison.OrdinalIgnoreCase))
            {
                nodePlan.conditionOutputCount = exportNode.outputs?.Count ?? 0;
                nodePlan.conditionTexts = BuildJoinedConditions(exportNode.outputs);
                return nodePlan;
            }

            return nodePlan;
        }

        private static void BuildWirePlans(CurrentFlowImportPlan plan, ExportNodeDto exportNode)
        {
            if (exportNode.outputs != null && exportNode.outputs.Count > 0)
            {
                for (int i = 0; i < exportNode.outputs.Count; i++)
                {
                    ExportNodeOutputDto output = exportNode.outputs[i];
                    if (output == null || string.IsNullOrEmpty(output.toNodeId))
                    {
                        continue;
                    }

                    plan.wires.Add(new CurrentFlowImportWirePlan
                    {
                        fromNodeId = exportNode.id ?? string.Empty,
                        fromPortName = string.IsNullOrEmpty(output.portName) ? CanonicalPortSemantics.Flow : output.portName,
                        toNodeId = output.toNodeId ?? string.Empty,
                        toPortName = string.IsNullOrEmpty(output.toPortName) ? CanonicalPortSemantics.Flow : output.toPortName
                    });
                }

                return;
            }

            if (exportNode.nextIds == null || exportNode.nextIds.Count == 0)
            {
                return;
            }

            for (int i = 0; i < exportNode.nextIds.Count; i++)
            {
                string nextId = exportNode.nextIds[i] ?? string.Empty;
                if (string.IsNullOrEmpty(nextId))
                {
                    continue;
                }

                plan.wires.Add(new CurrentFlowImportWirePlan
                {
                    fromNodeId = exportNode.id ?? string.Empty,
                    fromPortName = CanonicalPortSemantics.Flow,
                    toNodeId = nextId,
                    toPortName = CanonicalPortSemantics.Flow
                });
            }
        }

        private static string BuildJoinedLabels(List<ExportNodeOutputDto> outputs)
        {
            if (outputs == null || outputs.Count == 0)
            {
                return string.Empty;
            }

            var labels = new List<string>(outputs.Count);
            for (int i = 0; i < outputs.Count; i++)
            {
                string label = outputs[i] != null ? outputs[i].label : string.Empty;
                labels.Add(label ?? string.Empty);
            }

            return string.Join("\n", labels);
        }

        private static string BuildJoinedConditions(List<ExportNodeOutputDto> outputs)
        {
            if (outputs == null || outputs.Count < 2)
            {
                return string.Empty;
            }

            var conditions = new List<string>(outputs.Count - 1);
            for (int i = 0; i < outputs.Count - 1; i++)
            {
                string condition = outputs[i] != null ? outputs[i].condition : string.Empty;
                conditions.Add(condition ?? string.Empty);
            }

            return string.Join("\n", conditions);
        }

        private static int GetMaxDialogueActionInputOrder(ExportGraphDto graphDto, string dialogueNodeId)
        {
            if (graphDto == null || graphDto.nodes == null || string.IsNullOrEmpty(dialogueNodeId))
            {
                return -1;
            }

            int maxOrder = -1;
            for (int i = 0; i < graphDto.nodes.Count; i++)
            {
                ExportNodeDto node = graphDto.nodes[i];
                if (node == null
                    || !string.Equals(node.type, CanonicalNodeKinds.StageAction, StringComparison.OrdinalIgnoreCase)
                    || node.outputs == null)
                {
                    continue;
                }

                for (int outputIndex = 0; outputIndex < node.outputs.Count; outputIndex++)
                {
                    ExportNodeOutputDto output = node.outputs[outputIndex];
                    if (output == null || !string.Equals(output.toNodeId, dialogueNodeId, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (CanonicalPortSemantics.TryParseActionInputOrder(output.toPortName, out int order) && order > maxOrder)
                    {
                        maxOrder = order;
                    }
                }
            }

            return maxOrder;
        }
    }
}
// ===== 變更結束 =====
