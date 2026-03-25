# Findings & Decisions

## Requirements
- 不要只做 spec 搬遷
- 把前面承諾的主缺口開始真正補起來
- 優先處理 active plan 與 repo guard 深化

## Research Findings
- `Documentation/exec-plans/active/` 原本只有 `index.md`，沒有任何正式 active plan。
- `repo_guard v1` 只有 docs 必備檔、關鍵字、asmdef exact references、檔案大小檢查。
- 文章更在意的是：system-of-record、freshness、cross-links、active/completed plans、機械式護欄。

## Technical Decisions
| Decision | Rationale |
|----------|-----------|
| 新增 `Documentation/exec-plans/active/harness_system_of_record_v2.md` | 先讓 active plans 不再只是空目錄 |
| 在 `repo_guard.py` 新增 `last_updated_headers` 與 `min_non_index_docs` | 用最小成本補上 freshness 與 active plan existence |
| 用 `required_contains` 擴充 cross-links | 不需先引入更重的 parser，就能先守住正式入口連結 |

## Issues Encountered
| Issue | Resolution |
|-------|------------|
| `git diff --check` 抓到 `ARCHITECTURE.md` 尾端空白 | 修正後再次通過 |

## Resources
- `Tools/repo_guard.py`
- `Tools/repo_guard_rules.json`
- `Documentation/exec-plans/active/index.md`
- `Documentation/PLANS.md`
- `Documentation/QUALITY_SCORE.md`
- `Documentation/RELIABILITY.md`

## Visual/Browser Findings
- 無
