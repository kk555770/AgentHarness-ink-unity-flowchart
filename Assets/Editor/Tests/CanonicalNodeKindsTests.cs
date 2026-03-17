// ===== 變更開始 =====
// 2026/03/17 Opsidanos (修改原因：為 Batch 0 的 canonical node kind 骨架補上最小 EditMode 測試，先守住節點種類名稱與入口)
// 預期結果：後續抽 GraphToolkit 語意時，node kind 名稱有固定測試護欄，不會在重構中被悄悄改掉
using NUnit.Framework;
using OpsidanosInk.CanonicalGraph;

namespace OpsidanosInk.Tests.EditMode
{
    public sealed class CanonicalNodeKindsTests
    {
        [Test]
        public void CanonicalNodeKinds_All_維持最小固定順序()
        {
            CollectionAssert.AreEqual(
                new[]
                {
                    CanonicalNodeKinds.Start,
                    CanonicalNodeKinds.Dialogue,
                    CanonicalNodeKinds.StageAction,
                    CanonicalNodeKinds.Comment,
                    CanonicalNodeKinds.Choice,
                    CanonicalNodeKinds.Condition
                },
                CanonicalNodeKinds.All);
        }

        [Test]
        public void CanonicalNodeKinds_IsKnown_可辨識已知與未知節點種類()
        {
            Assert.IsTrue(CanonicalNodeKinds.IsKnown(CanonicalNodeKinds.Start));
            Assert.IsTrue(CanonicalNodeKinds.IsKnown(CanonicalNodeKinds.Dialogue));
            Assert.IsTrue(CanonicalNodeKinds.IsKnown(CanonicalNodeKinds.StageAction));
            Assert.IsTrue(CanonicalNodeKinds.IsKnown(CanonicalNodeKinds.Comment));
            Assert.IsTrue(CanonicalNodeKinds.IsKnown(CanonicalNodeKinds.Choice));
            Assert.IsTrue(CanonicalNodeKinds.IsKnown(CanonicalNodeKinds.Condition));
            Assert.IsFalse(CanonicalNodeKinds.IsKnown("unknown"));
        }
    }
}
// ===== 變更結束 =====
