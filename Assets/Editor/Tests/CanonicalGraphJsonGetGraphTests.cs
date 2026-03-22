// ===== 變更開始 =====
// 2026/03/22 Opsidanos (修改原因：為 Batch 9 的 GetGraph 補上最小 dispatcher 行為測試)
// 預期結果：GetGraph 可讀回 canonical snapshot，且 graphId 的 trim / not-found 邊界都固定不漂移
using NUnit.Framework;
using OpsidanosInk.CanonicalGraph;
using UnityEngine;

namespace OpsidanosInk.Tests.EditMode
{
    public sealed class CanonicalGraphJsonGetGraphTests
    {
        [Test]
        public void Dispatch_GetGraph_成功時會回傳Snapshot與AppliedFalse()
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
                        nodeType = CanonicalNodeKinds.Dialogue,
                        payload = new CanonicalGraphJsonNodePayload
                        {
                            content = "主句 -> 下一句"
                        }
                    }
                }
            });

            CanonicalGraphJsonResponse response = Dispatch(dispatcher, new CanonicalGraphJsonRequest
            {
                operation = "GetGraph",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = "chapter-01"
                }
            });

            Assert.IsTrue(response.success);
            Assert.IsFalse(response.applied);
            Assert.That(response.result.graphId, Is.EqualTo("chapter-01"));
            Assert.That(response.snapshot, Is.Not.Null);
            Assert.That(response.snapshot.graphId, Is.EqualTo("chapter-01"));
            Assert.That(response.snapshot.nodes.Count, Is.EqualTo(1));
            Assert.That(response.snapshot.nodes[0].payloadJson, Does.Contain("主句 -> 下一句"));
        }

        [Test]
        public void Dispatch_GetGraph_前後空白GraphId會自動正規化()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = new CanonicalGraphJsonCommandDispatcher();
            Dispatch(dispatcher, BuildCreateGraphRequest(" chapter-01 "));

            CanonicalGraphJsonResponse response = Dispatch(dispatcher, new CanonicalGraphJsonRequest
            {
                operation = "GetGraph",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = "chapter-01"
                }
            });

            Assert.IsTrue(response.success);
            Assert.That(response.snapshot, Is.Not.Null);
            Assert.That(response.snapshot.graphId, Is.EqualTo("chapter-01"));
        }

        [Test]
        public void Dispatch_GetGraph_不存在Graph_會回傳固定錯誤且不帶Snapshot()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = new CanonicalGraphJsonCommandDispatcher();
            CanonicalGraphJsonRequest request = new CanonicalGraphJsonRequest
            {
                operation = "GetGraph",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = "chapter-404"
                }
            };

            bool success = dispatcher.TryDispatch(JsonUtility.ToJson(request, true), out string responseJson);
            CanonicalGraphJsonResponse response = JsonUtility.FromJson<CanonicalGraphJsonResponse>(responseJson);

            Assert.IsFalse(response.success);
            Assert.IsFalse(success);
            Assert.IsFalse(response.applied);
            Assert.That(response.errors[0].code, Is.EqualTo("GRAPH_NOT_FOUND"));
            Assert.That(response.snapshot, Is.Not.Null);
            Assert.That(response.snapshot.graphId, Is.Empty);
            Assert.That(response.snapshot.nodes.Count, Is.EqualTo(0));
            Assert.That(response.snapshot.edges.Count, Is.EqualTo(0));
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
    }
}
// ===== 變更結束 =====
