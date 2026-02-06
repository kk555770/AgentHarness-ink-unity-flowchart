// ===== 變更開始 =====
// 2026/01/30 Opsidanos (修改原因：建立存檔資料格式，供存檔/讀檔/倒帶使用)
// 預期結果：Ink state + 畫面狀態 + 當句輸出可以被序列化成 JSON，讀回後可用來還原畫面與 UI
using System;

namespace OpsidanosInk.Runtime.Save
{
    // ===== 變更開始 =====
    // 2026/02/06 Opsidanos (修改原因：存檔需要保留「整條倒帶歷史 + 目前位置」，讀檔後才能倒帶到存檔點之前)
    // 預期結果：Load 後 rollbackBuffer 會回到存檔當下的歷史，不再只剩單點快照
    [Serializable]
    public sealed class InkSaveSlotData
    {
        public int version = 1;
        public int activeRollbackIndex;
        public InkSaveData[] rollbackHistory;
    }
    // ===== 變更結束 =====

    // ===== 變更開始 =====
    // 2026/02/06 Opsidanos (修改原因：多槽存檔需要一個可序列化的容器，承載手動槽與 Auto 槽)
    // 預期結果：之後若需要持久化到檔案，可直接序列化整個存檔銀行資料
    [Serializable]
    public sealed class InkSaveBankData
    {
        public int version = 1;
        public InkSaveSlotData[] manualSlots;
        public InkSaveSlotData autoSlot;
    }
    // ===== 變更結束 =====

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
