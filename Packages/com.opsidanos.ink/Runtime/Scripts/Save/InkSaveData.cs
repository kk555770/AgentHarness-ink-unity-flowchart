// ===== 變更開始 =====
// 2026/01/30 Opsidanos (修改原因：建立存檔資料格式，供存檔/讀檔/倒帶使用)
// 預期結果：Ink state + 畫面狀態 + 當句輸出可以被序列化成 JSON，讀回後可用來還原畫面與 UI
using System;

namespace OpsidanosInk.Runtime.Save
{
    [Serializable]
    public sealed class InkSaveData
    {
        public int version = 1;

        public string inkStateJson;
        public StoryOutputSnapshot output;
        public PresentationSnapshot presentation;
    }

    [Serializable]
    public sealed class StoryOutputSnapshot
    {
        public int outputId;
        public string speaker;
        public string lineText;
        public bool hasEnded;
        public ChoiceSnapshot[] choices;
    }

    [Serializable]
    public sealed class ChoiceSnapshot
    {
        public int index;
        public string text;
    }

    [Serializable]
    public sealed class PresentationSnapshot
    {
        public string bgId;
        public string bgmId;

        // cg tag 的 value：可能是 "clear" 或某個 cg id
        public string cgValue;

        // char tag 的 value：可能是 "clear" 或 JSON 字串
        public string charValue;

        // 舊式 char-left/center/right 的 id（可能為空）
        public string charLeftId;
        public string charCenterId;
        public string charRightId;
    }
}
// ===== 變更結束 =====

