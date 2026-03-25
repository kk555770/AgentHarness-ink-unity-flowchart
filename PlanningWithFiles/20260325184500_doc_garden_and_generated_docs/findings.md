# Findings & Decisions

## Requirements
- 補上 doc-gardening 的最小可執行版本
- 讓 `Documentation/generated/` 有真的內容
- 建立週期性 workflow

## Research Findings
- `Documentation/generated/index.md` 目前還是占位，尚未列出固定 generated docs。
- `Logs/TestResults/` 目前已有：
  - `OpsidanosInk_EditMode.xml`
  - `OpsidanosInk_PlayMode.xml`
- `Documentation/` 中目前缺少 `最後更新` 的 Markdown 只有四份：
  - `Documentation/AgentWorkflowRules.md`
  - `Documentation/GraphToolkitSpec.md`
  - `Documentation/InkPlayerWindow.md`
  - `Documentation/UIToolkitSpec.md`

## Technical Decisions
| Decision | Rationale |
|----------|-----------|
| 先補 `AgentWorkflowRules.md`、`GraphToolkitSpec.md`、`UIToolkitSpec.md` 的標頭 | 這些都屬於活文件，降低 doc-garden 雜訊 |
| `InkPlayerWindow.md` 先留在報告中 | 這份偏 legacy / reference，先不要順手大洗 |
| workflow 先跑 test results，再跑 doc garden | 讓 doc garden report 看到最新 generated docs 狀態 |

## Implementation Results
- 已新增：
  - `Tools/doc_garden.py`
  - `Tools/generate_test_results_index.py`
  - `.github/workflows/docs-garden.yml`
  - `Documentation/generated/doc_garden_report.md`
  - `Documentation/generated/test_results_index.md`
- 已更新：
  - `Documentation/generated/index.md`
  - `Documentation/QUALITY_SCORE.md`
  - `Documentation/RELIABILITY.md`
  - `Documentation/exec-plans/tech-debt.md`
  - `Documentation/AgentWorkflowRules.md`
  - `Documentation/GraphToolkitSpec.md`
  - `Documentation/UIToolkitSpec.md`
  - `Tools/repo_guard_rules.json`
- doc garden report 目前指出的剩餘缺口：
  - `Documentation/InkPlayerWindow.md` 缺少 `最後更新`
  - `design-docs`、`exec-plans/completed`、`product-specs`、`references` 仍然沒有非 index 正式文件

## Issues Encountered
| Issue | Resolution |
|-------|------------|
|       |            |

## Resources
- `Documentation/generated/index.md`
- `Logs/TestResults/OpsidanosInk_EditMode.xml`
- `Logs/TestResults/OpsidanosInk_PlayMode.xml`

## Visual/Browser Findings
- 無
