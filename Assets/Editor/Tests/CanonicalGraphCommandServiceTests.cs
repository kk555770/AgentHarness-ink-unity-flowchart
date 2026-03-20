// ===== 變更開始 =====
// 2026/03/19 Opsidanos (修改原因：為 Batch 4 bridge kickoff 補上最小 command service EditMode 測試)
// 預期結果：CreateGraph / CreateNode / ConnectPorts / ValidateGraph 的最小控制面語意有固定護欄，不會在後續 bridge 接入時被悄悄改壞
using NUnit.Framework;
using OpsidanosInk.CanonicalGraph;

namespace OpsidanosInk.Tests.EditMode
{
    public sealed class CanonicalGraphCommandServiceTests
    {
        [Test]
        public void CreateGraph_合法輸入_可建立最小GraphDocument()
        {
            CanonicalGraphOperationResult result = CanonicalGraphCommandService.CreateGraph("chapter-01", "canonical-1", "{}");

            Assert.IsTrue(result.success);
            Assert.IsTrue(result.applied);
            Assert.IsNotNull(result.graph);
            Assert.AreEqual("chapter-01", result.graph.graphId);
            Assert.AreEqual("canonical-1", result.graph.version);
            Assert.AreEqual("{}", result.graph.metadataJson);
            Assert.AreEqual(0, result.graph.nodes.Count);
            Assert.AreEqual(0, result.graph.edges.Count);
        }

        [Test]
        public void CreateNode_重複NodeId_會失敗()
        {
            CanonicalGraphDocument graph = CanonicalGraphCommandService.CreateGraph("chapter-01").graph;
            CanonicalGraphNodeRecord firstNode = new CanonicalGraphNodeRecord
            {
                nodeId = "N001",
                nodeType = CanonicalNodeKinds.Start
            };
            CanonicalGraphNodeRecord duplicateNode = new CanonicalGraphNodeRecord
            {
                nodeId = "N001",
                nodeType = CanonicalNodeKinds.Dialogue
            };

            CanonicalGraphOperationResult firstResult = CanonicalGraphCommandService.CreateNode(graph, firstNode);
            CanonicalGraphOperationResult duplicateResult = CanonicalGraphCommandService.CreateNode(graph, duplicateNode);

            Assert.IsTrue(firstResult.success);
            Assert.IsTrue(firstResult.applied);
            Assert.IsFalse(duplicateResult.success);
            Assert.IsFalse(duplicateResult.applied);
            Assert.AreEqual("DUPLICATE_NODE_ID", duplicateResult.errors[0].code);
            Assert.AreEqual(1, graph.nodes.Count);
        }

        [Test]
        public void ConnectPorts_相同Edge重試_成功但不重複套用()
        {
            CanonicalGraphDocument graph = BuildLinearGraph();

            CanonicalGraphOperationResult firstConnect = CanonicalGraphCommandService.ConnectPorts(
                graph,
                "N001",
                CanonicalPortSemantics.Flow,
                "N002",
                CanonicalPortSemantics.Flow);

            CanonicalGraphOperationResult secondConnect = CanonicalGraphCommandService.ConnectPorts(
                graph,
                "N001",
                CanonicalPortSemantics.Flow,
                "N002",
                CanonicalPortSemantics.Flow);

            Assert.IsTrue(firstConnect.success);
            Assert.IsTrue(firstConnect.applied);
            Assert.IsTrue(secondConnect.success);
            Assert.IsFalse(secondConnect.applied);
            Assert.AreEqual(firstConnect.edgeId, secondConnect.edgeId);
            Assert.AreEqual(1, graph.edges.Count);
        }

        [Test]
        public void ConnectPorts_不相容PortKind_會失敗()
        {
            CanonicalGraphDocument graph = CanonicalGraphCommandService.CreateGraph("chapter-01").graph;
            CanonicalGraphCommandService.CreateNode(graph, new CanonicalGraphNodeRecord
            {
                nodeId = "N001",
                nodeType = CanonicalNodeKinds.Start
            });
            CanonicalGraphCommandService.CreateNode(graph, new CanonicalGraphNodeRecord
            {
                nodeId = "N002",
                nodeType = CanonicalNodeKinds.StageAction
            });

            CanonicalGraphOperationResult result = CanonicalGraphCommandService.ConnectPorts(
                graph,
                "N001",
                CanonicalPortSemantics.Flow,
                "N002",
                CanonicalPortSemantics.ActionData);

            Assert.IsFalse(result.success);
            Assert.AreEqual("INVALID_PORT", result.errors[0].code);
        }

