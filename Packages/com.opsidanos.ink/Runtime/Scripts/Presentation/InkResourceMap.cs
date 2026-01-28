// ===== 變更開始 =====
// 2026/01/25 Opsidanos (修改原因：把資源映射集中成一份 JSON（TextAsset），讓 Flow Chart 未來能用管線化方式輸出並被 Runtime 讀取)
// 2026/01/28 Opsidanos (修改原因：ResourceMap 支援 Addressables address，讓 Player build 也能載入演出資源)
// 預期結果：bg/bgm/se/cg/char 的 id 對照不再分散在各元件 Inspector；Editor 用 assetPath；Player build 用 Addressables address
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OpsidanosInk.Runtime.Presentation
{
    public sealed class InkResourceMap : MonoBehaviour
    {
        [Serializable]
        private sealed class ResourceMapPayload
        {
            public int version = 2;
            public TextureBinding[] bg;
            public AudioBinding[] bgm;
            public AudioBinding[] se;
            public TextureBinding[] cg;
            public TextureBinding[] character;
            public ActorBinding[] actors;
        }

        [Serializable]
        private sealed class TextureBinding
        {
            public string id;
            public string assetPath;
            public string address;
        }

        [Serializable]
        private sealed class AudioBinding
        {
            public string id;
            public string assetPath;
            public string address;
        }

        [Serializable]
        private sealed class ActorBinding
        {
            public string actor;
            public string defaultExpr;
            public ActorExpressionBinding[] expressions;
        }

        [Serializable]
        private sealed class ActorExpressionBinding
        {
            public string expr;
            public string assetPath;
            public string address;
        }

        private sealed class ResourceLocation
        {
            public string AssetPath;
            public string Address;
        }

        private sealed class ActorEntry
        {
            public string DefaultExpr;
            public Dictionary<string, ResourceLocation> ExprLocationByExpr;
        }

        [Header("Refs")]
        [SerializeField] private TextAsset resourceMapJson;

        [Header("Debug")]
        [SerializeField] private bool logLoad;

        private bool isLoaded;
        private Dictionary<string, ResourceLocation> backgroundLocationById;
        private Dictionary<string, ResourceLocation> cgLocationById;
        private Dictionary<string, ResourceLocation> characterLocationById;
        private Dictionary<string, ResourceLocation> bgmLocationById;
        private Dictionary<string, ResourceLocation> seLocationById;
        private Dictionary<string, ActorEntry> actorById;

        private readonly Dictionary<string, Texture2D> textureCacheByLoadKey =
            new Dictionary<string, Texture2D>(StringComparer.Ordinal);

        private readonly Dictionary<string, AudioClip> audioCacheByLoadKey =
            new Dictionary<string, AudioClip>(StringComparer.Ordinal);

#if !UNITY_EDITOR
        private readonly Dictionary<string, AsyncOperationHandle> addressableHandleByLoadKey =
            new Dictionary<string, AsyncOperationHandle>(StringComparer.Ordinal);
#endif

        private void Awake()
        {
            EnsureLoaded();
        }

        private void OnDestroy()
        {
#if !UNITY_EDITOR
            foreach (AsyncOperationHandle handle in addressableHandleByLoadKey.Values)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }

            addressableHandleByLoadKey.Clear();
#endif
        }

        public bool TryGetBackgroundTexture(string id, out Texture2D texture)
        {
            return TryGetTexture(backgroundLocationById, "bg", id, out texture);
        }

        public bool TryGetCgTexture(string id, out Texture2D texture)
        {
            return TryGetTexture(cgLocationById, "cg", id, out texture);
        }

        public bool TryGetCharacterTexture(string id, out Texture2D texture)
        {
            return TryGetTexture(characterLocationById, "character", id, out texture);
        }

        public bool TryGetBgmClip(string id, out AudioClip clip)
        {
            return TryGetAudioClip(bgmLocationById, "bgm", id, out clip);
        }

        public bool TryGetSeClip(string id, out AudioClip clip)
        {
            return TryGetAudioClip(seLocationById, "se", id, out clip);
        }

        public bool TryGetActorExpressionTexture(string actor, string expression, out Texture2D texture, out string resolvedExpression)
        {
            texture = null;
            resolvedExpression = null;

            if (!EnsureLoaded())
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(actor))
            {
                Debug.LogError("[OpsidanosInk] InkResourceMap.TryGetActorExpressionTexture 收到空白 actor。", this);
                return false;
            }

            string actorKey = actor.Trim();
            if (!actorById.TryGetValue(actorKey, out ActorEntry entry) || entry == null)
            {
                Debug.LogError($"[OpsidanosInk] InkResourceMap 找不到 actor=\"{actorKey}\" 的對照。", this);
                return false;
            }

            string exprKey;
            if (string.IsNullOrWhiteSpace(expression))
            {
                if (string.IsNullOrWhiteSpace(entry.DefaultExpr))
                {
                    Debug.LogError($"[OpsidanosInk] InkResourceMap actor=\"{actorKey}\" 沒有 defaultExpr，且此處 expr 也未提供。", this);
                    return false;
                }

                exprKey = entry.DefaultExpr;
            }
            else
            {
                exprKey = expression.Trim();
            }

            resolvedExpression = exprKey;

            if (entry.ExprLocationByExpr == null ||
                !entry.ExprLocationByExpr.TryGetValue(exprKey, out ResourceLocation location) ||
                location == null)
            {
                Debug.LogError($"[OpsidanosInk] InkResourceMap 找不到 actor=\"{actorKey}\" expr=\"{exprKey}\" 的貼圖對照。", this);
                return false;
            }

            if (!TryGetLoadKey(location, "actorExpr", $"{actorKey}/{exprKey}", out string loadKey))
            {
                return false;
            }

            if (textureCacheByLoadKey.TryGetValue(loadKey, out texture) && texture != null)
            {
                return true;
            }

            texture = LoadAsset<Texture2D>("actorExpr", $"{actorKey}/{exprKey}", location);
            if (texture == null)
            {
                return false;
            }

            textureCacheByLoadKey[loadKey] = texture;
            return true;
        }

        public bool EnsureLoaded()
        {
            if (isLoaded)
            {
                return true;
            }

            backgroundLocationById = new Dictionary<string, ResourceLocation>(StringComparer.Ordinal);
            cgLocationById = new Dictionary<string, ResourceLocation>(StringComparer.Ordinal);
            characterLocationById = new Dictionary<string, ResourceLocation>(StringComparer.Ordinal);
            bgmLocationById = new Dictionary<string, ResourceLocation>(StringComparer.Ordinal);
            seLocationById = new Dictionary<string, ResourceLocation>(StringComparer.Ordinal);
            actorById = new Dictionary<string, ActorEntry>(StringComparer.Ordinal);

            if (resourceMapJson == null)
            {
                Debug.LogError("[OpsidanosInk] InkResourceMap 尚未指定 Resource Map Json（TextAsset）。", this);
                return false;
            }

            ResourceMapPayload payload;
            try
            {
                payload = JsonUtility.FromJson<ResourceMapPayload>(resourceMapJson.text);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OpsidanosInk] InkResourceMap 解析 JSON 失敗。Error={ex.Message}", this);
                return false;
            }

            if (payload == null)
            {
                Debug.LogError("[OpsidanosInk] InkResourceMap 解析 JSON 失敗（payload 為 null）。", this);
                return false;
            }

            AddTextureBindings(backgroundLocationById, payload.bg, "bg");
            AddTextureBindings(cgLocationById, payload.cg, "cg");
            AddTextureBindings(characterLocationById, payload.character, "character");
            AddAudioBindings(bgmLocationById, payload.bgm, "bgm");
            AddAudioBindings(seLocationById, payload.se, "se");
            AddActors(payload.actors);

            isLoaded = true;

            if (logLoad)
            {
                Debug.Log(
                    $"[OpsidanosInk][ResourceMap] loaded version={payload.version} bg={backgroundLocationById.Count} cg={cgLocationById.Count} char={characterLocationById.Count} bgm={bgmLocationById.Count} se={seLocationById.Count} actors={actorById.Count}",
                    this);
            }

            return true;
        }

        private void AddTextureBindings(Dictionary<string, ResourceLocation> target, TextureBinding[] bindings, string kind)
        {
            if (bindings == null || bindings.Length == 0)
            {
                return;
            }

            for (int i = 0; i < bindings.Length; i++)
            {
                TextureBinding binding = bindings[i];
                if (binding == null)
                {
                    Debug.LogError($"[OpsidanosInk] InkResourceMap.{kind}[{i}] 是 null。", this);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(binding.id))
                {
                    Debug.LogError($"[OpsidanosInk] InkResourceMap.{kind}[{i}] 的 id 不可為空。", this);
                    continue;
                }

                string id = binding.id.Trim();
                if (target.ContainsKey(id))
                {
                    Debug.LogError($"[OpsidanosInk] InkResourceMap.{kind} 出現重複 id：\"{id}\"。", this);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(binding.assetPath))
                {
                    Debug.LogError($"[OpsidanosInk] InkResourceMap.{kind}[{i}] id=\"{id}\" 的 assetPath 不可為空。", this);
                    continue;
                }

                target.Add(
                    id,
                    new ResourceLocation
                    {
                        AssetPath = binding.assetPath.Trim(),
                        Address = string.IsNullOrWhiteSpace(binding.address) ? null : binding.address.Trim()
                    });
            }
        }

        private void AddAudioBindings(Dictionary<string, ResourceLocation> target, AudioBinding[] bindings, string kind)
        {
            if (bindings == null || bindings.Length == 0)
            {
                return;
            }

            for (int i = 0; i < bindings.Length; i++)
            {
                AudioBinding binding = bindings[i];
                if (binding == null)
                {
                    Debug.LogError($"[OpsidanosInk] InkResourceMap.{kind}[{i}] 是 null。", this);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(binding.id))
                {
                    Debug.LogError($"[OpsidanosInk] InkResourceMap.{kind}[{i}] 的 id 不可為空。", this);
                    continue;
                }

                string id = binding.id.Trim();
                if (target.ContainsKey(id))
                {
                    Debug.LogError($"[OpsidanosInk] InkResourceMap.{kind} 出現重複 id：\"{id}\"。", this);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(binding.assetPath))
                {
                    Debug.LogError($"[OpsidanosInk] InkResourceMap.{kind}[{i}] id=\"{id}\" 的 assetPath 不可為空。", this);
                    continue;
                }

                target.Add(
                    id,
                    new ResourceLocation
                    {
                        AssetPath = binding.assetPath.Trim(),
                        Address = string.IsNullOrWhiteSpace(binding.address) ? null : binding.address.Trim()
                    });
            }
        }

        private void AddActors(ActorBinding[] actors)
        {
            if (actors == null || actors.Length == 0)
            {
                return;
            }

            for (int i = 0; i < actors.Length; i++)
            {
                ActorBinding actor = actors[i];
                if (actor == null)
                {
                    Debug.LogError($"[OpsidanosInk] InkResourceMap.actors[{i}] 是 null。", this);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(actor.actor))
                {
                    Debug.LogError($"[OpsidanosInk] InkResourceMap.actors[{i}] 的 actor 不可為空。", this);
                    continue;
                }

                string actorId = actor.actor.Trim();
                if (actorById.ContainsKey(actorId))
                {
                    Debug.LogError($"[OpsidanosInk] InkResourceMap.actors 出現重複 actor：\"{actorId}\"。", this);
                    continue;
                }

                var entry = new ActorEntry
                {
                    DefaultExpr = string.IsNullOrWhiteSpace(actor.defaultExpr) ? null : actor.defaultExpr.Trim(),
                    ExprLocationByExpr = new Dictionary<string, ResourceLocation>(StringComparer.Ordinal)
                };

                if (actor.expressions == null || actor.expressions.Length == 0)
                {
                    Debug.LogError($"[OpsidanosInk] InkResourceMap.actors actor=\"{actorId}\" 沒有任何 expressions。", this);
                    actorById.Add(actorId, entry);
                    continue;
                }

                for (int j = 0; j < actor.expressions.Length; j++)
                {
                    ActorExpressionBinding expr = actor.expressions[j];
                    if (expr == null)
                    {
                        Debug.LogError($"[OpsidanosInk] InkResourceMap.actors actor=\"{actorId}\" expressions[{j}] 是 null。", this);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(expr.expr))
                    {
                        Debug.LogError($"[OpsidanosInk] InkResourceMap.actors actor=\"{actorId}\" expressions[{j}] 的 expr 不可為空。", this);
                        continue;
                    }

                    string exprId = expr.expr.Trim();
                    if (entry.ExprLocationByExpr.ContainsKey(exprId))
                    {
                        Debug.LogError($"[OpsidanosInk] InkResourceMap.actors actor=\"{actorId}\" 出現重複 expr：\"{exprId}\"。", this);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(expr.assetPath))
                    {
                        Debug.LogError($"[OpsidanosInk] InkResourceMap.actors actor=\"{actorId}\" expr=\"{exprId}\" 的 assetPath 不可為空。", this);
                        continue;
                    }

                    entry.ExprLocationByExpr.Add(
                        exprId,
                        new ResourceLocation
                        {
                            AssetPath = expr.assetPath.Trim(),
                            Address = string.IsNullOrWhiteSpace(expr.address) ? null : expr.address.Trim()
                        });
                }

                if (!string.IsNullOrWhiteSpace(entry.DefaultExpr) && !entry.ExprLocationByExpr.ContainsKey(entry.DefaultExpr))
                {
                    Debug.LogError($"[OpsidanosInk] InkResourceMap.actors actor=\"{actorId}\" 的 defaultExpr=\"{entry.DefaultExpr}\" 找不到對應 expression。", this);
                }

                actorById.Add(actorId, entry);
            }
        }

        private bool TryGetLoadKey(ResourceLocation location, string kind, string id, out string loadKey)
        {
#if UNITY_EDITOR
            loadKey = location == null ? null : location.AssetPath;
            if (string.IsNullOrWhiteSpace(loadKey))
            {
                Debug.LogError($"[OpsidanosInk] InkResourceMap 找不到 {kind} id=\"{id}\" 的 assetPath。", this);
                return false;
            }
#else
            loadKey = location == null ? null : location.Address;
            if (string.IsNullOrWhiteSpace(loadKey))
            {
                Debug.LogError(
                    $"[OpsidanosInk] InkResourceMap 在 Player build 載入失敗：kind={kind} id=\"{id}\" 的 address 為空。請在 resource_map.json 填入 address，並把該資源設為 Addressable。",
                    this);
                return false;
            }
#endif

            loadKey = loadKey.Trim();
            return true;
        }

        private bool TryGetTexture(Dictionary<string, ResourceLocation> locationById, string kind, string id, out Texture2D texture)
        {
            texture = null;

            if (!EnsureLoaded())
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogError($"[OpsidanosInk] InkResourceMap 收到不合法的 {kind} id（空白）。", this);
                return false;
            }

            string key = id.Trim();

            if (locationById == null || !locationById.TryGetValue(key, out ResourceLocation location) || location == null)
            {
                Debug.LogError($"[OpsidanosInk] InkResourceMap 找不到 {kind} id=\"{key}\" 的對照。", this);
                return false;
            }

            if (!TryGetLoadKey(location, kind, key, out string loadKey))
            {
                return false;
            }

            if (textureCacheByLoadKey.TryGetValue(loadKey, out texture) && texture != null)
            {
                return true;
            }

            texture = LoadAsset<Texture2D>(kind, key, location);
            if (texture == null)
            {
                return false;
            }

            textureCacheByLoadKey[loadKey] = texture;
            return true;
        }

        private bool TryGetAudioClip(Dictionary<string, ResourceLocation> locationById, string kind, string id, out AudioClip clip)
        {
            clip = null;

            if (!EnsureLoaded())
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogError($"[OpsidanosInk] InkResourceMap 收到不合法的 {kind} id（空白）。", this);
                return false;
            }

            string key = id.Trim();

            if (locationById == null || !locationById.TryGetValue(key, out ResourceLocation location) || location == null)
            {
                Debug.LogError($"[OpsidanosInk] InkResourceMap 找不到 {kind} id=\"{key}\" 的對照。", this);
                return false;
            }

            if (!TryGetLoadKey(location, kind, key, out string loadKey))
            {
                return false;
            }

            if (audioCacheByLoadKey.TryGetValue(loadKey, out clip) && clip != null)
            {
                return true;
            }

            clip = LoadAsset<AudioClip>(kind, key, location);
            if (clip == null)
            {
                return false;
            }

            audioCacheByLoadKey[loadKey] = clip;
            return true;
        }

        private T LoadAsset<T>(string kind, string id, ResourceLocation location) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (location == null || string.IsNullOrWhiteSpace(location.AssetPath))
            {
                Debug.LogError($"[OpsidanosInk] InkResourceMap 找不到 {kind} id=\"{id}\" 的 assetPath。", this);
                return null;
            }

            string assetPath = location.AssetPath.Trim();
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                Debug.LogError($"[OpsidanosInk] InkResourceMap 載入資源失敗：kind={kind} id=\"{id}\" assetPath=\"{assetPath}\" type={typeof(T).Name}", this);
                return null;
            }

            return asset;
#else
            if (location == null || string.IsNullOrWhiteSpace(location.Address))
            {
                Debug.LogError(
                    $"[OpsidanosInk] InkResourceMap 在 Player build 載入失敗：kind={kind} id=\"{id}\" 的 address 為空。請在 resource_map.json 填入 address，並把該資源設為 Addressable。",
                    this);
                return null;
            }

            string address = location.Address.Trim();

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
            T asset = handle.WaitForCompletion();
            if (handle.Status != AsyncOperationStatus.Succeeded || asset == null)
            {
                string errorMessage = handle.OperationException != null ? handle.OperationException.Message : "Unknown";
                Debug.LogError(
                    $"[OpsidanosInk] InkResourceMap 透過 Addressables 載入資源失敗：kind={kind} id=\"{id}\" address=\"{address}\" type={typeof(T).Name} error={errorMessage}",
                    this);
                Addressables.Release(handle);
                return null;
            }

            if (!addressableHandleByLoadKey.ContainsKey(address))
            {
                addressableHandleByLoadKey.Add(address, handle);
            }

            return asset;
#endif
        }
    }
}
// ===== 變更結束 =====
