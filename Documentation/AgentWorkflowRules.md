# Agent 工作規則

> 文件負責人：harness
> 最後更新：2026/05/20

## 1. 修改流程

- 未經使用者同意前，只能做調查 / 讀取 / 搜尋 / 測試 / 更新 `PlanningWithFiles`；並行也只限這些非寫入工作。
- 提案必須包含：檔案清單、現況、改法、為何能解、預期效果、驗證方式。
- 不要在提案中詢問不能用 `[同意]` 或 `[不同意]` 回答的問題。
- 需要釐清時可直接問；條件不合理時可直接反對，不要猜測使用者意圖。
- 修改過程可使用 `PlanningWithFiles` 補充計畫 / 過程 / 規格。
- 修改 `findings.md`、`progress.md`、`task_plan.md` 不需要額外同意。
- `PlanningWithFiles` 要調查一步記一步、實作一步記一步，不要最後一次補寫。
- 可調查 git 狀態，但不要追逐與任務明確無關的改動。
- 若在修改流程中需要釐清問題，可一次只問一個問題。
- 若無法在已同意範圍內解決，應停止並明確回報，不要持續嘗試偏離同意內容的操作。
- 若需要調查 Console 或場景狀況，優先使用 Unity 相關工具，避免把機械式操作丟回給使用者。

## 2. Spec 入口

- 此專案目前以 Unity `6000.3.9f1` 為準。
- 動到 UI Toolkit 組件時，必讀 `Documentation/UIToolkitSpec.md`。
- 動到 Graph Toolkit 組件時，必讀 `Documentation/GraphToolkitSpec.md`。
- current projection / runtime 輸出契約在 `Documentation/DeveloperModeOutputContract.md`。

## 3. 語言規範

- 所有回應、文件與程式碼註解皆必須使用繁體中文。
- 避免簡體字或中英夾雜的機械式語氣。
- 盡量用 8 歲小孩也能懂的方式解釋現況。

## 4. 開發原則

- 禁止防禦性編碼。
- 盡可能地解耦、模塊化，並盡可能減少重複功能，以及避免單檔超過300行的情況。
- 不要透過腳本在 Play Mode 中對靜態功能介面偷偷補洞或默默自我修正，必要的動態功能介面除外。
- 有問題應直接透過 `Debug.Log` / `Debug.LogError` 暴露給開發者。

## 5. 程式碼風格

- C#：空白 4 格
- Markdown / JSON：空白 2 格
- 大型類別可用 `partial` 拆檔。
- 檔名習慣：`類別名.主題.cs`
- 例：`UIManager.Clone.cs`、`SystemUiManager.*.cs`
- 修改 `.cs` 時，必須用下面格式圈出變更：

```csharp
// ===== 變更開始 =====
// YYYY/MM/DD Opsidanos (修改原因：XXX)
// 預期結果：XXX
a = b + c;
// ===== 變更結束 =====
```

- 日期要寫，名字固定寫 `Opsidanos`，且要同時寫修改原因與預期結果。
- 常見做法可用上色輸出，讓 Console 更好找。
- 例：`"..." % Colorize.Green`、`"...".Color(Color.cyan)`

## 6. 標準驗證入口

- 專案目前閒置；GitHub Actions 遠端 workflow 已手動停用。
- 閒置期間不得自行開 PR、推送 branch、啟用 CI/CD、或恢復排程。
- GitHub Actions：`.github/workflows/CI.yml` 只作為人工啟動入口。
- 本地終端一鍵測試：`Tools/run_tests.sh`
