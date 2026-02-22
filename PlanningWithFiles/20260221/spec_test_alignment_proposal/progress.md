# 進度日誌：規範（契約）與測試/流程對齊（調整提案）

## 2026-02-21

### P-001 建立本輪 planning 檔
- 建立資料夾：`PlanningWithFiles/20260221/spec_test_alignment_proposal`
- 建立檔案：`task_plan.md`、`findings.md`、`progress.md`

### P-002 盤點：規範對齊缺口
- 已確認目前缺口集中在三件事：
  - 匯出未檢查 `action/comment` 內容禁止藏流程結構（§6.2.3）
  - Graph v2 匯入缺少測試覆蓋
  - 匯出 `.ink` 可編譯性缺少測試鎖定

### P-003 準備調整提案
- 提案方向：
  - 匯出階段直接擋下「內容藏流程」並給明確錯誤（符合「圖是權威」與「禁止補洞」）
  - 補齊 Graph v2 匯入（choice/condition）測試，鎖 else 規則與輸出埠接線
  - 在匯出測試加入 Ink 編譯檢驗，鎖住「匯出必可編譯」

### P-004 實作：匯出器新增 action 內容驗證、修正 comment 多行輸出
- 修改：`InkFlowChartExporter.cs`
- 內容：
  - 匯出前新增 `action` 內容檢查（禁止 `->`/`<-` 與行首結構語法）
  - comment 節點改為「每行都輸出 //」，避免多行註解破壞 Ink

### P-005 實作：匯出測試新增可編譯性檢驗 + 內容藏流程失敗案例
- 修改：`InkFlowChartExportTests.cs`
- 內容：
  - `ExportFixtureGraph_可產出Ink與FlowchartJson` 追加 Ink.Compiler 編譯檢驗
  - 新增 `ExportActionContent藏Divert_回傳失敗且錯誤訊息明確`

### P-006 實作：匯入測試補齊 Graph v2（choice/condition）
- 修改：`InkFlowChartImportTests.cs`
- 內容：
  - 新增 v2 choice/condition 成功匯入測試
  - 新增 condition 缺 else / else 不在最後的失敗測試

### P-007 MCP 測試結果與目前阻塞點
- EditMode 匯出測試：成功（`InkFlowChartExportTests` 5/5）。
- EditMode 匯入測試：失敗（`InkFlowChartImportTests` 8/8）。
  - 失敗點：`ImportGraphV2Choice_可還原節點連線與選項文字`
  - 錯誤訊息：`匯入失敗：節點選項 'ChoiceMode' 寫入失敗。`
- 下一步：修正 `InkFlowChartImporter` 對 enum option（ChoiceMode）的寫入方式，讓 Graph v2 choice 測試通過後再重跑整體 EditMode 測試。

### P-008 修正匯入器 enum option 寫入（ChoiceMode）並重跑測試
- 修改：`InkFlowChartImporter.cs`
  - 讓 enum option 改用 GraphToolkit 內部 `EnumValueReference` 形式寫入（反射建立 `EnumValueReference(Enum)` 後呼叫 `TrySetValue<EnumValueReference>`）。
- MCP 測試：
  - `InkFlowChartImportTests`：8/8 passed
  - `InkFlowChartExportTests`：6/6 passed
  - `PlayMode`（全跑）：220 total / 129 passed / 0 failed（其餘為 explicit/ignored 的套件測試）

### P-009 新增 Round-trip（閉環）回歸測試（尚未驗證/尚未 commit）
- 新增未追蹤檔案：
  - `Assets/Editor/Tests/InkFlowChartRoundTripTests.cs`
  - `Assets/Editor/Tests/InkFlowChartRoundTripTests.cs.meta`
- 測試目的：
  - 同一份 Graph v2 `flowchart.json` 匯入成 `.inkfc` 後，再匯出回 `flowchart.json`，檢查語意一致（不比較 nodeId，只比較節點 type/content 與 outputs/label/condition/else）。
  - 同時檢查匯出的 `.ink` 可被 Ink.Compiler 編譯。

