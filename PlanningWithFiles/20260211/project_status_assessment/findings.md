# 發現紀錄

## 需求整理
- 使用者要求：先完整盤點專案進度與內容，再評估下一步。
- 使用者要求：反覆檢查假設，避免樂觀推斷。
- 使用者要求：遵守 `AGENTS.md` 流程，修改前先提案（`task_plan.md`、`findings.md`、`progress.md` 例外）。

## 專案現況（大圖）
- 目前分支：`arcumit/CodexInk`，相對遠端 `ahead 9`。
- 最近已提交主線（由新到舊）：
  - `dc8a443`：收斂 Graph Toolkit 匯入 MVP 與測試穩定性
  - `84cfd67`：Graph Toolkit 匯出 MVP 與測試
  - `5885d3c`：工具入口統一到 `Tools/OpsidanosInk`
  - `6117234`：移除舊 IMGUI Flow Chart，改單一 Graph Toolkit 主線
  - `896d997`：多槽存讀 + Flow Chart 匯入匯出修復
- 目前工作樹重點：S4 驗收產生 `Assets/FlowCharts/*.inkfc`（3 組，未追蹤）。

## 資料鏈（程式 -> 場景 -> 測試）
### Runtime 玩家主線（已成形）
- 程式核心：
  - `InkStoryEngine`（故事輸出）
  - `InkTagEventRouter`（Tag 分派）
  - `VNPlayerPresenter`（UI 與互動）
  - `InkSaveSystem`（多槽/Auto/倒帶）
- 場景接線：`Assets/Scene/Test.unity`
  - `VNPlayerPresenter.saveSystem -> InkSaveSystem`
  - `InkSaveSystem.storyEngine -> InkStoryEngine`
  - `InkTagEventRouter.storyEngine -> InkStoryEngine`
  - `UIDocument.sourceAsset -> VNPlayer.uxml`
- Prefab 現況：
  - `Assets` 內僅有 2 個 Ink Demo prefab（`Text.prefab`、`Button.prefab`）。
  - 目前 OpsidanosInk 主流程不是靠 prefab 連線，而是直接在 `Test.unity` 的 `VNPlayer` 物件掛元件。
- 測試覆蓋：PlayMode + Editor 測試總數 24 筆（目前從檔案統計）
  - PlayMode：UI 點擊、存讀檔、倒帶壓測

### Editor GraphToolkit 主線（進行中）
- 程式核心：
  - `InkFlowChartGraph.cs`（建立新圖、匯出/匯入選單入口）
  - `InkFlowChartExporter.cs`（`.inkfc` -> `.ink + .flowchart.json`）
  - `InkFlowChartImporter.cs`（`.flowchart.json + .ink` -> `.inkfc`）
- 測試核心：
  - `InkFlowChartGraphSmokeTests.cs`
  - `InkFlowChartExportTests.cs`
  - `InkFlowChartImportTests.cs`（新檔）
- 規格對照：`GraphToolkitSpec.md` 已含 `PromptInProjectBrowserToCreateNewAsset`、`CreateGraph`、`LoadGraphForImporter` 基本規則。

## 本輪工作樹差異重點
- `InkFlowChartGraph.cs`：
  - 建立入口改成 `Tools/OpsidanosInk/Flow Chart Graph/建立新圖`
  - 建圖改走 `GraphDatabase.CreateGraph` + GUID 檔名
  - 新增匯入選單入口與路徑共用判斷
- `InkFlowChartImporter.cs`（新）：
  - 匯入流程改為先建暫存 `.inkfc` 再 Move 到正式路徑
  - 目標是降低 Undo/同路徑重建警告
- `InkFlowChartExporter.cs`：
  - `LoadGraphForImporter` 失敗時回退 `LoadGraph`
- `InkFlowChartExportTests.cs`：
  - 不再複製 fixture，改為每次直接建圖
  - 清理策略改只刪固定資料夾
- `InkFlowChartImportTests.cs`（新）：
  - 補 3 個匯入案例（成功、缺 startNodeId、缺 .ink）
  - 改用 `UnityTearDown` + 反射等待 InkCompiler 收尾再清理
- `.gitignore`：
  - 新增 Graph Toolkit 測試暫存路徑前綴忽略
- `Assets/OffMeshLinkScene.unity`：
  - 只有 fileID 變動，內容行為看起來像場景重新序列化。

## 碎片改動整理（可停點段落）
### A 段：匯入 MVP 主功能（可單獨停）
- `/Users/arcumit/Documents/GitHub/ink-unity-integration/Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`（新增）
- `/Users/arcumit/Documents/GitHub/ink-unity-integration/Assets/Editor/FlowChart/GraphToolkit/InkFlowChartGraph.cs`（新增匯入選單入口）
- 說明：這段是完整「選單觸發匯入 -> 建圖 -> 回傳結果」主流程，屬核心功能段落。

