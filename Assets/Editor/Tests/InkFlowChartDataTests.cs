// ===== 變更開始 =====
// 2026/02/06 Opsidanos (修改原因：補上 Flow Chart 最小 Editor 測試，避免資料模型被改壞)
// 預期結果：Flow Chart 節點資料可被 ScriptableObject 持有並可經 JsonUtility 往返還原
using NUnit.Framework;
using OpsidanosInk.Editor;
using UnityEngine;

namespace OpsidanosInk.Tests
{
    public sealed class InkFlowChartDataTests
    {
        [Test]
        public void InkFlowChartData_預設節點清單不可為Null()
        {
            InkFlowChartData data = ScriptableObject.CreateInstance<InkFlowChartData>();
            try
            {
                Assert.NotNull(data.nodes, "FlowChart 節點清單不可為 null。");
            }
            finally
            {
                Object.DestroyImmediate(data);
            }
        }

        [Test]
        public void InkFlowChartData_節點資料可RoundTrip()
        {
            InkFlowChartData data = ScriptableObject.CreateInstance<InkFlowChartData>();
            InkFlowChartData loaded = ScriptableObject.CreateInstance<InkFlowChartData>();

            try
            {
                data.nodes.Add(new InkFlowChartNode
                {
                    id = "node_a",
                    title = "起點",
                    nodeType = InkFlowChartNodeType.Ink,
                    rect = new Rect(100f, 120f, 280f, 200f),
                    body = "第一句",
                    nextNodeIds = { "node_b" }
                });

                data.nodes.Add(new InkFlowChartNode
                {
                    id = "node_b",
                    title = "註解",
                    nodeType = InkFlowChartNodeType.Comment,
                    rect = new Rect(500f, 120f, 280f, 200f),
                    body = "備註",
                    nextNodeIds = { }
                });

                string json = JsonUtility.ToJson(data);
                JsonUtility.FromJsonOverwrite(json, loaded);

                Assert.NotNull(loaded.nodes, "RoundTrip 後節點清單不可為 null。");
                Assert.AreEqual(2, loaded.nodes.Count, "RoundTrip 後節點數量不一致。");
                Assert.AreEqual("node_a", loaded.nodes[0].id);
                Assert.AreEqual(InkFlowChartNodeType.Ink, loaded.nodes[0].nodeType);
                Assert.AreEqual(1, loaded.nodes[0].nextNodeIds.Count);
                Assert.AreEqual("node_b", loaded.nodes[0].nextNodeIds[0]);
                Assert.AreEqual("node_b", loaded.nodes[1].id);
                Assert.AreEqual(InkFlowChartNodeType.Comment, loaded.nodes[1].nodeType);
            }
            finally
            {
                Object.DestroyImmediate(data);
                Object.DestroyImmediate(loaded);
            }
        }
    }
}
// ===== 變更結束 =====
