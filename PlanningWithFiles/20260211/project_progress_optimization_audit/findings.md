# Findings：專案進度整理、優化方案與下一步

## F-001 任務初始化
- 已建立本輪專用任務三檔。
- 本輪採用「調查一次、記錄一次；結論一條、單獨一筆」規則。

## F-002 目前版本基線
- （2026-02-11 當時）工作樹為乾淨狀態。
- 現行基線為 `dc8a443`，後續所有現況判讀都必須以這個提交為起點。

## F-003 調查重心分配
- 檔案量顯示主要複雜度在 `Packages`，次要在 `Assets`。
- 後續調查應先拆 `Packages/com.opsidanos.ink`（自有 Runtime）與 `Assets/Editor/FlowChart/GraphToolkit`（自有 Editor）兩條主線。

## F-004 歷史任務完整性
- 歷史任務已確認覆蓋 2026-01-20 到 2026-02-11。
- 本輪要避免斷層，必須至少逐份核對：
  - 對話主線：`20260120`、`20260125`、`20260126`、`20260130`、`20260201`
  - 驗收主線：`20260204`、`20260205`
  - GraphToolkit 主線：`20260206`、`20260208`
  - 狀態主線：`20260211/project_status_assessment`、`20260211/dialogue_restore_scope_audit`

## F-005 本輪調查的硬性約束
- 一般檔案不可直接改，先提案並等同意。
- 本輪可以直接改的只有 PlanningWithFiles 任務三檔。
- 本輪若涉及 UI/Graph 判讀，必須回扣 `UIToolkitSpec.md` 與 `GraphToolkitSpec.md`。
- 禁止防禦性編碼是專案明定規則，優化方案不能提出與此相反的方向。

## F-006 UI 規格評估基準
- 本專案 UI 評估必須以 `UIToolkitSpec.md` 的允許清單為準。
- 後續若提出 UI 優化，不能引入規格外的 UXML 元素或 USS 屬性。

## F-007 GraphToolkit 邊界基準
- GraphToolkit 僅限 Editor，不可當 Runtime 執行層使用。
- GraphToolkit 模組的問題應先落在 Editor 組件與 Editor 測試，不應直接套到 Runtime 對話播放鏈。

## F-008 對話系統原始規格主線（第一批）
- `20260120` 明定：Runtime 只讀 `.json`，Flow Chart 屬 Editor 作者工具鏈。
- `20260125` 明定：點擊兩段式（Busy 先 ForceComplete，完成後再點才 Continue）。
- `20260126` 明定：Auto 必須在「打字機+動畫完成後」再等 1 秒，且不能與點擊衝突。
- `20260130` 明定：Save/Load/Rollback 是主線能力，不是附屬功能。

## F-009 Restore 與驗收規格基準
- `20260201` 明定：Restore 層級規則要與 Normal 相反，且必須覆蓋「動作中 + 動作後重排 + ForceComplete」。
- `20260204` 明定：驗收重點是 Runtime 可玩性與 Save/Load/Rollback/Restore 層級一致性。
- `20260205` 明定：UI 點擊測試與一鍵跑測試屬驗證工具層，不等於功能設計本身。

## F-010 近期計畫之間的關係與張力
- `20260206` 把 GraphToolkit 匯入匯出與部分 Runtime 議題放在同一主計畫，邊界風險高。
- `20260208` 已把 GraphToolkit 反覆問題獨立成根因調查任務（屬正向拆分）。
- `20260211/project_status_assessment` 的 S5 記錄包含 Runtime 對話修補，顯示 GraphToolkit 線與對話線在交付末段再次交疊。
- `20260211/dialogue_restore_scope_audit` 是對上述交疊的補救型稽核任務。

## F-011 關鍵提交演進（可追證）
- `896d997`：一次混入 Runtime（Save/Presenter/UI）+ FlowChart Editor 舊版 + PlayMode 測試（37 檔，2578 行新增）。
- `84cfd67`：GraphToolkit 匯出 MVP 與測試（11 檔，866 行新增）。
- `dc8a443`：GraphToolkit 匯入 MVP 與測試穩定性（9 檔，770 行新增，無 Runtime 對話程式變更）。

