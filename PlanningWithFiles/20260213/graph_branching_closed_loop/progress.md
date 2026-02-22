# 進度日誌：Flow Chart 分岔節點規範與匯出/匯入閉環

## 2026-02-13

### P-001 建立任務規劃檔
- 建立資料夾：`PlanningWithFiles/20260213/graph_branching_closed_loop`
- 建立檔案：`task_plan.md`、`findings.md`、`progress.md`

### P-002 盤點現況（不修改程式）
- 指令：`git status --short`、讀取 GraphToolkit 節點/匯出/匯入相關檔案
- 結果：已確認目前匯出用多行 `->`，且匯出/匯入以「v1 線性流程」拒絕多 next

### P-003 盤點 Ink 基準（選項/條件語法）
- 指令：讀取 Ink 編譯器 Parser：`InkParser_Choices.cs`、`InkParser_Conditional.cs`
- 結果：
  - 選項只能用 `*`（一次性）或 `+`（可重複）
  - 條件分岔可用 `{ ... }` 多行 conditional，分支必須用 `-`，`else` 必須在最後

### P-004 更新規範文件（先列 Ink 基準，再定義 Flow Chart 子集合）
- 動作：改寫 `Documentation/DeveloperModeOutputContract.md` 的 Graph 章節，加入 Ink 基準層與 Graph v2（choice/condition）規範
- 結果：規範已明確要求「分岔必須用 choice/condition 節點」並定義對應 Ink 輸出（`*`/`+`、`{ ... }`）
