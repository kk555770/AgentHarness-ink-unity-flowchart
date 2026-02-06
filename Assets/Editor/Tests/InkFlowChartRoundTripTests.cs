// ===== 變更開始 =====
// 2026/02/06 Opsidanos (修改原因：補上 Flow Chart 匯出匯入往返測試，驗證可還原節點與連線；覆蓋標題連線與控制字元清理)
// 預期結果：`.ink + sidecar` 往返後，圖表資料仍與原始輸入等效，匯出文本不含控制碼
using System;
using System.IO;
using NUnit.Framework;
using OpsidanosInk.Editor;
using UnityEngine;

namespace OpsidanosInk.Tests
{
    public sealed class InkFlowChartRoundTripTests
    {
        [Test]
        public void FlowChart_匯出再匯入_可還原節點與連線()
        {
            InkFlowChartData sourceData = ScriptableObject.CreateInstance<InkFlowChartData>();
            InkFlowChartData loadedData = ScriptableObject.CreateInstance<InkFlowChartData>();
            string tempDirectoryPath = Path.Combine(Path.GetTempPath(), $"OpsidanosInk_FlowChart_{Guid.NewGuid():N}");
            string inkPath = Path.Combine(tempDirectoryPath, "story.ink");

            try
            {
                sourceData.nodes.Add(new InkFlowChartNode
                {
                    id = "start_node",
                    title = "開始",
                    nodeType = InkFlowChartNodeType.Ink,
                    rect = new Rect(120f, 140f, 280f, 200f),
                    body = "你好，這是開場。\u0003",
                    nextNodeIds = { "表情標籤", "comment_node" }
                });

                sourceData.nodes.Add(new InkFlowChartNode
                {
                    id = "tag_node",
                    title = "表情標籤",
                    nodeType = InkFlowChartNodeType.Tag,
                    rect = new Rect(520f, 140f, 280f, 200f),
                    body = "char=Alice mood=happy",
                    nextNodeIds = { "comment_node" }
                });

                sourceData.nodes.Add(new InkFlowChartNode
                {
                    id = "comment_node",
                    title = "設計備註",
                    nodeType = InkFlowChartNodeType.Comment,
                    rect = new Rect(920f, 140f, 280f, 200f),
                    body = "這裡預計接戰鬥分支",
                    nextNodeIds = { }
                });

                bool exportSuccess = InkFlowChartExporter.ExportToInkAndSidecar(sourceData, inkPath, out string sidecarPath, out string exportError);
                Assert.IsTrue(exportSuccess, exportError);
                Assert.IsTrue(File.Exists(inkPath), "匯出後找不到 Ink 檔案。");
                Assert.IsTrue(File.Exists(sidecarPath), "匯出後找不到 sidecar 檔案。");
                string exportedInk = File.ReadAllText(inkPath);
                Assert.IsFalse(exportedInk.Contains("\u0003"), "匯出後 Ink 不應包含控制字元。");
                Assert.IsTrue(exportedInk.Contains("* -> k_tag_node"), "匯出後應可由標題連線解析到 tag_node。");
                Assert.IsTrue(exportedInk.Contains("* -> k_comment_node"), "匯出後應保留 comment_node 連線。");
                string exportedSidecar = File.ReadAllText(sidecarPath);
                Assert.IsFalse(exportedSidecar.Contains("\\u0003"), "匯出後 sidecar 不應包含控制字元轉義。");

                bool importSuccess = InkFlowChartImporter.ImportFromInkAndSidecar(inkPath, sidecarPath, loadedData, out string importError);
                Assert.IsTrue(importSuccess, importError);

                Assert.NotNull(loadedData.nodes, "匯入後節點清單不可為 null。");
                Assert.AreEqual(3, loadedData.nodes.Count, "匯入後節點數量不一致。");

                Assert.AreEqual("start_node", loadedData.nodes[0].id);
                Assert.AreEqual(InkFlowChartNodeType.Ink, loadedData.nodes[0].nodeType);
                Assert.AreEqual(2, loadedData.nodes[0].nextNodeIds.Count);
                Assert.AreEqual("tag_node", loadedData.nodes[0].nextNodeIds[0]);
                Assert.AreEqual("comment_node", loadedData.nodes[0].nextNodeIds[1]);

                Assert.AreEqual("tag_node", loadedData.nodes[1].id);
                Assert.AreEqual(InkFlowChartNodeType.Tag, loadedData.nodes[1].nodeType);
                Assert.AreEqual("char=Alice mood=happy", loadedData.nodes[1].body);

                Assert.AreEqual("comment_node", loadedData.nodes[2].id);
                Assert.AreEqual(InkFlowChartNodeType.Comment, loadedData.nodes[2].nodeType);
                Assert.AreEqual(0, loadedData.nodes[2].nextNodeIds.Count);
            }
            finally
            {
                if (Directory.Exists(tempDirectoryPath))
                {
                    Directory.Delete(tempDirectoryPath, true);
                }

                UnityEngine.Object.DestroyImmediate(sourceData);
                UnityEngine.Object.DestroyImmediate(loadedData);
            }
        }
    }
}
// ===== 變更結束 =====
