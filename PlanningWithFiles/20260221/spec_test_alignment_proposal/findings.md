# Findings：規範（契約）與測試/流程對齊（調整提案）

## F-001 目前對齊程度（2026-02-21）
- `DeveloperModeOutputContract.md` 已把「Graph v2 子集合 + 圖是權威 + 輸出可重播/快照式」寫成契約，且已 commit（`16ab212`）。
- PlayMode 測試已鎖住「可重播 char steps」與「ContinueButton 兩段式點擊節奏」兩條主規範。
- GraphToolkit 匯出器已支援 Graph v2 `choice/condition`，並在匯出前做結構驗證（線性節點 0/1 輸出、分岔每埠必接、condition else 最後）。

## F-002 已鎖定的規範項目（2026-02-21）
- 契約 §6.2.3：`action/comment` 內容不得藏流程結構，已由匯出器檢查並有失敗測試覆蓋。
- 契約 §7：匯出的 `.ink` 必須可編譯，已由 Editor 測試覆蓋。
- Graph v2 匯入（choice/condition）成功與失敗案例已補測。

## F-003 目前剩餘缺口（2026-02-21）
- Round-trip（匯入→匯出→語意比對 + 匯出 ink 可編譯）已完成正式驗證流程，但目前驗證結果是失敗（2/2 failed），尚未修正。
- Round-trip 測試檔尚未 commit（目前為未追蹤檔案）。

## F-004 已落地的調整（2026-02-21）
- 匯出器已新增 action 內容檢查：只要內容出現未跳脫的 `->` / `<-`、或行首 `=`/`*`/`+`/`-`、或單獨一行 `{`/`}`、或行首 `INCLUDE`，匯出會直接失敗並提示正確做法（跳脫或改用節點/接線）。
- 匯出器已修正 comment 多行輸出：comment 內容每行都會被輸出成 `// ...`，避免第二行開始變成真正 Ink 內容而破壞流程。
- 已補齊自動測試：
  - 匯出 `.ink` 必須可被 Ink.Compiler 編譯（可檢驗性鎖定）
  - action 內容藏 divert（`->`）必須匯出失敗
  - Graph v2 匯入：choice/condition 成功與 condition else 規則失敗案例

## F-005 歷史問題已排除：ChoiceMode 寫入失敗（2026-02-21）
- 問題：`ImportGraphV2Choice_可還原節點連線與選項文字` 曾失敗，訊息為 `節點選項 'ChoiceMode' 寫入失敗`。
- 處理：匯入器已改為 `EnumValueReference` 寫入方式並重跑測試通過。
- 結論：此問題不再是目前阻塞點。

## F-006 注意：本輪工作樹包含 Unity/套件更新（2026-02-21）
- `ProjectSettings/ProjectVersion.txt` 已變更為 `6000.3.9f1`。
- `Packages/manifest.json` / `packages-lock.json` 有依賴版本更新（Addressables / AI Navigation / InputSystem）。
- 這類變更屬於高風險（套件管理/引擎版本），後續應分開 commit，並在 commit 前向使用者明確確認「要保留」或「要還原」。

## F-007 下一個缺口：Round-trip（匯入→匯出→再匯入）閉環測試尚未落地（2026-02-21）
- 目前狀態更新：round-trip 測試檔已新增，待正式驗證並納入版控。
- 價值不變：把「圖是權威、輸出可逆、可檢驗」直接鎖死，避免匯入與匯出各自通過、但合在一起無法閉環。

## F-008 MCP 測試橋接目前不穩定（2026-02-21）
- 本條僅作歷史紀錄，不作為當前自動動作依據。
- 目前策略：任何 MCP 啟停/重啟操作都先提案並取得明確同意，再執行。

## F-009 目前 Git 工作樹可驗證狀態（2026-02-21）
- `git diff`：沒有任何已追蹤檔案的修改。
- `git status`：只有新增未追蹤檔案（round-trip 測試與其 `.meta`），尚未 commit。

## F-010 Round-trip 驗證現況（2026-02-21）
- 依使用者同意後執行 `unityMCP/run_tests`（只跑 `OpsidanosInk.Tests.EditMode.InkFlowChartRoundTripTests`、不啟停 MCP）。
- 兩次呼叫結果一致：`error=tests_running`，`retry_after_ms=5000`，目前無法啟動新的測試 job。
- `unityMCP/read_console` 可讀取，內容顯示 MCP bridge 曾斷線再重連；但這不等於測試佔用已解除。
- 結論：當前阻塞點是「測試系統已有執行中工作」，不是 round-trip 測試本體失敗。
- 追加查核：`unityMCP/get_test_job(job_id=8deaccfc71fb4395a3c627faee9c5a78)` 回傳 `status=running`、`progress.completed=0`、`progress.total=0`，確認舊 job 仍佔用中。

