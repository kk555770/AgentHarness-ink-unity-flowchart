// ===== 變更開始 =====
// 2026/01/25 Opsidanos (修改原因：提供推進阻擋的最小介面，讓快速連點時能先把演出強制刷新到終點)
// 預期結果：Presenter 推進前可先判斷 IsBusy；Busy 時呼叫 ForceComplete()，避免上一句演出與下一句疊加造成錯亂
namespace OpsidanosInk.Runtime.UI
{
    public interface IAdvanceBlocker
    {
        bool IsBusy { get; }
        void ForceComplete();
    }
}
// ===== 變更結束 =====

