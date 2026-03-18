// ===== 變更開始 =====
// 2026/02/08 Opsidanos (修改原因：補齊 Graph Toolkit 匯入 MVP，讓 `.flowchart.json + .ink` 可還原成 `.inkfc`)
// 預期結果：可從 sidecar 重新建立開始/流程/註解節點、內容與連線，失敗時回傳明確錯誤訊息
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OpsidanosInk.CanonicalGraph;
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
        // ===== 變更開始 =====
        // 2026/02/22 Opsidanos (修改原因：Flow port key 改由共用 schema 常數提供，避免匯出匯入 key 漂移)
        // 預期結果：匯入端與節點定義端共用同一個 Flow key
        private const string FlowPortName = CanonicalPortSemantics.Flow;
        // ===== 變更結束 =====
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

                    if (!TryApplyNodeOptions(runtimeNode, exportNode, graphDto, out string contentError))
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
                            string toInputPortName = output != null && !string.IsNullOrEmpty(output.toPortName) ? output.toPortName : FlowPortName;
                            string pairKey = $"{exportNode.id}:{outputPortName}->{toNodeId}:{toInputPortName}";
                            if (connectedPairs.Contains(pairKey))
                            {
                                continue;
                            }

                            IPort fromOutputPort = fromNode.GetOutputPortByName(outputPortName);
                            IPort toInputPort = toNode.GetInputPortByName(toInputPortName);
                            if (fromOutputPort == null || toInputPort == null)
                            {
                                return Fail($"匯入失敗：節點 `{exportNode.id}` 或 `{toNodeId}` 缺少對應 port（from={outputPortName}, to={toInputPortName}）。");
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
            // ===== 變更開始 =====
            // 2026/02/22 Opsidanos (修改原因：節點 type 字串改由共用 schema 常數比對，避免匯入判斷與匯出不一致)
            // 預期結果：匯入可穩定識別 start/action/comment/choice/condition
            // ===== 變更開始 =====
            // 2026/02/23 Opsidanos (修改原因：GraphToolkit 標題取決於類別名，匯入時改建立中文節點型別)
            // 預期結果：從 sidecar 匯入的節點在圖上直接顯示繁中節點名
            if (string.Equals(nodeType, CanonicalNodeKinds.Start, StringComparison.OrdinalIgnoreCase))
            {
                return new 開始();
            }

            // ===== 變更開始 =====
            // 2026/03/11 Opsidanos (修改原因：匯入器改由 schema helper 統一辨識 canonical `dialogue` 與 legacy sidecar token `action`)
            // 預期結果：未來即使 sidecar 同時出現 `dialogue` 與 `action`，匯入端也只需維護單一 mapping 規則
            if (CurrentFlowProjectionNaming.IsDialogueNodeType(nodeType))
            {
                return new 對話();
            }
            // ===== 變更結束 =====

            // ===== 變更開始 =====
            // 2026/02/25 Opsidanos (修改原因：新增動作節點型別，匯入時需可建立資料節點)
            // 預期結果：sidecar type=stageAction 可還原成中文可見節點「動作」
            if (string.Equals(nodeType, CanonicalNodeKinds.StageAction, StringComparison.OrdinalIgnoreCase))
            {
                return new 動作();
            }
            // ===== 變更結束 =====

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
            // ===== 變更結束 =====
            // ===== 變更結束 =====

            return null;
        }

        private static bool TryApplyNodeOptions(INode node, ExportNodeDto exportNode, ExportGraphDto graphDto, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (node == null || exportNode == null)
            {
                return true;
            }

            // ===== 變更開始 =====
            // 2026/02/25 Opsidanos (修改原因：對話節點與動作節點已拆分，匯入要分別還原各自內容欄位)
            // 預期結果：type=action 還原對話內容；type=stageAction 還原動作內容，且對話節點依 sidecar 自動撐開動作輸入埠數量
            if (node is InkFlowDialogueNode)
            {
                int maxActionInputOrder = GetMaxDialogueActionInputOrder(graphDto, exportNode.id);
                int actionInputCount = maxActionInputOrder >= 0
                    ? Mathf.Max(InkFlowNodeSchema.DialogueActionInputCountDefaultValue, maxActionInputOrder + 1)
                    : InkFlowNodeSchema.DialogueActionInputCountDefaultValue;

                if (!TrySetNodeOptionValue((Node)node, InkFlowNodeSchema.DialogueActionInputCountOptionName, actionInputCount, out errorMessage))
                {
                    return false;
                }

                if (!TrySetNodeOptionValue((Node)node, InkFlowNodeSchema.DialogueContentOptionName, exportNode.content ?? string.Empty, out errorMessage))
                {
                    return false;
                }

                ((Node)node).DefineNode();
                return true;
            }

            if (node is InkFlowStageActionNode)
            {
                return TrySetNodeOptionValue((Node)node, InkFlowNodeSchema.StageActionContentOptionName, exportNode.content ?? string.Empty, out errorMessage);
            }
            // ===== 變更結束 =====

            // ===== 變更開始 =====
            // 2026/02/22 Opsidanos (修改原因：匯入 option key 改由共用 schema 常數提供，避免 key 字串散落)
            // 預期結果：Comment/Choice/Condition 匯入讀寫 key 與節點定義保持一致
            if (node is InkFlowCommentNode)
            {
                return TrySetNodeOptionValue((Node)node, InkFlowNodeSchema.CommentNoteOptionName, exportNode.content ?? string.Empty, out errorMessage);
            }

            if (node is InkFlowChoiceNode)
            {
                if (exportNode.outputs == null || exportNode.outputs.Count < 1)
                {
                    errorMessage = $"匯入失敗：choice 節點 `{exportNode.id}` 至少需要 1 個 outputs。";
                    return false;
                }

                if (!TrySetNodeOptionValue((Node)node, InkFlowNodeSchema.ChoiceOutputCountOptionName, exportNode.outputs.Count, out errorMessage))
                {
                    return false;
                }

                string choiceTexts = BuildJoinedLabels(exportNode.outputs);
                if (!TrySetNodeOptionValue((Node)node, InkFlowNodeSchema.ChoiceTextsOptionName, choiceTexts, out errorMessage))
                {
                    return false;
                }

                InkFlowChoiceMode mode = exportNode.choiceMode == "+" ? InkFlowChoiceMode.Repeatable : InkFlowChoiceMode.Once;
                if (!TrySetNodeOptionValue((Node)node, InkFlowNodeSchema.ChoiceModeOptionName, mode, out errorMessage))
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

                if (!TrySetNodeOptionValue((Node)node, InkFlowNodeSchema.ConditionOutputCountOptionName, exportNode.outputs.Count, out errorMessage))
                {
                    return false;
                }

                string conditionTexts = BuildJoinedConditions(exportNode.outputs);
                if (!TrySetNodeOptionValue((Node)node, InkFlowNodeSchema.ConditionTextsOptionName, conditionTexts, out errorMessage))
                {
                    return false;
                }

                ((Node)node).DefineNode();
                return true;
            }
            // ===== 變更結束 =====

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

        // ===== 變更開始 =====
        // 2026/02/25 Opsidanos (修改原因：匯入對話節點時需依 sidecar 的 toPortName 還原 ActionIn 埠數量)
        // 預期結果：若 sidecar 有 ActionIn2，對話節點會自動建立至少 3 個動作輸入埠
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
        // ===== 變更結束 =====

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
