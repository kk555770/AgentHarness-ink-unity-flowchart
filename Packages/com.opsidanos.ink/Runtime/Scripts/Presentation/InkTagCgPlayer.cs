// ===== 變更開始 =====
// 2026/01/25 Opsidanos (修改原因：新增 Tag → CG 切換綁定，讓 cg/cg:clear 可以直接驅動 CG 層)
// 預期結果：收到 cg tag 時顯示/切換 CG；收到 cg:clear 時隱藏 CG；並輸出 log 方便驗證
using System;
using System.Collections.Generic;
using OpsidanosInk.Runtime.Story;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpsidanosInk.Runtime.Presentation
{
    public sealed class InkTagCgPlayer : MonoBehaviour
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
        // 預期結果：cg tag 只要提供 id，就能從同一份資源映射取得 Texture2D
        [SerializeField] private InkResourceMap resourceMap;
        // ===== 變更結束 =====

        [Header("UXML")]
        [SerializeField] private string cgLayerElementName = "CgLayer";
        [SerializeField] private string cgImageElementName = "CgImage";

        [Header("Bindings")]
        [SerializeField] private List<TextureBinding> cgBindings = new List<TextureBinding>();

        [Header("Debug")]
        [SerializeField] private bool logCg = true;

        private VisualElement cgLayerElement;
        private VisualElement cgImageElement;

        private void Awake()
        {
            if (tagEventRouter == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagCgPlayer 尚未指定 Tag Event Router。", this);
                enabled = false;
                return;
            }

            if (uiDocument == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagCgPlayer 尚未指定 UIDocument。", this);
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

            tagEventRouter.CgTagReceived += OnCgTagReceived;
        }

        private void OnDisable()
        {
            if (tagEventRouter == null)
            {
                return;
            }

            tagEventRouter.CgTagReceived -= OnCgTagReceived;
        }

        private void OnCgTagReceived(StoryOutput output, InkTag tag)
        {
            if (tag == null || !tag.HasValue || string.IsNullOrWhiteSpace(tag.Value))
            {
                Debug.LogError("[OpsidanosInk] InkTagCgPlayer 收到不合法的 cg tag（需要值）。", this);
                return;
            }

            string id = tag.Value;

            if (string.Equals(id, "clear", StringComparison.Ordinal))
            {
                HideCg();
                return;
            }

            // ===== 變更開始 =====
            // 2026/01/25 Opsidanos (修改原因：支援 ResourceMap；若未指定 ResourceMap 則沿用舊 bindings（相容）)
            // 預期結果：有指定 ResourceMap 時以 ResourceMap 為準；沒有時仍可用舊 bindings 驗證
            Texture2D texture;
            if (resourceMap != null)
            {
                if (!resourceMap.TryGetCgTexture(id, out texture))
                {
                    return;
                }
            }
            else
            {
                TextureBinding binding = FindBinding(cgBindings, id);
                if (binding == null)
                {
                    Debug.LogError($"[OpsidanosInk] InkTagCgPlayer 找不到 CG id=\"{id}\" 的對照設定。", this);
                    return;
                }

                if (binding.Texture == null)
                {
                    Debug.LogError($"[OpsidanosInk] InkTagCgPlayer 的 CG id=\"{id}\" 尚未指定 Texture2D。", this);
                    return;
                }

                texture = binding.Texture;
            }
            // ===== 變更結束 =====

            VisualElement layer = GetCgLayerElement();
            if (layer == null)
            {
                return;
            }

            VisualElement image = GetCgImageElement();
            if (image == null)
            {
                return;
            }

            image.style.backgroundImage = new StyleBackground(texture);
            layer.RemoveFromClassList("vn-hidden");

            if (logCg)
            {
                Debug.Log($"[OpsidanosInk][CG] {id} -> {texture.name}", this);
            }
        }

        private void HideCg()
        {
            VisualElement layer = GetCgLayerElement();
            if (layer == null)
            {
                return;
            }

            VisualElement image = GetCgImageElement();
            if (image == null)
            {
                return;
            }

            image.style.backgroundImage = new StyleBackground((Texture2D)null);
            layer.AddToClassList("vn-hidden");

            if (logCg)
            {
                Debug.Log("[OpsidanosInk][CG] clear", this);
            }
        }

        private VisualElement GetCgLayerElement()
        {
            if (cgLayerElement != null)
            {
                return cgLayerElement;
            }

            cgLayerElement = GetElement(cgLayerElementName);
            return cgLayerElement;
        }

        private VisualElement GetCgImageElement()
        {
            if (cgImageElement != null)
            {
                return cgImageElement;
            }

            cgImageElement = GetElement(cgImageElementName);
            return cgImageElement;
        }

        private VisualElement GetElement(string elementName)
        {
            VisualElement root = uiDocument != null ? uiDocument.rootVisualElement : null;
            if (root == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagCgPlayer 找不到 rootVisualElement。", this);
                return null;
            }

            VisualElement element = root.Q<VisualElement>(elementName);
            if (element == null)
            {
                Debug.LogError($"[OpsidanosInk] InkTagCgPlayer 找不到 name=\"{elementName}\" 的 CG 元素。", this);
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
