// ===== 變更開始 =====
// 2026/02/05 Opsidanos (修改原因：新增 PlayMode 的 UI 點擊測試，確保 VNPlayer.uxml 的按鈕接線與狀態切換正常)
// 預期結果：在 Unity Test Runner（PlayMode）會真的「點」Backlog/Auto/Skip/Hide/Show 按鈕並驗證 UI 狀態
using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using OpsidanosInk.Runtime.Save;
// ===== 變更開始 =====
// 2026/02/12 Opsidanos (修改原因：新增 ContinueButton 兩段式點擊節奏回歸測試，需要直接取得角色演出 blocker)
// 預期結果：測試可使用 InkTagCharacterStatePlayer 建立 Busy 狀態並驗證 ForceComplete/冷卻/前進節奏
using OpsidanosInk.Runtime.Presentation;
// ===== 變更結束 =====
using OpsidanosInk.Runtime.Story;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace OpsidanosInk.Tests
{
    public sealed class OpsidanosInkPlayModeUiClickTests
    {
        private const string TestSceneName = "Test";

        [UnityTest]
        public IEnumerator BacklogButton_可開關Backlog面板()
        {
            yield return LoadTestScene();

            GameObject vnPlayer = FindVNPlayer();
            UIDocument uiDocument = GetRequiredComponent<UIDocument>(vnPlayer, "UIDocument");
            yield return WaitUntilUiReady(uiDocument, 3f);

            VisualElement root = uiDocument.rootVisualElement;
            Button backlogButton = root.Q<Button>("BacklogButton");
            Button backlogCloseButton = root.Q<Button>("BacklogCloseButton");
            VisualElement backlogPanel = root.Q<VisualElement>("BacklogPanel");

            Assert.IsNotNull(backlogButton, "找不到 BacklogButton。");
            Assert.IsNotNull(backlogCloseButton, "找不到 BacklogCloseButton。");
            Assert.IsNotNull(backlogPanel, "找不到 BacklogPanel。");

            Assert.IsTrue(backlogPanel.ClassListContains("vn-hidden"), "BacklogPanel 初始應該是關閉（vn-hidden）。");

            SimulateLeftClick(backlogButton);
            yield return null;

            Assert.IsFalse(backlogPanel.ClassListContains("vn-hidden"), "點 BacklogButton 後 BacklogPanel 應該打開。");
            Assert.IsTrue(backlogButton.ClassListContains("vn-toggle-on"), "點 BacklogButton 後 BacklogButton 應該有 vn-toggle-on。");

            SimulateLeftClick(backlogCloseButton);
            yield return null;

            Assert.IsTrue(backlogPanel.ClassListContains("vn-hidden"), "點 BacklogCloseButton 後 BacklogPanel 應該關閉。");
            Assert.IsFalse(backlogButton.ClassListContains("vn-toggle-on"), "關閉後 BacklogButton 不應該有 vn-toggle-on。");
        }

        [UnityTest]
        public IEnumerator AutoButton_可切換文字與樣式()
        {
            yield return LoadTestScene();

            GameObject vnPlayer = FindVNPlayer();
            UIDocument uiDocument = GetRequiredComponent<UIDocument>(vnPlayer, "UIDocument");
            yield return WaitUntilUiReady(uiDocument, 3f);

            VisualElement root = uiDocument.rootVisualElement;
            Button autoButton = root.Q<Button>("AutoButton");
            Button skipButton = root.Q<Button>("SkipButton");

            Assert.IsNotNull(autoButton, "找不到 AutoButton。");
            Assert.IsNotNull(skipButton, "找不到 SkipButton。");

            Assert.AreEqual("自動：關", autoButton.text, "AutoButton 初始文字應為 自動：關。");
            Assert.IsFalse(autoButton.ClassListContains("vn-toggle-on"), "AutoButton 初始不應該是 on（vn-toggle-on）。");

            SimulateLeftClick(autoButton);
            yield return null;

            Assert.AreEqual("自動：開", autoButton.text, "點擊後 AutoButton 文字應為 自動：開。");
            Assert.IsTrue(autoButton.ClassListContains("vn-toggle-on"), "點擊後 AutoButton 應該有 vn-toggle-on。");
            Assert.AreEqual("快轉：關", skipButton.text, "開 Auto 時 Skip 應該自動關閉。");
            Assert.IsFalse(skipButton.ClassListContains("vn-toggle-on"), "開 Auto 時 Skip 不應該是 on。");

            SimulateLeftClick(autoButton);
            yield return null;

            Assert.AreEqual("自動：關", autoButton.text, "再點一次後 AutoButton 文字應為 自動：關。");
            Assert.IsFalse(autoButton.ClassListContains("vn-toggle-on"), "再點一次後 AutoButton 不應該有 vn-toggle-on。");
        }

        [UnityTest]
        public IEnumerator SkipButton_可切換文字與樣式()
        {
            yield return LoadTestScene();

            GameObject vnPlayer = FindVNPlayer();
            UIDocument uiDocument = GetRequiredComponent<UIDocument>(vnPlayer, "UIDocument");
            yield return WaitUntilUiReady(uiDocument, 3f);

            VisualElement root = uiDocument.rootVisualElement;
            Button autoButton = root.Q<Button>("AutoButton");
            Button skipButton = root.Q<Button>("SkipButton");

            Assert.IsNotNull(autoButton, "找不到 AutoButton。");
            Assert.IsNotNull(skipButton, "找不到 SkipButton。");

            Assert.AreEqual("快轉：關", skipButton.text, "SkipButton 初始文字應為 快轉：關。");
            Assert.IsFalse(skipButton.ClassListContains("vn-toggle-on"), "SkipButton 初始不應該是 on（vn-toggle-on）。");

            SimulateLeftClick(skipButton);
            yield return null;

            Assert.AreEqual("快轉：開", skipButton.text, "點擊後 SkipButton 文字應為 快轉：開。");
            Assert.IsTrue(skipButton.ClassListContains("vn-toggle-on"), "點擊後 SkipButton 應該有 vn-toggle-on。");
            Assert.AreEqual("自動：關", autoButton.text, "開 Skip 時 Auto 應該自動關閉。");
            Assert.IsFalse(autoButton.ClassListContains("vn-toggle-on"), "開 Skip 時 Auto 不應該是 on。");

            SimulateLeftClick(skipButton);
            yield return null;

            Assert.AreEqual("快轉：關", skipButton.text, "再點一次後 SkipButton 文字應為 快轉：關。");
            Assert.IsFalse(skipButton.ClassListContains("vn-toggle-on"), "再點一次後 SkipButton 不應該有 vn-toggle-on。");
        }

        [UnityTest]
        public IEnumerator HideButton_可隱藏UI_並用ShowUIButton恢復()
        {
            yield return LoadTestScene();

            GameObject vnPlayer = FindVNPlayer();
            UIDocument uiDocument = GetRequiredComponent<UIDocument>(vnPlayer, "UIDocument");
            yield return WaitUntilUiReady(uiDocument, 3f);

            VisualElement root = uiDocument.rootVisualElement;
            VisualElement vnRoot = root.Q<VisualElement>("VNRoot");
            Button hideButton = root.Q<Button>("HideButton");
            Button showUIButton = root.Q<Button>("ShowUIButton");

            Assert.IsNotNull(vnRoot, "找不到 VNRoot。");
            Assert.IsNotNull(hideButton, "找不到 HideButton。");
            Assert.IsNotNull(showUIButton, "找不到 ShowUIButton。");

            Assert.IsFalse(vnRoot.ClassListContains("vn-ui-hidden"), "初始不應該隱藏 UI（vn-ui-hidden）。");

            SimulateLeftClick(hideButton);
            yield return null;
            yield return null;

            Assert.IsTrue(vnRoot.ClassListContains("vn-ui-hidden"), "點 HideButton 後 VNRoot 應該有 vn-ui-hidden。");
            Assert.AreEqual(DisplayStyle.Flex, showUIButton.resolvedStyle.display, "隱藏 UI 時 ShowUIButton 應該顯示（display:flex）。");

            SimulateLeftClick(showUIButton);
            yield return null;
            yield return null;

            Assert.IsFalse(vnRoot.ClassListContains("vn-ui-hidden"), "點 ShowUIButton 後應該取消隱藏 UI。");
            Assert.AreEqual(DisplayStyle.None, showUIButton.resolvedStyle.display, "取消隱藏後 ShowUIButton 應該不顯示（display:none）。");
        }

        // ===== 變更開始 =====
        // 2026/02/12 Opsidanos (修改原因：補齊 ContinueButton 的兩段式點擊節奏回歸測試（Busy→ForceComplete→冷卻→再點才前進）)
        // 預期結果：Busy 時第一次點擊只補完演出且不前進；冷卻內點擊不前進；冷卻後再點才前進（Normal/Restore 皆適用）
        [UnityTest]
        public IEnumerator ContinueButton_Busy時第一次只補完不前進_冷卻後第二次才前進_Normal()
        {
            yield return LoadTestScene();

            GameObject vnPlayer = FindVNPlayer();
            UIDocument uiDocument = GetRequiredComponent<UIDocument>(vnPlayer, "UIDocument");
            InkStoryEngine storyEngine = GetRequiredComponent<InkStoryEngine>(vnPlayer, "InkStoryEngine");
            InkTagCharacterStatePlayer charStatePlayer = GetRequiredComponent<InkTagCharacterStatePlayer>(vnPlayer, "InkTagCharacterStatePlayer");
            yield return WaitUntilUiReady(uiDocument, 3f);
            yield return WaitUntilNotBusy(charStatePlayer, 3f);

            Button continueButton = uiDocument.rootVisualElement.Q<Button>("ContinueButton");
            Assert.IsNotNull(continueButton, "找不到 ContinueButton。");

            var outputs = new List<StoryOutput>();
            Action<StoryOutput> handler = output => outputs.Add(output);
            storyEngine.OutputGenerated += handler;

            try
            {
                yield return null;
                yield return null;
                outputs.Clear();

                string charJson = BuildCharJson(
                    leftActor: "bs",
                    centerActor: "alice",
                    rightActor: "ss",
                    appearSeconds: 1.0f,
                    moveSeconds: 0f,
                    disappearSeconds: 0f);

                storyEngine.EmitExternalOutput(BuildCharOutput(9201, StoryOutputSource.Normal, charJson));
                yield return null;
                yield return WaitUntilBusy(charStatePlayer, 3f);

                int baselineOutputCount = outputs.Count;

                SimulateLeftClick(continueButton);
                yield return null;

                Assert.IsFalse(charStatePlayer.IsBusy, "Busy 時第一次點 ContinueButton 應該 ForceComplete 角色演出。");
                yield return AssertNoNewOutputForSeconds(outputs, baselineOutputCount, 0.2f, "Busy 第一次點擊只補完，不應前進。");

                SimulateLeftClick(continueButton);
                yield return null;
                yield return AssertNoNewOutputForSeconds(outputs, baselineOutputCount, 0.2f, "冷卻內點擊不應前進。");

                yield return new WaitForSecondsRealtime(0.35f);

                int beforeAdvance = outputs.Count;
                SimulateLeftClick(continueButton);
                yield return WaitForNewOutput(outputs, beforeAdvance, 3f, "冷卻結束後第二次點擊應前進並產生新輸出。");
            }
            finally
            {
                storyEngine.OutputGenerated -= handler;
            }
        }

        [UnityTest]
        public IEnumerator ContinueButton_Busy時第一次只補完不前進_冷卻後第二次才前進_Restore()
        {
            yield return LoadTestScene();

            GameObject vnPlayer = FindVNPlayer();
            UIDocument uiDocument = GetRequiredComponent<UIDocument>(vnPlayer, "UIDocument");
            InkStoryEngine storyEngine = GetRequiredComponent<InkStoryEngine>(vnPlayer, "InkStoryEngine");
            InkTagCharacterStatePlayer charStatePlayer = GetRequiredComponent<InkTagCharacterStatePlayer>(vnPlayer, "InkTagCharacterStatePlayer");
            yield return WaitUntilUiReady(uiDocument, 3f);
            yield return WaitUntilNotBusy(charStatePlayer, 3f);

            Button continueButton = uiDocument.rootVisualElement.Q<Button>("ContinueButton");
            Assert.IsNotNull(continueButton, "找不到 ContinueButton。");

            var outputs = new List<StoryOutput>();
            Action<StoryOutput> handler = output => outputs.Add(output);
            storyEngine.OutputGenerated += handler;

            try
            {
                yield return null;
                yield return null;
                outputs.Clear();

                string charJson = BuildCharJson(
                    leftActor: "bs",
                    centerActor: "alice",
                    rightActor: "ss",
                    appearSeconds: 1.0f,
                    moveSeconds: 0f,
                    disappearSeconds: 0f);

                storyEngine.EmitExternalOutput(BuildCharOutput(9202, StoryOutputSource.Restore, charJson));
                yield return null;
                yield return WaitUntilBusy(charStatePlayer, 3f);

                int baselineOutputCount = outputs.Count;

                SimulateLeftClick(continueButton);
                yield return null;

                Assert.IsFalse(charStatePlayer.IsBusy, "Restore Busy 時第一次點 ContinueButton 應該 ForceComplete 角色演出。");
                yield return AssertNoNewOutputForSeconds(outputs, baselineOutputCount, 0.2f, "Restore Busy 第一次點擊只補完，不應前進。");

                SimulateLeftClick(continueButton);
                yield return null;
                yield return AssertNoNewOutputForSeconds(outputs, baselineOutputCount, 0.2f, "Restore 冷卻內點擊不應前進。");

                yield return new WaitForSecondsRealtime(0.35f);

                int beforeAdvance = outputs.Count;
                SimulateLeftClick(continueButton);
                yield return WaitForNewOutput(outputs, beforeAdvance, 3f, "Restore 冷卻結束後第二次點擊應前進並產生新輸出。");
            }
            finally
            {
                storyEngine.OutputGenerated -= handler;
            }
        }
        // ===== 變更結束 =====

        // ===== 變更開始 =====
        // 2026/02/06 Opsidanos (修改原因：新增多槽與 Auto 槽按鈕的 UI 點擊驗證，確保按鈕真的接到對應 API)
        // 預期結果：透過 UI 點擊存/讀不同槽位時，讀回的 Ink state 與各槽位存檔時一致
        [UnityTest]
        public IEnumerator SlotButtons_可觸發對應槽位存讀()
        {
            yield return LoadTestScene();

            GameObject vnPlayer = FindVNPlayer();
            UIDocument uiDocument = GetRequiredComponent<UIDocument>(vnPlayer, "UIDocument");
            InkStoryEngine storyEngine = GetRequiredComponent<InkStoryEngine>(vnPlayer, "InkStoryEngine");
            yield return WaitUntilUiReady(uiDocument, 3f);

            VisualElement root = uiDocument.rootVisualElement;
            Button saveSlot1Button = root.Q<Button>("SaveSlot1Button");
            Button loadSlot1Button = root.Q<Button>("LoadSlot1Button");
            Button saveSlot2Button = root.Q<Button>("SaveSlot2Button");
            Button loadSlot2Button = root.Q<Button>("LoadSlot2Button");
            Button saveAutoSlotButton = root.Q<Button>("SaveAutoSlotButton");
            Button loadAutoSlotButton = root.Q<Button>("LoadAutoSlotButton");

            Assert.IsNotNull(saveSlot1Button, "找不到 SaveSlot1Button。");
            Assert.IsNotNull(loadSlot1Button, "找不到 LoadSlot1Button。");
            Assert.IsNotNull(saveSlot2Button, "找不到 SaveSlot2Button。");
            Assert.IsNotNull(loadSlot2Button, "找不到 LoadSlot2Button。");
            Assert.IsNotNull(saveAutoSlotButton, "找不到 SaveAutoSlotButton。");
            Assert.IsNotNull(loadAutoSlotButton, "找不到 LoadAutoSlotButton。");

            var outputs = new List<StoryOutput>();
            Action<StoryOutput> outputHandler = output => outputs.Add(output);
            storyEngine.OutputGenerated += outputHandler;

            try
            {
                yield return AdvanceStoryOneStep(storyEngine, outputs);
                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string stateSlot1), "應該能取得槽位 1 存檔狀態。");
                SimulateLeftClick(saveSlot1Button);
                yield return null;

                yield return AdvanceStoryOneStep(storyEngine, outputs);
                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string stateSlot2), "應該能取得槽位 2 存檔狀態。");
                SimulateLeftClick(saveSlot2Button);
                yield return null;

                yield return AdvanceStoryOneStep(storyEngine, outputs);
                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string stateAuto), "應該能取得 Auto 槽存檔狀態。");
                SimulateLeftClick(saveAutoSlotButton);
                yield return null;

                int beforeLoadCount = outputs.Count;
                SimulateLeftClick(loadSlot2Button);
                yield return WaitForNewOutput(outputs, beforeLoadCount, 3f, "點讀2後應送出 Restore 輸出");
                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string loadedSlot2), "點讀2後應能取得狀態。");
                Assert.AreEqual(stateSlot2, loadedSlot2, "讀2後應回到槽位 2 狀態。");

                beforeLoadCount = outputs.Count;
                SimulateLeftClick(loadSlot1Button);
                yield return WaitForNewOutput(outputs, beforeLoadCount, 3f, "點讀1後應送出 Restore 輸出");
                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string loadedSlot1), "點讀1後應能取得狀態。");
                Assert.AreEqual(stateSlot1, loadedSlot1, "讀1後應回到槽位 1 狀態。");

                beforeLoadCount = outputs.Count;
                SimulateLeftClick(loadAutoSlotButton);
                yield return WaitForNewOutput(outputs, beforeLoadCount, 3f, "點自讀後應送出 Restore 輸出");
                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string loadedAuto), "點自讀後應能取得狀態。");
                Assert.AreEqual(stateAuto, loadedAuto, "自讀後應回到 Auto 槽狀態。");
            }
            finally
            {
                storyEngine.OutputGenerated -= outputHandler;
            }
        }
        // ===== 變更結束 =====

        // ===== 變更開始 =====
        // 2026/02/06 Opsidanos (修改原因：驗證 RollbackButton 快速連按時，會排隊逐步倒帶，不會卡在中間)
        // 預期結果：連按多次 Rollback 後，狀態會穩定回到可倒帶最前句
        [UnityTest]
        public IEnumerator RollbackButton_快速連按_會排隊倒帶到最前句()
        {
            yield return LoadTestScene();

            GameObject vnPlayer = FindVNPlayer();
            UIDocument uiDocument = GetRequiredComponent<UIDocument>(vnPlayer, "UIDocument");
            InkStoryEngine storyEngine = GetRequiredComponent<InkStoryEngine>(vnPlayer, "InkStoryEngine");
            InkSaveSystem saveSystem = GetRequiredComponent<InkSaveSystem>(vnPlayer, "InkSaveSystem");
            yield return WaitUntilUiReady(uiDocument, 3f);

            var outputs = new List<StoryOutput>();
            Action<StoryOutput> handler = output => outputs.Add(output);
            storyEngine.OutputGenerated += handler;

            try
            {
                yield return AdvanceStoryOneStep(storyEngine, outputs);
                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out _), "應該能取得第一句狀態。");

                yield return AdvanceStoryOneStep(storyEngine, outputs);
                yield return AdvanceStoryOneStep(storyEngine, outputs);
                yield return AdvanceStoryOneStep(storyEngine, outputs);
                Assert.GreaterOrEqual(saveSystem.AvailableRollbackSteps, 3, "建立測試資料後，應至少有 3 步可倒帶。");
                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string stateBeforeRapidRollback), "快速倒帶前應能取得狀態。");

                Button rollbackButton = uiDocument.rootVisualElement.Q<Button>("RollbackButton");
                Assert.IsNotNull(rollbackButton, "找不到 RollbackButton。");

                int rollbackClicks = Mathf.Min(3, saveSystem.AvailableRollbackSteps);
                int expectedRemainingSteps = saveSystem.AvailableRollbackSteps - rollbackClicks;
                for (int i = 0; i < rollbackClicks; i++)
                {
                    SimulateLeftClick(rollbackButton);
                }

                float start = Time.realtimeSinceStartup;
                while (saveSystem.AvailableRollbackSteps > expectedRemainingSteps)
                {
                    if (Time.realtimeSinceStartup - start > 5f)
                    {
                        Assert.Fail("等待快速連按 Rollback 收斂逾時。");
                    }

                    yield return null;
                }

                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string stateAfterRollback), "倒帶後應該能取得狀態。");
                Assert.AreNotEqual(stateBeforeRapidRollback, stateAfterRollback, "快速連按 Rollback 後，狀態應該改變。");
                Assert.AreEqual(expectedRemainingSteps, saveSystem.AvailableRollbackSteps, "快速連按 Rollback 後，可倒帶步數應與預期一致。");
            }
            finally
            {
                storyEngine.OutputGenerated -= handler;
            }
        }
        // ===== 變更結束 =====

        // ===== 變更開始 =====
        // 2026/02/06 Opsidanos (修改原因：補上更高壓的實測，覆蓋「讀檔後 + 超量連按 rollback」情境)
        // 預期結果：Load 後連按 rollback 超過可倒帶步數，仍可穩定回到最前句，且不產生 rollbackBuffer 不足 Error
        [UnityTest]
        public IEnumerator RollbackButton_高壓連按_讀檔後仍可回到存檔前且無錯誤()
        {
            yield return LoadTestScene();

            GameObject vnPlayer = FindVNPlayer();
            UIDocument uiDocument = GetRequiredComponent<UIDocument>(vnPlayer, "UIDocument");
            InkStoryEngine storyEngine = GetRequiredComponent<InkStoryEngine>(vnPlayer, "InkStoryEngine");
            InkSaveSystem saveSystem = GetRequiredComponent<InkSaveSystem>(vnPlayer, "InkSaveSystem");
            yield return WaitUntilUiReady(uiDocument, 3f);

            var outputs = new List<StoryOutput>();
            Action<StoryOutput> outputHandler = output => outputs.Add(output);
            storyEngine.OutputGenerated += outputHandler;

            string savePointState = null;

            try
            {
                int safety = 20;
                while (safety-- > 0)
                {
                    if (outputs.Count > 0 && outputs[outputs.Count - 1].HasEnded)
                    {
                        break;
                    }

                    yield return AdvanceStoryOneStep(storyEngine, outputs);

                    if (savePointState == null && outputs.Count >= 5)
                    {
                        Assert.IsTrue(storyEngine.TryGetStoryStateJson(out savePointState), "應該能取得存檔點狀態。");
                        saveSystem.SaveToSlot();
                    }
                }

                Assert.IsNotNull(savePointState, "高壓測試前，必須先建立存檔點。");

                int beforeLoadCount = outputs.Count;
                saveSystem.LoadFromSlot();
                yield return WaitForNewOutput(outputs, beforeLoadCount, 3f, "LoadFromSlot 應該送出一筆輸出。");

                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string stateAfterLoad), "讀檔後應能取得狀態。");
                Assert.AreEqual(savePointState, stateAfterLoad, "讀檔後應回到存檔點。");
                Assert.GreaterOrEqual(saveSystem.AvailableRollbackSteps, 1, "讀檔後至少應該還有 1 步可倒帶。");

                Button rollbackButton = uiDocument.rootVisualElement.Q<Button>("RollbackButton");
                Assert.IsNotNull(rollbackButton, "找不到 RollbackButton。");

                var rollbackErrors = new List<string>();
                Application.LogCallback logHandler = (condition, stackTrace, type) =>
                {
                    if (type != LogType.Error)
                    {
                        return;
                    }

                    if (condition != null && condition.Contains("InkSaveSystem 無法倒帶"))
                    {
                        rollbackErrors.Add(condition);
                    }
                };

                int overClickCount = saveSystem.AvailableRollbackSteps + 30;
                Application.logMessageReceived += logHandler;
                try
                {
                    for (int i = 0; i < overClickCount; i++)
                    {
                        SimulateLeftClick(rollbackButton);
                    }

                    float start = Time.realtimeSinceStartup;
                    while (saveSystem.AvailableRollbackSteps > 0)
                    {
                        if (Time.realtimeSinceStartup - start > 15f)
                        {
                            Assert.Fail("高壓 rollback 等待收斂逾時。");
                        }

                        yield return null;
                    }
                }
                finally
                {
                    Application.logMessageReceived -= logHandler;
                }

                Assert.AreEqual(0, rollbackErrors.Count, "高壓 rollback 不應出現 rollbackBuffer 不足 Error。");
                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string stateAfterStressRollback), "高壓 rollback 後應能取得狀態。");
                Assert.AreNotEqual(savePointState, stateAfterStressRollback, "高壓 rollback 後，狀態應該已離開存檔點。");
                Assert.AreEqual(0, saveSystem.AvailableRollbackSteps, "高壓 rollback 後應收斂到最前句（無可倒帶步數）。");
                Assert.IsFalse(saveSystem.CanRollback, "高壓 rollback 後不應再可倒帶。");
            }
            finally
            {
                storyEngine.OutputGenerated -= outputHandler;
            }
        }
        // ===== 變更結束 =====

        // ===== 變更開始 =====
        // 2026/02/12 Opsidanos (修改原因：ContinueButton 節奏測試需要可重用的「建立 Busy」與「禁止前進」工具)
        // 預期結果：能穩定建立角色 Busy（appear>0），並精準驗證點擊後是否產生新輸出
        private static StoryOutput BuildCharOutput(int outputId, StoryOutputSource source, string charJson)
        {
            var rawTags = new List<string>
            {
                $"char:{charJson}"
            };

            var parsedTags = new List<InkTag>
            {
                new InkTag("char", charJson)
            };

            return new StoryOutput(
                outputId,
                speaker: "測試",
                lineText: "測試",
                hasEnded: false,
                tags: rawTags,
                parsedTags: parsedTags,
                choices: new List<ChoiceOutput>(),
                source: source);
        }

        private static string BuildCharJson(
            string leftActor,
            string centerActor,
            string rightActor,
            float appearSeconds,
            float moveSeconds,
            float disappearSeconds)
        {
            string centerJson = string.IsNullOrWhiteSpace(centerActor)
                ? string.Empty
                : ",\"center\":{"
                + $"\"actor\":\"{centerActor}\""
                + "}";

            return
                "{"
                + "\"transition\":{"
                + $"\"appear\":{appearSeconds.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)},"
                + $"\"move\":{moveSeconds.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)},"
                + $"\"disappear\":{disappearSeconds.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}"
                + "},"
                + "\"left\":{"
                + $"\"actor\":\"{leftActor}\""
                + "}"
                + centerJson
                + ",\"right\":{"
                + $"\"actor\":\"{rightActor}\""
                + "}"
                + "}";
        }

        private static IEnumerator WaitUntilBusy(InkTagCharacterStatePlayer player, float timeoutSeconds)
        {
            Assert.IsNotNull(player, "InkTagCharacterStatePlayer 不可為 null。");

            float start = Time.realtimeSinceStartup;
            while (!player.IsBusy)
            {
                if (Time.realtimeSinceStartup - start > timeoutSeconds)
                {
                    Assert.Fail("等待角色演出開始逾時（IsBusy 一直為 false）。");
                }

                yield return null;
            }
        }

        private static IEnumerator WaitUntilNotBusy(InkTagCharacterStatePlayer player, float timeoutSeconds)
        {
            Assert.IsNotNull(player, "InkTagCharacterStatePlayer 不可為 null。");

            float start = Time.realtimeSinceStartup;
            while (player.IsBusy)
            {
                if (Time.realtimeSinceStartup - start > timeoutSeconds)
                {
                    Assert.Fail("等待角色演出結束逾時（IsBusy 一直為 true）。");
                }

                yield return null;
            }
        }

        private static IEnumerator AssertNoNewOutputForSeconds(List<StoryOutput> outputs, int previousCount, float seconds, string reason)
        {
            float start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - start < seconds)
            {
                if (outputs.Count > previousCount)
                {
                    Assert.Fail($"不應產生新輸出：{reason}");
                }

                yield return null;
            }
        }
        // ===== 變更結束 =====

        // ===== 變更開始 =====
        // 2026/02/06 Opsidanos (修改原因：Rollback 快速連按測試需要可重用的推進步驟 helper)
        // 預期結果：每次呼叫都會產生一筆新輸出，若遇到選項就選第 1 個繼續推進
        private static IEnumerator AdvanceStoryOneStep(InkStoryEngine storyEngine, List<StoryOutput> outputs)
        {
            int before = outputs.Count;
            storyEngine.Continue();
            yield return WaitForNewOutput(outputs, before, 3f, "Continue() 後應產生新輸出");

            StoryOutput last = outputs[outputs.Count - 1];
            if (last.Choices == null || last.Choices.Count <= 0)
            {
                yield break;
            }

            before = outputs.Count;
            storyEngine.ChooseChoice(last.Choices[0].Index);
            yield return WaitForNewOutput(outputs, before, 3f, "ChooseChoice(0) 後應產生新輸出");
        }

        private static IEnumerator WaitForNewOutput(List<StoryOutput> outputs, int previousCount, float timeoutSeconds, string reason)
        {
            float start = Time.realtimeSinceStartup;
            while (outputs.Count <= previousCount)
            {
                if (Time.realtimeSinceStartup - start > timeoutSeconds)
                {
                    Assert.Fail($"等待輸出逾時：{reason}");
                }

                yield return null;
            }
        }
        // ===== 變更結束 =====

        private static IEnumerator LoadTestScene()
        {
            SceneManager.LoadScene(TestSceneName, LoadSceneMode.Single);
            yield return null;
            yield return null;
        }

        private static IEnumerator WaitUntilUiReady(UIDocument uiDocument, float timeoutSeconds)
        {
            Assert.IsNotNull(uiDocument, "UIDocument 不可為 null。");

            float start = Time.realtimeSinceStartup;
            while (uiDocument.rootVisualElement == null || uiDocument.rootVisualElement.panel == null)
            {
                if (Time.realtimeSinceStartup - start > timeoutSeconds)
                {
                    Assert.Fail("等待 UI 初始化逾時：rootVisualElement 或 panel 仍為 null。");
                }

                yield return null;
            }
        }

        private static GameObject FindVNPlayer()
        {
            GameObject vnPlayer = GameObject.Find("VNPlayer");
            Assert.IsNotNull(vnPlayer, "找不到場景物件 VNPlayer。請確認 `Assets/Scene/Test.unity` 的根物件名稱仍為 VNPlayer。");
            return vnPlayer;
        }

        private static T GetRequiredComponent<T>(GameObject go, string readableName) where T : Component
        {
            T component = go != null ? go.GetComponent<T>() : null;
            Assert.IsNotNull(component, $"VNPlayer 缺少元件：{readableName}（{typeof(T).Name}）。");
            return component;
        }

        private static void SimulateLeftClick(VisualElement element)
        {
            Assert.IsNotNull(element, "要點擊的 element 不可為 null。");
            Assert.IsNotNull(element.panel, "要點擊的 element.panel 不可為 null（UI 尚未初始化完成）。");

            int pointerId = PointerId.mousePointerId;
            const int leftButton = 0;
            const int pressedButtonsDown = 1;
            const int pressedButtonsUp = 0;

            Vector2 panelPosition = element.worldBound.center;
            Vector3 position = new Vector3(panelPosition.x, panelPosition.y, 0f);
            Vector2 localPanelPosition = element.WorldToLocal(panelPosition);
            Vector3 localPosition = new Vector3(localPanelPosition.x, localPanelPosition.y, 0f);

            var downPointer = new TestPointerEvent(pointerId, leftButton, position, localPosition, pressedButtonsDown);
            using (var downEvent = PointerDownEvent.GetPooled(downPointer))
            {
                element.SendEvent(downEvent);
            }

            var upPointer = new TestPointerEvent(pointerId, leftButton, position, localPosition, pressedButtonsUp);
            using (var upEvent = PointerUpEvent.GetPooled(upPointer))
            {
                element.SendEvent(upEvent);
            }
        }

        private sealed class TestPointerEvent : IPointerEvent
        {
            public int pointerId { get; }
            public string pointerType { get; }
            public bool isPrimary { get; }
            public int button { get; }
            public int pressedButtons { get; }
            public Vector3 position { get; }
            public Vector3 localPosition { get; }
            public Vector3 deltaPosition { get; }
            public float deltaTime { get; }
            public int clickCount { get; }
            public float pressure { get; }
            public float tangentialPressure { get; }
            public float altitudeAngle { get; }
            public float azimuthAngle { get; }
            public float twist { get; }
            public Vector2 tilt { get; }
            public PenStatus penStatus { get; }
            public Vector2 radius { get; }
            public Vector2 radiusVariance { get; }
            public EventModifiers modifiers { get; }

            public bool shiftKey => (modifiers & EventModifiers.Shift) != 0;
            public bool ctrlKey => (modifiers & EventModifiers.Control) != 0;
            public bool commandKey => (modifiers & EventModifiers.Command) != 0;
            public bool altKey => (modifiers & EventModifiers.Alt) != 0;
            public bool actionKey => Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer ? commandKey : ctrlKey;

            public Vector3 screenPosition { get; }
            public Vector3 screenDelta { get; }
            public Ray worldRay { get; }
            public UIDocument document { get; }
            public VisualElement elementTarget { get; }
            public VisualElement elementUnderPointer { get; }

            public TestPointerEvent(int pointerId, int button, Vector3 position, Vector3 localPosition, int pressedButtons)
            {
                this.pointerId = pointerId;
                pointerType = UnityEngine.UIElements.PointerType.mouse;
                isPrimary = true;
                this.button = button;
                this.pressedButtons = pressedButtons;
                this.position = position;
                this.localPosition = localPosition;
                deltaPosition = Vector3.zero;
                deltaTime = 0f;
                clickCount = 1;
                pressure = 0f;
                tangentialPressure = 0f;
                altitudeAngle = 0f;
                azimuthAngle = 0f;
                twist = 0f;
                tilt = Vector2.zero;
                penStatus = PenStatus.None;
                radius = Vector2.zero;
                radiusVariance = Vector2.zero;
                modifiers = EventModifiers.None;
                screenPosition = position;
                screenDelta = Vector3.zero;
                worldRay = new Ray(Vector3.zero, Vector3.forward);
                document = null;
                elementTarget = null;
                elementUnderPointer = null;
            }
        }
    }
}
// ===== 變更結束 =====
