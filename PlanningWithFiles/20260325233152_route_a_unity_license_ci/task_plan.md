# Task Plan

> 任務：實作 A 路線，讓 GitHub-hosted runner 在設定 `UNITY_LICENSE` 後形成可追溯的 Unity 驗證閉環
> 建立時間：2026/03/25 23:31:52

## Goal

把目前「缺少 `UNITY_LICENSE` 時 fail closed」的基線，收斂成更接近文章要求的 GitHub-hosted Unity feedback loop：

- 統一 repo guard + Unity verify 的主要入口
- 讓 CI / auto-fix / generated docs 對同一批 artifact 與 provenance 對齊
- 把 secret 需求、run metadata、evidence manifest 與正式文件寫清楚

## Phase Status

| Phase | 狀態 | 說明 |
| --- | --- | --- |
| 1 | 已完成 | 建立任務文件並盤點現況 |
| 2 | 已完成 | 實作 workflow / evidence / verify 收斂 |
| 3 | 已完成 | 執行本地驗證與 generated docs 更新 |
| 4 | 進行中 | 回寫 `Documentation/` 並確認外部 blocker |

## Work Items

- [x] 建立這輪 `PlanningWithFiles` 三件套
- [x] 盤點 `CI.yml` / `codex-auto-fix.yml` / `docs-garden.yml` / `run_tests.sh`
- [x] 盤點 GitHub secret 實際可操作邊界
- [x] 收斂 workflow verify gate
- [x] 補強 artifact manifest / provenance
- [x] 檢查 generated docs 是否可讀取新增證據
- [x] 跑 `python3 Tools/repo_guard.py`
- [x] 跑必要的本地驗證
- [x] 回寫 `RELIABILITY.md` / `QUALITY_SCORE.md` / `SECURITY.md` / active exec plan
- [ ] 取得使用者對敏感 GitHub secret 寫入的明確確認
- [ ] 若獲確認，將 Unity secrets 寫入 fork repo 並觸發實際 CI run

## Verification

- `python3 Tools/repo_guard.py`
- 必要時 `bash Tools/run_tests.sh`
- 視修改面決定是否重跑 generated docs 腳本

## Open Questions

1. 這個工作區是否已經有可用的 GitHub 驗證身分，可直接設定 repo secret？
2. A 路線是否應把本地與 CI 驗證入口合流成單一腳本？
3. docs-garden / test index 是否還缺 run provenance 欄位？
4. 若要把 A 路線做成真正可實跑，如何安全取得 `UNITY_EMAIL` / `UNITY_PASSWORD` 或 `UNITY_SERIAL`？

## Errors Encountered

| Error | Attempt | Resolution |
| --- | --- | --- |
