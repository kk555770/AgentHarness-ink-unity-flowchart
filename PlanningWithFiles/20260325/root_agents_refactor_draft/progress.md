# 進度紀錄：root AGENTS 精簡重寫

## 2026-03-25

- 建立 `PlanningWithFiles/20260325/root_agents_refactor_draft/`
- 已確認：
  - 這次只重寫 root `AGENTS.md` 與新增詳細規則文件
  - 不碰產品程式碼
  - 方向是「AGENTS 當入口，細節移到正式文件」
- 已完成：
  - 重寫 root `AGENTS.md`
  - 新增 `Documentation/AgentWorkflowRules.md`
  - 確認所有新舊連結目標都存在
  - `git diff --check` 無格式錯誤
  - 再精簡一輪文字，收斂成：
    - `AGENTS.md`：30 行
    - `Documentation/AgentWorkflowRules.md`：75 行
