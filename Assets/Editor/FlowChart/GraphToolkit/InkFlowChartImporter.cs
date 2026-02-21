// ===== 變更開始 =====
// 2026/02/08 Opsidanos (修改原因：補齊 Graph Toolkit 匯入 MVP，讓 `.flowchart.json + .ink` 可還原成 `.inkfc`)
// 預期結果：可從 sidecar 重新建立開始/流程/註解節點、內容與連線，失敗時回傳明確錯誤訊息
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace OpsidanosInk.Editor
{
    public readonly struct InkFlowChartImportResult
    {
        public bool success { get; }
        public string graphAssetPath { get; }
        public string errorMessage { get; }

        private InkFlowChartImportResult(bool success, string graphAssetPath, string errorMessage)
        {
            this.success = success;
            this.graphAssetPath = graphAssetPath ?? string.Empty;
            this.errorMessage = errorMessage ?? string.Empty;
        }

        public static InkFlowChartImportResult Success(string graphAssetPath)
        {
            return new InkFlowChartImportResult(true, graphAssetPath, string.Empty);
        }

        public static InkFlowChartImportResult Failure(string graphAssetPath, string errorMessage)
        {
            return new InkFlowChartImportResult(false, graphAssetPath, errorMessage);
        }
    }

    public static class InkFlowChartImporter
    {
        private const string ActionContentOptionName = "Content";
        private const string CommentNoteOptionName = "Note";
        private const string FlowPortName = "Flow";
        private const string FlowchartJsonSuffix = ".flowchart.json";

        public static InkFlowChartImportResult ImportFromFlowchartJson(string flowchartJsonPath)
        {
            string graphAssetPath = string.Empty;
            // ===== 變更開始 =====
            // 2026/02/08 Opsidanos (修改原因：避免直接在目標路徑刪除後重建，先用暫存圖資產建圖再替換，降低 Undo 卡住風險)
            // 預期結果：匯入過程不再直接觸發「同路徑新建資產」流程，失敗時只清理暫存圖
            string workingGraphAssetPath = string.Empty;
            bool workingGraphCreated = false;
            // ===== 變更結束 =====

            InkFlowChartImportResult Fail(string errorMessage)
            {
                if (workingGraphCreated && !string.IsNullOrEmpty(workingGraphAssetPath) && AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(workingGraphAssetPath) != null)
                {
                    AssetDatabase.DeleteAsset(workingGraphAssetPath);
                }

                return InkFlowChartImportResult.Failure(graphAssetPath, errorMessage);
            }

            try
            {
                if (string.IsNullOrEmpty(flowchartJsonPath))
                {
                    return Fail("匯入失敗：未提供 .flowchart.json 路徑。");
                }

                if (!flowchartJsonPath.EndsWith(FlowchartJsonSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    return Fail($"匯入失敗：不是 .flowchart.json 檔案（{flowchartJsonPath}）。");
                }

                if (!File.Exists(flowchartJsonPath))
                {
                    return Fail($"匯入失敗：找不到 sidecar 檔案（{flowchartJsonPath}）。");
                }

                string jsonContent = File.ReadAllText(flowchartJsonPath);
                ExportGraphDto graphDto = JsonUtility.FromJson<ExportGraphDto>(jsonContent);
                if (graphDto == null)
                {
                    return Fail($"匯入失敗：sidecar 內容無法解析（{flowchartJsonPath}）。");
                }

                if (graphDto.nodes == null)
                {
                    graphDto.nodes = new List<ExportNodeDto>();
                }

                if (string.IsNullOrEmpty(graphDto.startNodeId))
                {
                    return Fail("匯入失敗：startNodeId 為空。");
                }

                bool startNodeExists = graphDto.nodes.Any(node => node.id == graphDto.startNodeId);
                if (!startNodeExists)
                {
                    return Fail($"匯入失敗：startNodeId `{graphDto.startNodeId}` 不存在於 nodes。");
                }

                string flowchartDirectory = Path.GetDirectoryName(flowchartJsonPath) ?? "Assets";
                string baseFileName = GetBaseName(flowchartJsonPath);
                string inkPath = $"{flowchartDirectory}/{baseFileName}.ink";
                if (!File.Exists(inkPath))
                {
                    return Fail($"匯入失敗：缺少配對的 .ink 檔案（{inkPath}）。");
                }

                graphAssetPath = $"{flowchartDirectory}/{baseFileName}.inkfc";
                // ===== 變更開始 =====
                // 2026/02/08 Opsidanos (修改原因：匯入改為先建立暫存 `.inkfc`，建圖成功後再替換正式檔)
                // 預期結果：避免直接在目標路徑呼叫 `CreateGraph`，降低 Editor Undo 異常觸發機率
                workingGraphAssetPath = AssetDatabase.GenerateUniqueAssetPath($"{flowchartDirectory}/{baseFileName}__importing__.inkfc");
                InkFlowChartGraph graph = CreateGraphWithMutedLogger(workingGraphAssetPath);
                if (graph == null)
                {
                    return Fail($"匯入失敗：無法建立暫存圖資產（{workingGraphAssetPath}）。");
                }
                workingGraphCreated = true;
                // ===== 變更結束 =====

                object graphImplementation = GetGraphImplementation(graph);
                if (graphImplementation == null)
                {
                    return Fail("匯入失敗：找不到 Graph 內部實作。");
                }

                MethodInfo createNodeModelMethod = graphImplementation.GetType().GetMethod("CreateNodeModel", BindingFlags.Instance | BindingFlags.Public);
                MethodInfo createWireMethod = graphImplementation
                    .GetType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(method => method.Name == "CreateWire" && method.GetParameters().Length == 3);

                if (createNodeModelMethod == null || createWireMethod == null)
                {
                    return Fail("匯入失敗：找不到 Graph Toolkit 建圖方法（CreateNodeModel/CreateWire）。");
                }

                var nodeById = new Dictionary<string, INode>();
                for (int nodeIndex = 0; nodeIndex < graphDto.nodes.Count; nodeIndex++)
                {
                    ExportNodeDto exportNode = graphDto.nodes[nodeIndex];
                    if (string.IsNullOrEmpty(exportNode.id))
                    {
                        return Fail("匯入失敗：節點 id 不可為空。");
                    }

                    if (nodeById.ContainsKey(exportNode.id))
                    {
                        return Fail($"匯入失敗：節點 id 重複（{exportNode.id}）。");
                    }

                    INode runtimeNode = CreateNodeByType(exportNode.type);
                    if (runtimeNode == null)
                    {
                        return Fail($"匯入失敗：不支援的節點型別（{exportNode.type}）。");
                    }

                    var position = new Vector2(180f + (300f * nodeIndex), 180f);
                    createNodeModelMethod.Invoke(graphImplementation, new object[] { runtimeNode, position });

                    if (!TryApplyNodeOptions(runtimeNode, exportNode, out string contentError))
                    {
                        return Fail(contentError);
                    }

                    nodeById[exportNode.id] = runtimeNode;
                }

                var connectedPairs = new HashSet<string>(StringComparer.Ordinal);
                foreach (ExportNodeDto exportNode in graphDto.nodes)
                {
                    if (!nodeById.TryGetValue(exportNode.id, out INode fromNode))
                    {
                        return Fail($"匯入失敗：找不到來源節點（{exportNode.id}）。");
                    }

                    if (exportNode.outputs != null && exportNode.outputs.Count > 0)
                    {
                        for (int i = 0; i < exportNode.outputs.Count; i++)
                        {
                            ExportNodeOutputDto output = exportNode.outputs[i];
                            string toNodeId = output != null ? output.toNodeId : string.Empty;
                            if (string.IsNullOrEmpty(toNodeId))
                            {
                                string portName = output != null ? output.portName : string.Empty;
                                return Fail($"匯入失敗：節點 `{exportNode.id}` 的輸出埠 `{portName}` 缺少 toNodeId。");
                            }

                            if (!nodeById.TryGetValue(toNodeId, out INode toNode))
                            {
                                return Fail($"匯入失敗：`{exportNode.id}` 指向不存在節點 `{toNodeId}`。");
                            }

                            string outputPortName = output != null && !string.IsNullOrEmpty(output.portName) ? output.portName : FlowPortName;
                            string pairKey = $"{exportNode.id}:{outputPortName}->{toNodeId}";
                            if (connectedPairs.Contains(pairKey))
                            {
                                continue;
                            }

                            IPort fromOutputPort = fromNode.GetOutputPortByName(outputPortName);
                            IPort toInputPort = toNode.GetInputPortByName(FlowPortName);
                            if (fromOutputPort == null || toInputPort == null)
                            {
                                return Fail($"匯入失敗：節點 `{exportNode.id}` 或 `{toNodeId}` 缺少對應 port（from={outputPortName}, to={FlowPortName}）。");
                            }

                            createWireMethod.Invoke(graphImplementation, new object[] { toInputPort, fromOutputPort, default(Hash128) });
                            connectedPairs.Add(pairKey);
                        }

                        continue;
                    }

                    List<string> nextIds = exportNode.nextIds ?? new List<string>();
                    // ===== 變更開始 =====
                    // 2026/02/13 Opsidanos (修改原因：Graph v1（線性流程）只允許 0 或 1 條 next；多分岔必須改用 Graph v2 的 choice/condition 輸出結構)
                    // 預期結果：匯入 v1 sidecar 時仍能擋下多 next 的不閉環資料，避免匯入後圖看似分岔但 Ink 不成立
                    if (nextIds.Count > 1)
                    {
                        return Fail($"匯入失敗：Graph v1（線性流程）只允許每個節點有 0 或 1 條 next。節點 `{exportNode.id}`（type={exportNode.type}）有 {nextIds.Count} 條：{string.Join(", ", nextIds)}。");
                    }
                    // ===== 變更結束 =====

                    foreach (string nextId in nextIds)
                    {
                        if (!nodeById.TryGetValue(nextId, out INode toNode))
                        {
                            return Fail($"匯入失敗：`{exportNode.id}` 指向不存在節點 `{nextId}`。");
                        }

                        string pairKey = $"{exportNode.id}:{FlowPortName}->{nextId}";
                        if (connectedPairs.Contains(pairKey))
                        {
                            continue;
                        }

                        IPort fromOutputPort = fromNode.GetOutputPortByName(FlowPortName);
                        IPort toInputPort = toNode.GetInputPortByName(FlowPortName);
                        if (fromOutputPort == null || toInputPort == null)
                        {
                            return Fail($"匯入失敗：節點 `{exportNode.id}` 或 `{nextId}` 缺少 Flow port。");
                        }

                        createWireMethod.Invoke(graphImplementation, new object[] { toInputPort, fromOutputPort, default(Hash128) });
                        connectedPairs.Add(pairKey);
                    }
                }

                GraphDatabase.SaveGraphIfDirty(graph);
                // ===== 變更開始 =====
                // 2026/02/08 Opsidanos (修改原因：匯入完成後用「刪舊檔 + 移動暫存檔」替換正式圖，避免同路徑建圖)
                // 預期結果：正式輸出仍是同名 `.inkfc`，但建圖過程不直接操作正式路徑
                if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(graphAssetPath) != null)
                {
                    if (!AssetDatabase.DeleteAsset(graphAssetPath))
                    {
                        return Fail($"匯入失敗：無法刪除既有圖資產（{graphAssetPath}）。");
                    }
                }

                string moveError = AssetDatabase.MoveAsset(workingGraphAssetPath, graphAssetPath);
                if (!string.IsNullOrEmpty(moveError))
                {
                    return Fail($"匯入失敗：無法將暫存圖替換到目標路徑（{graphAssetPath}）：{moveError}");
                }

                workingGraphCreated = false;
                // ===== 變更結束 =====
                AssetDatabase.Refresh();
                return InkFlowChartImportResult.Success(graphAssetPath);
            }
            catch (Exception exception)
            {
                return Fail($"匯入失敗（{flowchartJsonPath}）：{exception.Message}");
            }
        }

        private static string GetBaseName(string flowchartJsonPath)
        {
            string fileName = Path.GetFileName(flowchartJsonPath);
            if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(FlowchartJsonSuffix, StringComparison.OrdinalIgnoreCase))
            {
                return fileName.Substring(0, fileName.Length - FlowchartJsonSuffix.Length);
            }

            return Path.GetFileNameWithoutExtension(flowchartJsonPath);
        }

        private static InkFlowChartGraph CreateGraphWithMutedLogger(string graphAssetPath)
        {
            bool originalLogEnabled = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                return GraphDatabase.CreateGraph<InkFlowChartGraph>(graphAssetPath);
            }
            finally
            {
                Debug.unityLogger.logEnabled = originalLogEnabled;
            }
        }

        private static object GetGraphImplementation(InkFlowChartGraph graph)
        {
            FieldInfo graphImplementationField = typeof(Graph).GetField("m_Implementation", BindingFlags.Instance | BindingFlags.NonPublic);
            return graphImplementationField?.GetValue(graph);
        }

        private static INode CreateNodeByType(string nodeType)
        {
            if (string.Equals(nodeType, "start", StringComparison.OrdinalIgnoreCase))
            {
                return new InkFlowStartNode();
            }

            if (string.Equals(nodeType, "action", StringComparison.OrdinalIgnoreCase))
            {
                return new InkFlowActionNode();
            }

            if (string.Equals(nodeType, "comment", StringComparison.OrdinalIgnoreCase))
            {
                return new InkFlowCommentNode();
            }

            if (string.Equals(nodeType, "choice", StringComparison.OrdinalIgnoreCase))
            {
                return new InkFlowChoiceNode();
            }

            if (string.Equals(nodeType, "condition", StringComparison.OrdinalIgnoreCase))
            {
                return new InkFlowConditionNode();
            }

            return null;
        }

        private static bool TryApplyNodeOptions(INode node, ExportNodeDto exportNode, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (node == null || exportNode == null)
            {
                return true;
            }

            if (node is InkFlowActionNode)
            {
                return TrySetNodeOptionValue((Node)node, ActionContentOptionName, exportNode.content ?? string.Empty, out errorMessage);
            }

            if (node is InkFlowCommentNode)
            {
                return TrySetNodeOptionValue((Node)node, CommentNoteOptionName, exportNode.content ?? string.Empty, out errorMessage);
            }

            if (node is InkFlowChoiceNode)
            {
                if (exportNode.outputs == null || exportNode.outputs.Count < 1)
                {
                    errorMessage = $"匯入失敗：choice 節點 `{exportNode.id}` 至少需要 1 個 outputs。";
                    return false;
                }

                if (!TrySetNodeOptionValue((Node)node, "OutputCount", exportNode.outputs.Count, out errorMessage))
                {
                    return false;
                }

                string choiceTexts = BuildJoinedLabels(exportNode.outputs);
                if (!TrySetNodeOptionValue((Node)node, "ChoiceTexts", choiceTexts, out errorMessage))
                {
                    return false;
                }

                InkFlowChoiceMode mode = exportNode.choiceMode == "+" ? InkFlowChoiceMode.Repeatable : InkFlowChoiceMode.Once;
                if (!TrySetNodeOptionValue((Node)node, "ChoiceMode", mode, out errorMessage))
                {
                    return false;
                }

                ((Node)node).DefineNode();
                return true;
            }

            if (node is InkFlowConditionNode)
            {
                if (exportNode.outputs == null || exportNode.outputs.Count < 2)
                {
                    errorMessage = $"匯入失敗：condition 節點 `{exportNode.id}` 至少需要 2 個 outputs（含 else）。";
                    return false;
                }

                ExportNodeOutputDto lastOutput = exportNode.outputs[exportNode.outputs.Count - 1];
                if (lastOutput == null || !lastOutput.isElse)
                {
                    errorMessage = $"匯入失敗：condition 節點 `{exportNode.id}` 的最後一個 outputs 必須是 else（isElse=true）。";
                    return false;
                }

                if (!TrySetNodeOptionValue((Node)node, "OutputCount", exportNode.outputs.Count, out errorMessage))
                {
                    return false;
                }

                string conditionTexts = BuildJoinedConditions(exportNode.outputs);
                if (!TrySetNodeOptionValue((Node)node, "ConditionTexts", conditionTexts, out errorMessage))
                {
                    return false;
                }

                ((Node)node).DefineNode();
                return true;
            }

            return true;
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

            // ===== 變更開始 =====
            // 2026/02/21 Opsidanos (修改原因：GraphToolkit 的 enum option 內部使用 EnumValueReference；直接 TrySetValue<Enum> 會失敗，導致 Graph v2 choice 匯入測試卡住)
            // 預期結果：匯入 enum option（如 ChoiceMode）可正確寫入並讓 Graph v2 choice/condition 測試通過；寫入失敗仍回傳明確錯誤訊息
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
            // ===== 變更結束 =====
        }

        // ===== 變更開始 =====
        // 2026/02/21 Opsidanos (修改原因：GraphToolkit enum constant 的 Type 是 EnumValueReference；需要用反射建立 EnumValueReference(Enum) 再 TrySetValue<EnumValueReference>)
        // 預期結果：匯入 choice/condition 節點時可寫入 ChoiceMode，並維持失敗即回報（不做防禦性補洞）
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
        // ===== 變更結束 =====
    }
}
// ===== 變更結束 =====
