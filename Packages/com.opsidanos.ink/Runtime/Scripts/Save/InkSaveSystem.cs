// ===== 變更開始 =====
// 2026/01/30 Opsidanos (修改原因：新增存檔/讀檔/倒帶系統，讓玩家模式能回到同一句並還原演出狀態)
// 預期結果：能存一份存檔槽位；能讀回並刷新 UI 與 Tag 演出；能最多倒帶 N 步
using System;
using System.Collections.Generic;
using OpsidanosInk.Runtime.Story;
using UnityEngine;

namespace OpsidanosInk.Runtime.Save
{
    public sealed class InkSaveSystem : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private InkStoryEngine storyEngine;

        [Header("Rollback")]
        [SerializeField] private int rollbackCapacity = 50;

        [Header("Debug")]
        [SerializeField] private bool logSaveLoad = true;

        private readonly List<InkSaveData> rollbackBuffer = new List<InkSaveData>();
        private InkSaveData saveSlot;

        private readonly PresentationSnapshot currentPresentation = new PresentationSnapshot
        {
            bgId = "clear",
            bgmId = "stop",
            cgValue = "clear",
            charValue = "clear",
            charLeftId = "clear",
            charCenterId = "clear",
            charRightId = "clear"
        };

        private bool suppressCapture;

