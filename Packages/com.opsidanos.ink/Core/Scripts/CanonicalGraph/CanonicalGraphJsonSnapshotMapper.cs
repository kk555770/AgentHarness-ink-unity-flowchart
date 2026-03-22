// ===== 變更開始 =====
// 2026/03/22 Opsidanos (修改原因：為 Batch 9 補上 canonical graph document -> JSON snapshot 的共用 mapper)
// 預期結果：GetGraph 會回傳穩定的 deep-cloned snapshot，不會把 graph store 裡的實體直接暴露給外部
using System.Collections.Generic;

namespace OpsidanosInk.CanonicalGraph
{
    public static class CanonicalGraphJsonSnapshotMapper
    {
        public static CanonicalGraphJsonGraphSnapshot ToSnapshot(CanonicalGraphDocument graph)
        {
            if (graph == null)
            {
                return null;
            }

            var snapshot = new CanonicalGraphJsonGraphSnapshot
            {
                graphId = graph.graphId ?? string.Empty,
                version = graph.version ?? string.Empty,
                metadataJson = graph.metadataJson ?? string.Empty,
                nodes = new List<CanonicalGraphNodeRecord>(graph.nodes.Count),
                edges = new List<CanonicalGraphEdgeRecord>(graph.edges.Count)
            };

            for (int i = 0; i < graph.nodes.Count; i++)
            {
                CanonicalGraphNodeRecord node = graph.nodes[i];
                if (node == null)
                {
                    continue;
                }

                snapshot.nodes.Add(new CanonicalGraphNodeRecord
                {
                    nodeId = node.nodeId ?? string.Empty,
                    nodeType = node.nodeType ?? string.Empty,
                    payloadJson = node.payloadJson ?? string.Empty,
                    dialogueActionInputCount = node.dialogueActionInputCount,
                    branchCount = node.branchCount,
                    branchModeToken = node.branchModeToken ?? string.Empty
                });
            }

            for (int i = 0; i < graph.edges.Count; i++)
            {
                CanonicalGraphEdgeRecord edge = graph.edges[i];
                if (edge == null)
                {
                    continue;
                }

                snapshot.edges.Add(new CanonicalGraphEdgeRecord
                {
                    edgeId = edge.edgeId ?? string.Empty,
                    fromNodeId = edge.fromNodeId ?? string.Empty,
                    fromPort = edge.fromPort ?? string.Empty,
                    toNodeId = edge.toNodeId ?? string.Empty,
                    toPort = edge.toPort ?? string.Empty
                });
            }

            return snapshot;
        }
    }
}
// ===== 變更結束 =====
