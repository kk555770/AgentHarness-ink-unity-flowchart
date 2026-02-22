# 任務計畫：專案現況盤點與下一步評估

## 目標
整理目前專案做到哪裡、哪些改動正在進行、哪些地方有風險，並提出可直接執行的下一步。

## 目前階段
階段 4（交付）

## 階段規劃

### 階段 1：現況蒐集
- [x] 讀取專案根目錄與 git 狀態
- [x] 讀取近期 PlanningWithFiles 任務脈絡（至少到 2026-02-08）
- [x] 讀取關鍵規格與核心程式位置（`GraphToolkitSpec.md`、Runtime/Editor 關鍵檔）
- **狀態：** complete

### 階段 2：資料鏈整理
- [x] 串起「程式 -> 場景/Prefab -> 測試 -> 風險」
- [x] 標記已完成、進行中、未完成項目
- **狀態：** complete

### 階段 3：下一步評估
- [x] 給出下一步優先順序
- [x] 說明每一步的前置條件與可能副作用
- [x] 列出需要使用者決策的點
- **狀態：** complete

### 階段 4：交付
- [x] 輸出精簡但完整的現況摘要
- [x] 輸出可執行的下一步清單
- **狀態：** complete

## 關鍵問題（本輪答案）
1. 最近一次實作主軸是否已從「匯出」延伸到「匯入」流程？
- 是。工作樹已有 `InkFlowChartImporter.cs` + `InkFlowChartImportTests.cs`，但仍是未提交狀態。
2. 目前未提交的檔案是否可以形成可測試的最小閉環？
- 可以形成「匯出 + 匯入 + 選單入口 + 測試」閉環，但目前缺最新一輪測試結果證明。
3. 下一步應先補「穩定性」還是先補「功能覆蓋率」？
- 先補穩定性。因為 2026-02-08 已定位過時序類 warning，若不先鎖住，功能再加只會擴大回歸成本。

## 已做決策
| 決策 | 理由 |
|------|------|
| 先以 PlanningWithFiles 歷史任務還原脈絡，再看工作樹差異 | 避免只看當前檔案造成誤判 |
| 先分兩條主線評估：Runtime（玩家流程）與 Editor（GraphToolkit） | 目前風險主要在 Editor，Runtime 已有較完整既有驗收 |
| 下一步先做「測試與警告收斂」，再談功能擴張 | 現階段最大風險是反覆回來的時序/資產路徑問題 |

## 錯誤紀錄
| 錯誤 | 次數 | 解法 |
|------|------|------|
| 無 | 1 | 持續監控 |

## 補充驗證（2026-02-11 深夜）
- 已用 Unity MCP 驗證：
  - `OpsidanosInk.EditModeTests`：11/11 passed
  - 測後 Console：warning=0、error=0
  - Active Scene：`Assets/Scene/Test.unity`
  - Build Settings 只啟用 `Assets/Scene/Test.unity`，不含 `Assets/OffMeshLinkScene.unity`

## S1 收斂狀態（2026-02-11 深夜追加）
- [x] 補齊 `.gitignore` 的匯入暫存前綴規則一致性
- [x] 整理 `InkFlowChartGraph.cs` 的重複結束標記
- [x] 排除 `OffMeshLinkScene.unity`（還原到 `HEAD`）
- [x] S1 後重跑 `OpsidanosInk.EditModeTests` 並確認 warning/error 為 0

## S2 收斂狀態（2026-02-11 深夜追加）
- [x] 再次收斂 `InkFlowChartGraph.cs` 註解邊界（移除多餘獨立註解區塊）
- [x] S2 後重跑 `OpsidanosInk.EditModeTests` 並確認 warning/error 仍為 0

## S3 封口狀態（2026-02-11 深夜追加）
- [x] 以主線檔案建立單一 commit：`dc8a443`
- [x] 排除 `OffMeshLinkScene.unity` 不入本批

## S4 驗收狀態（2026-02-11 深夜追加）
- [x] EditMode 回歸：`OpsidanosInk.EditModeTests` 11/11 passed
- [ ] PlayMode 回歸：`OpsidanosInk.PlayModeTests` 有 1 筆失敗（`Restore_缺少Appear時_不應產生CharTransitionStepsError`）
- [ ] 菜單三輪真流程：`建立新圖 -> 匯出 -> 匯入` 未達標（匯出缺開始節點、匯入選單在當前 selection 下停用）
- [ ] S4 產物清理：`Assets/FlowCharts/*.inkfc`（本輪驗收產生 3 組）待決策是否刪除

## S5 狀態（2026-02-11 深夜追加）
- [x] S5-1：已清理 S4 驗收產物（`Assets/FlowCharts/*.inkfc`）
- [x] S5-2：已修復 PlayMode 失敗測試（`Restore_缺少Appear...`）
- [x] S5-2 後驗證：`OpsidanosInk.PlayModeTests` 13/13、`OpsidanosInk.EditModeTests` 11/11、warning/error=0
