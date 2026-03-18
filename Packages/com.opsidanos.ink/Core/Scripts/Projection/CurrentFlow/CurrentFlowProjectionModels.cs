// ===== 變更開始 =====
// 2026/03/18 Opsidanos (修改原因：把 current projection 的 shared DTO 從 GraphToolkit editor 殼抽到 core seam，讓 importer/exporter/tests 共用同一份 sidecar contract)
// 預期結果：`.flowchart.json` 的 DTO 不再被誤認為 GraphToolkit 私有模型；後續 validator 與未來 bridge 可共用同一份 current projection 資料結構
using System;
using System.Collections.Generic;

namespace OpsidanosInk.CanonicalGraph
{
    [Serializable]
    public sealed class ExportGraphDto
    {
        public string version = "2.0";
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
        public string actionKind = string.Empty;
        public string choiceMode = string.Empty;
        public List<ExportNodeOutputDto> outputs = new List<ExportNodeOutputDto>();
        public List<string> nextIds = new List<string>();
    }

    [Serializable]
    public sealed class ExportNodeOutputDto
    {
        public string portName = string.Empty;
        public string toNodeId = string.Empty;
        public string toPortName = string.Empty;
        public string label = string.Empty;
        public string condition = string.Empty;
        public bool isElse = false;
    }
}
// ===== 變更結束 =====
