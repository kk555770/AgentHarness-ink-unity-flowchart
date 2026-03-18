# 調查發現

## 需求
- 使用者已同意繼續往下做。
- 本輪目標是開始 Batch 3，讓 GraphToolkit 更明確退成 baseline shell。

## 研究發現
- 文件與本地盤點都指向同一件事：Batch 3 第一刀不該再碰 Batch 1 / 2 已抽出的 projection / importer / exporter seam，而是要讓 `GraphToolkit shell` 退成 baseline。
- `InkFlowChartGraph.cs` 現在同時握有：
  - `.inkfc` graph asset 型別
  - Unity Editor `MenuItem` 入口
  - 選取資產路徑解析
  - 建圖目錄決策
  - `OnGraphChanged` 的 shell 驗證入口
- 這代表 `InkFlowChartGraph.cs` 不只是圖資產殼，也還像工具控制器；這正是 Batch 3 最適合先切的地方。
- `OpsidanosInk.FlowChartEditor.asmdef` 目前已經只依賴 `OpsidanosInk.CanonicalGraph` 與 GraphToolkit editor 組件，本身沒有明顯錯誤方向；所以 Batch 3 第一刀不一定需要先改 asmdef，先把 `Graph` 類別薄化風險更低。
- 最小可行切點是：
  - 把 `MenuItem` 與資產路徑 helper 從 `InkFlowChartGraph.cs` 搬到獨立 editor command 檔
  - 把 `OnGraphChanged` 的 shell 驗證橋接抽到獨立 helper
  - 讓 `InkFlowChartGraph` 只保留 graph asset 身分與最薄的 override 入口
- `InkFlowChartGraphSmokeTests.cs` 沒有直接綁 `InkFlowChartGraph.cs` 內的靜態選單方法，只驗 `.inkfc` 可建立/載入；因此搬走選單入口的回歸風險相對低。
- 第一刀完成後，新的切法變成：
  - `InkFlowChartEditorCommands.cs`：Unity Editor 選單入口、選取資產路徑解析、建立目標資料夾
  - `InkFlowChartGraphShellValidator.cs`：GraphToolkit shell 層的 start node 驗證橋接
  - `InkFlowChartGraph.cs`：只保留 `.inkfc` graph asset 殼與最薄的 `OnGraphChanged`
- 這代表 `InkFlowChartGraph.cs` 已不再兼做工具控制器，GraphToolkit shell 的角色邊界比原本清楚很多。
- 目前 `InkFlowChartNodes.cs` 還混著三種責任：
  - node 本體定義
  - `[UseWithGraph(typeof(InkFlowChartGraph))]` 的 graph-specific 可見節點註冊
  - option 讀值 / branch output label 小工具
- 下一刀若把 graph-specific 可見註冊與 helper 抽出去，`InkFlowChartNodes.cs` 就會更像「節點積木本體」，這很符合 Batch 3 要讓 GraphToolkit shell 退成 baseline 的方向。
- 第二刀完成後，新的切法變成：
  - `InkFlowChartVisibleNodes.cs`：graph-specific 可見節點註冊
  - `InkFlowChartNodeShellUtility.cs`：node option 讀值與 branch output label 小工具
  - `InkFlowChartNodes.cs`：節點本體與 port / option 定義
- 這代表 `InkFlowChartNodes.cs` 已經不像先前那麼像「節點本體 + 註冊 + 小工具」三合一抽屜，GraphToolkit baseline shell 的邊界又再清楚一點。

## 技術判斷
| Decision | Rationale |
|----------|-----------|
| Batch 3 第一刀先薄化 `InkFlowChartGraph.cs` | 這是目前 shell 最混、但風險又比碰 Runtime / 樣式低的地方 |
| 先不改 `InkFlowChartGraphStyleBootstrap.cs` 與 `.uss` | 文件已明示這批先不要動樣式與 UI 視覺殼 |
| asmdef 先觀察，不硬改 | 現況依賴方向已大致正確，先改類別責任比先改組件邊界更穩 |
| Batch 3 下一刀優先薄化 `InkFlowChartNodes.cs` | 這刀不改 projection / runtime，只做 graph-specific 註冊與 helper 抽離，回歸風險低 |

## 遇到的問題
| Issue | Resolution |
|-------|------------|

## 驗證結果
- 完整 EditMode gate 通過：
  - `OpsidanosInk.EditModeTests.dll`：`47/47 passed`
  - `InkFlowChartGraphSmokeTests`：`1/1 passed`
  - `InkFlowChartExportTests`：`6/6 passed`
  - `InkFlowChartImportTests`：`9/9 passed`
  - `InkFlowChartRoundTripTests`：`3/3 passed`
- PlayMode gate 也通過：
  - `OpsidanosInk.PlayModeTests.dll`：`17/17 passed`
  - `OpsidanosInkPlayModeTests`：`7/7 passed`
  - `OpsidanosInkPlayModeUiClickTests`：`10/10 passed`
- 第二刀完成後，再跑一次完整 gate 也維持綠燈：
  - EditMode：`261 passed / 0 failed / 4 skipped`
  - PlayMode：`130 passed / 0 failed / 91 skipped`

## 參考資源
- `Documentation/AuthoringPhase1ImplementationProposal.md`
- `Documentation/AuthoringPhase1ImplementationPlan.md`
- `Documentation/AuthoringRefactorBoundaries.md`
