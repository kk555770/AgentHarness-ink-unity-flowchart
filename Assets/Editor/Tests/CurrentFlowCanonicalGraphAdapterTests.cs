// ===== 變更開始 =====
// 2026/03/21 Opsidanos (修改原因：為 Batch 5 的 canonical/current projection bridge 補純資料測試)
// 預期結果：不依賴 GraphToolkit graph，也能先守住 `ExportGraphDto <-> CanonicalGraphDocument` 的關鍵 mapping 不漂移
using System.Collections.Generic;
using NUnit.Framework;
using OpsidanosInk.CanonicalGraph;

namespace OpsidanosInk.Tests.EditMode
{
    public sealed class CurrentFlowCanonicalGraphAdapterTests
    {
        [Test]
        public void TryBuildCanonicalGraph_DialogueWithStageActions_保留內容與資料線順序()
        {
            ExportGraphDto graphDto = new ExportGraphDto
            {
                version = "2.0",
                graphName = "BridgeFixture",
                startNodeId = "N000",
                nodes = new List<ExportNodeDto>
                {
                    new ExportNodeDto
                    {
                        id = "N000",
                        type = CanonicalNodeKinds.Start,
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto
                            {
                                portName = CanonicalPortSemantics.Flow,
                                toNodeId = "N001",
                                toPortName = CanonicalPortSemantics.Flow
                            }
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
                        content = "# action:a",
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto
                            {
                                portName = CanonicalPortSemantics.ActionData,
                                toNodeId = "N001",
                                toPortName = CanonicalPortSemantics.BuildActionInputPortName(0)
                            }
                        }
                    },
                    new ExportNodeDto
                    {
                        id = "N003",
                        type = CanonicalNodeKinds.StageAction,
                        content = "# action:b",
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto
                            {
                                portName = CanonicalPortSemantics.ActionData,
                                toNodeId = "N001",
                                toPortName = CanonicalPortSemantics.BuildActionInputPortName(1)
                            }
                        }
                    }
                }
            };

            bool success = CurrentFlowCanonicalGraphAdapter.TryBuildCanonicalGraph(graphDto, out CanonicalGraphDocument graph, out string errorMessage);

            Assert.IsTrue(success, errorMessage);
            Assert.That(graph, Is.Not.Null);
            Assert.That(graph.nodes.Count, Is.EqualTo(4));
            Assert.That(graph.edges.Count, Is.EqualTo(3));

