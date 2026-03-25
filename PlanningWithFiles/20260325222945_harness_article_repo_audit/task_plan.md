# Task Plan: 依據 OpenAI harness engineering 文章審核整個專案

## Goal
依據 OpenAI《運用工程技術：在智慧體優先的世界中善用 Codex》文章，審核整個 `ink-unity-integration` 專案目前有哪些地方已對齊、哪些地方仍不符合、以及哪些地方需要改進。

## Current Phase
Phase 5

## Phases

### Phase 1: 需求與審核標準建立
- [x] 理解使用者要求
- [x] 讀取 repo AGENTS / workflow 規則
- [x] 讀取文章並抽出審核準則
- [x] 把審核準則寫入 findings.md
- **Status:** complete

### Phase 2: 文件與治理層審核
- [x] 檢查 docs 是否真的是 system of record
- [x] 檢查 ownership / freshness / cross-link / active plan
- [x] 檢查 repo guard / docs garden / generated evidence 是否形成機械護欄
- **Status:** complete

### Phase 3: 程式 / 測試 / 驗證回饋迴路審核
- [x] 檢查 runtime / editor / control plane 相關實作與測試
- [x] 檢查 CI / run_tests / evidence bundle 是否支撐 agent 驗證
- [x] 檢查 deterministic / contract / regression 相關護欄
- **Status:** complete

### Phase 4: 缺口整合與嚴重度分級
- [x] 對照文章原則列出缺口
- [x] 區分高風險 / 中風險 / 低風險
- [x] 形成可執行的改善建議
- **Status:** complete

### Phase 5: 交付
- [x] 更新 progress.md / findings.md
- [x] 產出最終審核結果
- [x] 附上來源與證據
- **Status:** complete

## Key Questions
1. 這個 repo 是否真的把知識、規格、計畫、驗證回饋寫進 codebase，而不是只停在聊天上下文？
2. 文章強調的「agent 可讀性、機械式護欄、回饋迴路、垃圾回收」在這個 repo 哪些已落地、哪些仍只是文件宣示？
3. 哪些缺口最可能直接限制 agent 端到端交付能力？

## Decisions Made
| Decision | Rationale |
|----------|-----------|
| 先建立新的 PlanningWithFiles 工作區再做深度審核 | 符合 repo workflow，並讓審核過程可追溯 |
| 審核分成文件治理、機械護欄、程式測試回饋三條線 | 對應文章的 system of record、readability、feedback loop 與 entropy control |
| 只做審核，不修改產品碼 | 使用者要求是專案審核；目前尚未獲得產品碼修改同意 |

## Errors Encountered
| Error | Attempt | Resolution |
|-------|---------|------------|
|       | 1       |            |

## Notes
- 每完成一個審核階段就更新 phase 狀態
- 每 2 次以上的 search / view / browser 後就把結論寫進 findings.md
- 最終輸出以具體缺口與證據為主，不做空泛評論
