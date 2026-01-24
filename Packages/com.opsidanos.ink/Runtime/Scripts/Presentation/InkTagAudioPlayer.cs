// ===== 變更開始 =====
// 2026/01/24 Opsidanos (修改原因：新增 Tag → 音訊播放綁定，讓 bgm/se 可以直接驅動 AudioSource)
// 預期結果：收到 bgm/se tag 時播放對應音樂/音效，並輸出 log 方便驗證
using System;
using System.Collections.Generic;
using OpsidanosInk.Runtime.Story;
using UnityEngine;

namespace OpsidanosInk.Runtime.Presentation
{
    public sealed class InkTagAudioPlayer : MonoBehaviour
    {
        [Serializable]
        private sealed class ClipBinding
        {
            public string Id;
            public AudioClip Clip;
        }

        [Header("Refs")]
        [SerializeField] private InkTagEventRouter tagEventRouter;
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource seSource;

        [Header("Bindings")]
        [SerializeField] private List<ClipBinding> bgmBindings = new List<ClipBinding>();
        [SerializeField] private List<ClipBinding> seBindings = new List<ClipBinding>();

        [Header("Debug")]
        [SerializeField] private bool logBgm = true;
        [SerializeField] private bool logSe = true;

        private void Awake()
        {
            if (tagEventRouter == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagAudioPlayer 尚未指定 Tag Event Router。", this);
                enabled = false;
                return;
            }

            if (bgmSource == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagAudioPlayer 尚未指定 Bgm Source。", this);
                enabled = false;
                return;
            }

            if (seSource == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagAudioPlayer 尚未指定 Se Source。", this);
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

            tagEventRouter.BgmTagReceived += OnBgmTagReceived;
            tagEventRouter.SeTagReceived += OnSeTagReceived;
        }

        private void OnDisable()
        {
            if (tagEventRouter == null)
            {
                return;
            }

            tagEventRouter.BgmTagReceived -= OnBgmTagReceived;
            tagEventRouter.SeTagReceived -= OnSeTagReceived;
        }

        private void OnBgmTagReceived(StoryOutput output, InkTag tag)
        {
            if (tag == null || !tag.HasValue || string.IsNullOrWhiteSpace(tag.Value))
            {
                Debug.LogError("[OpsidanosInk] InkTagAudioPlayer 收到不合法的 bgm tag（需要值）。", this);
                return;
            }

            string id = tag.Value;

            ClipBinding binding = FindBinding(bgmBindings, id);
            if (binding == null)
            {
                Debug.LogError($"[OpsidanosInk] InkTagAudioPlayer 找不到 BGM id=\"{id}\" 的對照設定。", this);
                return;
            }

            if (binding.Clip == null)
            {
                Debug.LogError($"[OpsidanosInk] InkTagAudioPlayer 的 BGM id=\"{id}\" 尚未指定 AudioClip。", this);
                return;
            }

            if (bgmSource.clip == binding.Clip && bgmSource.isPlaying)
            {
                return;
            }

            bgmSource.clip = binding.Clip;
            bgmSource.Play();

            if (logBgm)
            {
                Debug.Log($"[OpsidanosInk][BGM] {id} -> {binding.Clip.name}", this);
            }
        }

        private void OnSeTagReceived(StoryOutput output, InkTag tag)
        {
            if (tag == null || !tag.HasValue || string.IsNullOrWhiteSpace(tag.Value))
            {
                Debug.LogError("[OpsidanosInk] InkTagAudioPlayer 收到不合法的 se tag（需要值）。", this);
                return;
            }

            string id = tag.Value;

            ClipBinding binding = FindBinding(seBindings, id);
            if (binding == null)
            {
                Debug.LogError($"[OpsidanosInk] InkTagAudioPlayer 找不到 SE id=\"{id}\" 的對照設定。", this);
                return;
            }

            if (binding.Clip == null)
            {
                Debug.LogError($"[OpsidanosInk] InkTagAudioPlayer 的 SE id=\"{id}\" 尚未指定 AudioClip。", this);
                return;
            }

            seSource.PlayOneShot(binding.Clip);

            if (logSe)
            {
                Debug.Log($"[OpsidanosInk][SE] {id} -> {binding.Clip.name}", this);
            }
        }

        private static ClipBinding FindBinding(List<ClipBinding> bindings, string id)
        {
            if (bindings == null)
            {
                return null;
            }

            for (int i = 0; i < bindings.Count; i++)
            {
                ClipBinding binding = bindings[i];
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
