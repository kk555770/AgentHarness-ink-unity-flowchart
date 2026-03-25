# Task Plan: Harness Closure Execution

## Goal
把這輪 harness 對齊工作、`ImportProjection(flowchart-json)` 的 control-plane 落地，以及後續 re-audit 的順序，整理成可持續更新的任務文件與 system-of-record 入口。

## Current Phase
Phase 4

## Phases

### Phase 1: 現況盤點與邊界固定
- [x] 讀取 OpenAI harness engineering 文章與 repo AGENTS / AgentWorkflowRules
- [x] 確認目前已落地的 workflow guard、docs guard、Ink compile validation
- [x] 釐清 `ImportProjection` 在 code 與 docs 的差異
- **Status:** complete

### Phase 2: 建立正式任務文件
- [x] 建立 `PlanningWithFiles/20260325224531_harness_closure_execution/` 三份記錄檔
- [x] 建立 `Documentation/exec-plans/active/harness_alignment_closure_loop.md`
- [x] 把改進項目、優先順序、驗證方式與完成定義寫成正式順序
- **Status:** complete

### Phase 3: System-of-record 同步
- [x] 更新 `Documentation/CurrentMilestones.md`
- [x] 更新 `Documentation/QUALITY_SCORE.md`
- [x] 更新 `Documentation/RELIABILITY.md`
- [x] 更新 `Documentation/exec-plans/tech-debt.md`
- [x] 更新 `Documentation/generated/index.md`
- [x] 更新 `Documentation/CanonicalGraphApiSpec.md`
- [x] 更新 `Documentation/CanonicalGraphJsonContract.md`
- **Status:** complete

### Phase 4: `ImportProjection` control-plane 落地與 re-audit
- [x] 補上 `ImportProjection(flowchart-json)` dispatcher / shared service / 測試
- [ ] 依 harness engineering 文章重讀正式文件與實作
- [ ] 記錄仍未解決的缺口
- [ ] 若有新缺口，更新 task_plan / findings / progress 再迭代
- **Status:** pending

## Key Questions

1. `ImportProjection(flowchart-json)` 落地後，文件要如何避免把 `ink` import 也誤寫成已完成？
2. 哪些文章缺口屬於 repo 可在這輪內補，哪些屬於較大平台缺口？

## Decisions Made

| Decision | Rationale |
|----------|-----------|
| `ImportProjection` 先用正式 contract 固定邊界，再由主線把 `flowchart-json` control-plane 補齊 | 先讓 system-of-record 穩住，再讓實作對齊同一份 contract |
| `flowchart-json` 先當成目前最小可守的 import target | 和現有 importer 路徑最接近，也最不容易和未來實作打架 |
| `PlanningWithFiles` 與 `Documentation` 共同作為閉環工作記錄 | 讓短期探索與長期 system-of-record 分工清楚 |

## Errors Encountered

| Error | Attempt | Resolution |
|-------|---------|------------|
| `PlanningWithFiles/20260325224531_harness_closure_execution/` 起初是空目錄 | 1 | 依 skill 建立三份 planning files，先固定這輪閉環記錄 |

## Notes

- 這份 task file 會跟著每一輪文件同步與 re-audit 更新。
- `ImportProjection(flowchart-json)` 已由主線補上 control-plane，接下來重點是 re-audit 與剩餘缺口分類。
- `ValidateProjection(Ink)` 的預設 compile probe 已進 core，Editor-bound 問題已縮小。
- `ink` import、`UNITY_LICENSE` 綁定的 CI feedback loop、observability、deeper code graph guard 仍需要後續迭代。
