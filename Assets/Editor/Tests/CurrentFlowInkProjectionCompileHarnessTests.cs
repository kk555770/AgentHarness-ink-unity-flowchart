// ===== 變更開始 =====
// 2026/03/25 Opsidanos (修改原因：為 Batch B 的 Ink projection validation 補上 compile-level regression harness)
// 預期結果：`ValidateProjection(Ink)` 與 `ProjectGraph(Ink)` 會實際經過可替換的 compile probe，且 JSON control plane 可直接鎖住編譯成功/失敗語意
using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpsidanosInk.CanonicalGraph;
using UnityEngine;
// ===== 變更結束 =====

namespace OpsidanosInk.Tests.EditMode
{
    public sealed class CurrentFlowInkProjectionCompileHarnessTests
    {
        private ICurrentFlowInkCompileProbe previousProbe;

        [SetUp]
        public void SetUp()
        {
            previousProbe = CurrentFlowInkCompileProbeRegistry.Current;
        }

        [TearDown]
        public void TearDown()
        {
            CurrentFlowInkCompileProbeRegistry.Current = previousProbe;
        }

        [Test]
        public void TryValidateProjection_Ink_會呼叫CompileProbe並回報成功()
        {
            CountingInkCompileProbe probe = new CountingInkCompileProbe();
            CurrentFlowInkCompileProbeRegistry.Current = probe;

            CanonicalGraphDocument graph = BuildLinearGraph();

            bool success = CurrentFlowProjectionService.TryValidateProjection(
                graph,
                CurrentFlowProjectionService.InkTarget,
                string.Empty,
                out string resolvedProjectionVersion,
                out string errorCode,
                out string errorMessage);

            Assert.IsTrue(success, errorMessage);
            Assert.That(errorCode, Is.Empty);
            Assert.That(errorMessage, Is.Empty);
            Assert.That(resolvedProjectionVersion, Is.EqualTo("2.0"));
            Assert.That(probe.CallCount, Is.EqualTo(1));
            StringAssert.Contains("=== knot_N001 ===", probe.LastContent);
            StringAssert.Contains("-> knot_N002", probe.LastContent);
        }

