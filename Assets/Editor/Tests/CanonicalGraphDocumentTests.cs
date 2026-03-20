// ===== 變更開始 =====
// 2026/03/19 Opsidanos (修改原因：為 Batch 4 bridge kickoff 補上 graph document 最小 EditMode 測試)
// 預期結果：canonical graph document 的預設集合與欄位骨架有固定測試護欄，不會在後續 bridge 重構中悄悄走樣
using NUnit.Framework;
using OpsidanosInk.CanonicalGraph;

namespace OpsidanosInk.Tests.EditMode
{
    public sealed class CanonicalGraphDocumentTests
    {
        [Test]
        public void CanonicalGraphDocument_Defaults_會建立可用的空集合()
        {
            CanonicalGraphDocument document = new CanonicalGraphDocument();

            Assert.IsNotNull(document.nodes);
            Assert.IsNotNull(document.edges);
            Assert.AreEqual(0, document.nodes.Count);
            Assert.AreEqual(0, document.edges.Count);
            Assert.AreEqual(string.Empty, document.graphId);
            Assert.AreEqual(string.Empty, document.version);
            Assert.AreEqual(string.Empty, document.metadataJson);
        }

        [Test]
        public void CanonicalGraphNodeRecord_Defaults_欄位都是可預期空值()
        {
            CanonicalGraphNodeRecord node = new CanonicalGraphNodeRecord();

            Assert.AreEqual(string.Empty, node.nodeId);
            Assert.AreEqual(string.Empty, node.nodeType);
            Assert.AreEqual(string.Empty, node.payloadJson);
            Assert.AreEqual(0, node.dialogueActionInputCount);
            Assert.AreEqual(0, node.branchCount);
            Assert.AreEqual(string.Empty, node.branchModeToken);
        }

        [Test]
        public void CanonicalGraphEdgeRecord_Defaults_欄位都是可預期空值()
        {
            CanonicalGraphEdgeRecord edge = new CanonicalGraphEdgeRecord();

            Assert.AreEqual(string.Empty, edge.edgeId);
            Assert.AreEqual(string.Empty, edge.fromNodeId);
            Assert.AreEqual(string.Empty, edge.fromPort);
            Assert.AreEqual(string.Empty, edge.toNodeId);
            Assert.AreEqual(string.Empty, edge.toPort);
        }
    }
}
// ===== 變更結束 =====