## F-011 使用者重啟後的重新確認（不依賴舊 job id）
- 重新確認方式：不使用既有 job id，直接重送 `run_tests`，並讀取最新 Console/session 記錄。
- 結果：`run_tests` 仍回 `tests_running`，代表重啟後仍存在測試佔用。
- 能力邊界：目前 Unity MCP 介面沒有「列出目前 active test job」的獨立 API；僅可用 `get_test_job(job_id)` 查特定 id。

## F-012 再次重啟後的最新驗證（已取得新 job，非舊 id）
- `run_tests` 已成功啟動新 job：`48d85a0fac7a4f4cb9e6f3f70ef13f6f`，表示「測試佔用」已不是現況主因。
- `get_test_job(wait_timeout=60)` 結果：`status=failed`、`completed=2`、`total=2`。
- 失敗項目：
  - `RoundTrip_GraphV2Condition_匯入再匯出_仍保持條件與Else規則`
    - 訊息：`Ink 編譯結果不應為 null。Expected: not null But was: null`
  - `RoundTrip_GraphV2Choice_匯入再匯出_仍保持結構與選項資料`
    - Console 有 unhandled compile error，暫存檔：`TmpGraphToolkitRoundTrip_RoundTripV2Choice_*.ink`
    - 主要堆疊：`Packages/Ink/InkLibs/InkRuntime/NativeFunctionCall.cs:449`
- 結論：目前主問題是 Round-trip 匯出內容/測試編譯流程造成 Ink 編譯失敗，不是 MCP 佔用。

## F-013 失敗邊界已縮小（Choice 單測可過、Condition 單測穩定失敗）
- 單測拆分結果：
  - `RoundTrip_GraphV2Choice...`：`passed (1/1)`。
  - `RoundTrip_GraphV2Condition...`：`failed (0/1)`，訊息仍為 `Ink 編譯結果不應為 null`。
- 推論（待實作驗證）：
  - 問題更可能落在「condition fixture 的 Ink 編譯前提」而非 choice/condition 結構 round-trip 比對本身。
  - `AssertInkCompiles` 目前先 `Assert.IsNotNull(story)`，若編譯錯誤存在，會先被 null 卡住，錯誤清單不易讀；測試診斷可讀性不足。

## F-014 套用方案 A 後的新觀察（並行編譯競態）
- 已完成：
  - condition fixture 補變數宣告（`VAR favor = 8`）。
  - `AssertInkCompiles` 診斷訊息順序修正。
- 但 Condition 單測仍失敗，且失敗型態改為：
  - `TearDown : Unhandled log message`
  - 來源是 Unity InkCompiler 背景編譯暫存 `.ink` 時例外（`Index was outside the bounds of the array`）。
- 推論（高機率）：
  - 測試內手動 `Ink.Compiler.Compile()` 與 Unity 背景 Ink 編譯同時發生，觸發 Ink 編譯器非執行緒安全區塊（先前也出現過 `NativeFunctionCall` 相關 unhandled error）。
  - 因此下一步應優先解「測試編譯同步」，而不是再調整 fixture 文本。

## F-015 最終排除方式與驗證結果（2026-02-22）
- 採用修正：
  - Round-trip 測試改為 `UnityTest`，手動編譯前先 `yield` 等待背景 Ink 編譯空閒。
  - condition fixture 的條件式改為常數比較 `8 > 7`，避免測試依賴外部變數宣告狀態。
- 驗證結果：
  - `RoundTrip_GraphV2Condition...` 單測通過（1/1）。
  - `InkFlowChartRoundTripTests` 類別全通過（2/2）。
- 結論：
  - 目前 Round-trip 測試已回到可重現且穩定通過狀態，下一步是整理 commit 提案。

## F-016 新增問題（Demo 輸出違反 steps 契約）與已修正結果
- 問題來源：
  - Demo `story.ink` 第 5~7 句的 `char.transition.steps` 未完整覆蓋 `appear/move/disappear`。
  - 第 5 句 `raiseActors=["alice"]` 先執行於無 `appear` 的步驟，在空畫面/倒退時會命中不存在角色錯誤。
