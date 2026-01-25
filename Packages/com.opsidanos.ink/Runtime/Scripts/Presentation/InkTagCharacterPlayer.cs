// ===== 變更開始 =====
// 2026/01/25 Opsidanos (修改原因：新增 Tag → 立繪切換綁定，讓 char-left/center/right 可以直接驅動角色層)
// 預期結果：收到 char-left/center/right tag 時，對應槽位會切換立繪，並輸出 log 方便驗證
using System;
using System.Collections.Generic;
using OpsidanosInk.Runtime.Story;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpsidanosInk.Runtime.Presentation
{
    public sealed class InkTagCharacterPlayer : MonoBehaviour
    {
        [Serializable]
        private sealed class TextureBinding
        {
            public string Id;
            public Texture2D Texture;
        }

        [Header("Refs")]
        [SerializeField] private InkTagEventRouter tagEventRouter;
        [SerializeField] private UIDocument uiDocument;

        [Header("UXML")]
        [SerializeField] private string characterLeftElementName = "CharacterLeft";
        [SerializeField] private string characterCenterElementName = "CharacterCenter";
        [SerializeField] private string characterRightElementName = "CharacterRight";

        [Header("Bindings")]
        [SerializeField] private List<TextureBinding> characterBindings = new List<TextureBinding>();

        [Header("Debug")]
        [SerializeField] private bool logCharacter = true;

        private VisualElement characterLeftElement;
        private VisualElement characterCenterElement;
        private VisualElement characterRightElement;

        private void Awake()
        {
            if (tagEventRouter == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagCharacterPlayer 尚未指定 Tag Event Router。", this);
                enabled = false;
                return;
            }

            if (uiDocument == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagCharacterPlayer 尚未指定 UIDocument。", this);
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

            tagEventRouter.CharacterLeftTagReceived += OnCharacterLeftTagReceived;
            tagEventRouter.CharacterCenterTagReceived += OnCharacterCenterTagReceived;
            tagEventRouter.CharacterRightTagReceived += OnCharacterRightTagReceived;
        }

        private void OnDisable()
        {
            if (tagEventRouter == null)
            {
                return;
            }

            tagEventRouter.CharacterLeftTagReceived -= OnCharacterLeftTagReceived;
            tagEventRouter.CharacterCenterTagReceived -= OnCharacterCenterTagReceived;
            tagEventRouter.CharacterRightTagReceived -= OnCharacterRightTagReceived;
        }

        private void OnCharacterLeftTagReceived(StoryOutput output, InkTag tag)
        {
            ApplyToSlot("left", GetCharacterLeftElement(), tag);
        }

        private void OnCharacterCenterTagReceived(StoryOutput output, InkTag tag)
        {
            ApplyToSlot("center", GetCharacterCenterElement(), tag);
        }

        private void OnCharacterRightTagReceived(StoryOutput output, InkTag tag)
        {
            ApplyToSlot("right", GetCharacterRightElement(), tag);
        }

        private void ApplyToSlot(string slotName, VisualElement slotElement, InkTag tag)
        {
            if (slotElement == null)
            {
                return;
            }

            if (tag == null || !tag.HasValue || string.IsNullOrWhiteSpace(tag.Value))
            {
                Debug.LogError($"[OpsidanosInk] InkTagCharacterPlayer 收到不合法的 char-{slotName} tag（需要值）。", this);
                return;
            }

            string id = tag.Value;
            if (string.Equals(id, "clear", StringComparison.Ordinal))
            {
                slotElement.style.backgroundImage = new StyleBackground((Texture2D)null);

                if (logCharacter)
                {
                    Debug.Log($"[OpsidanosInk][Char] {slotName} clear", this);
                }

                return;
            }

            TextureBinding binding = FindBinding(characterBindings, id);
            if (binding == null)
            {
                Debug.LogError($"[OpsidanosInk] InkTagCharacterPlayer 找不到 Char id=\"{id}\" 的對照設定。", this);
                return;
            }

            if (binding.Texture == null)
            {
                Debug.LogError($"[OpsidanosInk] InkTagCharacterPlayer 的 Char id=\"{id}\" 尚未指定 Texture2D。", this);
                return;
            }

            slotElement.style.backgroundImage = new StyleBackground(binding.Texture);

            if (logCharacter)
            {
                Debug.Log($"[OpsidanosInk][Char] {slotName} {id} -> {binding.Texture.name}", this);
            }
        }

        private VisualElement GetCharacterLeftElement()
        {
            if (characterLeftElement != null)
            {
                return characterLeftElement;
            }

            characterLeftElement = GetElement(characterLeftElementName);
            return characterLeftElement;
        }

        private VisualElement GetCharacterCenterElement()
        {
            if (characterCenterElement != null)
            {
                return characterCenterElement;
            }

            characterCenterElement = GetElement(characterCenterElementName);
            return characterCenterElement;
        }

        private VisualElement GetCharacterRightElement()
        {
            if (characterRightElement != null)
            {
                return characterRightElement;
            }

            characterRightElement = GetElement(characterRightElementName);
            return characterRightElement;
        }

        private VisualElement GetElement(string elementName)
        {
            VisualElement root = uiDocument != null ? uiDocument.rootVisualElement : null;
            if (root == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagCharacterPlayer 找不到 rootVisualElement。", this);
                return null;
            }

            VisualElement element = root.Q<VisualElement>(elementName);
            if (element == null)
            {
                Debug.LogError($"[OpsidanosInk] InkTagCharacterPlayer 找不到 name=\"{elementName}\" 的角色槽位元素。", this);
                return null;
            }

            return element;
        }

        private static TextureBinding FindBinding(List<TextureBinding> bindings, string id)
        {
            if (bindings == null)
            {
                return null;
            }

            for (int i = 0; i < bindings.Count; i++)
            {
                TextureBinding binding = bindings[i];
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
