# 閒置專案 CI/CD 停用進度

## 2026-05-20

- 建立本次可追溯工作記錄。
- 下一步：盤點 `.github`、workflow、排程、自動 PR 相關設定。
- 完成第一輪搜尋：找到 `.github` 目錄，未發現本機 Codex automation 目錄；
  下一步改用 hidden 掃描 `.github` 內容。
- 已修改 4 個 workflow source，移除背景自動觸發。
- 已用 GitHub CLI 將遠端 4 個 workflow 全部設為 `disabled_manually`。
- 已關閉 PR #1 到 #9，並刪除對應 `codex/docs-garden-*` branch。
- 已同步更新正式文件，標註專案目前閒置與 CI/CD 停用規則。
- 已完成驗證：
  - workflow YAML 解析成功
  - `git diff --check` 通過
  - `python3 Tools/repo_guard.py` 通過
  - 遠端 workflow 全部 `disabled_manually`
  - 遠端 open PR 為空
  - 遠端 queued / in-progress run 為空