### P-010 MCP 測試橋接卡死（阻塞 Round-trip 驗證）
- `unityMCP/run_tests` 啟動 job 後，狀態長時間維持 `running` 且 progress 0/0。
- `unityMCP/get_test_job` 在較長等待時間會 tool-call timeout；縮短等待可拿到狀態，但狀態不更新。
- 曾嘗試以 pidfile（`Library/MCPForUnity/RunState/mcp_http_8080.pid`）定位 bridge 進程並重啟以解卡，但目前工具呼叫仍不穩。
- 目前策略：先停止所有非必要操作；下一步會先提出「恢復 MCP 測試橋接」的明確操作提案，取得同意後才繼續。

### P-011 文件對齊（依規則先對齊文件再提案）
- 目標：清理 planning 檔案中「歷史未完成敘述」與「現況已完成敘述」同時存在的矛盾。
- 已更新：
  - `task_plan.md`：將 Round-trip 測試建立狀態標記為完成，並把待辦改成「驗證」與「commit 決策」。
  - `findings.md`：把已完成項與剩餘缺口分離；ChoiceMode 問題改為歷史已排除；MCP 條目改為需先提案再操作。
- 對齊後現況：
  - 工作樹僅有 Round-trip 測試檔與 `.meta` 為未追蹤。
  - 下一步可直接進行「驗證策略 + commit 策略」提案。

### P-012 依同意執行 Round-trip 驗證（不啟停 MCP）
- 執行：
  - `unityMCP/run_tests`（EditMode，test_names=`OpsidanosInk.Tests.EditMode.InkFlowChartRoundTripTests`）×2
  - `unityMCP/read_console`（檢查 bridge/runner 現況）
  - `unityMCP/get_test_job`（查核既有 job 狀態）
- 結果：
  - 兩次 `run_tests` 皆回傳 `error=tests_running`（`retry_after_ms=5000`），未取得新 job_id。
  - console 記錄可見 bridge 曾斷線/重連與本地 HTTP server 啟動訊息。
  - 舊 job `8deaccfc71fb4395a3c627faee9c5a78` 仍為 `running`，進度 `0/0`。
- 判定：
  - 目前阻塞為測試佔用狀態，尚未進入 round-trip 測試實際執行階段。
  - 依規則先停止，等待使用者決策下一步（是否授權清理測試佔用，或改先 commit）。

### P-013 使用者重啟後再次重試（不依賴舊 job id）
- 執行：
  - `unityMCP/read_console`（關鍵字：`test`、`job`、`MCP-FOR-UNITY`）
  - `unityMCP/run_tests`（EditMode，test_names=`OpsidanosInk.Tests.EditMode.InkFlowChartRoundTripTests`）
  - `list_mcp_resources` + `read_mcp_resource(mcpforunity://tests)`（確認是否有「目前執行中 job」可查詢入口）
- 結果：
  - `run_tests` 仍回 `error=tests_running`（`retry_after_ms=5000`）。
  - Console 可見 bridge 已重新註冊新的 session id，表示重啟已生效。
  - 目前工具集合只有 `run_tests` / `get_test_job(job_id)`，沒有可直接列出「當前 active job id」的 API。
- 判定：
  - 問題仍是測試系統存在執行中佔用，但目前缺少不帶 job_id 的查詢接口。

### P-014 使用者再次重啟後重試（取得新 job 並完成執行）
- 執行：
  - `unityMCP/run_tests`（EditMode，test_names=`OpsidanosInk.Tests.EditMode.InkFlowChartRoundTripTests`）
  - `unityMCP/get_test_job(job_id=48d85a0fac7a4f4cb9e6f3f70ef13f6f, wait_timeout=60)`
  - `unityMCP/read_console`（error + stacktrace）
