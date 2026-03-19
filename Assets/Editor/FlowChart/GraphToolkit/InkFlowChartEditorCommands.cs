// ===== 變更開始 =====
// 2026/03/18 Opsidanos (修改原因：開始落地 Batch 3，將 GraphToolkit shell 的選單入口與資產路徑處理從 InkFlowChartGraph 抽成獨立 editor commands)
// 預期結果：InkFlowChartGraph 回到圖資產殼；建立、匯出、匯入與選取路徑判斷則集中在 editor shell command 檔
using System;
using UnityEditor;
using UnityEngine;
using Unity.GraphToolkit.Editor;

namespace OpsidanosInk.Editor
{
    public static class InkFlowChartEditorCommands
    {
        [MenuItem("Tools/OpsidanosInk/Flow Chart Graph/建立新圖")]
        private static void CreateGraphAsset()
        {
            // ===== 變更開始 =====
            // 2026/03/19 Opsidanos (修改原因：繼續薄化 Batch 3，將建立新圖的資產路徑策略抽到 editor asset utility)
            // 預期結果：command 檔只保留「按下建立新圖時做什麼」，不再自己保管路徑與資料夾決策
            string uniqueAssetPath = InkFlowChartEditorAssetUtility.BuildUniqueGraphAssetPath();
            // ===== 變更結束 =====
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
            // ===== 變更開始 =====
            // 2026/03/19 Opsidanos (修改原因：繼續薄化 Batch 3，將選取 `.inkfc` 路徑判斷抽到 editor asset utility)
            // 預期結果：匯出 command 只保留動作本身，不再自己保管選取路徑 helper
            string graphAssetPath = InkFlowChartEditorAssetUtility.GetSelectedGraphAssetPath();
            // ===== 變更結束 =====
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
            return !string.IsNullOrEmpty(InkFlowChartEditorAssetUtility.GetSelectedGraphAssetPath());
        }

        [MenuItem("Tools/OpsidanosInk/Flow Chart Graph/匯入選中匯出檔")]
        private static void ImportSelectedFlowchartJson()
        {
            // ===== 變更開始 =====
            // 2026/03/19 Opsidanos (修改原因：繼續薄化 Batch 3，將選取 `.flowchart.json` 路徑判斷抽到 editor asset utility)
            // 預期結果：匯入 command 只保留動作本身，不再自己保管選取路徑 helper
            string flowchartJsonPath = InkFlowChartEditorAssetUtility.GetSelectedFlowchartJsonPath();
            // ===== 變更結束 =====
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
            return !string.IsNullOrEmpty(InkFlowChartEditorAssetUtility.GetSelectedFlowchartJsonPath());
        }
    }
}
// ===== 變更結束 =====
