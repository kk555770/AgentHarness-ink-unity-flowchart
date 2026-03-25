# Task Plan: Documentation Spec Relocation

## Goal
把 `GraphToolkitSpec.md` 與 `UIToolkitSpec.md` 收斂進 `Documentation/`，並同步修正所有正式入口與索引。

## Current Phase
Phase 4

## Phases

### Phase 1: 現況盤點
- [x] 確認 spec 檔案位置
- [x] 找出所有正式引用點
- [x] 建立本輪 PlanningWithFiles
- **Status:** complete

### Phase 2: 文件搬遷
- [x] 把 spec 檔案移進 `Documentation/`
- [x] 保持內容不被破壞
- **Status:** complete

### Phase 3: 索引與引用修正
- [x] 更新 `Documentation/references/index.md`
- [x] 更新 `Documentation/DocsIndex.md`
- [x] 更新其他正式文件引用
- **Status:** complete

### Phase 4: 驗證
- [x] 搜尋舊路徑殘留
- [x] 跑 `python3 Tools/repo_guard.py`
- [x] 檢查 git 狀態
- **Status:** complete

## Key Questions
1. 哪些引用屬於正式入口，哪些只是歷史工作記錄？
2. 搬遷後哪幾份索引必須一起更新？

## Decisions Made
| Decision | Rationale |
|----------|-----------|
| 只改正式文件與活的入口，不回頭改舊 `PlanningWithFiles/` | 歷史記錄應保留當時上下文，不需要為了搬檔全部洗一遍 |
| `GraphToolkitSpec.md` 與 `UIToolkitSpec.md` 直接移到 `Documentation/` 根層 | 它們是正式規格文件，不該留在 root |

## Errors Encountered
| Error | Attempt | Resolution |
|-------|---------|------------|
|       | 1       |            |
