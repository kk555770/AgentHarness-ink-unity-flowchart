// ===== 變更開始 =====
// 2026/01/28 Opsidanos (修改原因：提供一鍵把 Demo resource_map.json 內用到的資源設成 Addressable，並把 Address 設成 JSON 的 address)
// 預期結果：不需要手動逐一勾 Addressable；點一次選單就能完成 Demo 的 Addressables 設定
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace OpsidanosInk.EditorTools
{
    public static class InkResourceMapAddressablesSetup
    {
        private const string MenuPath = "OpsidanosInk/Addressables/套用 Demo ResourceMap（自動勾 Addressable）";
        private const string DemoResourceMapJsonPath = "Assets/OpsidanosInk/Demo/resource_map.json";

        [Serializable]
        private sealed class ResourceMapPayload
        {
            public int version = 2;
            public TextureEntry[] bg;
            public AudioEntry[] bgm;
            public AudioEntry[] se;
            public TextureEntry[] cg;
            public TextureEntry[] character;
            public ActorEntry[] actors;
        }

        [Serializable]
        private sealed class TextureEntry
        {
            public string id;
            public string assetPath;
            public string address;
        }

        [Serializable]
        private sealed class AudioEntry
        {
            public string id;
            public string assetPath;
            public string address;
        }

        [Serializable]
        private sealed class ActorEntry
        {
            public string actor;
            public string defaultExpr;
            public ActorExpressionEntry[] expressions;
        }

        [Serializable]
        private sealed class ActorExpressionEntry
        {
            public string expr;
            public string assetPath;
            public string address;
        }

        private sealed class MarkItem
        {
            public string Kind;
            public string Id;
            public string AssetPath;
            public string Address;
        }

        [MenuItem(MenuPath)]
        private static void ApplyDemoResourceMapAddressables()
        {
            TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>(DemoResourceMapJsonPath);
            if (json == null)
            {
                Debug.LogError($"[OpsidanosInk][Addressables] 找不到 Demo ResourceMap：{DemoResourceMapJsonPath}");
                return;
            }

            ResourceMapPayload payload;
            try
            {
                payload = JsonUtility.FromJson<ResourceMapPayload>(json.text);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OpsidanosInk][Addressables] 解析 ResourceMap JSON 失敗。Error={ex.Message}");
                return;
            }

            if (payload == null)
            {
                Debug.LogError("[OpsidanosInk][Addressables] 解析 ResourceMap JSON 失敗（payload 為 null）。");
                return;
            }

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            if (settings == null)
            {
                Debug.LogError("[OpsidanosInk][Addressables] 找不到 AddressableAssetSettings（且建立失敗）。");
                return;
            }

            AddressableAssetGroup group = settings.DefaultGroup;
            if (group == null)
            {
                Debug.LogError("[OpsidanosInk][Addressables] Addressables 的 DefaultGroup 為 null。");
                return;
            }

            var items = new List<MarkItem>(128);
            AppendTextureEntries(items, "bg", payload.bg);
            AppendAudioEntries(items, "bgm", payload.bgm);
            AppendAudioEntries(items, "se", payload.se);
            AppendTextureEntries(items, "cg", payload.cg);
            AppendTextureEntries(items, "character", payload.character);
            AppendActorExpressionEntries(items, payload.actors);

            int createdCount = 0;
            int updatedAddressCount = 0;
            int unchangedCount = 0;
            int errorCount = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 0; i < items.Count; i++)
                {
                    MarkItem item = items[i];
                    if (item == null)
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(item.AssetPath))
                    {
                        Debug.LogError($"[OpsidanosInk][Addressables] {item.Kind} id=\"{item.Id}\" 的 assetPath 為空，已跳過。");
                        errorCount++;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(item.Address))
                    {
                        Debug.LogError(
                            $"[OpsidanosInk][Addressables] {item.Kind} id=\"{item.Id}\" 的 address 為空，請先補齊 {DemoResourceMapJsonPath}。assetPath=\"{item.AssetPath}\"");
                        errorCount++;
                        continue;
                    }

                    string assetPath = item.AssetPath.Trim();
                    string guid = AssetDatabase.AssetPathToGUID(assetPath);
                    if (string.IsNullOrWhiteSpace(guid))
                    {
                        Debug.LogError($"[OpsidanosInk][Addressables] 找不到資源：{item.Kind} id=\"{item.Id}\" assetPath=\"{assetPath}\"");
                        errorCount++;
                        continue;
                    }

                    AddressableAssetEntry entry = settings.FindAssetEntry(guid);
                    if (entry == null)
                    {
                        entry = settings.CreateOrMoveEntry(guid, group, false, false);
                        if (entry == null)
                        {
                            Debug.LogError($"[OpsidanosInk][Addressables] 建立 Addressables Entry 失敗：{item.Kind} id=\"{item.Id}\" guid=\"{guid}\"");
                            errorCount++;
                            continue;
                        }

                        createdCount++;
                    }

                    string desiredAddress = item.Address.Trim();
                    if (entry.address != desiredAddress)
                    {
                        entry.address = desiredAddress;
                        updatedAddressCount++;
                    }
                    else
                    {
                        unchangedCount++;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.SaveAssets();

            Debug.Log(
                $"[OpsidanosInk][Addressables] 完成：新增={createdCount} 更新地址={updatedAddressCount} 無變更={unchangedCount} 錯誤={errorCount}\n" +
                $"下一步：Addressables > Build > New Build > Default Build Script");
        }

        private static void AppendTextureEntries(List<MarkItem> items, string kind, TextureEntry[] entries)
        {
            if (items == null || entries == null || entries.Length == 0)
            {
                return;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                TextureEntry entry = entries[i];
                if (entry == null)
                {
                    continue;
                }

                items.Add(new MarkItem
                {
                    Kind = kind,
                    Id = entry.id,
                    AssetPath = entry.assetPath,
                    Address = entry.address
                });
            }
        }

        private static void AppendAudioEntries(List<MarkItem> items, string kind, AudioEntry[] entries)
        {
            if (items == null || entries == null || entries.Length == 0)
            {
                return;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                AudioEntry entry = entries[i];
                if (entry == null)
                {
                    continue;
                }

                items.Add(new MarkItem
                {
                    Kind = kind,
                    Id = entry.id,
                    AssetPath = entry.assetPath,
                    Address = entry.address
                });
            }
        }

        private static void AppendActorExpressionEntries(List<MarkItem> items, ActorEntry[] actors)
        {
            if (items == null || actors == null || actors.Length == 0)
            {
                return;
            }

            for (int i = 0; i < actors.Length; i++)
            {
                ActorEntry actor = actors[i];
                if (actor == null || actor.expressions == null || actor.expressions.Length == 0)
                {
                    continue;
                }

                string actorName = string.IsNullOrWhiteSpace(actor.actor) ? $"actors[{i}]" : actor.actor.Trim();
                for (int j = 0; j < actor.expressions.Length; j++)
                {
                    ActorExpressionEntry expr = actor.expressions[j];
                    if (expr == null)
                    {
                        continue;
                    }

                    string exprName = string.IsNullOrWhiteSpace(expr.expr) ? $"expressions[{j}]" : expr.expr.Trim();
                    items.Add(new MarkItem
                    {
                        Kind = "actors",
                        Id = $"{actorName}/{exprName}",
                        AssetPath = expr.assetPath,
                        Address = expr.address
                    });
                }
            }
        }
    }
}
// ===== 變更結束 =====

