// ===== 變更開始 =====
// 2026/01/25 Opsidanos (修改原因：提供可檢驗的 ResourceMap 測試，確保 Demo resource_map.json 能被解析並成功載入各類資源)
// 預期結果：跑 EditMode Tests 時，能驗證 bg/bgm/se/cg/character/actors 的 id 都能從 ResourceMap 取得對應資源
using NUnit.Framework;
using OpsidanosInk.Runtime.Presentation;
using UnityEditor;
using UnityEngine;

namespace OpsidanosInk.Tests
{
    public sealed class InkResourceMapTests
    {
        [Test]
        public void InkResourceMap_DemoJson_AllKindsAreLoadable()
        {
            const string jsonPath = "Assets/OpsidanosInk/Demo/resource_map.json";

            TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonPath);
            Assert.IsNotNull(json, $"找不到測試用 resource_map.json：{jsonPath}");

            var go = new GameObject("InkResourceMapTests");
            try
            {
                InkResourceMap map = go.AddComponent<InkResourceMap>();

                var so = new SerializedObject(map);
                so.FindProperty("resourceMapJson").objectReferenceValue = json;
                so.FindProperty("logLoad").boolValue = false;
                so.ApplyModifiedPropertiesWithoutUndo();

                Assert.IsTrue(map.EnsureLoaded(), "EnsureLoaded 應該要成功。");

                Assert.IsTrue(map.TryGetBackgroundTexture("room_01", out Texture2D bg));
                Assert.IsNotNull(bg);

                Assert.IsTrue(map.TryGetCgTexture("alice_happy_cg", out Texture2D cg));
                Assert.IsNotNull(cg);

                Assert.IsTrue(map.TryGetCharacterTexture("alice_normal", out Texture2D charTexture));
                Assert.IsNotNull(charTexture);

                Assert.IsTrue(map.TryGetBgmClip("opening", out AudioClip bgm));
                Assert.IsNotNull(bgm);

                Assert.IsTrue(map.TryGetSeClip("open", out AudioClip se));
                Assert.IsNotNull(se);

                Assert.IsTrue(map.TryGetActorExpressionTexture("alice", "happy", out Texture2D actorExprTexture, out string resolvedExpr));
                Assert.AreEqual("happy", resolvedExpr);
                Assert.IsNotNull(actorExprTexture);

                Assert.IsTrue(map.TryGetActorExpressionTexture("alice", null, out Texture2D actorDefaultTexture, out string resolvedDefaultExpr));
                Assert.AreEqual("normal", resolvedDefaultExpr, "alice 的 defaultExpr 應該是 normal。");
                Assert.IsNotNull(actorDefaultTexture);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
// ===== 變更結束 =====