            CanonicalGraphNodeRecord dialogueNode = graph.nodes.Find(node => node.nodeId == "N001");
            Assert.That(dialogueNode, Is.Not.Null);
            Assert.That(dialogueNode.nodeType, Is.EqualTo(CanonicalNodeKinds.Dialogue));
            Assert.That(dialogueNode.dialogueActionInputCount, Is.EqualTo(2));
        }

        [Test]
        public void TryBuildProjection_ChoiceGraph_保留Label與Mode()
        {
            CanonicalGraphDocument graph = BuildChoiceGraph();

            bool success = CurrentFlowCanonicalGraphAdapter.TryBuildProjection(graph, out ExportGraphDto exportDto, out string errorMessage);

            Assert.IsTrue(success, errorMessage);
            Assert.That(exportDto, Is.Not.Null);
            Assert.That(exportDto.graphName, Is.EqualTo("ChoiceGraph"));
            Assert.That(exportDto.startNodeId, Is.EqualTo("N000"));

            ExportNodeDto choiceNode = exportDto.nodes.Find(node => node.id == "N001");
            Assert.That(choiceNode, Is.Not.Null);
            Assert.That(choiceNode.type, Is.EqualTo(CanonicalNodeKinds.Choice));
            Assert.That(choiceNode.choiceMode, Is.EqualTo("+"));
            Assert.That(choiceNode.outputs.Count, Is.EqualTo(2));
            Assert.That(choiceNode.outputs[0].label, Is.EqualTo("去 A"));
            Assert.That(choiceNode.outputs[1].label, Is.EqualTo("去 B"));
            Assert.That(choiceNode.outputs[0].toNodeId, Is.EqualTo("N002"));
            Assert.That(choiceNode.outputs[1].toNodeId, Is.EqualTo("N003"));
        }

        [Test]
        public void TryBuildProjection_ConditionGraph_保留Else在最後()
        {
            CanonicalGraphDocument graph = BuildConditionGraph();

            bool success = CurrentFlowCanonicalGraphAdapter.TryBuildProjection(graph, out ExportGraphDto exportDto, out string errorMessage);

            Assert.IsTrue(success, errorMessage);
            ExportNodeDto conditionNode = exportDto.nodes.Find(node => node.id == "N001");
            Assert.That(conditionNode, Is.Not.Null);
            Assert.That(conditionNode.outputs.Count, Is.EqualTo(2));
            Assert.IsFalse(conditionNode.outputs[0].isElse);
            Assert.That(conditionNode.outputs[0].condition, Is.EqualTo("favor > 7"));
            Assert.IsTrue(conditionNode.outputs[1].isElse);
            Assert.That(conditionNode.outputs[1].condition, Is.Empty);
        }

        [Test]
        public void RoundTrip_DtoToCanonicalToDto_保留DialogueStageAction與Condition關鍵語意()
        {
            ExportGraphDto sourceDto = BuildRoundTripGraphDto();

            bool buildGraphSuccess = CurrentFlowCanonicalGraphAdapter.TryBuildCanonicalGraph(sourceDto, out CanonicalGraphDocument graph, out string buildGraphError);
            Assert.IsTrue(buildGraphSuccess, buildGraphError);

            bool projectSuccess = CurrentFlowCanonicalGraphAdapter.TryBuildProjection(graph, out ExportGraphDto projectedDto, out string projectError);
            Assert.IsTrue(projectSuccess, projectError);

            ExportNodeDto dialogueNode = projectedDto.nodes.Find(node => node.id == "N001");
            ExportNodeDto stageActionA = projectedDto.nodes.Find(node => node.id == "N002");
            ExportNodeDto stageActionB = projectedDto.nodes.Find(node => node.id == "N003");
            ExportNodeDto conditionNode = projectedDto.nodes.Find(node => node.id == "N004");

            Assert.That(dialogueNode.type, Is.EqualTo(CurrentFlowProjectionNaming.DialogueNodeType));
            Assert.That(dialogueNode.content, Is.EqualTo("主句"));
            Assert.That(stageActionA.outputs[0].toPortName, Is.EqualTo(CanonicalPortSemantics.BuildActionInputPortName(0)));
            Assert.That(stageActionB.outputs[0].toPortName, Is.EqualTo(CanonicalPortSemantics.BuildActionInputPortName(1)));
            Assert.That(conditionNode.outputs[0].condition, Is.EqualTo("favor > 7"));
            Assert.IsTrue(conditionNode.outputs[1].isElse);
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

        private static ExportGraphDto BuildRoundTripGraphDto()
        {
            return new ExportGraphDto
            {
                version = "2.0",
                graphName = "RoundTripBridgeFixture",
                startNodeId = "N000",
                nodes = new List<ExportNodeDto>
                {
                    new ExportNodeDto
                    {
                        id = "N000",
                        type = CanonicalNodeKinds.Start,
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto
                            {
                                portName = CanonicalPortSemantics.Flow,
                                toNodeId = "N001",
                                toPortName = CanonicalPortSemantics.Flow
                            }
                        }
                    },
                    new ExportNodeDto
                    {
                        id = "N001",
                        type = CurrentFlowProjectionNaming.DialogueNodeType,
                        content = "主句",
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto
                            {
                                portName = CanonicalPortSemantics.Flow,
                                toNodeId = "N004",
                                toPortName = CanonicalPortSemantics.Flow
                            }
                        }
                    },
                    new ExportNodeDto
                    {
                        id = "N002",
                        type = CanonicalNodeKinds.StageAction,
                        content = "# action:a",
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto
                            {
                                portName = CanonicalPortSemantics.ActionData,
                                toNodeId = "N001",
                                toPortName = CanonicalPortSemantics.BuildActionInputPortName(0)
                            }
                        }
                    },
                    new ExportNodeDto
                    {
                        id = "N003",
                        type = CanonicalNodeKinds.StageAction,
                        content = "# action:b",
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto
                            {
                                portName = CanonicalPortSemantics.ActionData,
                                toNodeId = "N001",
                                toPortName = CanonicalPortSemantics.BuildActionInputPortName(1)
                            }
                        }
                    },
                    new ExportNodeDto
                    {
                        id = "N004",
                        type = CanonicalNodeKinds.Condition,
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto
                            {
                                portName = "Out0",
                                toNodeId = "N005",
                                toPortName = CanonicalPortSemantics.Flow,
                                condition = "favor > 7"
                            },
                            new ExportNodeOutputDto
                            {
                                portName = "Out1",
                                toNodeId = "N006",
                                toPortName = CanonicalPortSemantics.Flow,
                                isElse = true
                            }
                        }
                    },
                    new ExportNodeDto { id = "N005", type = CurrentFlowProjectionNaming.DialogueNodeType, content = "A" },
                    new ExportNodeDto { id = "N006", type = CurrentFlowProjectionNaming.DialogueNodeType, content = "B" }
                }
            };
        }
    }
}
// ===== 變更結束 =====
