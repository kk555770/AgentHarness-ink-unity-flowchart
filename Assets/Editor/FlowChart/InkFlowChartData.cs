// ===== 變更開始 =====
// 2026/02/06 Opsidanos (修改原因：建立 Flow Chart 的 ScriptableObject 容器，讓圖表資料獨立於場景)
// 預期結果：Flow Chart 可存成專案資產，切場景後仍可開啟與編輯
using System.Collections.Generic;
using UnityEngine;

namespace OpsidanosInk.Editor
{
    public sealed class InkFlowChartData : ScriptableObject
    {
        public int version = 1;
        public List<InkFlowChartNode> nodes = new List<InkFlowChartNode>();
    }
}
// ===== 變更結束 =====
