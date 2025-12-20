# AGENTS.md

本文件為本專案「代理行為準則」。所有代理（含 Codex、其他 LLM 工具）必須遵守。
若需修改本文件，請先提案並經專案負責人核可。

1. **使用繁體中文**  
   - 所有回應、文件與程式碼註解皆必須使用繁體中文，避免出現簡體字或英文介詞混雜，
   - 請把我當作8歲小孩來解釋現況，不要使用機械式或論文式說明。  
   - 回應前必須加上[AGENTS-已讀]，不然我不知道你有讀到。

2. **自動化運作**  
   - 指令：請以 `bash ./scripts/unity_tests.sh` 作為驗收開關（0=綠，非0=紅）。  
   - Git：開新分支 `codex/<短任務名>`；PR 標題以 `[codex]` 開頭。 
   - PR 綠燈後可自動合併。

3. **自動化測試**
   - Run tests with: python3 -m unittest -q
   - Constraints:
   - No third-party deps
   - No web search
   - Minimal change only