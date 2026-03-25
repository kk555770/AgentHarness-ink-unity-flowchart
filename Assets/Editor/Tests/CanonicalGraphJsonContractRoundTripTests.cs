// ===== 變更開始 =====
// 2026/03/22 Opsidanos (修改原因：擴充 Batch 8/9 的 JSON contract 測試，補上 GetGraph response 的 round-trip 護欄)
// 預期結果：request / response 形狀穩定，且 contractVersion / operation / choice payload / snapshot 的最小語意不漂移
using NUnit.Framework;
using OpsidanosInk.CanonicalGraph;
using UnityEngine;

namespace OpsidanosInk.Tests.EditMode
{
    public sealed class CanonicalGraphJsonContractRoundTripTests
    {
        [Test]
        public void RequestEnvelope_CreateNode_RoundTrip會保留Payload與ContractVersion()
        {
            CanonicalGraphJsonRequest request = new CanonicalGraphJsonRequest
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
                            labels = { "去 A", "去 B" }
                        }
                    }
                }
            };

            string json = JsonUtility.ToJson(request, true);
            CanonicalGraphJsonRequest restored = JsonUtility.FromJson<CanonicalGraphJsonRequest>(json);

            Assert.That(restored.contractVersion, Is.EqualTo(CanonicalGraphJsonRequest.PlainJsonContractVersion));
            Assert.That(restored.operation, Is.EqualTo("CreateNode"));
            Assert.That(restored.input.graphId, Is.EqualTo("chapter-01"));
            Assert.That(restored.input.node.nodeType, Is.EqualTo(CanonicalNodeKinds.Choice));
            Assert.That(restored.input.node.payload.labels.Count, Is.EqualTo(2));
            Assert.That(restored.input.node.payload.labels[0], Is.EqualTo("去 A"));
            Assert.That(restored.input.node.payload.labels[1], Is.EqualTo("去 B"));
        }

        // ===== 變更開始 =====
        // 2026/03/22 Opsidanos (修改原因：為 Batch 10 補上 mutation request 欄位的 contract round-trip 護欄)
        // 預期結果：`nodeId / edgeId` 等 mutation selector 序列化後不會遺失，後續 host bridge 可依賴固定 JSON shape
        [Test]
        public void RequestEnvelope_DisconnectEdge_RoundTrip會保留EdgeSelector()
        {
            CanonicalGraphJsonRequest request = new CanonicalGraphJsonRequest
            {
                operation = "DisconnectEdge",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = "chapter-01",
                    edgeId = "N001:Flow->N002:Flow",
                    fromNodeId = "N001",
                    fromPort = CanonicalPortSemantics.Flow,
                    toNodeId = "N002",
                    toPort = CanonicalPortSemantics.Flow
                }
            };

            string json = JsonUtility.ToJson(request, true);
            CanonicalGraphJsonRequest restored = JsonUtility.FromJson<CanonicalGraphJsonRequest>(json);

            Assert.That(restored.input.graphId, Is.EqualTo("chapter-01"));
            Assert.That(restored.input.edgeId, Is.EqualTo("N001:Flow->N002:Flow"));
            Assert.That(restored.input.fromNodeId, Is.EqualTo("N001"));
            Assert.That(restored.input.toNodeId, Is.EqualTo("N002"));
        }
        // ===== 變更結束 =====

        // ===== 變更開始 =====
        // 2026/03/23 Opsidanos (修改原因：為 Batch 11A 補上 projection request 的 contract round-trip 護欄)
        // 預期結果：`target / projectionVersion` 序列化後不會遺失，後續 host bridge 可依賴固定 JSON shape
        [Test]
        public void RequestEnvelope_ProjectGraph_RoundTrip會保留Target與ProjectionVersion()
        {
            CanonicalGraphJsonRequest request = new CanonicalGraphJsonRequest
            {
                operation = "ProjectGraph",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = "chapter-01",
                    target = CurrentFlowProjectionService.FlowchartJsonTarget,
                    projectionVersion = "2.0"
                }
            };

            string json = JsonUtility.ToJson(request, true);
            CanonicalGraphJsonRequest restored = JsonUtility.FromJson<CanonicalGraphJsonRequest>(json);

            Assert.That(restored.input.graphId, Is.EqualTo("chapter-01"));
            Assert.That(restored.input.target, Is.EqualTo(CurrentFlowProjectionService.FlowchartJsonTarget));
            Assert.That(restored.input.projectionVersion, Is.EqualTo("2.0"));
        }

        [Test]
        public void RequestEnvelope_ImportProjection_RoundTrip會保留ProjectionPayload()
        {
            CanonicalGraphJsonRequest request = new CanonicalGraphJsonRequest
            {
                operation = "ImportProjection",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = "chapter-02",
                    target = CurrentFlowProjectionService.FlowchartJsonTarget,
                    projectionVersion = "2.0",
                    projectionJson = "{\"graphName\":\"chapter-02\"}"
                }
            };

            string json = JsonUtility.ToJson(request, true);
            CanonicalGraphJsonRequest restored = JsonUtility.FromJson<CanonicalGraphJsonRequest>(json);

            Assert.That(restored.input.graphId, Is.EqualTo("chapter-02"));
            Assert.That(restored.input.target, Is.EqualTo(CurrentFlowProjectionService.FlowchartJsonTarget));
            Assert.That(restored.input.projectionVersion, Is.EqualTo("2.0"));
            Assert.That(restored.input.projectionJson, Is.EqualTo("{\"graphName\":\"chapter-02\"}"));
        }
        // ===== 變更結束 =====

        [Test]
        public void Dispatch_錯誤ContractVersion_會回傳穩定錯誤碼()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = new CanonicalGraphJsonCommandDispatcher();
            CanonicalGraphJsonRequest request = new CanonicalGraphJsonRequest
            {
                contractVersion = "plain-json-x",
                operation = "CreateGraph",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = "chapter-01"
                }
            };

            CanonicalGraphJsonResponse response = Dispatch(dispatcher, request);

            Assert.IsFalse(response.success);
            Assert.IsFalse(response.applied);
            Assert.That(response.errors[0].code, Is.EqualTo("UNSUPPORTED_CONTRACT_VERSION"));
        }

        [Test]
        public void Dispatch_未知Operation_會回傳穩定錯誤碼()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = new CanonicalGraphJsonCommandDispatcher();
            CanonicalGraphJsonRequest request = new CanonicalGraphJsonRequest
            {
                operation = "ProjectGraphX",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = "chapter-01"
                }
            };

            CanonicalGraphJsonResponse response = Dispatch(dispatcher, request);

            Assert.IsFalse(response.success);
            Assert.That(response.errors[0].code, Is.EqualTo("UNSUPPORTED_OPERATION"));
        }

        [Test]
        public void Dispatch_CreateNode_ChoicePayload未指定BranchCount_會自動推導()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = new CanonicalGraphJsonCommandDispatcher();
            Dispatch(dispatcher, new CanonicalGraphJsonRequest
            {
                operation = "CreateGraph",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = "chapter-01"
                }
            });

            CanonicalGraphJsonResponse response = Dispatch(dispatcher, new CanonicalGraphJsonRequest
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
                            labels = { "去 A", "去 B" }
                        }
                    }
                }
            });

            Assert.IsTrue(response.success);
            Assert.IsTrue(response.applied);
            Assert.That(response.result.nodeId, Is.EqualTo("N001"));
        }

        [Test]
        public void ResponseEnvelope_GetGraphSnapshot_RoundTrip會保留Metadata與節點資料()
        {
            CanonicalGraphJsonResponse response = new CanonicalGraphJsonResponse
            {
                success = true,
                applied = false,
                snapshot = new CanonicalGraphJsonGraphSnapshot
                {
                    graphId = "chapter-01",
                    version = "canonical-1",
                    metadataJson = "{\"title\":\"第一章\"}",
                    nodes =
                    {
                        new CanonicalGraphNodeRecord
                        {
                            nodeId = "N001",
                            nodeType = CanonicalNodeKinds.Dialogue,
                            payloadJson = "{\"content\":\"你好。\"}"
                        }
                    },
                    edges =
                    {
                        new CanonicalGraphEdgeRecord
                        {
                            edgeId = "N001:flow-out->N002:flow-in",
                            fromNodeId = "N001",
                            fromPort = CanonicalPortSemantics.Flow,
                            toNodeId = "N002",
                            toPort = CanonicalPortSemantics.Flow
                        }
                    }
                }
            };
            response.result.graphId = "chapter-01";
            response.result.version = "canonical-1";

            string json = JsonUtility.ToJson(response, true);
            CanonicalGraphJsonResponse restored = JsonUtility.FromJson<CanonicalGraphJsonResponse>(json);

            Assert.That(restored.snapshot, Is.Not.Null);
            Assert.That(restored.snapshot.metadataJson, Is.EqualTo("{\"title\":\"第一章\"}"));
            Assert.That(restored.snapshot.nodes.Count, Is.EqualTo(1));
            Assert.That(restored.snapshot.nodes[0].payloadJson, Is.EqualTo("{\"content\":\"你好。\"}"));
            Assert.That(restored.snapshot.edges.Count, Is.EqualTo(1));
            Assert.That(restored.snapshot.edges[0].edgeId, Is.EqualTo("N001:flow-out->N002:flow-in"));
        }

        // ===== 變更開始 =====
        // 2026/03/23 Opsidanos (修改原因：為 Batch 11A 補上 projection response 的 round-trip 護欄)
        // 預期結果：`target / projectionVersion / projectionText / projectionJson` 會在 JSON contract 序列化後完整保留
        [Test]
        public void ResponseEnvelope_ProjectGraph_RoundTrip會保留ProjectionResult()
        {
            CanonicalGraphJsonResponse response = new CanonicalGraphJsonResponse
            {
                success = true,
                applied = false
            };
            response.result.graphId = "chapter-01";
            response.result.version = "canonical-1";
            response.result.target = CurrentFlowProjectionService.FlowchartJsonTarget;
            response.result.projectionVersion = "2.0";
            response.result.projectionJson = "{\"graphName\":\"chapter-01\"}";
            response.result.projectionText = string.Empty;

            string json = JsonUtility.ToJson(response, true);
            CanonicalGraphJsonResponse restored = JsonUtility.FromJson<CanonicalGraphJsonResponse>(json);

            Assert.That(restored.result.graphId, Is.EqualTo("chapter-01"));
            Assert.That(restored.result.target, Is.EqualTo(CurrentFlowProjectionService.FlowchartJsonTarget));
            Assert.That(restored.result.projectionVersion, Is.EqualTo("2.0"));
            Assert.That(restored.result.projectionJson, Is.EqualTo("{\"graphName\":\"chapter-01\"}"));
            Assert.That(restored.result.projectionText, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ResponseEnvelope_ImportProjection_RoundTrip會保留Snapshot與Target()
        {
            CanonicalGraphJsonResponse response = new CanonicalGraphJsonResponse
            {
                success = true,
                applied = true,
                snapshot = new CanonicalGraphJsonGraphSnapshot
                {
                    graphId = "imported-01",
                    version = "canonical-1",
                    nodes =
                    {
                        new CanonicalGraphNodeRecord
                        {
                            nodeId = "N001",
                            nodeType = CanonicalNodeKinds.Start
                        }
                    }
                }
            };
            response.result.graphId = "imported-01";
            response.result.version = "canonical-1";
            response.result.target = CurrentFlowProjectionService.FlowchartJsonTarget;
            response.result.projectionVersion = "2.0";
            response.result.isValid = true;

            string json = JsonUtility.ToJson(response, true);
            CanonicalGraphJsonResponse restored = JsonUtility.FromJson<CanonicalGraphJsonResponse>(json);

            Assert.IsTrue(restored.applied);
            Assert.That(restored.result.graphId, Is.EqualTo("imported-01"));
            Assert.That(restored.result.target, Is.EqualTo(CurrentFlowProjectionService.FlowchartJsonTarget));
            Assert.That(restored.result.projectionVersion, Is.EqualTo("2.0"));
            Assert.That(restored.snapshot, Is.Not.Null);
            Assert.That(restored.snapshot.nodes.Count, Is.EqualTo(1));
            Assert.That(restored.snapshot.nodes[0].nodeId, Is.EqualTo("N001"));
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
    }
}
// ===== 變更結束 =====
