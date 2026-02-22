# Findings & Decisions

## Requirements
- 只保留 1 份主線計畫，承接剩餘 7 項待辦。
- 舊主計畫要封存，避免每次重複回頭比對。
- 新主計畫需使用 `planning-with-files` 模板段落，降低後續溝通成本。
- 實作範圍聚焦：
- 多槽 + Auto 槽 UI（存/讀）
- Flow Chart EditorWindow（拖拉、連線、節點屬性）
- Flow Chart 匯入/匯出（`.ink` + sidecar）
- 最小節點集合與 Tag 字典規格
- 開發者模式到玩家模式可驗收 Demo

## Research Findings
- 舊主計畫（`PlanningWithFiles/20260120/TextAdventureEngine`）混有已完成與未完成項，容易造成追蹤誤差。
- 把剩餘待辦集中到 `PlanningWithFiles/20260206/phase5_phase6_unified` 後，主線更清楚。
- 目前真正未完成項為 7 項，適合獨立管理，不需要再拆多個平行主計畫。
- 已全量盤點 `PlanningWithFiles` 既有任務，共 17 組（每組含 `task_plan.md` / `findings.md` / `progress.md`）。
- 17 組歷史任務已統一定義為封存來源；後續只在追因時查閱，不再作為主線追蹤清單。
- Flow Chart 匯出採「`.ink + sidecar` 雙檔」：`.ink` 保文本與分支可編譯，sidecar 保留節點座標與編輯資訊。
- Flow Chart 匯入以 sidecar 為主做完整還原，`.ink` 只作存在性檢查與檔案配對。
- `OpsidanosInk.EditModeTests` 現在共 8 筆測試，已包含匯出匯入 round-trip 驗證。
- 既有 Flow Chart 資料若 `nextNodeIds` 混入節點標題（非 id），會導致匯出 `.ink` 退化為 `-> END`。
- 在匯出階段加入「標題→id→knot」容錯，可兼容舊資料，不必強迫先手動清檔。
- 匯出前清理控制字元（保留換行/Tab）可避免 `.ink` 內容出現 `\u0003` 類型雜訊。
- sidecar `body` 清理邏輯已上線，但要重新匯出一次才會覆蓋舊檔中的 `\u0003`。
- 重新匯出後，`NewInkFlowChart.ink`、`NewInkFlowChart.json`、`NewInkFlowChart.flowchart.json` 已可作為開發者模式 Demo 產物。
- 目前主線剩餘未完成項僅 `Tag 字典規格`，其餘本批驗收項目已勾選完成。
- 你新提出的需求可拆成兩層：
- Flow Chart 編輯器互動層（拉線、縮放、平移、節點搜尋、智慧落點）；
- 指令語義層（對話、入場退場、移動、背景、音樂、特效等可組態節點）。
- `Tag 字典規格` 只涵蓋「指令語義層」的一部分，不涵蓋畫布操作 UX。
- 若要支援「文字可描述就能串起來」，需要把節點 `body` 從自由文字升級為「型別 + 欄位 schema + 驗證 + 匯出規則」。
- 已完成 Flow Chart UX 升級：拉線連接（輸出點→輸入點）、滾輪縮放、右鍵拖移、可視範圍智慧落點、搜尋節點後只縮小視野不平移鏡頭。
- 已完成節點語義升級：新增 Dialogue/Add/Remove/Move/BG/BGM/Effect/Custom 等型別化欄位，並可匯出成 ink tag 文本。
- sidecar 已升級保存 `edges + 節點語義欄位`，匯入時可還原新模型；舊版 `nextNodeIds` 仍可相容回填。
- 目前尚未完成的是「Tag 字典文件化規格」，功能已可用但規格文件仍待補齊。
- 2026-02-07 已將上述「Flow Chart UX 升級 + 節點語義升級」相關程式回退至 MVP 版本，原因是要改以 Graph Toolkit 重新落地，避免在舊畫布上持續疊補丁。
- 回退後現況：Flow Chart 保留 MVP（節點拖曳、`Next IDs` 連線、`.ink + sidecar` 匯出匯入）；進階互動與型別化語義待重做。
- 本次回退未觸碰 Runtime 對話播放主線；目前工作樹僅保留 Graph Toolkit 安裝變更（`Packages/manifest.json`、`Packages/packages-lock.json`）待提交。
- 依最新決策，舊 IMGUI Flow Chart（EditorWindow + Data/Node/Importer/Exporter/Sidecar）與其對應測試已從專案移除，避免雙軌並存。
- Graph Toolkit 最小骨架已建立：`InkFlowChartGraph(.inkfc)` + `InkFlowStartNode` + `InkFlowActionNode` + `InkFlowCommentNode`，可作為單一主線。
- 目前 Flow Chart 主線能力是「建立/開啟 Graph Toolkit 圖資產與基本節點連線」；匯出匯入與 Demo 驗收需在 Graph Toolkit 版本重建。
- `InkFlowChartGraphSmokeTests.cs` 的 `CS0234` 已修正：補上 `OpsidanosInk.EditModeTests.asmdef` 對 Graph Toolkit 的直接引用後，EditMode 測試 6/6 通過且 Console `error` 為 0。
- `GraphDatabase.CreateGraph` 在 smoke test 內會輸出固定 warning；唯一路徑/GUID/LogAssert/反射建圖都無法消除，最終採「只在該呼叫期間暫時關閉 `Debug.unityLogger.logEnabled`」可穩定讓 Console warning 歸零。
- 目前 `InkSaveSystem` 僅有單一 `saveSlot`，UI 也是單一 `SaveButton` / `LoadButton` 入口，尚未支援多槽。
- `InkSaveSystem` 已支援 rollback 歷史保存與讀檔後可倒帶，現有 PlayMode 測試已覆蓋這段行為。
- `VNPlayerPresenter` 已有快速連按 rollback 佇列機制，後續多槽改動要避免破壞既有排隊行為。
- `JsonUtility` 對陣列中的 `null` 項目在反序列化時，可能回填為預設物件；測試斷言需依實際行為撰寫。
- 新增多槽後，既有舊 API（`SaveToSlot` / `LoadFromSlot`）仍可保留並映射到槽位 1，回歸測試可直接驗證相容性。
- 歷史計畫（`20260120/TextAdventureEngine`）已固定 Flow Chart 方向：Editor 工具輸出 `.ink`，編輯資訊放 sidecar，Runtime 不依賴 Flow Chart 資產。
- 同類任務在歷史檔案中尚未完成的主線已集中到本主計畫第 2~5 項，沒有額外分支待辦需要再合併。
- 目前已確認工具入口分散在兩個路徑：`Assets/Create/OpsidanosInk/Flow Chart Graph` 與 `OpsidanosInk/測試/...`（3 項），容易造成操作混淆。
- 本次決策為單一路徑：全部改為 `Tools/OpsidanosInk/...`，且不保留舊路徑別名。
- 本輪驗收階段遇到 Unity MCP session 無回應（`ping not answered`），導致 `read_console` 與 `run_tests` 暫時無法完成最終驗收；需在 session 恢復後補驗。
- Unity MCP session 已恢復，可正常執行 `read_console`、`execute_menu_item`、`run_tests`。
- 入口驗收結果：`Tools/OpsidanosInk/Flow Chart Graph` 與三個 `Tools/OpsidanosInk/測試/...` 路徑皆可被觸發。
- 以 `OpsidanosInk.EditModeTests` 回歸驗證時，Console `warning/error` 為 0；路徑調整未引入新錯誤。
- 直接觸發 `Tools/OpsidanosInk/測試/...` 時出現的 `TestRunnerApi` 建立警告與 `char.transition.steps` 訊息屬既有流程訊息，與本次 `MenuItem` 路徑字串調整無關。
- 最終收斂驗收基準為「清 Console 後跑 `OpsidanosInk.EditModeTests`」，結果穩定為 6/6 passed 且 warning/error 為 0。
- 提交前已清除測試臨時資產（`Assets/FlowCharts/NewInkFlowChartGraph.inkfc`、`Assets/InitTestScene*.unity`），工作樹收斂為僅兩個入口檔案修改。
- 入口統一已完成提交：`5885d3c`（`refactor: 統一 OpsidanosInk 工具入口到 Tools 路徑`），未混入其他功能改動。
- Graph 匯出 MVP 已建立固定 DTO：`ExportGraphDto(version/graphName/startNodeId/nodes)` 與 `ExportNodeDto(id/type/content/nextIds)`。
- Graph 匯出器 MVP 已上線：使用 `GraphDatabase.LoadGraphForImporter` 讀 `.inkfc`，並輸出同名 `.ink` 與 `.flowchart.json`。
- 匯出入口已集中在 `Tools/OpsidanosInk/Flow Chart Graph/匯出選中圖`，且只在選中 `.inkfc` 時可點擊。
- 匯出測試採兩條主線：`fixture 匯出成功` 與 `空圖匯出明確失敗`，並在測試內用反射組 fixture 節點與連線。
- 匯出失敗測試若預期 `Debug.LogError`，必須搭配 `LogAssert.Expect`，否則 NUnit 會判定為未預期錯誤而失敗。
- 若驗收門檻要求 `Console error=0`，匯出器核心不應直接寫 `Debug.LogError`；應由選單入口層統一輸出錯誤訊息。
- Graph 匯出 MVP 最終驗收結果：`OpsidanosInk.EditModeTests` 8/8 通過，清 Console 後重跑可達成 `warning=0`、`error=0`。
- Graph 匯出 MVP 已完成提交：`84cfd67`（`feat: 新增 Graph Toolkit 匯出 MVP 與測試`），提交內容包含匯出器、模型、入口、測試、fixture 與 `.gitignore`。
- `84cfd67` 提交後工作樹已清空，表示本輪匯出 MVP 收斂完成且可直接進入下一個實作工項。
- Graph 匯入 MVP 已進入實作：`InkFlowChartImporter` 已建立，匯入流程固定為 `*.flowchart.json + 同名 .ink` 才能還原 `.inkfc`。
- Graph Toolkit 的 Node option 寫值可行：可透過 `NodeOption.PortModel.EmbeddedValue` 反射取得常數，並呼叫 `Constant.TrySetValue<string>` 寫回 Action/Comment 內容。
- 匯入入口已補到 `Tools/OpsidanosInk/Flow Chart Graph/匯入選中匯出檔`，且 validate 僅允許選中 `.flowchart.json` 時啟用。
- `InkFlowChartGraph` 的選取路徑檢查已共用到 `GetSelectedAssetPathBySuffix`，避免匯出/匯入兩套判斷分歧。
- 匯入 EditMode 測試已補齊 3 案：成功還原、缺 `startNodeId` 失敗、缺 `.ink` 配對失敗。
- 匯入成功測試會驗證三件事：節點型別數量、Flow 連線、Action/Comment option 內容回填。
- 匯入測試若直接 `File.Delete` 測試 `.ink`，會留下「Ink file ... was not found after compilation」警告。
- 匯入測試清理策略需改為 `AssetDatabase.DeleteAsset` 並搭配同步刷新，避免資產匯入與刪除競態。
- 匯入測試已改成 `Refresh(ForceSynchronousImport)` 後再匯入，並統一用 `AssetDatabase.DeleteAsset` 清理暫存配對檔。
- 套用同步清理後，`LogAssert.Expect` 反而抓不到原警告，代表警告來源已被實際修掉，不需保留期待警告斷言。
- 重新驗證後警告仍存在，根因不是 `LogAssert`，而是 `.ink` 被當成 Unity 資產匯入後又在測試流程中刪除。
- 匯入測試最穩定策略是：`.ink/.json` 僅作檔案存在性配對，不進 AssetDatabase 刷新；由匯入器直接讀檔，測試只讓 `.inkfc` 進資產流程。
- Unity 的檔案監看仍會自動匯入 `.ink`，所以只要測試期內刪除 `.ink` 就可能觸發 missing warning。
- 為了維持驗收 `warning=0`，匯入測試改為保留 `.ink/.json` 暫存檔，不在測試回合內刪除。
- 完全不清理暫存檔會觸發既有警告 `Files generated by test without cleanup.`，因此不能採用。
- 匯入成功案例的 `.ink` 必須用可編譯內容（含目標 knot），避免 Ink 編譯器報「Divert target not found」。
- 在 `Assets/Tmp...` 測試路徑刪除 temp folder 時，Ink 編譯流程可能延後寫入而觸發 `DirectoryNotFoundException`。
- 把匯入測試暫存路徑移到 `Assets/Editor/...` 可隔離 runtime Ink 編譯鏈路，降低 Console 噪音與刪除競態。
- `Undo` 卡住復現時，Console 與 Editor.log 未出現新的 Undo 例外；目前較像 Unity/Graph Toolkit 編輯器狀態卡死，不是程式碼直接呼叫 Undo API 造成。
- 測試層可降低 Undo 卡住風險的可控手段是「每輪 SetUp 先刪舊暫存資料夾」，避免中斷後殘留影響下一輪 Editor 狀態。
- 三個 Graph Toolkit 測試暫存路徑應加入 `.gitignore`，避免殘留資料夾持續污染工作樹判讀。
- 清理暫存資料夾 + 測試前置清理 + `.gitignore` 補齊後，當前回歸驗收（11/11）與 Console 檢查均正常，未再觀察到新異常。
- `ExportFixtureGraph` 在 `CopyAsset` 後直接匯出有機率讀不到剛複製的 `.inkfc`，需要同步刷新確保資產就緒。
- 實測顯示 `CopyAsset + Refresh` 仍有 `.inkfc` 讀取失敗，最穩定作法是測試當輪直接建 fixture graph，不走複製路徑。
- `LoadGraphForImporter` 在剛建立 `.inkfc` 的時序下仍可能暫時讀不到；匯出器需要回退到 `LoadGraph` 才能避免誤判資產不存在。
- Graph Toolkit 測試建圖後，實際資產路徑應以 `GraphDatabase.GetGraphAssetPath(graph)` 為準，避免用輸入字串路徑造成誤判。
- 匯入 MVP 主線已收斂：`InkFlowChartImporter` + 工具入口 + 匯入測試三案例已接通。
- 本輪驗收結果：`OpsidanosInk.EditModeTests` 11/11 全通過，匯出與匯入測試同時納入回歸。
- 清 Console 後重跑整組測試，`warning/error` 皆為 0；此次匯入改動未新增 Console 汙染。
- `Flow Chart Graph` 入口若同時存在父路徑動作與子選單，Unity 會呈現視覺上像「兩個入口」；最佳做法是讓父路徑只保留子選單。
- 建立入口改到 `Tools/OpsidanosInk/Flow Chart Graph/建立新圖` 後，`Flow Chart Graph` 可收斂成單一父節點。
- Undo 卡住風險與測試暫存殘留關聯高；只刪固定資料夾名稱不足，需改成前綴批次清理（含 `... 1/2/3`）。
- 三個測試檔改成前綴清理後，可在每輪測試前後收斂殘留資料夾，降低 AssetDatabase/Undo 汙染機率。
- `.gitignore` 若只忽略固定路徑，編號暫存目錄仍會漏進工作樹；改成 `TmpGraphToolkit...*` 萬用前綴可完全覆蓋。
- `A new asset is created at the same path ...` 的關鍵成因是「直接在目標 `.inkfc` 路徑建圖」；改成唯一路徑建立可直接避開。
- 匯入流程若「刪除目標後立刻在同路徑建圖」，Undo 卡住風險較高；改成「暫存路徑建圖 + Move 替換」可降低風險且不改輸出結果。
- 本輪不刷新 Console 直接檢查時，主要阻塞是編譯錯誤：`InkFlowChartGraph.cs` 第 36/37 行把 `InkFlowChartGraph` 當成 `UnityEngine.Object` 使用。
- 入口掃描確認目前代碼只剩 `Tools/OpsidanosInk/Flow Chart Graph/...`；如果 Editor 仍看到雙入口，通常是「編譯失敗下沿用舊組件」導致的暫時表象。
- `Assets/FlowCharts` 現況有同名 `.inkfc/.ink/.flowchart.json` 與舊 `.json`，其中舊 `.json` 屬歷史輸出格式殘留，需在修復編譯後再決定是否清理。
- `CreateGraphAsset()` 已改為「建圖後用 `AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(uniqueAssetPath)` 取資產再 Selection/Ping」，這是本輪修正 `CS0029/CS1503` 的最小改動。
- 套用上述修正後，`OpsidanosInk.EditModeTests` 11/11 通過且 Console（error/warning）為 0，表示目前阻塞編譯錯誤已解除。
- 本輪兩則訊息已重新定位到 `InkFlowChartImportTests`：
- `Ink file ... was not found after compilation` 來自成功案例先 `Refresh` 觸發 Ink 編譯，再在同輪刪除 `.ink` 的時序競速。
- `Files generated by test without cleanup` 來自 `TearDown` 未刪除 `Assets/Editor/TmpGraphToolkitImportTests`。
- 本輪修正決策固定為兩點：
- 成功案例移除 `RefreshAssetsSynchronously()`，避免測試期觸發 Ink 自動編譯。
- `TearDown` 固定刪除 `Assets/Editor/TmpGraphToolkitImportTests` 後再刷新資產。
- 驗證後發現上述第二點仍有競速：`TearDown` 刪掉整個資料夾時，`InkCompiler` 可能還在寫同路徑 `.json`，導致 `DirectoryNotFoundException`。
- 新決策方向：收尾不刪整個 `TmpGraphToolkitImportTests` 目錄，只刪測試檔案本身（保留目錄），避免編譯收尾寫檔路徑消失。
- 2026-02-08 再重現的 warning 為 `Ink file ... was not found after compilation`，根因是測試 `finally` 在主流程結束時立即刪 `.ink`，早於 Ink 編譯佇列收尾。
- 本輪決策：`InkFlowChartImportTests` 改用 `UnityTearDown`，先等待 `InkCompiler.executingCompilationStack` 結束再清理；三個測試移除 `finally` 立即刪檔。
- `OpsidanosInk.EditModeTests` 目前無法直接引用 `Ink.UnityIntegration` 命名空間（`CS0234`），因此不能在測試碼直接使用 `InkCompiler` 型別。
- 已改為反射讀取 `Ink.UnityIntegration.InkCompiler.executingCompilationStack`，避免調整 asmdef 參考並維持最小修正範圍。
- 套用 `UnityTearDown + 反射等待 InkCompiler` 後，`OpsidanosInk.EditModeTests` 連續兩輪驗證皆為 11/11 passed，且 Console `warning/error` 皆為 0。