## F-012 自有組件邊界（可驗證）
- `OpsidanosInk.Runtime`：對話與播放核心，依賴 Ink + Addressables。
- `OpsidanosInk.FlowChartEditor`：Editor-only，依賴 GraphToolkit 三個 Editor 組件。
- `OpsidanosInk.EditModeTests`：同時驗 Runtime 與 FlowChartEditor。
- `OpsidanosInk.PlayModeTests`：只驗 Runtime。
- 因此：GraphToolkit 問題與 Runtime 對話問題可以在 asmdef 層明確分流，不應混判。

## F-013 目前程式工作量分佈
- 自有功能核心檔案數量不大（Runtime 16 + Editor 19），理論上可完整建立人工可讀的資料鏈。
- 外部導航測試（38 檔）遠多於自有 PlayMode 測試（2 檔），若跑整組測試容易被外部測試噪音淹沒。

## F-014 場景主鏈與測試主鏈一致
- 目前執行主場景是 `Assets/Scene/Test.unity`（BuildSettings enabled）。
- 自有 PlayMode 測試也明確要求 `Test.unity` + `VNPlayer`。
- `Assets/OffMeshLinkScene.unity` 目前屬「存在但未啟用」場景，不是自有對話回歸主鏈。

## F-015 `Test.unity` 的 VN 主鏈已形成「單根物件全綁定」
- `VNPlayer` 這個根物件同時承載 Story、UI、Presenter、Tag Router、Tag Players、SaveSystem、ResourceMap。
- 這種配置的優點是可快速驗證；風險是任一元件掛點都會影響整條對話播放鏈。
- 既有 PlayMode 測試直接依賴此配置（找 `VNPlayer`），因此場景結構改動屬高風險變更。

## F-016 對話前進規則在程式層已落地為「兩段式點擊」
- `VNPlayerPresenter.OnClickContinue/OnClickChoice` 都是同一個順序：
  1) 若 Auto 倒數中，先作廢這次 Auto
  2) 若 Busy（打字機或動畫），第一次點擊只 ForceComplete 並進冷卻
  3) 冷卻後第二次點擊才真正 `Continue/ChooseChoice`
- 場景參數 `forceCompleteClickCooldownSeconds=0.3`、`autoDelaySeconds=1` 與既有規格敘述相符。

## F-017 目前測試覆蓋有缺口：兩段式點擊「節奏行為」缺直接驗證
- 既有 PlayMode 測試有驗 Save/Load/Rollback/Restore 層級、多槽，以及 UI 按鈕接線。
- 但缺少「連續點 ContinueButton：第一次只補完、等待冷卻、第二次才前進」這條直接 UI 行為驗證。
- 也缺少「倒退（Restore）進行中，點擊第一次 ForceComplete、冷卻後第二次再前進」的整段回歸測試。

## F-018 Restore 層級相反規則已在實作與單點測試存在
- `InkTagCharacterStatePlayer` 在 Transition 與 ForceComplete 兩條路徑都看 `StoryOutputSource.Restore`，採相反重排。
- 既有 `CharLayer_Normal與Restore_固定重排規則相反_且ForceComplete不會破壞` 有覆蓋「Restore + ForceComplete 後固定重排仍相反」。
- 風險：目前測試偏「單點驗證」，未覆蓋多步連續點擊與冷卻節奏的整段行為。

## F-019 Story/資源/UI 的資料鏈可閉合
- `story.json` 產生 tag -> `InkTagEventRouter` 分流 -> 各 Tag Player 讀 `resource_map.json` -> 操作 `VNPlayer.uxml` 的對應元素。
- 場景上 `VNPlayer` 同時持有上述元件與引用，資料鏈不需要跨物件查找即可完成。
- 風險：單點集中配置提高了回歸敏感度（任何一個欄位遺失會直接破主流程）。

## F-020 GraphToolkit 與 Runtime 對話是兩條可分離主線
- GraphToolkit 功能與測試位於 `Assets/Editor/FlowChart/GraphToolkit` 與 `Assets/Editor/Tests`，屬 Editor 線。
- Runtime 對話與 PlayMode 驗收位於 `Packages/com.opsidanos.ink/Runtime` 與 `Assets/Tests/PlayMode`。
- 兩者可在 asmdef 與目錄層明確分流；若任務目標是 GraphToolkit，不應用 Runtime 對話測試當主驗收。

