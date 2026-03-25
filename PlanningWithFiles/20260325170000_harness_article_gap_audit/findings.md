# Findings & Decisions

## Requirements
- 完整研讀 OpenAI harness engineering 文章。
- 對照目前 repo 現況。
- 說清楚現在還缺什麼。
- 提出下一步應該做什麼。

## Research Findings
- 文章把 `AGENTS.md` 定位成約 100 行的目錄表，不是百科全書；正式知識應放在結構化 `docs/`，並作為 system of record。
- 文章明確要求 `docs/` 內要有可導航的知識分層，例如 design-docs、exec-plans、generated、product-specs、references，以及品質/可靠性/安全等高階入口。
- 文章把 plans 視為一級工件；小改動可用輕量計畫，複雜工作則要有版本化的 exec plans、進度與決策記錄，而且 active/completed/tech debt 集中管理。
- 文章強調漸進式揭露：agent 先看小而穩定的入口，再被導到更深入的文件，而不是一開始吃大手冊。
- 文章要求用專用 linter 與 CI 檢查 docs 的 freshness、cross-links、coverage、ownership 與結構；另有定期 doc-gardening agent 掃描過時文件並開 PR。
- 文章把應用程式可讀性視為核心能力：UI 要可被 agent 驅動與驗證，日誌、指標、追蹤要能以查詢語言直接檢索。
- 文章要求把架構與「品味不變量」寫成可機械執行的 guard，例如檔案大小限制、命名規則、結構化記錄、平台可靠性要求與結構測試。
- 文章指出真正的缺口訊號不是「模型不夠努力」，而是環境少了工具、文件、護欄或回饋迴路。
- repo 目前的 `AGENTS.md` 已是 63 行目錄表，`README.md` 是 67 行入口，方向接近文章的小入口設計。
- repo 已補高階入口：`ARCHITECTURE.md`、`PLANS.md`、`PRODUCT_SENSE.md`、`QUALITY_SCORE.md`、`RELIABILITY.md`、`SECURITY.md`、`DocsIndex.md`、`CurrentMilestones.md`。
- repo 已有 `repo_guard.py`，但目前只檢查 docs 必備檔、關鍵字存在、少數 asmdef exact references、以及 `.cs` 檔案行數上限。
- `Documentation/design-docs/`、`Documentation/product-specs/`、`Documentation/references/`、`Documentation/generated/` 幾乎都只有 `index.md`；`generated/` 明寫目前還沒有固定 generated docs 清單。
- `Documentation/exec-plans/active/` 也只有 `index.md`，目前沒有真正的正式 active plan 文件；進行中的細部規劃仍主要留在 `PlanningWithFiles/`。
- `Documentation/references/` 目前只有單一 `index.md`，尚未像文章示例那樣有可直接給 agent 消化的 LLM-oriented reference files。
- `Documentation/SECURITY.md` 明確寫出目前沒有成形的安全入口文件，也沒有 threat model 或 secret-handling 規範。
- `CI.yml` 與 `codex-auto-fix.yml` 已改成 Unity-first，並先跑 `repo_guard.py`，這一點已比之前更接近文章的「先驗證環境與護欄」。
- 但 GitHub Actions 的 Unity 測試與 auto-fix 完整 verify 仍依賴 `UNITY_LICENSE`；沒有授權時只剩 repo guard 與說明訊息。
- repo 有 Unity MCP 依賴、PlayMode / EditMode 測試、DeveloperModeOutputContract 與 Playwright/MCP 對接語意文件，但沒有文章那種本地 logs/metrics/traces 查詢堆疊，也沒有影片錄製/自動查詢能力。
- 量化上，`Documentation/` 共有 32 個檔案；新補的知識地圖與分類索引合計 362 行，但五個分類資料夾總共只有 8 個檔案、124 行，代表分類骨架遠多於分類內容。

## Technical Decisions
| Decision | Rationale |
|----------|-----------|
| 以文章中的具體結構與護欄為主，不用自己的抽象版本 | 使用者明確要求先忠實對照原文 |
| 先把文章主張轉成可檢查清單，再盤 repo | 避免只做印象式評論 |
| 缺口判斷以「是否已機械化、是否已有實體內容、是否可被 agent 直接使用」為主 | 這三點最貼近文章的 harness 標準 |

## Issues Encountered
| Issue | Resolution |
|-------|------------|
|       |            |

## Resources
- OpenAI harness engineering 文章：https://openai.com/zh-Hant/index/harness-engineering/
- Repo 入口：`AGENTS.md`
- 正式文件：`Documentation/`
- 文章重點段落：提高應用程式可讀性、將程式碼庫知識設為系統的記錄依據、強制規範架構與品味

## Visual/Browser Findings
- 文章目錄直接把重點列成：提升應用程式可讀性、讓 repo 知識成為 system of record、強制規範架構與品味、提升自主性與回饋迴路。
- 文中示例的知識庫版面不是單頁入口，而是資料夾分層加高階入口並存。

## Gap Draft
- 已有：
  - 短 `AGENTS.md`、`Documentation/` 入口骨架、`CurrentMilestones.md`
  - Unity-first CI / auto-fix
  - repo guard v1
  - EditMode / PlayMode / contract 測試
- 半完成：
  - `docs/` 分類結構已建，但多數分類仍是入口殼
  - plans 已被承認是一級工件，但正式 active plans 仍未進 `Documentation/exec-plans/active/`
  - 品質 / 可靠性 / 安全文件已建立，但安全仍接近占位說明
- 明顯缺：
  - docs freshness / ownership / cross-links / coverage 機械檢查
  - doc-gardening 背景清理機制
  - 真正可直接給 agent 消化的 references/generated 內容
  - 更深層結構測試與命名/日誌/契約 guard
  - UI/log/metrics/trace 層級的 agent 可觀測堆疊
  - 不依賴 `UNITY_LICENSE` 的穩定雲端回饋閉環
