# 閒置專案 CI/CD 停用發現

## 初始狀態

- 工作目錄：`/Users/arcumit/Documents/GitHub/AgentHarness-ink-unity-flowchart`
- 分支：`master`
- 已知未追蹤項目：
  `PlanningWithFiles/20260325235455_public_repo_rename_master_sync/`
- 本次不會碰既有未追蹤項目。

## 發現

- Repo 內存在 `.github` 目錄，需要專門掃 hidden 內容確認是否有
  GitHub Actions workflow。
- 初步全 repo 關鍵字搜尋未看到 `gh pr`、auto-merge 或明顯自動開 PR 腳本。
- `/Users/arcumit/.codex/automations` 不存在；目前沒有在該預設路徑發現
  Codex 本機 automation 持續針對此 repo 跑。
- `Assets/DEV_README.md` 與 `Assets/Publishing/Editor/Tools/PublishingTools.cs`
  提到 release，但看起來是 Unity 編輯器內的手動發佈工具，不是自動 CI/CD。
- `.github/workflows/CI.yml` 原本會在 `push` 與 `pull_request` 自動跑。
- `.github/workflows/docs-garden.yml` 原本會在 `CI` completed、每週排程、
  手動觸發時執行，且會使用 `peter-evans/create-pull-request@v6`
  自動建立 docs-garden PR。
- `.github/workflows/codex-auto-fix.yml` 原本會在 `CI` failure 後自動跑
  Codex，成功後會自動建立修復 PR。
- `.github/workflows/Update-UPM-Branch.yml` 原本會在版本 tag push 時自動跑。
- 遠端 workflow 停用後狀態：
  - `CI`：`disabled_manually`
  - `Update-UPM-Branch`：`disabled_manually`
  - `Codex Auto-Fix on Failure`：`disabled_manually`
  - `Docs Garden`：`disabled_manually`
- 驗證時沒有 queued / in-progress GitHub Actions run。
- 原本存在 9 個由 `app/github-actions` 建立的 open docs-garden PR
  （#1 到 #9）；已全部關閉並刪除對應 branch。
- 本地 workflow 觸發字面檢查沒有再找到 top-level `push`、`pull_request`、
  `schedule`、`workflow_run` 觸發。
- Ruby YAML 解析 4 個 workflow 成功。
- `python3 Tools/repo_guard.py` 通過。
- 遠端 open PR 查詢為空。
- 遠端 queued / in-progress run 查詢為空。
