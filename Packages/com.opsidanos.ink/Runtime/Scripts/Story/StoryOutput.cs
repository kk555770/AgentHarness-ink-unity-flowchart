// ===== 變更開始 =====
// 2026/01/21 Opsidanos (修改原因：建立玩家模式最小輸出資料結構)
// 預期結果：Ink 故事引擎能用一致的資料格式把文字/選項/Tag 交給 UI
// ===== 變更開始 =====
// 2026/01/22 Opsidanos (修改原因：補上 HasEnded 與 ParsedTags，避免用字串判斷故事結束，並打通 Tag 結構化管線)
// 預期結果：UI/系統不再依賴「（故事結束）」字串；Tag 能以結構化資料往下傳
using System.Collections.Generic;

namespace OpsidanosInk.Runtime.Story
{
    // ===== 變更開始 =====
    // 2026/02/01 Opsidanos (修改原因：為了倒退（Rollback/Load）能套用相反的 char 層級規則，需要區分輸出來源)
    // 預期結果：一般推進輸出標記為 Normal；倒退/讀檔的外部輸出標記為 Restore
    public enum StoryOutputSource
    {
        Normal = 0,
        Restore = 1
    }
    // ===== 變更結束 =====

    public sealed class StoryOutput
    {
        public int OutputId { get; }
        public string Speaker { get; }
        public string LineText { get; }
        public bool HasEnded { get; }
        // ===== 變更開始 =====
        // 2026/02/01 Opsidanos (修改原因：讓各系統能分辨目前輸出是正常推進或倒退回放)
        // 預期結果：倒退回放時，演出元件可依 Source 套用相反的顯示規則
        public StoryOutputSource Source { get; }
        // ===== 變更結束 =====
        public List<string> Tags { get; }
        public List<InkTag> ParsedTags { get; }
        public List<ChoiceOutput> Choices { get; }

        public StoryOutput(
            int outputId,
            string speaker,
            string lineText,
            bool hasEnded,
            List<string> tags,
            List<InkTag> parsedTags,
            List<ChoiceOutput> choices,
            // ===== 變更開始 =====
            // 2026/02/01 Opsidanos (修改原因：新增來源參數，避免倒退輸出被當成一般輸出)
            // 預期結果：未指定時維持 Normal；需要倒退回放時由呼叫端明確指定 Restore
            StoryOutputSource source = StoryOutputSource.Normal)
            // ===== 變更結束 =====
        {
            OutputId = outputId;
            Speaker = speaker;
            LineText = lineText;
            HasEnded = hasEnded;
            Source = source;
            Tags = tags;
            ParsedTags = parsedTags;
            Choices = choices;
        }
    }

    public sealed class ChoiceOutput
    {
        public int Index { get; }
        public string Text { get; }

        public ChoiceOutput(int index, string text)
        {
            Index = index;
            Text = text;
        }
    }
}
// ===== 變更結束 =====
// ===== 變更結束 =====
