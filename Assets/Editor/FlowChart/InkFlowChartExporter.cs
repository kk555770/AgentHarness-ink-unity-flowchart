// ===== 變更開始 =====
// 2026/02/06 Opsidanos (修改原因：新增 Flow Chart 匯出器，將圖表轉成 `.ink` 與 sidecar 檔；修正連線容錯與控制字元清理)
// 預期結果：開發者可直接把 Editor 圖表匯出成可編譯文本，且保留完整還原資訊；匯出文本不含非必要控制碼
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace OpsidanosInk.Editor
{
    public static class InkFlowChartExporter
    {
        public const string SidecarSuffix = ".flowchart.json";

        public static bool ExportToInkAndSidecar(InkFlowChartData data, string inkPath, out string sidecarPath, out string error)
        {
            sidecarPath = string.Empty;
            error = string.Empty;

            if (data == null)
            {
                error = "[OpsidanosInk][FlowChart] 匯出失敗：Flow Chart 資料為 null。";
                return false;
            }

            if (string.IsNullOrWhiteSpace(inkPath))
            {
                error = "[OpsidanosInk][FlowChart] 匯出失敗：Ink 輸出路徑為空。";
                return false;
            }

            if (data.nodes == null)
            {
                error = "[OpsidanosInk][FlowChart] 匯出失敗：節點清單為 null。";
                return false;
            }

            try
            {
                string folderPath = Path.GetDirectoryName(inkPath);
                if (!string.IsNullOrWhiteSpace(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                Dictionary<string, string> knotNameMap = BuildKnotNameMap(data.nodes);
                string inkText = BuildInkText(data.nodes, knotNameMap);
                File.WriteAllText(inkPath, inkText, Encoding.UTF8);

                sidecarPath = BuildSidecarPath(inkPath);
                InkFlowChartSidecar sidecar = BuildSidecar(data.nodes, Path.GetFileName(inkPath), knotNameMap);
                string sidecarJson = JsonUtility.ToJson(sidecar, true);
                File.WriteAllText(sidecarPath, sidecarJson, Encoding.UTF8);

                return true;
            }
            catch (Exception exception)
            {
                error = $"[OpsidanosInk][FlowChart] 匯出失敗：{exception.Message}";
                return false;
            }
        }

        private static string BuildSidecarPath(string inkPath)
        {
            string folderPath = Path.GetDirectoryName(inkPath) ?? string.Empty;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inkPath);
            return Path.Combine(folderPath, $"{fileNameWithoutExtension}{SidecarSuffix}");
        }

        private static Dictionary<string, string> BuildKnotNameMap(List<InkFlowChartNode> nodes)
        {
            var map = new Dictionary<string, string>();
            var usedNames = new HashSet<string>();

            for (int i = 0; i < nodes.Count; i++)
            {
                InkFlowChartNode node = nodes[i];
                if (node == null || string.IsNullOrWhiteSpace(node.id))
                {
                    continue;
                }

                if (map.ContainsKey(node.id))
                {
                    continue;
                }

                string knotName = BuildSafeKnotName(node.id, usedNames);
                map.Add(node.id, knotName);
            }

            return map;
        }

        private static string BuildSafeKnotName(string nodeId, HashSet<string> usedNames)
        {
            var builder = new StringBuilder("k_");
            for (int i = 0; i < nodeId.Length; i++)
            {
                char character = nodeId[i];
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(character);
                }
                else
                {
                    builder.Append('_');
                }
            }

            string baseName = builder.ToString();
            string finalName = baseName;
            int suffix = 1;
            while (usedNames.Contains(finalName))
            {
                finalName = $"{baseName}_{suffix}";
                suffix++;
            }

            usedNames.Add(finalName);
            return finalName;
        }

        private static string BuildInkText(List<InkFlowChartNode> nodes, Dictionary<string, string> knotNameMap)
        {
            Dictionary<string, string> uniqueTitleToIdMap = BuildUniqueTitleToIdMap(nodes);
            var builder = new StringBuilder();
            builder.AppendLine("// OpsidanosInk FlowChart Export");
            builder.AppendLine("// 此檔案由 Flow Chart Editor 自動產生");
            builder.AppendLine();

            for (int i = 0; i < nodes.Count; i++)
            {
                InkFlowChartNode node = nodes[i];
                if (node == null || string.IsNullOrWhiteSpace(node.id))
                {
                    continue;
                }

                if (!knotNameMap.TryGetValue(node.id, out string knotName))
                {
                    continue;
                }

                builder.AppendLine($"=== {knotName} ===");
                AppendNodeBody(builder, node);
                AppendNodeFlow(builder, node, knotNameMap, uniqueTitleToIdMap);
                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static void AppendNodeBody(StringBuilder builder, InkFlowChartNode node)
        {
            string body = SanitizeTextForInk(node.body ?? string.Empty);
            if (string.IsNullOrWhiteSpace(body))
            {
                builder.AppendLine("// (空內容)");
                return;
            }

            string[] lines = body.Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (node.nodeType == InkFlowChartNodeType.Tag)
                {
                    builder.AppendLine($"# {line}");
                    continue;
                }

                if (node.nodeType == InkFlowChartNodeType.Comment)
                {
                    builder.AppendLine($"// {line}");
                    continue;
                }

                builder.AppendLine(line);
            }
        }

        private static void AppendNodeFlow(
            StringBuilder builder,
            InkFlowChartNode node,
            Dictionary<string, string> knotNameMap,
            Dictionary<string, string> uniqueTitleToIdMap)
        {
            if (node.nextNodeIds == null || node.nextNodeIds.Count == 0)
            {
                builder.AppendLine("-> END");
                return;
            }

            var validTargets = new List<string>();
            var seenTargets = new HashSet<string>();
            for (int i = 0; i < node.nextNodeIds.Count; i++)
            {
                string nextToken = node.nextNodeIds[i]?.Trim();
                if (string.IsNullOrWhiteSpace(nextToken))
                {
                    continue;
                }

                if (knotNameMap.TryGetValue(nextToken, out string nextKnotById))
                {
                    if (!seenTargets.Contains(nextKnotById))
                    {
                        validTargets.Add(nextKnotById);
                        seenTargets.Add(nextKnotById);
                    }
                    continue;
                }

                if (uniqueTitleToIdMap.TryGetValue(nextToken, out string idByTitle) &&
                    knotNameMap.TryGetValue(idByTitle, out string nextKnotByTitle) &&
                    !seenTargets.Contains(nextKnotByTitle))
                {
                    validTargets.Add(nextKnotByTitle);
                    seenTargets.Add(nextKnotByTitle);
                }
            }

            if (validTargets.Count == 0)
            {
                builder.AppendLine("-> END");
                return;
            }

            if (validTargets.Count == 1)
            {
                builder.AppendLine($"-> {validTargets[0]}");
                return;
            }

            for (int i = 0; i < validTargets.Count; i++)
            {
                builder.AppendLine($"* -> {validTargets[i]}");
            }
        }

        private static Dictionary<string, string> BuildUniqueTitleToIdMap(List<InkFlowChartNode> nodes)
        {
            var map = new Dictionary<string, string>();
            var firstSeen = new Dictionary<string, string>();
            var duplicatedTitles = new HashSet<string>();

            for (int i = 0; i < nodes.Count; i++)
            {
                InkFlowChartNode node = nodes[i];
                if (node == null || string.IsNullOrWhiteSpace(node.id))
                {
                    continue;
                }

                string title = node.title?.Trim();
                if (string.IsNullOrWhiteSpace(title))
                {
                    continue;
                }

                if (!firstSeen.ContainsKey(title))
                {
                    firstSeen.Add(title, node.id);
                    continue;
                }

                duplicatedTitles.Add(title);
            }

            foreach (KeyValuePair<string, string> pair in firstSeen)
            {
                if (duplicatedTitles.Contains(pair.Key))
                {
                    continue;
                }

                map.Add(pair.Key, pair.Value);
            }

            return map;
        }

        private static string SanitizeTextForInk(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                char character = input[i];
                if (character == '\n' || character == '\r' || character == '\t')
                {
                    builder.Append(character);
                    continue;
                }

                if (char.IsControl(character))
                {
                    continue;
                }

                builder.Append(character);
            }

            return builder.ToString();
        }

        private static InkFlowChartSidecar BuildSidecar(List<InkFlowChartNode> nodes, string sourceInkFileName, Dictionary<string, string> knotNameMap)
        {
            Dictionary<string, string> uniqueTitleToIdMap = BuildUniqueTitleToIdMap(nodes);
            var sidecar = new InkFlowChartSidecar
            {
                version = 1,
                sourceInkFileName = sourceInkFileName,
                nodes = new List<InkFlowChartSidecarNode>()
            };

            for (int i = 0; i < nodes.Count; i++)
            {
                InkFlowChartNode node = nodes[i];
                if (node == null || string.IsNullOrWhiteSpace(node.id))
                {
                    continue;
                }

                knotNameMap.TryGetValue(node.id, out string knotName);
                sidecar.nodes.Add(new InkFlowChartSidecarNode
                {
                    id = node.id,
                    title = node.title,
                    nodeType = node.nodeType,
                    rect = node.rect,
                    body = SanitizeTextForInk(node.body ?? string.Empty),
                    nextNodeIds = NormalizeNextNodeIds(node.nextNodeIds, knotNameMap, uniqueTitleToIdMap),
                    knotName = knotName
                });
            }

            return sidecar;
        }

        private static List<string> NormalizeNextNodeIds(
            List<string> nextNodeIds,
            Dictionary<string, string> knotNameMap,
            Dictionary<string, string> uniqueTitleToIdMap)
        {
            var result = new List<string>();
            if (nextNodeIds == null || nextNodeIds.Count == 0)
            {
                return result;
            }

            for (int i = 0; i < nextNodeIds.Count; i++)
            {
                string token = nextNodeIds[i]?.Trim();
                if (string.IsNullOrWhiteSpace(token))
                {
                    continue;
                }

                if (knotNameMap.ContainsKey(token))
                {
                    if (!result.Contains(token))
                    {
                        result.Add(token);
                    }
                    continue;
                }

                if (uniqueTitleToIdMap.TryGetValue(token, out string idByTitle))
                {
                    if (!result.Contains(idByTitle))
                    {
                        result.Add(idByTitle);
                    }
                    continue;
                }

                if (!result.Contains(token))
                {
                    result.Add(token);
                }
            }

            return result;
        }
    }
}
// ===== 變更結束 =====
