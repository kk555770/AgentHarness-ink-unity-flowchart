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
        // ===== 變更開始 =====
        // 2026/02/06 Opsidanos (修改原因：Phase 3-1 需要支援多槽存讀 + Auto 槽，不能再只用單一 saveSlot)
        // 預期結果：手動 3 槽與 Auto 槽可獨立存讀，且保留既有 SaveToSlot/LoadFromSlot 相容行為
        private const int ManualSlotCount = 3;
        private readonly InkSaveSlotData[] manualSlots = new InkSaveSlotData[ManualSlotCount];
        private InkSaveSlotData autoSlot;
        // ===== 變更結束 =====

        // ===== 變更開始 =====
        // 2026/02/06 Opsidanos (修改原因：提供 UI/流程可判斷的倒帶能力資訊，避免倒帶到底時一直噴錯)
        // 預期結果：上層可先判斷是否可倒帶、還剩幾步，並可做排隊控制
        public int AvailableRollbackSteps => Mathf.Max(rollbackBuffer.Count - 1, 0);
        public bool CanRollback => rollbackBuffer.Count > 1;
        // ===== 變更結束 =====

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
            // ===== 變更開始 =====
            // 2026/02/06 Opsidanos (修改原因：保留舊 API，相容既有按鈕與流程)
            // 預期結果：舊 SaveToSlot 仍可使用，行為等同存到手動槽位 1
            SaveToManualSlot(1);
            // ===== 變更結束 =====
        }

        // ===== 變更開始 =====
        // 2026/02/06 Opsidanos (修改原因：新增手動多槽存檔 API)
        // 預期結果：可指定 1~3 槽位存檔，槽位資料互不覆蓋
        public void SaveToManualSlot(int slotNumber)
        {
            if (!TryGetLatestSnapshot(out InkSaveData latestSnapshot))
            {
                Debug.LogError("[OpsidanosInk] InkSaveSystem 無法存檔：目前還沒有任何可存的輸出。", this);
                return;
            }

            if (!TryNormalizeManualSlotIndex(slotNumber, out int manualSlotIndex))
            {
                return;
            }

            manualSlots[manualSlotIndex] = CreateCurrentSlotData();

            if (logSaveLoad)
            {
                int outputId = latestSnapshot.output != null ? latestSnapshot.output.outputId : -1;
                Debug.Log($"[OpsidanosInk][Save][ManualSlot{slotNumber}] 存檔完成 OutputId={outputId}", this);
            }
        }
        // ===== 變更結束 =====

        public void LoadFromSlot()
        {
            // ===== 變更開始 =====
            // 2026/02/06 Opsidanos (修改原因：保留舊 API，相容既有按鈕與流程)
            // 預期結果：舊 LoadFromSlot 仍可使用，行為等同讀取手動槽位 1
            LoadFromManualSlot(1);
            // ===== 變更結束 =====
        }

        // ===== 變更開始 =====
        // 2026/02/06 Opsidanos (修改原因：新增手動多槽讀檔 API)
        // 預期結果：可指定 1~3 槽位讀檔，且讀檔後仍可倒帶到存檔點之前
        public void LoadFromManualSlot(int slotNumber)
        {
            if (!TryNormalizeManualSlotIndex(slotNumber, out int manualSlotIndex))
            {
                return;
            }

            InkSaveSlotData targetSlot = manualSlots[manualSlotIndex];
            if (targetSlot == null)
            {
                Debug.LogError($"[OpsidanosInk] InkSaveSystem 無法讀檔：手動槽位 {slotNumber} 目前沒有存檔資料。", this);
                return;
            }

            if (!TryRestoreRollbackHistoryFromSlot(targetSlot, out InkSaveData target))
            {
                return;
            }

            Restore(target, resetRollbackBuffer: false, logPrefix: $"[OpsidanosInk][Load][ManualSlot{slotNumber}]");
        }
        // ===== 變更結束 =====

        // ===== 變更開始 =====
        // 2026/02/06 Opsidanos (修改原因：新增 Auto 槽存讀 API)
        // 預期結果：Auto 槽可獨立存讀，不覆蓋手動槽資料
        public void SaveToAutoSlot()
        {
            if (!TryGetLatestSnapshot(out InkSaveData latestSnapshot))
            {
                Debug.LogError("[OpsidanosInk] InkSaveSystem 無法自動存檔：目前還沒有任何可存的輸出。", this);
                return;
            }

            autoSlot = CreateCurrentSlotData();

            if (logSaveLoad)
            {
                int outputId = latestSnapshot.output != null ? latestSnapshot.output.outputId : -1;
                Debug.Log($"[OpsidanosInk][Save][AutoSlot] 存檔完成 OutputId={outputId}", this);
            }
        }

        public void LoadFromAutoSlot()
        {
            if (autoSlot == null)
            {
                Debug.LogError("[OpsidanosInk] InkSaveSystem 無法讀取自動存檔：Auto 槽位目前沒有資料。", this);
                return;
            }

            if (!TryRestoreRollbackHistoryFromSlot(autoSlot, out InkSaveData target))
            {
                return;
            }

            Restore(target, resetRollbackBuffer: false, logPrefix: "[OpsidanosInk][Load][AutoSlot]");
        }
        // ===== 變更結束 =====

        public void RollbackOnce()
        {
            if (TryRollbackOnce())
            {
                return;
            }

            Debug.LogError("[OpsidanosInk] InkSaveSystem 無法倒帶：rollbackBuffer 不足（至少需要 2 份快照）。", this);
        }

        // ===== 變更開始 =====
        // 2026/02/06 Opsidanos (修改原因：提供不噴紅字的倒帶 API，給 UI 佇列使用)
        // 預期結果：快速連按倒帶時可穩定排隊，且到最前句時不會一直刷 Error
        public bool TryRollbackOnce()
        {
            if (rollbackBuffer.Count <= 1)
            {
                return false;
            }

            rollbackBuffer.RemoveAt(rollbackBuffer.Count - 1);
            InkSaveData target = rollbackBuffer[rollbackBuffer.Count - 1];
            Restore(target, resetRollbackBuffer: false, logPrefix: "[OpsidanosInk][Rollback]");
            return true;
        }
        // ===== 變更結束 =====

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

        // ===== 變更開始 =====
        // 2026/02/06 Opsidanos (修改原因：存檔/讀檔要能保留並還原整條 rollback 歷史)
        // 預期結果：讀檔後 rollbackBuffer 會回到存檔那一刻的狀態，並可繼續向前倒帶
        private static InkSaveData[] CloneRollbackHistory(List<InkSaveData> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<InkSaveData>();
            }

            var snapshots = new InkSaveData[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                snapshots[i] = CloneSaveData(source[i]);
            }

            return snapshots;
        }

        // ===== 變更開始 =====
        // 2026/02/06 Opsidanos (修改原因：多槽存讀需要共用槽位資料建構與槽位索引驗證)
        // 預期結果：手動槽位編號驗證一致，並統一產生可讀回倒帶歷史的槽位資料
        private InkSaveSlotData CreateCurrentSlotData()
        {
            return new InkSaveSlotData
            {
                version = 1,
                activeRollbackIndex = rollbackBuffer.Count - 1,
                rollbackHistory = CloneRollbackHistory(rollbackBuffer)
            };
        }

        private bool TryNormalizeManualSlotIndex(int slotNumber, out int manualSlotIndex)
        {
            manualSlotIndex = slotNumber - 1;
            if (manualSlotIndex >= 0 && manualSlotIndex < ManualSlotCount)
            {
                return true;
            }

            Debug.LogError($"[OpsidanosInk] InkSaveSystem 無效的手動槽位：{slotNumber}。允許範圍為 1 ~ {ManualSlotCount}。", this);
            return false;
        }
        // ===== 變更結束 =====

        private bool TryRestoreRollbackHistoryFromSlot(InkSaveSlotData slot, out InkSaveData target)
        {
            target = null;
            if (slot == null)
            {
                Debug.LogError("[OpsidanosInk] InkSaveSystem 無法讀檔：存檔槽位為 null。", this);
                return false;
            }

            if (slot.rollbackHistory == null || slot.rollbackHistory.Length == 0)
            {
                Debug.LogError("[OpsidanosInk] InkSaveSystem 無法讀檔：rollbackHistory 為空。", this);
                return false;
            }

            rollbackBuffer.Clear();
            for (int i = 0; i < slot.rollbackHistory.Length; i++)
            {
                InkSaveData snapshot = CloneSaveData(slot.rollbackHistory[i]);
                if (snapshot == null)
                {
                    continue;
                }

                rollbackBuffer.Add(snapshot);
            }

            if (rollbackBuffer.Count == 0)
            {
                Debug.LogError("[OpsidanosInk] InkSaveSystem 無法讀檔：rollbackHistory 全部為無效快照。", this);
                return false;
            }

            if (rollbackBuffer.Count > rollbackCapacity)
            {
                int overCount = rollbackBuffer.Count - rollbackCapacity;
                rollbackBuffer.RemoveRange(0, overCount);
            }

            int activeIndex = Mathf.Clamp(slot.activeRollbackIndex, 0, rollbackBuffer.Count - 1);
            if (activeIndex < rollbackBuffer.Count - 1)
            {
                rollbackBuffer.RemoveRange(activeIndex + 1, rollbackBuffer.Count - activeIndex - 1);
            }

            target = rollbackBuffer[rollbackBuffer.Count - 1];
            if (target != null)
            {
                return true;
            }

            Debug.LogError("[OpsidanosInk] InkSaveSystem 無法讀檔：activeRollbackIndex 指向的快照為 null。", this);
            return false;
        }
        // ===== 變更結束 =====

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
