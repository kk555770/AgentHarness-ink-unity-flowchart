// ===== 變更開始 =====
// 2026/03/19 Opsidanos (修改原因：繼續薄化 Batch 3 的 GraphToolkit shell，將選取資產判斷、建立新圖路徑與資料夾解析從 editor commands 抽成獨立 helper)
// 預期結果：InkFlowChartEditorCommands 只保留 MenuItem 行為入口；資產路徑與建立策略則集中在單一 shell utility
using System;
using System.IO;
using UnityEditor;

namespace OpsidanosInk.Editor
{
    public static class InkFlowChartEditorAssetUtility
    {
        private const string DefaultGraphName = "NewInkFlowChartGraph";
        private const string DefaultGraphFolder = "Assets/FlowCharts";

        public static string BuildUniqueGraphAssetPath()
        {
            string targetFolder = ResolveCreateTargetFolder();
            string uniqueGraphName = $"{DefaultGraphName}_{Guid.NewGuid():N}";
            return AssetDatabase.GenerateUniqueAssetPath($"{targetFolder}/{uniqueGraphName}.{InkFlowChartGraph.AssetExtension}");
        }

        public static string GetSelectedFlowchartJsonPath()
        {
            return GetSelectedAssetPathBySuffix(".flowchart.json");
        }

        public static string GetSelectedGraphAssetPath()
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