        [Test]
        public void ValidateGraph_缺少Start_回傳Invalid但Success()
        {
            CanonicalGraphDocument graph = CanonicalGraphCommandService.CreateGraph("chapter-01").graph;
            CanonicalGraphCommandService.CreateNode(graph, new CanonicalGraphNodeRecord
            {
                nodeId = "N002",
                nodeType = CanonicalNodeKinds.Dialogue
            });

            CanonicalGraphValidationResult result = CanonicalGraphCommandService.ValidateGraph(graph);

            Assert.IsTrue(result.success);
            Assert.IsFalse(result.isValid);
            Assert.AreEqual("MISSING_START_NODE", result.errors[0].code);
        }

        [Test]
        public void ValidateGraph_多個Start_回傳對應錯誤()
        {
            CanonicalGraphDocument graph = CanonicalGraphCommandService.CreateGraph("chapter-01").graph;
            CanonicalGraphCommandService.CreateNode(graph, new CanonicalGraphNodeRecord
            {
                nodeId = "N001",
                nodeType = CanonicalNodeKinds.Start
            });
            CanonicalGraphCommandService.CreateNode(graph, new CanonicalGraphNodeRecord
            {
                nodeId = "N002",
                nodeType = CanonicalNodeKinds.Start
            });

            CanonicalGraphValidationResult result = CanonicalGraphCommandService.ValidateGraph(graph);

            Assert.IsTrue(result.success);
            Assert.IsFalse(result.isValid);
            Assert.AreEqual("MULTIPLE_START_NODES", result.errors[0].code);
        }

        [Test]
        public void CreateNode_Condition缺Else_會直接失敗()
        {
            CanonicalGraphDocument graph = CanonicalGraphCommandService.CreateGraph("chapter-01").graph;
            CanonicalGraphOperationResult result = CanonicalGraphCommandService.CreateNode(graph, new CanonicalGraphNodeRecord
            {
                nodeId = "N002",
                nodeType = CanonicalNodeKinds.Condition,
                branchCount = 1
            });

            Assert.IsFalse(result.success);
            Assert.IsFalse(result.applied);
            Assert.AreEqual("ELSE_BRANCH_MISSING", result.errors[0].code);
            Assert.AreEqual(0, graph.nodes.Count);
        }

        [Test]
        public void ValidateGraph_手動塞入缺Else的Condition_仍會回傳對應錯誤()
        {
            CanonicalGraphDocument graph = CanonicalGraphCommandService.CreateGraph("chapter-01").graph;
            CanonicalGraphCommandService.CreateNode(graph, new CanonicalGraphNodeRecord
            {
                nodeId = "N001",
                nodeType = CanonicalNodeKinds.Start
            });
            graph.nodes.Add(new CanonicalGraphNodeRecord
            {
                nodeId = "N002",
                nodeType = CanonicalNodeKinds.Condition,
                branchCount = 1
            });

            CanonicalGraphValidationResult result = CanonicalGraphCommandService.ValidateGraph(graph);

            Assert.IsTrue(result.success);
            Assert.IsFalse(result.isValid);
            Assert.AreEqual("ELSE_BRANCH_MISSING", result.errors[0].code);
        }

        private static CanonicalGraphDocument BuildLinearGraph()
        {
            CanonicalGraphDocument graph = CanonicalGraphCommandService.CreateGraph("chapter-01").graph;
            CanonicalGraphCommandService.CreateNode(graph, new CanonicalGraphNodeRecord
            {
                nodeId = "N001",
                nodeType = CanonicalNodeKinds.Start
            });
            CanonicalGraphCommandService.CreateNode(graph, new CanonicalGraphNodeRecord
            {
                nodeId = "N002",
                nodeType = CanonicalNodeKinds.Dialogue
            });
            return graph;
        }
    }
}
// ===== 變更結束 =====