- 結果：
  - 已成功取得 **新 job_id**：`48d85a0fac7a4f4cb9e6f3f70ef13f6f`（非舊 id）。
  - job 最終狀態：`failed`，進度 `2/2`（表示測試確實執行完成，不是卡在 `tests_running`）。
  - 失敗主因 1：`RoundTrip_GraphV2Condition_匯入再匯出_仍保持條件與Else規則`
    - 訊息：`Ink 編譯結果不應為 null。Expected: not null But was: null`
  - 失敗主因 2（Console unhandled error）：`RoundTripV2Choice` 暫存 ink 編譯時發生例外
    - 檔案：`Assets/Editor/TmpGraphToolkitRoundTrip_RoundTripV2Choice_b88f092fd30242318cf2b5dc914a5d91.ink`
    - 主要堆疊：`Packages/Ink/InkLibs/InkRuntime/NativeFunctionCall.cs:449`
- 判定：
  - 目前阻塞已從「測試佔用」轉為「Round-trip 輸出內容觸發 Ink 編譯錯誤」。
  - 下一步應聚焦在「匯出內容/生成規則」與「測試 fixture 內容」的規範對齊與修正提案。

### P-015 拆分驗證（分開執行 Choice / Condition，定位失敗邊界）
- 執行：
  - `unityMCP/run_tests`（只跑 `RoundTrip_GraphV2Choice_匯入再匯出_仍保持結構與選項資料`）
  - `unityMCP/get_test_job(wait_timeout=60)`（job=`67478138fb194720af8c2828af179552`）
  - `unityMCP/run_tests`（只跑 `RoundTrip_GraphV2Condition_匯入再匯出_仍保持條件與Else規則`）
  - `unityMCP/get_test_job(wait_timeout=60)`（job=`fce11a91ecc840e88b238e70f48da159`）
- 結果：
  - Choice 單測：`passed (1/1)`。
  - Condition 單測：`failed (0/1)`，訊息仍為 `Ink 編譯結果不應為 null`。
- 判定：
  - 目前失敗可穩定縮小到「condition round-trip + 編譯檢驗」路徑，不是 choice 結構比對邏輯本身。
  - 下一步提案應優先處理 `RoundTrip_GraphV2Condition` fixture 與編譯檢驗判斷順序（先輸出可讀錯誤再 assert）。

### P-016 依方案 A 實作後重測（Condition 仍失敗，失敗型態改變）
- 已實作（`InkFlowChartRoundTripTests.cs`）：
  - condition fixture 補變數宣告節點（`VAR favor = 8`）並更新結構斷言。
  - `AssertInkCompiles` 改成先檢查 `errors` 再檢查 `story`，提升錯誤可讀性。
- 重測：
  - 單跑 `RoundTrip_GraphV2Condition...`（job=`c6394bf05ccf4a779e6f8e84e3392ac2`）仍失敗。
  - 目前失敗訊息是 `TearDown : Unhandled log message`，來自 Unity InkCompiler 背景編譯暫存 `.ink` 時拋例外（`Index was outside the bounds of the array`）。
- 判定：
  - 問題已從「測試 assertion 不可讀」轉成「背景 Ink 編譯與測試內手動編譯可能並行，導致編譯器非預期例外」。
  - 下一步應改為先同步等待 Ink 背景編譯完成，再做手動 `AssertInkCompiles`（可用 `UnityTest + yield` 方式）。

### P-017 第二輪修正：改為 UnityTest 同步等待 + fixture 條件改成常數比較
- 修改：`Assets/Editor/Tests/InkFlowChartRoundTripTests.cs`
- 內容：
  - 兩個 round-trip 測試改為 `[UnityTest] IEnumerator`。
  - 新增 `WaitForInkCompilerIdle(context)`，在手動編譯前先等待背景 Ink 編譯佇列清空。
  - condition fixture 從 `favor > 7` 改為 `8 > 7`，避免測試依賴外部變數宣告前提。
  - condition 結構斷言同步改為 `8 > 7` 並恢復 start 直連 condition。

### P-018 第二輪重測結果（已通過）
- 單測：
  - `RoundTrip_GraphV2Condition...`（job=`4d52626fbe1142878276ff0cd5161dcb`）`passed (1/1)`。
