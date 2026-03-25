# QUALITY_SCORE

## 目前盤點

- 知識地圖：黃
  - 正式入口已補，但分類仍在收斂中
- 計畫版本化：黃
  - `PlanningWithFiles/` 已存在，正式升格流程剛補上
- 契約與測試：綠
  - canonical JSON contract 與 current projection contract 已有文件與測試
- 機械式護欄：黃
  - `Tools/repo_guard.py` 已補上 docs 入口 / asmdef / 檔案大小 gate
  - 仍缺更深層的 freshness、cross-links 與 code graph guard
- auto-fix harness：黃
  - workflow 已改成 Unity-first
  - 完整 verify 仍依賴 `UNITY_LICENSE`