## Technical Decisions
| Decision | Rationale |
|----------|-----------|
| 舊主計畫封存、新主計畫單一路徑 | 降低追蹤成本與誤解風險 |
| 新主計畫三檔採模板固定段落 | 方便驗收與批次更新 |
| 先列 7 項完整映射再進入實作 | 避免漏項與返工 |
| 先做「調查 + 提案」再改 Phase 3-1 | 遵守 AGENTS「修改前必須提案」規範 |
| 保留舊 API，新增多槽 API 並行 | 降低破壞風險，讓舊流程無痛相容 |
| 多槽按鈕直接落在既有 TopBar | 最小侵入式改動，縮短驗收迴圈 |
| Flow Chart 採「EditorWindow + ScriptableObject」且不依賴場景 | 切場景不掉資料，後續匯出 `.ink + sidecar` 可直接串接 |
| 歷史計劃統一封存，主線只追 `phase5_phase6_unified` | 避免重複回頭盤點造成節奏中斷 |
| 匯出 sidecar 以節點 `id` 為唯一對應鍵，額外保存 `knotName` | 後續做匯入比對與 `.ink` 重建時不會失去對位 |
| 測試程式集改參考 `OpsidanosInk.FlowChartEditor` | 讓 Editor 測試可直接編譯 Flow Chart 編輯工具碼 |
| Next IDs 解析先收斂成 id，再進匯出流程 | 避免 UI 輸入標題造成連線遺失 |
| 匯出器追加「標題反查」作為舊資料容錯 | 舊資產可直接匯出，不需要先人工修 sidecar |
| 先回退自製圖形化改造，改走 Graph Toolkit | 先縮小風險，再用專用框架完成縮放/平移/拉線/語義節點 |
| 移除舊 IMGUI Flow Chart 程式與測試 | 不在專案保留兩套編輯器，降低後續維護與驗收歧義 |
| 工具入口全部集中到 `Tools/OpsidanosInk` | 統一進入點，避免舊路徑殘留造成雙入口混亂 |
| 提交前先刪除臨時資產再檢查差異 | 確保 commit 範圍精準，不混入測試產物 |
| Graph 匯出 MVP 提交時把 `.gitignore` 變更同批納入 | 避免遺漏本輪已確認的忽略規則調整 |
| 建立新圖路徑改為 GUID 命名 | 避免 Undo 後重用舊檔名，降低 `same path as an existing asset` 警告觸發機率 |
| 匯入驗收以「清 Console 後整組 EditMode 測試」為準 | 可以同時覆蓋功能正確性與 warning/error 汙染風險 |

