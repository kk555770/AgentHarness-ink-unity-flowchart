# Progress：strict_harness_alignment

## 2026-03-25 20:10

- 建立本輪 `PlanningWithFiles/20260325201000_strict_harness_alignment/`
- 重新讀取 repo `AGENTS.md`
- 重新讀取 `planning-with-files` skill
- 重新打開 OpenAI harness engineering 文章並對照上一輪審核結果

## 2026-03-25 20:16

- 讀取 `Tools/repo_guard.py`
- 讀取 `Tools/repo_guard_rules.json`
- 讀取 `Tools/doc_garden.py`
- 讀取 `Tools/generate_test_results_index.py`
- 讀取 `.github/workflows/docs-garden.yml`
- 讀取 `Documentation/generated/` 目前輸出
- 讀取 `Documentation/QUALITY_SCORE.md`、`Documentation/RELIABILITY.md`、`Documentation/exec-plans/tech-debt.md`
- 讀取各分類 index，確認分類目前是清單式路徑而不是 markdown link

## 2026-03-25 20:28

- 新增 `Tools/doc_guard_utils.py`，統一處理 `最後更新`、index 路徑解析、git 最新變更日期
- 新增 `Tools/fetch_ci_test_results.py`，改成可從 GitHub Actions 最新成功 CI run 抓測試 artifact
- 改寫 `Tools/generate_test_results_index.py`，支援 `--results-root` 與 CI source metadata
- 改寫 `Tools/doc_garden.py`，分類統計改看 index 指到的正式文件，並補 stale docs 掃描
- 擴充 `Tools/repo_guard.py`，新增 indexed doc counts 與真正的 freshness 檢查
- 擴充 `Tools/repo_guard_rules.json`，加入 `indexed_doc_counts` 與 `freshness_checks`
- 更新 `.github/workflows/CI.yml`，補測試結果 artifact upload
- 更新 `.github/workflows/docs-garden.yml`，改成抓 CI artifact、重建 generated docs、建立 PR

## 2026-03-25 21:46

- 先用本地 `Logs/TestResults` 驗證新版 `generate_test_results_index.py`
- `repo_guard` 第一次失敗，只剩 4 個文件同步缺口：`generated/index.md`、`QUALITY_SCORE.md`、`RELIABILITY.md`、`InkPlayerWindow.md`
- 補上正式文件同步與 `InkPlayerWindow.md` 的 `最後更新`
- 本地執行 `fetch_ci_test_results.py`，確認目前最新成功 CI run `23543399252` 尚無 Unity 測試 artifact
- 以 `Artifacts/CI/TestResults/_source.json` 為 truth source 重新生成 `test_results_index.md`
- 重跑 `python3 Tools/doc_garden.py`
- 重跑 `python3 Tools/repo_guard.py`，已通過
- 重跑 `git diff --check`，已通過

## 2026-03-25 21:58

- 最終再審核時抓到兩個剩餘結構性問題：
  - metadata 存在但 artifact 為空時，`generate_test_results_index.py` 理論上可能誤讀舊 XML
  - `CI.yml` 的 EditMode / PlayMode 共用 `artifactsPath`
- 第二輪修正：
  - `Tools/generate_test_results_index.py` 改成 metadata 存在就只讀 `result_roots`
  - `Tools/fetch_ci_test_results.py` 查詢成功 run 範圍從 `per_page=20` 擴到 `per_page=100`
  - `.github/workflows/CI.yml` 改成 `artifacts/editmode`、`artifacts/playmode`
- 第二輪後重跑：
  - `python3 Tools/generate_test_results_index.py --results-root Artifacts/CI/TestResults`
  - `python3 Tools/doc_garden.py`
  - `python3 Tools/repo_guard.py`
  - `git diff --check`
- 結果：全部通過，最終再審核未再發現新的阻塞 findings
