namespace OpsidanosInk.Editor
{
    // ===== 變更開始 =====
    // 2026/03/18 Opsidanos (修改原因：將 GraphToolkit option key、顯示名稱與預設值從節點檔抽離，讓節點定義與表單 schema 分層)
    // 預期結果：`InkFlowChartNodes.cs` 只保留節點本體；Nodes/Exporter/Importer 共用同一份 Editor-only option schema
    public static class InkFlowNodeOptionSchema
    {
        public const string DialogueContentOptionName = "Content";
        public const string DialogueContentOptionDisplayName = "對話內容";
        public const string DialogueActionInputCountOptionName = "ActionInputCount";
        public const string DialogueActionInputCountOptionDisplayName = "動作輸入數量";
        public const int DialogueActionInputCountDefaultValue = 1;

        public const string StageActionContentOptionName = "Content";
        public const string StageActionContentOptionDisplayName = "動作內容";

        public const string CommentNoteOptionName = "Note";
        public const string CommentNoteOptionDisplayName = "註解";

        public const string ChoiceOutputCountOptionName = "OutputCount";
        public const string ChoiceTextsOptionName = "ChoiceTexts";
        public const string ChoiceModeOptionName = "ChoiceMode";
        public const string ChoiceOutputCountOptionDisplayName = "選項數量";
        public const string ChoiceModeOptionDisplayName = "選項模式";
        public const string ChoiceTextsOptionDisplayName = "選項文字（每行一個）";
        public const string ChoiceTextsDefaultValue = "選項1\n選項2";

        public const string ConditionOutputCountOptionName = "OutputCount";
        public const string ConditionTextsOptionName = "ConditionTexts";
        public const string ConditionOutputCountOptionDisplayName = "分支數量（含否則）";
        public const string ConditionTextsOptionDisplayName = "條件（每行一個；最後一個輸出埠為否則）";
        public const string ConditionTextsDefaultValue = "favor > 7";
    }
    // ===== 變更結束 =====
}
