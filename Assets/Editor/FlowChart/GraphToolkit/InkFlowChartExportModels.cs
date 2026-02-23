// ===== 變更開始 =====
// 2026/02/08 Opsidanos (修改原因：建立 Graph Toolkit 匯出資料模型，固定 `.flowchart.json` 最小結構)
// 預期結果：匯出流程可使用固定 DTO 結構輸出 version / graphName / startNodeId / nodes
using System;
using System.Collections.Generic;

namespace OpsidanosInk.Editor
{
    [Serializable]
    public sealed class ExportGraphDto
    {
        // ===== 變更開始 =====
        // 2026/02/13 Opsidanos (修改原因：加入 Graph v2 分岔節點資料結構（choice/condition），需要能保存「每個輸出埠對應到哪個節點」與其文字/條件)
        // 預期結果：`.flowchart.json` 可完整保存多輸出埠連線與分支資料，支援 `.inkfc ⇄ (.ink + .flowchart.json)` 可逆閉環
        public string version = "2.0";
        // ===== 變更結束 =====
        public string graphName = string.Empty;
        public string startNodeId = string.Empty;
        public List<ExportNodeDto> nodes = new List<ExportNodeDto>();
    }

    [Serializable]
    public sealed class ExportNodeDto
    {
        public string id = string.Empty;
        public string type = string.Empty;
        public string content = string.Empty;
        // ===== 變更開始 =====
        // 2026/02/22 Opsidanos (修改原因：Action 節點新增下拉類型，需保存到 sidecar 才能匯入後保留設定)
        // 預期結果：`.flowchart.json` 可保存 actionKind，避免匯入後遺失「對話/動作/自訂」選擇
        public string actionKind = string.Empty;
        // ===== 變更結束 =====
        public string choiceMode = string.Empty;
        public List<ExportNodeOutputDto> outputs = new List<ExportNodeOutputDto>();

        // v1 相容欄位：舊版只支援線性 nextIds（0 或 1）
        public List<string> nextIds = new List<string>();
    }

    [Serializable]
    public sealed class ExportNodeOutputDto
    {
        public string portName = string.Empty;
        public string toNodeId = string.Empty;
        public string label = string.Empty;
        public string condition = string.Empty;
        public bool isElse = false;
    }
}
// ===== 變更結束 =====
