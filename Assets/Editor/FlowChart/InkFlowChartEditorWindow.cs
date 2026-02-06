// ===== 變更開始 =====
// 2026/02/06 Opsidanos (修改原因：建立 Flow Chart EditorWindow MVP，並加入 `.ink + sidecar` 匯出匯入流程；修正新建資產無反應與 GUILayout 例外中斷；修正 Next IDs 可誤填標題導致連線失效)
// 預期結果：作者可在 Editor 裡編輯獨立資產圖表，且可直接做匯出與匯入還原；新建資產穩定可用；連線儲存統一為節點 id
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OpsidanosInk.Editor
{
    public sealed class InkFlowChartEditorWindow : EditorWindow
    {
        private const float ToolbarHeight = 40f;
        private const float CanvasWidth = 5000f;
        private const float CanvasHeight = 5000f;
        private const float NodeWidth = 280f;
        private const float NodeHeight = 200f;
        private const string FlowChartFolderPath = "Assets/FlowCharts";
        private const string DefaultFlowChartAssetName = "NewInkFlowChart.asset";

        private readonly Dictionary<string, InkFlowChartNode> nodeMap = new Dictionary<string, InkFlowChartNode>();
        private Vector2 canvasScroll = new Vector2(1200f, 800f);
        private InkFlowChartData currentData;
        private string pendingRemoveNodeId;

        [MenuItem("OpsidanosInk/Flow Chart/開啟 Flow Chart 編輯器")]
        private static void OpenWindow()
        {
            var window = GetWindow<InkFlowChartEditorWindow>("Ink Flow Chart");
            window.minSize = new Vector2(960f, 640f);
            window.Show();
        }

        [MenuItem("OpsidanosInk/Flow Chart/建立新 Flow Chart 資產")]
        private static void CreateFlowChartAsset()
        {
            bool success = TryCreateFlowChartAsset(out InkFlowChartData asset, out string path, out string error);
            if (!success)
            {
                Debug.LogError(error);
                return;
            }

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            Debug.Log($"[OpsidanosInk][FlowChart] 已建立新圖表資產：{path}");
        }

        private void OnGUI()
        {
            DrawToolbar();

            Rect canvasRect = new Rect(0f, ToolbarHeight, position.width, position.height - ToolbarHeight);
            DrawCanvas(canvasRect);

            if (!string.IsNullOrWhiteSpace(pendingRemoveNodeId))
            {
                RemoveNodeById(pendingRemoveNodeId);
                pendingRemoveNodeId = null;
            }
        }

        private void DrawToolbar()
        {
            Rect toolbarRect = new Rect(0f, 0f, position.width, ToolbarHeight);
            GUILayout.BeginArea(toolbarRect, EditorStyles.toolbar);
            try
            {
                GUILayout.BeginHorizontal();
                try
                {
                    var nextData = (InkFlowChartData)EditorGUILayout.ObjectField(currentData, typeof(InkFlowChartData), false, GUILayout.Width(320f));
                    if (nextData != currentData)
                    {
                        currentData = nextData;
                    }

                    if (GUILayout.Button("新建資產", EditorStyles.toolbarButton, GUILayout.Width(80f)))
                    {
                        CreateFlowChartAssetAndAssign();
                    }

                    if (GUILayout.Button("新增 Ink 節點", EditorStyles.toolbarButton, GUILayout.Width(100f)))
                    {
                        AddNode(InkFlowChartNodeType.Ink);
                    }

                    if (GUILayout.Button("新增 Tag 節點", EditorStyles.toolbarButton, GUILayout.Width(100f)))
                    {
                        AddNode(InkFlowChartNodeType.Tag);
                    }

                    if (GUILayout.Button("新增註解節點", EditorStyles.toolbarButton, GUILayout.Width(110f)))
                    {
                        AddNode(InkFlowChartNodeType.Comment);
                    }

                    if (GUILayout.Button("存檔", EditorStyles.toolbarButton, GUILayout.Width(60f)))
                    {
                        SaveCurrentData();
                    }

                    if (GUILayout.Button("匯出 Ink+Sidecar", EditorStyles.toolbarButton, GUILayout.Width(130f)))
                    {
                        ExportCurrentFlowChart();
                    }

                    if (GUILayout.Button("從 Ink+Sidecar 匯入", EditorStyles.toolbarButton, GUILayout.Width(150f)))
                    {
                        ImportFlowChartFromInkAndSidecar();
                    }

                    GUILayout.FlexibleSpace();
                }
                finally
                {
                    GUILayout.EndHorizontal();
                }
            }
            finally
            {
                GUILayout.EndArea();
            }
        }

        private void DrawCanvas(Rect canvasRect)
        {
            if (currentData == null)
            {
                EditorGUI.HelpBox(canvasRect, "請先指定或建立一個 Flow Chart 資產。", MessageType.Info);
                return;
            }

            RebuildNodeMap();

            Rect viewRect = new Rect(0f, 0f, CanvasWidth, CanvasHeight);
            canvasScroll = GUI.BeginScrollView(canvasRect, canvasScroll, viewRect);

            DrawGrid(viewRect);
            DrawConnections();

            BeginWindows();
            for (int i = 0; i < currentData.nodes.Count; i++)
            {
                InkFlowChartNode node = currentData.nodes[i];
                if (node == null)
                {
                    continue;
                }

                Rect oldRect = node.rect;
                Rect newRect = GUI.Window(i, oldRect, DrawNodeWindow, BuildNodeWindowTitle(node));
                if (newRect != oldRect)
                {
                    node.rect = newRect;
                    MarkCurrentDataDirty();
                }
            }
            EndWindows();

            GUI.EndScrollView();
        }

        private void DrawNodeWindow(int windowId)
        {
            if (currentData == null || windowId < 0 || windowId >= currentData.nodes.Count)
            {
                return;
            }

            InkFlowChartNode node = currentData.nodes[windowId];
            if (node == null)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();

            string title = EditorGUILayout.TextField("標題", node.title ?? string.Empty);
            string body = EditorGUILayout.TextArea(node.body ?? string.Empty, GUILayout.MinHeight(70f));
            string nextCsv = BuildNextNodeCsv(node.nextNodeIds);
            string editedCsv = EditorGUILayout.TextField("Next IDs", nextCsv);

            if (EditorGUI.EndChangeCheck())
            {
                node.title = title;
                node.body = body;
                node.nextNodeIds = ParseNextNodeCsv(editedCsv, currentData.nodes);
                MarkCurrentDataDirty();
            }

            if (GUILayout.Button("刪除節點"))
            {
                pendingRemoveNodeId = node.id;
            }

            GUI.DragWindow(new Rect(0f, 0f, 10000f, 22f));
        }

        private void DrawGrid(Rect viewRect)
        {
            Handles.BeginGUI();
            Color oldColor = Handles.color;
            Handles.color = new Color(1f, 1f, 1f, 0.05f);

            const float spacing = 40f;
            for (float x = 0f; x < viewRect.width; x += spacing)
            {
                Handles.DrawLine(new Vector3(x, 0f), new Vector3(x, viewRect.height));
            }

            for (float y = 0f; y < viewRect.height; y += spacing)
            {
                Handles.DrawLine(new Vector3(0f, y), new Vector3(viewRect.width, y));
            }

            Handles.color = oldColor;
            Handles.EndGUI();
        }

        private void DrawConnections()
        {
            Handles.BeginGUI();
            Color oldColor = Handles.color;
            Handles.color = Color.cyan;

            for (int i = 0; i < currentData.nodes.Count; i++)
            {
                InkFlowChartNode fromNode = currentData.nodes[i];
                if (fromNode == null || fromNode.nextNodeIds == null || fromNode.nextNodeIds.Count == 0)
                {
                    continue;
                }

                for (int j = 0; j < fromNode.nextNodeIds.Count; j++)
                {
                    string nextId = fromNode.nextNodeIds[j];
                    if (string.IsNullOrWhiteSpace(nextId))
                    {
                        continue;
                    }

                    if (!nodeMap.TryGetValue(nextId, out InkFlowChartNode toNode))
                    {
                        continue;
                    }

                    Vector3 start = new Vector3(fromNode.rect.xMax, fromNode.rect.center.y, 0f);
                    Vector3 end = new Vector3(toNode.rect.xMin, toNode.rect.center.y, 0f);
                    Vector3 startTan = start + Vector3.right * 70f;
                    Vector3 endTan = end + Vector3.left * 70f;
                    Handles.DrawBezier(start, end, startTan, endTan, Handles.color, null, 3f);
                }
            }

            Handles.color = oldColor;
            Handles.EndGUI();
        }

        private void AddNode(InkFlowChartNodeType nodeType)
        {
            if (currentData == null)
            {
                Debug.LogError("[OpsidanosInk][FlowChart] 請先指定或建立 Flow Chart 資產。", this);
                return;
            }

            string nodeId = $"node_{Guid.NewGuid():N}";
            var node = new InkFlowChartNode
            {
                id = nodeId,
                title = BuildDefaultTitle(nodeType, currentData.nodes.Count + 1),
                nodeType = nodeType,
                rect = new Rect(1200f + currentData.nodes.Count * 24f, 800f + currentData.nodes.Count * 20f, NodeWidth, NodeHeight),
                body = string.Empty,
                nextNodeIds = new List<string>()
            };

            currentData.nodes.Add(node);
            MarkCurrentDataDirty();
        }

        private void RemoveNodeById(string nodeId)
        {
            if (currentData == null || string.IsNullOrWhiteSpace(nodeId))
            {
                return;
            }

            int removeIndex = -1;
            for (int i = 0; i < currentData.nodes.Count; i++)
            {
                InkFlowChartNode node = currentData.nodes[i];
                if (node != null && node.id == nodeId)
                {
                    removeIndex = i;
                    break;
                }
            }

            if (removeIndex < 0)
            {
                return;
            }

            currentData.nodes.RemoveAt(removeIndex);

            for (int i = 0; i < currentData.nodes.Count; i++)
            {
                InkFlowChartNode node = currentData.nodes[i];
                if (node?.nextNodeIds == null)
                {
                    continue;
                }

                node.nextNodeIds.RemoveAll(id => id == nodeId);
            }

            MarkCurrentDataDirty();
        }

        private void SaveCurrentData()
        {
            if (currentData == null)
            {
                Debug.LogError("[OpsidanosInk][FlowChart] 沒有可存的 Flow Chart 資產。", this);
                return;
            }

            EditorUtility.SetDirty(currentData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[OpsidanosInk][FlowChart] 已儲存：{AssetDatabase.GetAssetPath(currentData)}", this);
        }

        private void CreateFlowChartAssetAndAssign()
        {
            bool success = TryCreateFlowChartAsset(out InkFlowChartData asset, out string path, out string error);
            if (!success)
            {
                Debug.LogError(error, this);
                return;
            }

            currentData = asset;
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            Repaint();
            Debug.Log($"[OpsidanosInk][FlowChart] 已建立並載入圖表資產：{path}", this);
        }

        private void ExportCurrentFlowChart()
        {
            if (currentData == null)
            {
                Debug.LogError("[OpsidanosInk][FlowChart] 匯出失敗：沒有可匯出的 Flow Chart 資產。", this);
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(currentData);
            string defaultFolder = ResolveDefaultAbsoluteFolder(assetPath);
            string defaultFileName = string.IsNullOrWhiteSpace(assetPath) ? "NewInkFlowChart" : Path.GetFileNameWithoutExtension(assetPath);

            string inkPath = EditorUtility.SaveFilePanel("匯出 Flow Chart 為 Ink", defaultFolder, defaultFileName, "ink");
            if (string.IsNullOrWhiteSpace(inkPath))
            {
                return;
            }

            bool success = InkFlowChartExporter.ExportToInkAndSidecar(currentData, inkPath, out string sidecarPath, out string error);
            if (!success)
            {
                Debug.LogError(error, this);
                return;
            }

            AssetDatabase.Refresh();
            Debug.Log($"[OpsidanosInk][FlowChart] 匯出完成：Ink={inkPath} | Sidecar={sidecarPath}", this);
        }

        private void ImportFlowChartFromInkAndSidecar()
        {
            if (currentData == null)
            {
                Debug.LogError("[OpsidanosInk][FlowChart] 匯入失敗：請先指定 Flow Chart 資產。", this);
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(currentData);
            string defaultFolder = ResolveDefaultAbsoluteFolder(assetPath);
            string inkPath = EditorUtility.OpenFilePanel("選擇要匯入的 Ink 檔", defaultFolder, "ink");
            if (string.IsNullOrWhiteSpace(inkPath))
            {
                return;
            }

            string sidecarPath = InkFlowChartImporter.BuildDefaultSidecarPath(inkPath);
            if (!File.Exists(sidecarPath))
            {
                sidecarPath = EditorUtility.OpenFilePanel("找不到預設 sidecar，請手動指定", Path.GetDirectoryName(inkPath), "json");
                if (string.IsNullOrWhiteSpace(sidecarPath))
                {
                    return;
                }
            }

            bool success = InkFlowChartImporter.ImportFromInkAndSidecar(inkPath, sidecarPath, currentData, out string error);
            if (!success)
            {
                Debug.LogError(error, this);
                return;
            }

            MarkCurrentDataDirty();
            AssetDatabase.SaveAssets();
            Repaint();
            Debug.Log($"[OpsidanosInk][FlowChart] 匯入完成：Ink={inkPath} | Sidecar={sidecarPath}", this);
        }

        private void MarkCurrentDataDirty()
        {
            if (currentData == null)
            {
                return;
            }

            EditorUtility.SetDirty(currentData);
        }

        private void RebuildNodeMap()
        {
            nodeMap.Clear();
            if (currentData == null || currentData.nodes == null)
            {
                return;
            }

            for (int i = 0; i < currentData.nodes.Count; i++)
            {
                InkFlowChartNode node = currentData.nodes[i];
                if (node == null || string.IsNullOrWhiteSpace(node.id))
                {
                    continue;
                }

                if (!nodeMap.ContainsKey(node.id))
                {
                    nodeMap.Add(node.id, node);
                }
            }
        }

        private static string ResolveDefaultAbsoluteFolder(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return Application.dataPath;
            }

            string directory = Path.GetDirectoryName(assetPath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                return Application.dataPath;
            }

            string projectRoot = Directory.GetCurrentDirectory();
            return Path.GetFullPath(Path.Combine(projectRoot, directory));
        }

        private static bool TryCreateFlowChartAsset(out InkFlowChartData asset, out string path, out string error)
        {
            asset = null;
            path = string.Empty;
            error = string.Empty;

            try
            {
                EnsureFlowChartFolder();
                string basePath = $"{FlowChartFolderPath}/{DefaultFlowChartAssetName}";
                path = AssetDatabase.GenerateUniqueAssetPath(basePath);
                if (string.IsNullOrWhiteSpace(path))
                {
                    error = "[OpsidanosInk][FlowChart] 建立資產失敗：產生的資產路徑為空。";
                    return false;
                }

                asset = CreateInstance<InkFlowChartData>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return true;
            }
            catch (Exception exception)
            {
                if (asset != null)
                {
                    DestroyImmediate(asset);
                    asset = null;
                }

                error = $"[OpsidanosInk][FlowChart] 建立資產失敗：{exception.Message}";
                return false;
            }
        }

        private static void EnsureFlowChartFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/FlowCharts"))
            {
                AssetDatabase.CreateFolder("Assets", "FlowCharts");
            }
        }

        private static string BuildNodeWindowTitle(InkFlowChartNode node)
        {
            string typeText = node.nodeType.ToString();
            string title = string.IsNullOrWhiteSpace(node.title) ? "(未命名)" : node.title;
            return $"{typeText} | {title}";
        }

        private static string BuildDefaultTitle(InkFlowChartNodeType nodeType, int index)
        {
            return nodeType switch
            {
                InkFlowChartNodeType.Ink => $"Ink 節點 {index}",
                InkFlowChartNodeType.Tag => $"Tag 節點 {index}",
                InkFlowChartNodeType.Comment => $"註解節點 {index}",
                _ => $"節點 {index}"
            };
        }

        private static string BuildNextNodeCsv(List<string> nextNodeIds)
        {
            if (nextNodeIds == null || nextNodeIds.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(",", nextNodeIds);
        }

        private static List<string> ParseNextNodeCsv(string csv, List<InkFlowChartNode> nodes)
        {
            var result = new List<string>();
            var titleToId = BuildUniqueTitleToIdMap(nodes);
            if (string.IsNullOrWhiteSpace(csv))
            {
                return result;
            }

            string[] parts = csv.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                string value = parts[i]?.Trim();
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                string normalizedId = NormalizeNextNodeId(value, nodes, titleToId);
                if (string.IsNullOrWhiteSpace(normalizedId))
                {
                    continue;
                }

                if (!result.Contains(normalizedId))
                {
                    result.Add(normalizedId);
                }
            }

            return result;
        }

        private static string NormalizeNextNodeId(string rawValue, List<InkFlowChartNode> nodes, Dictionary<string, string> titleToId)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return string.Empty;
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                InkFlowChartNode node = nodes[i];
                if (node == null || string.IsNullOrWhiteSpace(node.id))
                {
                    continue;
                }

                if (string.Equals(node.id, rawValue, StringComparison.Ordinal))
                {
                    return node.id;
                }
            }

            if (titleToId.TryGetValue(rawValue, out string idByTitle))
            {
                return idByTitle;
            }

            return rawValue;
        }

        private static Dictionary<string, string> BuildUniqueTitleToIdMap(List<InkFlowChartNode> nodes)
        {
            var firstSeen = new Dictionary<string, string>();
            var duplicateTitles = new HashSet<string>();

            for (int i = 0; i < nodes.Count; i++)
            {
                InkFlowChartNode node = nodes[i];
                if (node == null || string.IsNullOrWhiteSpace(node.id))
                {
                    continue;
                }

                string title = node.title?.Trim();
                if (string.IsNullOrWhiteSpace(title))
                {
                    continue;
                }

                if (!firstSeen.ContainsKey(title))
                {
                    firstSeen.Add(title, node.id);
                    continue;
                }

                duplicateTitles.Add(title);
            }

            foreach (string duplicateTitle in duplicateTitles)
            {
                firstSeen.Remove(duplicateTitle);
            }

            return firstSeen;
        }
    }
}
// ===== 變更結束 =====
