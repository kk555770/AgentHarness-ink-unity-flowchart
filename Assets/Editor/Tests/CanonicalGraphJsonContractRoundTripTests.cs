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
                operation = "ProjectGraph",
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
