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
        // ===== 變更開始 =====
        // 2026/01/25 Opsidanos (修改原因：改用集中式 ResourceMap（JSON）做資源映射，避免每個元件各自維護 bindings)
        // 預期結果：char-left/center/right tag 只要提供 id，就能從同一份資源映射取得 Texture2D
        [SerializeField] private InkResourceMap resourceMap;
        // ===== 變更結束 =====

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

            // ===== 變更開始 =====
            // 2026/01/25 Opsidanos (修改原因：支援 ResourceMap；若未指定 ResourceMap 則沿用舊 bindings（相容）)
            // 預期結果：有指定 ResourceMap 時以 ResourceMap 為準；沒有時仍可用舊 bindings 驗證
            Texture2D texture;
            if (resourceMap != null)
            {
                if (!resourceMap.TryGetCharacterTexture(id, out texture))
                {
                    return;
                }
            }
            else
            {
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

                texture = binding.Texture;
            }
            // ===== 變更結束 =====

            slotElement.style.backgroundImage = new StyleBackground(texture);

            if (logCharacter)
            {
                Debug.Log($"[OpsidanosInk][Char] {slotName} {id} -> {texture.name}", this);
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