- 類別整體：
  - `OpsidanosInk.Tests.EditMode.InkFlowChartRoundTripTests`（job=`035504da10c347ebb66396ace206fc57`）`passed (2/2)`。
- 判定：
  - Round-trip 失敗已解除，現況可進入 commit 提案階段。

### P-019 修正 Demo `char.transition.steps` 契約違規（依規範流程）
- 背景：
  - 使用者回報在快速手動連點/倒退時出現紅字。
  - Console 錯誤對應為 `OutputId=7/8/9` 的 `steps` 缺少 `Appear` 與 `raiseActors` 時機不合法。
- 修改：
  - `Assets/OpsidanosInk/Demo/story.ink`
    - 第 5 句：改為完整三動作 steps（`appear`/`move`/`disappear`）並把 `raiseActors=["alice"]` 放在含 `appear` 的步驟。
    - 第 6 句：從 `steps=[disappear]->[move]` 改為 `steps=[disappear]->[move,appear]`。
    - 第 7 句：從 `steps=[move,disappear]` 改為 `steps=[appear,move,disappear]`。
  - Unity 重新編譯後同步更新：
    - `Assets/OpsidanosInk/Demo/story.json`

### P-020 同步測試期待（鎖住新契約輸出）
- 修改：
  - `Assets/Editor/Tests/CharTagJsonTests.cs`
- 內容：
  - 第 5/6/7 句斷言改為符合「有 steps 就必須含三動作」。
  - 新增對 `appear` 值與 actions 組合的檢查（例如第 7 句必須是 `appear+move+disappear`）。

### P-021 MCP 驗證結果（修正後）
- EditMode：
  - `CharTagJson_DemoStory_SequenceIsParsable`（job=`11375170b8e047d189a8610e5271b3e4`）passed。
- PlayMode：
  - `Restore_可重播CharJson_不應產生CharTransitionStepsError`（job=`9b65645b2d904522a8817eae3514034f`）passed。
  - `CharTransitionSteps_可重播契約_可從任意狀態收斂`（job=`5f60c299ec6d43518b60d1633411189d`）passed。
  - `RollbackButton_快速連按_會排隊倒帶到最前句`（job=`f69613c2ca9d4c9aa4fdfe630050a88f`）passed。
  - `RollbackButton_高壓連按_讀檔後仍可回到存檔前且無錯誤`（job=`33d5593029814310be3cbe804a714d6b`）passed。
- Console：
  - `read_console(types=["error"])` 目前為 0 筆。

### P-022 使用者同意後提交本輪修正
- commit：`35ca22e`
- 訊息：`修正Demo char steps契約並同步更新測試`
- 檔案：
  - `Assets/OpsidanosInk/Demo/story.ink`
  - `Assets/OpsidanosInk/Demo/story.json`
  - `Assets/Editor/Tests/CharTagJsonTests.cs`

### P-023 提交後再次確認「倒帶邏輯 vs 點擊節奏」
- 測試（PlayMode）：
  - `ContinueButton_Busy時第一次只補完不前進_冷卻後第二次才前進_Normal`（job=`629a88ed34d3422699a14ce586cf385c`）passed
  - `ContinueButton_Busy時第一次只補完不前進_冷卻後第二次才前進_Restore`（job=`dc9ccbe8561c42c2a92c69d579bf5d37`）passed
  - `RollbackButton_快速連按_會排隊倒帶到最前句`（job=`2b208c1a4be3435eb2225a9786ef8b20`）passed
  - `RollbackButton_高壓連按_讀檔後仍可回到存檔前且無錯誤`（job=`2f39fdf2a24145d8a73a2a2906cba15a`）passed
  - `CharLayer_Normal與Restore_固定重排規則相反_且ForceComplete不會破壞`（job=`47ba380456e049c1bb04039d950d2fc1`）passed
- Console：
  - `read_console(types=["error"])`：0 筆
- 判定：
  - 倒帶（Restore）邏輯與一般點擊（Normal）的重排差異、以及 Busy→ForceComplete→冷卻→再前進節奏，皆維持既定規範，未被本次修正破壞。

