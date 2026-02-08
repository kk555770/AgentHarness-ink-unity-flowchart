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
        public string version = "1.0";
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
        public List<string> nextIds = new List<string>();
    }
}
// ===== 變更結束 =====
