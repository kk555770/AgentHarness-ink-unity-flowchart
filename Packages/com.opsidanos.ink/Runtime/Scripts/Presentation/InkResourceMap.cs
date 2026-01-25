// ===== 變更開始 =====
// 2026/01/25 Opsidanos (修改原因：把資源映射集中成一份 JSON（TextAsset），讓 Flow Chart 未來能用管線化方式輸出並被 Runtime 讀取)
// 預期結果：bg/bgm/se/cg/char 的 id 對照不再分散在各元件 Inspector；只要更新一份 JSON 就能驅動整個演出系統
using System;
using System.Collections.Generic;
using UnityEngine;

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
            public int version = 1;
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
        }

        [Serializable]
        private sealed class AudioBinding
        {
            public string id;
            public string assetPath;
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
        }

        private sealed class ActorEntry
        {
            public string DefaultExpr;
            public Dictionary<string, string> ExprPathByExpr;
        }

        [Header("Refs")]
        [SerializeField] private TextAsset resourceMapJson;

        [Header("Debug")]
        [SerializeField] private bool logLoad;

        private bool isLoaded;
        private Dictionary<string, string> backgroundPathById;
        private Dictionary<string, string> cgPathById;
        private Dictionary<string, string> characterPathById;
        private Dictionary<string, string> bgmPathById;
        private Dictionary<string, string> sePathById;
        private Dictionary<string, ActorEntry> actorById;

        private readonly Dictionary<string, Texture2D> textureCacheByAssetPath =
            new Dictionary<string, Texture2D>(StringComparer.Ordinal);

        private readonly Dictionary<string, AudioClip> audioCacheByAssetPath =
            new Dictionary<string, AudioClip>(StringComparer.Ordinal);

        private void Awake()
        {
            EnsureLoaded();
        }

        public bool TryGetBackgroundTexture(string id, out Texture2D texture)
        {
            return TryGetTexture(backgroundPathById, "bg", id, out texture);
        }

        public bool TryGetCgTexture(string id, out Texture2D texture)
        {
            return TryGetTexture(cgPathById, "cg", id, out texture);
        }

        public bool TryGetCharacterTexture(string id, out Texture2D texture)
        {
            return TryGetTexture(characterPathById, "character", id, out texture);
        }

        public bool TryGetBgmClip(string id, out AudioClip clip)
        {
            return TryGetAudioClip(bgmPathById, "bgm", id, out clip);
        }

        public bool TryGetSeClip(string id, out AudioClip clip)
        {
            return TryGetAudioClip(sePathById, "se", id, out clip);
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

            if (entry.ExprPathByExpr == null || !entry.ExprPathByExpr.TryGetValue(exprKey, out string assetPath) || string.IsNullOrWhiteSpace(assetPath))
            {
                Debug.LogError($"[OpsidanosInk] InkResourceMap 找不到 actor=\"{actorKey}\" expr=\"{exprKey}\" 的貼圖對照。", this);
                return false;
            }

            if (textureCacheByAssetPath.TryGetValue(assetPath, out texture) && texture != null)
            {
                return true;
            }

            texture = LoadAsset<Texture2D>("actorExpr", $"{actorKey}/{exprKey}", assetPath);
            if (texture == null)
            {
                return false;
            }

            textureCacheByAssetPath[assetPath] = texture;
            return true;
        }

        public bool EnsureLoaded()
        {
            if (isLoaded)
            {
                return true;
            }

            backgroundPathById = new Dictionary<string, string>(StringComparer.Ordinal);
            cgPathById = new Dictionary<string, string>(StringComparer.Ordinal);
            characterPathById = new Dictionary<string, string>(StringComparer.Ordinal);
            bgmPathById = new Dictionary<string, string>(StringComparer.Ordinal);
            sePathById = new Dictionary<string, string>(StringComparer.Ordinal);
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

            AddTextureBindings(backgroundPathById, payload.bg, "bg");
            AddTextureBindings(cgPathById, payload.cg, "cg");
            AddTextureBindings(characterPathById, payload.character, "character");
            AddAudioBindings(bgmPathById, payload.bgm, "bgm");
            AddAudioBindings(sePathById, payload.se, "se");
            AddActors(payload.actors);

            isLoaded = true;

            if (logLoad)
            {
                Debug.Log(
                    $"[OpsidanosInk][ResourceMap] loaded version={payload.version} bg={backgroundPathById.Count} cg={cgPathById.Count} char={characterPathById.Count} bgm={bgmPathById.Count} se={sePathById.Count} actors={actorById.Count}",
                    this);
            }

            return true;
        }

        private void AddTextureBindings(Dictionary<string, string> target, TextureBinding[] bindings, string kind)
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

                target.Add(id, binding.assetPath.Trim());
            }
        }

        private void AddAudioBindings(Dictionary<string, string> target, AudioBinding[] bindings, string kind)
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

                target.Add(id, binding.assetPath.Trim());
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
                    ExprPathByExpr = new Dictionary<string, string>(StringComparer.Ordinal)
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
                    if (entry.ExprPathByExpr.ContainsKey(exprId))
                    {
                        Debug.LogError($"[OpsidanosInk] InkResourceMap.actors actor=\"{actorId}\" 出現重複 expr：\"{exprId}\"。", this);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(expr.assetPath))
                    {
                        Debug.LogError($"[OpsidanosInk] InkResourceMap.actors actor=\"{actorId}\" expr=\"{exprId}\" 的 assetPath 不可為空。", this);
                        continue;
                    }

                    entry.ExprPathByExpr.Add(exprId, expr.assetPath.Trim());
                }

                if (!string.IsNullOrWhiteSpace(entry.DefaultExpr) && !entry.ExprPathByExpr.ContainsKey(entry.DefaultExpr))
                {
                    Debug.LogError($"[OpsidanosInk] InkResourceMap.actors actor=\"{actorId}\" 的 defaultExpr=\"{entry.DefaultExpr}\" 找不到對應 expression。", this);
                }

                actorById.Add(actorId, entry);
            }
        }

        private bool TryGetTexture(Dictionary<string, string> pathById, string kind, string id, out Texture2D texture)
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

            if (pathById == null || !pathById.TryGetValue(key, out string assetPath) || string.IsNullOrWhiteSpace(assetPath))
            {
                Debug.LogError($"[OpsidanosInk] InkResourceMap 找不到 {kind} id=\"{key}\" 的 assetPath。", this);
                return false;
            }

            if (textureCacheByAssetPath.TryGetValue(assetPath, out texture) && texture != null)
            {
                return true;
            }

            texture = LoadAsset<Texture2D>(kind, key, assetPath);
            if (texture == null)
            {
                return false;
            }

            textureCacheByAssetPath[assetPath] = texture;
            return true;
        }

        private bool TryGetAudioClip(Dictionary<string, string> pathById, string kind, string id, out AudioClip clip)
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

            if (pathById == null || !pathById.TryGetValue(key, out string assetPath) || string.IsNullOrWhiteSpace(assetPath))
            {
                Debug.LogError($"[OpsidanosInk] InkResourceMap 找不到 {kind} id=\"{key}\" 的 assetPath。", this);
                return false;
            }

            if (audioCacheByAssetPath.TryGetValue(assetPath, out clip) && clip != null)
            {
                return true;
            }

            clip = LoadAsset<AudioClip>(kind, key, assetPath);
            if (clip == null)
            {
                return false;
            }

            audioCacheByAssetPath[assetPath] = clip;
            return true;
        }

        private T LoadAsset<T>(string kind, string id, string assetPath) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                Debug.LogError($"[OpsidanosInk] InkResourceMap 載入資源失敗：kind={kind} id=\"{id}\" path=\"{assetPath}\" type={typeof(T).Name}", this);
                return null;
            }

            return asset;
#else
            Debug.LogError($"[OpsidanosInk] InkResourceMap 在 Player build 目前無法用 assetPath 載入資源（需要 Addressables/Resources）。kind={kind} id=\"{id}\" path=\"{assetPath}\"", this);
            return null;
#endif
        }
    }
}
// ===== 變更結束 =====
