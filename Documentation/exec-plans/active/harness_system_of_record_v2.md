# Harness System Of Record V2

> 最後更新：2026/03/25
> 角色：**正式進行中計畫 / Active Exec Plan**

## 目標

把目前 repo 從「已有文件地圖與 guard 骨架」推進到更接近 harness engineering 文章的第二階段：

- `Documentation/` 不只是一張地圖，還要有可直接引用的正式內容
- `repo_guard` 不只檢查檔案存在，還要開始守 freshness / cross-links / active plans
- `exec-plans/active/` 不再是空入口

## 目前已完成

- `AGENTS.md` 已收斂成目錄表
- `Documentation/` 已有高階入口與分類索引
- `CurrentMilestones.md` 已把 Batch 8~11A 拉回正式文件
- `CI.yml` / `codex-auto-fix.yml` 已改成 Unity-first
- `repo_guard v1` 已守 docs 入口 / asmdef / `.cs` 檔案大小

## 這一輪要補的事

1. `repo_guard v2`
   - 補 `最後更新` 標頭檢查
   - 補正式 cross-links 檢查
   - 補 `Documentation/exec-plans/active/` 至少一份正式 active plan 檢查

2. 文件升格
   - 把真正正在進行的 harness 收斂工作寫成正式 active exec plan
   - 讓 `PLANS.md`、`QUALITY_SCORE.md`、`RELIABILITY.md` 指回這份計畫

3. 留給下一輪的缺口
   - docs ownership
   - doc-gardening 背景清理機制
   - 更深層 code graph / naming / structured logging guard
   - 不依賴 `UNITY_LICENSE` 的更穩定雲端回饋閉環
   - UI / log / metrics / trace 層級的 agent 可觀測性

## 驗收標準

- `Documentation/exec-plans/active/` 至少有一份非 `index.md` 的正式計畫
- `python3 Tools/repo_guard.py` 能檢查 freshness / cross-links / active plan
- `PLANS.md`、`QUALITY_SCORE.md`、`RELIABILITY.md` 已把目前活躍計畫寫回正式文件
