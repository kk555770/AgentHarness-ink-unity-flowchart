// ===== 變更開始 =====
// 2026/03/18 Opsidanos (修改原因：為 Batch 1 新增 current projection naming 測試，先守住 canonical 與 legacy `dialogue/action` 的對照規則)
// 預期結果：匯出匯入與未來作者工具共用的 current projection naming 不會再散落，也不會在重構時默默漂移
using NUnit.Framework;
using OpsidanosInk.CanonicalGraph;

namespace OpsidanosInk.Tests.EditMode
{
    public sealed class CurrentFlowProjectionNamingTests
    {
        [Test]
        public void CurrentFlowProjectionNaming_固定名稱維持穩定()
        {
            Assert.AreEqual("action", CurrentFlowProjectionNaming.DialogueNodeType);
            Assert.AreEqual("dialogue", CurrentFlowProjectionNaming.DialogueActionKindToken);
            Assert.AreEqual("action", CurrentFlowProjectionNaming.StageActionActionKindToken);
            Assert.AreEqual("custom", CurrentFlowProjectionNaming.CustomActionKindToken);
            Assert.AreEqual("unknown", CurrentFlowProjectionNaming.UnknownNodeType);
        }

        [Test]
        public void IsDialogueNodeType_可同時辨識Canonical與Legacy命名()
        {
            Assert.IsTrue(CurrentFlowProjectionNaming.IsDialogueNodeType(CanonicalNodeKinds.Dialogue));
            Assert.IsTrue(CurrentFlowProjectionNaming.IsDialogueNodeType(CurrentFlowProjectionNaming.DialogueNodeType));
            Assert.IsFalse(CurrentFlowProjectionNaming.IsDialogueNodeType(CanonicalNodeKinds.Start));
            Assert.IsFalse(CurrentFlowProjectionNaming.IsDialogueNodeType(CanonicalNodeKinds.StageAction));
            Assert.IsFalse(CurrentFlowProjectionNaming.IsDialogueNodeType("unknown"));
        }

        [Test]
        public void ToNodeType_會把Canonical對話轉成CurrentProjection命名()
        {
            Assert.AreEqual(CanonicalNodeKinds.Start, CurrentFlowProjectionNaming.ToNodeType(CanonicalNodeKinds.Start));
            Assert.AreEqual(CurrentFlowProjectionNaming.DialogueNodeType, CurrentFlowProjectionNaming.ToNodeType(CanonicalNodeKinds.Dialogue));
            Assert.AreEqual(CanonicalNodeKinds.StageAction, CurrentFlowProjectionNaming.ToNodeType(CanonicalNodeKinds.StageAction));
            Assert.AreEqual(CanonicalNodeKinds.Comment, CurrentFlowProjectionNaming.ToNodeType(CanonicalNodeKinds.Comment));
            Assert.AreEqual(CanonicalNodeKinds.Choice, CurrentFlowProjectionNaming.ToNodeType(CanonicalNodeKinds.Choice));
            Assert.AreEqual(CanonicalNodeKinds.Condition, CurrentFlowProjectionNaming.ToNodeType(CanonicalNodeKinds.Condition));
            Assert.AreEqual(CurrentFlowProjectionNaming.UnknownNodeType, CurrentFlowProjectionNaming.ToNodeType("unknown"));
        }
    }
}
// ===== 變更結束 =====
