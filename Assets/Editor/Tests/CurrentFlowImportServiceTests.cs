// ===== 變更開始 =====
// 2026/03/18 Opsidanos (修改原因：為 Batch 2 第三刀補 import service 的純 DTO 測試，鎖住 `.flowchart.json` -> import plan` 的共用整理規則)
// 預期結果：不需要 GraphToolkit graph，也能直接驗證 dialogue action input、choice 文本與 linear fallback wire 的整理結果
using System.Collections.Generic;
using NUnit.Framework;
using OpsidanosInk.CanonicalGraph;
// ===== 變更結束 =====

namespace OpsidanosInk.Tests.EditMode
{
    public sealed class CurrentFlowImportServiceTests
    {
        [Test]
        public void BuildPlan_DialogueWithStageAction_會找出最大ActionInputOrder()
        {
            // ===== 變更開始 =====
            // 2026/03/18 Opsidanos (修改原因：對話節點的動作輸入埠數量現在由 import service 先整理最大序號，需要鎖住不會漏抓 ActionIn)
            // 預期結果：dialogue node plan 會保留最大的 action input order，讓 adapter 後續能正確展開輸入埠
            ExportGraphDto graphDto = new ExportGraphDto
            {
                startNodeId = "N000",
                nodes = new List<ExportNodeDto>
                {
                    new ExportNodeDto { id = "N000", type = CanonicalNodeKinds.Start },
                    new ExportNodeDto { id = "N001", type = CurrentFlowProjectionNaming.DialogueNodeType, content = "主句" },
                    new ExportNodeDto
                    {
                        id = "N002",
                        type = CanonicalNodeKinds.StageAction,
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto
                            {
                                portName = CanonicalPortSemantics.ActionData,
                                toNodeId = "N001",
                                toPortName = CanonicalPortSemantics.BuildActionInputPortName(3)
                            }
                        }
                    }
                }
            };

            CurrentFlowImportPlan plan = CurrentFlowImportService.BuildPlan(graphDto);
            CurrentFlowImportNodePlan dialoguePlan = plan.nodes.Find(node => node.nodeId == "N001");

            Assert.That(dialoguePlan, Is.Not.Null);
            Assert.That(dialoguePlan.maxDialogueActionInputOrder, Is.EqualTo(3));
            // ===== 變更結束 =====
        }

        [Test]
        public void BuildPlan_ChoiceNode_會整理選項文字與模式()
        {
            // ===== 變更開始 =====
            // 2026/03/18 Opsidanos (修改原因：choice 的 option 文本與模式整理已從 importer 抽到 import service，需要鎖住 joined text 與 mode token)
            // 預期結果：choice node plan 會保留 outputs 數量、換行文本與 `+`/`*` 模式
            ExportGraphDto graphDto = new ExportGraphDto
            {
                startNodeId = "N000",
                nodes = new List<ExportNodeDto>
                {
                    new ExportNodeDto { id = "N000", type = CanonicalNodeKinds.Start },
                    new ExportNodeDto
                    {
                        id = "N001",
                        type = CanonicalNodeKinds.Choice,
                        choiceMode = "+",
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto { portName = "Out0", label = "去左邊" },
                            new ExportNodeOutputDto { portName = "Out1", label = "去右邊" }
                        }
                    }
                }
            };

            CurrentFlowImportPlan plan = CurrentFlowImportService.BuildPlan(graphDto);
            CurrentFlowImportNodePlan choicePlan = plan.nodes.Find(node => node.nodeId == "N001");

            Assert.That(choicePlan, Is.Not.Null);
            Assert.That(choicePlan.choiceOutputCount, Is.EqualTo(2));
            Assert.That(choicePlan.choiceTexts, Is.EqualTo("去左邊\n去右邊"));
            Assert.That(choicePlan.choiceModeToken, Is.EqualTo("+"));
            // ===== 變更結束 =====
        }

        [Test]
        public void BuildPlan_LinearNextIdsFallback_會轉成FlowWire()
        {
            // ===== 變更開始 =====
            // 2026/03/18 Opsidanos (修改原因：linear node 的 nextIds fallback 現在由 import service 正規化，需要鎖住會被轉成顯性 Flow wire)
            // 預期結果：沒有 outputs 但有 nextIds 的節點，會在 import plan 裡產生 Flow -> Flow 連線
            ExportGraphDto graphDto = new ExportGraphDto
            {
                startNodeId = "N000",
                nodes = new List<ExportNodeDto>
                {
                    new ExportNodeDto
                    {
                        id = "N000",
                        type = CanonicalNodeKinds.Start,
                        nextIds = new List<string> { "N001" }
                    },
                    new ExportNodeDto { id = "N001", type = CurrentFlowProjectionNaming.DialogueNodeType, content = "下一句" }
                }
            };

            CurrentFlowImportPlan plan = CurrentFlowImportService.BuildPlan(graphDto);

            Assert.That(plan.wires.Count, Is.EqualTo(1));
            Assert.That(plan.wires[0].fromNodeId, Is.EqualTo("N000"));
            Assert.That(plan.wires[0].fromPortName, Is.EqualTo(CanonicalPortSemantics.Flow));
            Assert.That(plan.wires[0].toNodeId, Is.EqualTo("N001"));
            Assert.That(plan.wires[0].toPortName, Is.EqualTo(CanonicalPortSemantics.Flow));
            // ===== 變更結束 =====
        }
    }
}
