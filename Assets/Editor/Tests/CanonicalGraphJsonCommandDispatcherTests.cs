// ===== 變更開始 =====
// 2026/03/22 Opsidanos (修改原因：為 Batch 8 的 JSON command bridge 補上 dispatcher 行為測試)
// 預期結果：`CreateGraph / CreateNode / ConnectPorts / ValidateGraph` 可透過 JSON request 正確驅動 canonical core，且重試 / 衝突語意不漂移
using NUnit.Framework;
using OpsidanosInk.CanonicalGraph;
using UnityEngine;

namespace OpsidanosInk.Tests.EditMode
{
    public sealed class CanonicalGraphJsonCommandDispatcherTests
    {
        [Test]
        public void Dispatch_CreateGraph_空白Version會套用預設值()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = new CanonicalGraphJsonCommandDispatcher();
            CanonicalGraphJsonRequest request = new CanonicalGraphJsonRequest
            {
                operation = "CreateGraph",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = "chapter-01"
                }
            };

            CanonicalGraphJsonResponse response = Dispatch(dispatcher, request);

            Assert.IsTrue(response.success);
            Assert.IsTrue(response.applied);
            Assert.That(response.result.graphId, Is.EqualTo("chapter-01"));
            Assert.That(response.result.version, Is.EqualTo("canonical-1"));
        }

        [Test]
        public void Dispatch_CreateGraph_重複GraphId_會失敗而不是覆蓋()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = new CanonicalGraphJsonCommandDispatcher();

            CanonicalGraphJsonResponse firstResponse = Dispatch(dispatcher, BuildCreateGraphRequest("chapter-01"));
            CanonicalGraphJsonResponse secondResponse = Dispatch(dispatcher, BuildCreateGraphRequest("chapter-01"));

            Assert.IsTrue(firstResponse.success);
            Assert.IsFalse(secondResponse.success);
            Assert.IsFalse(secondResponse.applied);
            Assert.That(secondResponse.errors[0].code, Is.EqualTo("DUPLICATE_GRAPH_ID"));
        }

        [Test]
        public void Dispatch_CreateNode_DialoguePayload_會回傳NodeId()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = new CanonicalGraphJsonCommandDispatcher();
            Dispatch(dispatcher, BuildCreateGraphRequest("chapter-01"));

            CanonicalGraphJsonRequest request = new CanonicalGraphJsonRequest
            {
                operation = "CreateNode",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = "chapter-01",
                    node = new CanonicalGraphJsonNodeInput
                    {
                        nodeId = "N001",
                        nodeType = CanonicalNodeKinds.Dialogue,
                        payload = new CanonicalGraphJsonNodePayload
                        {
                            content = "你好。"
                        }
                    }
                }
            };

            CanonicalGraphJsonResponse response = Dispatch(dispatcher, request);

            Assert.IsTrue(response.success);
            Assert.IsTrue(response.applied);
            Assert.That(response.result.graphId, Is.EqualTo("chapter-01"));
            Assert.That(response.result.nodeId, Is.EqualTo("N001"));
        }

        [Test]
        public void Dispatch_ConnectPorts_相同Edge重送_第二次AppliedFalse()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = new CanonicalGraphJsonCommandDispatcher();
            Dispatch(dispatcher, BuildCreateGraphRequest("chapter-01"));
            Dispatch(dispatcher, BuildCreateNodeRequest("chapter-01", "N001", CanonicalNodeKinds.Start));
            Dispatch(dispatcher, BuildCreateNodeRequest("chapter-01", "N002", CanonicalNodeKinds.Dialogue, "主句"));

            CanonicalGraphJsonRequest request = BuildConnectPortsRequest("chapter-01", "N001", CanonicalPortSemantics.Flow, "N002", CanonicalPortSemantics.Flow);

            CanonicalGraphJsonResponse firstResponse = Dispatch(dispatcher, request);
            CanonicalGraphJsonResponse secondResponse = Dispatch(dispatcher, request);

            Assert.IsTrue(firstResponse.success);
            Assert.IsTrue(firstResponse.applied);
            Assert.IsTrue(secondResponse.success);
            Assert.IsFalse(secondResponse.applied);
            Assert.That(secondResponse.result.edgeId, Is.EqualTo(firstResponse.result.edgeId));
        }

        [Test]
        public void Dispatch_ConnectPorts_輸出埠已被占用_會回傳對應錯誤()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = new CanonicalGraphJsonCommandDispatcher();
            Dispatch(dispatcher, BuildCreateGraphRequest("chapter-01"));
            Dispatch(dispatcher, BuildCreateNodeRequest("chapter-01", "N001", CanonicalNodeKinds.Start));
            Dispatch(dispatcher, BuildCreateNodeRequest("chapter-01", "N002", CanonicalNodeKinds.Dialogue, "A"));
            Dispatch(dispatcher, BuildCreateNodeRequest("chapter-01", "N003", CanonicalNodeKinds.Dialogue, "B"));

            CanonicalGraphJsonResponse firstResponse = Dispatch(dispatcher, BuildConnectPortsRequest("chapter-01", "N001", CanonicalPortSemantics.Flow, "N002", CanonicalPortSemantics.Flow));
            CanonicalGraphJsonResponse secondResponse = Dispatch(dispatcher, BuildConnectPortsRequest("chapter-01", "N001", CanonicalPortSemantics.Flow, "N003", CanonicalPortSemantics.Flow));

            Assert.IsTrue(firstResponse.success);
            Assert.IsFalse(secondResponse.success);
            Assert.That(secondResponse.errors[0].code, Is.EqualTo("EDGE_CARDINALITY_VIOLATION"));
        }

        // ===== 變更開始 =====
        // 2026/03/22 Opsidanos (修改原因：為 Batch 10 的 JSON mutation bridge 補上 dispatcher 測試)
        // 預期結果：ReplaceNodePayload / DisconnectEdge / RemoveNode 透過 JSON request 呼叫時，會維持 strict replace、idempotent disconnect 與同步清 edge 的語意
        [Test]
        public void Dispatch_ReplaceNodePayload_更新Choice內容後GetGraph會反映新分支數()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = new CanonicalGraphJsonCommandDispatcher();
            Dispatch(dispatcher, BuildCreateGraphRequest("chapter-01"));
            Dispatch(dispatcher, new CanonicalGraphJsonRequest
            {
                operation = "CreateNode",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = "chapter-01",
                    node = new CanonicalGraphJsonNodeInput
                    {
                        nodeId = "N001",
                        nodeType = CanonicalNodeKinds.Choice,
                        branchModeToken = "+",
                        payload = new CanonicalGraphJsonNodePayload
                        {
                            labels = { "去A", "去B" }
                        }
                    }
                }
            });

            CanonicalGraphJsonResponse replaceResponse = Dispatch(dispatcher, new CanonicalGraphJsonRequest
            {
                operation = "ReplaceNodePayload",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = "chapter-01",
                    nodeId = "N001",
                    node = new CanonicalGraphJsonNodeInput
                    {
                        payload = new CanonicalGraphJsonNodePayload
                        {
                            labels = { "去A", "去B", "去C" }
                        }
                    }
                }
            });
            CanonicalGraphJsonResponse snapshotResponse = Dispatch(dispatcher, BuildGetGraphRequest("chapter-01"));

            Assert.IsTrue(replaceResponse.success);
            Assert.IsTrue(replaceResponse.applied);
            Assert.That(snapshotResponse.snapshot.nodes.Count, Is.EqualTo(1));
            Assert.That(snapshotResponse.snapshot.nodes[0].branchCount, Is.EqualTo(3));
            Assert.That(snapshotResponse.snapshot.nodes[0].payloadJson, Is.EqualTo("{\"labels\":[\"去A\",\"去B\",\"去C\"]}"));
        }

        [Test]
        public void Dispatch_DisconnectEdge_已存在Edge可用EdgeId移除()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = BuildConnectedLinearDispatcher();

            CanonicalGraphJsonResponse disconnectResponse = Dispatch(dispatcher, new CanonicalGraphJsonRequest
            {
                operation = "DisconnectEdge",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = "chapter-01",
                    edgeId = "N001:Flow->N002:Flow"
                }
            });
            CanonicalGraphJsonResponse snapshotResponse = Dispatch(dispatcher, BuildGetGraphRequest("chapter-01"));

            Assert.IsTrue(disconnectResponse.success);
            Assert.IsTrue(disconnectResponse.applied);
            Assert.That(snapshotResponse.snapshot.edges.Count, Is.EqualTo(0));
        }

        [Test]
        public void Dispatch_DisconnectEdge_目標不存在時SuccessTrueAppliedFalse()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = new CanonicalGraphJsonCommandDispatcher();
            Dispatch(dispatcher, BuildCreateGraphRequest("chapter-01"));

            CanonicalGraphJsonResponse response = Dispatch(dispatcher, new CanonicalGraphJsonRequest
            {
                operation = "DisconnectEdge",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = "chapter-01",
                    fromNodeId = "N001",
                    fromPort = CanonicalPortSemantics.Flow,
                    toNodeId = "N002",
                    toPort = CanonicalPortSemantics.Flow
                }
            });

            Assert.IsTrue(response.success);
            Assert.IsFalse(response.applied);
            Assert.That(response.result.edgeId, Is.EqualTo("N001:Flow->N002:Flow"));
        }

        [Test]
        public void Dispatch_DisconnectEdge_缺少Selector會回傳固定錯誤碼()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = new CanonicalGraphJsonCommandDispatcher();
            Dispatch(dispatcher, BuildCreateGraphRequest("chapter-01"));

            CanonicalGraphJsonResponse response = Dispatch(dispatcher, new CanonicalGraphJsonRequest
            {
                operation = "DisconnectEdge",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = "chapter-01"
                }
            });

            Assert.IsFalse(response.success);
            Assert.That(response.errors[0].code, Is.EqualTo("INVALID_EDGE_SELECTOR"));
        }

        [Test]
        public void Dispatch_RemoveNode_會同步清掉相關Edges()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = BuildConnectedLinearDispatcher();

            CanonicalGraphJsonResponse removeResponse = Dispatch(dispatcher, new CanonicalGraphJsonRequest
            {
                operation = "RemoveNode",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = "chapter-01",
                    nodeId = "N002"
                }
            });
            CanonicalGraphJsonResponse snapshotResponse = Dispatch(dispatcher, BuildGetGraphRequest("chapter-01"));

            Assert.IsTrue(removeResponse.success);
            Assert.IsTrue(removeResponse.applied);
            Assert.That(snapshotResponse.snapshot.nodes.Count, Is.EqualTo(1));
            Assert.That(snapshotResponse.snapshot.edges.Count, Is.EqualTo(0));
        }
        // ===== 變更結束 =====

        [Test]
        public void Dispatch_ValidateGraph_缺少Start_成功但Invalid()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = new CanonicalGraphJsonCommandDispatcher();
            Dispatch(dispatcher, BuildCreateGraphRequest("chapter-01"));
            Dispatch(dispatcher, BuildCreateNodeRequest("chapter-01", "N002", CanonicalNodeKinds.Dialogue, "主句"));

            CanonicalGraphJsonResponse response = Dispatch(dispatcher, new CanonicalGraphJsonRequest
            {
                operation = "ValidateGraph",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = "chapter-01"
                }
            });

            Assert.IsTrue(response.success);
            Assert.IsFalse(response.applied);
            Assert.IsFalse(response.result.isValid);
            Assert.That(response.errors[0].code, Is.EqualTo("MISSING_START_NODE"));
        }

        [Test]
        public void Dispatch_CreateGraph後續查找_前後空白GraphId會自動正規化()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = new CanonicalGraphJsonCommandDispatcher();
            Dispatch(dispatcher, BuildCreateGraphRequest(" chapter-01 "));

            CanonicalGraphJsonResponse response = Dispatch(dispatcher, BuildCreateNodeRequest("chapter-01", "N001", CanonicalNodeKinds.Dialogue, "主句"));

            Assert.IsTrue(response.success);
            Assert.IsTrue(response.applied);
            Assert.That(response.result.graphId, Is.EqualTo("chapter-01"));
        }

        // ===== 變更開始 =====
        // 2026/03/23 Opsidanos (修改原因：為 Batch 11A 補上 projection-specific dispatcher 測試)
        // 預期結果：ValidateProjection / ProjectGraph 可透過 JSON command 穩定處理 `flowchart-json / ink`，並維持 unsupported target 的固定錯誤碼
        [Test]
        public void Dispatch_ValidateProjection_FlowchartJson_成功且AppliedFalse()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = BuildConnectedLinearDispatcher();

            CanonicalGraphJsonResponse response = Dispatch(dispatcher, BuildValidateProjectionRequest("chapter-01", CurrentFlowProjectionService.FlowchartJsonTarget));

            Assert.IsTrue(response.success);
            Assert.IsFalse(response.applied);
            Assert.IsTrue(response.result.isValid);
            Assert.That(response.result.target, Is.EqualTo(CurrentFlowProjectionService.FlowchartJsonTarget));
            Assert.That(response.result.projectionVersion, Is.EqualTo("2.0"));
        }

        [Test]
        public void Dispatch_ProjectGraph_FlowchartJson_會回傳ProjectionJson()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = BuildConnectedLinearDispatcher();

            CanonicalGraphJsonResponse response = Dispatch(dispatcher, BuildProjectGraphRequest("chapter-01", CurrentFlowProjectionService.FlowchartJsonTarget));

            Assert.IsTrue(response.success);
            Assert.IsFalse(response.applied);
            Assert.That(response.result.target, Is.EqualTo(CurrentFlowProjectionService.FlowchartJsonTarget));
            Assert.That(response.result.projectionText, Is.EqualTo(string.Empty));

            ExportGraphDto restored = JsonUtility.FromJson<ExportGraphDto>(response.result.projectionJson);
            Assert.That(restored, Is.Not.Null);
            Assert.That(restored.startNodeId, Is.EqualTo("N001"));
            Assert.That(restored.nodes.Count, Is.EqualTo(2));
        }

        [Test]
        public void Dispatch_ProjectGraph_Ink_會回傳ProjectionText()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = BuildConnectedLinearDispatcher();

            CanonicalGraphJsonResponse response = Dispatch(dispatcher, BuildProjectGraphRequest("chapter-01", CurrentFlowProjectionService.InkTarget));

            Assert.IsTrue(response.success);
            Assert.IsFalse(response.applied);
            Assert.That(response.result.target, Is.EqualTo(CurrentFlowProjectionService.InkTarget));
            Assert.That(response.result.projectionJson, Is.EqualTo(string.Empty));
            StringAssert.Contains("=== knot_N001 ===", response.result.projectionText);
            StringAssert.Contains("=== knot_N002 ===", response.result.projectionText);
        }

        [Test]
        public void Dispatch_ProjectGraph_不支援Target_會回固定錯誤碼()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = BuildConnectedLinearDispatcher();

            CanonicalGraphJsonResponse response = Dispatch(dispatcher, BuildProjectGraphRequest("chapter-01", "story-json"));

            Assert.IsFalse(response.success);
            Assert.IsFalse(response.applied);
            Assert.That(response.errors[0].code, Is.EqualTo("PROJECTION_UNSUPPORTED"));
        }
        // ===== 變更結束 =====

        private static CanonicalGraphJsonResponse Dispatch(CanonicalGraphJsonCommandDispatcher dispatcher, CanonicalGraphJsonRequest request)
        {
            bool success = dispatcher.TryDispatch(JsonUtility.ToJson(request, true), out string responseJson);
            CanonicalGraphJsonResponse response = JsonUtility.FromJson<CanonicalGraphJsonResponse>(responseJson);
            Assert.That(response, Is.Not.Null);
            Assert.That(success, Is.EqualTo(response.success));
            return response;
        }

        private static CanonicalGraphJsonRequest BuildCreateGraphRequest(string graphId)
        {
            return new CanonicalGraphJsonRequest
            {
                operation = "CreateGraph",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = graphId
                }
            };
        }

        private static CanonicalGraphJsonRequest BuildCreateNodeRequest(string graphId, string nodeId, string nodeType, string content = "")
        {
            return new CanonicalGraphJsonRequest
            {
                operation = "CreateNode",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = graphId,
                    node = new CanonicalGraphJsonNodeInput
                    {
                        nodeId = nodeId,
                        nodeType = nodeType,
                        payload = new CanonicalGraphJsonNodePayload
                        {
                            content = content
                        }
                    }
                }
            };
        }

        private static CanonicalGraphJsonRequest BuildConnectPortsRequest(string graphId, string fromNodeId, string fromPort, string toNodeId, string toPort)
        {
            return new CanonicalGraphJsonRequest
            {
                operation = "ConnectPorts",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = graphId,
                    fromNodeId = fromNodeId,
                    fromPort = fromPort,
                    toNodeId = toNodeId,
                    toPort = toPort
                }
            };
        }

        // ===== 變更開始 =====
        // 2026/03/22 Opsidanos (修改原因：為 Batch 10 的 dispatcher 測試補上共用 helper)
        // 預期結果：GetGraph 與 connected graph 的測試組裝邏輯集中，避免每個測試各自手拼 request 造成噪音
        private static CanonicalGraphJsonRequest BuildGetGraphRequest(string graphId)
        {
            return new CanonicalGraphJsonRequest
            {
                operation = "GetGraph",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = graphId
                }
            };
        }

        // ===== 變更開始 =====
        // 2026/03/23 Opsidanos (修改原因：為 Batch 11A 的 dispatcher 測試補上 projection request helper)
        // 預期結果：ValidateProjection / ProjectGraph 測試可共用固定 request 組裝邏輯，避免每支測試各自手拼 target 欄位
        private static CanonicalGraphJsonRequest BuildValidateProjectionRequest(string graphId, string target)
        {
            return new CanonicalGraphJsonRequest
            {
                operation = "ValidateProjection",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = graphId,
                    target = target
                }
            };
        }

        private static CanonicalGraphJsonRequest BuildProjectGraphRequest(string graphId, string target)
        {
            return new CanonicalGraphJsonRequest
            {
                operation = "ProjectGraph",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = graphId,
                    target = target
                }
            };
        }
        // ===== 變更結束 =====

        private static CanonicalGraphJsonCommandDispatcher BuildConnectedLinearDispatcher()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = new CanonicalGraphJsonCommandDispatcher();
            Dispatch(dispatcher, BuildCreateGraphRequest("chapter-01"));
            Dispatch(dispatcher, BuildCreateNodeRequest("chapter-01", "N001", CanonicalNodeKinds.Start));
            Dispatch(dispatcher, BuildCreateNodeRequest("chapter-01", "N002", CanonicalNodeKinds.Dialogue, "主句"));
            Dispatch(dispatcher, BuildConnectPortsRequest("chapter-01", "N001", CanonicalPortSemantics.Flow, "N002", CanonicalPortSemantics.Flow));
            return dispatcher;
        }
        // ===== 變更結束 =====
    }
}
// ===== 變更結束 =====
