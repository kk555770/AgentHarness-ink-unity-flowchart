// ===== 變更開始 =====
// 2026/02/06 Opsidanos (修改原因：建立 Graph Toolkit 版 Flow Chart 最小圖模型，取代舊 IMGUI 編輯器主線)
// 預期結果：可在專案建立 .inkfc 圖資產，雙擊後由 Graph Toolkit 開啟，並提供基本圖驗證
using System;
using System.IO;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace OpsidanosInk.Editor
{
    [Serializable]
    [Graph(AssetExtension)]
    public sealed class InkFlowChartGraph : Graph
    {
        public const string AssetExtension = "inkfc";
        private const string DefaultGraphName = "NewInkFlowChartGraph";
        private const string DefaultGraphFolder = "Assets/FlowCharts";

        // ===== 變更開始 =====
        // 2026/02/08 Opsidanos (修改原因：避免同路徑同時出現可點選與子選單，統一為單一父入口)
        // 預期結果：Flow Chart Graph 只顯示一個父節點，建立功能移到子項「建立新圖」
        [MenuItem("Tools/OpsidanosInk/Flow Chart Graph/建立新圖")]
        // ===== 變更結束 =====
        private static void CreateGraphAsset()
        {
            // ===== 變更開始 =====
            // 2026/02/08 Opsidanos (修改原因：避免 Undo 後重用舊檔名導致同路徑資產警告，改為 GUID 檔名建立)
            // 預期結果：每次建立新圖都使用全新路徑，不再出現「same path as an existing asset」warning
            string targetFolder = ResolveCreateTargetFolder();
            string uniqueGraphName = $"{DefaultGraphName}_{Guid.NewGuid():N}";
            string uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath($"{targetFolder}/{uniqueGraphName}.{AssetExtension}");
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
            // ===== 變更結束 =====
        }

        // ===== 變更開始 =====
        // 2026/02/08 Opsidanos (修改原因：補上 Graph Toolkit 匯出入口，從 Tools/OpsidanosInk 可直接匯出選中 .inkfc)
        // 預期結果：選中 .inkfc 後可從 Tools/OpsidanosInk/Flow Chart Graph/匯出選中圖 產生 .ink 與 .flowchart.json
        [MenuItem("Tools/OpsidanosInk/Flow Chart Graph/匯出選中圖")]
        // ===== 變更結束 =====
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

        // ===== 變更開始 =====
        // 2026/02/08 Opsidanos (修改原因：限制匯出入口只在選中 `.inkfc` 時可點擊，避免誤觸)
        // 預期結果：未選中 `.inkfc` 時選單項目會停用
        [MenuItem("Tools/OpsidanosInk/Flow Chart Graph/匯出選中圖", true)]
        // ===== 變更結束 =====
        private static bool ValidateExportSelectedGraphAsset()
        {
            return !string.IsNullOrEmpty(GetSelectedGraphAssetPath());
        }

        // ===== 變更開始 =====
        // 2026/02/08 Opsidanos (修改原因：補上 Graph Toolkit 匯入入口，讓選中的 `.flowchart.json` 可還原成 `.inkfc`)
        // 預期結果：選中 `.flowchart.json` 後可從 Tools/OpsidanosInk/Flow Chart Graph/匯入選中匯出檔 執行匯入
        [MenuItem("Tools/OpsidanosInk/Flow Chart Graph/匯入選中匯出檔")]
        // ===== 變更結束 =====
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

        // ===== 變更開始 =====
        // 2026/02/08 Opsidanos (修改原因：限制匯入入口只在選中 `.flowchart.json` 時可點擊，避免誤觸)
        // 預期結果：未選中 `.flowchart.json` 時匯入選單會停用
        [MenuItem("Tools/OpsidanosInk/Flow Chart Graph/匯入選中匯出檔", true)]
        // ===== 變更結束 =====
        private static bool ValidateImportSelectedFlowchartJson()
        {
            return !string.IsNullOrEmpty(GetSelectedFlowchartJsonPath());
        }

        // ===== 變更開始 =====
        // 2026/02/08 Opsidanos (修改原因：共用選取資產路徑判斷，避免匯出與匯入入口重複維護)
        // 預期結果：可依副檔名取得選中資產路徑，供匯出 `.inkfc` 與匯入 `.flowchart.json` 共用
        private static string GetSelectedFlowchartJsonPath()
        {
            return GetSelectedAssetPathBySuffix(".flowchart.json");
        }

        private static string GetSelectedGraphAssetPath()
        {
            return GetSelectedAssetPathBySuffix($".{AssetExtension}");
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

        // ===== 變更開始 =====
        // 2026/02/08 Opsidanos (修改原因：建立新圖時統一解析目標資料夾，避免無效路徑導致 fallback 重建同路徑)
        // 預期結果：有選取資料夾或資產時優先建立於該目錄；無選取時保底建立於 `Assets/FlowCharts`
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
        // 2026/02/11 Opsidanos (修改原因：收斂重複區塊註解，移除多餘的獨立註解區塊)
        // 預期結果：保留單一區塊結束標記，程式邏輯完全不變
        // ===== 變更結束 =====

        public override void OnGraphChanged(GraphLogger graphLogger)
        {
            base.OnGraphChanged(graphLogger);

            int startNodeCount = GetNodes().Count(node => node is InkFlowStartNode);
            if (startNodeCount == 0)
            {
                graphLogger.LogError("Flow Chart 需要 1 個開始節點。", this);
                return;
            }

            if (startNodeCount > 1)
            {
                // ===== 變更開始 =====
                // 2026/02/13 Opsidanos (修改原因：Graph v2 規範要求 start 必須且只能 1 個；超過 1 個會讓匯出入口不唯一，閉環失效)
                // 預期結果：在圖上直接以 Error 提醒作者修正，而不是僅 warning 讓問題延後到匯出才爆
                graphLogger.LogError("Flow Chart 只允許 1 個開始節點。", this);
                // ===== 變更結束 =====
            }
        }
    }
}
// ===== 變更結束 =====
