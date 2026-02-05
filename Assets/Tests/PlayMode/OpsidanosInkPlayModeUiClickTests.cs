// ===== 變更開始 =====
// 2026/02/05 Opsidanos (修改原因：新增 PlayMode 的 UI 點擊測試，確保 VNPlayer.uxml 的按鈕接線與狀態切換正常)
// 預期結果：在 Unity Test Runner（PlayMode）會真的「點」Backlog/Auto/Skip/Hide/Show 按鈕並驗證 UI 狀態
using System.Collections;
using NUnit.Framework;
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
