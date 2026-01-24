// ===== 變更開始 =====
// 2026/01/22 Opsidanos (修改原因：修正 Story 命名衝突造成的編譯錯誤)
// 預期結果：Unity Console 不再出現 CS0118（'Story' is a namespace but is used like a type）
using System;
using System.Collections.Generic;
using UnityEngine;
using InkRuntimeStory = Ink.Runtime.Story;
using InkRuntimeChoice = Ink.Runtime.Choice;

namespace OpsidanosInk.Runtime.Story
{
    // ===== 變更開始 =====
    // 2026/01/22 Opsidanos (修改原因：打通 Tag→結構化→事件，並避免用字串判斷故事結束)
    // 預期結果：StoryOutput 帶有 HasEnded 與 ParsedTags，讓 UI/系統不再依賴「（故事結束）」字串
    public sealed class InkStoryEngine : MonoBehaviour
    {
        [Header("Ink")]
        [SerializeField] private TextAsset storyJsonAsset;

        public event Action<StoryOutput> OutputGenerated;

        private InkRuntimeStory story;
        private int nextOutputId = 1;
        private bool hasEnded;

        private void Start()
        {
            if (storyJsonAsset == null)
            {
                Debug.LogError("[OpsidanosInk] 尚未指定 Story Json Asset（TextAsset）。", this);
                return;
            }

            story = new InkRuntimeStory(storyJsonAsset.text);
            EmitNext();
        }

        public void Continue()
        {
            if (hasEnded)
            {
                Debug.LogError("[OpsidanosInk] 故事已結束，無法繼續推進。", this);
                return;
            }

            EmitNext();
        }

        public void ChooseChoice(int choiceIndex)
        {
            if (hasEnded)
            {
                Debug.LogError("[OpsidanosInk] 故事已結束，無法選擇選項。", this);
                return;
            }

            if (story == null)
            {
                Debug.LogError("[OpsidanosInk] Story 尚未初始化，無法選擇選項。", this);
                return;
            }

            story.ChooseChoiceIndex(choiceIndex);
            EmitNext();
        }

        private void EmitNext()
        {
            if (hasEnded)
            {
                Debug.LogError("[OpsidanosInk] 故事已結束，無法繼續推進。", this);
                return;
            }

            if (story == null)
            {
                Debug.LogError("[OpsidanosInk] Story 尚未初始化，無法推進。", this);
                return;
            }

            string lineText = null;
            bool outputHasEnded = false;

            if (story.canContinue)
            {
                lineText = story.Continue().Trim();
            }
            else if (story.currentChoices.Count == 0)
            {
                lineText = "（故事結束）";
                hasEnded = true;
                outputHasEnded = true;
            }

            List<string> rawTags;
            List<InkTag> parsedTags;

            // ===== 變更開始 =====
            // 2026/01/24 Opsidanos (修改原因：避免故事結束時重複沿用上一句 Tag，造成 Console Tag log 重複)
            // 預期結果：HasEnded 那一筆輸出不再帶出上一句 Tag，Tag log 更清楚
            if (outputHasEnded)
            {
                rawTags = new List<string>(0);
                parsedTags = new List<InkTag>(0);
            }
            else
            {
                rawTags = new List<string>(story.currentTags);
                parsedTags = InkTagParser.Parse(rawTags);
            }
            // ===== 變更結束 =====
            var speaker = TryGetSpeakerFromTags(parsedTags);
            var choices = new List<ChoiceOutput>(story.currentChoices.Count);

            for (int i = 0; i < story.currentChoices.Count; i++)
            {
                InkRuntimeChoice choice = story.currentChoices[i];
                choices.Add(new ChoiceOutput(choice.index, choice.text.Trim()));
            }

            OutputGenerated?.Invoke(new StoryOutput(nextOutputId++, speaker, lineText, outputHasEnded, rawTags, parsedTags, choices));
        }

        private static string TryGetSpeakerFromTags(List<InkTag> tags)
        {
            for (int i = 0; i < tags.Count; i++)
            {
                InkTag tag = tags[i];
                if (tag.Key == "speaker")
                {
                    return tag.Value;
                }
            }

            return null;
        }
    }
    // ===== 變更結束 =====
}
// ===== 變更結束 =====
