// ===== 變更開始 =====
// 2026/01/22 Opsidanos (修改原因：建立 Ink Tag 的結構化資料)
// 預期結果：Tag 能用 key/value（或只有 key）表示，後續事件管線可直接使用
namespace OpsidanosInk.Runtime.Story
{
    public sealed class InkTag
    {
        public string Key { get; }
        public string Value { get; }
        public bool HasValue => Value != null;

        public InkTag(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
// ===== 變更結束 =====
