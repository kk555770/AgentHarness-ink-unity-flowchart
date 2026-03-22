// ===== 變更開始 =====
// 2026/03/21 Opsidanos (修改原因：為 Batch 7 補 canonical-first import service 測試)
// 預期結果：`CanonicalGraphDocument -> CurrentFlowImportPlan` 的主入口可直接守住成功案例、缺少 start 失敗與 choice label 特殊字元
using NUnit.Framework;
using OpsidanosInk.CanonicalGraph;

namespace OpsidanosInk.Tests.EditMode
{
    public sealed class CurrentFlowCanonicalImportServiceTests
    {
        [Test]
        public void TryBuildPlan_DialogueWithStageActions_會保留最大ActionInputOrder()
        {
            // ===== 變更開始 =====
            // 2026/03/21 Opsidanos (修改原因：canonical-first 匯入入口現在會負責先從 graph 正規化成 import plan，需要鎖住 dialogue ActionIn 序號不會漂移)
            // 預期結果：dialogue node plan 會保留最大 action input order，讓 GraphToolkit rebuild 後續能正確展開輸入埠
            CanonicalGraphDocument graph = BuildDialogueWithActionsGraph();

            bool success = CurrentFlowImportService.TryBuildPlan(graph, out CurrentFlowImportPlan plan, out string errorMessage);

            Assert.IsTrue(success, errorMessage);
            Assert.That(plan, Is.Not.Null);
            Assert.That(plan.startNodeId, Is.EqualTo("N000"));

            CurrentFlowImportNodePlan dialoguePlan = plan.nodes.Find(node => node.nodeId == "N001");
            Assert.That(dialoguePlan, Is.Not.Null);
            Assert.That(dialoguePlan.maxDialogueActionInputOrder, Is.EqualTo(1));
            // ===== 變更結束 =====
        }

        [Test]
        public void TryBuildPlan_ChoiceLabels含冒號與問號_會保留原始文字()
        {
            // ===== 變更開始 =====
            // 2026/03/21 Opsidanos (修改原因：choice label 可能含冒號、問號、驚嘆號等一般標點，需要鎖住 canonical-first 匯入入口不會把文字吃掉或變形)
            // 預期結果：import plan 內的 joined labels 仍保留原始 label 內容
            CanonicalGraphDocument graph = BuildChoiceGraph("去左邊：A?", "去右邊：B!");

            bool success = CurrentFlowImportService.TryBuildPlan(graph, out CurrentFlowImportPlan plan, out string errorMessage);

            Assert.IsTrue(success, errorMessage);
            CurrentFlowImportNodePlan choicePlan = plan.nodes.Find(node => node.nodeId == "N001");
            Assert.That(choicePlan, Is.Not.Null);
            Assert.That(choicePlan.choiceTexts, Is.EqualTo("去左邊：A?\n去右邊：B!"));
            Assert.That(choicePlan.choiceModeToken, Is.EqualTo("+"));
            // ===== 變更結束 =====
        }

        [Test]
        public void TryBuildPlan_缺少Start節點_會回傳失敗()
        {
            // ===== 變更開始 =====
            // 2026/03/21 Opsidanos (修改原因：canonical-first 匯入入口現在直接吃 canonical graph，需要鎖住缺少 start 的圖會提早失敗)
            // 預期結果：service 會回傳 false，並指出 canonical graph 驗證失敗
            CanonicalGraphDocument graph = CanonicalGraphCommandService.CreateGraph(
                "missing-start",
                "canonical-1",
                "{\"graphName\":\"MissingStart\",\"projectionVersion\":\"2.0\"}").graph;

            CanonicalGraphCommandService.CreateNode(graph, new CanonicalGraphNodeRecord
            {
                nodeId = "N001",
                nodeType = CanonicalNodeKinds.Dialogue,
                payloadJson = "{\"content\":\"主句\"}"
            });

            bool success = CurrentFlowImportService.TryBuildPlan(graph, out CurrentFlowImportPlan plan, out string errorMessage);

            Assert.IsFalse(success);
            Assert.That(plan, Is.Null);
            StringAssert.Contains("start", errorMessage);
            // ===== 變更結束 =====
        }

