// ===== 變更開始 =====
// 2026/03/21 Opsidanos (修改原因：為 Batch 6 補 canonical-first projection service 測試，鎖住 `CanonicalGraphDocument -> ExportGraphDto/.ink` 入口)
// 預期結果：projection service 可直接吃 canonical graph，並維持 current projection 的 choice、condition、stageAction、dialogue 關鍵語意
using System.Collections.Generic;
using NUnit.Framework;
using OpsidanosInk.CanonicalGraph;

namespace OpsidanosInk.Tests.EditMode
{
    public sealed class CurrentFlowCanonicalProjectionServiceTests
    {
        // ===== 變更開始 =====
        // 2026/03/23 Opsidanos (修改原因：為 Batch 11A 補 target-based projection service 測試)
        // 預期結果：service 除了既有 canonical-first DTO/ink 入口外，也能穩定支援 ValidateProjection / ProjectGraph 的 target-based 語意
        [Test]
        public void TryValidateProjection_FlowchartJson_合法ChoiceGraph會成功()
        {
            CanonicalGraphDocument graph = BuildChoiceGraph();

            bool success = CurrentFlowProjectionService.TryValidateProjection(
                graph,
                CurrentFlowProjectionService.FlowchartJsonTarget,
                string.Empty,
                out string resolvedProjectionVersion,
                out string errorCode,
                out string errorMessage);

            Assert.IsTrue(success, errorMessage);
            Assert.That(errorCode, Is.Empty);
            Assert.That(resolvedProjectionVersion, Is.EqualTo("2.0"));
        }

        [Test]
        public void TryProjectGraph_FlowchartJson_會回傳CurrentProjectionJson()
        {
            CanonicalGraphDocument graph = BuildChoiceGraph();

            bool success = CurrentFlowProjectionService.TryProjectGraph(
                graph,
                CurrentFlowProjectionService.FlowchartJsonTarget,
                string.Empty,
                out string resolvedProjectionVersion,
                out string projectionText,
                out string projectionJson,
                out string errorCode,
                out string errorMessage);

            Assert.IsTrue(success, errorMessage);
            Assert.That(errorCode, Is.Empty);
            Assert.That(resolvedProjectionVersion, Is.EqualTo("2.0"));
            Assert.That(projectionText, Is.EqualTo(string.Empty));

            ExportGraphDto restored = UnityEngine.JsonUtility.FromJson<ExportGraphDto>(projectionJson);
            Assert.That(restored, Is.Not.Null);
            Assert.That(restored.startNodeId, Is.EqualTo("N000"));
            Assert.That(restored.nodes.Count, Is.EqualTo(4));
            Assert.That(restored.nodes.Find(node => node.id == "N001")?.choiceMode, Is.EqualTo("+"));
        }

        [Test]
        public void TryProjectGraph_Ink_會回傳InkText()
        {
            CanonicalGraphDocument graph = BuildChoiceGraph();

            bool success = CurrentFlowProjectionService.TryProjectGraph(
                graph,
                CurrentFlowProjectionService.InkTarget,
                string.Empty,
                out string resolvedProjectionVersion,
                out string projectionText,
                out string projectionJson,
                out string errorCode,
                out string errorMessage);

            Assert.IsTrue(success, errorMessage);
            Assert.That(errorCode, Is.EqualTo(string.Empty));
            Assert.That(resolvedProjectionVersion, Is.EqualTo("2.0"));
            Assert.That(projectionJson, Is.EqualTo(string.Empty));
            StringAssert.Contains("+ [去 A] -> knot_N002", projectionText);
            StringAssert.Contains("+ [去 B] -> knot_N003", projectionText);
        }
        // ===== 變更結束 =====