### P-024 補上「Rollback 體感路徑」回歸測試並完成整包驗證
- 修改：
  - `Assets/Tests/PlayMode/OpsidanosInkPlayModeUiClickTests.cs`
- 新增測試：
  - `Rollback後_相反重排可感知`
  - 以實際 UI 點擊推進到目標句後，先驗證 Normal 路徑最上層角色是 `ss`，再點擊 `RollbackButton` 驗證 Restore 路徑最上層角色切為 `bs`。
- 新增輔助方法：
  - `GetCharacterActorLayer`、`GetTopmostActorName`、`GetChildIndex`
  - `IsTargetNormalOutputForRollbackPerception`（避免硬編步數，改用 tag 條件搜尋目標句）。
- MCP 測試：
  - 單測：`OpsidanosInk.Tests.OpsidanosInkPlayModeUiClickTests.Rollback後_相反重排可感知`
    - job=`e2cf51d931994a9a89885cf93b379e80`，`passed (1/1)`
  - 整包類別：`OpsidanosInk.Tests.OpsidanosInkPlayModeUiClickTests`
    - job=`d2a6f00bea674a17aad9b356b679d2e9`，`passed (10/10)`
  - Console：`read_console(types=["error"])` = 0 筆
- 判定：
  - 「倒帶路徑的相反重排」已用可量測的 UI 體感路徑鎖住，且不影響既有 Continue 冷卻節奏與 Rollback 連按行為。

### P-025 重新盤點：為何仍有「倒退連按可連續跳句」體感
- 盤點結果：
  - Runtime 現況仍為「Rollback 排隊協程」設計（`pendingRollbackRequests` + `RollbackCoroutine`）。
  - PlayMode 現況測試仍鎖「快速連按會排隊倒帶到最前句」語義。
- 結論：
  - 問題不是使用者誤感，而是程式與測試共同把舊語義鎖住，尚未改成「倒退與前進同節奏」。

### P-026 本輪修正策略（已同意）
- 目標：
  - `OnClickRollback` 對齊 `OnClickContinue` 的節奏：`Busy→ForceComplete→冷卻→下一次才倒帶`。
- 具體做法：
  - Runtime：移除 `pendingRollbackRequests`/`RollbackCoroutine`/`StopRollbackCoroutine` 舊路徑。
  - Test：移除「排隊倒帶」語義，改為驗證「Busy 首點不倒帶、冷卻內不倒帶、冷卻後才倒帶一步」。

### P-027 已實作：Runtime + PlayMode 契約同步（待驗證）
- Runtime 修改：
  - `VNPlayerPresenter.OnClickRollback()` 已改為：
    1. 先套用 `IsClickInCooldown()`
    2. Busy 時先 `TryForceCompleteIfBusy(isClick:true)` 並返回
    3. 非 Busy 且非冷卻才 `saveSystem.TryRollbackOnce()` 一次
  - 已移除舊排隊路徑：`pendingRollbackRequests`、`RollbackCoroutine()`、`StopRollbackCoroutine()`
- PlayMode 測試修改：
  - 移除舊語義測試（排隊倒帶/高壓清空）
  - 新增同節奏語義測試：
    - `RollbackButton_Busy時第一次只補完不倒帶_冷卻後第二次才倒帶`
    - `RollbackButton_快速連按_不會排隊連續倒帶`
- 下一步：
  - 用 MCP 跑 `OpsidanosInkPlayModeUiClickTests` 目標測試與整包，確認 Console 無 Error。

### P-028 MCP 驗證與修正回圈（完成）
- 第一次單測失敗：
  - 測試：`RollbackButton_Busy時第一次只補完不倒帶_冷卻後第二次才倒帶`
  - job=`97664ab2790a403285b2b9b263caff87`
  - 錯誤：`Busy 第一次點擊不應消耗 rollback 步數`（預期 2，實際 3）
