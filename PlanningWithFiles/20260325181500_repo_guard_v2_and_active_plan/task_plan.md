# Task Plan: Repo Guard V2 And Active Plan

## Goal
補上真正的下一步：讓 `Documentation/exec-plans/active/` 有正式 active plan，並把 `repo_guard` 升到 v2，開始守 freshness / cross-links / active-plan existence。

## Current Phase
Phase 4

## Phases

### Phase 1: 現況盤點
- [x] 重讀 `AGENTS.md`
- [x] 盤點 `repo_guard` 現況
- [x] 盤點 `exec-plans/active/` 現況
- **Status:** complete

### Phase 2: 正式 active plan
- [x] 新增 active exec plan
- [x] 更新 `PLANS.md` 與 active index
- **Status:** complete

### Phase 3: repo_guard v2
- [x] 補 freshness header 檢查
- [x] 補 cross-links 檢查
- [x] 補 active plan existence 檢查
- **Status:** complete

### Phase 4: 驗證
- [x] 跑 `python3 Tools/repo_guard.py`
- [x] 跑 `git diff --check`
- [x] 檢查 git 狀態
- **Status:** complete

## Key Questions
1. 哪一層最能證明「下一步真的有做」？
2. 哪些規則最適合先機械化，而不是只寫在文件？

## Decisions Made
| Decision | Rationale |
|----------|-----------|
| 先補 active exec plan | 文章把 plans 視為第一級工件，這裡原本是空的 |
| `repo_guard v2` 先做 freshness / cross-links / active plan | 這三項最直接對應文章缺口，且能立刻落地 |

## Errors Encountered
| Error | Attempt | Resolution |
|-------|---------|------------|
| `Documentation/ARCHITECTURE.md` 尾端空白 | 1 | 修正空白後重新驗證 |
