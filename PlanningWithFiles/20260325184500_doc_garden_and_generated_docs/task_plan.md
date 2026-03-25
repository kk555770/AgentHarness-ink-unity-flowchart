# Task Plan: Doc Garden And Generated Docs

## Goal
補上 doc-gardening 與 generated docs 的最小落地版本：新增可重跑腳本、產生正式 generated docs、並加上週期性 workflow。

## Current Phase
Phase 4

## Phases

### Phase 1: 現況盤點
- [x] 確認 `Documentation/generated/` 目前只有 index
- [x] 確認 `Logs/TestResults/` 有可用 XML
- [x] 盤點缺少 `最後更新` 的正式文件
- **Status:** complete

### Phase 2: 腳本實作
- [x] 新增 doc-garden report 腳本
- [x] 新增 test results index 產生腳本
- **Status:** complete

### Phase 3: 正式文件與 workflow
- [x] 新增 generated docs
- [x] 更新 `Documentation/generated/index.md`
- [x] 新增 `docs-garden` workflow
- **Status:** complete

### Phase 4: 驗證
- [x] 跑 generated docs 腳本
- [x] 跑 `repo_guard`
- [x] 跑 `git diff --check`
- **Status:** complete

## Key Questions
1. 哪些 generated docs 最值得先版本化？
2. doc-gardening 先做到哪個深度最划算？

## Decisions Made
| Decision | Rationale |
|----------|-----------|
| 先做 doc-garden report + test results index | 這兩個都能直接用現有 repo 資料生成，成本低、價值高 |
| workflow 先做 artifact/report 型，不先做自動 commit | 先把週期性掃描跑起來，避免一次做太重 |

## Errors Encountered
| Error | Attempt | Resolution |
|-------|---------|------------|
|       | 1       |            |
