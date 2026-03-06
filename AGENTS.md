# AGENTS.md

1. **修改流程**
   - 未經同意前，僅允許調查、執行讀取/搜尋/測試、以及更新 `PlanningWithFiles` 例外文件；並行僅限上述非寫入性工作。
   - 本專案規範採分層：先遵守 `~/.codex/AGENTS.md` 的全域規範；若本檔有更具體/更嚴格規定，則以本檔為準。
   - 提案必含：要改的檔案清單、現況、改法、為何能解、預期效果、驗證方式（跑哪些測試/如何確認）
   - 不要在提案中詢問不能用[同意]與[不同意]回答的問題，請直接提出修改方案，有問題我會重新提問或更新條件後請你重新修改。
   - 若並非提案，而是想釐清我的想法，則可以直接詢問，或是有不合理的地方也可以反對，不要猜我的想法，盡可能詳細思考尋找完善解法用於提案，但不要追問過多細節。
   - 修改過程可積極調用Skills-[planning-with-files]來正確的理解並補充專案計畫/過程/規格等文件，修改[findings.md]、[progress.md]、[task_plan.md]不需要經過我同意。
      - Skills-[planning-with-files]的使用是調查/實作一步、記錄一步，連續循環直到調查/實作完畢，不是調查/實作完所有東西再一次補。
   - 可調查，但不要關注確定與你任務範圍無關的git改動，請專注於你的任務範圍。
   - 修改流程中有問題可積極調用MCP-[AskUserQuestionTool]，一次問一個問題，直到問題問完為止，不要猜我的想法； 提案前就不用調用，直接主動詢問就好。
   - 若無法透過調用MCP-[AskUserQuestionTool]解決，請直接停止並回應，而不是不斷嘗試可能偏離同意內容的操作。
   - 需要調查Console或場景狀況可積極調用MCP-[Unity-Mcp]，盡可能避免讓使用者不斷機械式的調整場景、添加Component或複製貼上。

2. **UI Tookit Spec / Graph Tookit Spec / DeveloperModeOutputContract**
   - 此專案目前暫時為Unity 6000.3.9f1
   - 有需要動到任何UI Toolkit組件時請務必參考[UIToolkitSpec.md]，你可以透過[cat UIToolkitSpec.md]來閱讀它
   - 有需要動到任何Graph Toolkit組件時請務必參考[GraphToolkitSpec.md]，你可以透過[cat GraphToolkitSpec.md]來閱讀它
   - 開發者模式輸出契約則放在[DeveloperModeOutputContract.md]中

3. **使用繁體中文**  
   - 所有回應、文件與程式碼註解皆必須使用繁體中文，避免出現簡體字或英文介詞混雜，
   - 使用8歲小孩也能懂的例子來詳細解釋現況，不要使用機械式或論文式說明。

4. **禁止防禦性編碼**
   - 為了追求易讀性，必須要在Unity Editor上所見即所得，有問題就應該要直接透過debug.Log提供給開發者知道，而不是用防禦性編碼，在Play Mode中讓腳本自行修改，但最終只會使bug更難找。

5. **程式碼風格**
   - 縮排：
     - C#：空白 4 格
     - Markdown／JSON：空白 2 格
   - 拆檔習慣：大型類別常用 `partial` 拆檔，檔名習慣 `類別名.主題.cs`（例：`UIManager.Clone.cs`、`SystemUiManager.*.cs`）
   - 修改 `.cs` 必寫註解：每次改到 `.cs` 的任何一段，都要用下面格式把「範圍圈起來」，並寫清楚「日期 + 人名（Opsidanos，這是我的名字）+ 修改原因」與「預期結果」
      ```csharp
      // ===== 變更開始 =====
      // YYYY/MM/DD 人名 (修改原因：XXX)
      // 預期結果：XXX
      a = b + c ;(這裡是你更改的代碼內容)
      // ===== 變更結束 =====
      ```
   - 除錯輸出：常見用上色輸出，讓 Console 更好找（例：`"..." % Colorize.Green`、`"...".Color(Color.cyan)`）