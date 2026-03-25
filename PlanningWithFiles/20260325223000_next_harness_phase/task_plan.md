# 任務計畫：next_harness_phase

## 目標

- 重新完整閱讀 OpenAI harness engineering 文章。
- 在目前已完成的 harness 基礎上，找出下一個最缺且最值得落地的能力。
- 直接完成該能力的第一版落地，並更新正式文件、guard 與 workflow。
- 完成後再次審核，確認這一步確實有把 repo 往文章目標推進。

## 階段

| 階段 | 狀態 | 說明 |
| --- | --- | --- |
| 1 | 已完成 | 重新讀文章、盤點下一步候選 |
| 2 | 已完成 | 確定本輪實作範圍 |
| 3 | 已完成 | 實作與同步文件 |
| 4 | 已完成 | 驗證與再審核 |

## 候選方向

- docs ownership 與 golden principles
- 更深層的架構 / naming / structured logging guard
- entropy cleanup 背景任務
- agent-readable observability / log query 入口

## 本輪定案

- `Documentation/DocsOwnership.md`
- `Documentation/design-docs/core-beliefs.md`
- `Documentation/**/*.md` 的 `文件負責人` header
- `repo_guard` ownership 檢查
- `doc_garden` owner coverage

## 驗證

- `python3 Tools/repo_guard.py`
- `git diff --check`
- 本輪新增的腳本或 workflow 驗證
