# 調查發現：Graph Toolkit 反覆問題根因

## 2026-02-08 初始
- 已建立調查任務，準備先做歷史任務盤點，再做 commit 前後比對。

## 2026-02-08 基線盤點
- 目前分支：`arcumit/CodexInk`，相對遠端 `ahead 9`。
- 目前工作樹已有 Graph Toolkit 相關未提交修改：
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartGraph.cs`
  - `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`（新檔）
  - `Assets/Editor/Tests/InkFlowChartExportTests.cs`
  - `Assets/Editor/Tests/InkFlowChartGraphSmokeTests.cs`
  - `Assets/Editor/Tests/InkFlowChartImportTests.cs`（新檔）
- 最近 commit（最新在上）：
  - `84cfd67` feat: 新增 Graph Toolkit 匯出 MVP 與測試
  - `5885d3c` refactor: 統一 OpsidanosInk 工具入口到 Tools 路徑
  - `6117234` refactor: 移除 IMGUI Flow Chart 並穩定 Graph Toolkit smoke test
  - `b724138` chore: 安裝 Graph Toolkit 套件
## 2026-02-08 歷史任務全域掃描（PlanningWithFiles）
- 主要 Graph Toolkit 歷史集中在 `PlanningWithFiles/20260206/phase5_phase6_unified/*`。
- 歷史中「重複被修過」的高頻問題包含：
  - `CS0234: Unity.GraphToolkit` 命名空間錯誤（最後靠 asmdef 補引用修正）。
  - 建圖流程出現 `same path as an existing asset` warning（來源定位到 package `GraphObjectFactory.RegisterNewGraphObject`）。
  - 匯入/匯出測試暫存資料夾清理與 Ink 編譯時機競速（`DirectoryNotFoundException`、cleanup 警示）。
  - `Selection.activeObject` 指派型別錯誤（`CS0029/CS1503`）。
  - 匯出測試偶發「找不到 Graph 資產」（需用 `GraphDatabase.GetGraphAssetPath(graph)` 取實際路徑）。
- 歷史紀錄顯示最新 Graph 匯出 MVP 提交為 `84cfd67`，與使用者敘述「上一個 commit 前沒有此批問題」一致，下一步需確認這些修正是否在此 commit 被遺漏或覆蓋。
## 2026-02-08 目前工作樹差異（相對 `84cfd67`）
- `InkFlowChartGraph.cs`：
  - 建圖入口從 `PromptInProjectBrowserToCreateNewAsset` 改成 `GraphDatabase.CreateGraph` + GUID 路徑 + `Selection/Ping`。
  - 新增匯入選單入口與路徑判斷共用。
  - 新增 `ResolveCreateTargetFolder` 與 `EnsureDefaultGraphFolderExists`。
- `InkFlowChartExporter.cs`：新增 fallback `LoadGraphForImporter` 失敗時再 `LoadGraph`。
- `InkFlowChartExportTests.cs`：
  - `SetUp` 不再批次清理 `TmpGraphToolkitExportTests*`。
  - fixture 測試由「複製 fixture 檔」改為「每次建圖 + 寫節點」。
  - `DeleteTempFolderIfExists` 改為只刪固定資料夾。
- `InkFlowChartGraphSmokeTests.cs`：刪除前綴批次清理，改只刪固定資料夾。
- 新增 `InkFlowChartImporter.cs`、`InkFlowChartImportTests.cs`（尚未提交）。

## 2026-02-08 Unity Console 證據
- 目前 `warning` 主要為同一類：
  - `A new asset is created at the same path as an existing asset. The existing asset will be unloaded.`
- 堆疊定位：
  - package：`GraphObjectFactory.RegisterNewGraphObject`（`Library/PackageCache/com.unity.graphtoolkit.../GraphObjectFactory.cs:384`）
  - 專案呼叫點：`Assets/Editor/FlowChart/GraphToolkit/InkFlowChartGraph.cs:34`（`CreateGraphAsset`）
- 關鍵觀察：即使檔名含 GUID（每次不同），仍持續出現同類 warning，表示問題不只「路徑字串重複」，更可能是 Graph Toolkit 內部註冊/生命週期時序衝突。

## 2026-02-08 根因定位：`same path` warning 其實不是「路徑重複」
- 直接結論：
  - 這則 warning 的真正成因是 **Graph Toolkit 套件在匯入流程中「先載入了一份 GraphObject」**，導致後續 `RegisterNewGraphObject()` 一定會偵測到「同 GUID 已經有 GraphObject」而印出 warning。
  - 所以「改成 GUID 檔名」只能避免我們自己重複用同一路徑，但 **無法根治** 這則 warning。

- 證據鏈（從套件原始碼直接對上 warning 的觸發條件）：
  1) `GraphDatabase.CreateGraph<T>(assetPath)` 會走到 `GraphObject.AttachToAssetFile()` → `DoCreateAssetFile()`，在 foreign asset 分支會呼叫 `AssetDatabase.ImportAsset(filePath)` 後，再呼叫 `GraphObjectFactory.RegisterNewGraphObject(this, guid)`。
     - 關鍵檔案：`Library/PackageCache/com.unity.graphtoolkit@*/GraphToolkitPublic/Graph/GraphDatabase.cs`
     - 關鍵檔案：`Library/PackageCache/com.unity.graphtoolkit@*/GraphToolkitEditor/Model/GraphObject.cs`
     - 關鍵檔案：`Library/PackageCache/com.unity.graphtoolkit@*/GraphToolkitEditor/Model/GraphObjectFactory.cs`
  2) 但 `ImportAsset` 的同一輪匯入回呼中，Graph Toolkit 自己的 `WindowAssetPostprocessingWatcher.OnPostprocessAllAssets(...)` 會遍歷 `importedAssets`，對每個路徑呼叫：
     - `GraphObject.LoadGraphObjectAtPath(assetFilePath)`
     - 這個呼叫會把「剛匯入的那份 graph」先放進 `s_LoadedGraphObjects` 快取（也就是：先「載入一份」在記憶體裡）。
     - 關鍵檔案：`Library/PackageCache/com.unity.graphtoolkit@*/GraphToolkitEditor/UI/Windows/WindowAssetPostprocessingWatcher.cs`
  3) 因此回到 `RegisterNewGraphObject(this, guid)` 時：
     - `existingAsset = GetLoadedAsset(guid)` 一定拿得到「剛剛在 postprocess 被 LoadGraphObjectAtPath 先載入的那份」
     - 且 `existingAsset != this`（因為 `this` 是建立流程中臨時建立、用來寫檔/匯入的那個 GraphObject）
     - 於是命中 `GraphObjectFactory.RegisterNewGraphObject` 內的 warning 條件：
       - `existingAsset != null && existingAsset != graphObject`
       - 進而輸出：`A new asset is created at the same path as an existing asset...`

- 這個根因的「最重要含意」：
  - 這則 warning 是 Graph Toolkit 0.4.x 在「建立 foreign graph 資產」流程中，非常容易（甚至可視為固定）出現的 Console 噪音。
  - 若專案驗收硬性要求 `warning=0`，就必須在我們自己的建立流程/測試流程 **額外做處理**（例如：測試用 `LogAssert`，或只在極短區段關閉 logger），否則就會反覆出現。

## 2026-02-08 反覆問題根因總表（5 類）
| 類別 | 常見現象 | 共同根因 | 最直接證據 | 最小可行預防法（不代表要改，僅供決策） |
|---|---|---|---|---|
| GTK `same path` warning | `A new asset is created at the same path...` | GTK 在 `ImportAsset` 回呼裡先 `LoadGraphObjectAtPath`，之後 `RegisterNewGraphObject` 必定看到 `existingAsset` | `WindowAssetPostprocessingWatcher` + `GraphObjectFactory.RegisterNewGraphObject` | 測試層用 `LogAssert` 或在「建圖瞬間」短暫關 logger；否則只能接受噪音或 patch 套件 |
| `CS0234: Unity.GraphToolkit` | 找不到 `Unity.GraphToolkit` 命名空間 | asmdef 未引用 GTK 三個 Editor assemblies | `GraphToolkitSpec.md` §3.2 + 歷史紀錄（20260206） | 在 `*.asmdef` references 明確加入 `Unity.GraphToolkit.*.Editor` |
| 暫存清理/編譯競速 | `Ink file ... was not found after compilation`、`DirectoryNotFoundException`、`Files generated by test without cleanup` | Ink 編譯/資產匯入是非同步時序；測試太早刪檔/刪資料夾或沒同步刷新 | `PlanningWithFiles/20260206/phase5_phase6_unified/*` 多次修正紀錄 | 收尾用 `UnityTearDown` 等待 Ink 編譯結束後再清理；刪除用 `AssetDatabase.DeleteAsset` + 同步刷新 |
| `Selection.activeObject` 型別錯 | `CS0029/CS1503` | 把 `Graph`（非 `UnityEngine.Object`）當成資產物件去選取/ Ping | 歷史紀錄（20260206）+ `InkFlowChartGraph.cs` 差異 | 用 `AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path)` 再 `Selection/Ping` |
| 匯出偶發找不到 Graph | 匯出器/測試偶發回報「找不到 Graph 資產」 | Graph 物件的「實際資產路徑」不能用輸入字串假設；需要查 GTK 回傳的實際 path | `PlanningWithFiles/20260206/phase5_phase6_unified/findings.md` | 一律用 `GraphDatabase.GetGraphAssetPath(graph)` 取得實際路徑再做後續處理 |
## 2026-02-08 規格對照（GraphToolkitSpec）
- 規格允許兩種建圖路徑：
  - 互動式：`GraphDatabase.PromptInProjectBrowserToCreateNewAsset<T>`（`GraphToolkitSpec.md` 第 406 行）
  - 程式式：`GraphDatabase.CreateGraph<T>(assetPath)`（`GraphToolkitSpec.md` 第 411 行）
- 規格並未保證 `CreateGraph` 不會吐 warning；而目前 Console 證據顯示 warning 來自 package 內 `GraphObjectFactory.RegisterNewGraphObject`，屬套件內部行為。

## 2026-02-08 `84cfd67` 與目前工作樹關鍵差異（會影響問題表現）
- `84cfd67` 的 `InkFlowChartGraph.CreateGraphAsset` 仍使用 `PromptInProjectBrowserToCreateNewAsset`。
- 目前工作樹改為直接 `GraphDatabase.CreateGraph`（`InkFlowChartGraph.cs:34`），且這個呼叫點已被 Console stack 反覆命中 warning。
- `84cfd67` 尚未包含 `InkFlowChartImporter.cs` / `InkFlowChartImportTests.cs`；這兩檔目前為未提交新增檔，等同把新流程與新清理策略放在「未提交狀態」疊代。

## 2026-02-08 測試執行狀態
- 以 Unity MCP 跑整組 EditMode 測試時，工作卡在 `Unity.AI.Navigation` 套件測試（progress 124/231，`editor_unfocused`）。
- 因為卡在外部套件測試，本輪未取得完整 `OpsidanosInk.EditModeTests` 最新 pass/fail 清單；但 Console warning 證據已可直接定位一條主因路徑。
## 2026-02-08 根因分群（現象 → 根因）
1. 現象：建立新圖反覆出現 `same path as an existing asset`
   - 根因：建立流程切到 `GraphDatabase.CreateGraph` 後，觸發 Graph Toolkit package 內部註冊路徑（`GraphObjectFactory.RegisterNewGraphObject`）的既知 warning 行為；此 warning 並非單純靠 GUID 檔名可消失。
   - 證據：`InkFlowChartGraph.cs` 第 34 行 + Console stack 定位到 package `GraphObjectFactory.cs:384`。

2. 現象：同類問題反覆回來（修過又出現）
   - 根因：歷史解法多為「局部 workaround」（如測試內暫時關 logger、改清理策略），但未形成單一路徑規範；當建圖 API、測試清理策略或命名規則再改一次，就容易把舊風險帶回來。
   - 證據：`20260206` 歷史檔記錄同一 warning 曾連續 4 次嘗試；並記錄「GUID 路徑仍失敗」。

3. 現象：匯入/匯出測試與資產清理問題反覆出現
   - 根因：測試暫存策略曾在「固定資料夾」「前綴批次」「單檔前綴」之間多次切換，且 `.gitignore`、清理函式、檔名規則未完全一致，導致殘留/競速風險反覆。
   - 證據：歷史檔明確記錄需採前綴批次清理；目前新匯入測試改為 `Assets/Editor/TmpGraphToolkitImport_...` 單檔前綴，策略與舊規則分歧。

4. 現象：你感覺「上一個 commit 前沒這麼多問題，現在都在」
   - 根因：`84cfd67` 後存在一批未提交的 Graph Toolkit 變更（包含新檔與核心流程調整），目前實際執行的是「commit 內容 + 未提交疊代」，不是單純 `84cfd67`。
   - 證據：`git status` 顯示 5 個修改檔 + 4 個未追蹤新檔都集中在 Graph Toolkit/測試。
## 2026-02-08 外部測試副作用
- 在整組 EditMode 測試卡住期間，工作樹新增 `Assets/TempLinkUpgrade/`（含 `Test_Links_Created_With_2_0_0.unity`），推測由外部套件測試（Unity.AI.Navigation）產生。
- 此路徑不屬 Graph Toolkit 功能程式，但會污染工作樹判讀；後續若要比對 Graph Toolkit 問題，需先排除這類外部測試產物。
