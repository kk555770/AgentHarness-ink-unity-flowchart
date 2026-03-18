// ===== 變更開始 =====
// 2026/03/18 Opsidanos (修改原因：開始落地 Batch 2 的 import adapter seam，把 import plan -> GraphToolkit graph 的重建流程從 importer 抽成 editor adapter)
// 預期結果：Importer 退回檔案入口與資產替換；GraphToolkit 專屬節點建立、option 寫入與 wire 建立則集中到單一 adapter
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpsidanosInk.CanonicalGraph;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace OpsidanosInk.Editor
{
    public static class CurrentFlowGraphToolkitImportAdapter
    {
        private const string FlowPortName = CanonicalPortSemantics.Flow;

        public static bool TryPopulateGraph(InkFlowChartGraph graph, CurrentFlowImportPlan importPlan, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (graph == null)
            {
                errorMessage = "匯入失敗：graph 不可為空。";
                return false;
            }

            object graphImplementation = GetGraphImplementation(graph);
            if (graphImplementation == null)
            {
                errorMessage = "匯入失敗：找不到 Graph 內部實作。";
                return false;
            }

            MethodInfo createNodeModelMethod = graphImplementation.GetType().GetMethod("CreateNodeModel", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo createWireMethod = graphImplementation
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(method => method.Name == "CreateWire" && method.GetParameters().Length == 3);

            if (createNodeModelMethod == null || createWireMethod == null)
            {
                errorMessage = "匯入失敗：找不到 Graph Toolkit 建圖方法（CreateNodeModel/CreateWire）。";
                return false;
            }

            var nodeById = new Dictionary<string, INode>(StringComparer.Ordinal);
            List<CurrentFlowImportNodePlan> nodePlans = importPlan?.nodes ?? new List<CurrentFlowImportNodePlan>();
            for (int nodeIndex = 0; nodeIndex < nodePlans.Count; nodeIndex++)
            {
                CurrentFlowImportNodePlan nodePlan = nodePlans[nodeIndex];
                if (nodePlan == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(nodePlan.nodeId))
                {
                    errorMessage = "匯入失敗：節點 id 不可為空。";
                    return false;
                }

                if (nodeById.ContainsKey(nodePlan.nodeId))
                {
                    errorMessage = $"匯入失敗：節點 id 重複（{nodePlan.nodeId}）。";
                    return false;
                }

                INode runtimeNode = CreateNodeByType(nodePlan.nodeType);
                if (runtimeNode == null)
                {
                    errorMessage = $"匯入失敗：不支援的節點型別（{nodePlan.nodeType}）。";
                    return false;
                }

                var position = new Vector2(180f + (300f * nodeIndex), 180f);
                createNodeModelMethod.Invoke(graphImplementation, new object[] { runtimeNode, position });

                if (!TryApplyNodePlan(runtimeNode, nodePlan, out errorMessage))
                {
                    return false;
                }

                nodeById[nodePlan.nodeId] = runtimeNode;
            }

            var connectedPairs = new HashSet<string>(StringComparer.Ordinal);
            List<CurrentFlowImportWirePlan> wirePlans = importPlan?.wires ?? new List<CurrentFlowImportWirePlan>();
            for (int i = 0; i < wirePlans.Count; i++)
            {
                CurrentFlowImportWirePlan wirePlan = wirePlans[i];
                if (wirePlan == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(wirePlan.fromNodeId) || string.IsNullOrEmpty(wirePlan.toNodeId))
                {
                    errorMessage = "匯入失敗：wire plan 缺少 fromNodeId 或 toNodeId。";
                    return false;
                }

                if (!nodeById.TryGetValue(wirePlan.fromNodeId, out INode fromNode))
                {
                    errorMessage = $"匯入失敗：找不到來源節點（{wirePlan.fromNodeId}）。";
                    return false;
                }

                if (!nodeById.TryGetValue(wirePlan.toNodeId, out INode toNode))
                {
                    errorMessage = $"匯入失敗：`{wirePlan.fromNodeId}` 指向不存在節點 `{wirePlan.toNodeId}`。";
                    return false;
                }

                string fromPortName = string.IsNullOrEmpty(wirePlan.fromPortName) ? FlowPortName : wirePlan.fromPortName;
                string toPortName = string.IsNullOrEmpty(wirePlan.toPortName) ? FlowPortName : wirePlan.toPortName;
                string pairKey = $"{wirePlan.fromNodeId}:{fromPortName}->{wirePlan.toNodeId}:{toPortName}";
                if (connectedPairs.Contains(pairKey))
                {
                    continue;
                }

                IPort fromOutputPort = fromNode.GetOutputPortByName(fromPortName);
                IPort toInputPort = toNode.GetInputPortByName(toPortName);
                if (fromOutputPort == null || toInputPort == null)
                {
                    errorMessage = $"匯入失敗：節點 `{wirePlan.fromNodeId}` 或 `{wirePlan.toNodeId}` 缺少對應 port（from={fromPortName}, to={toPortName}）。";
                    return false;
                }

                createWireMethod.Invoke(graphImplementation, new object[] { toInputPort, fromOutputPort, default(Hash128) });
                connectedPairs.Add(pairKey);
            }

            return true;
        }

        private static object GetGraphImplementation(InkFlowChartGraph graph)
        {
            FieldInfo graphImplementationField = typeof(Graph).GetField("m_Implementation", BindingFlags.Instance | BindingFlags.NonPublic);
            return graphImplementationField?.GetValue(graph);
        }

        private static INode CreateNodeByType(string nodeType)
        {
            if (string.Equals(nodeType, CanonicalNodeKinds.Start, StringComparison.OrdinalIgnoreCase))
            {
                return new 開始();
            }

            if (CurrentFlowProjectionNaming.IsDialogueNodeType(nodeType))
            {
                return new 對話();
            }

            if (string.Equals(nodeType, CanonicalNodeKinds.StageAction, StringComparison.OrdinalIgnoreCase))
            {
                return new 動作();
            }

            if (string.Equals(nodeType, CanonicalNodeKinds.Comment, StringComparison.OrdinalIgnoreCase))
            {
                return new 註解();
            }

            if (string.Equals(nodeType, CanonicalNodeKinds.Choice, StringComparison.OrdinalIgnoreCase))
            {
                return new 選項();
            }

            if (string.Equals(nodeType, CanonicalNodeKinds.Condition, StringComparison.OrdinalIgnoreCase))
            {
                return new 條件();
            }

            return null;
        }

        private static bool TryApplyNodePlan(INode node, CurrentFlowImportNodePlan nodePlan, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (node == null || nodePlan == null)
            {
                return true;
            }

            if (node is InkFlowDialogueNode)
            {
                int actionInputCount = nodePlan.maxDialogueActionInputOrder >= 0
                    ? Mathf.Max(InkFlowNodeOptionSchema.DialogueActionInputCountDefaultValue, nodePlan.maxDialogueActionInputOrder + 1)
                    : InkFlowNodeOptionSchema.DialogueActionInputCountDefaultValue;

                if (!TrySetNodeOptionValue((Node)node, InkFlowNodeOptionSchema.DialogueActionInputCountOptionName, actionInputCount, out errorMessage))
                {
                    return false;
                }

                if (!TrySetNodeOptionValue((Node)node, InkFlowNodeOptionSchema.DialogueContentOptionName, nodePlan.content ?? string.Empty, out errorMessage))
                {
                    return false;
                }

                ((Node)node).DefineNode();
                return true;
            }

            if (node is InkFlowStageActionNode)
            {
                return TrySetNodeOptionValue((Node)node, InkFlowNodeOptionSchema.StageActionContentOptionName, nodePlan.content ?? string.Empty, out errorMessage);
            }

            if (node is InkFlowCommentNode)
            {
                return TrySetNodeOptionValue((Node)node, InkFlowNodeOptionSchema.CommentNoteOptionName, nodePlan.content ?? string.Empty, out errorMessage);
            }

            if (node is InkFlowChoiceNode)
            {
                if (nodePlan.choiceOutputCount < 1)
                {
                    errorMessage = $"匯入失敗：choice 節點 `{nodePlan.nodeId}` 至少需要 1 個 outputs。";
                    return false;
                }

                if (!TrySetNodeOptionValue((Node)node, InkFlowNodeOptionSchema.ChoiceOutputCountOptionName, nodePlan.choiceOutputCount, out errorMessage))
                {
                    return false;
                }

                if (!TrySetNodeOptionValue((Node)node, InkFlowNodeOptionSchema.ChoiceTextsOptionName, nodePlan.choiceTexts ?? string.Empty, out errorMessage))
                {
                    return false;
                }

                InkFlowChoiceMode mode = nodePlan.choiceModeToken == "+" ? InkFlowChoiceMode.Repeatable : InkFlowChoiceMode.Once;
                if (!TrySetNodeOptionValue((Node)node, InkFlowNodeOptionSchema.ChoiceModeOptionName, mode, out errorMessage))
                {
                    return false;
                }

                ((Node)node).DefineNode();
                return true;
            }

            if (node is InkFlowConditionNode)
            {
                if (nodePlan.conditionOutputCount < 2)
                {
                    errorMessage = $"匯入失敗：condition 節點 `{nodePlan.nodeId}` 至少需要 2 個 outputs（含 else）。";
                    return false;
                }

                if (!TrySetNodeOptionValue((Node)node, InkFlowNodeOptionSchema.ConditionOutputCountOptionName, nodePlan.conditionOutputCount, out errorMessage))
                {
                    return false;
                }

                if (!TrySetNodeOptionValue((Node)node, InkFlowNodeOptionSchema.ConditionTextsOptionName, nodePlan.conditionTexts ?? string.Empty, out errorMessage))
                {
                    return false;
                }

                ((Node)node).DefineNode();
                return true;
            }

            return true;
        }

        private static bool TrySetNodeOptionValue<T>(Node node, string optionName, T value, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (node == null)
            {
                return true;
            }

            INodeOption option = node.GetNodeOptionByName(optionName);
            if (option == null)
            {
                errorMessage = $"匯入失敗：找不到節點選項 `{optionName}`。";
                return false;
            }

            PropertyInfo portModelProperty = option.GetType().GetProperty("PortModel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            object portModelObject = portModelProperty?.GetValue(option);
            if (portModelObject == null)
            {
                errorMessage = $"匯入失敗：節點選項 `{optionName}` 缺少 PortModel。";
                return false;
            }

            PropertyInfo embeddedValueProperty = portModelObject.GetType().GetProperty("EmbeddedValue", BindingFlags.Instance | BindingFlags.Public);
            object embeddedValueObject = embeddedValueProperty?.GetValue(portModelObject);
            if (embeddedValueObject == null)
            {
                errorMessage = $"匯入失敗：節點選項 `{optionName}` 缺少 EmbeddedValue。";
                return false;
            }

            MethodInfo trySetValueOpenGeneric = embeddedValueObject.GetType().GetMethod("TrySetValue", BindingFlags.Instance | BindingFlags.Public);
            if (trySetValueOpenGeneric == null || !trySetValueOpenGeneric.IsGenericMethodDefinition)
            {
                errorMessage = $"匯入失敗：節點選項 `{optionName}` 無法寫入內容（找不到 EmbeddedValue.TrySetValue<T>）。";
                return false;
            }

            MethodInfo trySetValueMethod = trySetValueOpenGeneric.MakeGenericMethod(typeof(T));
            object setResult = trySetValueMethod.Invoke(embeddedValueObject, new object[] { value });
            bool success = setResult is bool boolResult && boolResult;
            if (success)
            {
                return true;
            }

            if (typeof(T).IsEnum)
            {
                Enum enumValue = value == null ? null : (Enum)(object)value;
                if (enumValue == null)
                {
                    errorMessage = $"匯入失敗：節點選項 `{optionName}` enum 值不可為空。";
                    return false;
                }

                if (TrySetNodeOptionEnumValueReference(embeddedValueObject, trySetValueOpenGeneric, enumValue, out string enumError))
                {
                    return true;
                }

                errorMessage = $"匯入失敗：節點選項 `{optionName}` 寫入失敗（enum 需要 EnumValueReference）。{enumError}";
                return false;
            }

            errorMessage = $"匯入失敗：節點選項 `{optionName}` 寫入失敗。";
            return false;
        }

        private static bool TrySetNodeOptionEnumValueReference(object embeddedValueObject, MethodInfo trySetValueOpenGeneric, Enum enumValue, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (embeddedValueObject == null || trySetValueOpenGeneric == null || enumValue == null)
            {
                errorMessage = "EmbeddedValue 或 enumValue 為空。";
                return false;
            }

            Type enumValueReferenceType = FindTypeInLoadedAssemblies("Unity.GraphToolkit.EnumValueReference");
            if (enumValueReferenceType == null)
            {
                errorMessage = "找不到 Unity.GraphToolkit.EnumValueReference（可能是 GraphToolkit 版本差異）。";
                return false;
            }

            ConstructorInfo enumValueReferenceCtor = enumValueReferenceType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: new[] { typeof(Enum) },
                modifiers: null);

            if (enumValueReferenceCtor == null)
            {
                errorMessage = "找不到 EnumValueReference(Enum) 建構子。";
                return false;
            }

            object enumValueReference = enumValueReferenceCtor.Invoke(new object[] { enumValue });
            MethodInfo trySetEnumValueReferenceMethod = trySetValueOpenGeneric.MakeGenericMethod(enumValueReferenceType);
            object setResult = trySetEnumValueReferenceMethod.Invoke(embeddedValueObject, new object[] { enumValueReference });
            bool success = setResult is bool boolResult && boolResult;
            if (!success)
            {
                errorMessage = "TrySetValue<EnumValueReference> 回傳 false。";
                return false;
            }

            return true;
        }

        private static Type FindTypeInLoadedAssemblies(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return null;
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName, throwOnError: false);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
// ===== 變更結束 =====