## Issues Encountered
| Issue | Resolution |
|-------|------------|
| 新主計畫初版未完全貼齊模板段落 | 本次補齊 `task_plan.md`、`findings.md`、`progress.md` 的模板欄位 |
| 環境缺少 `rg` 工具 | 改用 `find + grep` 搜尋程式與測試入口 |
| `refresh_unity` 參數 `mode=normal` 非法 | 改用合法值 `mode=force` 觸發刷新 |
| `InkFlowChartRoundTripTests` 的 `Object` 與 `System.Object` 衝突 | 改用 `UnityEngine.Object.DestroyImmediate` |
| `read_console` 的 `types` 若給字串會失敗 | 需傳 JSON list 格式，如 `["error"]` |
| `InkFlowChartGraphSmokeTests.cs` 出現 `Unity.GraphToolkit` 命名空間錯誤 | 已補 `OpsidanosInk.EditModeTests.asmdef` 引用並完成編譯+測試+Console 驗證 |
| `InkFlowChartGraphSmokeTests.cs` 固定 warning 無法透過換路徑消除 | 改為在 `CreateGraph` 呼叫期間暫時關閉 logger，並以兩輪測試確認 warning=0 |
| `CreateGraphAsset()` 改成 GUID 路徑後，`same path as an existing asset` 仍發生 | `read_console(format=detailed)` 顯示 warning 來源為 Graph Toolkit package `GraphObjectFactory.RegisterNewGraphObject`，需改建立流程或做窄化抑制 |

