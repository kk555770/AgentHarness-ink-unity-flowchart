# Harness System Of Record V2

> 最後更新：2026/03/25
> 角色：**正式進行中計畫 / Active Exec Plan**

## 目標

把目前 repo 從「已有文件地圖與 guard 骨架」推進到更接近 harness engineering 文章的第二階段：

- `Documentation/` 不只是一張地圖，還要有可直接引用的正式內容
- `repo_guard` 不只檢查檔案存在，還要守 freshness / cross-links / active plans
- `exec-plans/active/` 不再是空入口
- generated docs 要能對回 CI 真相，而不是本地快照
- doc-gardening 要能回寫 repo / 開 PR，而不只是吐 artifact

## 目前已完成

- `AGENTS.md` 已收斂成目錄表
- `Documentation/` 已有高階入口與分類索引
- `CurrentMilestones.md` 已把 Batch 8~11A 拉回正式文件
- `CI.yml` / `codex-auto-fix.yml` 已改成 Unity-first
- `repo_guard v1` 已守 docs 入口 / asmdef / `.cs` 檔案大小
- `repo_guard v2` 已守 `最後更新` 標頭 / cross-links / active exec plan 入口

## 這一輪新完成

1. `repo_guard v3`
   - 補 index 指到的正式文件數檢查
   - 補 watched paths 的 git 日期比對，讓 freshness 變成真的機械檢查

2. generated docs 流程
   - 補 `Tools/fetch_ci_test_results.py`
   - `test_results_index.md` 改成只接受本地明確輸入或 CI artifact 輸入
   - `doc_garden_report.md` 改成看分類 index 指到的正式文件，不再只數資料夾裡有幾個檔

3. doc-gardening 回寫
   - `docs-garden.yml` 現在會抓最新成功 CI run 的測試 artifact
   - 重建 generated docs
   - 若有變更就自動開 PR

## 決策紀錄

- 不再把本地 `Logs/TestResults` 直接描述成最新 CI 真相
- 如果目前找不到新版 Unity 測試 artifact，`test_results_index.md` 必須顯示 `Missing`
- freshness 一律以文件 `最後更新` 日期對 watched paths 的最新 git 日期判斷

## 留給後續的缺口

- docs ownership
- 更深層 code graph / naming / structured logging guard
- 不依賴 `UNITY_LICENSE` 的更穩定雲端回饋閉環
- UI / log / metrics / trace 層級的 agent 可觀測性

## 驗收標準

- `Documentation/exec-plans/active/` 至少有一份非 `index.md` 的正式計畫
- `python3 Tools/repo_guard.py` 能檢查真正的 freshness / index counts / cross-links / active plan
- `PLANS.md`、`QUALITY_SCORE.md`、`RELIABILITY.md` 已把目前活躍計畫寫回正式文件
- `docs-garden.yml` 已能回寫 generated docs 並建立 PR