### B 段：匯出穩定性補丁（可單獨停）
- `/Users/arcumit/Documents/GitHub/ink-unity-integration/Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`
- 說明：只補 `LoadGraphForImporter` 失敗 fallback，屬穩定性修補，不改功能介面。

### C 段：測試收斂（可單獨停）
- `/Users/arcumit/Documents/GitHub/ink-unity-integration/Assets/Editor/Tests/InkFlowChartImportTests.cs`（新增）
- `/Users/arcumit/Documents/GitHub/ink-unity-integration/Assets/Editor/Tests/InkFlowChartExportTests.cs`（改 fixture 建法與清理）
- `/Users/arcumit/Documents/GitHub/ink-unity-integration/Assets/Editor/Tests/InkFlowChartGraphSmokeTests.cs`（清理策略微調）
- 說明：這段主要是把測試從「容易競速」改成「等待編譯收尾再清」。

### D 段：工作樹噪音（建議切離）
- `/Users/arcumit/Documents/GitHub/ink-unity-integration/Assets/OffMeshLinkScene.unity`
- 說明：目前只見 fileID 重排，沒有功能邏輯變化，屬非主線噪音候選。

## Unity MCP 實證（本輪追加）
- `manage_scene.get_active`：目前活動場景是 `Assets/Scene/Test.unity`。
- `manage_scene.get_build_settings`：Build Settings 只有 `Assets/Scene/Test.unity` 啟用；`OffMeshLinkScene` 不在啟用清單。
- `run_tests(EditMode, OpsidanosInk.EditModeTests)`：11/11 passed。
- `read_console(types=[warning,error])`：測前清空後，測後 0 筆 warning/error。
- `OffMeshLinkScene.unity` 內元件 guid 對應 `Packages/com.unity.ai.navigation/Runtime/NavMeshLink.cs.meta`，屬導航套件元件，不是 OpsidanosInk 功能腳本。

## 本輪結論（根據 MCP 與檔案證據）
1. 目前未提交 GraphToolkit 這批改動，在你現在的 Unity 狀態下可通過 `OpsidanosInk.EditModeTests` 且無 warning/error。
2. `OffMeshLinkScene.unity` 目前可合理視為非主線噪音（不在 active scene，不在 build 啟用場景，且內容為導航套件場景物件序列化）。
3. 仍不能樂觀保證「之後永遠不回歸」：GraphToolkit warning 的歷史根因仍是套件時序，後續若改建圖/清理策略，風險可再出現。

## S1 收斂執行結果（已實作）
1. 已補 `.gitignore`：新增 `/Assets/Editor/TmpGraphToolkitImport_*`，修正與匯入測試前綴不一致問題。
2. 已整理 `InkFlowChartGraph.cs`：把重複結束標記改成單一可讀區塊說明，程式邏輯不變。
3. 已排除 `OffMeshLinkScene.unity`：還原到 `HEAD`，不再混入本批主線變更。
4. S1 後再驗證：`OpsidanosInk.EditModeTests` 11/11 通過，warning/error 仍為 0。

## S2 收斂執行結果（已實作）
1. 已再收斂 `InkFlowChartGraph.cs` 註解邊界：移除多餘的獨立註解區塊，保留單一結束標記與清楚的日期原因說明。
2. S2 後再驗證：`OpsidanosInk.EditModeTests` 11/11 通過，warning/error 仍為 0。

## S4 驗收結果（本輪新增）
### 1) 測試層
- EditMode：`OpsidanosInk.EditModeTests` 11/11 通過，warning/error 0。
- PlayMode：`OpsidanosInk.PlayModeTests` 13 筆中 1 筆失敗。
  - 失敗測試：`OpsidanosInk.Tests.OpsidanosInkPlayModeTests.Restore_缺少Appear時_不應產生CharTransitionStepsError`
  - 失敗訊息：`char.transition.steps 缺少必要動作：Appear`（另有 `raiseActors` 指向不存在角色）
  - 單獨重跑該測試仍失敗，屬可重現，不是偶發。

### 2) 菜單真流程（3 輪）
- 每輪執行：`建立新圖 -> 匯出選中圖 -> 匯入選中匯出檔`
- 三輪都出現同樣結果：
  - `建立新圖`：成功建立 `.inkfc`，但 Console 出現 `same path as an existing asset` warning（來源 Graph Toolkit 內部）
  - `匯出選中圖`：失敗（新建空圖缺少開始節點）
  - `匯入選中匯出檔`：選單執行失敗（disabled/context-dependent，因 selection 不是 `.flowchart.json`）

### 3) 驗收副產物
- 產生 3 組未追蹤檔案：
  - `Assets/FlowCharts/NewInkFlowChartGraph_ed90e3160ac54bcebb99e9991bcac964.inkfc` + `.meta`
  - `Assets/FlowCharts/NewInkFlowChartGraph_e62664f05ffe4b22a7cc13e9a1adc4a4.inkfc` + `.meta`
  - `Assets/FlowCharts/NewInkFlowChartGraph_880c03beaabf4531affbf21761848bd8.inkfc` + `.meta`