        private static CanonicalGraphDocument BuildDialogueWithActionsGraph()
        {
            CanonicalGraphDocument graph = CanonicalGraphCommandService.CreateGraph(
                "dialogue-action-import",
                "canonical-1",
                "{\"graphName\":\"DialogueActionImport\",\"projectionVersion\":\"2.0\"}").graph;

            CanonicalGraphCommandService.CreateNode(graph, new CanonicalGraphNodeRecord
            {
                nodeId = "N000",
                nodeType = CanonicalNodeKinds.Start
            });
            CanonicalGraphCommandService.CreateNode(graph, new CanonicalGraphNodeRecord
            {
                nodeId = "N001",
                nodeType = CanonicalNodeKinds.Dialogue,
                dialogueActionInputCount = 2,
                payloadJson = "{\"content\":\"主句\"}"
            });
            CanonicalGraphCommandService.CreateNode(graph, new CanonicalGraphNodeRecord
            {
                nodeId = "N002",
                nodeType = CanonicalNodeKinds.StageAction,
                payloadJson = "{\"content\":\"# action:a\"}"
            });
            CanonicalGraphCommandService.CreateNode(graph, new CanonicalGraphNodeRecord
            {
                nodeId = "N003",
                nodeType = CanonicalNodeKinds.StageAction,
                payloadJson = "{\"content\":\"# action:b\"}"
            });

            CanonicalGraphCommandService.ConnectPorts(graph, "N000", CanonicalPortSemantics.Flow, "N001", CanonicalPortSemantics.Flow);
            CanonicalGraphCommandService.ConnectPorts(graph, "N002", CanonicalPortSemantics.ActionData, "N001", CanonicalPortSemantics.BuildActionInputPortName(1));
            CanonicalGraphCommandService.ConnectPorts(graph, "N003", CanonicalPortSemantics.ActionData, "N001", CanonicalPortSemantics.BuildActionInputPortName(0));
            return graph;
        }

        private static CanonicalGraphDocument BuildChoiceGraph(string firstLabel, string secondLabel)
        {
            CanonicalGraphDocument graph = CanonicalGraphCommandService.CreateGraph(
                "choice-import",
                "canonical-1",
                "{\"graphName\":\"ChoiceImport\",\"projectionVersion\":\"2.0\"}").graph;

            CanonicalGraphCommandService.CreateNode(graph, new CanonicalGraphNodeRecord
            {
                nodeId = "N000",
                nodeType = CanonicalNodeKinds.Start
            });
            CanonicalGraphCommandService.CreateNode(graph, new CanonicalGraphNodeRecord
            {
                nodeId = "N001",
                nodeType = CanonicalNodeKinds.Choice,
                branchCount = 2,
                branchModeToken = "+",
                payloadJson = $"{{\"labels\":[\"{firstLabel}\",\"{secondLabel}\"]}}"
            });
            CanonicalGraphCommandService.CreateNode(graph, new CanonicalGraphNodeRecord
            {
                nodeId = "N002",
                nodeType = CanonicalNodeKinds.Dialogue,
                payloadJson = "{\"content\":\"A\"}"
            });
            CanonicalGraphCommandService.CreateNode(graph, new CanonicalGraphNodeRecord
            {
                nodeId = "N003",
                nodeType = CanonicalNodeKinds.Dialogue,
                payloadJson = "{\"content\":\"B\"}"
            });

            CanonicalGraphCommandService.ConnectPorts(graph, "N000", CanonicalPortSemantics.Flow, "N001", CanonicalPortSemantics.Flow);
            CanonicalGraphCommandService.ConnectPorts(graph, "N001", "Out0", "N002", CanonicalPortSemantics.Flow);
            CanonicalGraphCommandService.ConnectPorts(graph, "N001", "Out1", "N003", CanonicalPortSemantics.Flow);
            return graph;
        }
    }
}
// ===== 變更結束 =====
