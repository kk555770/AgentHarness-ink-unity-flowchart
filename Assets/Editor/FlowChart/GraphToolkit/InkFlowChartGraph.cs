// ===== 變更開始 =====
// 2026/02/06 Opsidanos (修改原因：建立 Graph Toolkit 版 Flow Chart 最小圖模型，取代舊 IMGUI 編輯器主線)
// 預期結果：可在專案建立 .inkfc 圖資產，雙擊後由 Graph Toolkit 開啟，並提供基本圖驗證
using System;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor;

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
