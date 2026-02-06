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
using OpsidanosInk.Runtime.Save;
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
        // ===== 變更開始 =====
        // 2026/01/30 Opsidanos (修改原因：接上存檔/讀檔/倒帶系統，讓 UI 按鈕能呼叫)
        // 預期結果：點擊「存檔/讀檔/倒帶」時能正確呼叫 InkSaveSystem
        [SerializeField] private InkSaveSystem saveSystem;
        // ===== 變更結束 =====

        [Header("Auto / Skip")]
        // ===== 變更開始 =====
        // 2026/01/26 Opsidanos (修改原因：Auto 規則改成「打字機+動畫完成後再等 1 秒」才推進，且點擊期間會作廢這次 Auto)
        // 預期結果：Auto 不再因為固定間隔或點擊冷卻而跳過內容；玩家點擊會接管推進，不會被 Auto 偷偷再推一次
        [SerializeField] private float autoDelaySeconds = 1f;
        // ===== 變更結束 =====
        [SerializeField] private float skipIntervalSeconds = 0.05f;

        // ===== 變更開始 =====
        // 2026/01/26 Opsidanos (修改原因：加入打字機效果與可調參數)
        // 預期結果：對話文字逐字顯示；點擊 ForceComplete 時可立刻顯示完整文字
        [Header("Typewriter")]
        [SerializeField] private bool typewriterEnabled = true;
        [SerializeField] private float typewriterCharsPerSecond = 45f;
        // ===== 變更結束 =====

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
        // ===== 變更開始 =====
        // 2026/01/30 Opsidanos (修改原因：新增存檔/讀檔/倒帶按鈕)
        // 預期結果：VNPlayer.uxml 有對應按鈕時，Presenter 能找到並綁定事件
        private Button saveButton;
        private Button loadButton;
        private Button rollbackButton;
        // ===== 變更開始 =====
        // 2026/02/06 Opsidanos (修改原因：Phase 3-1 新增手動 1~3 槽與 Auto 槽按鈕)
        // 預期結果：Presenter 能綁定並處理所有新槽位按鈕點擊
        private Button saveSlot1Button;
        private Button loadSlot1Button;
        private Button saveSlot2Button;
        private Button loadSlot2Button;
        private Button saveSlot3Button;
        private Button loadSlot3Button;
        private Button saveAutoSlotButton;
        private Button loadAutoSlotButton;
        // ===== 變更結束 =====
        // ===== 變更結束 =====
        private Button showUIButton;

        private readonly List<BacklogEntry> backlogEntries = new List<BacklogEntry>();

        private StoryOutput lastOutput;
        private bool isBacklogOpen;
        private bool isAutoEnabled;
        private bool isSkipEnabled;
        private bool isUiHidden;
        private Coroutine autoSkipCoroutine;
        // ===== 變更開始 =====
        // 2026/02/06 Opsidanos (修改原因：快速連按倒帶時，需要把多次點擊排隊，避免動作重疊)
        // 預期結果：Rollback 會一步一步執行，不會「還沒到終點就進下一步」
        private Coroutine rollbackCoroutine;
        private int pendingRollbackRequests;
        // ===== 變更結束 =====
        // ===== 變更開始 =====
        // 2026/01/25 Opsidanos (修改原因：記錄點擊冷卻時間與可用的 blocker 介面)
        // 預期結果：強制完成後，短時間內點擊不會直接推進，避免快速連點跳過內容
        private float clickCooldownUntilUnscaled;
        private readonly List<IAdvanceBlocker> resolvedAdvanceBlockers = new List<IAdvanceBlocker>();
        // ===== 變更結束 =====

        // ===== 變更開始 =====
        // 2026/01/26 Opsidanos (修改原因：打字機狀態 + Auto 1 秒等待狀態，並避免 Auto/點擊同一幀重複推進)
        // 預期結果：Busy 同時包含打字機與人物動畫；Auto 等待期間若點擊會作廢；不會因同一幀重複觸發而連跳
        private Coroutine typewriterRoutine;
        private int typewriterOutputId;
        private string typewriterFullText;
        private int typewriterVisibleCharCount;

        private bool isAutoAdvanceCountdownActive;
        private int autoAdvanceCountdownOutputId;
        private float autoAdvanceCountdownUntilUnscaled;
        private int autoAdvanceSuppressedOutputId = -1;

        private int lastAdvanceFrame = -1;
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
            // ===== 變更開始 =====
            // 2026/01/30 Opsidanos (修改原因：讀取新的存檔/讀檔/倒帶按鈕)
            // 預期結果：若 UXML 缺少按鈕會直接報錯，避免按鈕有顯示但功能失效
            saveButton = root.Q<Button>("SaveButton");
            loadButton = root.Q<Button>("LoadButton");
            rollbackButton = root.Q<Button>("RollbackButton");
            // ===== 變更開始 =====
            // 2026/02/06 Opsidanos (修改原因：讀取多槽與 Auto 槽按鈕)
            // 預期結果：若 UXML 缺少任一新按鈕，啟動時立即報錯，避免功能默默失效
            saveSlot1Button = root.Q<Button>("SaveSlot1Button");
            loadSlot1Button = root.Q<Button>("LoadSlot1Button");
            saveSlot2Button = root.Q<Button>("SaveSlot2Button");
            loadSlot2Button = root.Q<Button>("LoadSlot2Button");
            saveSlot3Button = root.Q<Button>("SaveSlot3Button");
            loadSlot3Button = root.Q<Button>("LoadSlot3Button");
            saveAutoSlotButton = root.Q<Button>("SaveAutoSlotButton");
            loadAutoSlotButton = root.Q<Button>("LoadAutoSlotButton");
            // ===== 變更結束 =====
            // ===== 變更結束 =====
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
                // ===== 變更開始 =====
                // 2026/01/30 Opsidanos (修改原因：檢查新按鈕是否存在)
                // 預期結果：避免 UXML 遺漏造成 NullReference
                saveButton == null ||
                loadButton == null ||
                rollbackButton == null ||
                saveSlot1Button == null ||
                loadSlot1Button == null ||
                saveSlot2Button == null ||
                loadSlot2Button == null ||
                saveSlot3Button == null ||
                loadSlot3Button == null ||
                saveAutoSlotButton == null ||
                loadAutoSlotButton == null ||
                // ===== 變更結束 =====
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
            // ===== 變更開始 =====
            // 2026/01/30 Opsidanos (修改原因：綁定存檔/讀檔/倒帶按鈕事件)
            // 預期結果：按鈕點下去會呼叫 InkSaveSystem
            saveButton.clicked += OnClickSave;
            loadButton.clicked += OnClickLoad;
            rollbackButton.clicked += OnClickRollback;
            // ===== 變更開始 =====
            // 2026/02/06 Opsidanos (修改原因：綁定多槽與 Auto 槽按鈕事件)
            // 預期結果：玩家可直接透過 UI 指定槽位存讀
            saveSlot1Button.clicked += OnClickSaveSlot1;
            loadSlot1Button.clicked += OnClickLoadSlot1;
            saveSlot2Button.clicked += OnClickSaveSlot2;
            loadSlot2Button.clicked += OnClickLoadSlot2;
            saveSlot3Button.clicked += OnClickSaveSlot3;
            loadSlot3Button.clicked += OnClickLoadSlot3;
            saveAutoSlotButton.clicked += OnClickSaveAutoSlot;
            loadAutoSlotButton.clicked += OnClickLoadAutoSlot;
            // ===== 變更結束 =====
            // ===== 變更結束 =====
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
            // ===== 變更開始 =====
            // 2026/02/06 Opsidanos (修改原因：物件停用時，倒帶佇列也要停止，避免殘留請求在重新啟用後誤觸發)
            // 預期結果：切場景/停用物件後，不會有舊的倒帶請求繼續執行
            StopRollbackCoroutine();
            // ===== 變更結束 =====
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

            // ===== 變更開始 =====
            // 2026/01/30 Opsidanos (修改原因：補上存檔/讀檔/倒帶按鈕的反註冊，避免重複綁定)
            // 預期結果：物件重建/切場景時不會留下重複 callback
            if (saveButton != null)
            {
                saveButton.clicked -= OnClickSave;
            }

            if (loadButton != null)
            {
                loadButton.clicked -= OnClickLoad;
            }

            if (rollbackButton != null)
            {
                rollbackButton.clicked -= OnClickRollback;
            }

            // ===== 變更開始 =====
            // 2026/02/06 Opsidanos (修改原因：補齊多槽與 Auto 槽按鈕反註冊，避免重複 callback)
            // 預期結果：重建物件或切場景後，不會遺留重複綁定
            if (saveSlot1Button != null)
            {
                saveSlot1Button.clicked -= OnClickSaveSlot1;
            }

            if (loadSlot1Button != null)
            {
                loadSlot1Button.clicked -= OnClickLoadSlot1;
            }

            if (saveSlot2Button != null)
            {
                saveSlot2Button.clicked -= OnClickSaveSlot2;
            }

            if (loadSlot2Button != null)
            {
                loadSlot2Button.clicked -= OnClickLoadSlot2;
            }

            if (saveSlot3Button != null)
            {
                saveSlot3Button.clicked -= OnClickSaveSlot3;
            }

            if (loadSlot3Button != null)
            {
                loadSlot3Button.clicked -= OnClickLoadSlot3;
            }

            if (saveAutoSlotButton != null)
            {
                saveAutoSlotButton.clicked -= OnClickSaveAutoSlot;
            }

            if (loadAutoSlotButton != null)
            {
                loadAutoSlotButton.clicked -= OnClickLoadAutoSlot;
            }
            // ===== 變更結束 =====
            // ===== 變更結束 =====

            if (showUIButton != null)
            {
                showUIButton.clicked -= OnClickShowUI;
            }
        }

        private void OnClickContinue()
        {
            // ===== 變更開始 =====
            // 2026/01/26 Opsidanos (修改原因：Auto 等待期間若點擊，這一次 Auto 直接作廢，改由點擊規則接管)
            // 預期結果：玩家點擊時不會被 Auto 偷偷再推一次；就算點擊被冷卻擋住，也會讓 Auto 停下來
            SuppressAutoAdvanceIfCountdownActive();
            // ===== 變更結束 =====

            // ===== 變更開始 =====
            // 2026/01/25 Opsidanos (修改原因：快速連點時先強制完成演出，再允許推進；並加入點擊冷卻避免立刻跳過)
            // 預期結果：Busy 時點擊只會 ForceComplete；冷卻結束後再點才 Continue
            if (IsClickInCooldown())
            {
                return;
            }

            if (TryForceCompleteIfBusy(isClick: true))
            {
                // ===== 變更開始 =====
                // 2026/01/26 Opsidanos (修改原因：Auto 開啟時，點擊 ForceComplete 後要立刻開始算 1 秒，若期間沒點擊才自動推進)
                // 預期結果：點一下補完後放著不動，Auto 會自然接著播放；若再點一下就交給點擊推進，不會被 Auto 再推一次
                StartAutoAdvanceCountdownIfPossible();
                // ===== 變更結束 =====
                return;
            }

            // ===== 變更開始 =====
            // 2026/01/26 Opsidanos (修改原因：點擊推進要重置 Auto 計時，並避免 Auto/點擊同一幀重複推進)
            // 預期結果：點擊進下一句後不會沿用上一句的 Auto 倒數；不會同一幀連跳兩句
            if (!TryMarkAdvancedThisFrame())
            {
                return;
            }

            ResetAutoAdvanceState();
            storyEngine.Continue();
            // ===== 變更結束 =====
            // ===== 變更結束 =====
        }

        // ===== 變更開始 =====
        // 2026/01/30 Opsidanos (修改原因：提供存檔/讀檔/倒帶按鈕行為)
        // 預期結果：玩家能在 UI 上直接存檔、讀檔、倒帶
        private void OnClickSave()
        {
            if (saveSystem == null)
            {
                Debug.LogError("[OpsidanosInk] VNPlayerPresenter 尚未指定 Save System（InkSaveSystem）。", this);
                return;
            }

            saveSystem.SaveToSlot();
        }

        private void OnClickLoad()
        {
            if (saveSystem == null)
            {
                Debug.LogError("[OpsidanosInk] VNPlayerPresenter 尚未指定 Save System（InkSaveSystem）。", this);
                return;
            }

            saveSystem.LoadFromSlot();
        }

        // ===== 變更開始 =====
        // 2026/02/06 Opsidanos (修改原因：新增多槽與 Auto 槽按鈕行為)
        // 預期結果：按鈕點擊可準確呼叫對應槽位的存讀 API
        private void OnClickSaveSlot1()
        {
            if (saveSystem == null)
            {
                Debug.LogError("[OpsidanosInk] VNPlayerPresenter 尚未指定 Save System（InkSaveSystem）。", this);
                return;
            }

            saveSystem.SaveToManualSlot(1);
        }

        private void OnClickLoadSlot1()
        {
            if (saveSystem == null)
            {
                Debug.LogError("[OpsidanosInk] VNPlayerPresenter 尚未指定 Save System（InkSaveSystem）。", this);
                return;
            }

            saveSystem.LoadFromManualSlot(1);
        }

        private void OnClickSaveSlot2()
        {
            if (saveSystem == null)
            {
                Debug.LogError("[OpsidanosInk] VNPlayerPresenter 尚未指定 Save System（InkSaveSystem）。", this);
                return;
            }

            saveSystem.SaveToManualSlot(2);
        }

        private void OnClickLoadSlot2()
        {
            if (saveSystem == null)
            {
                Debug.LogError("[OpsidanosInk] VNPlayerPresenter 尚未指定 Save System（InkSaveSystem）。", this);
                return;
            }

            saveSystem.LoadFromManualSlot(2);
        }

        private void OnClickSaveSlot3()
        {
            if (saveSystem == null)
            {
                Debug.LogError("[OpsidanosInk] VNPlayerPresenter 尚未指定 Save System（InkSaveSystem）。", this);
                return;
            }

            saveSystem.SaveToManualSlot(3);
        }

        private void OnClickLoadSlot3()
        {
            if (saveSystem == null)
            {
                Debug.LogError("[OpsidanosInk] VNPlayerPresenter 尚未指定 Save System（InkSaveSystem）。", this);
                return;
            }

            saveSystem.LoadFromManualSlot(3);
        }

        private void OnClickSaveAutoSlot()
        {
            if (saveSystem == null)
            {
                Debug.LogError("[OpsidanosInk] VNPlayerPresenter 尚未指定 Save System（InkSaveSystem）。", this);
                return;
            }

            saveSystem.SaveToAutoSlot();
        }

        private void OnClickLoadAutoSlot()
        {
            if (saveSystem == null)
            {
                Debug.LogError("[OpsidanosInk] VNPlayerPresenter 尚未指定 Save System（InkSaveSystem）。", this);
                return;
            }

            saveSystem.LoadFromAutoSlot();
        }
        // ===== 變更結束 =====

        private void OnClickRollback()
        {
            if (saveSystem == null)
            {
                Debug.LogError("[OpsidanosInk] VNPlayerPresenter 尚未指定 Save System（InkSaveSystem）。", this);
                return;
            }

            // ===== 變更開始 =====
            // 2026/02/06 Opsidanos (修改原因：快速連按倒帶時，不能直接重入 Rollback；要排隊逐步執行)
            // 預期結果：多次點擊會一筆一筆倒帶，且到最前句時停止，不會刷紅字 Error
            isAutoEnabled = false;
            isSkipEnabled = false;
            RefreshToggleButtons();
            ResetAutoAdvanceState();

            pendingRollbackRequests += 1;
            if (rollbackCoroutine == null)
            {
                rollbackCoroutine = StartCoroutine(RollbackCoroutine());
            }
            // ===== 變更結束 =====
        }
        // ===== 變更結束 =====

        // ===== 變更開始 =====
        // 2026/01/25 Opsidanos (修改原因：選項點擊也要遵守同一套「Busy → ForceComplete → 冷卻」規則)
        // 預期結果：快速連點不會讓選項直接跳過上一句演出，避免狀態錯亂
        private void OnClickChoice(int choiceIndex)
        {
            // ===== 變更開始 =====
            // 2026/01/26 Opsidanos (修改原因：Auto 等待期間若點擊，這一次 Auto 直接作廢，改由點擊規則接管)
            // 預期結果：玩家點選時不會被 Auto 再推一次；就算點擊被冷卻擋住，也會讓 Auto 停下來
            SuppressAutoAdvanceIfCountdownActive();
            // ===== 變更結束 =====

            if (IsClickInCooldown())
            {
                return;
            }

            if (TryForceCompleteIfBusy(isClick: true))
            {
                // ===== 變更開始 =====
                // 2026/01/26 Opsidanos (修改原因：Auto 開啟時，點擊 ForceComplete 後要立刻開始算 1 秒)
                // 預期結果：點一下補完後放著不動，Auto 會自然接著播放
                StartAutoAdvanceCountdownIfPossible();
                // ===== 變更結束 =====
                return;
            }

            // ===== 變更開始 =====
            // 2026/01/26 Opsidanos (修改原因：點擊推進要重置 Auto 計時，並避免 Auto/點擊同一幀重複推進)
            // 預期結果：點選分支後不會沿用上一句的 Auto 倒數；不會同一幀連跳
            if (!TryMarkAdvancedThisFrame())
            {
                return;
            }

            ResetAutoAdvanceState();
            storyEngine.ChooseChoice(choiceIndex);
            // ===== 變更結束 =====
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
                // ===== 變更開始 =====
                // 2026/01/26 Opsidanos (修改原因：Auto 關閉時要清掉等待狀態，避免下次開啟立刻跳句)
                // 預期結果：Auto 再次開啟時會重新從「完成後等 1 秒」開始計時
                ResetAutoAdvanceState();
                // ===== 變更結束 =====
                return;
            }

            isSkipEnabled = false;
            isAutoEnabled = true;
            RefreshToggleButtons();
            // ===== 變更開始 =====
            // 2026/01/26 Opsidanos (修改原因：Auto 開啟時先清掉舊狀態，避免沿用上一句倒數)
            // 預期結果：Auto 行為一致且可預期
            ResetAutoAdvanceState();
            // ===== 變更結束 =====
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
            // ===== 變更開始 =====
            // 2026/01/26 Opsidanos (修改原因：切換成 Skip 時清掉 Auto 等待狀態)
            // 預期結果：Skip 不會被 Auto 的倒數狀態干擾
            ResetAutoAdvanceState();
            // ===== 變更結束 =====
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
            // ===== 變更開始 =====
            // 2026/01/26 Opsidanos (修改原因：每次進入新一句都要重置 Auto 等待狀態，避免沿用上一句倒數)
            // 預期結果：Auto 倒數永遠只對應目前這一句
            ResetAutoAdvanceState();
            // ===== 變更結束 =====

            if (speakerLabel != null)
            {
                speakerLabel.text = string.IsNullOrWhiteSpace(output.Speaker) ? "" : output.Speaker;
            }

            // ===== 變更開始 =====
            // 2026/01/26 Opsidanos (修改原因：加入打字機效果（逐字顯示對話）)
            // 預期結果：對話文字會逐字跑出；Busy 時點一下可直接顯示完整文字
            StartTypewriter(output);
            // ===== 變更結束 =====

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

        // ===== 變更開始 =====
        // 2026/02/06 Opsidanos (修改原因：統一停止倒帶協程與清空佇列，避免停用後殘留舊請求)
        // 預期結果：OnDisable/切場景時倒帶狀態會被正確重置
        private void StopRollbackCoroutine()
        {
            pendingRollbackRequests = 0;
            if (rollbackCoroutine == null)
            {
                return;
            }

            StopCoroutine(rollbackCoroutine);
            rollbackCoroutine = null;
        }

        private IEnumerator RollbackCoroutine()
        {
            while (pendingRollbackRequests > 0)
            {
                pendingRollbackRequests -= 1;

                while (IsAnyBusy())
                {
                    TryForceCompleteIfBusy(isClick: false);
                    yield return null;
                }

                if (!saveSystem.TryRollbackOnce())
                {
                    pendingRollbackRequests = 0;
                    Debug.Log("[OpsidanosInk][Rollback] 已到最前句，停止倒帶。", this);
                    break;
                }

                yield return null;
            }

            rollbackCoroutine = null;
        }
        // ===== 變更結束 =====

        private IEnumerator AutoSkipCoroutine()
        {
            while (isAutoEnabled || isSkipEnabled)
            {
                if (isBacklogOpen || isUiHidden)
                {
                    // ===== 變更開始 =====
                    // 2026/01/26 Opsidanos (修改原因：Auto/Skip 被 UI 狀態暫停時要清掉等待狀態)
                    // 預期結果：回到正常狀態後，不會沿用舊倒數造成突然跳句
                    ResetAutoAdvanceState();
                    // ===== 變更結束 =====
                    yield return null;
                    continue;
                }

                if (lastOutput == null)
                {
                    ResetAutoAdvanceState();
                    yield return null;
                    continue;
                }

                if (lastOutput.HasEnded || lastOutput.Choices.Count > 0)
                {
                    isAutoEnabled = false;
                    isSkipEnabled = false;
                    RefreshToggleButtons();
                    ResetAutoAdvanceState();
                    break;
                }

                if (isSkipEnabled)
                {
                    float waitSeconds = skipIntervalSeconds;
                    if (waitSeconds > 0f)
                    {
                        yield return new WaitForSecondsRealtime(waitSeconds);
                    }
                    else
                    {
                        yield return null;
                    }

                    if (!isSkipEnabled)
                    {
                        continue;
                    }

                    // ===== 變更開始 =====
                    // 2026/01/26 Opsidanos (修改原因：Skip 模式也需要把打字機/動畫視為 Busy，Busy 時先 ForceComplete)
                    // 預期結果：Skip 不會因為打字機或人物動畫尚未結束而造成狀態錯亂
                    if (!TryForceCompleteIfBusy(isClick: false))
                    {
                        if (!TryMarkAdvancedThisFrame())
                        {
                            continue;
                        }

                        ResetAutoAdvanceState();
                        storyEngine.Continue();
                    }
                    // ===== 變更結束 =====

                    continue;
                }

                if (!isAutoEnabled)
                {
                    continue;
                }

                // ===== 變更開始 =====
                // 2026/01/26 Opsidanos (修改原因：Auto 規則改成「打字機+動畫完成後，再等 1 秒」才推進；等待期間若點擊則作廢)
                // 預期結果：Auto 播放自然且不會與點擊互搶；點擊會接管推進，Auto 不會再偷推一次
                if (IsAnyBusy())
                {
                    isAutoAdvanceCountdownActive = false;
                    yield return null;
                    continue;
                }

                if (autoAdvanceSuppressedOutputId == lastOutput.OutputId)
                {
                    yield return null;
                    continue;
                }

                if (!isAutoAdvanceCountdownActive || autoAdvanceCountdownOutputId != lastOutput.OutputId)
                {
                    if (autoDelaySeconds <= 0f)
                    {
                        autoAdvanceCountdownUntilUnscaled = Time.unscaledTime;
                    }
                    else
                    {
                        autoAdvanceCountdownUntilUnscaled = Time.unscaledTime + autoDelaySeconds;
                    }

                    isAutoAdvanceCountdownActive = true;
                    autoAdvanceCountdownOutputId = lastOutput.OutputId;
                }

                if (Time.unscaledTime >= autoAdvanceCountdownUntilUnscaled)
                {
                    isAutoAdvanceCountdownActive = false;

                    if (!TryMarkAdvancedThisFrame())
                    {
                        yield return null;
                        continue;
                    }

                    ResetAutoAdvanceState();
                    storyEngine.Continue();
                }

                yield return null;
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
            // ===== 變更開始 =====
            // 2026/01/26 Opsidanos (修改原因：Busy 需要包含打字機與人物動畫；ForceComplete 也要能補完打字機)
            // 預期結果：打字機跑到一半時點一下會立刻顯示完整文字，且不會直接進下一句
            bool anyBusy = false;

            if (IsTypewriterBusy())
            {
                anyBusy = true;
            }

            if (resolvedAdvanceBlockers.Count == 0)
            {
                if (!anyBusy)
                {
                    return false;
                }
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
                    anyBusy = true;
                }
            }

            if (!anyBusy)
            {
                return false;
            }

            ForceCompleteTypewriter();

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
            // ===== 變更結束 =====
        }

        // ===== 變更開始 =====
        // 2026/01/26 Opsidanos (修改原因：打字機與 Auto 狀態管理)
        // 預期結果：Auto/點擊/Skip 不互相衝突；打字機可被補完；Auto 等待期間點擊會作廢
        private void ResetAutoAdvanceState()
        {
            isAutoAdvanceCountdownActive = false;
            autoAdvanceCountdownOutputId = 0;
            autoAdvanceCountdownUntilUnscaled = 0f;
            autoAdvanceSuppressedOutputId = -1;
        }

        private void SuppressAutoAdvanceIfCountdownActive()
        {
            if (!isAutoEnabled)
            {
                return;
            }

            if (!isAutoAdvanceCountdownActive)
            {
                return;
            }

            if (lastOutput == null)
            {
                return;
            }

            if (autoAdvanceCountdownOutputId != lastOutput.OutputId)
            {
                return;
            }

            isAutoAdvanceCountdownActive = false;
            autoAdvanceSuppressedOutputId = lastOutput.OutputId;
        }

        private void StartAutoAdvanceCountdownIfPossible()
        {
            if (!isAutoEnabled)
            {
                return;
            }

            if (lastOutput == null)
            {
                return;
            }

            if (lastOutput.HasEnded || lastOutput.Choices.Count > 0)
            {
                return;
            }

            if (IsAnyBusy())
            {
                return;
            }

            isAutoAdvanceCountdownActive = true;
            autoAdvanceCountdownOutputId = lastOutput.OutputId;
            autoAdvanceSuppressedOutputId = -1;
            autoAdvanceCountdownUntilUnscaled = Time.unscaledTime + Mathf.Max(0f, autoDelaySeconds);
        }

        private bool TryMarkAdvancedThisFrame()
        {
            if (Time.frameCount == lastAdvanceFrame)
            {
                return false;
            }

            lastAdvanceFrame = Time.frameCount;
            return true;
        }

        private bool IsAnyBusy()
        {
            if (IsTypewriterBusy())
            {
                return true;
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
                    return true;
                }
            }

            return false;
        }

        private void StartTypewriter(StoryOutput output)
        {
            if (bodyLabel == null)
            {
                return;
            }

            StopTypewriterRoutine();

            typewriterFullText = output.LineText ?? "";
            typewriterVisibleCharCount = 0;
            typewriterOutputId = output.OutputId;

            if (!typewriterEnabled || string.IsNullOrEmpty(typewriterFullText) || typewriterCharsPerSecond <= 0f)
            {
                bodyLabel.text = typewriterFullText;
                typewriterVisibleCharCount = typewriterFullText.Length;
                return;
            }

            bodyLabel.text = "";
            typewriterRoutine = StartCoroutine(TypewriterCoroutine(typewriterOutputId, typewriterFullText));
        }

        private IEnumerator TypewriterCoroutine(int outputId, string fullText)
        {
            float visibleFloat = 0f;

            while (true)
            {
                if (lastOutput == null || lastOutput.OutputId != outputId)
                {
                    break;
                }

                if (typewriterVisibleCharCount >= fullText.Length)
                {
                    break;
                }

                visibleFloat += typewriterCharsPerSecond * Time.unscaledDeltaTime;
                int nextCount = Mathf.Clamp(Mathf.FloorToInt(visibleFloat), 0, fullText.Length);

                if (nextCount != typewriterVisibleCharCount)
                {
                    typewriterVisibleCharCount = nextCount;
                    bodyLabel.text = fullText.Substring(0, typewriterVisibleCharCount);
                }

                yield return null;
            }

            typewriterRoutine = null;

            if (lastOutput != null && lastOutput.OutputId == outputId)
            {
                typewriterVisibleCharCount = fullText.Length;
                bodyLabel.text = fullText;
            }
        }

        private void StopTypewriterRoutine()
        {
            if (typewriterRoutine == null)
            {
                return;
            }

            StopCoroutine(typewriterRoutine);
            typewriterRoutine = null;
        }

        private bool IsTypewriterBusy()
        {
            if (typewriterRoutine == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(typewriterFullText))
            {
                return false;
            }

            return typewriterVisibleCharCount < typewriterFullText.Length;
        }

        private void ForceCompleteTypewriter()
        {
            if (bodyLabel == null)
            {
                return;
            }

            if (!IsTypewriterBusy())
            {
                return;
            }

            StopTypewriterRoutine();
            typewriterVisibleCharCount = typewriterFullText.Length;
            bodyLabel.text = typewriterFullText;
        }
        // ===== 變更結束 =====

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
