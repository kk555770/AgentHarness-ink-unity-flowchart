# QUALITY_SCORE

> 最後更新：2026/03/25

## 目前盤點

- 知識地圖：黃
  - 正式入口已補，但分類仍在收斂中
- 計畫版本化：黃
  - `PlanningWithFiles/` 已存在，且已開始升格到 `Documentation/exec-plans/active/harness_system_of_record_v2.md`
- 契約與測試：綠
  - canonical JSON contract 與 current projection contract 已有文件與測試
- 機械式護欄：黃
  - `repo_guard v2` 已開始補 docs freshness / cross-links / active exec plan 檢查
  - 仍缺更深層的 ownership 與 code graph guard
- doc-gardening：黃
  - 已補 `Tools/doc_garden.py` 與 `.github/workflows/docs-garden.yml`
  - 目前先產生報告與 artifact，還沒有自動開 PR
- auto-fix harness：黃
  - workflow 已改成 Unity-first
  - 完整 verify 仍依賴 `UNITY_LICENSE`
