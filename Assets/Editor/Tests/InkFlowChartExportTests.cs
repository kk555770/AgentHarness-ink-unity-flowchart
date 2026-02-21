// ===== 變更開始 =====
// 2026/02/08 Opsidanos (修改原因：為 Graph Toolkit 匯出 MVP 補齊自動測試，確保 `.inkfc` 可匯出 `.ink + .flowchart.json`)
// 預期結果：fixture 圖與空圖兩種情境可穩定驗證，並能自動清理測試暫存檔
using System;
// ===== 變更開始 =====
// 2026/02/21 Opsidanos (修改原因：補上 Ink 編譯檢驗與內容驗證測試，需要收集編譯錯誤)
// 預期結果：匯出 `.ink` 必須可被 Ink 編譯器接受；且 action 內容若藏流程語法會被匯出階段擋下
using System.Collections.Generic;
// ===== 變更結束 =====
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
            // ===== 變更開始 =====
            // 2026/02/08 Opsidanos (修改原因：移除測試啟動時的批次清理，避免測試與匯入編譯時序衝突拖垮 Editor Undo)
            // 預期結果：匯出測試啟動時只建立固定暫存資料夾，不再主動清除 `TmpGraphToolkitExportTests*`
            EnsureTempFolder();
            // ===== 變更結束 =====
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
                // ===== 變更開始 =====
                // 2026/02/08 Opsidanos (修改原因：`CopyAsset` 的 .inkfc 在特定時序會出現讀取失敗，改為每次直接建立 fixture 圖)
                // 預期結果：匯出測試不再受複製時序影響，穩定取得可匯出的 Graph 資產
                InkFlowChartGraph copiedGraph = CreateGraphWithMutedLogger(copiedGraphPath);
                BuildFixtureGraph(copiedGraph);
                GraphDatabase.SaveGraphIfDirty(copiedGraph);
                string createdGraphPath = GraphDatabase.GetGraphAssetPath(copiedGraph);
                if (!string.IsNullOrEmpty(createdGraphPath))
                {
                    copiedGraphPath = createdGraphPath;
                }
                // ===== 變更結束 =====

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

                // ===== 變更開始 =====
                // 2026/02/21 Opsidanos (修改原因：依輸出契約 §7，可檢驗性必須鎖住「匯出的 .ink 可被 Ink 編譯」)
                // 預期結果：匯出結果至少能被 Ink 編譯成 Runtime Story，避免產出看似 Ink 但其實不可編譯的檔案
                AssertInkCompiles(inkContent);
                // ===== 變更結束 =====
            }
            finally
            {
                DeleteFileIfExists(exportedInkPath);
                DeleteFileIfExists(exportedJsonPath);
                DeleteAssetIfExists(copiedGraphPath);
            }
        }

        // ===== 變更開始 =====
        // 2026/02/21 Opsidanos (修改原因：依輸出契約 §6.2.3，action 內容不得藏流程結構)
        // 預期結果：若 action 內容出現未跳脫的 `->`，匯出必須失敗並提示如何跳脫或改用分岔節點
        [Test]
        public void ExportActionContent藏Divert_回傳失敗且錯誤訊息明確()
        {
            string graphPath = string.Empty;
            try
            {
                graphPath = BuildUniqueGraphPath("HiddenFlowInContent");
                InkFlowChartGraph graph = CreateGraphWithMutedLogger(graphPath);
                BuildHiddenFlowInActionContentGraph(graph);
                GraphDatabase.SaveGraphIfDirty(graph);

                InkFlowChartExportResult exportResult = InkFlowChartExporter.ExportGraphAsset(graphPath);
                Assert.IsFalse(exportResult.success, "內容藏流程的圖匯出應回傳失敗。");
                StringAssert.Contains("內容", exportResult.errorMessage, "錯誤訊息應指出是內容問題。");
                StringAssert.Contains("->", exportResult.errorMessage, "錯誤訊息應指出不合法的 divert。");
                StringAssert.Contains("\\\\->", exportResult.errorMessage, "錯誤訊息應提示跳脫寫法。");
            }
            finally
            {
                DeleteAssetIfExists(graphPath);
            }
        }
        // ===== 變更結束 =====

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

        // ===== 變更開始 =====
        // 2026/02/13 Opsidanos (修改原因：Graph v1（線性流程）每個節點只能有 0 或 1 條 next，避免 Ink 多 divert 造成死線)
        // 預期結果：當圖上某節點有多條下一步連線時，匯出應回傳失敗且錯誤訊息明確
        [Test]
        public void ExportMultiNextGraph_回傳失敗且錯誤訊息明確()
        {
            string graphPath = string.Empty;
            try
            {
                graphPath = BuildUniqueGraphPath("MultiNextExport");
                InkFlowChartGraph graph = CreateGraphWithMutedLogger(graphPath);
                BuildMultiNextGraph(graph);
                GraphDatabase.SaveGraphIfDirty(graph);

                InkFlowChartExportResult exportResult = InkFlowChartExporter.ExportGraphAsset(graphPath);
                Assert.IsFalse(exportResult.success, "多條 next 的圖匯出應回傳失敗。");
                StringAssert.Contains("線性節點", exportResult.errorMessage, "失敗訊息應指出線性節點不允許多輸出。");
                StringAssert.Contains("choice/condition", exportResult.errorMessage, "失敗訊息應提示改用分岔節點。");
            }
            finally
            {
                DeleteAssetIfExists(graphPath);
            }
        }
        // ===== 變更結束 =====

        // ===== 變更開始 =====
        // 2026/02/13 Opsidanos (修改原因：補上 Graph v2 分岔節點匯出驗證，確保 choice/condition 會輸出正確 Ink 基準語法)
        // 預期結果：choice 匯出包含 `* [`；condition 匯出包含 `{` 與 `- else:`
        [Test]
        public void ExportChoiceGraph_可產出InkChoice語法()
        {
            string graphPath = string.Empty;
            string exportedInkPath = string.Empty;
            string exportedJsonPath = string.Empty;

            try
            {
                graphPath = BuildUniqueGraphPath("ChoiceExport");
                InkFlowChartGraph graph = CreateGraphWithMutedLogger(graphPath);
                BuildChoiceGraph(graph);
                GraphDatabase.SaveGraphIfDirty(graph);

                InkFlowChartExportResult exportResult = InkFlowChartExporter.ExportGraphAsset(graphPath);
                Assert.IsTrue(exportResult.success, $"匯出應該成功，但失敗：{exportResult.errorMessage}");

                exportedInkPath = exportResult.inkOutputPath;
                exportedJsonPath = exportResult.jsonOutputPath;

                string inkContent = File.ReadAllText(exportedInkPath);
                StringAssert.Contains("* [", inkContent, "INK 應包含 `*` choice 語法。");
                StringAssert.Contains("-> knot_", inkContent, "INK 應包含 divert。");
            }
            finally
            {
                DeleteFileIfExists(exportedInkPath);
                DeleteFileIfExists(exportedJsonPath);
                DeleteAssetIfExists(graphPath);
            }
        }

        [Test]
        public void ExportConditionGraph_可產出InkConditional語法()
        {
            string graphPath = string.Empty;
            string exportedInkPath = string.Empty;
            string exportedJsonPath = string.Empty;

            try
            {
                graphPath = BuildUniqueGraphPath("ConditionExport");
                InkFlowChartGraph graph = CreateGraphWithMutedLogger(graphPath);
                BuildConditionGraph(graph);
                GraphDatabase.SaveGraphIfDirty(graph);

                InkFlowChartExportResult exportResult = InkFlowChartExporter.ExportGraphAsset(graphPath);
                Assert.IsTrue(exportResult.success, $"匯出應該成功，但失敗：{exportResult.errorMessage}");

                exportedInkPath = exportResult.inkOutputPath;
                exportedJsonPath = exportResult.jsonOutputPath;

                string inkContent = File.ReadAllText(exportedInkPath);
                StringAssert.Contains("{", inkContent, "INK 應包含條件區塊 `{ ... }`。");
                StringAssert.Contains("- else:", inkContent, "INK 應包含 `- else:` 分支。");
                StringAssert.Contains("-> knot_", inkContent, "INK 應包含 divert。");
            }
            finally
            {
                DeleteFileIfExists(exportedInkPath);
                DeleteFileIfExists(exportedJsonPath);
                DeleteAssetIfExists(graphPath);
            }
        }
        // ===== 變更結束 =====

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

        // ===== 變更開始 =====
        // 2026/02/21 Opsidanos (修改原因：建立「內容藏流程」fixture 圖，驗證匯出會擋下 action 內容中的 divert 語法)
        // 預期結果：action 內容含 `->` 時，匯出失敗（因為圖必須是權威，流程不得藏在文字）
        private static void BuildHiddenFlowInActionContentGraph(InkFlowChartGraph graph)
        {
            FieldInfo graphImplementationField = typeof(Graph).GetField("m_Implementation", BindingFlags.Instance | BindingFlags.NonPublic);
            object graphImplementation = graphImplementationField?.GetValue(graph);
            if (graphImplementation == null)
            {
                throw new InvalidOperationException("找不到 Graph 內部實作，無法建立內容藏流程 fixture。");
            }

            MethodInfo createNodeModelMethod = graphImplementation.GetType().GetMethod("CreateNodeModel", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo createWireMethod = graphImplementation
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .First(method => method.Name == "CreateWire" && method.GetParameters().Length == 3);

            var startNode = new InkFlowStartNode();
            var actionNode = new InkFlowActionNode();

            createNodeModelMethod?.Invoke(graphImplementation, new object[] { startNode, new Vector2(120f, 120f) });
            createNodeModelMethod?.Invoke(graphImplementation, new object[] { actionNode, new Vector2(420f, 120f) });

            IPort startOutput = startNode.GetOutputPortByName("Flow");
            IPort actionInput = actionNode.GetInputPortByName("Flow");
            createWireMethod.Invoke(graphImplementation, new object[] { actionInput, startOutput, default(Hash128) });

            if (!TrySetNodeOptionValue(actionNode, "Content", "-> END", out string errorMessage))
            {
                throw new InvalidOperationException($"無法寫入 action content：{errorMessage}");
            }
        }

        private static void AssertInkCompiles(string inkContent)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            var compiler = new Ink.Compiler(
                inkContent,
                new Ink.Compiler.Options
                {
                    errorHandler = (message, type) =>
                    {
                        if (type == Ink.ErrorType.Error)
                        {
                            errors.Add(message);
                            return;
                        }

                        warnings.Add(message);
                    }
                });

            Ink.Runtime.Story story = compiler.Compile();
            Assert.IsNotNull(story, "Ink 編譯結果不應為 null。");
            Assert.IsTrue(errors.Count == 0, $"Ink 編譯不應出現 Error：{string.Join(" | ", errors)}");

            string storyJson = story.ToJson();
            Assert.IsFalse(string.IsNullOrEmpty(storyJson), "編譯後的 Story JSON 不應為空。");
        }

        private static bool TrySetNodeOptionValue<T>(Node node, string optionName, T value, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (node == null)
            {
                return true;
            }

            INodeOption option = node.GetNodeOptionByName(optionName);
            if (option == null)
            {
                errorMessage = $"找不到節點選項 `{optionName}`。";
                return false;
            }

            PropertyInfo portModelProperty = option.GetType().GetProperty("PortModel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            object portModelObject = portModelProperty?.GetValue(option);
            if (portModelObject == null)
            {
                errorMessage = $"節點選項 `{optionName}` 缺少 PortModel。";
                return false;
            }

            PropertyInfo embeddedValueProperty = portModelObject.GetType().GetProperty("EmbeddedValue", BindingFlags.Instance | BindingFlags.Public);
            object embeddedValueObject = embeddedValueProperty?.GetValue(portModelObject);
            if (embeddedValueObject == null)
            {
                errorMessage = $"節點選項 `{optionName}` 缺少 EmbeddedValue。";
                return false;
            }

            MethodInfo trySetValueMethod = embeddedValueObject
                .GetType()
                .GetMethod("TrySetValue", BindingFlags.Instance | BindingFlags.Public)?
                .MakeGenericMethod(typeof(T));

            if (trySetValueMethod == null)
            {
                errorMessage = $"節點選項 `{optionName}` 無法寫入內容。";
                return false;
            }

            object setResult = trySetValueMethod.Invoke(embeddedValueObject, new object[] { value });
            bool success = setResult is bool boolResult && boolResult;
            if (!success)
            {
                errorMessage = $"節點選項 `{optionName}` 寫入失敗。";
                return false;
            }

            return true;
        }
        // ===== 變更結束 =====

        // ===== 變更開始 =====
        // 2026/02/13 Opsidanos (修改原因：建立「多 next」fixture 圖，用來驗證匯出會拒絕不閉環的 v1 分岔)
        // 預期結果：action 節點同時連到兩個 comment，觸發匯出失敗分支
        private static void BuildMultiNextGraph(InkFlowChartGraph graph)
        {
            FieldInfo graphImplementationField = typeof(Graph).GetField("m_Implementation", BindingFlags.Instance | BindingFlags.NonPublic);
            object graphImplementation = graphImplementationField?.GetValue(graph);
            if (graphImplementation == null)
            {
                throw new InvalidOperationException("找不到 Graph 內部實作，無法建立多 next fixture。");
            }

            MethodInfo createNodeModelMethod = graphImplementation.GetType().GetMethod("CreateNodeModel", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo createWireMethod = graphImplementation
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .First(method => method.Name == "CreateWire" && method.GetParameters().Length == 3);

            var startNode = new InkFlowStartNode();
            var actionNode = new InkFlowActionNode();
            var commentNodeA = new InkFlowCommentNode();
            var commentNodeB = new InkFlowCommentNode();

            createNodeModelMethod?.Invoke(graphImplementation, new object[] { startNode, new Vector2(120f, 120f) });
            createNodeModelMethod?.Invoke(graphImplementation, new object[] { actionNode, new Vector2(420f, 120f) });
            createNodeModelMethod?.Invoke(graphImplementation, new object[] { commentNodeA, new Vector2(720f, 60f) });
            createNodeModelMethod?.Invoke(graphImplementation, new object[] { commentNodeB, new Vector2(720f, 180f) });

            IPort startOutput = startNode.GetOutputPortByName("Flow");
            IPort actionInput = actionNode.GetInputPortByName("Flow");
            IPort actionOutput = actionNode.GetOutputPortByName("Flow");
            IPort commentInputA = commentNodeA.GetInputPortByName("Flow");
            IPort commentInputB = commentNodeB.GetInputPortByName("Flow");

            createWireMethod.Invoke(graphImplementation, new object[] { actionInput, startOutput, default(Hash128) });
            createWireMethod.Invoke(graphImplementation, new object[] { commentInputA, actionOutput, default(Hash128) });
            createWireMethod.Invoke(graphImplementation, new object[] { commentInputB, actionOutput, default(Hash128) });
        }

        private static void BuildChoiceGraph(InkFlowChartGraph graph)
        {
            FieldInfo graphImplementationField = typeof(Graph).GetField("m_Implementation", BindingFlags.Instance | BindingFlags.NonPublic);
            object graphImplementation = graphImplementationField?.GetValue(graph);
            if (graphImplementation == null)
            {
                throw new InvalidOperationException("找不到 Graph 內部實作，無法建立 choice fixture。");
            }

            MethodInfo createNodeModelMethod = graphImplementation.GetType().GetMethod("CreateNodeModel", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo createWireMethod = graphImplementation
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .First(method => method.Name == "CreateWire" && method.GetParameters().Length == 3);

            var startNode = new InkFlowStartNode();
            var choiceNode = new InkFlowChoiceNode();
            var actionNodeA = new InkFlowActionNode();
            var actionNodeB = new InkFlowActionNode();

            createNodeModelMethod?.Invoke(graphImplementation, new object[] { startNode, new Vector2(120f, 120f) });
            createNodeModelMethod?.Invoke(graphImplementation, new object[] { choiceNode, new Vector2(420f, 120f) });
            createNodeModelMethod?.Invoke(graphImplementation, new object[] { actionNodeA, new Vector2(720f, 60f) });
            createNodeModelMethod?.Invoke(graphImplementation, new object[] { actionNodeB, new Vector2(720f, 180f) });

            IPort startOutput = startNode.GetOutputPortByName("Flow");
            IPort choiceInput = choiceNode.GetInputPortByName("Flow");
            createWireMethod.Invoke(graphImplementation, new object[] { choiceInput, startOutput, default(Hash128) });

            IPort choiceOut0 = choiceNode.GetOutputPortByName("Out0");
            IPort choiceOut1 = choiceNode.GetOutputPortByName("Out1");
            IPort actionInputA = actionNodeA.GetInputPortByName("Flow");
            IPort actionInputB = actionNodeB.GetInputPortByName("Flow");

            createWireMethod.Invoke(graphImplementation, new object[] { actionInputA, choiceOut0, default(Hash128) });
            createWireMethod.Invoke(graphImplementation, new object[] { actionInputB, choiceOut1, default(Hash128) });
        }

        private static void BuildConditionGraph(InkFlowChartGraph graph)
        {
            FieldInfo graphImplementationField = typeof(Graph).GetField("m_Implementation", BindingFlags.Instance | BindingFlags.NonPublic);
            object graphImplementation = graphImplementationField?.GetValue(graph);
            if (graphImplementation == null)
            {
                throw new InvalidOperationException("找不到 Graph 內部實作，無法建立 condition fixture。");
            }

            MethodInfo createNodeModelMethod = graphImplementation.GetType().GetMethod("CreateNodeModel", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo createWireMethod = graphImplementation
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .First(method => method.Name == "CreateWire" && method.GetParameters().Length == 3);

            var startNode = new InkFlowStartNode();
            var conditionNode = new InkFlowConditionNode();
            var actionNodeA = new InkFlowActionNode();
            var actionNodeB = new InkFlowActionNode();

            createNodeModelMethod?.Invoke(graphImplementation, new object[] { startNode, new Vector2(120f, 120f) });
            createNodeModelMethod?.Invoke(graphImplementation, new object[] { conditionNode, new Vector2(420f, 120f) });
            createNodeModelMethod?.Invoke(graphImplementation, new object[] { actionNodeA, new Vector2(720f, 60f) });
            createNodeModelMethod?.Invoke(graphImplementation, new object[] { actionNodeB, new Vector2(720f, 180f) });

            IPort startOutput = startNode.GetOutputPortByName("Flow");
            IPort conditionInput = conditionNode.GetInputPortByName("Flow");
            createWireMethod.Invoke(graphImplementation, new object[] { conditionInput, startOutput, default(Hash128) });

            IPort condOut0 = conditionNode.GetOutputPortByName("Out0");
            IPort condOut1 = conditionNode.GetOutputPortByName("Out1");
            IPort actionInputA = actionNodeA.GetInputPortByName("Flow");
            IPort actionInputB = actionNodeB.GetInputPortByName("Flow");

            createWireMethod.Invoke(graphImplementation, new object[] { actionInputA, condOut0, default(Hash128) });
            createWireMethod.Invoke(graphImplementation, new object[] { actionInputB, condOut1, default(Hash128) });
        }
        // ===== 變更結束 =====

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
            // ===== 變更開始 =====
            // 2026/02/08 Opsidanos (修改原因：取消前綴批次刪除，改回只清理固定暫存資料夾避免 Undo 時序問題)
            // 預期結果：TearDown 只會刪除 `Assets/TmpGraphToolkitExportTests`，不再掃描刪除批次資料夾
            if (AssetDatabase.IsValidFolder(TempFolderPath))
            {
                AssetDatabase.DeleteAsset(TempFolderPath);
            }
            // ===== 變更結束 =====
        }
    }
}
// ===== 變更結束 =====