- 修正策略：
  - 直接修正輸出源（Demo 文本與自動生成的 `story.json`），不依賴 Runtime 補救。
  - 同步更新 EditMode 測試斷言，鎖住新契約輸出格式。
- 驗證結果：
  - EditMode 與關鍵 PlayMode（含 rollback 快速連按/高壓連按）全數通過。
  - 最新 Console error 為 0 筆，原本 `缺少必要動作：Appear` 與 `raiseActors` 紅字未再出現。

## F-017 提交後回歸確認（倒帶與點擊節奏）
- 已提交：`35ca22e`（Demo steps 契約修正 + 測試同步）。
- 提交後再次驗證重點：
  - ContinueButton Busy 流程（Normal/Restore）皆通過，表示「第一次只補完動畫、冷卻後第二次才前進」節奏仍正確。
  - Rollback 快速連按與高壓連按皆通過，表示倒帶隊列與收斂行為仍穩定。
  - `CharLayer_Normal與Restore_固定重排規則相反_且ForceComplete不會破壞` 通過，表示 Normal/Restore 的重排差異邏輯仍符合既定規範。
- 結論：
  - 本次修正只修契約違規輸出，不影響你關心的「倒帶邏輯與點擊節奏差異」。

## F-018 體感驗證缺口已補齊（Rollback UI 路徑）
- 先前缺口：
  - 既有測試偏重「邏輯是否正確」，對「手動點擊時是否有明顯相反重排體感」驗證不足。
- 已補強：
  - 新增 PlayMode 測試 `Rollback後_相反重排可感知`。
  - 測試直接透過 `Continue` 推進到目標句，再透過 `RollbackButton` 觸發 Restore，量測 `CharacterActorLayer` 子層級順序。
- 驗證結果：
  - Normal 路徑：最上層角色為 `ss`。
  - Rollback（Restore）路徑：最上層角色切為 `bs`。
  - 整包 `OpsidanosInkPlayModeUiClickTests` = `10/10 passed`，Console error = 0。
- 結論：
  - 目前測試已同時覆蓋「邏輯正確」與「UI 點擊體感可感知」兩層，符合你要求的玩家模式驗證方向。

## F-019 新增根因：Rollback 點擊節奏尚未對齊 Continue（2026-02-22）
- 現況程式確認：
  - `VNPlayerPresenter.cs` 的 `OnClickRollback()` 仍採「`pendingRollbackRequests` 排隊 + `RollbackCoroutine` 逐步消耗」。
  - 該路徑沒有沿用 `OnClickContinue()` 的 `IsClickInCooldown()` 與「Busy 時先 `TryForceCompleteIfBusy(isClick:true)` 再退出」。
- 現況測試確認：
  - `OpsidanosInkPlayModeUiClickTests.cs` 仍有 `RollbackButton_快速連按_會排隊倒帶到最前句`，語義是「連按可連續倒帶」。
- 判定：
  - 這與目前契約方向（點擊節奏一致、避免快速連按直接跨句）不一致，會造成你描述的體感差異：
    - 前進：`下一句 → 動畫快速定位 → 冷卻 → 下一句`
    - 倒退：`上一句 → 上一句 → 上一句`
- 修正方向（已同意）：
  - 移除 Rollback 排隊協程機制，改成與 Continue 同節奏：
    1. 冷卻中點擊無效
    2. Busy 時第一次點擊只 ForceComplete 並進入冷卻
    3. 冷卻後再次點擊才執行一次 `TryRollbackOnce()`

## F-020 已套用修正（待測試驗證）
- Runtime：
  - `VNPlayerPresenter` 已移除 `pendingRollbackRequests` 相關排隊邏輯。
  - `OnClickRollback` 現在直接沿用與 Continue 相同的點擊節奏規則（冷卻 + Busy 先補完）。
- PlayMode：
  - 舊測試（排隊倒帶語義）已替換為新測試（同節奏語義）。
  - 目前尚待 MCP 實跑，確認行為與 Console 都通過。

## F-021 MCP 驗證完成（2026-02-22）
- 單測結果：
  - `RollbackButton_Busy時第一次只補完不倒帶_冷卻後第二次才倒帶`
    - job=`a0abe0e5509c40c9a5f92406d803045f`，`passed (1/1)`
  - `RollbackButton_快速連按_不會排隊連續倒帶`
    - job=`d8e9f10907eb4458a4a881a013f1a0b7`，`passed (1/1)`
