// ===== 變更開始 =====
// 2026/01/30 Opsidanos (修改原因：新增存檔資料格式測試，避免存檔 JSON 版型被不小心改壞)
// 預期結果：InkSaveData 可以正確 ToJson/FromJson，主要欄位不會遺失
using NUnit.Framework;
using OpsidanosInk.Runtime.Save;
using UnityEngine;

namespace OpsidanosInk.Tests
{
    public sealed class InkSaveDataTests
    {
        [Test]
        public void InkSaveData_RoundTrip_JsonUtility()
        {
            var data = new InkSaveData
            {
                version = 1,
                inkStateJson = "{\"dummy\":true}",
                output = new StoryOutputSnapshot
                {
                    outputId = 123,
                    speaker = "旁白",
                    lineText = "第一句",
                    hasEnded = false,
                    choices = new[]
                    {
                        new ChoiceSnapshot { index = 0, text = "選項 A" },
                        new ChoiceSnapshot { index = 1, text = "選項 B" }
                    }
                },
                presentation = new PresentationSnapshot
                {
                    bgId = "room_01",
                    bgmId = "opening",
                    cgValue = "clear",
                    charValue = "{\"center\":{\"actor\":\"alice\",\"expr\":\"happy\"}}",
                    charLeftId = "clear",
                    charCenterId = "clear",
                    charRightId = "clear"
                }
            };

            string json = JsonUtility.ToJson(data);
            Assert.IsFalse(string.IsNullOrWhiteSpace(json), "InkSaveData ToJson 不應該產生空字串。");

            InkSaveData loaded = JsonUtility.FromJson<InkSaveData>(json);
            Assert.NotNull(loaded, "InkSaveData FromJson 不應該回傳 null。");

            Assert.AreEqual(1, loaded.version);
            Assert.AreEqual("{\"dummy\":true}", loaded.inkStateJson);

            Assert.NotNull(loaded.output, "output 不可為 null。");
            Assert.AreEqual(123, loaded.output.outputId);
            Assert.AreEqual("旁白", loaded.output.speaker);
            Assert.AreEqual("第一句", loaded.output.lineText);
            Assert.IsFalse(loaded.output.hasEnded);
            Assert.NotNull(loaded.output.choices);
            Assert.AreEqual(2, loaded.output.choices.Length);
            Assert.AreEqual(0, loaded.output.choices[0].index);
            Assert.AreEqual("選項 A", loaded.output.choices[0].text);
            Assert.AreEqual(1, loaded.output.choices[1].index);
            Assert.AreEqual("選項 B", loaded.output.choices[1].text);

            Assert.NotNull(loaded.presentation, "presentation 不可為 null。");
            Assert.AreEqual("room_01", loaded.presentation.bgId);
            Assert.AreEqual("opening", loaded.presentation.bgmId);
            Assert.AreEqual("clear", loaded.presentation.cgValue);
            Assert.AreEqual("{\"center\":{\"actor\":\"alice\",\"expr\":\"happy\"}}", loaded.presentation.charValue);
            Assert.AreEqual("clear", loaded.presentation.charLeftId);
            Assert.AreEqual("clear", loaded.presentation.charCenterId);
            Assert.AreEqual("clear", loaded.presentation.charRightId);
        }

        // ===== 變更開始 =====
        // 2026/02/06 Opsidanos (修改原因：新增多槽容器序列化測試，避免 manualSlots/autoSlot 版型被改壞)
        // 預期結果：InkSaveBankData 可正確 ToJson/FromJson，且槽位資料不遺失
        [Test]
        public void InkSaveBankData_RoundTrip_JsonUtility()
        {
            var bank = new InkSaveBankData
            {
                version = 1,
                manualSlots = new[]
                {
                    new InkSaveSlotData
                    {
                        version = 1,
                        activeRollbackIndex = 0,
                        rollbackHistory = new[]
                        {
                            new InkSaveData { version = 1, inkStateJson = "{\"slot\":1}" }
                        }
                    },
                    null,
                    new InkSaveSlotData
                    {
                        version = 1,
                        activeRollbackIndex = 0,
                        rollbackHistory = new[]
                        {
                            new InkSaveData { version = 1, inkStateJson = "{\"slot\":3}" }
                        }
                    }
                },
                autoSlot = new InkSaveSlotData
                {
                    version = 1,
                    activeRollbackIndex = 0,
                    rollbackHistory = new[]
                    {
                        new InkSaveData { version = 1, inkStateJson = "{\"slot\":\"auto\"}" }
                    }
                }
            };

            string json = JsonUtility.ToJson(bank);
            Assert.IsFalse(string.IsNullOrWhiteSpace(json), "InkSaveBankData ToJson 不應該產生空字串。");

            InkSaveBankData loaded = JsonUtility.FromJson<InkSaveBankData>(json);
            Assert.NotNull(loaded, "InkSaveBankData FromJson 不應該回傳 null。");
            Assert.AreEqual(1, loaded.version);
            Assert.NotNull(loaded.manualSlots, "manualSlots 不可為 null。");
            Assert.AreEqual(3, loaded.manualSlots.Length);
            Assert.NotNull(loaded.manualSlots[0], "manualSlots[0] 不可為 null。");
            Assert.AreEqual("{\"slot\":1}", loaded.manualSlots[0].rollbackHistory[0].inkStateJson);
            Assert.NotNull(loaded.manualSlots[1], "manualSlots[1] 會被 JsonUtility 還原成預設物件。");
            Assert.IsTrue(
                loaded.manualSlots[1].rollbackHistory == null || loaded.manualSlots[1].rollbackHistory.Length == 0,
                "manualSlots[1] 應該沒有有效 rollbackHistory。");
            Assert.NotNull(loaded.manualSlots[2], "manualSlots[2] 不可為 null。");
            Assert.AreEqual("{\"slot\":3}", loaded.manualSlots[2].rollbackHistory[0].inkStateJson);
            Assert.NotNull(loaded.autoSlot, "autoSlot 不可為 null。");
            Assert.AreEqual("{\"slot\":\"auto\"}", loaded.autoSlot.rollbackHistory[0].inkStateJson);
        }
        // ===== 變更結束 =====
    }
}
// ===== 變更結束 =====
