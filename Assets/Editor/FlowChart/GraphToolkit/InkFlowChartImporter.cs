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

                    if (!TryApplyNodeContent(runtimeNode, exportNode.content, out string contentError))
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

                    List<string> nextIds = exportNode.nextIds ?? new List<string>();
                    foreach (string nextId in nextIds)
                    {
                        if (!nodeById.TryGetValue(nextId, out INode toNode))
                        {
                            return Fail($"匯入失敗：`{exportNode.id}` 指向不存在節點 `{nextId}`。");
                        }

                        string pairKey = $"{exportNode.id}->{nextId}";
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

            return null;
        }

        private static bool TryApplyNodeContent(INode node, string nodeContent, out string errorMessage)
        {
            errorMessage = string.Empty;
            string optionName = string.Empty;

            if (node is InkFlowActionNode)
            {
                optionName = ActionContentOptionName;
            }
            else if (node is InkFlowCommentNode)
            {
                optionName = CommentNoteOptionName;
            }
            else
            {
                return true;
            }

            INodeOption option = ((Node)node).GetNodeOptionByName(optionName);
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

            MethodInfo trySetValueMethod = embeddedValueObject
                .GetType()
                .GetMethod("TrySetValue", BindingFlags.Instance | BindingFlags.Public)?
                .MakeGenericMethod(typeof(string));

            if (trySetValueMethod == null)
            {
                errorMessage = $"匯入失敗：節點選項 `{optionName}` 無法寫入內容。";
                return false;
            }

            object setResult = trySetValueMethod.Invoke(embeddedValueObject, new object[] { nodeContent ?? string.Empty });
            bool success = setResult is bool boolResult && boolResult;
            if (!success)
            {
                errorMessage = $"匯入失敗：節點選項 `{optionName}` 寫入失敗。";
                return false;
            }

            return true;
        }
    }
}
// ===== 變更結束 =====
