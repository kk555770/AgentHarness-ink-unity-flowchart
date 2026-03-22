// ===== 變更開始 =====
// 2026/03/22 Opsidanos (修改原因：為 Batch 9 的 snapshot mapper 補上 deep-clone 與欄位保留測試)
// 預期結果：CanonicalGraphDocument -> JSON snapshot 時，不會丟掉 metadata / payload / edge 資訊，也不會把原 graph 直接外漏
using NUnit.Framework;
using OpsidanosInk.CanonicalGraph;

namespace OpsidanosInk.Tests.EditMode
{
    public sealed class CanonicalGraphJsonSnapshotMapperTests
    {
        [Test]
        public void ToSnapshot_會保留GraphNodeEdge與Metadata()
        {
            CanonicalGraphDocument graph = new CanonicalGraphDocument
            {
                graphId = "chapter-01",
                version = "canonical-1",
                metadataJson = "{\"title\":\"第一章\"}"
            };
            graph.nodes.Add(new CanonicalGraphNodeRecord
            {
                nodeId = "N001",
                nodeType = CanonicalNodeKinds.Choice,
                payloadJson = "{\"labels\":[\"去 A\",\"去 B\"]}",
                branchCount = 2,
                branchModeToken = "+"
            });
            graph.edges.Add(new CanonicalGraphEdgeRecord
            {
                edgeId = "N001:flow-out->N002:flow-in",
                fromNodeId = "N001",
                fromPort = CanonicalPortSemantics.Flow,
                toNodeId = "N002",
                toPort = CanonicalPortSemantics.Flow
            });

            CanonicalGraphJsonGraphSnapshot snapshot = CanonicalGraphJsonSnapshotMapper.ToSnapshot(graph);

            Assert.That(snapshot, Is.Not.Null);
            Assert.That(snapshot.graphId, Is.EqualTo("chapter-01"));
            Assert.That(snapshot.version, Is.EqualTo("canonical-1"));
            Assert.That(snapshot.metadataJson, Is.EqualTo("{\"title\":\"第一章\"}"));
            Assert.That(snapshot.nodes.Count, Is.EqualTo(1));
            Assert.That(snapshot.nodes[0].payloadJson, Is.EqualTo("{\"labels\":[\"去 A\",\"去 B\"]}"));
            Assert.That(snapshot.edges.Count, Is.EqualTo(1));
            Assert.That(snapshot.edges[0].edgeId, Is.EqualTo("N001:flow-out->N002:flow-in"));
        }

        [Test]
        public void ToSnapshot_會做DeepClone而不是共享原始資料盒()
        {
            CanonicalGraphDocument graph = new CanonicalGraphDocument
            {
                graphId = "chapter-01",
                version = "canonical-1"
            };
            graph.nodes.Add(new CanonicalGraphNodeRecord
            {
                nodeId = "N001",
                nodeType = CanonicalNodeKinds.Dialogue,
                payloadJson = "{\"content\":\"原句\"}"
            });

            CanonicalGraphJsonGraphSnapshot snapshot = CanonicalGraphJsonSnapshotMapper.ToSnapshot(graph);
            graph.nodes[0].payloadJson = "{\"content\":\"改過了\"}";

            Assert.That(snapshot.nodes[0].payloadJson, Is.EqualTo("{\"content\":\"原句\"}"));
        }
    }
}
// ===== 變更結束 =====
