// ===== 變更開始 =====
// 2026/03/17 Opsidanos (修改原因：為 Batch 0 的 canonical port semantics 骨架補上最小 EditMode 測試，先守住埠語意名稱與 ActionIn 命名規則)
// 預期結果：後續抽出 port 語意時，Flow / ActionData / ActionIn 命名已有固定測試護欄
using NUnit.Framework;
using OpsidanosInk.CanonicalGraph;

namespace OpsidanosInk.Tests.EditMode
{
    public sealed class CanonicalPortSemanticsTests
    {
        [Test]
        public void CanonicalPortSemantics_固定名稱維持穩定()
        {
            Assert.AreEqual("Flow", CanonicalPortSemantics.Flow);
            Assert.AreEqual("ActionData", CanonicalPortSemantics.ActionData);
            Assert.AreEqual("ActionIn", CanonicalPortSemantics.ActionInputPrefix);
        }

        [Test]
        public void BuildActionInputPortName_會產生穩定名稱()
        {
            Assert.AreEqual("ActionIn0", CanonicalPortSemantics.BuildActionInputPortName(0));
            Assert.AreEqual("ActionIn3", CanonicalPortSemantics.BuildActionInputPortName(3));
        }

        [Test]
        public void TryParseActionInputOrder_可辨識合法與非法輸入埠名稱()
        {
            Assert.IsTrue(CanonicalPortSemantics.TryParseActionInputOrder("ActionIn0", out int order0));
            Assert.AreEqual(0, order0);

            Assert.IsTrue(CanonicalPortSemantics.TryParseActionInputOrder("ActionIn7", out int order7));
            Assert.AreEqual(7, order7);

            Assert.IsFalse(CanonicalPortSemantics.TryParseActionInputOrder("ActionData", out int invalidOrder));
            Assert.AreEqual(-1, invalidOrder);

            Assert.IsFalse(CanonicalPortSemantics.TryParseActionInputOrder("ActionIn-1", out int negativeOrder));
            Assert.AreEqual(-1, negativeOrder);
        }
    }
}
// ===== 變更結束 =====