        [Test]
        public void TryBuildProjectionDto_ChoiceGraph_保留Label與Mode()
        {
            CanonicalGraphDocument graph = BuildChoiceGraph();

            bool success = CurrentFlowProjectionService.TryBuildProjectionDto(graph, out ExportGraphDto exportDto, out string errorMessage);

            Assert.IsTrue(success, errorMessage);
            Assert.That(exportDto, Is.Not.Null);
            Assert.That(exportDto.graphName, Is.EqualTo("ChoiceGraph"));
            Assert.That(exportDto.startNodeId, Is.EqualTo("N000"));

            ExportNodeDto choiceNode = exportDto.nodes.Find(node => node.id == "N001");
            Assert.That(choiceNode, Is.Not.Null);
            Assert.That(choiceNode.choiceMode, Is.EqualTo("+"));
            Assert.That(choiceNode.outputs.Count, Is.EqualTo(2));
            Assert.That(choiceNode.outputs[0].label, Is.EqualTo("去 A"));
            Assert.That(choiceNode.outputs[1].label, Is.EqualTo("去 B"));
        }

        [Test]
        public void TryBuildInkContent_ConditionGraph_保留Else與Divert()
        {
            CanonicalGraphDocument graph = BuildConditionGraph();

            bool success = CurrentFlowProjectionService.TryBuildInkContent(graph, out string inkContent, out string errorMessage);

            Assert.IsTrue(success, errorMessage);
            StringAssert.Contains("- favor > 7:", inkContent);
            StringAssert.Contains("- else:", inkContent);
            StringAssert.Contains("-> knot_N002", inkContent);
            StringAssert.Contains("-> knot_N003", inkContent);
        }

        [Test]
        public void TryBuildInkContent_DialogueWithStageActions_依ActionIn順序輸出()
        {
            CanonicalGraphDocument graph = BuildDialogueWithActionsGraph();

            bool success = CurrentFlowProjectionService.TryBuildInkContent(graph, out string inkContent, out string errorMessage);

            Assert.IsTrue(success, errorMessage);

            int actionBIndex = inkContent.IndexOf("# action:b", System.StringComparison.Ordinal);
            int actionAIndex = inkContent.IndexOf("# action:a", System.StringComparison.Ordinal);
            int dialogueIndex = inkContent.IndexOf("主句", System.StringComparison.Ordinal);

            Assert.That(actionBIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(actionAIndex, Is.GreaterThan(actionBIndex));
            Assert.That(dialogueIndex, Is.GreaterThan(actionAIndex));
        }

        private static CanonicalGraphDocument BuildChoiceGraph()
        {
            CanonicalGraphDocument graph = CanonicalGraphCommandService.CreateGraph(
                "choice-graph",
                "canonical-1",
                "{\"graphName\":\"ChoiceGraph\",\"projectionVersion\":\"2.0\"}").graph;

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
                payloadJson = "{\"labels\":[\"去 A\",\"去 B\"]}"
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

        private static CanonicalGraphDocument BuildConditionGraph()
        {
            CanonicalGraphDocument graph = CanonicalGraphCommandService.CreateGraph(
                "condition-graph",
                "canonical-1",
                "{\"graphName\":\"ConditionGraph\",\"projectionVersion\":\"2.0\"}").graph;

            CanonicalGraphCommandService.CreateNode(graph, new CanonicalGraphNodeRecord
            {
                nodeId = "N000",
                nodeType = CanonicalNodeKinds.Start
            });
            CanonicalGraphCommandService.CreateNode(graph, new CanonicalGraphNodeRecord
            {
                nodeId = "N001",
                nodeType = CanonicalNodeKinds.Condition,
                branchCount = 2,
                payloadJson = "{\"conditions\":[\"favor > 7\"]}"
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

        private static CanonicalGraphDocument BuildDialogueWithActionsGraph()
        {
            CanonicalGraphDocument graph = CanonicalGraphCommandService.CreateGraph(
                "dialogue-action-graph",
                "canonical-1",
                "{\"graphName\":\"DialogueActionGraph\",\"projectionVersion\":\"2.0\"}").graph;

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
    }
}
// ===== 變更結束 =====
