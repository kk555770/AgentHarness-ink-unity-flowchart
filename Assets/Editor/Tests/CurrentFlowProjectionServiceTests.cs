// ===== 變更開始 =====
// 2026/03/18 Opsidanos (修改原因：為 Batch 2 第二刀補 projection service 的純 DTO 測試，鎖住 `.flowchart.json` -> `.ink` 的共用投影規則)
// 預期結果：不需要 GraphToolkit graph，也能直接驗證 choice/comment/stageAction 對話合併的 Ink 輸出內容
using System.Collections.Generic;
using NUnit.Framework;
using OpsidanosInk.CanonicalGraph;
// ===== 變更結束 =====

namespace OpsidanosInk.Tests.EditMode
{
    public sealed class CurrentFlowProjectionServiceTests
    {
        [Test]
        public void BuildInkContent_ChoiceGraph_包含Choice語法()
        {
            // ===== 變更開始 =====
            // 2026/03/18 Opsidanos (修改原因：把 choice 的 Ink 輸出組裝搬到 shared service 後，需要鎖住最小投影語法)
            // 預期結果：choice 會輸出 `* [` 與對應 divert
            ExportGraphDto graphDto = new ExportGraphDto
            {
                startNodeId = "N000",
                nodes = new List<ExportNodeDto>
                {
                    new ExportNodeDto
                    {
                        id = "N000",
                        type = CanonicalNodeKinds.Start,
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto { portName = CanonicalPortSemantics.Flow, toNodeId = "N001", toPortName = CanonicalPortSemantics.Flow }
                        }
                    },
                    new ExportNodeDto
                    {
                        id = "N001",
                        type = CanonicalNodeKinds.Choice,
                        choiceMode = "*",
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto { portName = "Out0", toNodeId = "N002", label = "去 A" },
                            new ExportNodeOutputDto { portName = "Out1", toNodeId = "N003", label = "去 B" }
                        }
                    },
                    new ExportNodeDto { id = "N002", type = CurrentFlowProjectionNaming.DialogueNodeType, content = "A" },
                    new ExportNodeDto { id = "N003", type = CurrentFlowProjectionNaming.DialogueNodeType, content = "B" }
                }
            };

            string inkContent = CurrentFlowProjectionService.BuildInkContent(graphDto);

            StringAssert.Contains("* [去 A] -> knot_N002", inkContent);
            StringAssert.Contains("* [去 B] -> knot_N003", inkContent);
            // ===== 變更結束 =====
        }

        [Test]
        public void BuildInkContent_多行Comment_每行都會輸出註解()
        {
            // ===== 變更開始 =====
            // 2026/03/18 Opsidanos (修改原因：comment 的逐行 `//` 規則搬到 shared service 後，需要鎖住不會只有第一行加註解)
            // 預期結果：多行 comment 會逐行轉成 Ink 註解，不會讓後續行漏掉 `//`
            ExportGraphDto graphDto = new ExportGraphDto
            {
                startNodeId = "N000",
                nodes = new List<ExportNodeDto>
                {
                    new ExportNodeDto
                    {
                        id = "N000",
                        type = CanonicalNodeKinds.Start,
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto { portName = CanonicalPortSemantics.Flow, toNodeId = "N001", toPortName = CanonicalPortSemantics.Flow }
                        }
                    },
                    new ExportNodeDto
                    {
                        id = "N001",
                        type = CanonicalNodeKinds.Comment,
                        content = "第一行\n第二行"
                    }
                }
            };

            string inkContent = CurrentFlowProjectionService.BuildInkContent(graphDto);

            StringAssert.Contains("// 第一行", inkContent);
            StringAssert.Contains("// 第二行", inkContent);
            // ===== 變更結束 =====
        }

        [Test]
        public void BuildInkContent_DialogueWithStageActions_依輸入埠順序合併動作內容()
        {
            // ===== 變更開始 =====
            // 2026/03/18 Opsidanos (修改原因：對話與 stageAction 合併規則搬到 shared service 後，需要鎖住 ActionIn 順序不會亂掉)
            // 預期結果：對話前面的動作內容會依 `ActionIn0`、`ActionIn1` 順序輸出
            ExportGraphDto graphDto = new ExportGraphDto
            {
                startNodeId = "N000",
                nodes = new List<ExportNodeDto>
                {
                    new ExportNodeDto
                    {
                        id = "N000",
                        type = CanonicalNodeKinds.Start,
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto { portName = CanonicalPortSemantics.Flow, toNodeId = "N001", toPortName = CanonicalPortSemantics.Flow }
                        }
                    },
                    new ExportNodeDto
                    {
                        id = "N001",
                        type = CurrentFlowProjectionNaming.DialogueNodeType,
                        content = "主句"
                    },
                    new ExportNodeDto
                    {
                        id = "N002",
                        type = CanonicalNodeKinds.StageAction,
                        content = "動作A",
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto { portName = CanonicalPortSemantics.ActionData, toNodeId = "N001", toPortName = CanonicalPortSemantics.BuildActionInputPortName(1) }
                        }
                    },
                    new ExportNodeDto
                    {
                        id = "N003",
                        type = CanonicalNodeKinds.StageAction,
                        content = "動作B",
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto { portName = CanonicalPortSemantics.ActionData, toNodeId = "N001", toPortName = CanonicalPortSemantics.BuildActionInputPortName(0) }
                        }
                    }
                }
            };

            string inkContent = CurrentFlowProjectionService.BuildInkContent(graphDto);
            int actionBIndex = inkContent.IndexOf("動作B", System.StringComparison.Ordinal);
            int actionAIndex = inkContent.IndexOf("動作A", System.StringComparison.Ordinal);
            int dialogueIndex = inkContent.IndexOf("主句", System.StringComparison.Ordinal);

            Assert.That(actionBIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(actionAIndex, Is.GreaterThan(actionBIndex));
            Assert.That(dialogueIndex, Is.GreaterThan(actionAIndex));
            // ===== 變更結束 =====
        }
    }
}
