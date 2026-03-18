// ===== 變更開始 =====
// 2026/02/06 Opsidanos (修改原因：建立 Graph Toolkit 版 Flow Chart 最小圖模型，取代舊 IMGUI 編輯器主線)
// 預期結果：可在專案建立 .inkfc 圖資產，雙擊後由 Graph Toolkit 開啟，並提供基本圖驗證
using System;
using Unity.GraphToolkit.Editor;

namespace OpsidanosInk.Editor
{
    [Serializable]
    [Graph(AssetExtension)]
    public sealed class InkFlowChartGraph : Graph
    {
        public const string AssetExtension = "inkfc";

        public override void OnGraphChanged(GraphLogger graphLogger)
        {
            base.OnGraphChanged(graphLogger);
            // ===== 變更開始 =====
            // 2026/03/18 Opsidanos (修改原因：開始落地 Batch 3，讓 InkFlowChartGraph 只保留最薄的 Graph shell 入口)
            // 預期結果：Graph 類別不再同時承擔 MenuItem 與驗證細節；OnGraphChanged 只負責轉交 shell validator
            InkFlowChartGraphShellValidator.Validate(this, graphLogger);
            // ===== 變更結束 =====
        }
    }
}
// ===== 變更結束 =====