        [Test]
        public void TryValidateProjection_Ink_編譯失敗時會回報ProjectionCompileFailed()
        {
            FailingInkCompileProbe probe = new FailingInkCompileProbe("模擬 Ink 編譯失敗。");
            CurrentFlowInkCompileProbeRegistry.Current = probe;

            CanonicalGraphDocument graph = BuildLinearGraph();

            bool success = CurrentFlowProjectionService.TryValidateProjection(
                graph,
                CurrentFlowProjectionService.InkTarget,
                string.Empty,
                out string resolvedProjectionVersion,
                out string errorCode,
                out string errorMessage);

            Assert.IsFalse(success);
            Assert.That(errorCode, Is.EqualTo("PROJECTION_COMPILE_FAILED"));
            StringAssert.Contains("模擬 Ink 編譯失敗", errorMessage);
            Assert.That(resolvedProjectionVersion, Is.EqualTo("2.0"));
            Assert.That(probe.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void TryProjectGraph_Ink_會呼叫CompileProbe並回傳可編譯文本()
        {
            CountingInkCompileProbe probe = new CountingInkCompileProbe();
            CurrentFlowInkCompileProbeRegistry.Current = probe;

            CanonicalGraphDocument graph = BuildLinearGraph();

            bool success = CurrentFlowProjectionService.TryProjectGraph(
                graph,
                CurrentFlowProjectionService.InkTarget,
                string.Empty,
                out string resolvedProjectionVersion,
                out string projectionText,
                out string projectionJson,
                out string errorCode,
                out string errorMessage);

            Assert.IsTrue(success, errorMessage);
            Assert.That(errorCode, Is.Empty);
            Assert.That(errorMessage, Is.Empty);
            Assert.That(resolvedProjectionVersion, Is.EqualTo("2.0"));
            Assert.That(projectionJson, Is.EqualTo(string.Empty));
            Assert.That(probe.CallCount, Is.EqualTo(1));
            Assert.That(probe.LastContent, Is.EqualTo(projectionText));
            StringAssert.Contains("=== knot_N001 ===", projectionText);
            StringAssert.Contains("=== knot_N002 ===", projectionText);
        }

        [Test]
        public void TryValidateProjection_Ink_未手動安裝Probe時會退回預設CompilerProbe()
        {
            CurrentFlowInkCompileProbeRegistry.Current = null;

            CanonicalGraphDocument graph = BuildLinearGraph();

            bool success = CurrentFlowProjectionService.TryValidateProjection(
                graph,
                CurrentFlowProjectionService.InkTarget,
                string.Empty,
                out string resolvedProjectionVersion,
                out string errorCode,
                out string errorMessage);

            Assert.IsTrue(success, errorMessage);
            Assert.That(errorCode, Is.Empty);
            Assert.That(errorMessage, Is.Empty);
            Assert.That(resolvedProjectionVersion, Is.EqualTo("2.0"));
            Assert.That(CurrentFlowInkCompileProbeRegistry.Current, Is.TypeOf<CurrentFlowDefaultInkCompileProbe>());
        }

        [Test]
        public void Dispatch_ProjectGraph_Ink_會呼叫CompileProbe並回傳ProjectionText()
        {
            CountingInkCompileProbe probe = new CountingInkCompileProbe();
            CurrentFlowInkCompileProbeRegistry.Current = probe;

            CanonicalGraphJsonCommandDispatcher dispatcher = BuildConnectedLinearDispatcher();
            CanonicalGraphJsonResponse response = Dispatch(dispatcher, BuildProjectGraphRequest("chapter-01", CurrentFlowProjectionService.InkTarget));

            Assert.IsTrue(response.success);
            Assert.IsFalse(response.applied);
            Assert.That(response.result.target, Is.EqualTo(CurrentFlowProjectionService.InkTarget));
            Assert.That(response.result.projectionJson, Is.EqualTo(string.Empty));
            Assert.That(probe.CallCount, Is.EqualTo(1));
            Assert.That(response.result.projectionText, Is.EqualTo(probe.LastContent));
            StringAssert.Contains("=== knot_N001 ===", response.result.projectionText);
            StringAssert.Contains("=== knot_N002 ===", response.result.projectionText);
        }

        private static CanonicalGraphDocument BuildLinearGraph()
        {
            CanonicalGraphDocument graph = CanonicalGraphCommandService.CreateGraph(
                "harness-graph",
                "canonical-1",
                "{\"graphName\":\"HarnessGraph\",\"projectionVersion\":\"2.0\"}").graph;

            CanonicalGraphCommandService.CreateNode(graph, new CanonicalGraphNodeRecord
            {
                nodeId = "N001",
                nodeType = CanonicalNodeKinds.Start
            });
            CanonicalGraphCommandService.CreateNode(graph, new CanonicalGraphNodeRecord
            {
                nodeId = "N002",
                nodeType = CanonicalNodeKinds.Dialogue,
                payloadJson = "{\"content\":\"主句\"}"
            });

            CanonicalGraphCommandService.ConnectPorts(graph, "N001", CanonicalPortSemantics.Flow, "N002", CanonicalPortSemantics.Flow);
            return graph;
        }

        private static CanonicalGraphJsonResponse Dispatch(CanonicalGraphJsonCommandDispatcher dispatcher, CanonicalGraphJsonRequest request)
        {
            bool success = dispatcher.TryDispatch(JsonUtility.ToJson(request, true), out string responseJson);
            CanonicalGraphJsonResponse response = JsonUtility.FromJson<CanonicalGraphJsonResponse>(responseJson);
            Assert.That(response, Is.Not.Null);
            Assert.That(success, Is.EqualTo(response.success));
            return response;
        }

        private static CanonicalGraphJsonRequest BuildProjectGraphRequest(string graphId, string target)
        {
            return new CanonicalGraphJsonRequest
            {
                operation = "ProjectGraph",
                input = new CanonicalGraphJsonRequestInput
                {
                    graphId = graphId,
                    target = target
                }
            };
        }

        private static CanonicalGraphJsonCommandDispatcher BuildConnectedLinearDispatcher()
        {
            CanonicalGraphJsonCommandDispatcher dispatcher = new CanonicalGraphJsonCommandDispatcher();
            Dispatch(dispatcher, BuildCreateGraphRequest("chapter-01"));
            Dispatch(dispatcher, BuildCreateNodeRequest("chapter-01", "N001", CanonicalNodeKinds.Start));
            Dispatch(dispatcher, BuildCreateNodeRequest("chapter-01", "N002", CanonicalNodeKinds.Dialogue, "主句"));
            Dispatch(dispatcher, BuildConnectPortsRequest("chapter-01", "N001", CanonicalPortSemantics.Flow, "N002", CanonicalPortSemantics.Flow));
            return dispatcher;
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

        private sealed class CountingInkCompileProbe : ICurrentFlowInkCompileProbe
        {
            public int CallCount { get; private set; }

            public string LastContent { get; private set; } = string.Empty;

            public bool TryCompile(string inkContent, out string errorMessage)
            {
                CallCount++;
                LastContent = inkContent ?? string.Empty;
                errorMessage = string.Empty;

                try
                {
                    List<string> errors = new List<string>();
                    var compiler = new Ink.Compiler(
                        LastContent,
                        new Ink.Compiler.Options
                        {
                            errorHandler = (message, type) =>
                            {
                                if (type == Ink.ErrorType.Error)
                                {
                                    errors.Add(message);
                                }
                            }
                        });

                    Ink.Runtime.Story story = compiler.Compile();
                    if (errors.Count > 0)
                    {
                        errorMessage = string.Join(" | ", errors);
                        return false;
                    }

                    if (story == null)
                    {
                        errorMessage = "Ink.Compiler.Compile() 回傳 null。";
                        return false;
                    }

                    string storyJson = story.ToJson();
                    if (string.IsNullOrEmpty(storyJson))
                    {
                        errorMessage = "編譯後的 Story JSON 不應為空。";
                        return false;
                    }

                    return true;
                }
                catch (Exception exception)
                {
                    errorMessage = $"Ink 編譯器丟出例外：{exception.Message}";
                    return false;
                }
            }
        }

        private sealed class FailingInkCompileProbe : ICurrentFlowInkCompileProbe
        {
            private readonly string failureMessage;

            public FailingInkCompileProbe(string failureMessage)
            {
                this.failureMessage = failureMessage ?? string.Empty;
            }

            public int CallCount { get; private set; }

            public bool TryCompile(string inkContent, out string errorMessage)
            {
                CallCount++;
                errorMessage = failureMessage;
                return false;
            }
        }
    }
}
// ===== 變更結束 =====
