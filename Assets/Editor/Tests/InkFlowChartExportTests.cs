// ===== 變更開始 =====
// 2026/02/08 Opsidanos (修改原因：為 Graph Toolkit 匯出 MVP 補齊自動測試，確保 `.inkfc` 可匯出 `.ink + .flowchart.json`)
// 預期結果：fixture 圖與空圖兩種情境可穩定驗證，並能自動清理測試暫存檔
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using OpsidanosInk.Editor;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OpsidanosInk.Tests.EditMode
{
    public sealed class InkFlowChartExportTests
    {
        private const string TempFolderPath = "Assets/TmpGraphToolkitExportTests";
        private const string FixtureFolderPath = "Assets/Editor/Tests/Fixtures";
        private const string FixtureGraphPath = "Assets/Editor/Tests/Fixtures/InkFlowChartExportFixture.inkfc";

        [SetUp]
        public void SetUp()
        {
            EnsureTempFolder();
            EnsureFixtureGraphExists();
        }

        [TearDown]
        public void TearDown()
        {
            DeleteTempFolderIfExists();
            AssetDatabase.Refresh();
        }

        [Test]
        public void ExportFixtureGraph_可產出Ink與FlowchartJson()
        {
            string copiedGraphPath = string.Empty;
            string exportedInkPath = string.Empty;
            string exportedJsonPath = string.Empty;

            try
            {
                copiedGraphPath = BuildUniqueGraphPath("FixtureExport");
                bool copySucceeded = AssetDatabase.CopyAsset(FixtureGraphPath, copiedGraphPath);
                Assert.IsTrue(copySucceeded, $"無法複製 fixture 圖：{FixtureGraphPath}");

                InkFlowChartExportResult exportResult = InkFlowChartExporter.ExportGraphAsset(copiedGraphPath);
                Assert.IsTrue(exportResult.success, $"匯出應該成功，但失敗：{exportResult.errorMessage}");

                exportedInkPath = exportResult.inkOutputPath;
                exportedJsonPath = exportResult.jsonOutputPath;

                Assert.IsTrue(File.Exists(exportedInkPath), "匯出後必須存在 `.ink`。");
                Assert.IsTrue(File.Exists(exportedJsonPath), "匯出後必須存在 `.flowchart.json`。");

                string jsonContent = File.ReadAllText(exportedJsonPath);
                string inkContent = File.ReadAllText(exportedInkPath);

                StringAssert.Contains("\"startNodeId\"", jsonContent, "JSON 需包含 startNodeId。");
                StringAssert.Contains("\"nodes\"", jsonContent, "JSON 需包含 nodes。");
                StringAssert.Contains("-> knot_", inkContent, "INK 需包含起始跳轉。");
                StringAssert.Contains("=== knot_", inkContent, "INK 需包含 knot 區塊。");
            }
            finally
            {
                DeleteFileIfExists(exportedInkPath);
                DeleteFileIfExists(exportedJsonPath);
                DeleteAssetIfExists(copiedGraphPath);
            }
        }

        [Test]
        public void ExportEmptyGraph_回傳失敗且錯誤訊息明確()
        {
            string emptyGraphPath = string.Empty;
            try
            {
                emptyGraphPath = BuildUniqueGraphPath("EmptyExport");
                CreateGraphWithMutedLogger(emptyGraphPath);

                InkFlowChartExportResult exportResult = InkFlowChartExporter.ExportGraphAsset(emptyGraphPath);
                Assert.IsFalse(exportResult.success, "空圖匯出應回傳失敗。");
                StringAssert.Contains("找不到開始節點", exportResult.errorMessage, "失敗訊息應明確指出缺少開始節點。");
            }
            finally
            {
                DeleteAssetIfExists(emptyGraphPath);
            }
        }

        private static void EnsureFixtureGraphExists()
        {
            EnsureFixtureFolder();
            if (AssetDatabase.LoadAssetAtPath<Object>(FixtureGraphPath) != null)
            {
                return;
            }

            InkFlowChartGraph fixtureGraph = CreateGraphWithMutedLogger(FixtureGraphPath);
            BuildFixtureGraph(fixtureGraph);
            GraphDatabase.SaveGraphIfDirty(fixtureGraph);
            AssetDatabase.Refresh();
        }

        private static void BuildFixtureGraph(InkFlowChartGraph graph)
        {
            FieldInfo graphImplementationField = typeof(Graph).GetField("m_Implementation", BindingFlags.Instance | BindingFlags.NonPublic);
            object graphImplementation = graphImplementationField?.GetValue(graph);
            if (graphImplementation == null)
            {
                throw new InvalidOperationException("找不到 Graph 內部實作，無法建立 fixture。");
            }

            MethodInfo createNodeModelMethod = graphImplementation.GetType().GetMethod("CreateNodeModel", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo createWireMethod = graphImplementation
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .First(method => method.Name == "CreateWire" && method.GetParameters().Length == 3);

            var startNode = new InkFlowStartNode();
            var actionNode = new InkFlowActionNode();
            var commentNode = new InkFlowCommentNode();

            createNodeModelMethod?.Invoke(graphImplementation, new object[] { startNode, new Vector2(120f, 120f) });
            createNodeModelMethod?.Invoke(graphImplementation, new object[] { actionNode, new Vector2(420f, 120f) });
            createNodeModelMethod?.Invoke(graphImplementation, new object[] { commentNode, new Vector2(720f, 120f) });

            IPort startOutput = startNode.GetOutputPortByName("Flow");
            IPort actionInput = actionNode.GetInputPortByName("Flow");
            IPort actionOutput = actionNode.GetOutputPortByName("Flow");
            IPort commentInput = commentNode.GetInputPortByName("Flow");

            createWireMethod.Invoke(graphImplementation, new object[] { actionInput, startOutput, default(Hash128) });
            createWireMethod.Invoke(graphImplementation, new object[] { commentInput, actionOutput, default(Hash128) });
        }

        private static InkFlowChartGraph CreateGraphWithMutedLogger(string graphPath)
        {
            bool originalLogEnabled = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                return GraphDatabase.CreateGraph<InkFlowChartGraph>(graphPath);
            }
            finally
            {
                Debug.unityLogger.logEnabled = originalLogEnabled;
            }
        }

        private static void EnsureTempFolder()
        {
            if (!AssetDatabase.IsValidFolder(TempFolderPath))
            {
                AssetDatabase.CreateFolder("Assets", "TmpGraphToolkitExportTests");
            }
        }

        private static void EnsureFixtureFolder()
        {
            if (!AssetDatabase.IsValidFolder(FixtureFolderPath))
            {
                AssetDatabase.CreateFolder("Assets/Editor/Tests", "Fixtures");
            }
        }

        private static string BuildUniqueGraphPath(string prefix)
        {
            string uniqueFileName = $"{prefix}_{Guid.NewGuid():N}.inkfc";
            string rawPath = $"{TempFolderPath}/{uniqueFileName}";
            return AssetDatabase.GenerateUniqueAssetPath(rawPath);
        }

        private static void DeleteAssetIfExists(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<Object>(assetPath) != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
        }

        private static void DeleteFileIfExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private static void DeleteTempFolderIfExists()
        {
            if (AssetDatabase.IsValidFolder(TempFolderPath))
            {
                AssetDatabase.DeleteAsset(TempFolderPath);
            }
        }
    }
}
// ===== 變更結束 =====
