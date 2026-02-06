// ===== 變更開始 =====
// 2026/02/06 Opsidanos (修改原因：建立 Flow Chart 最小節點資料模型，供 EditorWindow 操作與後續匯入匯出共用)
// 預期結果：每個節點都能保存類型、位置、內容與下一節點連線資訊
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpsidanosInk.Editor
{
    public enum InkFlowChartNodeType
    {
        Ink = 0,
        Tag = 1,
        Comment = 2
    }

    [Serializable]
    public sealed class InkFlowChartNode
    {
        public string id;
        public string title;
        public InkFlowChartNodeType nodeType;
        public Rect rect;
        public string body;
        public List<string> nextNodeIds = new List<string>();
    }
}
// ===== 變更結束 =====
