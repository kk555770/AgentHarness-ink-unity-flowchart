# Findings：全專案深度調查（重啟版）

## F-001 初始化
- 已重啟調查任務。
- 記錄模式改為「一步一記」。

## F-002 Git 現況
- 目前工作樹為乾淨狀態。
- 目前調查基線提交為 `dc8a443`（即你要求要稽核的上一次提交）。

## F-003 調查範圍確認
- 專案規模重心在 `Packages` 與 `Assets`。
- 這輪調查需優先把 `Packages/com.opsidanos.ink`（自有 Runtime）與 `Assets/Editor/FlowChart/GraphToolkit`（自有 Editor）兩條主線拆開。

## F-004 歷史任務覆蓋範圍
- 歷史任務橫跨：對話系統、Save/Load/Rollback、UI 點擊、GraphToolkit 匯入匯出、專案狀態盤點。
- 後續稽核 `dc8a443` 時，至少要對照：
  - `20260206/phase5_phase6_unified`
  - `20260208/graphtoolkit_repeated_issues_root_cause`
  - `20260211/project_status_assessment`