## S5-1 清理結果（本輪新增）
1. 已依使用者同意刪除上述 3 組驗收副產物，並清空 `Assets/FlowCharts`。
2. 清理後 `git status` 為乾淨，確認沒有遺留未追蹤驗收檔案。

## S5-2 修復結果（本輪新增）
1. 修改檔案：`Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/InkTagCharacterStatePlayer.cs`。
2. 修復策略：
   - `BuildTransitionSchedule(...)` 在 `Restore` 且缺 `Appear` 時，改為把 `Appear` 補到排程最前面（不再補到最後）。
   - `Restore` 的「缺必要動作」訊息改為一般 `Debug.Log`，`Normal` 仍維持 `Debug.LogError`。
3. 為何有效：
   - 先補 `Appear` 後，`raiseActors` 不會先在角色尚未出現時執行，避免誤報錯誤。
   - `Restore` 自動補步驟屬預期修復流程，不應再被視為 Error。
4. 驗證結果：
   - 單測：`Restore_缺少Appear時_不應產生CharTransitionStepsError` 通過。
   - PlayMode 全組：`OpsidanosInk.PlayModeTests` 13/13 通過。
   - EditMode 全組：`OpsidanosInk.EditModeTests` 11/11 通過。
   - 兩次測後 Console：warning/error 皆為 0。

## 本輪發現的不一致（需要先整理）
| 項目 | 現況 | 風險 |
|------|------|------|
| 匯入暫存前綴 vs `.gitignore` | 已補上 `TmpGraphToolkitImport_*` | 風險已降為低 |
| `InkFlowChartGraph.cs` 變更標記結尾 | 已收斂成單一結束標記 | 風險已降為低 |
| `OffMeshLinkScene` 混入本批 | 不在 GraphToolkit 匯入/匯出資料鏈中 | 提交邊界不清，回溯會混淆 |
| 菜單流程與功能期待 | `建立新圖` 產生空圖，但驗收流程緊接著要求可匯出可匯入 | 若無「先補節點」步驟，菜單驗收會固定失敗 |

## 關鍵風險（先不要樂觀）
| 風險 | 目前證據 | 影響 |
|------|----------|------|
| Graph Toolkit `same path` warning 可能再次回來 | 2026-02-08 已定位根因在 package 內部時序，不是只有檔名問題 | 若驗收要求 warning=0，新改動很容易再破功 |
| 匯入測試清理時序容易踩 Ink 編譯競速 | 歷史多次記錄 `Ink file ... was not found after compilation` | 會出現假失敗與不穩定回歸 |
| `LoadGraphForImporter` 讀不到新建圖資產 | 工作樹已補 fallback，代表此問題確實發生過 | 匯出可能誤報「找不到圖」 |
| Scene 檔混入非主線改動 | `Assets/OffMeshLinkScene.unity` 目前只有序列化 id 變動 | 提交時容易污染本次功能範圍 |
| PlayMode 既有測試失敗 | `Restore_缺少Appear時_不應產生CharTransitionStepsError` 可重現失敗 | 目前專案不是完整綠燈狀態，後續變更驗收會被 Runtime 失敗遮蔽 |

## 目前判斷（像 8 歲也能懂）
- Runtime 像「已經能玩的玩具車」，現在主要是微調，不是壞掉。
- GraphToolkit 像「正在組裝的新軌道」，車子可以放上去，但還沒完成最後穩定固定。
- 所以下一步不是先加更多軌道，而是先把現在這段軌道鎖緊，確保每次推都不會掉下來。

## 下一步建議（先後順序）
1. 先做「未提交 GraphToolkit 改動」的驗收收斂：
- 目標：跑一輪只針對 `OpsidanosInk.EditModeTests` 的乾淨驗收，確認 warning/error 與 3 類匯入案例。
2. 做「範圍清理」：
- 目標：把 `Assets/OffMeshLinkScene.unity` 這類非主線變更分離，避免影響本批判讀。
3. 驗收通過後再提案提交：
- 目標：用一個聚焦 commit 收斂匯入 MVP（程式 + 測試 + `.gitignore`），不混其他改動。

## 需要使用者決策
1. 這一批要不要把 `Assets/OffMeshLinkScene.unity` 排除在主線提交外？
2. 本批驗收門檻是否維持「`warning=0` + `error=0`」？
3. 匯入 MVP 這批要不要先提交，再做下一輪 UX/語義擴充？

## 重要資源
- `AGENTS.md`
- `GraphToolkitSpec.md`
- `PlanningWithFiles/20260206/phase5_phase6_unified/task_plan.md`
- `PlanningWithFiles/20260208/graphtoolkit_repeated_issues_root_cause/findings.md`
- `Assets/Scene/Test.unity`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartGraph.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`
- `Assets/Editor/Tests/InkFlowChartImportTests.cs`