## F-021 本輪已捕捉並修正一個調查偏差
- 錯誤路徑 `Assets/Tests/EditMode` 已修正為 `Assets/Editor/Tests`。
- 已把錯誤與修正過程留在 `progress.md`，避免再次走錯目錄。

## F-022 最近三個關鍵提交的「目標一致性」現況
- `dc8a443` 與 `84cfd67` 主體是 GraphToolkit（Editor 線）。
- `896d997` 是 Runtime 對話 + 舊 FlowChart Editor 的混合提交，這是歷史上的邊界混線來源。
- 因此若要判讀「最近一次提交是否誤改對話系統」，答案是：`dc8a443` 本身沒有改 Runtime 對話；混線問題主要來自較早的混合提交結構。

## F-023 OffMeshLinkScene 目前不在對話主驗收鏈
- `OffMeshLinkScene` 不在 BuildSettings 啟用場景中。
- 目前自有 PlayMode 測試與主場景都是 `Test.unity`。
- 這代表它目前是旁支資產，不是對話主線交付阻塞點。

## F-024 現有測試矩陣可分三層
- Editor-資料層：`CharTagJson`、`ResourceMap`、`SaveData`
- Editor-GraphToolkit：`Export/Import/Smoke`
- PlayMode-對話流程：`Save/Load/Rollback/Restore` + UI 按鈕接線
- 目前最關鍵的回歸缺口在 PlayMode-對話流程層：缺少 ContinueButton 兩段式點擊節奏測試。

## F-025 優化方案主軸 A：先補「兩段式點擊節奏」回歸測試
- 先補測試再動程式，可避免再次在理解不完整時改動核心對話邏輯。
- 必補案例：
  - Continue 點第一次只 ForceComplete，不前進
  - 冷卻內連點不前進
  - 冷卻後第二次點擊才前進
  - Restore 輸出下同樣遵守上述節奏

## F-026 優化方案主軸 B：GraphToolkit/Runtime 驗收分流
- 每次任務先標記主線目標（GraphToolkit 或 Runtime 對話），再只跑該主線測試。
- GraphToolkit 任務：優先 `Assets/Editor/Tests/InkFlowChart*`
- Runtime 對話任務：優先 `Assets/Tests/PlayMode/OpsidanosInkPlayMode*`
- 這能降低「改 Editor 卻用 Runtime 問題解釋」與「改 Runtime 卻只看 Editor 結果」的風險。

## F-027 優化方案主軸 C：降低 `VNPlayer` 單點耦合風險
- 現在 `VNPlayer` 同點承載 Story/UI/Save/Tag，回歸敏感度高。
- 短期不拆架構，先補「場景鏈完整性檢查清單」：
  - `story.json`、`resource_map.json`、`UIDocument`、`InkSaveSystem`、`advanceBlockers` 是否全綁定
  - 缺一項就直接報錯並終止驗收
- 中期再評估是否把 `VNPlayer` 提煉成可重用 Prefab + 最小測試場景，降低手改場景造成的隱性回歸。

## F-028 Git/工作樹追證：關鍵敘述與 HEAD 一致
- `896d997` / `84cfd67` / `dc8a443` 的提交統計與檔案範圍，與 F-011 的數字一致。
- `dc8a443` 的變更清單只包含 `.gitignore`、GraphToolkit 程式、Editor 測試；未包含 `Packages/com.opsidanos.ink/Runtime`（與 F-022 一致）。
- `ProjectSettings/EditorBuildSettings.asset` 目前啟用場景為 `Assets/Scene/Test.unity`，`Assets/OffMeshLinkScene.unity` 存在但不在啟用清單（與 F-014 / F-023 一致）。
- `Assets/Scene/Test.unity` 內的 `VNPlayer` 直接綁定：
  - `InkStoryEngine.storyJsonAsset` -> `Assets/OpsidanosInk/Demo/story.json`
  - `InkResourceMap.resourceMapJson` -> `Assets/OpsidanosInk/Demo/resource_map.json`
  - `UIDocument.sourceAsset` -> `Packages/com.opsidanos.ink/Runtime/UI/UXML/VNPlayer.uxml`

