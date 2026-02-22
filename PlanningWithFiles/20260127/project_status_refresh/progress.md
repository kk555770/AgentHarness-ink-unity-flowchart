# 進度紀錄：專案進度盤點與下一步規劃（2026-01-27）

## Session：2026-01-27

### 已完成（本次只做盤點，不改代碼）
- 讀取 `PlanningWithFiles/` 所有既有 `task_plan.md` / `progress.md`，整理目前完成度與未完成方向
- 實際盤點專案內容：
  - `Packages/`：`Ink`（原始整合）+ `com.opsidanos.ink`（自家 VN Runtime）
  - `Assets/Scene/Test.unity`、`Assets/OpsidanosInk/Demo/*`
  - 主要 Runtime 腳本：`InkStoryEngine` / `VNPlayerPresenter` / `InkTagEventRouter` / Tag Players / `InkResourceMap`
  - 測試：`Assets/Editor/Tests/*`
- 把盤點結果寫入本任務 `findings.md`
- 整理下一步提案：Player build 支援 ResourceMap（Addressables）

### 後續追蹤結果（2026-02-06 同步）
- 對應關係表細化已由後續盤點任務補齊（`20260201/project_status_update`）
- 原提案「Player build 支援 ResourceMap（Addressables）」已在後續任務實作與驗證完成
- 本任務改標記為歷史盤點完成，不再保留進行中/待做項目
