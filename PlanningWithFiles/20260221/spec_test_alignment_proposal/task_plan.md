# 任務計畫：規範（契約）與測試/流程對齊（調整提案）

## 目標
- 讓「測試檔」與「實際流程/程式行為」完全符合 `/Users/arcumit/Documents/GitHub/ink-unity-integration/Documentation/DeveloperModeOutputContract.md` 的規範。
- 把目前規範中「已寫出但尚未被程式/測試鎖住」的部分，落實到 GraphToolkit 匯出/匯入與自動測試。

## 範圍
- GraphToolkit（Editor）：
  - 匯出：`Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`
  - 匯入：`Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`
  - 節點：`Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodes.cs`
  - DTO：`Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExportModels.cs`
  - 測試：`Assets/Editor/Tests/InkFlowChartExportTests.cs`、`Assets/Editor/Tests/InkFlowChartImportTests.cs`
- 玩家模式（Runtime/PlayMode）：
  - 測試：`Assets/Tests/PlayMode/OpsidanosInkPlayModeTests.cs`、`Assets/Tests/PlayMode/OpsidanosInkPlayModeUiClickTests.cs`

## 原則（遵循 AGENTS.md）
- 修改任何一般檔案前先提案並取得同意（本任務先做提案）。
- 禁止防禦性編碼：遇到不符合規範的輸出/輸入，應該「直接失敗 + 明確錯誤訊息」，不可在 PlayMode 偷偷補洞。
- 使用繁體中文；程式碼變更需用「變更開始/結束」框起來並寫日期與原因。

## 階段
| 階段 | 狀態 | 說明 |
|---|---|---|
| 1 | complete | 盤點：哪些規範已被程式/測試鎖住、哪些還沒 |
| 2 | complete | 提出調整提案（修改檔案清單 + 方法 + 預期效果 + 詢問同意） |
| 3 | complete | 實作：匯出時檢查 action/comment 禁止藏流程結構（§6.2.3） |
| 4 | complete | 補測試：Graph v2 匯入（choice/condition）成功/失敗案例 |
| 5 | complete | 補測試：匯出 `.ink` 必須可被 Ink 編譯（可檢驗性） |
| 6 | complete | 跑 MCP/Unity 測試、修正阻塞點並分批 commit（Editor/PlayMode 分開） |
| 7 | complete | 補測試：Graph v2 匯入→匯出 Round-trip（閉環可檢驗） |
| 8 | complete | 驗證：依使用者同意後執行 Round-trip 測試（不啟停 MCP） |
| 9 | complete | commit：Round-trip 測試檔（僅新增測試檔與 meta） |
| 10 | complete | 對齊文件：清理前後矛盾描述並更新下一步提案 |
| 11 | complete | 提案與修正：Round-trip 失敗根因定位（條件 fixture + 編譯同步） |
| 12 | complete | 修正 Demo `steps` 契約違規（倒退快速連點紅字） |
| 13 | complete | 驗證：EditMode + PlayMode（含 rollback 快速連按） |
| 14 | complete | 提交修正並回歸確認倒帶/點擊節奏 |
| 15 | complete | 補測試：Rollback 體感路徑（相反重排可感知） |
| 16 | complete | 驗證：整包 `OpsidanosInkPlayModeUiClickTests` 與 Console 無錯 |
| 17 | complete | 盤點：Rollback 仍為排隊倒帶，與 Continue 節奏契約不一致 |
| 18 | complete | 文件對齊：記錄根因、修正目標與測試策略（已獲同意） |
| 19 | complete | 實作：Rollback 改為「Busy→ForceComplete→冷卻→下一次才倒帶」 |
| 20 | complete | 補測試與 MCP 驗證：移除排隊語義，改鎖節奏語義 |
| 21 | complete | 測試架構升級：引入 caseId 注入，避免每次重寫整段流程 |
| 22 | complete | 對齊契約：補回今日變更對系統流程的條文與驗收規範 |

## 目前阻塞（2026-02-21）
- ✅ 已排除：`InkFlowChartImportTests` 的 `ChoiceMode`（enum option）寫入失敗。
- ✅ 已完成：整理變更並分批 commit（GraphToolkit / PlayMode / 版本升級分開）。
- ✅ 已完成：新增 `InkFlowChartRoundTripTests.cs`（尚未 commit）。
- ✅ 已排除：`tests_running` 佔用阻塞（重啟後可取得新 job 並完成執行）。
- ✅ 已排除：Round-trip 失敗（目前 `InkFlowChartRoundTripTests` = 2/2 passed）。
- ✅ 已排除：Demo `char.transition.steps` 在快速連點/倒退時的契約紅字（`缺少必要動作：Appear`、`raiseActors` 時機錯誤）。
- ✅ 已完成：提交本輪修正（commit `35ca22e`）。
- ✅ 已完成：提交後回歸確認「倒帶邏輯與點擊節奏差異」未被破壞。
- ✅ 已確認：現況程式仍使用 Rollback 排隊協程（`pendingRollbackRequests`），尚未對齊 Continue 兩段式節奏。
- ✅ 已完成：Rollback 改為同節奏（Busy 首點只補完、冷卻後再倒帶一步）。
- ✅ 已完成：新測試與整包 `OpsidanosInkPlayModeUiClickTests` 驗證通過（10/10，Console error 0）。
- ✅ 已完成：Rollback 測試改為 `caseId` 注入架構（`RBK_001` / `RBK_002`）。
- ✅ 已完成：`DeveloperModeOutputContract.md` 補回「輸入節奏不變式 / caseId 測試治理 / 最小驗收集合」。
- 目前待決策：是否直接 commit 本輪 Runtime + 測試 + 契約文件更新。