## F-029 數量口徑補註（避免誤讀）
- F-013 的「Editor 自有 `.cs`：19」是 `Assets/Editor` 全部 `.cs` 的數量，包含 `Assets/Editor/Tests`（若只算非測試腳本，為 12）。
- F-013 的「外部 `com.unity.ai.navigation` 測試 `.cs`：38」是 `Packages/com.unity.ai.navigation/Tests` 的測試檔數（整個 `Packages/com.unity.ai.navigation` 的 `.cs` 總數為 91）。

## F-030 玩家模式的「正確規範」來源是 Runtime 實作（不是測試註解）
- `InkTagCharacterStatePlayer` 會根據「目前畫面角色狀態」與「目標角色狀態」計算 `requiredActions`：
  - 新角色進場 -> 必須有 `Appear`
  - 角色換槽 -> 必須有 `Move`
  - 角色離場 -> 必須有 `Disappear`
- `BuildTransitionSchedule(...)` 若發現 `steps` 沒有覆蓋 `requiredActions`，會 `Debug.LogError`：
  - `[OpsidanosInk] char.transition.steps 缺少必要動作：...`
  - 並把缺少的動作補到排程最後面（確保畫面狀態正確）

## F-031 目前 PlayMode 測試失敗的根因是「測試用錯規範」
- 失敗測試：`OpsidanosInkPlayModeTests.Restore_缺少Appear時_不應產生CharTransitionStepsError`
- 該測試餵的 `charJson` 明確缺少 `Appear`，但同時又期望「不產生 Error」。
- 依 F-030 的 Runtime 規範，缺少必要動作必然會產生 Error log；因此此測試的期待值（不應產生 Error）是錯的。

## F-032 `steps` 是「狀態相依」還是「可重播」：這是開發者模式必須先定義的契約
- `Assets/OpsidanosInk/Demo/story.ink` 的範例明確存在一種寫法：`steps` 只做 `raiseActors` + `move`，不包含 `appear`。
  - 這種寫法在「正常順序播放」時可能完全合法，因為被 raise 的角色（例如 `alice`）在上一句已經存在。
  - 但一旦把同一筆輸出套用在「空畫面」（例如讀檔後畫面未重建、或測試剛載入場景）就會變成不合法：`raiseActors` 會指向不存在角色、且 `requiredActions` 會要求 `Appear`。
- 因此要讓整條鏈（GraphToolkit 輸出 -> Ink -> 玩家模式 -> Save/Load/Rollback/Restore -> 逆向讀回）閉環，只有兩種一致的制度化設計：
  1) **輸出可重播（建議）**：每一筆 `char` 輸出都必須能在「任意合理初始畫面狀態」下收斂到目標狀態（至少包含「空畫面」）。做法是：不輸出 `steps`（用預設 Appear/Move/Disappear），或輸出的 `steps` 必須包含 `appear/move/disappear` 的動作類型（多餘動作允許但不可缺必要動作）。
  2) **輸出狀態相依**：允許 `steps` 假設上一句的畫面狀態存在；但這會迫使 Restore/Load 必須「重播一段歷史」或「額外保存完整畫面快照」，否則必然遇到缺 `Appear` 這類錯。

## F-033 已選定並文件化的制度化設計：輸出可重播（快照式）
- 文件：`Documentation/DeveloperModeOutputContract.md`
- 核心規則（最小契約）：
  - `char` 必須用狀態化 JSON 描述 left/center/right 的最終站位
  - 只要輸出 `transition.steps`，就必須在 steps 裡安排 `appear/move/disappear` 三種動作（各最多一次），避免 Runtime 因缺必要動作而噴紅字再補齊
  - `raiseActors` 必須保證當下角色存在（同一步有 appear 或之前已存在）

## F-034 目前工作樹狀態（2026-02-13）
- 目前工作樹有未提交變更（GraphToolkit/測試/README 等多檔），且 `Documentation/DeveloperModeOutputContract.md` 目前尚未被 git 追蹤（`git status` 顯示 `??`）。
- 後續若要以 `dc8a443` 重現或比對，必須先釐清哪些變更要保留、哪些要還原，避免把「工作樹狀態」誤當成「基線狀態」。
