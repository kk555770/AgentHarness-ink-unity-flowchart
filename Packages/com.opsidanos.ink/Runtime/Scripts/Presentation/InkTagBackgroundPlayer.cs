// ===== 變更開始 =====
// 2026/01/25 Opsidanos (修改原因：新增 Tag → 背景切換綁定，讓 bg 可以直接驅動 UI Toolkit 背景)
// 預期結果：收到 bg tag 時切換 VNPlayer 背景圖，並輸出 log 方便驗證
using System;
using System.Collections.Generic;
using OpsidanosInk.Runtime.Story;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpsidanosInk.Runtime.Presentation
{
    public sealed class InkTagBackgroundPlayer : MonoBehaviour
    {
        [Serializable]
        private sealed class SpriteBinding
        {
            public string Id;
            public Sprite Sprite;
        }

        [Header("Refs")]
        [SerializeField] private InkTagEventRouter tagEventRouter;
        [SerializeField] private UIDocument uiDocument;

        [Header("UXML")]
        [SerializeField] private string backgroundElementName = "Background";

        [Header("Bindings")]
        [SerializeField] private List<SpriteBinding> backgroundBindings = new List<SpriteBinding>();

        [Header("Debug")]
        [SerializeField] private bool logBackground = true;

        private VisualElement backgroundElement;

        private void Awake()
        {
            if (tagEventRouter == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagBackgroundPlayer 尚未指定 Tag Event Router。", this);
                enabled = false;
                return;
            }

            if (uiDocument == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagBackgroundPlayer 尚未指定 UIDocument。", this);
                enabled = false;
                return;
            }
        }

        private void OnEnable()
        {
            if (tagEventRouter == null)
            {
                return;
            }

            tagEventRouter.BackgroundTagReceived += OnBackgroundTagReceived;
        }

        private void OnDisable()
        {
            if (tagEventRouter == null)
            {
                return;
            }

            tagEventRouter.BackgroundTagReceived -= OnBackgroundTagReceived;
        }

        private void OnBackgroundTagReceived(StoryOutput output, InkTag tag)
        {
            if (tag == null || !tag.HasValue || string.IsNullOrWhiteSpace(tag.Value))
            {
                Debug.LogError("[OpsidanosInk] InkTagBackgroundPlayer 收到不合法的 bg tag（需要值）。", this);
                return;
            }

            string id = tag.Value;

            SpriteBinding binding = FindBinding(backgroundBindings, id);
            if (binding == null)
            {
                Debug.LogError($"[OpsidanosInk] InkTagBackgroundPlayer 找不到 BG id=\"{id}\" 的對照設定。", this);
                return;
            }

            if (binding.Sprite == null)
            {
                Debug.LogError($"[OpsidanosInk] InkTagBackgroundPlayer 的 BG id=\"{id}\" 尚未指定 Sprite。", this);
                return;
            }

            VisualElement element = GetBackgroundElement();
            if (element == null)
            {
                return;
            }

            element.style.backgroundImage = new StyleBackground(binding.Sprite);

            if (logBackground)
            {
                Debug.Log($"[OpsidanosInk][BG] {id} -> {binding.Sprite.name}", this);
            }
        }

        private VisualElement GetBackgroundElement()
        {
            if (backgroundElement != null)
            {
                return backgroundElement;
            }

            VisualElement root = uiDocument != null ? uiDocument.rootVisualElement : null;
            if (root == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagBackgroundPlayer 找不到 rootVisualElement。", this);
                return null;
            }

            backgroundElement = root.Q<VisualElement>(backgroundElementName);
            if (backgroundElement == null)
            {
                Debug.LogError($"[OpsidanosInk] InkTagBackgroundPlayer 找不到 name=\"{backgroundElementName}\" 的背景元素。", this);
                return null;
            }

            return backgroundElement;
        }

        private static SpriteBinding FindBinding(List<SpriteBinding> bindings, string id)
        {
            if (bindings == null)
            {
                return null;
            }

            for (int i = 0; i < bindings.Count; i++)
            {
                SpriteBinding binding = bindings[i];
                if (binding == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(binding.Id))
                {
                    continue;
                }

                if (!string.Equals(binding.Id, id, StringComparison.Ordinal))
                {
                    continue;
                }

                return binding;
            }

            return null;
        }
    }
}
// ===== 變更結束 =====
