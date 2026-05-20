# Task Plan

> 任務：把公開 repo 改名為 `AgentHarness-ink-unity-flowchart`，並把目前最新內容併回 `master`
> 建立時間：2026/03/25 23:54:55

## Goal

收斂公開 repo 的對外名稱與分支狀態：

- GitHub 公開 repo 改名為 `AgentHarness-ink-unity-flowchart`
- 沿用 `master` 作為公開預設分支
- 把目前最新內容從 `codex/private-harness-continuation` 併回公開 `master`
- 本地 public 工作目錄與 remote URL 一起對齊新名稱

## Phase Status

| Phase | 狀態 | 說明 |
| --- | --- | --- |
| 1 | 進行中 | 盤點分支祖先關係與新名稱可用性 |
| 2 | 未開始 | 改 GitHub 公開 repo 名稱並對齊 remote |
| 3 | 未開始 | 把最新內容推回公開 `master` |
| 4 | 未開始 | 對齊本地 public 工作目錄名稱與最終驗證 |

## Work Items

- [ ] 建立本輪 `PlanningWithFiles` 三件套
- [ ] 確認 `master` 是否是最新分支的祖先
- [ ] 確認 `AgentHarness-ink-unity-flowchart` 尚未被占用
- [ ] 改 public repo 名稱
- [ ] 更新 public repo `origin` URL
- [ ] 把 `codex/private-harness-continuation` 的最新內容推到 public `master`
- [ ] 驗證公開 repo 的 default branch / remote / branch status
- [ ] 視需要對齊本地資料夾名稱

## Verification

- `git rev-list --left-right --count master...codex/private-harness-continuation`
- `git merge-base --is-ancestor master codex/private-harness-continuation`
- `gh repo view kk555770/AgentHarness-ink-unity-flowchart --json ...`
- `git push origin codex/private-harness-continuation:master`
- `git status --short --branch`

## Open Questions

1. 本地 public 工作目錄是否也要同步改成 `AgentHarness-ink-unity-flowchart`？
2. push 完公開 `master` 後，是否要保留 `origin/arcumit/CodexInk` 分支不動？

## Errors Encountered

| Error | Attempt | Resolution |
| --- | --- | --- |
