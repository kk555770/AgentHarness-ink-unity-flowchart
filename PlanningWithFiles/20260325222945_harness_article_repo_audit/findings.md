# Findings & Decisions

## Requirements
- 依據 OpenAI harness engineering 文章，嚴格審核整個專案。
- 找出目前不符合文章原則、需要改進的地方。
- 遵守 repo `AGENTS.md` 與 `Documentation/AgentWorkflowRules.md`。
- 可以使用 sub agent 並行調查，但不要主動中斷，等其自行完成後再關閉。
- 這輪以審核為主，不直接改產品碼。

## Research Findings
- repo root `AGENTS.md` 已明確把自己定位成目錄表，並導向 `Documentation/` 與 `PlanningWithFiles/`。
- `Documentation/CurrentMilestones.md`、`PLANS.md`、`PRODUCT_SENSE.md`、`DocsOwnership.md`、`QUALITY_SCORE.md`、`RELIABILITY.md`、`SECURITY.md` 形成了明確的治理入口。
- `Documentation/AgentWorkflowRules.md` 規定未經使用者同意前，只能做調查 / 搜尋 / 測試 / 更新 `PlanningWithFiles`。
- OpenAI 文章的核心審核面向至少包含：system of record、agent readability、機械式護欄、回饋迴路、黃金原則、entropy / garbage collection、自主性門檻。
- `python3 Tools/repo_guard.py` 在目前 checkout 會通過，表示 docs 入口、owner header、active plan、部分 freshness 與 asmdef / 檔案大小 gate 都已落地成機械檢查。
- `Documentation/generated/test_results_index.md` 顯示最新成功 CI run `23545345565` 的 artifact 類型是 `none`，Unity 測試 job 為 `skipped`，目前 evidence loop 仍停在「能誠實回報缺證據」，還不是「能持續產出真實證據」。
- `Tools/repo_guard_rules.json` 目前只對 37 份 `Documentation/**/*.md` 中的 7 份文件做 freshness 檢查，代表 freshness guard 已存在，但覆蓋面仍很窄。
- core code / tests 已明確覆蓋 `CreateGraph / CreateNode / ConnectPorts / ValidateGraph / GetGraph / ReplaceNodePayload / DisconnectEdge / RemoveNode / ValidateProjection / ProjectGraph`。
- `ImportProjection` 仍只存在於 spec / 文件層，尚未進入 JSON dispatcher 或 target-based control plane。
- repo 已有 `CurrentFlowImportService` 與對應測試，表示 import 基礎能力存在，但還沒有升格成對外 control plane。
- 本地 `Tools/run_tests.sh` 可在目前環境直接跑通：EditMode `103/103` 通過，PlayMode `17/17` 通過；代表本地回饋迴路已可用，主要缺口仍在 CI / generated evidence 閉環，而不是本地測試入口本身壞掉。

## Audit Findings
- 高：
  - 主 CI / generated evidence / auto-fix 仍缺真正的 Unity 閉環。`UNITY_LICENSE` 缺席時，CI 仍可成功，但 `Documentation/generated/test_results_index.md` 會落在 `artifact 類型 none`、`Missing`、`Missing console source`；auto-fix 也只剩 repo guards 驗證。這和文章強調的 agent-driven feedback loop 還有明顯距離。
  - `ValidateProjection(Ink)` 目前只做 `BuildInkContent(...)`，沒有進一步編譯或語意驗證；對 agent 來說這等於「看起來可投影」但不保證輸出真的可用。
- 中：
  - docs freshness guard 已存在，但目前只覆蓋 37 份文件中的 7 份；大量正式架構 / 規格 / 工作流文件尚未進入 freshness 機械檢查。
  - `ImportProjection` 已在 spec 裡佔正式 API 位置，也已有 import service 基礎能力，但尚未進入 JSON control plane，導致 control plane 仍是單向的 `validate/project` 多於真正可回寫的 authoring loop。
  - repo 自己承認仍缺更深層的 code graph / naming / structured logging guard，以及 UI / metrics / trace 層級的 agent observability；這代表 entropy cleanup 主要還停在 docs 層。
  - 安全入口仍是最小殼：缺 threat model、secret-handling 規格與 workflow / external service 的機械式檢查。
  - docs-garden 的證據來源在沒有指定 `workflow_run` 時會退回 `latest_successful_ci`，比較像週期輪詢，而不是緊貼某次具體執行的 provenance。
- 低：
  - `generated` 證據入口雖然存在，但沒有進到 `DocsIndex.md` 的主要 `Start Here`。
  - `ValidateGraph` 與對應測試已有最小 invariant 護欄，但 branch completeness 類型的驗證仍未形成明確回歸保護。
  - 本地測試會產生 `Tools/__pycache__/` 這類未追蹤產物，顯示 repo 仍有少量測試 / 腳本副產物沒有被工作流程吸收。

## Technical Decisions
| Decision | Rationale |
|----------|-----------|
| 以文章原則為審核 rubric，而不是只做一般 code review | 使用者要求是「嚴格按照這篇文章的說法」 |
| 審核證據同時看 docs、tools、tests、workflow、實作 | 文章強調環境、文件、工具與回饋迴路是一體的 |
| 先優先讀正式 docs，再回看既有 PlanningWithFiles | 符合 repo 規則：正式知識在 `Documentation/` |

## Issues Encountered
| Issue | Resolution |
|-------|------------|
| 尚未確認 repo 先前的 harness 審核是否已過期 | 已比對多份 `PlanningWithFiles` 與當前工具/文件，確認有些舊 finding 已修正，這次需重新以當前檔案為準 |
| 嘗試打開不存在的 `CurrentFlowCanonicalImportService.cs` | 改回檢查實際存在的 `CurrentFlowImportService.cs` 與其測試 |

## Resources
- OpenAI harness engineering 文章：https://openai.com/zh-Hant/index/harness-engineering/
- `AGENTS.md`
- `Documentation/AgentWorkflowRules.md`
- `Documentation/CurrentMilestones.md`
- `Documentation/QUALITY_SCORE.md`
- `Documentation/RELIABILITY.md`
- `.github/workflows/CI.yml`
- `.github/workflows/docs-garden.yml`
- `.github/workflows/codex-auto-fix.yml`
- `Tools/repo_guard.py`
- `Tools/repo_guard_rules.json`
- `Tools/doc_garden.py`
- `Tools/generate_test_results_index.py`
- `Tools/fetch_ci_test_results.py`
- `Documentation/generated/test_results_index.md`
- `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CanonicalGraphJsonCommandDispatcher.cs`
- `Packages/com.opsidanos.ink/Core/Scripts/Projection/CurrentFlow/CurrentFlowProjectionService.cs`

## Visual/Browser Findings
- 文章明確把 `AGENTS.md` 定位成目錄表，而不是百科全書。
- 文章強調知識要進 `docs/`，並且需要 ownership、freshness、cross-link、coverage 這類可機械檢查的治理。
- 文章強調代理應能自己使用工具、測試、UI 驗證、日誌與指標，而不只是看原始碼。
- 文章強調需要固定循環的垃圾回收與 golden principles，避免 agent 複製 repo 內既有壞模式。
- 文章也明寫 agent 可自主驗證 bug、跑 UI 路徑、查日誌 / 指標 / 追蹤並閉環修復；這會成為本 repo 後續判斷「是否仍過度依賴人工或手動 Unity 驗證」的重要標準。

---
*Update this file after every 2 view/browser/search operations*
