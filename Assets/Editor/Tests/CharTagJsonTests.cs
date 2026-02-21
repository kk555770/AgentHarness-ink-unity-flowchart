// ===== 變更開始 =====
// 2026/01/25 Opsidanos (修改原因：提供可檢驗的 char JSON tag 測試，確保 Ink Runtime 輸出的 tag 值可被 JSON 解析)
// 預期結果：跑 EditMode Tests 時，能驗證 Demo story 的 `char:<json>` 逐句輸出符合預期（含 transition.steps、move=1、move=0、clear）
using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace OpsidanosInk.Tests
{
    public sealed class CharTagJsonTests
    {
        [Serializable]
        private sealed class Payload
        {
            public Transition transition;
            public Slot left;
            public Slot center;
            public Slot right;
        }

        [Serializable]
        private sealed class Transition
        {
            public float appear = -1f;
            public float disappear = -1f;
            public float fade = -1f;
            public float move = -1f;
            public bool instant;
            public Step[] steps;
        }

        [Serializable]
        private sealed class Step
        {
            public string[] actions;
            public bool raise = true;
            public string[] raiseActors;
        }

        [Serializable]
        private sealed class Slot
        {
            public string actor;
            public string expr;
        }

        [Test]
        public void CharTagJson_DemoStory_SequenceIsParsable()
        {
            string jsonPath = Path.Combine(Application.dataPath, "OpsidanosInk/Demo/story.json");
            Assert.IsTrue(File.Exists(jsonPath), $"找不到測試用 story.json：{jsonPath}");

            string storyJson = File.ReadAllText(jsonPath);
            var story = new Ink.Runtime.Story(storyJson);

            story.Continue();
            story.Continue();

            Assert.GreaterOrEqual(story.currentChoices.Count, 1, "Demo story 應該至少有一個選項（角色狀態測試）。");
            story.ChooseChoiceIndex(0);

            // 1) alice 中間 normal、bs 左邊
            story.Continue();
            Payload p1 = ParseCharPayloadFromCurrentTags(story);
            Assert.AreEqual("bs", p1.left.actor);
            Assert.AreEqual("alice", p1.center.actor);
            Assert.AreEqual("normal", p1.center.expr);

            // 2) ss 中間（appear=0.5，steps=[appear+move]）、alice 右邊（normal）
            story.Continue();
            Payload p2 = ParseCharPayloadFromCurrentTags(story);
            Assert.IsNotNull(p2.transition);
            Assert.AreEqual(0.5f, p2.transition.appear);
            Assert.IsNotNull(p2.transition.steps);
            Assert.AreEqual(1, p2.transition.steps.Length);
            CollectionAssert.AreEqual(new[] { "appear", "move" }, p2.transition.steps[0].actions);
            Assert.IsTrue(p2.transition.steps[0].raise);
            Assert.IsTrue(p2.transition.steps[0].raiseActors == null || p2.transition.steps[0].raiseActors.Length == 0);
            Assert.AreEqual("bs", p2.left.actor);
            Assert.AreEqual("ss", p2.center.actor);
            Assert.AreEqual("alice", p2.right.actor);
            Assert.AreEqual("normal", p2.right.expr);

            // 3) move=1，alice 中間 happy、bs 左邊、ss 右邊
            story.Continue();
            Payload p3 = ParseCharPayloadFromCurrentTags(story);
            Assert.IsNotNull(p3.transition);
            Assert.AreEqual(1f, p3.transition.move);
            Assert.AreEqual("bs", p3.left.actor);
            Assert.AreEqual("alice", p3.center.actor);
            Assert.AreEqual("happy", p3.center.expr);
            Assert.AreEqual("ss", p3.right.actor);

            // 4) move=1，bs 右邊、ss 左邊、alice 中間 happy
            story.Continue();
            Payload p4 = ParseCharPayloadFromCurrentTags(story);
            Assert.IsNotNull(p4.transition);
            Assert.AreEqual(1f, p4.transition.move);
            Assert.AreEqual("ss", p4.left.actor);
            Assert.AreEqual("alice", p4.center.actor);
            Assert.AreEqual("happy", p4.center.expr);
            Assert.AreEqual("bs", p4.right.actor);

            // ===== 變更開始 =====
            // 2026/02/22 Opsidanos (修改原因：契約要求只要有 steps 就必須完整安排 appear/move/disappear，且 raiseActors 要落在可用角色時機)
            // 預期結果：Demo 第 5~7 句 payload 符合可重播契約，Rollback/快速連點不再觸發「缺少必要動作」與 raiseActors 紅字
            // 5) appear=0, move=1, disappear=0，steps=[appear(raiseActors=alice,raise=false)] -> [move(raise=false)] -> [disappear(raise=false)]
            story.Continue();
            Payload p5 = ParseCharPayloadFromCurrentTags(story);
            Assert.IsNotNull(p5.transition);
            Assert.AreEqual(0f, p5.transition.appear);
            Assert.AreEqual(1f, p5.transition.move);
            Assert.AreEqual(0f, p5.transition.disappear);
            Assert.AreEqual("bs", p5.left.actor);
            Assert.AreEqual("alice", p5.center.actor);
            Assert.AreEqual("happy", p5.center.expr);
            Assert.AreEqual("ss", p5.right.actor);
            Assert.IsNotNull(p5.transition.steps);
            Assert.AreEqual(3, p5.transition.steps.Length);
            CollectionAssert.AreEqual(new[] { "appear" }, p5.transition.steps[0].actions);
            Assert.IsFalse(p5.transition.steps[0].raise);
            CollectionAssert.AreEqual(new[] { "alice" }, p5.transition.steps[0].raiseActors);
            CollectionAssert.AreEqual(new[] { "move" }, p5.transition.steps[1].actions);
            Assert.IsFalse(p5.transition.steps[1].raise);
            Assert.IsTrue(p5.transition.steps[1].raiseActors == null || p5.transition.steps[1].raiseActors.Length == 0);
            CollectionAssert.AreEqual(new[] { "disappear" }, p5.transition.steps[2].actions);
            Assert.IsFalse(p5.transition.steps[2].raise);
            Assert.IsTrue(p5.transition.steps[2].raiseActors == null || p5.transition.steps[2].raiseActors.Length == 0);

            // 6) appear=0.3、move=0.7、disappear=0.5，steps=[disappear]->[move+appear]，alice 消失後 ss 再移動並可從空畫面重播
            story.Continue();
            Payload p6 = ParseCharPayloadFromCurrentTags(story);
            Assert.IsNotNull(p6.transition);
            Assert.AreEqual(0.3f, p6.transition.appear);
            Assert.AreEqual(0.7f, p6.transition.move);
            Assert.AreEqual(0.5f, p6.transition.disappear);
            Assert.AreEqual("bs", p6.left.actor);
            Assert.AreEqual("ss", p6.center.actor);
            Assert.IsTrue(p6.right == null || string.IsNullOrEmpty(p6.right.actor), "right 應該是空的（允許 null 或空物件）。");
            Assert.IsNotNull(p6.transition.steps);
            Assert.AreEqual(2, p6.transition.steps.Length);
            CollectionAssert.AreEqual(new[] { "disappear" }, p6.transition.steps[0].actions);
            CollectionAssert.AreEqual(new[] { "move", "appear" }, p6.transition.steps[1].actions);

            // 7) appear=0、move=0、disappear=0.5，steps=[appear+move+disappear]，bs 右邊、ss 消失
            story.Continue();
            Payload p7 = ParseCharPayloadFromCurrentTags(story);
            Assert.IsNotNull(p7.transition);
            Assert.AreEqual(0f, p7.transition.appear);
            Assert.AreEqual(0f, p7.transition.move);
            Assert.AreEqual(0.5f, p7.transition.disappear);
            Assert.IsTrue(p7.left == null || string.IsNullOrEmpty(p7.left.actor), "left 應該是空的（允許 null 或空物件）。");
            Assert.IsTrue(p7.center == null || string.IsNullOrEmpty(p7.center.actor), "center 應該是空的（允許 null 或空物件）。");
            Assert.AreEqual("bs", p7.right.actor);
            Assert.IsNotNull(p7.transition.steps);
            Assert.AreEqual(1, p7.transition.steps.Length);
            CollectionAssert.AreEqual(new[] { "appear", "move", "disappear" }, p7.transition.steps[0].actions);
            // ===== 變更結束 =====

            // 8) clear
            story.Continue();
            string clearValue = ParseCharValueFromCurrentTags(story);
            Assert.AreEqual("clear", clearValue);
        }

        private static Payload ParseCharPayloadFromCurrentTags(Ink.Runtime.Story story)
        {
            string value = ParseCharValueFromCurrentTags(story);
            Assert.IsTrue(value.StartsWith("{", StringComparison.Ordinal), $"char tag 值必須是 JSON：{value}");
            return JsonUtility.FromJson<Payload>(value);
        }

        private static string ParseCharValueFromCurrentTags(Ink.Runtime.Story story)
        {
            string charTag = story.currentTags.FirstOrDefault(t => t.StartsWith("char:", StringComparison.Ordinal));
            Assert.IsNotNull(charTag, "找不到 char tag。");
            return charTag.Substring("char:".Length).Trim();
        }
    }
}
// ===== 變更結束 =====
