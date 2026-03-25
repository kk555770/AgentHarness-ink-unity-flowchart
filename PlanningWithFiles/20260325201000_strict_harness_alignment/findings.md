# Findings：strict_harness_alignment

## 2026-03-25 初始審核摘要

1. `repo_guard` 只檢查 `最後更新：` 字串存在，還不是真正的 freshness validation。
2. `docs-garden.yml` 只會產生 artifact，不會回寫 repo 或開 PR。
3. `generate_test_results_index.py` 只讀本地 `Logs/TestResults`，不是可重建的 CI 真相。
4. `doc_garden.py` 只數資料夾內的 markdown，沒有把 index 指向的正式文件算進去。

## 本輪對齊原則

- 不把「第一版骨架」描述成「已完全對齊文章」。
- 能機械檢查的規則要寫進工具或 workflow。
- generated docs 必須能由固定流程重建。
- doc-gardening 至少要做到可自動回寫，不再只是報告。

## 2026-03-25 第二輪閱讀補充

- `Tools/repo_guard.py` 目前的 freshness 仍只有 `最後更新：` 字串存在性檢查。
- `Tools/doc_garden.py` 現在是看資料夾內 markdown 數量，不是看分類 index 指到哪些正式文件。
- `.github/workflows/docs-garden.yml` 目前只有 `contents: read`，沒有 commit / PR 能力。
- `Tools/generate_test_results_index.py` 目前固定讀 `Logs/TestResults/*.xml`，沒有明確的 CI artifact 輸入模式。
- `Documentation/design-docs/index.md`、`Documentation/product-specs/index.md`、`Documentation/references/index.md` 採用的是清單式路徑，不是 markdown link；分類統計腳本要支援這種格式。

## 2026-03-25 第一輪實作後再審核

- 已修正：`repo_guard` 現在有 watched paths + git 日期 freshness 檢查。
- 已修正：`docs-garden.yml` 現在能回寫 repo 並建立 PR。
- 已修正：`test_results_index.md` 現在以 CI artifact metadata 為 truth source；若沒有 artifact，會顯示 `Missing`。
- 已修正：`doc_garden_report.md` 現在按分類 index 指到的正式文件計數。
- 新發現：`generate_test_results_index.py` 在 metadata 存在但本輪 artifact 為空時，理論上可能誤讀舊 XML。
- 新發現：`CI.yml` 的 EditMode / PlayMode 共用 `artifactsPath`，路徑語意不夠乾淨。

## 2026-03-25 第二輪實作後最終再審核

- 已修正：`generate_test_results_index.py` 只要偵測到 `_source.json`，就只讀 metadata 指定的 `result_roots`，不再回頭掃舊 XML。
- 已修正：`CI.yml` 改成 `artifacts/editmode` 與 `artifacts/playmode` 分離路徑。
- 最終判定：本輪審核指向的結構性缺口已補齊，沒有再發現新的阻塞 findings。
