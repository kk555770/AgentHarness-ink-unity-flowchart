// ===== 變更開始 =====
// 2026/03/17 Opsidanos (修改原因：為 Batch 0 的 canonical invariant 骨架補上最小 EditMode 測試，先守住 invariant 名稱與線性節點判定入口)
// 預期結果：後續 validator 開始實作前，最小 invariant 名稱與單一 Flow 輸出規則已有固定測試護欄
using NUnit.Framework;
using OpsidanosInk.CanonicalGraph;

namespace OpsidanosInk.Tests.EditMode
{
    public sealed class CanonicalGraphInvariantTests
    {
        [Test]
        public void CanonicalGraphInvariant_固定名稱維持穩定()
        {
            Assert.AreEqual("requiresStartNode", CanonicalGraphInvariant.RequiresStartNode);
            Assert.AreEqual("singleFlowOutput", CanonicalGraphInvariant.SingleFlowOutput);
            Assert.AreEqual("conditionElseLast", CanonicalGraphInvariant.ConditionElseLast);
        }

        [Test]
        public void UsesSingleFlowOutput_可辨識線性與分岔節點()
        {
            Assert.IsTrue(CanonicalGraphInvariant.UsesSingleFlowOutput(CanonicalNodeKinds.Start));
            Assert.IsTrue(CanonicalGraphInvariant.UsesSingleFlowOutput(CanonicalNodeKinds.Dialogue));
            Assert.IsTrue(CanonicalGraphInvariant.UsesSingleFlowOutput(CanonicalNodeKinds.StageAction));
            Assert.IsTrue(CanonicalGraphInvariant.UsesSingleFlowOutput(CanonicalNodeKinds.Comment));

            Assert.IsFalse(CanonicalGraphInvariant.UsesSingleFlowOutput(CanonicalNodeKinds.Choice));
            Assert.IsFalse(CanonicalGraphInvariant.UsesSingleFlowOutput(CanonicalNodeKinds.Condition));
            Assert.IsFalse(CanonicalGraphInvariant.UsesSingleFlowOutput("unknown"));
        }
    }
}
// ===== 變更結束 =====
