// ===== 變更開始 =====
// 2026/02/06 Opsidanos (修改原因：新增 Flow Chart 匯入器，讀取 `.ink + sidecar` 還原可編輯圖表)
// 預期結果：讀檔後可回到原本節點布局與連線，繼續在 Editor 內修改
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpsidanosInk.Editor
{
    public static class InkFlowChartImporter
    {
        public static bool ImportFromInkAndSidecar(string inkPath, string sidecarPath, InkFlowChartData targetData, out string error)
        {
            error = string.Empty;

            if (targetData == null)
            {
                error = "[OpsidanosInk][FlowChart] 匯入失敗：目標 Flow Chart 資料為 null。";
                return false;
            }

            if (string.IsNullOrWhiteSpace(inkPath))
            {
                error = "[OpsidanosInk][FlowChart] 匯入失敗：Ink 路徑為空。";
                return false;
            }

            if (string.IsNullOrWhiteSpace(sidecarPath))
            {
                error = "[OpsidanosInk][FlowChart] 匯入失敗：sidecar 路徑為空。";
                return false;
            }

            if (!File.Exists(inkPath))
            {
                error = $"[OpsidanosInk][FlowChart] 匯入失敗：找不到 Ink 檔案：{inkPath}";
                return false;
            }

            if (!File.Exists(sidecarPath))
            {
                error = $"[OpsidanosInk][FlowChart] 匯入失敗：找不到 sidecar 檔案：{sidecarPath}";
                return false;
            }

            try
            {
                string sidecarJson = File.ReadAllText(sidecarPath, Encoding.UTF8);
                InkFlowChartSidecar sidecar = UnityEngine.JsonUtility.FromJson<InkFlowChartSidecar>(sidecarJson);
                if (sidecar == null || sidecar.nodes == null)
                {
                    error = "[OpsidanosInk][FlowChart] 匯入失敗：sidecar 內容無法解析。";
                    return false;
                }

                targetData.version = sidecar.version <= 0 ? 1 : sidecar.version;
                targetData.nodes.Clear();

                for (int i = 0; i < sidecar.nodes.Count; i++)
                {
                    InkFlowChartSidecarNode sidecarNode = sidecar.nodes[i];
                    if (sidecarNode == null || string.IsNullOrWhiteSpace(sidecarNode.id))
                    {
                        continue;
                    }

                    targetData.nodes.Add(new InkFlowChartNode
                    {
                        id = sidecarNode.id,
                        title = sidecarNode.title,
                        nodeType = sidecarNode.nodeType,
                        rect = sidecarNode.rect,
                        body = sidecarNode.body,
                        nextNodeIds = sidecarNode.nextNodeIds == null ? new List<string>() : new List<string>(sidecarNode.nextNodeIds)
                    });
                }

                return true;
            }
            catch (Exception exception)
            {
                error = $"[OpsidanosInk][FlowChart] 匯入失敗：{exception.Message}";
                return false;
            }
        }

        public static string BuildDefaultSidecarPath(string inkPath)
        {
            string folderPath = Path.GetDirectoryName(inkPath) ?? string.Empty;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inkPath);
            return Path.Combine(folderPath, $"{fileNameWithoutExtension}{InkFlowChartExporter.SidecarSuffix}");
        }
    }
}
// ===== 變更結束 =====
