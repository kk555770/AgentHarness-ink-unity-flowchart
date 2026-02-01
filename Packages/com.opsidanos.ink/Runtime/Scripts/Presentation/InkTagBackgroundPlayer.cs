// ===== 變更開始 =====
// 2026/01/25 Opsidanos (修改原因：新增 Tag → 背景切換綁定，並改用 Texture2D 以符合目前 BG 資源型別)
// 預期結果：收到 bg tag 時切換 VNPlayer 背景圖，並輸出 log 方便驗證
using System;
using System.Collections.Generic;
using OpsidanosInk.Runtime.Story;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace OpsidanosInk.Runtime.Presentation
{
    public sealed class InkTagBackgroundPlayer : MonoBehaviour
    {
        [Serializable]
        private sealed class SpriteBinding
        {
            public string Id;
            [FormerlySerializedAs("Sprite")]
            public Texture2D Texture;
        }

        [Header("Refs")]
        [SerializeField] private InkTagEventRouter tagEventRouter;
        [SerializeField] private UIDocument uiDocument;
        // ===== 變更開始 =====
        // 2026/01/25 Opsidanos (修改原因：改用集中式 ResourceMap（JSON）做資源映射，避免每個元件各自維護 bindings)
        // 預期結果：bg tag 只要提供 id，就能從同一份資源映射取得 Texture2D
        [SerializeField] private InkResourceMap resourceMap;
        // ===== 變更結束 =====

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

            // ===== 變更開始 =====
            // 2026/01/31 Opsidanos (修改原因：存讀檔需要能把背景還原為「空白」，所以增加 bg:clear 支援；並修正變數命名衝突造成的編譯錯誤)
            // 預期結果：收到 bg:clear 時背景圖會被清空，且此檔案可正常編譯
            if (string.Equals(id, "clear", StringComparison.Ordinal))
            {
                VisualElement background = GetBackgroundElement();
                if (background == null)
                {
                    return;
                }

                background.style.backgroundImage = new StyleBackground((Texture2D)null);

                if (logBackground)
                {
                    Debug.Log("[OpsidanosInk][BG] clear", this);
                }

                return;
            }
            // ===== 變更結束 =====

            // ===== 變更開始 =====
            // 2026/01/25 Opsidanos (修改原因：支援 ResourceMap；若未指定 ResourceMap 則沿用舊 bindings（相容）)
            // 預期結果：有指定 ResourceMap 時以 ResourceMap 為準；沒有時仍可用舊 bindings 驗證
            Texture2D texture;
            if (resourceMap != null)
            {
                if (!resourceMap.TryGetBackgroundTexture(id, out texture))
                {
                    return;
                }
            }
            else
            {
                SpriteBinding binding = FindBinding(backgroundBindings, id);
                if (binding == null)
                {
                    Debug.LogError($"[OpsidanosInk] InkTagBackgroundPlayer 找不到 BG id=\"{id}\" 的對照設定。", this);
                    return;
                }

                if (binding.Texture == null)
                {
                    Debug.LogError($"[OpsidanosInk] InkTagBackgroundPlayer 的 BG id=\"{id}\" 尚未指定 Texture2D。", this);
                    return;
                }

                texture = binding.Texture;
            }
            // ===== 變更結束 =====

            VisualElement element = GetBackgroundElement();
            if (element == null)
            {
                return;
            }

            element.style.backgroundImage = new StyleBackground(texture);

            if (logBackground)
            {
                Debug.Log($"[OpsidanosInk][BG] {id} -> {texture.name}", this);
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
