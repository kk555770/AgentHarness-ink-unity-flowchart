// ===== 變更開始 =====
// 2026/01/28 Opsidanos (修改原因：確保 Demo resource_map.json 已補齊 Addressables 用的 address 欄位，避免只在打包後才發現資源載入失敗)
// 預期結果：跑 EditMode Tests 時，能驗證 resource_map.json 的 bg/bgm/se/cg/character/actors 全部都有 address
using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace OpsidanosInk.Tests
{
    public sealed class InkResourceMapAddressablesTests
    {
        [Serializable]
        private sealed class Payload
        {
            public int version;
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

        [Test]
        public void InkResourceMap_DemoJson_AllEntriesHaveAddress()
        {
            const string jsonPath = "Assets/OpsidanosInk/Demo/resource_map.json";

            TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonPath);
            Assert.IsNotNull(json, $"找不到測試用 resource_map.json：{jsonPath}");

            Payload payload = JsonUtility.FromJson<Payload>(json.text);
            Assert.IsNotNull(payload, "resource_map.json 應該能被 JsonUtility 解析。");
            Assert.GreaterOrEqual(payload.version, 2, "resource_map.json 的 version 應該至少為 2（含 address 欄位）。");

            AssertAllHasAddress("bg", payload.bg);
            AssertAllHasAddress("bgm", payload.bgm);
            AssertAllHasAddress("se", payload.se);
            AssertAllHasAddress("cg", payload.cg);
            AssertAllHasAddress("character", payload.character);
            AssertActorsHasAddress(payload.actors);
        }

        private static void AssertAllHasAddress(string kind, TextureEntry[] entries)
        {
            Assert.NotNull(entries, $"{kind} 不可為 null。");
            for (int i = 0; i < entries.Length; i++)
            {
                TextureEntry entry = entries[i];
                Assert.NotNull(entry, $"{kind}[{i}] 不可為 null。");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.id), $"{kind}[{i}] 的 id 不可為空。");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.address), $"{kind}[{i}] id=\"{entry.id}\" 的 address 不可為空。");
            }
        }

        private static void AssertAllHasAddress(string kind, AudioEntry[] entries)
        {
            Assert.NotNull(entries, $"{kind} 不可為 null。");
            for (int i = 0; i < entries.Length; i++)
            {
                AudioEntry entry = entries[i];
                Assert.NotNull(entry, $"{kind}[{i}] 不可為 null。");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.id), $"{kind}[{i}] 的 id 不可為空。");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.address), $"{kind}[{i}] id=\"{entry.id}\" 的 address 不可為空。");
            }
        }

        private static void AssertActorsHasAddress(ActorEntry[] actors)
        {
            Assert.NotNull(actors, "actors 不可為 null。");
            for (int i = 0; i < actors.Length; i++)
            {
                ActorEntry actor = actors[i];
                Assert.NotNull(actor, $"actors[{i}] 不可為 null。");
                Assert.IsFalse(string.IsNullOrWhiteSpace(actor.actor), $"actors[{i}] 的 actor 不可為空。");

                Assert.NotNull(actor.expressions, $"actors[{i}] actor=\"{actor.actor}\" 的 expressions 不可為 null。");
                for (int j = 0; j < actor.expressions.Length; j++)
                {
                    ActorExpressionEntry expr = actor.expressions[j];
                    Assert.NotNull(expr, $"actors[{i}] actor=\"{actor.actor}\" expressions[{j}] 不可為 null。");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(expr.expr), $"actors[{i}] actor=\"{actor.actor}\" expressions[{j}] 的 expr 不可為空。");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(expr.address), $"actors[{i}] actor=\"{actor.actor}\" expr=\"{expr.expr}\" 的 address 不可為空。");
                }
            }
        }
    }
}
// ===== 變更結束 =====
