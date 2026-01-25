// ===== 變更開始 =====
// 2026/01/21 Opsidanos (修改原因：建立玩家模式最小 UI Toolkit Presenter)
// 預期結果：收到 StoryOutput 後更新 UI，並能點選「下一句」與選項推進故事；顯示/隱藏只切換 class
// ===== 變更開始 =====
// 2026/01/22 Opsidanos (修改原因：補齊玩家模式 MVP 操作：回看/Auto/Skip/隱藏 UI，並避免 Start 順序造成第一次輸出不顯示)
// 預期結果：玩家能在 Runtime 操作回看/自動/快轉/隱藏 UI；Play 後第一句就能正常顯示
// ===== 變更開始 =====
// 2026/01/25 Opsidanos (修改原因：修正快速連點造成「上一句演出未結束就進下一句」的狀態錯亂；加入強制完成與點擊冷卻)
// 預期結果：動畫/打字機未完成時點擊只會強制刷新到終點，且有 0.3 秒冷卻避免立刻跳下一句；完成後再點才推進
using System.Collections;
using System.Collections.Generic;
using OpsidanosInk.Runtime.Story;
using OpsidanosInk.Runtime.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpsidanosInk.Runtime.UI
{
    public sealed class VNPlayerPresenter : MonoBehaviour
    {
        private sealed class BacklogEntry
        {
            public int OutputId { get; }
            public string Speaker { get; }
            public string LineText { get; }

            public BacklogEntry(int outputId, string speaker, string lineText)
            {
                OutputId = outputId;
                Speaker = speaker;
                LineText = lineText;
            }
        }

        [Header("Refs")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private InkStoryEngine storyEngine;

        [Header("Auto / Skip")]
        [SerializeField] private float autoDelaySeconds = 1.2f;
        [SerializeField] private float skipIntervalSeconds = 0.05f;

        // ===== 變更開始 =====
        // 2026/01/25 Opsidanos (修改原因：支援快速連點：Busy 時先 ForceComplete，並加入點擊冷卻避免直接跳過)
        // 預期結果：點一下只會把當句演出跑到最後；0.3 秒內再點不會推進；超過後再點才推進下一句
        [Header("Advance Control")]
        [SerializeField] private float forceCompleteClickCooldownSeconds = 0.3f;
        [SerializeField] private List<MonoBehaviour> advanceBlockers = new List<MonoBehaviour>();
        // ===== 變更結束 =====

        private VisualElement vnRoot;
        private VisualElement backlogPanel;
        private ScrollView backlogScrollView;

        private Label speakerLabel;
        private Label bodyLabel;
        private Button continueButton;
        private VisualElement choicesContainer;

        private Button backlogButton;
        private Button backlogCloseButton;
        private Button autoButton;
        private Button skipButton;
        private Button hideButton;
        private Button showUIButton;

        private readonly List<BacklogEntry> backlogEntries = new List<BacklogEntry>();

        private StoryOutput lastOutput;
        private bool isBacklogOpen;
        private bool isAutoEnabled;
        private bool isSkipEnabled;
        private bool isUiHidden;
        private Coroutine autoSkipCoroutine;
        // ===== 變更開始 =====
        // 2026/01/25 Opsidanos (修改原因：記錄點擊冷卻時間與可用的 blocker 介面)
        // 預期結果：強制完成後，短時間內點擊不會直接推進，避免快速連點跳過內容
        private float clickCooldownUntilUnscaled;
        private readonly List<IAdvanceBlocker> resolvedAdvanceBlockers = new List<IAdvanceBlocker>();
        // ===== 變更結束 =====

        private void Awake()
        {
            if (uiDocument == null)
            {
                Debug.LogError("[OpsidanosInk] VNPlayerPresenter 尚未指定 Ui Document。", this);
                enabled = false;
                return;
            }

            if (storyEngine == null)
            {
                Debug.LogError("[OpsidanosInk] VNPlayerPresenter 尚未指定 Story Engine。", this);
                enabled = false;
                return;
            }

            VisualElement root = uiDocument.rootVisualElement;

            vnRoot = root.Q<VisualElement>("VNRoot");
            speakerLabel = root.Q<Label>("SpeakerLabel");
            bodyLabel = root.Q<Label>("BodyLabel");
            continueButton = root.Q<Button>("ContinueButton");
            choicesContainer = root.Q<VisualElement>("ChoicesContainer");

            backlogPanel = root.Q<VisualElement>("BacklogPanel");
            backlogScrollView = root.Q<ScrollView>("BacklogScrollView");
            backlogButton = root.Q<Button>("BacklogButton");
            backlogCloseButton = root.Q<Button>("BacklogCloseButton");
            autoButton = root.Q<Button>("AutoButton");
            skipButton = root.Q<Button>("SkipButton");
            hideButton = root.Q<Button>("HideButton");
            showUIButton = root.Q<Button>("ShowUIButton");

            if (vnRoot == null ||
                speakerLabel == null ||
                bodyLabel == null ||
                continueButton == null ||
                choicesContainer == null ||
                backlogPanel == null ||
                backlogScrollView == null ||
                backlogButton == null ||
                backlogCloseButton == null ||
                autoButton == null ||
                skipButton == null ||
                hideButton == null ||
                showUIButton == null)
            {
                Debug.LogError("[OpsidanosInk] VNPlayer.uxml 缺少必要的元素。", this);
                enabled = false;
                return;
            }

            continueButton.clicked += OnClickContinue;
            backlogButton.clicked += OnClickToggleBacklog;
            backlogCloseButton.clicked += OnClickCloseBacklog;
            autoButton.clicked += OnClickToggleAuto;
            skipButton.clicked += OnClickToggleSkip;
            hideButton.clicked += OnClickHideUI;
            showUIButton.clicked += OnClickShowUI;

            // ===== 變更開始 =====
            // 2026/01/25 Opsidanos (修改原因：把 Inspector 指定的 MonoBehaviour 解析成 IAdvanceBlocker，避免執行時才發現型別不對)
            // 預期結果：若設定錯誤會明確印 Error；正確時 Presenter 能統一處理 Busy/ForceComplete
            ResolveAdvanceBlockers();
            // ===== 變更結束 =====

            RefreshToggleButtons();
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

            StopAutoSkipCoroutine();
        }

        private void OnDestroy()
        {
            if (continueButton != null)
            {
                continueButton.clicked -= OnClickContinue;
            }

            if (backlogButton != null)
            {
                backlogButton.clicked -= OnClickToggleBacklog;
            }

            if (backlogCloseButton != null)
            {
                backlogCloseButton.clicked -= OnClickCloseBacklog;
            }

            if (autoButton != null)
            {
                autoButton.clicked -= OnClickToggleAuto;
            }

            if (skipButton != null)
            {
                skipButton.clicked -= OnClickToggleSkip;
            }

            if (hideButton != null)
            {
                hideButton.clicked -= OnClickHideUI;
            }

            if (showUIButton != null)
            {
                showUIButton.clicked -= OnClickShowUI;
            }
        }

        private void OnClickContinue()
        {
            // ===== 變更開始 =====
            // 2026/01/25 Opsidanos (修改原因：快速連點時先強制完成演出，再允許推進；並加入點擊冷卻避免立刻跳過)
            // 預期結果：Busy 時點擊只會 ForceComplete；冷卻結束後再點才 Continue
            if (IsClickInCooldown())
            {
                return;
            }

            if (TryForceCompleteIfBusy(isClick: true))
            {
                return;
            }

            storyEngine.Continue();
            // ===== 變更結束 =====
        }

        // ===== 變更開始 =====
        // 2026/01/25 Opsidanos (修改原因：選項點擊也要遵守同一套「Busy → ForceComplete → 冷卻」規則)
        // 預期結果：快速連點不會讓選項直接跳過上一句演出，避免狀態錯亂
        private void OnClickChoice(int choiceIndex)
        {
            if (IsClickInCooldown())
            {
                return;
            }

            if (TryForceCompleteIfBusy(isClick: true))
            {
                return;
            }

            storyEngine.ChooseChoice(choiceIndex);
        }
        // ===== 變更結束 =====

        private void OnClickToggleBacklog()
        {
            if (isBacklogOpen)
            {
                CloseBacklog();
                return;
            }

            OpenBacklog();
        }

        private void OnClickCloseBacklog()
        {
            CloseBacklog();
        }

        private void OnClickToggleAuto()
        {
            if (isAutoEnabled)
            {
                isAutoEnabled = false;
                RefreshToggleButtons();
                return;
            }

            isSkipEnabled = false;
            isAutoEnabled = true;
            RefreshToggleButtons();
            StartAutoSkipIfNeeded();
        }

        private void OnClickToggleSkip()
        {
            if (isSkipEnabled)
            {
                isSkipEnabled = false;
                RefreshToggleButtons();
                return;
            }

            isAutoEnabled = false;
            isSkipEnabled = true;
            RefreshToggleButtons();
            StartAutoSkipIfNeeded();
        }

        private void OnClickHideUI()
        {
            SetUiHidden(true);
        }

        private void OnClickShowUI()
        {
            SetUiHidden(false);
        }

        private void OnStoryOutput(StoryOutput output)
        {
            lastOutput = output;

            if (speakerLabel != null)
            {
                speakerLabel.text = string.IsNullOrWhiteSpace(output.Speaker) ? "" : output.Speaker;
            }

            if (bodyLabel != null)
            {
                bodyLabel.text = output.LineText ?? "";
            }

            if (output.HasEnded || output.Choices.Count > 0)
            {
                isAutoEnabled = false;
                isSkipEnabled = false;
                RefreshToggleButtons();
            }

            choicesContainer.Clear();

            if (output.Choices.Count > 0)
            {
                continueButton.AddToClassList("vn-hidden");
                choicesContainer.RemoveFromClassList("vn-hidden");

                for (int i = 0; i < output.Choices.Count; i++)
                {
                    ChoiceOutput choice = output.Choices[i];
                    // ===== 變更開始 =====
                    // 2026/01/25 Opsidanos (修改原因：選項按鈕改走 OnClickChoice，統一處理 Busy/冷卻)
                    // 預期結果：選項點擊也不會因為快速連點而跳過演出
                    var button = new Button(() => OnClickChoice(choice.Index))
                    {
                        text = choice.Text
                    };
                    // ===== 變更結束 =====
                    button.AddToClassList("vn-choice-button");
                    choicesContainer.Add(button);
                }
            }
            else
            {
                choicesContainer.AddToClassList("vn-hidden");

                if (output.HasEnded)
                {
                    continueButton.AddToClassList("vn-hidden");
                    return;
                }

                continueButton.RemoveFromClassList("vn-hidden");
            }

            TryAppendBacklog(output);
        }

        private void TryAppendBacklog(StoryOutput output)
        {
            if (string.IsNullOrWhiteSpace(output.LineText))
            {
                return;
            }

            var entry = new BacklogEntry(output.OutputId, output.Speaker, output.LineText);
            backlogEntries.Add(entry);

            string speakerText = string.IsNullOrWhiteSpace(entry.Speaker) ? "" : $"{entry.Speaker}：";
            var label = new Label($"{speakerText}{entry.LineText}");
            label.AddToClassList("vn-backlog-entry");
            backlogScrollView.contentContainer.Add(label);

            if (isBacklogOpen)
            {
                backlogScrollView.ScrollTo(label);
            }
        }

        private void OpenBacklog()
        {
            isAutoEnabled = false;
            isSkipEnabled = false;
            RefreshToggleButtons();

            isBacklogOpen = true;
            backlogPanel.RemoveFromClassList("vn-hidden");
            backlogButton.AddToClassList("vn-toggle-on");

            if (backlogScrollView.contentContainer.childCount > 0)
            {
                VisualElement lastItem = backlogScrollView.contentContainer[backlogScrollView.contentContainer.childCount - 1];
                backlogScrollView.ScrollTo(lastItem);
            }
        }

        private void CloseBacklog()
        {
            isBacklogOpen = false;
            backlogPanel.AddToClassList("vn-hidden");
            backlogButton.RemoveFromClassList("vn-toggle-on");
        }

        private void RefreshToggleButtons()
        {
            autoButton.text = isAutoEnabled ? "自動：開" : "自動：關";
            skipButton.text = isSkipEnabled ? "快轉：開" : "快轉：關";

            if (isAutoEnabled)
            {
                autoButton.AddToClassList("vn-toggle-on");
            }
            else
            {
                autoButton.RemoveFromClassList("vn-toggle-on");
            }

            if (isSkipEnabled)
            {
                skipButton.AddToClassList("vn-toggle-on");
            }
            else
            {
                skipButton.RemoveFromClassList("vn-toggle-on");
            }
        }

        private void SetUiHidden(bool hidden)
        {
            if (hidden == isUiHidden)
            {
                return;
            }

            isUiHidden = hidden;

            if (hidden)
            {
                isAutoEnabled = false;
                isSkipEnabled = false;
                RefreshToggleButtons();
                CloseBacklog();
                vnRoot.AddToClassList("vn-ui-hidden");
                return;
            }

            vnRoot.RemoveFromClassList("vn-ui-hidden");
        }

        private void StartAutoSkipIfNeeded()
        {
            if (autoSkipCoroutine != null)
            {
                return;
            }

            if (!isAutoEnabled && !isSkipEnabled)
            {
                return;
            }

            autoSkipCoroutine = StartCoroutine(AutoSkipCoroutine());
        }

        private void StopAutoSkipCoroutine()
        {
            if (autoSkipCoroutine == null)
            {
                return;
            }

            StopCoroutine(autoSkipCoroutine);
            autoSkipCoroutine = null;
        }

        private IEnumerator AutoSkipCoroutine()
        {
            while (isAutoEnabled || isSkipEnabled)
            {
                if (isBacklogOpen || isUiHidden)
                {
                    yield return null;
                    continue;
                }

                float waitSeconds = isSkipEnabled ? skipIntervalSeconds : autoDelaySeconds;
                if (waitSeconds > 0f)
                {
                    yield return new WaitForSeconds(waitSeconds);
                }
                else
                {
                    yield return null;
                }

                if (!(isAutoEnabled || isSkipEnabled))
                {
                    break;
                }

                if (lastOutput == null)
                {
                    continue;
                }

                if (lastOutput.HasEnded || lastOutput.Choices.Count > 0)
                {
                    isAutoEnabled = false;
                    isSkipEnabled = false;
                    RefreshToggleButtons();
                    break;
                }

                // ===== 變更開始 =====
                // 2026/01/25 Opsidanos (修改原因：Auto/Skip 推進也需要先處理 Busy，避免演出尚未結束就推進造成錯亂)
                // 預期結果：Busy 時先 ForceComplete；下一次迴圈再推進
                if (!TryForceCompleteIfBusy(isClick: false))
                {
                    storyEngine.Continue();
                }
                // ===== 變更結束 =====
            }

            autoSkipCoroutine = null;
        }

        // ===== 變更開始 =====
        // 2026/01/25 Opsidanos (修改原因：集中管理 Click 冷卻與 Busy 判斷，避免到處複製邏輯)
        // 預期結果：規則一致：Busy → ForceComplete；若是點擊觸發 ForceComplete，會進入短暫冷卻
        private bool IsClickInCooldown()
        {
            if (forceCompleteClickCooldownSeconds <= 0f)
            {
                return false;
            }

            return Time.unscaledTime < clickCooldownUntilUnscaled;
        }

        private bool TryForceCompleteIfBusy(bool isClick)
        {
            if (resolvedAdvanceBlockers.Count == 0)
            {
                return false;
            }

            bool anyBusy = false;

            for (int i = 0; i < resolvedAdvanceBlockers.Count; i++)
            {
                IAdvanceBlocker blocker = resolvedAdvanceBlockers[i];
                if (blocker == null)
                {
                    continue;
                }

                if (blocker.IsBusy)
                {
                    anyBusy = true;
                }
            }

            if (!anyBusy)
            {
                return false;
            }

            for (int i = 0; i < resolvedAdvanceBlockers.Count; i++)
            {
                IAdvanceBlocker blocker = resolvedAdvanceBlockers[i];
                if (blocker == null)
                {
                    continue;
                }

                if (blocker.IsBusy)
                {
                    blocker.ForceComplete();
                }
            }

            if (isClick && forceCompleteClickCooldownSeconds > 0f)
            {
                clickCooldownUntilUnscaled = Time.unscaledTime + forceCompleteClickCooldownSeconds;
            }

            return true;
        }

        private void ResolveAdvanceBlockers()
        {
            resolvedAdvanceBlockers.Clear();

            if (advanceBlockers == null || advanceBlockers.Count == 0)
            {
                return;
            }

            for (int i = 0; i < advanceBlockers.Count; i++)
            {
                MonoBehaviour behaviour = advanceBlockers[i];
                if (behaviour == null)
                {
                    Debug.LogError("[OpsidanosInk] VNPlayerPresenter 的 advanceBlockers 出現空值，請檢查 Inspector。", this);
                    continue;
                }

                if (behaviour is IAdvanceBlocker blocker)
                {
                    resolvedAdvanceBlockers.Add(blocker);
                    continue;
                }

                Debug.LogError($"[OpsidanosInk] VNPlayerPresenter 的 advanceBlockers 只接受 IAdvanceBlocker，但你放入的是：{behaviour.GetType().Name}", this);
            }
        }
        // ===== 變更結束 =====
    }
}
// ===== 變更結束 =====
// ===== 變更結束 =====
// ===== 變更結束 =====
