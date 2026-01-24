// ===== 變更開始 =====
// 2026/01/22 Opsidanos (修改原因：提供 Ink Tag 字串解析器)
// 預期結果：能把 raw tags（字串）解析成 InkTag（key/value），並在格式錯誤時用 Debug.LogError 指出
using System.Collections.Generic;
using UnityEngine;

namespace OpsidanosInk.Runtime.Story
{
    public static class InkTagParser
    {
        public static List<InkTag> Parse(IReadOnlyList<string> rawTags)
        {
            if (rawTags == null)
            {
                Debug.LogError("[OpsidanosInk] InkTagParser.Parse 收到 null 的 rawTags。");
                return new List<InkTag>(0);
            }

            var result = new List<InkTag>(rawTags.Count);

            for (int i = 0; i < rawTags.Count; i++)
            {
                string raw = rawTags[i];
                if (string.IsNullOrWhiteSpace(raw))
                {
                    continue;
                }

                string trimmed = raw.Trim();
                int colonIndex = trimmed.IndexOf(':');
                if (colonIndex < 0)
                {
                    result.Add(new InkTag(trimmed, null));
                    continue;
                }

                string key = trimmed.Substring(0, colonIndex).Trim();
                string value = trimmed.Substring(colonIndex + 1).Trim();

                if (string.IsNullOrWhiteSpace(key))
                {
                    Debug.LogError($"[OpsidanosInk] 解析 Tag 失敗：鍵不可為空：\"{trimmed}\"");
                    continue;
                }

                result.Add(new InkTag(key, value));
            }

            return result;
        }
    }
}
// ===== 變更結束 =====
