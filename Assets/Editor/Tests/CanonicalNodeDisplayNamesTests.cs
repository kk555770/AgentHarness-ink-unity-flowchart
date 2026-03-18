// ===== 變更開始 =====
// 2026/03/18 Opsidanos (修改原因：為 Batch 1 新增節點顯示名稱測試，先守住動作輸入、選項/條件 fallback 與 else 標籤的共用顯示語意)
// 預期結果：GraphToolkit 與未來作者工具共用的顯示名稱來源固定，不會因為 UI 重整把核心顯示語意洗掉
using NUnit.Framework;
using OpsidanosInk.CanonicalGraph;

namespace OpsidanosInk.Tests.EditMode
{
    public sealed class CanonicalNodeDisplayNamesTests
    {
        [Test]
        public void CanonicalNodeDisplayNames_固定名稱維持穩定()
        {
            Assert.AreEqual("選項", CanonicalNodeDisplayNames.Choice);
            Assert.AreEqual("條件", CanonicalNodeDisplayNames.Condition);
            Assert.AreEqual("動作", CanonicalNodeDisplayNames.DialogueActionInputPrefix);
            Assert.AreEqual("動作資料", CanonicalNodeDisplayNames.StageActionDataPort);
            Assert.AreEqual("否則", CanonicalNodeDisplayNames.ConditionElsePort);
        }

        [Test]
        public void BuildDialogueActionInputDisplayName_會產生穩定顯示名稱()
        {
            Assert.AreEqual("動作1", CanonicalNodeDisplayNames.BuildDialogueActionInputDisplayName(0));
            Assert.AreEqual("動作4", CanonicalNodeDisplayNames.BuildDialogueActionInputDisplayName(3));
        }

        [Test]
        public void FallbackLabel_會產生穩定的選項與條件名稱()
        {
            Assert.AreEqual("選項1", CanonicalNodeDisplayNames.GetChoicePortFallbackLabel(1));
            Assert.AreEqual("選項3", CanonicalNodeDisplayNames.GetChoicePortFallbackLabel(3));
            Assert.AreEqual("條件1", CanonicalNodeDisplayNames.GetConditionPortFallbackLabel(1));
            Assert.AreEqual("條件2", CanonicalNodeDisplayNames.GetConditionPortFallbackLabel(2));
        }
    }
}
// ===== 變更結束 =====
