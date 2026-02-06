// ===== 變更開始 =====
// 2026/02/04 Opsidanos (修改原因：新增 PlayMode 自動測試，避免每次都要手動按 UI 驗收存/讀/倒帶與倒退角色層級規則)
// 預期結果：在 Unity Test Runner（PlayMode）可一鍵驗證 Save/Load/Rollback 與 Restore 的角色層級規則（含 ForceComplete）
using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using OpsidanosInk.Runtime.Presentation;
using OpsidanosInk.Runtime.Save;
using OpsidanosInk.Runtime.Story;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace OpsidanosInk.Tests
{
    public sealed class OpsidanosInkPlayModeTests
    {
        private const string TestSceneName = "Test";

        [UnityTest]
        public IEnumerator SaveLoad_還原InkState_並送出Restore輸出()
        {
            yield return LoadTestScene();

            GameObject vnPlayer = FindVNPlayer();
            InkStoryEngine storyEngine = GetRequiredComponent<InkStoryEngine>(vnPlayer, "InkStoryEngine");
            InkSaveSystem saveSystem = GetRequiredComponent<InkSaveSystem>(vnPlayer, "InkSaveSystem");

            var outputs = new List<StoryOutput>();
            Action<StoryOutput> handler = output => outputs.Add(output);
            storyEngine.OutputGenerated += handler;

            try
            {
                yield return AdvanceStoryOneStep(storyEngine, outputs);
                Assert.AreEqual(StoryOutputSource.Normal, outputs[outputs.Count - 1].Source, "推進產生的輸出來源應為 Normal。");

                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string savedState), "應該能取得存檔前的 Ink state JSON。");
                saveSystem.SaveToSlot();

                yield return AdvanceStoryOneStep(storyEngine, outputs);

                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string advancedState), "應該能取得推進後的 Ink state JSON。");
                Assert.AreNotEqual(savedState, advancedState, "推進後狀態應該改變，否則此測試沒有意義。");

                int beforeLoadCount = outputs.Count;
                saveSystem.LoadFromSlot();
                yield return WaitForNewOutput(outputs, beforeLoadCount, 3f, "LoadFromSlot 應該送出一筆外部輸出");

                StoryOutput restoredOutput = outputs[outputs.Count - 1];
                Assert.AreEqual(StoryOutputSource.Restore, restoredOutput.Source, "讀檔輸出來源應為 Restore。");

                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string loadedState), "讀檔後應該能取得 Ink state JSON。");
                Assert.AreEqual(savedState, loadedState, "讀檔後 Ink state 應該回到存檔那一刻。");
            }
            finally
            {
                storyEngine.OutputGenerated -= handler;
            }
        }

        [UnityTest]
        public IEnumerator Rollback_回到上一句InkState_並送出Restore輸出()
        {
            yield return LoadTestScene();

            GameObject vnPlayer = FindVNPlayer();
            InkStoryEngine storyEngine = GetRequiredComponent<InkStoryEngine>(vnPlayer, "InkStoryEngine");
            InkSaveSystem saveSystem = GetRequiredComponent<InkSaveSystem>(vnPlayer, "InkSaveSystem");

            var outputs = new List<StoryOutput>();
            Action<StoryOutput> handler = output => outputs.Add(output);
            storyEngine.OutputGenerated += handler;

            try
            {
                yield return AdvanceStoryOneStep(storyEngine, outputs);
                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string stateBefore), "應該能取得倒帶前（目標）的 Ink state JSON。");

                yield return AdvanceStoryOneStep(storyEngine, outputs);
                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string stateAfterAdvance), "應該能取得推進後的 Ink state JSON。");
                Assert.AreNotEqual(stateBefore, stateAfterAdvance, "推進後狀態應該改變，否則此測試沒有意義。");

                int beforeRollbackCount = outputs.Count;
                saveSystem.RollbackOnce();
                yield return WaitForNewOutput(outputs, beforeRollbackCount, 3f, "RollbackOnce 應該送出一筆外部輸出");

                StoryOutput restoredOutput = outputs[outputs.Count - 1];
                Assert.AreEqual(StoryOutputSource.Restore, restoredOutput.Source, "倒帶輸出來源應為 Restore。");

                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string stateAfterRollback), "倒帶後應該能取得 Ink state JSON。");
                Assert.AreEqual(stateBefore, stateAfterRollback, "倒帶後 Ink state 應該回到上一句。");
            }
            finally
            {
                storyEngine.OutputGenerated -= handler;
            }
        }

        // ===== 變更開始 =====
        // 2026/02/06 Opsidanos (修改原因：文字遊戲引擎的讀檔後，必須仍可倒帶回存檔點之前)
        // 預期結果：LoadFromSlot 後 rollbackBuffer 仍保留歷史，TryRollbackOnce 可回到更早句子
        [UnityTest]
        public IEnumerator LoadFromSlot_仍可倒帶回存檔點之前()
        {
            yield return LoadTestScene();

            GameObject vnPlayer = FindVNPlayer();
            InkStoryEngine storyEngine = GetRequiredComponent<InkStoryEngine>(vnPlayer, "InkStoryEngine");
            InkSaveSystem saveSystem = GetRequiredComponent<InkSaveSystem>(vnPlayer, "InkSaveSystem");

            var outputs = new List<StoryOutput>();
            Action<StoryOutput> handler = output => outputs.Add(output);
            storyEngine.OutputGenerated += handler;

            try
            {
                yield return AdvanceStoryOneStep(storyEngine, outputs);
                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string stateAtStep1), "應該能取得第 1 句的 Ink state。");

                yield return AdvanceStoryOneStep(storyEngine, outputs);
                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string stateAtSavePoint), "應該能取得存檔點的 Ink state。");
                saveSystem.SaveToSlot();

                yield return AdvanceStoryOneStep(storyEngine, outputs);
                yield return AdvanceStoryOneStep(storyEngine, outputs);

                int beforeLoadCount = outputs.Count;
                saveSystem.LoadFromSlot();
                yield return WaitForNewOutput(outputs, beforeLoadCount, 3f, "LoadFromSlot 應該送出一筆 Restore 輸出");

                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string stateAfterLoad), "讀檔後應該能取得 Ink state。");
                Assert.AreEqual(stateAtSavePoint, stateAfterLoad, "讀檔後應該回到存檔點。");

                int beforeRollbackCount = outputs.Count;
                Assert.IsTrue(saveSystem.TryRollbackOnce(), "讀檔後應該還能倒帶到存檔點之前。");
                yield return WaitForNewOutput(outputs, beforeRollbackCount, 3f, "Rollback 應該送出一筆 Restore 輸出");

                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string stateAfterRollback), "倒帶後應該能取得 Ink state。");
                Assert.AreEqual(stateAtStep1, stateAfterRollback, "倒帶後應該可回到存檔點之前的句子。");
            }
            finally
            {
                storyEngine.OutputGenerated -= handler;
            }
        }
        // ===== 變更結束 =====

        [UnityTest]
        public IEnumerator CharLayer_Normal與Restore_固定重排規則相反_且ForceComplete不會破壞()
        {
            yield return LoadTestScene();

            GameObject vnPlayer = FindVNPlayer();
            InkStoryEngine storyEngine = GetRequiredComponent<InkStoryEngine>(vnPlayer, "InkStoryEngine");
            InkTagCharacterStatePlayer charStatePlayer = GetRequiredComponent<InkTagCharacterStatePlayer>(vnPlayer, "InkTagCharacterStatePlayer");
            UIDocument uiDocument = GetRequiredComponent<UIDocument>(vnPlayer, "UIDocument");

            yield return WaitUntilNotBusy(charStatePlayer, 3f);

            const string leftActor = "alice";
            const string rightActor = "bs";

            string instantCharJson = BuildCharJson(
                leftActor,
                rightActor,
                appearSeconds: 0f,
                moveSeconds: 0f,
                disappearSeconds: 0f);

            storyEngine.EmitExternalOutput(BuildCharOutput(9001, StoryOutputSource.Normal, instantCharJson));
            yield return null;
            yield return null;

            VisualElement actorLayer = GetCharacterActorLayer(uiDocument);
            Assert.AreEqual(rightActor, GetTopmostActorName(actorLayer, leftActor, rightActor), "Normal 時（固定重排）右邊應該在最上面。");

            const string centerActor = "ss";
            string slowAppearCharJson = BuildCharJson(
                leftActor,
                centerActor,
                rightActor,
                appearSeconds: 1.0f,
                moveSeconds: 0f,
                disappearSeconds: 0f);

            storyEngine.EmitExternalOutput(BuildCharOutput(9002, StoryOutputSource.Restore, slowAppearCharJson));
            yield return null;

            Assert.IsTrue(charStatePlayer.IsBusy, "Restore 的角色演出應該在播放中（用來測試 ForceComplete）。");
            charStatePlayer.ForceComplete();
            yield return null;

            Assert.IsFalse(charStatePlayer.IsBusy, "ForceComplete 後應該不再 Busy。");
            actorLayer = GetCharacterActorLayer(uiDocument);
            Assert.AreEqual(leftActor, GetTopmostActorName(actorLayer, leftActor, centerActor, rightActor), "Restore 時（固定重排）左邊應該在最上面，且 ForceComplete 不可破壞規則。");
        }

        // ===== 變更開始 =====
        // 2026/02/06 Opsidanos (修改原因：快速倒帶（Restore）遇到 steps 缺少 Appear 時，會產生 Error（missing Appear / raiseActors 找不到角色）)
        // 預期結果：Restore 缺少 Appear 時，系統會自動補齊且不產生紅字 Error
        [UnityTest]
        public IEnumerator Restore_缺少Appear時_不應產生CharTransitionStepsError()
        {
            yield return LoadTestScene();

            GameObject vnPlayer = FindVNPlayer();
            InkStoryEngine storyEngine = GetRequiredComponent<InkStoryEngine>(vnPlayer, "InkStoryEngine");
            InkTagCharacterStatePlayer charStatePlayer = GetRequiredComponent<InkTagCharacterStatePlayer>(vnPlayer, "InkTagCharacterStatePlayer");

            yield return WaitUntilNotBusy(charStatePlayer, 3f);

            const int outputId = 9101;
            string charJson =
                "{\"transition\":{\"appear\":0,\"move\":0,\"steps\":[{\"raiseActors\":[\"alice\"]},{\"actions\":[\"move\"],\"raise\":false}]},\"left\":{\"actor\":\"bs\"},\"center\":{\"actor\":\"alice\",\"expr\":\"happy\"},\"right\":{\"actor\":\"ss\"}}";

            var errors = new List<string>();
            Application.LogCallback handler = (condition, stackTrace, type) =>
            {
                if (type != LogType.Error)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(condition) && condition.Contains("[OpsidanosInk] char.transition.steps"))
                {
                    errors.Add(condition);
                }
            };

            Application.logMessageReceived += handler;
            try
            {
                storyEngine.EmitExternalOutput(BuildCharOutput(outputId, StoryOutputSource.Restore, charJson));
                yield return null;

                yield return WaitUntilNotBusy(charStatePlayer, 3f);
            }
            finally
            {
                Application.logMessageReceived -= handler;
            }

            Assert.AreEqual(0, errors.Count, $"Restore 缺少 Appear 時不應產生 char.transition.steps 的 Error。OutputId={outputId}");
        }
        // ===== 變更結束 =====

        // ===== 變更開始 =====
        // 2026/02/06 Opsidanos (修改原因：新增多槽 + Auto 槽實測，確保各槽位資料獨立且可正確讀回)
        // 預期結果：手動槽 1/2/3 與 Auto 槽讀回時，皆能回到各自存檔時刻的 Ink state
        [UnityTest]
        public IEnumerator MultiSlotAndAuto_可各自讀回正確InkState()
        {
            yield return LoadTestScene();

            GameObject vnPlayer = FindVNPlayer();
            InkStoryEngine storyEngine = GetRequiredComponent<InkStoryEngine>(vnPlayer, "InkStoryEngine");
            InkSaveSystem saveSystem = GetRequiredComponent<InkSaveSystem>(vnPlayer, "InkSaveSystem");

            var outputs = new List<StoryOutput>();
            Action<StoryOutput> handler = output => outputs.Add(output);
            storyEngine.OutputGenerated += handler;

            try
            {
                yield return AdvanceStoryOneStep(storyEngine, outputs);
                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string stateSlot1), "應該能取得手動槽 1 的 Ink state。");
                saveSystem.SaveToManualSlot(1);

                yield return AdvanceStoryOneStep(storyEngine, outputs);
                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string stateSlot2), "應該能取得手動槽 2 的 Ink state。");
                saveSystem.SaveToManualSlot(2);

                yield return AdvanceStoryOneStep(storyEngine, outputs);
                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string stateSlot3), "應該能取得手動槽 3 的 Ink state。");
                saveSystem.SaveToManualSlot(3);

                yield return AdvanceStoryOneStep(storyEngine, outputs);
                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string stateAuto), "應該能取得 Auto 槽的 Ink state。");
                saveSystem.SaveToAutoSlot();

                int beforeLoadCount = outputs.Count;
                saveSystem.LoadFromManualSlot(2);
                yield return WaitForNewOutput(outputs, beforeLoadCount, 3f, "LoadFromManualSlot(2) 應該送出一筆 Restore 輸出");
                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string loadedSlot2), "讀取手動槽 2 後應能取得 Ink state。");
                Assert.AreEqual(stateSlot2, loadedSlot2, "手動槽 2 讀回狀態不一致。");

                beforeLoadCount = outputs.Count;
                saveSystem.LoadFromAutoSlot();
                yield return WaitForNewOutput(outputs, beforeLoadCount, 3f, "LoadFromAutoSlot 應該送出一筆 Restore 輸出");
                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string loadedAuto), "讀取 Auto 槽後應能取得 Ink state。");
                Assert.AreEqual(stateAuto, loadedAuto, "Auto 槽讀回狀態不一致。");

                beforeLoadCount = outputs.Count;
                saveSystem.LoadFromManualSlot(1);
                yield return WaitForNewOutput(outputs, beforeLoadCount, 3f, "LoadFromManualSlot(1) 應該送出一筆 Restore 輸出");
                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string loadedSlot1), "讀取手動槽 1 後應能取得 Ink state。");
                Assert.AreEqual(stateSlot1, loadedSlot1, "手動槽 1 讀回狀態不一致。");

                beforeLoadCount = outputs.Count;
                saveSystem.LoadFromManualSlot(3);
                yield return WaitForNewOutput(outputs, beforeLoadCount, 3f, "LoadFromManualSlot(3) 應該送出一筆 Restore 輸出");
                Assert.IsTrue(storyEngine.TryGetStoryStateJson(out string loadedSlot3), "讀取手動槽 3 後應能取得 Ink state。");
                Assert.AreEqual(stateSlot3, loadedSlot3, "手動槽 3 讀回狀態不一致。");
            }
            finally
            {
                storyEngine.OutputGenerated -= handler;
            }
        }
        // ===== 變更結束 =====

        private static IEnumerator LoadTestScene()
        {
            SceneManager.LoadScene(TestSceneName, LoadSceneMode.Single);
            yield return null;
            yield return null;
        }

        private static IEnumerator AdvanceStoryOneStep(InkStoryEngine storyEngine, List<StoryOutput> outputs)
        {
            int before = outputs.Count;
            storyEngine.Continue();
            yield return WaitForNewOutput(outputs, before, 3f, "Continue() 後應該產生新輸出");

            StoryOutput last = outputs[outputs.Count - 1];
            if (last.Choices != null && last.Choices.Count > 0)
            {
                before = outputs.Count;
                storyEngine.ChooseChoice(last.Choices[0].Index);
                yield return WaitForNewOutput(outputs, before, 3f, "ChooseChoice(0) 後應該產生新輸出");
            }
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

        private static IEnumerator WaitUntilNotBusy(InkTagCharacterStatePlayer player, float timeoutSeconds)
        {
            float start = Time.realtimeSinceStartup;
            while (player != null && player.IsBusy)
            {
                if (Time.realtimeSinceStartup - start > timeoutSeconds)
                {
                    Assert.Fail("等待角色演出結束逾時（IsBusy 一直為 true）。");
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

        private static string BuildCharJson(string leftActor, string rightActor, float appearSeconds, float moveSeconds, float disappearSeconds)
        {
            return BuildCharJson(leftActor, null, rightActor, appearSeconds, moveSeconds, disappearSeconds);
        }

        private static string BuildCharJson(string leftActor, string centerActor, string rightActor, float appearSeconds, float moveSeconds, float disappearSeconds)
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

        private static VisualElement GetCharacterActorLayer(UIDocument uiDocument)
        {
            VisualElement root = uiDocument != null ? uiDocument.rootVisualElement : null;
            Assert.IsNotNull(root, "UIDocument.rootVisualElement 不可為 null。");

            VisualElement actorLayer = root.Q<VisualElement>("CharacterActorLayer");
            Assert.IsNotNull(actorLayer, "找不到 CharacterActorLayer。請確認 CharacterTag 已成功建立角色層。");
            return actorLayer;
        }

        private static string GetTopmostActorName(VisualElement actorLayer, params string[] actors)
        {
            Assert.IsNotNull(actorLayer, "actorLayer 不可為 null。");

            Assert.IsNotNull(actors, "actors 不可為 null。");
            Assert.GreaterOrEqual(actors.Length, 2, "至少要提供 2 個 actor 才能比較層級。");

            string topmostActor = null;
            int topmostIndex = int.MinValue;

            for (int i = 0; i < actors.Length; i++)
            {
                string actor = actors[i];
                Assert.IsFalse(string.IsNullOrWhiteSpace(actor), "actor 名稱不可為空白。");

                VisualElement element = actorLayer.Q<VisualElement>($"Actor_{actor}");
                Assert.IsNotNull(element, $"找不到角色元素 Actor_{actor}。");

                int index = GetChildIndex(actorLayer, element);
                Assert.GreaterOrEqual(index, 0, $"Actor_{actor} 必須在 CharacterActorLayer 底下。");

                if (index > topmostIndex)
                {
                    topmostIndex = index;
                    topmostActor = actor;
                }
            }

            Assert.IsNotNull(topmostActor, "topmostActor 不可為 null。");
            return topmostActor;
        }

        private static int GetChildIndex(VisualElement parent, VisualElement child)
        {
            if (parent == null || child == null)
            {
                return -1;
            }

            int index = 0;
            foreach (VisualElement element in parent.Children())
            {
                if (element == child)
                {
                    return index;
                }

                index++;
            }

            return -1;
        }
    }
}
// ===== 變更結束 =====
