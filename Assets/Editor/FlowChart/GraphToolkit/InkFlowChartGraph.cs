// ===== 變更開始 =====
// 2026/02/06 Opsidanos (修改原因：建立 Graph Toolkit 版 Flow Chart 最小圖模型，取代舊 IMGUI 編輯器主線)
// 預期結果：可在專案建立 .inkfc 圖資產，雙擊後由 Graph Toolkit 開啟，並提供基本圖驗證
using System;
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

        // ===== 變更開始 =====
        // 2026/02/07 Opsidanos (修改原因：將 Flow Chart 工具入口統一到 Tools/OpsidanosInk，避免入口分散)
        // 預期結果：只從 Tools/OpsidanosInk/Flow Chart Graph 進入，不再保留舊路徑入口
        [MenuItem("Tools/OpsidanosInk/Flow Chart Graph")]
        // ===== 變更結束 =====
        private static void CreateGraphAsset()
        {
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<InkFlowChartGraph>(DefaultGraphName);
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

        private static string GetSelectedGraphAssetPath()
        {
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

            string expectedExtension = $".{AssetExtension}";
            if (!selectedPath.EndsWith(expectedExtension, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return selectedPath;
        }

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
                graphLogger.LogWarning("Flow Chart 只允許 1 個開始節點。", this);
            }
        }
    }
}
// ===== 變更結束 =====