- 根因與修正：
  - 根因：`rollbackStepsBefore` 取值時機太早（在 `EmitExternalOutput` 前），導致基準步數偏差。
  - 修正：兩個新測試都改為「`WaitUntilBusy` 後才擷取 `rollbackStepsBefore`」。
- 重跑結果：
  - `RollbackButton_Busy時第一次只補完不倒帶_冷卻後第二次才倒帶`
    - job=`a0abe0e5509c40c9a5f92406d803045f`，`passed (1/1)`
  - `RollbackButton_快速連按_不會排隊連續倒帶`
    - job=`d8e9f10907eb4458a4a881a013f1a0b7`，`passed (1/1)`
  - 整包 `OpsidanosInk.Tests.OpsidanosInkPlayModeUiClickTests`
    - job=`28745365763c4e4692f784c6b2b7249f`，`passed (10/10)`
  - Console `error`：0 筆

### P-029 新任務啟動：Rollback 測試改為 caseId 注入
- 目標：
  - 把目前兩條 Rollback 節奏測試抽成「共用流程 + caseId 參數表」。
  - 後續若規格差異只需改 case，不需重寫整段測試。
- 預計改動：
  - `OpsidanosInkPlayModeUiClickTests.cs` 新增 case 定義模型與查表方法。
  - 兩個 Rollback 測試改為只傳入 `RBK_xxx`，共用同一條執行流程。

### P-030 已實作：Rollback 測試改為 caseId 注入架構
- 已新增：
  - `RollbackRhythmCase` 定義（`CaseId`、步數門檻、點擊密度、外部輸出代號）。
  - `RollbackRhythmCases` 對照表（`RBK_001`、`RBK_002`）。
  - 共用 runner：`RunRollbackRhythmCaseById(caseId)`。
  - 共用點擊 helper：`ClickButtonMultipleTimes`。
- 已調整：
  - `RollbackButton_Busy時第一次只補完不倒帶_冷卻後第二次才倒帶` → 改為呼叫 `RBK_001`
  - `RollbackButton_快速連按_不會排隊連續倒帶` → 改為呼叫 `RBK_002`
- 狀態：
  - 程式修改完成，待 MCP 實跑驗證。

### P-031 MCP 驗證：caseId 注入架構
- 單測：
  - `RollbackButton_Busy時第一次只補完不倒帶_冷卻後第二次才倒帶`
    - job=`9f0dbf2a174b44dda85d37e34f5588a6`，`passed (1/1)`
  - `RollbackButton_快速連按_不會排隊連續倒帶`
    - job=`12241c6765714bfeaf2b038096968ca0`，`passed (1/1)`
- 整包：
  - `OpsidanosInk.Tests.OpsidanosInkPlayModeUiClickTests`
    - job=`3a90e6d2c9774248bc9312eb5ac6baab`，`passed (10/10)`
- Console：
  - `read_console(types=[\"error\"])` = 0 筆
- 判定：
  - caseId 注入架構可用，且與目前同節奏契約一致。

### P-032 契約比對（只讀審核）
- 已讀：
  - `Documentation/DeveloperModeOutputContract.md`
  - `VNPlayerPresenter.cs` 今日差異
  - `OpsidanosInkPlayModeUiClickTests.cs` 今日差異
- 結果：
  - 未發現硬違反條文。
  - 發現 3 個文件補全點（測試索引、Rollback 同節奏不變式、caseId 測試治理）。
- 下一步：
  - 先向使用者提出「DeveloperModeOutputContract 補回提案」，確認後再改文件。

### P-033 依同意完成契約補回
- 修改檔案：
  - `Documentation/DeveloperModeOutputContract.md`
- 補回內容：
  - 更新最後更新日期（`2026/02/22`）。
  - §7 補齊目前測試索引（含 `OpsidanosInkPlayModeUiClickTests.cs`）。
  - 新增 §7.1（輸入節奏不變式）。
  - 新增 §7.2（Rollback caseId 測試治理）。
  - 新增 §7.3（節奏改動最小驗收集合）。
- 驗證：
  - 以 `rg` 檢查章節標題與關鍵字已存在（`7.1/7.2/7.3`）。
