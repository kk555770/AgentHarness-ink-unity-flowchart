# Findings & Decisions

## Requirements
- 重新完整研讀文章
- 審核先前更新的正確性
- 明確指出符合與不符合之處

## Research Findings
- 文章在 `docs/` 這段強調的是：`AGENTS.md` 只是目錄表，真正重要的是結構化知識庫、freshness/cross-links/ownership/coverage 的機械檢查，以及 doc-gardening agent 會對過時文件開 PR。
- 文章在 `架構與品味` 這段強調的是：自訂 lint 與結構測試要真的守住不變量，而不是只有存在檢查。
- 文章在 `自主性` 與 `熵與垃圾回收` 這段強調的是：背景任務要能掃描偏移、更新品質評級、推動修正，而不是只上傳報告 artifact。
- 最近審核主體是三個 commit：
  - `f7bb7fb`：Unity-first workflow + 文件入口
  - `3cc9971`：spec 收斂 + repo_guard v2 + active exec plan
  - `0762b67`：doc-gardening + generated docs

## Audit Findings Draft
- 高嚴重度：
  - `repo_guard v2` 目前把 freshness 簡化成「有沒有 `最後更新：` 這個字串」，這不等於文章講的 freshness 驗證。
  - `repo_guard v2` 也把 generated docs 當成必備檔存在檢查，但沒有驗證它們是否與最新資料同步，因此容易產生「檔案存在但內容早就漂了」的假安全感。
- 中嚴重度：
  - `docs-garden.yml` 目前只產生 artifact，不會更新 repo、也不會開 PR；如果把它描述成文章那種 doc-gardening agent，就屬於過度宣稱。
  - `generate_test_results_index.py` 只讀本地 `Logs/TestResults/*.xml`；`docs-garden.yml` 也沒有先跑測試或抓 artifact，所以定期 workflow 產出的測試索引通常不會代表最新 CI。
  - `doc_garden.py` 以資料夾實體檔案數量當分類成熟度指標，但目前 repo 很多正式文件仍在 `Documentation/` 根層，由各 `index.md` 去索引，這會讓報告誤判多個分類為 0。
- 低嚴重度 / 正確方向：
  - `AGENTS.md` 改成目錄表是對的。
  - `GraphToolkitSpec.md` / `UIToolkitSpec.md` 移進 `Documentation/` 是對的。
  - `Documentation/exec-plans/active/` 補上至少一份正式 active plan 是對的。

## Technical Decisions
| Decision | Rationale |
|----------|-----------|
| 先讀文章，再讀 commit 與現況 | 避免又用印象評論 |

## Issues Encountered
| Issue | Resolution |
|-------|------------|
|       |            |

## Resources
- OpenAI harness engineering 文章
- 最近 harness 相關 commit
- `Documentation/`
- `.github/workflows/`
- `Tools/repo_guard.py`

## Visual/Browser Findings
- 文章原文明確寫出：專用 linter/CI 會驗證 docs 是否最新、交叉連結、結構正確，doc-gardening agent 會掃描過時文件並開 PR；這些都比目前 repo 的做法更深一層。