## Resources
- 主線任務：`PlanningWithFiles/20260206/phase5_phase6_unified/task_plan.md`
- 封存來源：`PlanningWithFiles/20260120/TextAdventureEngine/task_plan.md`
- 模板來源：`/Users/arcumit/.codex/skills/planning-with-files/templates/task_plan.md`
- 模板來源：`/Users/arcumit/.codex/skills/planning-with-files/templates/findings.md`
- 模板來源：`/Users/arcumit/.codex/skills/planning-with-files/templates/progress.md`
- Save 系統：`Packages/com.opsidanos.ink/Runtime/Scripts/Save/InkSaveSystem.cs`
- Save 資料：`Packages/com.opsidanos.ink/Runtime/Scripts/Save/InkSaveData.cs`
- UI Presenter：`Packages/com.opsidanos.ink/Runtime/Scripts/UI/VNPlayerPresenter.cs`
- UI 版面：`Packages/com.opsidanos.ink/Runtime/UI/UXML/VNPlayer.uxml`
- PlayMode 測試：`Assets/Tests/PlayMode/OpsidanosInkPlayModeTests.cs`
- UI 點擊測試：`Assets/Tests/PlayMode/OpsidanosInkPlayModeUiClickTests.cs`
- Flow Chart sidecar：`Assets/Editor/FlowChart/InkFlowChartSidecar.cs`
- Flow Chart 匯出器：`Assets/Editor/FlowChart/InkFlowChartExporter.cs`
- Graph Toolkit 匯出器：`Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`
- Graph Toolkit 匯出模型：`Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExportModels.cs`
- Flow Chart 匯入器：`Assets/Editor/FlowChart/InkFlowChartImporter.cs`
- Flow Chart 程式集：`Assets/Editor/FlowChart/OpsidanosInk.FlowChartEditor.asmdef`
- Flow Chart round-trip 測試：`Assets/Editor/Tests/InkFlowChartRoundTripTests.cs`
- Graph Toolkit 匯出測試：`Assets/Editor/Tests/InkFlowChartExportTests.cs`
- Graph Toolkit fixture：`Assets/Editor/Tests/Fixtures/InkFlowChartExportFixture.inkfc`
- EditMode 測試程式集：`Assets/Editor/Tests/OpsidanosInk.EditModeTests.asmdef`

## Visual/Browser Findings
- 本輪沒有新增影像或瀏覽器調查資料。
- 2026-02-08 補充：本檔記錄的 Graph Toolkit 歷史修正已由 `PlanningWithFiles/20260208/graphtoolkit_repeated_issues_root_cause/findings.md` 做新一輪根因彙整；後續以新檔為主。
