// ===== 變更開始 =====
// 2026/01/24 Opsidanos (修改原因：補上 Tag → 事件路由與除錯輸出，方便後續接演出/資源綁定)
// 預期結果：Play Mode 時能清楚看到每句輸出觸發哪些 Tag，並提供事件讓其他元件訂閱
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpsidanosInk.Runtime.Story
{
    public sealed class InkTagEventRouter : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private InkStoryEngine storyEngine;

        [Header("Debug")]
        [SerializeField] private bool logKnownTags = true;
        [SerializeField] private bool logUnknownTags;

        public event Action<StoryOutput, InkTag> SpeakerTagReceived;
        public event Action<StoryOutput, InkTag> BackgroundTagReceived;
        public event Action<StoryOutput, InkTag> BgmTagReceived;
        public event Action<StoryOutput, InkTag> SeTagReceived;
        public event Action<StoryOutput, InkTag> ShakeTagReceived;
        public event Action<StoryOutput, InkTag> UnknownTagReceived;

        private void Awake()
        {
            if (storyEngine == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagEventRouter 尚未指定 Story Engine。", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (storyEngine != null)
            {
                storyEngine.OutputGenerated += OnStoryOutput;
            }
        }

        private void OnDisable()
        {
            if (storyEngine != null)
            {
                storyEngine.OutputGenerated -= OnStoryOutput;
            }
        }

        private void OnStoryOutput(StoryOutput output)
        {
            if (output == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagEventRouter 收到 null 的 StoryOutput。", this);
                return;
            }

            List<InkTag> tags = output.ParsedTags;
            if (tags == null || tags.Count == 0)
            {
                return;
            }

            List<string> knownTagTexts = null;
            List<string> unknownTagTexts = null;

            for (int i = 0; i < tags.Count; i++)
            {
                InkTag tag = tags[i];
                if (tag == null || string.IsNullOrWhiteSpace(tag.Key))
                {
                    continue;
                }

                bool isKnown = RouteTag(output, tag);
                if (isKnown)
                {
                    if (logKnownTags)
                    {
                        if (knownTagTexts == null)
                        {
                            knownTagTexts = new List<string>();
                        }

                        knownTagTexts.Add(ToTagText(tag));
                    }

                    continue;
                }

                UnknownTagReceived?.Invoke(output, tag);

                if (!logUnknownTags)
                {
                    continue;
                }

                if (unknownTagTexts == null)
                {
                    unknownTagTexts = new List<string>();
                }

                unknownTagTexts.Add(ToTagText(tag));
            }

            if (logKnownTags && knownTagTexts != null && knownTagTexts.Count > 0)
            {
                Debug.Log($"[OpsidanosInk][Tag] OutputId={output.OutputId} Tags={string.Join(", ", knownTagTexts)}", this);
            }

            if (logUnknownTags && unknownTagTexts != null && unknownTagTexts.Count > 0)
            {
                Debug.Log($"[OpsidanosInk][Tag][Unknown] OutputId={output.OutputId} Tags={string.Join(", ", unknownTagTexts)}", this);
            }
        }

        private bool RouteTag(StoryOutput output, InkTag tag)
        {
            switch (tag.Key)
            {
                case "speaker":
                    SpeakerTagReceived?.Invoke(output, tag);
                    return true;
                case "bg":
                    BackgroundTagReceived?.Invoke(output, tag);
                    return true;
                case "bgm":
                    BgmTagReceived?.Invoke(output, tag);
                    return true;
                case "se":
                    SeTagReceived?.Invoke(output, tag);
                    return true;
                case "shake":
                    ShakeTagReceived?.Invoke(output, tag);
                    return true;
                default:
                    return false;
            }
        }

        private static string ToTagText(InkTag tag)
        {
            return tag.HasValue ? $"{tag.Key}:{tag.Value}" : tag.Key;
        }
    }
}
// ===== 變更結束 =====

