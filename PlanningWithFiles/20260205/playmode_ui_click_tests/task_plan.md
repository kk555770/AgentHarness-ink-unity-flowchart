# 任務計畫：PlayMode UI 點擊自動測試 + 一鍵跑完（只跑我們的）

日期：2026/02/05  
任務資料夾：`PlanningWithFiles/20260205/playmode_ui_click_tests/`  
相關任務（前一個）：`PlanningWithFiles/20260204/playmode_acceptance_checklist/`

## 目標（8歲也看得懂）
- 我想用「自動測試」去幫我按 UI 按鈕，確定按鈕真的有接好
- 我想要「一鍵跑完」我們自己的 EditMode + PlayMode 測試，不要跑到別人的 package 測試卡住

## 範圍與限制
- 只新增測試與測試用工具（Editor Menu），不改遊戲功能
- 測試場景固定：`Assets/Scene/Test.unity`
- UI 測試只驗證「按鈕按下去會改 UI 狀態」，不驗證美術外觀

## 交付物
- 新增 PlayMode UI 點擊測試：`Assets/Tests/PlayMode/OpsidanosInkPlayModeUiClickTests.cs`
- 新增一鍵跑完（Editor Menu）：`Assets/Editor/OpsidanosInkTestRunnerMenu.cs`
- `findings.md`：測試結果（通過/失敗）
- `progress.md`：工作紀錄與錯誤紀錄

## Phase（調查/實作一步、記錄一步）
| Phase | 內容 | 狀態 |
| --- | --- | --- |
| 1 | 盤點要點擊的 UI 元件（name/class） | 完成 |
| 2 | 寫 PlayMode UI 點擊測試（Backlog/Auto/Skip/Hide） | 完成 |
| 3 | 寫一鍵跑完（只跑我們的測試） | 完成 |
| 4 | 用 Unity MCP 跑測試並記錄結果 | 完成 |
| 5 | 整理版控（忽略 `_Recovery`、移除 `EditorUserSettings`）並提交 | 完成 |
| 6 | 整理版控（暫時忽略 `EditorSettings.asset`）並提交 | 完成 |
| 7 | 新增終端機一鍵跑完測試腳本（batchmode） | 完成 |
| 8 | 新增 GitHub Actions CI（只跑我們的測試） | 完成 |

## 驗收清單
- [x] 點 `BacklogButton` → `BacklogPanel` 會打開；點 `BacklogCloseButton` 會關掉
- [x] 點 `AutoButton` → 文字變 `自動：開/關` 且有/沒有 `vn-toggle-on`
- [x] 點 `SkipButton` → 文字變 `快轉：開/關` 且有/沒有 `vn-toggle-on`
- [x] 點 `HideButton` → `VNRoot` 有 `vn-ui-hidden` 且 `ShowUIButton` 會出現；點 `ShowUIButton` 會回復
- [x] 一鍵跑完：只跑 `OpsidanosInk.EditModeTests` 與 `OpsidanosInk.PlayModeTests`

## 錯誤紀錄
| Error | Attempt | Resolution |
| --- | --- | --- |
| Unity MCP 顯示沒有連線 Session | 1 | 需要在 Unity Editor 按一次「Start Session」，讓 MCP 連上 |
| Unity Batchmode 顯示專案已被另一個 Unity instance 開啟 | 1 | 目前 Unity 正在開著此專案；要嘛關掉 Unity 再跑 batchmode，要嘛用 Editor 內的 Test Runner 跑 |
| PlayMode 測試卡住（Unity 無法進入 PlayMode） | 1 | 修正腳本編譯錯誤 → reimport → 用一次 EditMode Run 解除 MCP 卡住狀態 → 重新跑 PlayMode |
| 沒有先提案就執行「會啟動 Unity」的指令（例如 `Unity -help`） | 1 | 後續規則：任何啟動 Unity 的指令都先提案，等使用者回「同意」才執行 |

## 下一步候選（需要提案才會做）
- （目前沒有新的下一步）
