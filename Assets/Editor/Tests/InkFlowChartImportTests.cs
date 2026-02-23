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
    // ===== 變更開始 =====
    // 2026/02/23 Opsidanos (修改原因：建立 GraphToolkit 專用測試分類與 60 秒上限，避免測試卡死拖垮 Unity/MCP)
    // 預期結果：此類別所有測試可被安全入口精準篩選，且單測最長 60 秒逾時失敗
    [Category("GraphToolkitFlowSafe")]
    [Timeout(60000)]
    // ===== 變更結束 =====
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
            // ===== 變更開始 =====
            // 2026/02/22 Opsidanos (修改原因：新增 ActionKind 後需要驗證舊 sidecar（缺 actionKind）匯入時的預設值)
            // 預期結果：未提供 actionKind 時，Action 節點預設為 Dialogue，維持舊資料可匯入
            AssertNodeOptionValue(actionNode, "ActionKind", InkFlowActionKind.Dialogue);
            // ===== 變更結束 =====
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

        // ===== 變更開始 =====
        // 2026/02/13 Opsidanos (修改原因：Graph v1（線性流程）每個節點最多只能有 1 條 next；多條 next 在 Ink 只會走第一條 divert，會變成死線)
        // 預期結果：遇到多 next 的 sidecar 直接匯入失敗，且不產生 `.inkfc`
        [Test]
        public void ImportMultipleNextIds_回傳失敗且不產生圖資產()
        {
            string uniqueName = BuildUniqueName("ImportMultiNext");
            string jsonPath = BuildAssetPath(uniqueName, ".flowchart.json");
            string inkPath = BuildAssetPath(uniqueName, ".ink");
            string graphPath = BuildAssetPath(uniqueName, ".inkfc");

            ExportGraphDto graphDto = BuildValidGraphDto();
            graphDto.nodes[0].nextIds = new List<string> { "N001", "N002" };
            WriteFile(jsonPath, JsonUtility.ToJson(graphDto, true));
            WriteFile(inkPath, "-> knot_N000\n=== knot_N000 ===\n-> END\n");

            InkFlowChartImportResult importResult = InkFlowChartImporter.ImportFromFlowchartJson(jsonPath);
            Assert.IsFalse(importResult.success, "多條 next 的 sidecar 匯入應回傳失敗。");
            StringAssert.Contains("線性流程", importResult.errorMessage, "錯誤訊息應指出 v1 線性流程限制。");
            Assert.IsNull(AssetDatabase.LoadAssetAtPath<Object>(graphPath), "失敗時不應產生 `.inkfc`。");
        }
        // ===== 變更結束 =====

        // ===== 變更開始 =====
        // 2026/02/21 Opsidanos (修改原因：補齊 Graph v2 匯入測試，鎖住 choice/condition 節點的可逆閉環規則)
        // 預期結果：`.flowchart.json` 內含 outputs 的 v2 圖可成功匯入；condition 必須有 else 且 else 必須在最後
        [Test]
        public void ImportGraphV2Choice_可還原節點連線與選項文字()
        {
            string uniqueName = BuildUniqueName("ImportV2Choice");
            string jsonPath = BuildAssetPath(uniqueName, ".flowchart.json");
            string inkPath = BuildAssetPath(uniqueName, ".ink");
            string graphPath = BuildAssetPath(uniqueName, ".inkfc");

            ExportGraphDto graphDto = BuildV2ChoiceGraphDto();
            WriteFile(jsonPath, JsonUtility.ToJson(graphDto, true));
            WriteFile(inkPath, "-> knot_N000\n=== knot_N000 ===\n-> END\n");

            InkFlowChartImportResult importResult = InkFlowChartImporter.ImportFromFlowchartJson(jsonPath);
            Assert.IsTrue(importResult.success, $"匯入應成功，但失敗：{importResult.errorMessage}");
            Assert.AreEqual(graphPath, importResult.graphAssetPath, "匯入輸出路徑應為同目錄同名 `.inkfc`。");

            InkFlowChartGraph loadedGraph = GraphDatabase.LoadGraphForImporter<InkFlowChartGraph>(graphPath);
            Assert.NotNull(loadedGraph, "匯入成功後必須可載入 `.inkfc`。");

            List<INode> nodes = loadedGraph.GetNodes().ToList();
            Assert.AreEqual(4, nodes.Count, "匯入後節點數應與 v2 JSON 一致。");

            InkFlowStartNode startNode = nodes.OfType<InkFlowStartNode>().Single();
            InkFlowChoiceNode choiceNode = nodes.OfType<InkFlowChoiceNode>().Single();
            List<InkFlowActionNode> actionNodes = nodes.OfType<InkFlowActionNode>().ToList();
            Assert.AreEqual(2, actionNodes.Count, "choice 節點應該分出兩個 action 目標。");

            InkFlowActionNode actionA = actionNodes.Single(node => GetNodeOptionString(node, "Content") == "A");
            InkFlowActionNode actionB = actionNodes.Single(node => GetNodeOptionString(node, "Content") == "B");

            AssertNodeOptionValue(choiceNode, "ChoiceTexts", "去 A\n去 B");
            AssertNodeOptionValue(choiceNode, "OutputCount", 2);
            AssertNodeOptionValue(choiceNode, "ChoiceMode", InkFlowChoiceMode.Once);
            // ===== 變更開始 =====
            // 2026/02/22 Opsidanos (修改原因：v2 匯入要驗證 actionKind 可被還原)
            // 預期結果：Action A/B 的內容類型下拉值可依 sidecar 還原
            AssertNodeOptionValue(actionA, "ActionKind", InkFlowActionKind.Dialogue);
            AssertNodeOptionValue(actionB, "ActionKind", InkFlowActionKind.StageAction);
            // ===== 變更結束 =====

            AssertEdgeExists(startNode, choiceNode);
            AssertEdgeExists(choiceNode, "Out0", actionA);
            AssertEdgeExists(choiceNode, "Out1", actionB);
        }

        [Test]
        public void ImportGraphV2Condition_可還原節點連線與Else規則()
        {
            string uniqueName = BuildUniqueName("ImportV2Condition");
            string jsonPath = BuildAssetPath(uniqueName, ".flowchart.json");
            string inkPath = BuildAssetPath(uniqueName, ".ink");
            string graphPath = BuildAssetPath(uniqueName, ".inkfc");

            ExportGraphDto graphDto = BuildV2ConditionGraphDto();
            WriteFile(jsonPath, JsonUtility.ToJson(graphDto, true));
            WriteFile(inkPath, "-> knot_N000\n=== knot_N000 ===\n-> END\n");

            InkFlowChartImportResult importResult = InkFlowChartImporter.ImportFromFlowchartJson(jsonPath);
            Assert.IsTrue(importResult.success, $"匯入應成功，但失敗：{importResult.errorMessage}");
            Assert.AreEqual(graphPath, importResult.graphAssetPath, "匯入輸出路徑應為同目錄同名 `.inkfc`。");

            InkFlowChartGraph loadedGraph = GraphDatabase.LoadGraphForImporter<InkFlowChartGraph>(graphPath);
            Assert.NotNull(loadedGraph, "匯入成功後必須可載入 `.inkfc`。");

            List<INode> nodes = loadedGraph.GetNodes().ToList();
            Assert.AreEqual(4, nodes.Count, "匯入後節點數應與 v2 JSON 一致。");

            InkFlowStartNode startNode = nodes.OfType<InkFlowStartNode>().Single();
            InkFlowConditionNode conditionNode = nodes.OfType<InkFlowConditionNode>().Single();
            List<InkFlowActionNode> actionNodes = nodes.OfType<InkFlowActionNode>().ToList();
            Assert.AreEqual(2, actionNodes.Count, "condition 節點應該分出兩個 action 目標（含 else）。");

            InkFlowActionNode actionA = actionNodes.Single(node => GetNodeOptionString(node, "Content") == "A");
            InkFlowActionNode actionB = actionNodes.Single(node => GetNodeOptionString(node, "Content") == "B");

            AssertNodeOptionValue(conditionNode, "ConditionTexts", "favor > 7");
            AssertNodeOptionValue(conditionNode, "OutputCount", 2);
            // ===== 變更開始 =====
            // 2026/02/22 Opsidanos (修改原因：condition 路徑也要驗證 actionKind 匯入還原)
            // 預期結果：Action A/B 的內容類型下拉值可依 sidecar 還原
            AssertNodeOptionValue(actionA, "ActionKind", InkFlowActionKind.Dialogue);
            AssertNodeOptionValue(actionB, "ActionKind", InkFlowActionKind.StageAction);
            // ===== 變更結束 =====

            AssertEdgeExists(startNode, conditionNode);
            AssertEdgeExists(conditionNode, "Out0", actionA);
            AssertEdgeExists(conditionNode, "Out1", actionB);
        }

        [Test]
        public void ImportGraphV2Condition_缺Else_回傳失敗且不產生圖資產()
        {
            string uniqueName = BuildUniqueName("ImportV2CondNoElse");
            string jsonPath = BuildAssetPath(uniqueName, ".flowchart.json");
            string inkPath = BuildAssetPath(uniqueName, ".ink");
            string graphPath = BuildAssetPath(uniqueName, ".inkfc");

            ExportGraphDto graphDto = BuildV2ConditionGraphDto_NoElse();
            WriteFile(jsonPath, JsonUtility.ToJson(graphDto, true));
            WriteFile(inkPath, "-> knot_N000\n=== knot_N000 ===\n-> END\n");

            InkFlowChartImportResult importResult = InkFlowChartImporter.ImportFromFlowchartJson(jsonPath);
            Assert.IsFalse(importResult.success, "缺 else 的 condition 匯入應回傳失敗。");
            StringAssert.Contains("condition", importResult.errorMessage, "錯誤訊息應指出 condition 節點規則。");
            StringAssert.Contains("outputs", importResult.errorMessage, "錯誤訊息應指出 outputs 數量不足。");
            Assert.IsNull(AssetDatabase.LoadAssetAtPath<Object>(graphPath), "失敗時不應產生 `.inkfc`。");
        }

        [Test]
        public void ImportGraphV2Condition_Else不在最後_回傳失敗且不產生圖資產()
        {
            string uniqueName = BuildUniqueName("ImportV2CondElseNotLast");
            string jsonPath = BuildAssetPath(uniqueName, ".flowchart.json");
            string inkPath = BuildAssetPath(uniqueName, ".ink");
            string graphPath = BuildAssetPath(uniqueName, ".inkfc");

            ExportGraphDto graphDto = BuildV2ConditionGraphDto_ElseNotLast();
            WriteFile(jsonPath, JsonUtility.ToJson(graphDto, true));
            WriteFile(inkPath, "-> knot_N000\n=== knot_N000 ===\n-> END\n");

            InkFlowChartImportResult importResult = InkFlowChartImporter.ImportFromFlowchartJson(jsonPath);
            Assert.IsFalse(importResult.success, "else 不在最後的 condition 匯入應回傳失敗。");
            StringAssert.Contains("最後一個", importResult.errorMessage, "錯誤訊息應指出 else 必須在最後。");
            Assert.IsNull(AssetDatabase.LoadAssetAtPath<Object>(graphPath), "失敗時不應產生 `.inkfc`。");
        }
        // ===== 變更結束 =====

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

        // ===== 變更開始 =====
        // 2026/02/21 Opsidanos (修改原因：補齊 Graph v2 匯入測試所需 sidecar DTO 建立器)
        // 預期結果：測試可直接用最小 v2 結構描述 choice/condition，並能對應匯入器的 outputs/else 規則
        private static ExportGraphDto BuildV2ChoiceGraphDto()
        {
            return new ExportGraphDto
            {
                version = "2.0",
                graphName = "ImportV2ChoiceFixture",
                startNodeId = "N000",
                nodes = new List<ExportNodeDto>
                {
                    new ExportNodeDto
                    {
                        id = "N000",
                        type = "start",
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto { portName = "Flow", toNodeId = "N001" }
                        }
                    },
                    new ExportNodeDto
                    {
                        id = "N001",
                        type = "choice",
                        choiceMode = "*",
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto { portName = "Out0", toNodeId = "N002", label = "去 A" },
                            new ExportNodeOutputDto { portName = "Out1", toNodeId = "N003", label = "去 B" }
                        }
                    },
                    new ExportNodeDto
                    {
                        id = "N002",
                        type = "action",
                        content = "A",
                        // ===== 變更開始 =====
                        // 2026/02/22 Opsidanos (修改原因：補上 actionKind fixture，驗證匯入可還原下拉值)
                        // 預期結果：A 節點匯入後 ActionKind = Dialogue
                        actionKind = "dialogue",
                        // ===== 變更結束 =====
                        outputs = new List<ExportNodeOutputDto>()
                    },
                    new ExportNodeDto
                    {
                        id = "N003",
                        type = "action",
                        content = "B",
                        // ===== 變更開始 =====
                        // 2026/02/22 Opsidanos (修改原因：補上 actionKind fixture，驗證匯入可還原下拉值)
                        // 預期結果：B 節點匯入後 ActionKind = StageAction
                        actionKind = "action",
                        // ===== 變更結束 =====
                        outputs = new List<ExportNodeOutputDto>()
                    }
                }
            };
        }

        private static ExportGraphDto BuildV2ConditionGraphDto()
        {
            return new ExportGraphDto
            {
                version = "2.0",
                graphName = "ImportV2ConditionFixture",
                startNodeId = "N000",
                nodes = new List<ExportNodeDto>
                {
                    new ExportNodeDto
                    {
                        id = "N000",
                        type = "start",
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto { portName = "Flow", toNodeId = "N001" }
                        }
                    },
                    new ExportNodeDto
                    {
                        id = "N001",
                        type = "condition",
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto { portName = "Out0", toNodeId = "N002", condition = "favor > 7", isElse = false },
                            new ExportNodeOutputDto { portName = "Out1", toNodeId = "N003", isElse = true }
                        }
                    },
                    new ExportNodeDto
                    {
                        id = "N002",
                        type = "action",
                        content = "A",
                        // ===== 變更開始 =====
                        // 2026/02/22 Opsidanos (修改原因：補上 actionKind fixture，驗證匯入可還原下拉值)
                        // 預期結果：A 節點匯入後 ActionKind = Dialogue
                        actionKind = "dialogue",
                        // ===== 變更結束 =====
                        outputs = new List<ExportNodeOutputDto>()
                    },
                    new ExportNodeDto
                    {
                        id = "N003",
                        type = "action",
                        content = "B",
                        // ===== 變更開始 =====
                        // 2026/02/22 Opsidanos (修改原因：補上 actionKind fixture，驗證匯入可還原下拉值)
                        // 預期結果：B 節點匯入後 ActionKind = StageAction
                        actionKind = "action",
                        // ===== 變更結束 =====
                        outputs = new List<ExportNodeOutputDto>()
                    }
                }
            };
        }

        private static ExportGraphDto BuildV2ConditionGraphDto_NoElse()
        {
            return new ExportGraphDto
            {
                version = "2.0",
                graphName = "ImportV2ConditionNoElseFixture",
                startNodeId = "N000",
                nodes = new List<ExportNodeDto>
                {
                    new ExportNodeDto
                    {
                        id = "N000",
                        type = "start",
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto { portName = "Flow", toNodeId = "N001" }
                        }
                    },
                    new ExportNodeDto
                    {
                        id = "N001",
                        type = "condition",
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto { portName = "Out0", toNodeId = "N002", condition = "favor > 7", isElse = false }
                        }
                    },
                    new ExportNodeDto
                    {
                        id = "N002",
                        type = "action",
                        content = "A",
                        outputs = new List<ExportNodeOutputDto>()
                    }
                }
            };
        }

        private static ExportGraphDto BuildV2ConditionGraphDto_ElseNotLast()
        {
            return new ExportGraphDto
            {
                version = "2.0",
                graphName = "ImportV2ConditionElseNotLastFixture",
                startNodeId = "N000",
                nodes = new List<ExportNodeDto>
                {
                    new ExportNodeDto
                    {
                        id = "N000",
                        type = "start",
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto { portName = "Flow", toNodeId = "N001" }
                        }
                    },
                    new ExportNodeDto
                    {
                        id = "N001",
                        type = "condition",
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto { portName = "Out0", toNodeId = "N002", isElse = true },
                            new ExportNodeOutputDto { portName = "Out1", toNodeId = "N003", condition = "favor > 7", isElse = false }
                        }
                    },
                    new ExportNodeDto
                    {
                        id = "N002",
                        type = "action",
                        content = "A",
                        outputs = new List<ExportNodeOutputDto>()
                    },
                    new ExportNodeDto
                    {
                        id = "N003",
                        type = "action",
                        content = "B",
                        outputs = new List<ExportNodeOutputDto>()
                    }
                }
            };
        }
        // ===== 變更結束 =====

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

        // ===== 變更開始 =====
        // 2026/02/21 Opsidanos (修改原因：Graph v2 的 choice/condition 需要驗證指定輸出埠的連線)
        // 預期結果：可指定 Out0/Out1 等輸出埠名稱，驗證對應連線存在
        private static void AssertEdgeExists(INode fromNode, string fromOutputPortName, INode toNode)
        {
            IPort fromOutputPort = fromNode.GetOutputPortByName(fromOutputPortName);
            IPort toInputPort = toNode.GetInputPortByName("Flow");
            Assert.NotNull(fromOutputPort, $"來源節點必須有 `{fromOutputPortName}` 輸出埠。");
            Assert.NotNull(toInputPort, "目標節點必須有 Flow 輸入埠。");

            var connectedPorts = new List<IPort>();
            fromOutputPort.GetConnectedPorts(connectedPorts);
            bool edgeFound = connectedPorts.Any(port => port == toInputPort);
            Assert.IsTrue(edgeFound, $"匯入後應存在對應的連線（from={fromOutputPortName}）。");
        }

        private static string GetNodeOptionString(Node node, string optionName)
        {
            INodeOption option = node.GetNodeOptionByName(optionName);
            if (option == null)
            {
                return string.Empty;
            }

            if (option.TryGetValue(out string value))
            {
                return value ?? string.Empty;
            }

            return string.Empty;
        }

        private static void AssertNodeOptionValue(Node node, string optionName, int expectedValue)
        {
            INodeOption option = node.GetNodeOptionByName(optionName);
            Assert.NotNull(option, $"節點必須存在 `{optionName}` option。");

            bool readSuccess = option.TryGetValue(out int actualValue);
            Assert.IsTrue(readSuccess, $"節點 `{optionName}` option 應可讀取 int 值。");
            Assert.AreEqual(expectedValue, actualValue, $"節點 `{optionName}` option 值不符合預期。");
        }

        private static void AssertNodeOptionValue(Node node, string optionName, InkFlowChoiceMode expectedValue)
        {
            INodeOption option = node.GetNodeOptionByName(optionName);
            Assert.NotNull(option, $"節點必須存在 `{optionName}` option。");

            bool readSuccess = option.TryGetValue(out InkFlowChoiceMode actualValue);
            Assert.IsTrue(readSuccess, $"節點 `{optionName}` option 應可讀取 choice mode 值。");
            Assert.AreEqual(expectedValue, actualValue, $"節點 `{optionName}` option 值不符合預期。");
        }

        // ===== 變更開始 =====
        // 2026/02/22 Opsidanos (修改原因：新增 ActionKind 驗證需求，補上 enum 斷言工具)
        // 預期結果：測試可直接驗證 Action 節點內容類型下拉值
        private static void AssertNodeOptionValue(Node node, string optionName, InkFlowActionKind expectedValue)
        {
            INodeOption option = node.GetNodeOptionByName(optionName);
            Assert.NotNull(option, $"節點必須存在 `{optionName}` option。");

            bool readSuccess = option.TryGetValue(out InkFlowActionKind actualValue);
            Assert.IsTrue(readSuccess, $"節點 `{optionName}` option 應可讀取 action kind 值。");
            Assert.AreEqual(expectedValue, actualValue, $"節點 `{optionName}` option 值不符合預期。");
        }
        // ===== 變更結束 =====
        // ===== 變更結束 =====

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
