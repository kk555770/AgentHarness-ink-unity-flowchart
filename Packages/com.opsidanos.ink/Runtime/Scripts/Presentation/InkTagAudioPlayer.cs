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
        // ===== 變更開始 =====
        // 2026/01/25 Opsidanos (修改原因：改用集中式 ResourceMap（JSON）做資源映射，避免每個元件各自維護 bindings)
        // 預期結果：bgm/se tag 只要提供 id，就能從同一份資源映射取得 AudioClip
        [SerializeField] private InkResourceMap resourceMap;
        // ===== 變更結束 =====
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

            // ===== 變更開始 =====
            // 2026/01/30 Opsidanos (修改原因：存讀檔需要能停止 BGM，避免讀檔後殘留上一段音樂)
            // 預期結果：收到 bgm:stop 時會停止並清空 BGM；讀檔回到沒有 BGM 的句子時不會繼續播放舊 BGM
            if (string.Equals(id, "stop", StringComparison.Ordinal))
            {
                bgmSource.Stop();
                bgmSource.clip = null;

                if (logBgm)
                {
                    Debug.Log("[OpsidanosInk][BGM] stop", this);
                }

                return;
            }
            // ===== 變更結束 =====

            // ===== 變更開始 =====
            // 2026/01/25 Opsidanos (修改原因：支援 ResourceMap；若未指定 ResourceMap 則沿用舊 bindings（相容）)
            // 預期結果：有指定 ResourceMap 時以 ResourceMap 為準；沒有時仍可用舊 bindings 驗證
            AudioClip clip;
            if (resourceMap != null)
            {
                if (!resourceMap.TryGetBgmClip(id, out clip))
                {
                    return;
                }
            }
            else
            {
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

                clip = binding.Clip;
            }
            // ===== 變更結束 =====

            if (bgmSource.clip == clip && bgmSource.isPlaying)
            {
                return;
            }

            bgmSource.clip = clip;
            bgmSource.Play();

            if (logBgm)
            {
                Debug.Log($"[OpsidanosInk][BGM] {id} -> {clip.name}", this);
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
            // ===== 變更開始 =====
            // 2026/01/25 Opsidanos (修改原因：支援 ResourceMap；若未指定 ResourceMap 則沿用舊 bindings（相容）)
            // 預期結果：有指定 ResourceMap 時以 ResourceMap 為準；沒有時仍可用舊 bindings 驗證
            AudioClip clip;
            if (resourceMap != null)
            {
                if (!resourceMap.TryGetSeClip(id, out clip))
                {
                    return;
                }
            }
            else
            {
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

                clip = binding.Clip;
            }
            // ===== 變更結束 =====

            seSource.PlayOneShot(clip);

            if (logSe)
            {
                Debug.Log($"[OpsidanosInk][SE] {id} -> {clip.name}", this);
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
