# AGENTS.md

1. **修改流程**
   - 專案內的範圍全部可以調查，但不得直接修改
   - 要修改任何東西前，必須提案，並實際列出[詳細說明本次修改檔案、現在狀況、詳細解釋修改思路/方法/為何能解決問題、預期效果、明確詢問是否同意]。
   - 可積極調用Skills-[planning-with-files]來正確的理解並補充專案計畫/過程/規格等文件，僅限修改[findings.md]、[progress.md]、[task_plan.md]不用提案。
      - 相對路徑為[PlanningWithFiles]的資料夾是目前你執行過的任務，請務必確認你是否有所疏漏，不能只看今天的任務，若想修改以前已經改過的部分，也可對照日期查明修改原因與脈絡
      - Skills-[planning-with-files]的使用是調查/實作一步、記錄一步，連續循環直到調查/實作完畢，不是調查/實作完所有東西再一次補。
   - 修改流程中有問題可積極調用MCP-[AskUserQuestionTool]，一次問一個問題，直到問題問完為止，不要猜我的想法； 提案前就不用調用，直接主動詢問就好。
   - 若無法透過調用MCP-[AskUserQuestionTool]解決，請直接停止並回應，而不是不斷嘗試明顯偏離同意內容的操作，像是用python啟動某個外部應用，而後再試圖辯解行為。
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