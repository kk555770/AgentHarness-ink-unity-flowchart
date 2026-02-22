# 進度日誌：全專案深度調查（不使用 Unity MCP）

## 2026-02-11

### 步驟 1：初始化
- 建立調查資料夾：`PlanningWithFiles/20260211/full_project_deep_audit`
- 建立檔案：`task_plan.md`、`findings.md`、`progress.md`
- 狀態：開始執行階段 1（基線盤點）

### 步驟 2：Git 基線盤點
- 取得分支與 `HEAD`：`arcumit/CodexInk` / `dc8a443`
- 取得工作樹未提交清單：2 檔（Runtime + ProjectSettings）
- 取得近 25 筆提交：確認近期主線偏向 Graph Toolkit 與測試穩定

### 步驟 3：專案結構盤點
- 統計 `Assets` / `Packages` / `ProjectSettings` 檔案量
- 盤點 `Assets`、`Packages` 一級結構
- 確認 `Documentation` 文件清單

### 步驟 4：規格與約束矩陣（第一輪）
- 讀取 `AGENTS.md`、`UIToolkitSpec.md`、`GraphToolkitSpec.md`、`README.md`
- 抽取硬性規則與優先順序
- 記錄「上游 README 與本地規格不等價」風險

### 步驟 5：套件與組件邊界盤點
- 讀取 `Packages/manifest.json` 與 `Packages/com.opsidanos.ink/package.json`
- 盤點全部 asmdef 並抽取自有四個組件的 references
- 確認 Runtime 線與 GraphToolkit Editor 線的分界

### 步驟 6：新增「上一個提交誤解風險稽核」子任務
- 依使用者新要求，將 `dc8a443` 納入獨立稽核階段。
- 取得 `dc8a443` 檔案清單與統計：9 檔、770 行新增、8 行刪除。
- 初步確認範圍集中在 GraphToolkit Editor 與 Editor 測試。

### 步驟 7：`dc8a443` 逐檔差異讀取
- 讀取 9 個變更檔案的 commit diff 與目前檔案內容。
- 完成事實層整理：Graph/Exporter/Importer/三個測試/.gitignore 的具體改動。
- 下一步：做「意圖 vs 效果」與「是否誤解」判定。

### 步驟 8：程式模組地圖（第一輪）
- 掃描 Runtime / Editor / PlayMode 測試的 `.cs` 清單。
- 統計外部套件 `com.unity.ai.navigation` 測試檔數（38）。
- 記錄自有模組與外部測試混跑風險。

### 步驟 9：修正記錄節奏（立即生效）
- 觸發原因：使用者指出未達「調查一步、記錄一步」。
- 已執行：新增 `F-009`，把記錄節奏改為「每一步即時寫入」。
- 後續執行方式：每次調查動作完成後先記錄，再進下一步。

### 步驟 10：抽取 `Test.unity` 腳本 GUID 清單
- 指令：`rg -o "guid: [0-9a-f]{32}" Assets/Scene/Test.unity | sort -u`
- 結果：取得 27 個唯一 GUID（含 Unity 內建 GUID 與專案腳本 GUID）。
- 下一步：將非內建 GUID 對照 `.meta` 還原腳本名稱。
