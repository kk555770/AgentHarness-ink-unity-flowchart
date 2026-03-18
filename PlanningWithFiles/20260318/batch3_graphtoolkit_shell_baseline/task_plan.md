# 任務計畫：Batch 3 GraphToolkit shell baseline

## 目標
開始落地 Batch 3：讓 GraphToolkit editor 進一步退成 baseline shell，減少它同時保管 editor 殼、語意入口、projection 入口與 adapter 細節的混合責任。

## 當前階段
Phase 4

## 階段

### Phase 1：確認 Batch 3 最小切點
- [x] 重讀 Batch 3 文件與邊界說明
- [x] 盤點 `GraphToolkit` shell 目前還握有哪些責任
- [x] 決定第一刀先切哪個 editor shell seam
- **Status:** complete

### Phase 2：實作 Batch 3 第一刀
- [x] 調整 editor shell / asmdef / adapter 邊界
- [x] 補最小測試或更新既有 gate
- **Status:** complete

### Phase 3：完整驗證與整理
- [x] 跑完整 EditMode gate
- [x] 清理暫存輸出與測試噪音
- [x] 整理 findings / progress / commit 準備
- **Status:** complete

### Phase 4：薄化 node shell 與 graph 綁定
- [x] 將 graph-specific 可見節點註冊從 `InkFlowChartNodes.cs` 抽離
- [x] 將 node option / branch label 小工具從 `InkFlowChartNodes.cs` 抽離
- [x] 跑完整 EditMode 與必要 PlayMode gate
- [x] 清理暫存輸出並整理結果
- **Status:** complete

## 關鍵問題
1. Batch 3 第一刀最值得先切的是 asmdef 邊界、Graph 類別入口，還是 node / shell 類型註冊？
2. 哪些責任現在仍明顯屬於 GraphToolkit shell，不該再和 shared core / projection service 混在一起？
3. 這一刀怎麼切，才能讓後面 Web-first authoring 更容易接上，而不是只是換一層殼？

## 已做決策
| 決策 | 原因 |
|------|------|
| 先建立新的 Batch 3 planning 記錄 | 這輪要正式進入下一批，不能混用 Batch 2 記錄 |
| 先平行盤點文件與 editor shell | 一邊看北極星，一邊看現況，才不會只憑感覺動刀 |
| Batch 3 第一刀先薄化 `InkFlowChartGraph.cs` | 這是目前最混、但又能低風險驗證 shell baseline 方向的切點 |
| 第一刀先不硬改 asmdef | 現況依賴方向已大致正確，先切類別責任比先動組件邊界更穩 |
| Batch 3 下一刀先薄化 `InkFlowChartNodes.cs` | 這個檔仍混著節點本體、graph-specific 可見註冊與 option/branch helper，低風險且符合 shell baseline 方向 |

## 錯誤紀錄
| Error | Attempt | Resolution |
|-------|---------|------------|

## 備註
- 使用者已明確授權我主動使用 multi-agent / sub-agent；只要子任務沒有寫入衝突，就優先平行拆解。
- Batch 3 第一刀完整 EditMode gate 已通過：
  - `OpsidanosInk.EditModeTests.dll`：`47/47 passed`
  - `InkFlowChartGraphSmokeTests`：`1/1 passed`
  - `InkFlowChartExportTests`：`6/6 passed`
  - `InkFlowChartImportTests`：`9/9 passed`
  - `InkFlowChartRoundTripTests`：`3/3 passed`
- Batch 3 第一刀 PlayMode gate 也已通過：
  - `OpsidanosInk.PlayModeTests.dll`：`17/17 passed`
  - `OpsidanosInkPlayModeTests`：`7/7 passed`
  - `OpsidanosInkPlayModeUiClickTests`：`10/10 passed`
- Batch 3 第二刀完整 EditMode gate 已通過：
  - `OpsidanosInk.EditModeTests.dll`：`47/47 passed`
  - `InkFlowChartGraphSmokeTests`：`1/1 passed`
  - `InkFlowChartExportTests`：`6/6 passed`
  - `InkFlowChartImportTests`：`9/9 passed`
  - `InkFlowChartRoundTripTests`：`3/3 passed`
- Batch 3 第二刀 PlayMode gate 也已通過：
  - `OpsidanosInk.PlayModeTests.dll`：`17/17 passed`
  - `OpsidanosInkPlayModeTests`：`7/7 passed`
  - `OpsidanosInkPlayModeUiClickTests`：`10/10 passed`
