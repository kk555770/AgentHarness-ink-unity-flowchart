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
    }
}
// ===== 變更結束 =====
