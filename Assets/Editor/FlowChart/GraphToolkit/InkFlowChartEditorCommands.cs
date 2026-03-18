// ===== 變更開始 =====
// 2026/03/18 Opsidanos (修改原因：開始落地 Batch 3，將 GraphToolkit shell 的選單入口與資產路徑處理從 InkFlowChartGraph 抽成獨立 editor commands)
// 預期結果：InkFlowChartGraph 回到圖資產殼；建立、匯出、匯入與選取路徑判斷則集中在 editor shell command 檔
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.GraphToolkit.Editor;

namespace OpsidanosInk.Editor
{
    public static class InkFlowChartEditorCommands
    {
        private const string DefaultGraphName = "NewInkFlowChartGraph";
        private const string DefaultGraphFolder = "Assets/FlowCharts";

        [MenuItem("Tools/OpsidanosInk/Flow Chart Graph/建立新圖")]
        private static void CreateGraphAsset()
        {
            string targetFolder = ResolveCreateTargetFolder();
            string uniqueGraphName = $"{DefaultGraphName}_{Guid.NewGuid():N}";
            string uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath($"{targetFolder}/{uniqueGraphName}.{InkFlowChartGraph.AssetExtension}");
            InkFlowChartGraph createdGraph = GraphDatabase.CreateGraph<InkFlowChartGraph>(uniqueAssetPath);
            if (createdGraph != null)
            {
                UnityEngine.Object createdGraphAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(uniqueAssetPath);
                if (createdGraphAsset != null)
                {
                    Selection.activeObject = createdGraphAsset;
                    EditorGUIUtility.PingObject(createdGraphAsset);
                }
            }
        }

        [MenuItem("Tools/OpsidanosInk/Flow Chart Graph/匯出選中圖")]
        private static void ExportSelectedGraphAsset()
        {
            string graphAssetPath = GetSelectedGraphAssetPath();
            InkFlowChartExportResult exportResult = InkFlowChartExporter.ExportGraphAsset(graphAssetPath);
            if (exportResult.success)
            {
                Debug.Log($"Flow Chart 匯出完成：{exportResult.inkOutputPath} / {exportResult.jsonOutputPath}");
                AssetDatabase.Refresh();
                return;
            }

            Debug.LogError($"Flow Chart 匯出失敗：{exportResult.errorMessage}");
        }

        [MenuItem("Tools/OpsidanosInk/Flow Chart Graph/匯出選中圖", true)]
        private static bool ValidateExportSelectedGraphAsset()
        {
            return !string.IsNullOrEmpty(GetSelectedGraphAssetPath());
        }

        [MenuItem("Tools/OpsidanosInk/Flow Chart Graph/匯入選中匯出檔")]
        private static void ImportSelectedFlowchartJson()
        {
            string flowchartJsonPath = GetSelectedFlowchartJsonPath();
            InkFlowChartImportResult importResult = InkFlowChartImporter.ImportFromFlowchartJson(flowchartJsonPath);
            if (importResult.success)
            {
                Debug.Log($"Flow Chart 匯入完成：{importResult.graphAssetPath}");
                AssetDatabase.Refresh();
                return;
            }

            Debug.LogError($"Flow Chart 匯入失敗：{importResult.errorMessage}");
        }

        [MenuItem("Tools/OpsidanosInk/Flow Chart Graph/匯入選中匯出檔", true)]
        private static bool ValidateImportSelectedFlowchartJson()
        {
            return !string.IsNullOrEmpty(GetSelectedFlowchartJsonPath());
        }

        private static string GetSelectedFlowchartJsonPath()
        {
            return GetSelectedAssetPathBySuffix(".flowchart.json");
        }

        private static string GetSelectedGraphAssetPath()
        {
            return GetSelectedAssetPathBySuffix($".{InkFlowChartGraph.AssetExtension}");
        }

        private static string GetSelectedAssetPathBySuffix(string suffix)
        {
            if (string.IsNullOrEmpty(suffix))
            {
                return string.Empty;
            }

            UnityEngine.Object selectedObject = Selection.activeObject;
            if (selectedObject == null)
            {
                return string.Empty;
            }

            string selectedPath = AssetDatabase.GetAssetPath(selectedObject);
            if (string.IsNullOrEmpty(selectedPath))
            {
                return string.Empty;
            }

            if (!selectedPath.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return selectedPath;
        }

        private static string ResolveCreateTargetFolder()
        {
            EnsureDefaultGraphFolderExists();

            UnityEngine.Object selectedObject = Selection.activeObject;
            if (selectedObject == null)
            {
                return DefaultGraphFolder;
            }

            string selectedPath = AssetDatabase.GetAssetPath(selectedObject);
            if (string.IsNullOrEmpty(selectedPath))
            {
                return DefaultGraphFolder;
            }

            if (AssetDatabase.IsValidFolder(selectedPath))
            {
                return selectedPath;
            }

            string directoryPath = Path.GetDirectoryName(selectedPath);
            if (string.IsNullOrEmpty(directoryPath))
            {
                return DefaultGraphFolder;
            }

            directoryPath = directoryPath.Replace("\\", "/");
            if (AssetDatabase.IsValidFolder(directoryPath))
            {
                return directoryPath;
            }

            return DefaultGraphFolder;
        }

        private static void EnsureDefaultGraphFolderExists()
        {
            if (AssetDatabase.IsValidFolder(DefaultGraphFolder))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder("Assets"))
            {
                return;
            }

            AssetDatabase.CreateFolder("Assets", "FlowCharts");
        }
    }
}
// ===== 變更結束 =====
