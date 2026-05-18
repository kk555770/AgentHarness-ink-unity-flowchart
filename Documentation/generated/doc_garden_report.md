# doc garden report

> 文件負責人：harness
> 最後更新：2026/05/18
> 來源：`python3 Tools/doc_garden.py`

## 摘要

- Documentation Markdown 總數：38
- 缺少 `最後更新` 標頭：0
- 缺少 `文件負責人` 標頭：0
- freshness checks 覆蓋文件數：24
- workflow permissions 覆蓋 workflow 數：3

## 分類內容盤點

| 類別 | index 指到的正式文件數 |
|------|------------------------|
| `DocsIndex` | 15 |
| `design-docs` | 13 |
| `exec-plans` | 7 |
| `exec-plans/active` | 4 |
| `exec-plans/completed` | 3 |
| `product-specs` | 6 |
| `references` | 6 |
| `generated` | 2 |

## 缺少 `最後更新` 的文件

- 無

## 文件負責人覆蓋

- `architecture`：6
- `authoring`：6
- `harness`：17
- `projection`：3
- `reference`：4
- `runtime`：1
- `security`：1

## 缺少 `文件負責人` 的文件

- 無

## 可能已過時的文件

- 無

## Workflow / Security 檢查

| workflow | 狀態 | 實際 permissions | 預期 permissions |
|----------|------|------------------|------------------|
| `.github/workflows/CI.yml` | OK | `checks: write, contents: read` | `checks: write, contents: read` |
| `.github/workflows/docs-garden.yml` | OK | `actions: read, contents: write, pull-requests: write` | `actions: read, contents: write, pull-requests: write` |
| `.github/workflows/codex-auto-fix.yml` | OK | `actions: read, contents: write, pull-requests: write` | `actions: read, contents: write, pull-requests: write` |

## 備註

- 這份報告現在以分類 index 指到的正式文件為準，不再只數資料夾內有幾個 markdown。
- 若文件日期早於 watched paths 的最新 git 變更日期，表示它可能已過時，應由 docs-garden 回寫或開 PR。
- Workflow 權限檢查會與 `repo_guard` 同源，避免 docs 與 guard 對安全邊界的判斷不一致。
