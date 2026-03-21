// ===== 變更開始 =====
// 2026/02/08 Opsidanos (修改原因：補齊 Graph Toolkit 匯入 MVP，讓 `.flowchart.json + .ink` 可還原成 `.inkfc`)
// 預期結果：可從 sidecar 重新建立開始/流程/註解節點、內容與連線，失敗時回傳明確錯誤訊息
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpsidanosInk.CanonicalGraph;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace OpsidanosInk.Editor
{
    public readonly struct InkFlowChartImportResult
    {
        public bool success { get; }
        public string graphAssetPath { get; }
        public string errorMessage { get; }

        private InkFlowChartImportResult(bool success, string graphAssetPath, string errorMessage)
        {
            this.success = success;
            this.graphAssetPath = graphAssetPath ?? string.Empty;
            this.errorMessage = errorMessage ?? string.Empty;
        }

        public static InkFlowChartImportResult Success(string graphAssetPath)
        {
            return new InkFlowChartImportResult(true, graphAssetPath, string.Empty);
        }

        public static InkFlowChartImportResult Failure(string graphAssetPath, string errorMessage)
        {
            return new InkFlowChartImportResult(false, graphAssetPath, errorMessage);
        }
    }

    public static class InkFlowChartImporter
    {
        private const string FlowchartJsonSuffix = ".flowchart.json";

        public static InkFlowChartImportResult ImportFromFlowchartJson(string flowchartJsonPath)
        {
            string graphAssetPath = string.Empty;
            // ===== 變更開始 =====
            // 2026/02/08 Opsidanos (修改原因：避免直接在目標路徑刪除後重建，先用暫存圖資產建圖再替換，降低 Undo 卡住風險)
            // 預期結果：匯入過程不再直接觸發「同路徑新建資產」流程，失敗時只清理暫存圖
            string workingGraphAssetPath = string.Empty;
            bool workingGraphCreated = false;
            // ===== 變更結束 =====

            InkFlowChartImportResult Fail(string errorMessage)
            {
                if (workingGraphCreated && !string.IsNullOrEmpty(workingGraphAssetPath) && AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(workingGraphAssetPath) != null)
                {
                    AssetDatabase.DeleteAsset(workingGraphAssetPath);
                }

                return InkFlowChartImportResult.Failure(graphAssetPath, errorMessage);
            }

            try
            {
                if (string.IsNullOrEmpty(flowchartJsonPath))
                {
                    return Fail("匯入失敗：未提供 .flowchart.json 路徑。");
                }

                if (!flowchartJsonPath.EndsWith(FlowchartJsonSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    return Fail($"匯入失敗：不是 .flowchart.json 檔案（{flowchartJsonPath}）。");
                }

                if (!File.Exists(flowchartJsonPath))
                {
                    return Fail($"匯入失敗：找不到 sidecar 檔案（{flowchartJsonPath}）。");
                }

                string jsonContent = File.ReadAllText(flowchartJsonPath);
                ExportGraphDto graphDto = JsonUtility.FromJson<ExportGraphDto>(jsonContent);
                if (graphDto == null)
                {
                    return Fail($"匯入失敗：sidecar 內容無法解析（{flowchartJsonPath}）。");
                }

                if (graphDto.nodes == null)
                {
                    graphDto.nodes = new List<ExportNodeDto>();
                }

                if (string.IsNullOrEmpty(graphDto.startNodeId))
                {
                    return Fail("匯入失敗：startNodeId 為空。");
                }

                // ===== 變更開始 =====
                // 2026/03/18 Opsidanos (修改原因：開始落地 Batch 2，匯入端先共用 current projection validator，讓 sidecar 契約規則不再只活在 exporter 或局部匯入分支裡)
                // 預期結果：`.flowchart.json` 只要一進來就能先用同一套 shared validator 擋下 condition/choice/stageAction/dialogue 的結構錯誤
                if (!CurrentFlowProjectionValidator.TryValidate(graphDto, out string projectionError))
                {
                    return Fail($"匯入失敗：{projectionError}");
                }
                // ===== 變更結束 =====

                // ===== 變更開始 =====
                // 2026/03/21 Opsidanos (修改原因：開始落地 Batch 5，讓 current projection 匯入先經過 canonical bridge，再回到 normalized DTO)
                // 預期結果：importer 不再只依賴 sidecar DTO 自己往下流，後續 canonical graph 會真正參與 current working line
                if (!CurrentFlowCanonicalGraphAdapter.TryBuildCanonicalGraph(graphDto, out CanonicalGraphDocument canonicalGraph, out string canonicalError))
                {
                    return Fail($"匯入失敗：{canonicalError}");
                }

                if (!CurrentFlowCanonicalGraphAdapter.TryBuildProjection(canonicalGraph, out graphDto, out canonicalError))
                {
                    return Fail($"匯入失敗：{canonicalError}");
                }
                // ===== 變更結束 =====

                // ===== 變更開始 =====
                // 2026/03/18 Opsidanos (修改原因：開始落地 Batch 2 第三刀，先把 current projection DTO -> import plan 的純資料整理抽到 shared service)
                // 預期結果：匯入器不再自己整理 choice/condition/dialogue 的 option 文本與 wire fallback，後續只接收一份 normalized import plan
                CurrentFlowImportPlan importPlan = CurrentFlowImportService.BuildPlan(graphDto);
                // ===== 變更結束 =====

                bool startNodeExists = graphDto.nodes.Any(node => node.id == graphDto.startNodeId);
                if (!startNodeExists)
                {
                    return Fail($"匯入失敗：startNodeId `{graphDto.startNodeId}` 不存在於 nodes。");
                }

                string flowchartDirectory = Path.GetDirectoryName(flowchartJsonPath) ?? "Assets";
                string baseFileName = GetBaseName(flowchartJsonPath);
                string inkPath = $"{flowchartDirectory}/{baseFileName}.ink";
                if (!File.Exists(inkPath))
                {
                    return Fail($"匯入失敗：缺少配對的 .ink 檔案（{inkPath}）。");
                }

                graphAssetPath = $"{flowchartDirectory}/{baseFileName}.inkfc";
                // ===== 變更開始 =====
                // 2026/02/08 Opsidanos (修改原因：匯入改為先建立暫存 `.inkfc`，建圖成功後再替換正式檔)
                // 預期結果：避免直接在目標路徑呼叫 `CreateGraph`，降低 Editor Undo 異常觸發機率
                workingGraphAssetPath = AssetDatabase.GenerateUniqueAssetPath($"{flowchartDirectory}/{baseFileName}__importing__.inkfc");
                InkFlowChartGraph graph = CreateGraphWithMutedLogger(workingGraphAssetPath);
                if (graph == null)
                {
                    return Fail($"匯入失敗：無法建立暫存圖資產（{workingGraphAssetPath}）。");
                }
                workingGraphCreated = true;
                // ===== 變更結束 =====

                // ===== 變更開始 =====
                // 2026/03/18 Opsidanos (修改原因：把 import plan -> GraphToolkit graph 的節點建立、option 套用與 wire 建立集中到 editor adapter)
                // 預期結果：Importer 不再直接管理 GraphToolkit rebuild 細節；未來若前端殼改變，adapter 邊界會更清楚
                if (!CurrentFlowGraphToolkitImportAdapter.TryPopulateGraph(graph, importPlan, out string rebuildError))
                {
                    return Fail(rebuildError);
                }
                // ===== 變更結束 =====

                GraphDatabase.SaveGraphIfDirty(graph);
                // ===== 變更開始 =====
                // 2026/02/08 Opsidanos (修改原因：匯入完成後用「刪舊檔 + 移動暫存檔」替換正式圖，避免同路徑建圖)
                // 預期結果：正式輸出仍是同名 `.inkfc`，但建圖過程不直接操作正式路徑
                if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(graphAssetPath) != null)
                {
                    if (!AssetDatabase.DeleteAsset(graphAssetPath))
                    {
                        return Fail($"匯入失敗：無法刪除既有圖資產（{graphAssetPath}）。");
                    }
                }

                string moveError = AssetDatabase.MoveAsset(workingGraphAssetPath, graphAssetPath);
                if (!string.IsNullOrEmpty(moveError))
                {
                    return Fail($"匯入失敗：無法將暫存圖替換到目標路徑（{graphAssetPath}）：{moveError}");
                }

                workingGraphCreated = false;
                // ===== 變更結束 =====
                AssetDatabase.Refresh();
                return InkFlowChartImportResult.Success(graphAssetPath);
            }
            catch (Exception exception)
            {
                return Fail($"匯入失敗（{flowchartJsonPath}）：{exception.Message}");
            }
        }

        private static string GetBaseName(string flowchartJsonPath)
        {
            string fileName = Path.GetFileName(flowchartJsonPath);
            if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(FlowchartJsonSuffix, StringComparison.OrdinalIgnoreCase))
            {
                return fileName.Substring(0, fileName.Length - FlowchartJsonSuffix.Length);
            }

            return Path.GetFileNameWithoutExtension(flowchartJsonPath);
        }

        private static InkFlowChartGraph CreateGraphWithMutedLogger(string graphAssetPath)
        {
            bool originalLogEnabled = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                return GraphDatabase.CreateGraph<InkFlowChartGraph>(graphAssetPath);
            }
            finally
            {
                Debug.unityLogger.logEnabled = originalLogEnabled;
            }
        }

    }
}
// ===== 變更結束 =====
