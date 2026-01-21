// ===== 變更開始 =====
// 2026/01/21 Opsidanos (修改原因：建立玩家模式最小輸出資料結構)
// 預期結果：Ink 故事引擎能用一致的資料格式把文字/選項/Tag 交給 UI
using System.Collections.Generic;

namespace OpsidanosInk.Runtime.Story
{
    public sealed class StoryOutput
    {
        public int OutputId { get; }
        public string Speaker { get; }
        public string LineText { get; }
        public List<string> Tags { get; }
        public List<ChoiceOutput> Choices { get; }

        public StoryOutput(
            int outputId,
            string speaker,
            string lineText,
            List<string> tags,
            List<ChoiceOutput> choices)
        {
            OutputId = outputId;
            Speaker = speaker;
            LineText = lineText;
            Tags = tags;
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

