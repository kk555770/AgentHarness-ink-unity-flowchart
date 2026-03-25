# Findings & Decisions

## Requirements
- 把 `GraphToolkitSpec.md` 與 `UIToolkitSpec.md` 放進 `Documentation/`
- 更新對應索引
- 更新正式文件中的引用

## Research Findings
- 目前兩份 spec 檔都在 repo root。
- 正式引用點在：
  - `Documentation/AgentWorkflowRules.md`
  - `Documentation/references/index.md`
  - `Documentation/CanonicalGraphSchema.md`
  - `Documentation/NarrativeGraphArchitecture.md`
- 補充的正式入口同步點：
  - `Documentation/DocsIndex.md`
  - `README.md`
- 舊 `PlanningWithFiles/` 也有歷史引用，但這些屬於過去記錄，不是這輪要清洗的目標。

## Technical Decisions
| Decision | Rationale |
|----------|-----------|
| 更新 `DocsIndex.md` 與 `references/index.md` | 這兩個是使用者點名要同步的索引 |
| `README.md` 一起補入口 | root README 也是活的入口，不補會讓使用者從首頁看不到新位置 |

## Implementation Results
- `GraphToolkitSpec.md` 已移到 `Documentation/GraphToolkitSpec.md`
- `UIToolkitSpec.md` 已移到 `Documentation/UIToolkitSpec.md`
- 已更新正式引用：
  - `Documentation/AgentWorkflowRules.md`
  - `Documentation/references/index.md`
  - `Documentation/CanonicalGraphSchema.md`
  - `Documentation/NarrativeGraphArchitecture.md`
  - `Documentation/DocsIndex.md`
  - `README.md`
- 針對 `Documentation`、`README`、`AGENTS`、`Tools`、`Packages`、`Assets` 的舊路徑搜尋已無殘留

## Issues Encountered
| Issue | Resolution |
|-------|------------|
|       |            |

## Resources
- `GraphToolkitSpec.md`
- `UIToolkitSpec.md`
- `Documentation/DocsIndex.md`
- `Documentation/references/index.md`

## Visual/Browser Findings
- 無
