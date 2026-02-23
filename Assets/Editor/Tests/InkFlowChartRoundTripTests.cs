// ===== 變更開始 =====
// 2026/02/21 Opsidanos (修改原因：依輸出契約 §6，補上「匯入→匯出」round-trip 回歸測試，正式鎖住 Graph v2 閉環可逆)
// 預期結果：`.flowchart.json + .ink` 匯入 `.inkfc` 後再匯出，choice/condition 的 outputs/label/condition/else 規則不會變形，且匯出 `.ink` 仍可編譯
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using OpsidanosInk.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace OpsidanosInk.Tests.EditMode
{
    // ===== 變更開始 =====
    // 2026/02/23 Opsidanos (修改原因：建立 GraphToolkit 專用測試分類與 60 秒上限，避免測試卡死拖垮 Unity/MCP)
    // 預期結果：此類別所有測試可被安全入口精準篩選，且單測最長 60 秒逾時失敗
    [Category("GraphToolkitFlowSafe")]
    [Timeout(60000)]
    // ===== 變更結束 =====
    public sealed class InkFlowChartRoundTripTests
    {
        private const string TempAssetPrefix = "Assets/Editor/TmpGraphToolkitRoundTrip_";

        [UnityTearDown]
        public IEnumerator TearDown()
        {
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
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        // ===== 變更開始 =====
        // 2026/02/22 Opsidanos (修改原因：避免背景 Ink 編譯與手動編譯並行，造成測試期間出現競態例外)
        // 預期結果：RoundTrip 測試在手動編譯前先等待背景編譯結束，維持檢驗穩定性
        [UnityTest]
        public IEnumerator RoundTrip_GraphV2Choice_匯入再匯出_仍保持結構與選項資料()
        {
            string uniqueName = BuildUniqueName("RoundTripV2Choice");
            string jsonPath = BuildAssetPath(uniqueName, ".flowchart.json");
            string inkPath = BuildAssetPath(uniqueName, ".ink");
            string graphPath = BuildAssetPath(uniqueName, ".inkfc");

            ExportGraphDto graphDto = BuildV2ChoiceGraphDto();
            WriteFile(jsonPath, JsonUtility.ToJson(graphDto, true));
            WriteFile(inkPath, "-> knot_N000\n=== knot_N000 ===\n-> END\n");

            InkFlowChartImportResult importResult = InkFlowChartImporter.ImportFromFlowchartJson(jsonPath);
            Assert.IsTrue(importResult.success, $"匯入應成功，但失敗：{importResult.errorMessage}");
            Assert.AreEqual(graphPath, importResult.graphAssetPath, "匯入輸出路徑應為同目錄同名 `.inkfc`。");

            InkFlowChartExportResult exportResult = InkFlowChartExporter.ExportGraphAsset(importResult.graphAssetPath);
            Assert.IsTrue(exportResult.success, $"匯出應成功，但失敗：{exportResult.errorMessage}");
            Assert.IsTrue(File.Exists(exportResult.jsonOutputPath), "匯出後必須存在 `.flowchart.json`。");
            Assert.IsTrue(File.Exists(exportResult.inkOutputPath), "匯出後必須存在 `.ink`。");

            ExportGraphDto exportedDto = ParseExportGraphDto(exportResult.jsonOutputPath);
            Assert.AreEqual("2.0", exportedDto.version, "round-trip 後仍應維持 Graph v2 版本。");

            ExportNodeDto startNode = GetStartNode(exportedDto);
            ExportNodeDto choiceNode = exportedDto.nodes.Single(node => node != null && node.type == "choice");
            ExportNodeDto actionA = exportedDto.nodes.Single(node => node != null && node.type == "action" && node.content == "A");
            ExportNodeDto actionB = exportedDto.nodes.Single(node => node != null && node.type == "action" && node.content == "B");

            Assert.AreEqual("*", choiceNode.choiceMode, "choiceMode 應維持 Once（*）。");
            Assert.AreEqual(2, choiceNode.outputs.Count, "choice 節點輸出埠數量應維持 2。");

            ExportNodeOutputDto out0 = choiceNode.outputs.Single(output => output != null && output.portName == "Out0");
            ExportNodeOutputDto out1 = choiceNode.outputs.Single(output => output != null && output.portName == "Out1");

            Assert.AreEqual("去 A", out0.label, "Out0 label 應維持一致。");
            Assert.AreEqual("去 B", out1.label, "Out1 label 應維持一致。");
            Assert.AreEqual(actionA.id, out0.toNodeId, "Out0 應指向 action A。");
            Assert.AreEqual(actionB.id, out1.toNodeId, "Out1 應指向 action B。");
            // ===== 變更開始 =====
            // 2026/02/22 Opsidanos (修改原因：RoundTrip 需驗證 actionKind 不會在匯入→匯出過程遺失)
            // 預期結果：Action A/B 的 actionKind 維持 dialogue/action
            Assert.AreEqual("dialogue", actionA.actionKind, "Action A 的 actionKind 應維持 dialogue。");
            Assert.AreEqual("action", actionB.actionKind, "Action B 的 actionKind 應維持 action。");
            // ===== 變更結束 =====

            Assert.AreEqual(1, startNode.outputs.Count, "start 節點應只有 1 條 Flow 輸出。");
            Assert.AreEqual(choiceNode.id, startNode.outputs[0].toNodeId, "start 的 Flow 應指向 choice。");

            yield return WaitForInkCompilerIdle("RoundTrip_GraphV2Choice");
            string exportedInk = File.ReadAllText(exportResult.inkOutputPath);
            AssertInkCompiles(exportedInk);
        }

        [UnityTest]
        public IEnumerator RoundTrip_GraphV2Condition_匯入再匯出_仍保持條件與Else規則()
        {
            string uniqueName = BuildUniqueName("RoundTripV2Condition");
            string jsonPath = BuildAssetPath(uniqueName, ".flowchart.json");
            string inkPath = BuildAssetPath(uniqueName, ".ink");
            string graphPath = BuildAssetPath(uniqueName, ".inkfc");

            ExportGraphDto graphDto = BuildV2ConditionGraphDto();
            WriteFile(jsonPath, JsonUtility.ToJson(graphDto, true));
            WriteFile(inkPath, "-> knot_N000\n=== knot_N000 ===\n-> END\n");

            InkFlowChartImportResult importResult = InkFlowChartImporter.ImportFromFlowchartJson(jsonPath);
            Assert.IsTrue(importResult.success, $"匯入應成功，但失敗：{importResult.errorMessage}");
            Assert.AreEqual(graphPath, importResult.graphAssetPath, "匯入輸出路徑應為同目錄同名 `.inkfc`。");

            InkFlowChartExportResult exportResult = InkFlowChartExporter.ExportGraphAsset(importResult.graphAssetPath);
            Assert.IsTrue(exportResult.success, $"匯出應成功，但失敗：{exportResult.errorMessage}");
            Assert.IsTrue(File.Exists(exportResult.jsonOutputPath), "匯出後必須存在 `.flowchart.json`。");
            Assert.IsTrue(File.Exists(exportResult.inkOutputPath), "匯出後必須存在 `.ink`。");

            ExportGraphDto exportedDto = ParseExportGraphDto(exportResult.jsonOutputPath);
            Assert.AreEqual("2.0", exportedDto.version, "round-trip 後仍應維持 Graph v2 版本。");

            ExportNodeDto startNode = GetStartNode(exportedDto);
            ExportNodeDto conditionNode = exportedDto.nodes.Single(node => node != null && node.type == "condition");
            ExportNodeDto actionA = exportedDto.nodes.Single(node => node != null && node.type == "action" && node.content == "A");
            ExportNodeDto actionB = exportedDto.nodes.Single(node => node != null && node.type == "action" && node.content == "B");

            Assert.AreEqual(2, conditionNode.outputs.Count, "condition 節點輸出埠數量應維持 2（含 else）。");

            ExportNodeOutputDto out0 = conditionNode.outputs.Single(output => output != null && output.portName == "Out0");
            ExportNodeOutputDto out1 = conditionNode.outputs.Single(output => output != null && output.portName == "Out1");

            Assert.IsFalse(out0.isElse, "Out0 不應是 else。");
            Assert.AreEqual("8 > 7", out0.condition, "Out0 condition 應維持一致。");
            Assert.AreEqual(actionA.id, out0.toNodeId, "Out0 應指向 action A。");

            Assert.IsTrue(out1.isElse, "Out1 應是 else。");
            Assert.IsTrue(string.IsNullOrEmpty(out1.condition), "else output 不應包含 condition。");
            Assert.AreEqual(actionB.id, out1.toNodeId, "else 應指向 action B。");
            // ===== 變更開始 =====
            // 2026/02/22 Opsidanos (修改原因：RoundTrip 需驗證 condition 路徑的 actionKind 不會遺失)
            // 預期結果：Action A/B 的 actionKind 維持 dialogue/action
            Assert.AreEqual("dialogue", actionA.actionKind, "Action A 的 actionKind 應維持 dialogue。");
            Assert.AreEqual("action", actionB.actionKind, "Action B 的 actionKind 應維持 action。");
            // ===== 變更結束 =====

            Assert.AreEqual(1, startNode.outputs.Count, "start 節點應只有 1 條 Flow 輸出。");
            Assert.AreEqual(conditionNode.id, startNode.outputs[0].toNodeId, "start 的 Flow 應指向 condition。");

            yield return WaitForInkCompilerIdle("RoundTrip_GraphV2Condition");
            string exportedInk = File.ReadAllText(exportResult.inkOutputPath);
            AssertInkCompiles(exportedInk);
        }
        // ===== 變更結束 =====

        // ===== 變更開始 =====
        // 2026/02/22 Opsidanos (修改原因：提供可重用的等待函式，統一處理 RoundTrip 測試中的 Ink 背景編譯同步)
        // 預期結果：在手動編譯檢驗前，能可靠等到背景編譯佇列清空
        private static IEnumerator WaitForInkCompilerIdle(string context)
        {
            int waitFrameCount = 0;
            while (IsInkCompilerExecutingCompilationStack())
            {
                if (waitFrameCount > 600)
                {
                    Assert.Fail($"等待 Ink 編譯器空閒逾時：{context}");
                }

                waitFrameCount++;
                yield return null;
            }
        }
        // ===== 變更結束 =====

        private static ExportGraphDto ParseExportGraphDto(string jsonAssetPath)
        {
            Assert.IsFalse(string.IsNullOrEmpty(jsonAssetPath), "jsonAssetPath 不可為空。");
            Assert.IsTrue(File.Exists(jsonAssetPath), $"找不到 JSON 檔案：{jsonAssetPath}");

            string jsonContent = File.ReadAllText(jsonAssetPath);
            ExportGraphDto dto = JsonUtility.FromJson<ExportGraphDto>(jsonContent);
            Assert.NotNull(dto, "JSON 解析結果不應為 null。");
            Assert.NotNull(dto.nodes, "JSON nodes 不應為 null。");
            return dto;
        }

        private static ExportNodeDto GetStartNode(ExportGraphDto dto)
        {
            Assert.IsFalse(string.IsNullOrEmpty(dto.startNodeId), "startNodeId 不可為空。");
            ExportNodeDto startNode = dto.nodes.Single(node => node != null && node.id == dto.startNodeId);
            Assert.NotNull(startNode, "startNodeId 對應節點不可為 null。");
            Assert.AreEqual("start", startNode.type, "startNodeId 必須指向 start 節點。");
            Assert.NotNull(startNode.outputs, "start 節點 outputs 不可為 null。");
            return startNode;
        }

        private static ExportGraphDto BuildV2ChoiceGraphDto()
        {
            return new ExportGraphDto
            {
                version = "2.0",
                graphName = "RoundTripV2ChoiceFixture",
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
                        // 2026/02/22 Opsidanos (修改原因：補上 actionKind fixture，驗證 round-trip 可保留內容類型下拉)
                        // 預期結果：Action A 匯出後維持 dialogue
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
                        // 2026/02/22 Opsidanos (修改原因：補上 actionKind fixture，驗證 round-trip 可保留內容類型下拉)
                        // 預期結果：Action B 匯出後維持 action
                        actionKind = "action",
                        // ===== 變更結束 =====
                        outputs = new List<ExportNodeOutputDto>()
                    }
                }
            };
        }

        private static ExportGraphDto BuildV2ConditionGraphDto()
        {
            // ===== 變更開始 =====
            // 2026/02/22 Opsidanos (修改原因：RoundTrip fixture 的 condition 改為純常數比較，避免測試依賴外部變數宣告時序)
            // 預期結果：fixture 仍驗證 condition/else 結構，且匯出 .ink 可穩定編譯
            return new ExportGraphDto
            {
                version = "2.0",
                graphName = "RoundTripV2ConditionFixture",
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
                            new ExportNodeOutputDto { portName = "Out0", toNodeId = "N002", condition = "8 > 7", isElse = false },
                            new ExportNodeOutputDto { portName = "Out1", toNodeId = "N003", isElse = true }
                        }
                    },
                    new ExportNodeDto
                    {
                        id = "N002",
                        type = "action",
                        content = "A",
                        // ===== 變更開始 =====
                        // 2026/02/22 Opsidanos (修改原因：補上 actionKind fixture，驗證 round-trip 可保留內容類型下拉)
                        // 預期結果：Action A 匯出後維持 dialogue
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
                        // 2026/02/22 Opsidanos (修改原因：補上 actionKind fixture，驗證 round-trip 可保留內容類型下拉)
                        // 預期結果：Action B 匯出後維持 action
                        actionKind = "action",
                        // ===== 變更結束 =====
                        outputs = new List<ExportNodeOutputDto>()
                    }
                }
            };
            // ===== 變更結束 =====
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
            // ===== 變更開始 =====
            // 2026/02/22 Opsidanos (修改原因：先報出編譯錯誤清單，避免 story 為 null 時看不到真正錯因)
            // 預期結果：測試失敗時能直接看到 Ink 編譯錯誤內容，縮短定位時間
            Assert.IsTrue(errors.Count == 0, $"Ink 編譯不應出現 Error：{string.Join(" | ", errors)}");
            Assert.IsNotNull(story, $"Ink 編譯結果不應為 null。Warnings: {string.Join(" | ", warnings)}");
            // ===== 變更結束 =====

            string storyJson = story.ToJson();
            Assert.IsFalse(string.IsNullOrEmpty(storyJson), "編譯後的 Story JSON 不應為空。");
        }

        private static bool IsInkCompilerExecutingCompilationStack()
        {
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
        }

        private static string BuildUniqueName(string prefix)
        {
            return $"{prefix}_{Guid.NewGuid():N}";
        }

        private static string BuildAssetPath(string uniqueName, string extension)
        {
            return $"{TempAssetPrefix}{uniqueName}{extension}";
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

            string[] tempPaths = Directory.GetFiles(editorDirectoryPath, "TmpGraphToolkitRoundTrip_*", SearchOption.TopDirectoryOnly);
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
