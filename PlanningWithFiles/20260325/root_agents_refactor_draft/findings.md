# 發現紀錄：root AGENTS 精簡重寫

## 現況

- 目前 root `AGENTS.md` 雖然只有 41 行，但已混合：
  - 修改流程
  - spec 入口
  - 語言規範
  - 開發原則
  - 程式碼風格
- 對 agent 來說，它已經不只是入口，開始承擔詳細規則文件的角色。

## 收斂方向

- root `AGENTS.md` 應保留：
  - 這份文件的角色
  - 第一次進 repo 先讀哪裡
  - 編輯前一定要知道的硬規則
  - 標準驗證入口
- 詳細規則外移到：
  - `Documentation/AgentWorkflowRules.md`

## 必須保留的硬規則

- 未經同意前，只能做讀取 / 搜尋 / 測試 / 更新 `PlanningWithFiles`
- 所有回應、文件、註解皆使用繁體中文
- Unity 版本目前是 `6000.3.9f1`
- 禁止防禦性編碼
- 動到 UI Toolkit / Graph Toolkit / current projection 時必讀對應 spec
- 動到 `.cs` 前必讀詳細規則文件
