# SECURITY

> 文件負責人：security
> 最後更新：2026/03/25

## 目的

把這個 repo 在 agent-first 開發下最容易出事的安全邊界寫清楚，避免：

- 祕密外洩到文件、artifact、log 或 generated docs
- workflow 權限過大或來源不明
- agent 在沒有明確 guard 的情況下接觸外部服務
- 本地 / CI evidence 被誤當成可公開或可長期保留的資料

## Threat Model

### 1. Secrets / Credentials

高風險項目包含：

- `OPENAI_API_KEY`
- `UNITY_LICENSE`
- GitHub token / Actions 權限
- 任意未來接上的外部 API token

主要風險：

- 被寫進 `Documentation/`、`PlanningWithFiles/`、測試輸出、artifact 或 Unity log
- 被 agent 直接印到 Console 或 PR 內容

### 2. Workflow / Automation

高風險情境包含：

- workflow 沒有明確 `permissions`
- 寫入型 workflow 權限大於實際需要
- auto-fix / docs-garden 在來源不明的情況下回寫 repo

### 3. External Services

目前 repo 內已可見的外部服務風險面：

- GitHub Actions / GitHub API
- OpenAI API（`codex-auto-fix.yml`）
- 任何未來透過 CI 或 editor script 接上的外部 API

原則：

- 若不是必要服務，就不要讓 workflow 或 agent 碰到
- 若必須碰，文件、workflow 與 guard 都要留下明確痕跡

## Repo-local Rules

### Secrets Handling

- 不可把 secret、license、token 原文寫進：
  - `Documentation/`
  - `PlanningWithFiles/`
  - `Logs/`
  - generated docs
  - PR 描述
- 若 log / artifact 需要保留證據，只保留狀態、來源與最少必要 metadata，不保留敏感值本身。

### Workflow Permissions

- 每個 workflow 都必須明確宣告 `permissions:`，禁止依賴隱含預設值。
- 寫入型 workflow 必須解釋為什麼需要 `contents: write` 或 `pull-requests: write`。
- 驗證型 workflow 優先使用 read-only 權限。

### External-service Usage

- `codex-auto-fix.yml` 只能在 CI failure 後執行，且修改必須回到 PR。
- 任何新增的外部 API 整合，都必須先把：
  - 用途
  - 所需 secret
  - 失敗模式
  - 回寫範圍
  寫進正式文件。

### Evidence Hygiene

- `Documentation/generated/` 只放可重建產物，不放手工整理的敏感內容。
- Unity evidence / test results 若需進 repo，必須能說清楚來源是：
  - CI artifact
  - 或明確的本地輸入

## Mechanical Guard Direction

這份文件不是只給人看，也要給 guard 用。

目前至少要逐步做到：

- workflow 必須顯式宣告 `permissions`
- security 文件本身必須有 threat model / secret-handling / workflow rules
- generated docs 不得把 secret 當內容來源
- 外部服務入口要能被 repo 內腳本或規則指出來

## 目前已知仍待補強

- 更完整的 workflow / external service 機械檢查
- 對 generated docs / artifact 的敏感資訊掃描
- 對 agent write-back 流程的更細權限模型
