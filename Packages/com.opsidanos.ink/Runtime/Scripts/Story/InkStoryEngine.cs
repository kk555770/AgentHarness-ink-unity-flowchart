// ===== 變更開始 =====
// 2026/01/21 Opsidanos (修改原因：建立玩家模式最小 Ink 故事引擎)
// 預期結果：指定編譯後的 Ink `.json`（TextAsset）就能推進故事並吐出文字/選項，結束時不會無限空白推進
using System;
using System.Collections.Generic;
using Ink.Runtime;
using UnityEngine;

namespace OpsidanosInk.Runtime.Story
{
    public sealed class InkStoryEngine : MonoBehaviour
    {
        [Header("Ink")]
        [SerializeField] private TextAsset storyJsonAsset;

        public event Action<StoryOutput> OutputGenerated;

        private Story story;
        private int nextOutputId = 1;
        private bool hasEnded;

        private void Start()
        {
            if (storyJsonAsset == null)
            {
                Debug.LogError("[OpsidanosInk] 尚未指定 Story Json Asset（TextAsset）。", this);
                return;
            }

            story = new Story(storyJsonAsset.text);
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

            if (story.canContinue)
            {
                lineText = story.Continue().Trim();
            }
            else if (story.currentChoices.Count == 0)
            {
                lineText = "（故事結束）";
                hasEnded = true;
            }

            var tags = new List<string>(story.currentTags);
            var speaker = TryGetSpeakerFromTags(tags);
            var choices = new List<ChoiceOutput>(story.currentChoices.Count);

            for (int i = 0; i < story.currentChoices.Count; i++)
            {
                Choice choice = story.currentChoices[i];
                choices.Add(new ChoiceOutput(choice.index, choice.text.Trim()));
            }

            OutputGenerated?.Invoke(new StoryOutput(nextOutputId++, speaker, lineText, tags, choices));
        }

        private static string TryGetSpeakerFromTags(List<string> tags)
        {
            for (int i = 0; i < tags.Count; i++)
            {
                string tag = tags[i];
                if (tag.StartsWith("speaker:", StringComparison.Ordinal))
                {
                    return tag.Substring("speaker:".Length).Trim();
                }
            }

            return null;
        }
    }
}
// ===== 變更結束 =====
