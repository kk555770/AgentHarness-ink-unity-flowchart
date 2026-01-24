// ===== 變更開始 =====
// 2026/01/21 Opsidanos (修改原因：建立玩家模式最小輸出資料結構)
// 預期結果：Ink 故事引擎能用一致的資料格式把文字/選項/Tag 交給 UI
// ===== 變更開始 =====
// 2026/01/22 Opsidanos (修改原因：補上 HasEnded 與 ParsedTags，避免用字串判斷故事結束，並打通 Tag 結構化管線)
// 預期結果：UI/系統不再依賴「（故事結束）」字串；Tag 能以結構化資料往下傳
using System.Collections.Generic;

namespace OpsidanosInk.Runtime.Story
{
    public sealed class StoryOutput
    {
        public int OutputId { get; }
        public string Speaker { get; }
        public string LineText { get; }
        public bool HasEnded { get; }
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
            List<ChoiceOutput> choices)
        {
            OutputId = outputId;
            Speaker = speaker;
            LineText = lineText;
            HasEnded = hasEnded;
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
