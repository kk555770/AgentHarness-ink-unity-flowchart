# QUALITY_SCORE

> 文件負責人：harness
> 最後更新：2026/03/25

## 目前盤點

- 知識地圖：黃
  - 正式入口已補，但分類仍在收斂中
- 文件所有權：黃
  - `Documentation/DocsOwnership.md` 已建立
  - `Documentation/**/*.md` 已補 `> 文件負責人：...`
  - `repo_guard v4` 與 `doc_garden_report.md` 已開始檢查 owner header 與 owner coverage
- golden principles：黃
  - `Documentation/design-docs/core-beliefs.md` 已建立正式入口
  - repo-local truth、control plane first、機械化護欄已寫回文件
- 計畫版本化：黃
  - `PlanningWithFiles/` 已存在，且已開始升格到 `Documentation/exec-plans/active/harness_system_of_record_v2.md`
- 契約與測試：綠
  - canonical JSON contract 與 current projection contract 已有文件與測試
- `ImportProjection`：黃
  - `flowchart-json` 已納入 `CurrentMilestones.md`、`exec-plans`、`CanonicalGraphApiSpec.md`、`CanonicalGraphJsonContract.md` 與 control-plane 實作
  - 目前仍缺 `ink` import、legacy mapping warnings 與更多 target 的 round-trip
- Ink compile validation：黃
  - compile probe 已進 canonical core，`ValidateProjection(Ink)` 不再只靠 Editor bootstrap
  - 目前仍缺跨 worktree observability 與更多非本地 harness 的統一驗證入口
- 機械式護欄：黃
  - `repo_guard v4` 已開始補 docs freshness、index 內容量、cross-links、active exec plan、ownership、workflow permissions 檢查
  - freshness 現在是用 watched paths 的 git 最新日期比對，不再只是看有沒有 `最後更新：`
  - 仍缺更深層的 code graph guard
- doc-gardening：黃
  - 已補 `Tools/doc_garden.py`、`Tools/fetch_ci_test_results.py` 與 `.github/workflows/docs-garden.yml`
  - 目前會抓最新成功 CI run 的 artifact、重建 generated docs、並自動開 PR
- evidence bundle：黃
  - `Documentation/generated/test_results_index.md` 已升級成 Unity evidence index
  - 現在會把 XML、console 摘要、log source、CI job 狀態收斂到同一頁
  - generated docs 不再只知道「有沒有測試」，也知道「發生了什麼」
- auto-fix harness：黃
  - workflow 已改成 Unity-first，而且缺少 `UNITY_LICENSE` 時會 fail closed
  - 完整 verify 仍需要有效 `UNITY_LICENSE`

## 補充

- `test_results_index.md` 現在只接受 CI artifact 或明確的本地輸入來源
- 若目前還沒有新的 Unity 測試 artifact，generated docs 會顯示 `Missing`，這是 bootstrap 狀態，不再是假裝最新 CI 真相
- `doc_garden_report.md` 現在會列出 owner coverage 與缺 owner 文件
