# 任務計畫：專案進度盤點與下一步規劃（2026-01-27）

## 目標
- 在**不修改任何代碼**的前提下：
  - 盤點目前專案「有哪些東西」與「可以做到什麼」
  - 整理 `PlanningWithFiles/` 各任務資料夾：對應內容 + 完成度 + 主要影響檔案
  - 找出還沒做完／可能卡住的點
  - 產出「下一步」的可執行提案（範圍／現況／預期效果），等你同意後再開始改代碼

## 目前階段
- 已完成（歷史盤點任務，封存）

## Phases

### Phase 1：盤點 PlanningWithFiles 既有任務
- [x] 列出所有 `task_plan.md` 並逐一閱讀狀態
- [x] 整理：哪些任務已完成、哪些仍寫 in_progress/pending
- [x] 確認：`20260120/TextAdventureEngine` Phase 4 目前只剩「Addressables/Resources（打包用）」未完成，其餘演出與 ResourceMap 已由後續任務補齊
- **狀態：complete**

### Phase 2：實際調查專案內容（Packages/Assets）
- [x] 盤點專案主要結構：`Packages/Ink`（原始 ink-unity-integration）+ `Packages/com.opsidanos.ink`（自家 VN Runtime）
- [x] 盤點示範內容：`Assets/Scene/Test.unity`、`Assets/OpsidanosInk/Demo/*`
- [x] 盤點主要 Runtime 組件：`InkStoryEngine` / `VNPlayerPresenter` / `InkTagEventRouter` / 各 Tag Player / `InkResourceMap`
- [x] 盤點測試：`Assets/Editor/Tests/*`
- **狀態：complete**

### Phase 3：建立「對應關係表」並整理完成度
- [x] 建立表格：PlanningWithFiles 任務 → 對應功能 → 主要檔案/資料位置
- [x] 將表格與整理結果寫入 `findings.md`
- **狀態：complete**

### Phase 4：決定下一步（只做提案，不動手）
- [x] 從現況選出下一步：Player build 支援 ResourceMap（Addressables）
- [x] 以「提案格式」寫出：範圍／現況／預期效果／你需要驗證的點（見 `findings.md`）
- **狀態：complete**

### Phase 5：交付（更新 planning 檔）
- [x] 更新本任務 `progress.md` 與狀態
- [x] 已於後續任務完成 Addressables 實作，並在主計畫同步狀態
- **狀態：complete**

## 狀態同步（2026-02-06）
- [x] 本任務定位維持「歷史盤點」：不再追蹤進行中項目
- [x] 下一步提案（Player build 支援 ResourceMap/Addressables）已在後續任務落地

## 決策紀錄
| 決策 | 原因 |
|------|------|
|      |      |

## 錯誤紀錄
| 錯誤 | 嘗試次數 | 解法 |
|------|----------|------|
|      | 1        |      |
