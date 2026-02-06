// ===== 變更開始 =====
// 2026/02/06 Opsidanos (修改原因：建立 Flow Chart 匯入匯出的 sidecar 結構，保存節點完整編輯資訊)
// 預期結果：`.ink` 僅負責文本，sidecar 可還原節點位置、類型、連線與標題
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpsidanosInk.Editor
{
    [Serializable]
    public sealed class InkFlowChartSidecar
    {
        public int version = 1;
        public string sourceInkFileName;
        public List<InkFlowChartSidecarNode> nodes = new List<InkFlowChartSidecarNode>();
    }

    [Serializable]
    public sealed class InkFlowChartSidecarNode
    {
        public string id;
        public string title;
        public InkFlowChartNodeType nodeType;
        public Rect rect;
        public string body;
        public List<string> nextNodeIds = new List<string>();
        public string knotName;
    }
}
// ===== 變更結束 =====