- 整包結果：
  - `OpsidanosInk.Tests.OpsidanosInkPlayModeUiClickTests`
    - job=`28745365763c4e4692f784c6b2b7249f`，`passed (10/10)`
- Console：
  - `read_console(types=[\"error\"])` 回傳 0 筆。
- 結論：
  - Runtime 與測試契約已一致，不再鎖住「Rollback 排隊連續倒帶」舊語義。

## F-022 測試維護性缺口（caseId 注入需求）
- 使用者需求：
  - 不希望每次規格微調都刪掉重寫整段測試。
  - 希望改成「固定測試流程 + 注入事件代號(caseId)」模式，條件變動時只改對應 case。
- 可行性判定：
  - Unity PlayMode + NUnit 可在同一測試檔內建立 case 定義表（`caseId -> 參數`），再由共用 coroutine 執行。
  - MCP 不會阻擋這個設計；MCP 只負責「執行哪些測試」，case 注入屬於測試程式內部能力。

## F-023 caseId 注入架構已落地（待驗證）
- `OpsidanosInkPlayModeUiClickTests.cs` 已新增：
  - `RollbackRhythmCase` + `RollbackRhythmCases`（`RBK_001`、`RBK_002`）
  - `RunRollbackRhythmCaseById(caseId)` 共用流程
  - `ClickButtonMultipleTimes` 共用點擊 helper
- 影響：
  - 後續若規格改動，只需調整 `RollbackRhythmCases` 對應 case，不需重寫整段流程。

## F-024 caseId 注入驗證完成（2026-02-22）
- 單測：
  - `RollbackButton_Busy時第一次只補完不倒帶_冷卻後第二次才倒帶`（`RBK_001`）  
    job=`9f0dbf2a174b44dda85d37e34f5588a6`，`passed (1/1)`
  - `RollbackButton_快速連按_不會排隊連續倒帶`（`RBK_002`）  
    job=`12241c6765714bfeaf2b038096968ca0`，`passed (1/1)`
- 整包：
  - `OpsidanosInk.Tests.OpsidanosInkPlayModeUiClickTests`  
    job=`3a90e6d2c9774248bc9312eb5ac6baab`，`passed (10/10)`
- Console：
  - `read_console(types=[\"error\"])` = 0 筆。
- 結論：
  - 「固定流程 + caseId 注入」已可用，且未破壞既有 PlayMode 行為。

## F-025 契約合規盤點（DeveloperModeOutputContract）
- 比對範圍：
  - Runtime：`VNPlayerPresenter.OnClickRollback()` 改為同節奏（Busy 首點只補完、冷卻後才倒帶一步）。
  - 測試：`OpsidanosInkPlayModeUiClickTests` 導入 `RBK_001`/`RBK_002` caseId 注入。
- 合規結論：
  - **沒有發現硬性違反** `DeveloperModeOutputContract` 既有條文（特別是「Restore/Rollback 可重播且不應刷紅字」）。
  - MCP 驗證結果亦符合契約目標（兩條 Rollback 測試 + 整包 10/10 通過，Console error 0）。
- 文件缺口（需補回）：
  1. §7 測試索引目前只列 `OpsidanosInkPlayModeTests.cs`，未涵蓋新的 `OpsidanosInkPlayModeUiClickTests.cs`。
  2. 契約未明寫「Rollback 點擊節奏與 Continue 一致」這條播放器不變式，容易再次被誤改成排隊倒帶語義。
  3. 契約未明寫 `caseId` 測試治理方式，後續維護可能又回到重寫整段測試。

## F-026 契約補回已完成（2026-02-22）
- 已更新 `DeveloperModeOutputContract.md`：
  - 最後更新日期改為 `2026/02/22`。
  - §7 補齊測試索引：`OpsidanosInkPlayModeTests.cs` + `OpsidanosInkPlayModeUiClickTests.cs`。
  - 新增 §7.1：玩家輸入節奏不變式（Continue/Choice/Rollback 同節奏）。
  - 新增 §7.2：Rollback 測試治理（caseId 注入，`RBK_001` / `RBK_002`）。
  - 新增 §7.3：節奏相關改動的最小驗收集合（2 條 Rollback 單測 + UIClickTests 整包 + Console error 0）。
- 結論：
  - 今日調整已被契約文件完整覆蓋，避免後續再出現「程式已改、契約沒跟上」的落差。