        private void Awake()
        {
            if (storyEngine == null)
            {
                Debug.LogError("[OpsidanosInk] InkSaveSystem 尚未指定 Story Engine。", this);
                enabled = false;
                return;
            }

            if (rollbackCapacity <= 0)
            {
                Debug.LogError("[OpsidanosInk] InkSaveSystem rollbackCapacity 必須大於 0。", this);
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
            if (suppressCapture)
            {
                return;
            }

            if (output == null)
            {
                Debug.LogError("[OpsidanosInk] InkSaveSystem 收到 null 的 StoryOutput。", this);
                return;
            }

            UpdatePresentationFromTags(output.ParsedTags);

            if (!storyEngine.TryGetStoryStateJson(out string storyStateJson))
            {
                return;
            }

            InkSaveData snapshot = BuildSaveData(output, storyStateJson, currentPresentation);
            PushRollback(snapshot);
        }

        public void SaveToSlot()
        {
            if (!TryGetLatestSnapshot(out InkSaveData latestSnapshot))
            {
                Debug.LogError("[OpsidanosInk] InkSaveSystem 無法存檔：目前還沒有任何可存的輸出。", this);
                return;
            }

            saveSlot = CloneSaveData(latestSnapshot);

            if (logSaveLoad)
            {
                int outputId = saveSlot.output != null ? saveSlot.output.outputId : -1;
                Debug.Log($"[OpsidanosInk][Save] 存檔完成 OutputId={outputId}", this);
            }
        }

        public void LoadFromSlot()
        {
            if (saveSlot == null)
            {
                Debug.LogError("[OpsidanosInk] InkSaveSystem 無法讀檔：目前沒有存檔資料。", this);
                return;
            }

            Restore(saveSlot, resetRollbackBuffer: true, logPrefix: "[OpsidanosInk][Load]");
        }

        public void RollbackOnce()
        {
            if (rollbackBuffer.Count <= 1)
            {
                Debug.LogError("[OpsidanosInk] InkSaveSystem 無法倒帶：rollbackBuffer 不足（至少需要 2 份快照）。", this);
                return;
            }

            rollbackBuffer.RemoveAt(rollbackBuffer.Count - 1);
            InkSaveData target = rollbackBuffer[rollbackBuffer.Count - 1];
            Restore(target, resetRollbackBuffer: false, logPrefix: "[OpsidanosInk][Rollback]");
        }

        private void Restore(InkSaveData target, bool resetRollbackBuffer, string logPrefix)
        {
            if (target == null)
            {
                Debug.LogError("[OpsidanosInk] InkSaveSystem Restore 收到 null 的存檔資料。", this);
                return;
            }

            if (string.IsNullOrWhiteSpace(target.inkStateJson))
            {
                Debug.LogError("[OpsidanosInk] InkSaveSystem Restore 失敗：inkStateJson 為空。", this);
                return;
            }

            if (target.output == null)
            {
                Debug.LogError("[OpsidanosInk] InkSaveSystem Restore 失敗：output 為 null。", this);
                return;
            }

            if (target.presentation == null)
            {
                Debug.LogError("[OpsidanosInk] InkSaveSystem Restore 失敗：presentation 為 null。", this);
                return;
            }

            suppressCapture = true;
            try
            {
                if (!storyEngine.TryLoadStoryStateJson(target.inkStateJson))
                {
                    return;
                }

                // ===== 變更開始 =====
                // 2026/02/01 Opsidanos (修改原因：倒退/讀檔只還原畫面，卻沒同步 currentPresentation，導致後續快照被污染)
                // 預期結果：倒退/讀檔後再推進，背景/CG/角色等狀態不會被錯誤沿用；倒帶會在正確句子切換
                ApplyRestoredPresentationToCurrent(target.presentation);
                // ===== 變更結束 =====

                if (resetRollbackBuffer)
                {
                    rollbackBuffer.Clear();
                    rollbackBuffer.Add(CloneSaveData(target));
                }

                StoryOutput restoredOutput = BuildStoryOutput(target.output, target.presentation);
                storyEngine.EmitExternalOutput(restoredOutput);

                if (logSaveLoad)
                {
                    Debug.Log($"{logPrefix} 完成 OutputId={target.output.outputId}", this);
                }
            }
            finally
            {
                suppressCapture = false;
            }
        }

        // ===== 變更開始 =====
        // 2026/02/01 Opsidanos (修改原因：Restore 期間 suppressCapture 會跳過 OnStoryOutput，必須手動同步 currentPresentation)
        // 預期結果：Restore 後 currentPresentation 會與畫面狀態一致，避免下一句快照把舊狀態寫回去
        private void ApplyRestoredPresentationToCurrent(PresentationSnapshot restored)
        {
            currentPresentation.bgId = NormalizeValueOrFallback(restored.bgId, "clear");
            currentPresentation.bgmId = NormalizeValueOrFallback(restored.bgmId, "stop");
            currentPresentation.cgValue = NormalizeValueOrFallback(restored.cgValue, "clear");
            currentPresentation.charValue = NormalizeValueOrFallback(restored.charValue, "clear");
            currentPresentation.charLeftId = NormalizeValueOrFallback(restored.charLeftId, "clear");
            currentPresentation.charCenterId = NormalizeValueOrFallback(restored.charCenterId, "clear");
            currentPresentation.charRightId = NormalizeValueOrFallback(restored.charRightId, "clear");
        }
        // ===== 變更結束 =====

        private static InkSaveData BuildSaveData(StoryOutput output, string storyStateJson, PresentationSnapshot presentation)
        {
            return new InkSaveData
            {
                version = 1,
                inkStateJson = storyStateJson,
                output = BuildOutputSnapshot(output),
                presentation = ClonePresentation(presentation)
            };
        }

        private static StoryOutputSnapshot BuildOutputSnapshot(StoryOutput output)
        {
            ChoiceSnapshot[] choices = BuildChoiceSnapshots(output.Choices);

            return new StoryOutputSnapshot
            {
                outputId = output.OutputId,
                speaker = output.Speaker,
                lineText = output.LineText,
                hasEnded = output.HasEnded,
                choices = choices
            };
        }

        private static ChoiceSnapshot[] BuildChoiceSnapshots(List<ChoiceOutput> choices)
        {
            if (choices == null || choices.Count == 0)
            {
                return Array.Empty<ChoiceSnapshot>();
            }

            var snapshots = new ChoiceSnapshot[choices.Count];
            for (int i = 0; i < choices.Count; i++)
            {
                ChoiceOutput choice = choices[i];
                snapshots[i] = choice == null
                    ? null
                    : new ChoiceSnapshot
                    {
                        index = choice.Index,
                        text = choice.Text
                    };
            }

            return snapshots;
        }

        private static StoryOutput BuildStoryOutput(StoryOutputSnapshot output, PresentationSnapshot presentation)
        {
            List<string> rawTags = new List<string>();
            List<InkTag> parsedTags = new List<InkTag>();

            AddTag(rawTags, parsedTags, "bg", NormalizeValueOrFallback(presentation.bgId, "clear"));
            AddTag(rawTags, parsedTags, "bgm", NormalizeValueOrFallback(presentation.bgmId, "stop"));
            AddTag(rawTags, parsedTags, "cg", NormalizeValueOrFallback(presentation.cgValue, "clear"));
            AddTag(rawTags, parsedTags, "char", NormalizeValueOrFallback(presentation.charValue, "clear"));
            AddTag(rawTags, parsedTags, "char-left", NormalizeValueOrFallback(presentation.charLeftId, "clear"));
            AddTag(rawTags, parsedTags, "char-center", NormalizeValueOrFallback(presentation.charCenterId, "clear"));
            AddTag(rawTags, parsedTags, "char-right", NormalizeValueOrFallback(presentation.charRightId, "clear"));

            List<ChoiceOutput> choices = new List<ChoiceOutput>();
            if (output.choices != null)
            {
                for (int i = 0; i < output.choices.Length; i++)
                {
                    ChoiceSnapshot choice = output.choices[i];
                    if (choice == null)
                    {
                        continue;
                    }

                    choices.Add(new ChoiceOutput(choice.index, choice.text));
                }
            }

            // ===== 變更開始 =====
            // 2026/02/01 Opsidanos (修改原因：倒退/讀檔的外部輸出需要標記來源，讓 char 演出能套用相反層級規則)
            // 預期結果：Restore 模式下，先做的動作會在上面
            return new StoryOutput(output.outputId, output.speaker, output.lineText, output.hasEnded, rawTags, parsedTags, choices, StoryOutputSource.Restore);
            // ===== 變更結束 =====
        }

        private void UpdatePresentationFromTags(List<InkTag> tags)
        {
            if (tags == null || tags.Count == 0)
            {
                return;
            }

            for (int i = 0; i < tags.Count; i++)
            {
                InkTag tag = tags[i];
                if (tag == null || string.IsNullOrWhiteSpace(tag.Key))
                {
                    continue;
                }

                string key = tag.Key;
                string value = tag.HasValue ? tag.Value : null;

                switch (key)
                {
                    case "bg":
                        currentPresentation.bgId = NormalizeValueOrFallback(value, "clear");
                        break;
                    case "bgm":
                        currentPresentation.bgmId = NormalizeValueOrFallback(value, "stop");
                        break;
                    case "cg":
                        currentPresentation.cgValue = NormalizeValueOrFallback(value, "clear");
                        break;
                    case "char":
                        currentPresentation.charValue = NormalizeValueOrFallback(value, "clear");
                        currentPresentation.charLeftId = "clear";
                        currentPresentation.charCenterId = "clear";
                        currentPresentation.charRightId = "clear";
                        break;
                    case "char-left":
                        currentPresentation.charValue = "clear";
                        currentPresentation.charLeftId = NormalizeValueOrFallback(value, "clear");
                        break;
                    case "char-center":
                        currentPresentation.charValue = "clear";
                        currentPresentation.charCenterId = NormalizeValueOrFallback(value, "clear");
                        break;
                    case "char-right":
                        currentPresentation.charValue = "clear";
                        currentPresentation.charRightId = NormalizeValueOrFallback(value, "clear");
                        break;
                }
            }
        }

        private void PushRollback(InkSaveData snapshot)
        {
            rollbackBuffer.Add(snapshot);

            if (rollbackBuffer.Count <= rollbackCapacity)
            {
                return;
            }

            int overCount = rollbackBuffer.Count - rollbackCapacity;
            rollbackBuffer.RemoveRange(0, overCount);
        }

        private bool TryGetLatestSnapshot(out InkSaveData snapshot)
        {
            if (rollbackBuffer.Count == 0)
            {
                snapshot = null;
                return false;
            }

            snapshot = rollbackBuffer[rollbackBuffer.Count - 1];
            return snapshot != null;
        }

        private static InkSaveData CloneSaveData(InkSaveData source)
        {
            if (source == null)
            {
                return null;
            }

            return new InkSaveData
            {
                version = source.version,
                inkStateJson = source.inkStateJson,
                output = CloneOutput(source.output),
                presentation = ClonePresentation(source.presentation)
            };
        }

        private static StoryOutputSnapshot CloneOutput(StoryOutputSnapshot output)
        {
            if (output == null)
            {
                return null;
            }

            ChoiceSnapshot[] clonedChoices;
            if (output.choices == null || output.choices.Length == 0)
            {
                clonedChoices = Array.Empty<ChoiceSnapshot>();
            }
            else
            {
                clonedChoices = new ChoiceSnapshot[output.choices.Length];
                for (int i = 0; i < output.choices.Length; i++)
                {
                    ChoiceSnapshot choice = output.choices[i];
                    clonedChoices[i] = choice == null ? null : new ChoiceSnapshot { index = choice.index, text = choice.text };
                }
            }

            return new StoryOutputSnapshot
            {
                outputId = output.outputId,
                speaker = output.speaker,
                lineText = output.lineText,
                hasEnded = output.hasEnded,
                choices = clonedChoices
            };
        }

        private static PresentationSnapshot ClonePresentation(PresentationSnapshot source)
        {
            if (source == null)
            {
                return null;
            }

            return new PresentationSnapshot
            {
                bgId = source.bgId,
                bgmId = source.bgmId,
                cgValue = source.cgValue,
                charValue = source.charValue,
                charLeftId = source.charLeftId,
                charCenterId = source.charCenterId,
                charRightId = source.charRightId
            };
        }

        private static string NormalizeValueOrFallback(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static void AddTag(List<string> rawTags, List<InkTag> parsedTags, string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            if (value == null)
            {
                rawTags.Add(key);
                parsedTags.Add(new InkTag(key, null));
                return;
            }

            rawTags.Add($"{key}:{value}");
            parsedTags.Add(new InkTag(key, value));
        }
    }
}
// ===== 變更結束 =====
