// ===== 變更開始 =====
// 2026/02/08 Opsidanos (修改原因：補上 Graph Toolkit 匯入 MVP 自動測試，驗證 `.flowchart.json + .ink` 可還原 `.inkfc` 並覆蓋失敗分支)
// 預期結果：可穩定驗證匯入成功案例、缺少 startNodeId 失敗、缺少 `.ink` 配對失敗，且測試可清理暫存資產
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using OpsidanosInk.Editor;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace OpsidanosInk.Tests.EditMode
{
    public sealed class InkFlowChartImportTests
    {
        private const string TempAssetPrefix = "Assets/Editor/TmpGraphToolkitImport_";

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // ===== 變更開始 =====
            // 2026/02/08 Opsidanos (修改原因：避免測試主流程過早刪除 `.ink` 觸發編譯後找不到檔案，改為在收尾等待 Ink 編譯完成再統一清理)
            // 預期結果：移除 `Ink file ... was not found after compilation`，且不再殘留 `TmpGraphToolkitImport_*` 測試檔案
            int waitFrameCount = 0;
            while (IsInkCompilerExecutingCompilationStack())
            {
                if (waitFrameCount > 600)
                {
                    Assert.Fail("Ink 編譯佇列等待逾時，請檢查 Ink 編譯流程狀態。");
                }

                waitFrameCount++;
                yield return null;
            }

            DeleteTempAssetsByPrefix();
            // ===== 變更結束 =====
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static bool IsInkCompilerExecutingCompilationStack()
        {
            // ===== 變更開始 =====
            // 2026/02/08 Opsidanos (修改原因：測試程式集無法直接參考 InkEditor 命名空間，改用反射讀取 InkCompiler 狀態)
            // 預期結果：不需新增 asmdef 參考也能等待 Ink 編譯佇列完成
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type compilerType = assembly.GetType("Ink.UnityIntegration.InkCompiler", false);
                if (compilerType == null)
                {
                    continue;
                }

                PropertyInfo propertyInfo = compilerType.GetProperty("executingCompilationStack", BindingFlags.Public | BindingFlags.Static);
                if (propertyInfo == null || propertyInfo.PropertyType != typeof(bool))
                {
                    return false;
                }

                object propertyValue = propertyInfo.GetValue(null);
                return propertyValue is bool isExecuting && isExecuting;
            }

            return false;
            // ===== 變更結束 =====
        }

        [Test]
        public void ImportValidFlowchartJson_可還原節點連線與內容()
        {
            string uniqueName = BuildUniqueName("ImportValid");
            string jsonPath = BuildAssetPath(uniqueName, ".flowchart.json");
            string inkPath = BuildAssetPath(uniqueName, ".ink");
            string graphPath = BuildAssetPath(uniqueName, ".inkfc");

            // ===== 變更開始 =====
            // 2026/02/08 Opsidanos (修改原因：避免在測試主流程中提前刪檔，改由 UnityTearDown 統一等待編譯完成後清理)
            // 預期結果：成功案例不再因 finally 提前刪除 `.ink` 造成 warning
            ExportGraphDto graphDto = BuildValidGraphDto();
            WriteFile(jsonPath, JsonUtility.ToJson(graphDto, true));
            WriteFile(inkPath, "-> knot_N000\n=== knot_N000 ===\n-> END\n");

            InkFlowChartImportResult importResult = InkFlowChartImporter.ImportFromFlowchartJson(jsonPath);
            Assert.IsTrue(importResult.success, $"匯入應成功，但失敗：{importResult.errorMessage}");
            Assert.AreEqual(graphPath, importResult.graphAssetPath, "匯入輸出路徑應為同目錄同名 `.inkfc`。");

            InkFlowChartGraph loadedGraph = GraphDatabase.LoadGraphForImporter<InkFlowChartGraph>(graphPath);
            Assert.NotNull(loadedGraph, "匯入成功後必須可載入 `.inkfc`。");

            List<INode> nodes = loadedGraph.GetNodes().ToList();
            Assert.AreEqual(3, nodes.Count, "匯入後節點數應與 JSON 一致。");

            InkFlowStartNode startNode = nodes.OfType<InkFlowStartNode>().Single();
            InkFlowActionNode actionNode = nodes.OfType<InkFlowActionNode>().Single();
            InkFlowCommentNode commentNode = nodes.OfType<InkFlowCommentNode>().Single();

            AssertNodeOptionValue(actionNode, "Content", "你好，旅人。");
            AssertNodeOptionValue(commentNode, "Note", "這是註解節點");

            AssertEdgeExists(startNode, actionNode);
            AssertEdgeExists(actionNode, commentNode);
            // ===== 變更結束 =====
        }

        [Test]
        public void ImportMissingStartNodeId_回傳失敗且不產生圖資產()
        {
            string uniqueName = BuildUniqueName("ImportNoStart");
            string jsonPath = BuildAssetPath(uniqueName, ".flowchart.json");
            string graphPath = BuildAssetPath(uniqueName, ".inkfc");

            ExportGraphDto graphDto = BuildValidGraphDto();
            graphDto.startNodeId = string.Empty;
            WriteFile(jsonPath, JsonUtility.ToJson(graphDto, true));

            InkFlowChartImportResult importResult = InkFlowChartImporter.ImportFromFlowchartJson(jsonPath);
            Assert.IsFalse(importResult.success, "缺少 startNodeId 應回傳失敗。");
            StringAssert.Contains("startNodeId", importResult.errorMessage, "錯誤訊息應指出 startNodeId 問題。");
            Assert.IsNull(AssetDatabase.LoadAssetAtPath<Object>(graphPath), "失敗時不應產生 `.inkfc`。");
        }

        [Test]
        public void ImportMissingInkPair_回傳失敗且不產生圖資產()
        {
            string uniqueName = BuildUniqueName("ImportNoInk");
            string jsonPath = BuildAssetPath(uniqueName, ".flowchart.json");
            string graphPath = BuildAssetPath(uniqueName, ".inkfc");

            ExportGraphDto graphDto = BuildValidGraphDto();
            WriteFile(jsonPath, JsonUtility.ToJson(graphDto, true));

            InkFlowChartImportResult importResult = InkFlowChartImporter.ImportFromFlowchartJson(jsonPath);
            Assert.IsFalse(importResult.success, "缺少同名 `.ink` 時應回傳失敗。");
            StringAssert.Contains("缺少配對的 .ink", importResult.errorMessage, "錯誤訊息應指出缺少 `.ink`。");
            Assert.IsNull(AssetDatabase.LoadAssetAtPath<Object>(graphPath), "失敗時不應產生 `.inkfc`。");
        }

        private static ExportGraphDto BuildValidGraphDto()
        {
            return new ExportGraphDto
            {
                version = "1.0.0",
                graphName = "ImportFixture",
                startNodeId = "N000",
                nodes = new List<ExportNodeDto>
                {
                    new ExportNodeDto
                    {
                        id = "N000",
                        type = "start",
                        content = string.Empty,
                        nextIds = new List<string> { "N001" }
                    },
                    new ExportNodeDto
                    {
                        id = "N001",
                        type = "action",
                        content = "你好，旅人。",
                        nextIds = new List<string> { "N002" }
                    },
                    new ExportNodeDto
                    {
                        id = "N002",
                        type = "comment",
                        content = "這是註解節點",
                        nextIds = new List<string>()
                    }
                }
            };
        }

        private static void AssertNodeOptionValue(Node node, string optionName, string expectedValue)
        {
            INodeOption option = node.GetNodeOptionByName(optionName);
            Assert.NotNull(option, $"節點必須存在 `{optionName}` option。");

            bool readSuccess = option.TryGetValue(out string actualValue);
            Assert.IsTrue(readSuccess, $"節點 `{optionName}` option 應可讀取字串值。");
            Assert.AreEqual(expectedValue, actualValue, $"節點 `{optionName}` option 值不符合預期。");
        }

        private static void AssertEdgeExists(INode fromNode, INode toNode)
        {
            IPort fromOutputPort = fromNode.GetOutputPortByName("Flow");
            IPort toInputPort = toNode.GetInputPortByName("Flow");
            Assert.NotNull(fromOutputPort, "來源節點必須有 Flow 輸出埠。");
            Assert.NotNull(toInputPort, "目標節點必須有 Flow 輸入埠。");

            var connectedPorts = new List<IPort>();
            fromOutputPort.GetConnectedPorts(connectedPorts);
            bool edgeFound = connectedPorts.Any(port => port == toInputPort);
            Assert.IsTrue(edgeFound, "匯入後應存在對應的 Flow 連線。");
        }

        private static string BuildUniqueName(string prefix)
        {
            return $"{prefix}_{Guid.NewGuid():N}";
        }

        private static string BuildAssetPath(string uniqueName, string extension)
        {
            // ===== 變更開始 =====
            // 2026/02/08 Opsidanos (修改原因：避免建立固定資料夾，改為 Assets/Editor 底下唯一前綴檔名)
            // 預期結果：測試不再產生 `TmpGraphToolkitImportTests` 目錄
            return $"{TempAssetPrefix}{uniqueName}{extension}";
            // ===== 變更結束 =====
        }

        private static void WriteFile(string assetPath, string content)
        {
            string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
            File.WriteAllText(absolutePath, content);
        }

        private static void DeleteTempAssetsByPrefix()
        {
            string editorDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/Editor");
            if (!Directory.Exists(editorDirectoryPath))
            {
                return;
            }

            string[] tempPaths = Directory.GetFiles(editorDirectoryPath, "TmpGraphToolkitImport_*", SearchOption.TopDirectoryOnly);
            foreach (string tempPath in tempPaths)
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                string metaPath = $"{tempPath}.meta";
                if (File.Exists(metaPath))
                {
                    File.Delete(metaPath);
                }
            }
        }

    }
}
// ===== 變更結束 =====
