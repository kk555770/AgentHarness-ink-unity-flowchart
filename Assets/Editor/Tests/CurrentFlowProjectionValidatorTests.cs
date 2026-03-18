// ===== 變更開始 =====
// 2026/03/18 Opsidanos (修改原因：新增 shared current projection validator 的純 DTO 測試檔，先守住 Batch 2 第一刀最重要的 sidecar 契約規則)
// 預期結果：不需要先建 GraphToolkit graph，也能直接測試 current projection validator 的合法與非法案例
using NUnit.Framework;
using OpsidanosInk.CanonicalGraph;
using System.Collections.Generic;
// ===== 變更結束 =====

namespace OpsidanosInk.Tests.EditMode
{
    public sealed class CurrentFlowProjectionValidatorTests
    {
        [Test]
        public void TryValidate_合法ChoiceGraph_通過()
        {
            // ===== 變更開始 =====
            // 2026/03/18 Opsidanos (修改原因：為 Batch 2 shared validator 補最小純 DTO 測試，不再每次都得靠 GraphToolkit graph 才能驗 sidecar 規則)
            // 預期結果：合法的 current projection choice 圖可直接通過 validator
            ExportGraphDto graphDto = new ExportGraphDto
            {
                startNodeId = "N000",
                nodes = new List<ExportNodeDto>
                {
                    new ExportNodeDto
                    {
                        id = "N000",
                        type = CanonicalNodeKinds.Start,
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto
                            {
                                portName = CanonicalPortSemantics.Flow,
                                toNodeId = "N001",
                                toPortName = CanonicalPortSemantics.Flow
                            }
                        },
                        nextIds = new List<string> { "N001" }
                    },
                    new ExportNodeDto
                    {
                        id = "N001",
                        type = CanonicalNodeKinds.Choice,
                        choiceMode = "*",
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto { portName = "Out0", toNodeId = "N002", label = "去 A" },
                            new ExportNodeOutputDto { portName = "Out1", toNodeId = "N003", label = "去 B" }
                        }
                    },
                    new ExportNodeDto { id = "N002", type = CurrentFlowProjectionNaming.DialogueNodeType, content = "A" },
                    new ExportNodeDto { id = "N003", type = CurrentFlowProjectionNaming.DialogueNodeType, content = "B" }
                }
            };

            bool success = CurrentFlowProjectionValidator.TryValidate(graphDto, out string errorMessage);

            Assert.IsTrue(success, $"合法 current projection 應驗證成功，但失敗：{errorMessage}");
            Assert.IsEmpty(errorMessage);
            // ===== 變更結束 =====
        }

        [Test]
        public void TryValidate_對話內容藏Divert_失敗且訊息明確()
        {
            // ===== 變更開始 =====
            // 2026/03/18 Opsidanos (修改原因：把 dialogue content 禁止偷藏流程結構的規則移到 shared validator 後，需要鎖住純 DTO 入口也會擋下來)
            // 預期結果：即使還沒建 GraphToolkit graph，只要 sidecar 內容有未跳脫 `->` 就會直接失敗
            ExportGraphDto graphDto = new ExportGraphDto
            {
                startNodeId = "N000",
                nodes = new List<ExportNodeDto>
                {
                    new ExportNodeDto
                    {
                        id = "N000",
                        type = CanonicalNodeKinds.Start,
                        nextIds = new List<string> { "N001" }
                    },
                    new ExportNodeDto
                    {
                        id = "N001",
                        type = CurrentFlowProjectionNaming.DialogueNodeType,
                        content = "你好 -> END"
                    }
                }
            };

            bool success = CurrentFlowProjectionValidator.TryValidate(graphDto, out string errorMessage);

            Assert.IsFalse(success, "內容藏流程的 DTO 應驗證失敗。");
            StringAssert.Contains("內容", errorMessage);
            StringAssert.Contains("->", errorMessage);
            StringAssert.Contains("\\\\->", errorMessage);
            // ===== 變更結束 =====
        }

        [Test]
        public void TryValidate_StageAction目標不是Dialogue_失敗()
        {
            // ===== 變更開始 =====
            // 2026/03/18 Opsidanos (修改原因：stageAction 的 sidecar 規則也要能在 shared validator 先被擋下，不必等 exporter/importer 各自重寫同一套訊息)
            // 預期結果：動作節點若把資料線接到非 dialogue 節點，validator 會直接回報失敗
            ExportGraphDto graphDto = new ExportGraphDto
            {
                startNodeId = "N000",
                nodes = new List<ExportNodeDto>
                {
                    new ExportNodeDto
                    {
                        id = "N000",
                        type = CanonicalNodeKinds.Start,
                        nextIds = new List<string> { "N001" }
                    },
                    new ExportNodeDto
                    {
                        id = "N001",
                        type = CanonicalNodeKinds.StageAction,
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto
                            {
                                portName = CanonicalPortSemantics.ActionData,
                                toNodeId = "N002",
                                toPortName = CanonicalPortSemantics.BuildActionInputPortName(0)
                            }
                        }
                    },
                    new ExportNodeDto
                    {
                        id = "N002",
                        type = CanonicalNodeKinds.Comment,
                        content = "不是對話"
                    }
                }
            };

            bool success = CurrentFlowProjectionValidator.TryValidate(graphDto, out string errorMessage);

            Assert.IsFalse(success, "stageAction 若接到非 dialogue 節點，應驗證失敗。");
            StringAssert.Contains("只能接到對話節點", errorMessage);
            // ===== 變更結束 =====
        }

        [Test]
        public void TryValidate_ConditionElse不在最後_失敗()
        {
            // ===== 變更開始 =====
            // 2026/03/18 Opsidanos (修改原因：condition else 規則要能由 shared validator 統一守住，避免 importer/exporter 各自藏一套)
            // 預期結果：else 不在最後時，純 DTO 驗證就會直接回報失敗
            ExportGraphDto graphDto = new ExportGraphDto
            {
                startNodeId = "N000",
                nodes = new List<ExportNodeDto>
                {
                    new ExportNodeDto
                    {
                        id = "N000",
                        type = CanonicalNodeKinds.Start,
                        nextIds = new List<string> { "N001" }
                    },
                    new ExportNodeDto
                    {
                        id = "N001",
                        type = CanonicalNodeKinds.Condition,
                        outputs = new List<ExportNodeOutputDto>
                        {
                            new ExportNodeOutputDto { portName = "Out0", toNodeId = "N002", isElse = true },
                            new ExportNodeOutputDto { portName = "Out1", toNodeId = "N003", condition = "favor > 7" }
                        }
                    },
                    new ExportNodeDto { id = "N002", type = CurrentFlowProjectionNaming.DialogueNodeType, content = "A" },
                    new ExportNodeDto { id = "N003", type = CurrentFlowProjectionNaming.DialogueNodeType, content = "B" }
                }
            };

            bool success = CurrentFlowProjectionValidator.TryValidate(graphDto, out string errorMessage);

            Assert.IsFalse(success, "condition else 不在最後時，應驗證失敗。");
            StringAssert.Contains("只有最後一個輸出埠可以是 else", errorMessage);
            // ===== 變更結束 =====
        }
    }
}
