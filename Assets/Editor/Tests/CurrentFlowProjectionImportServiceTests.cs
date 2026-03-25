// ===== 變更開始 =====
// 2026/03/25 Opsidanos (修改原因：為 ImportProjection 補 shared service 測試，避免 control plane 以外沒有核心匯入護欄)
// 預期結果：`ImportProjection(flowchart-json)` 的成功/失敗語意會先被純資料測試鎖住，dispatcher 只負責外層路由
using System.Collections.Generic;
using NUnit.Framework;
using OpsidanosInk.CanonicalGraph;
using UnityEngine;

namespace OpsidanosInk.Tests.EditMode
{
    public sealed class CurrentFlowProjectionImportServiceTests
    {
        [Test]
        public void TryImportProjection_FlowchartJson_會轉成CanonicalGraph()
        {
            bool success = CurrentFlowProjectionImportService.TryImportProjection(
                CurrentFlowProjectionService.FlowchartJsonTarget,
                string.Empty,
                string.Empty,
                BuildLinearFlowchartJson(),
                out string resolvedProjectionVersion,
                out CanonicalGraphDocument graph,
                out string errorCode,
                out string errorMessage);

            Assert.IsTrue(success, errorMessage);
            Assert.That(errorCode, Is.Empty);
            Assert.That(errorMessage, Is.Empty);
            Assert.That(resolvedProjectionVersion, Is.EqualTo("2.0"));
            Assert.That(graph, Is.Not.Null);
            Assert.That(graph.graphId, Is.EqualTo("ImportedChapter"));
            Assert.That(graph.version, Is.EqualTo("canonical-1"));
            Assert.That(graph.nodes.Count, Is.EqualTo(2));
            Assert.That(graph.edges.Count, Is.EqualTo(1));
        }

        [Test]
        public void TryImportProjection_Ink_目前會回固定錯誤碼()
        {
            bool success = CurrentFlowProjectionImportService.TryImportProjection(
                CurrentFlowProjectionService.InkTarget,
                string.Empty,
                "=== knot_start ===\n-> END\n",
                string.Empty,
                out _,
                out CanonicalGraphDocument graph,
                out string errorCode,
                out string errorMessage);

            Assert.IsFalse(success);
            Assert.That(graph, Is.Null);
            Assert.That(errorCode, Is.EqualTo("PROJECTION_UNSUPPORTED"));
            StringAssert.Contains("尚未支援", errorMessage);
        }

        private static string BuildLinearFlowchartJson()
        {
            ExportGraphDto graphDto = new ExportGraphDto
            {
                version = "2.0",
                graphName = "ImportedChapter",
                startNodeId = "N001",
                nodes = new List<ExportNodeDto>
                {
                    new ExportNodeDto
                    {
                        id = "N001",
                        type = CanonicalNodeKinds.Start,
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto
                            {
                                portName = CanonicalPortSemantics.Flow,
                                toNodeId = "N002",
                                toPortName = CanonicalPortSemantics.Flow
                            }
                        }
                    },
                    new ExportNodeDto
                    {
                        id = "N002",
                        type = CurrentFlowProjectionNaming.DialogueNodeType,
                        content = "主句"
                    }
                }
            };

            return JsonUtility.ToJson(graphDto, true);
        }
    }
}
// ===== 變更結束 =====
