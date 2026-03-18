# 任務計畫：Batch 1 option schema seam

## 目標
把 `InkFlowChartNodes.cs` 內剩下的 `option key / 顯示欄位 / 預設值` 設定抽成獨立的 Editor-only schema 檔，讓 `InkFlowChartNodes.cs` 更接近純節點殼，同時守住現有 GraphToolkit 的 smoke / export / import / round-trip 測試。

## 當前階段
Phase 3

## 階段

### Phase 1：確認剩餘耦合邊界
- [x] 重新盤點 `InkFlowNodeSchema` 的剩餘責任
- [x] 確認只切 Editor option schema，不碰 Runtime 與 canonical core
- [x] 確認 `Nodes / Exporter / Importer` 的改接範圍
- **Status:** complete

### Phase 2：切出 Editor-only option schema
- [x] 新增獨立 schema 檔
- [x] 將 `InkFlowChartNodes.cs` 改接新 schema
- [x] 將 `InkFlowChartExporter.cs` 與 `InkFlowChartImporter.cs` 改接新 schema
- **Status:** complete

### Phase 3：補驗證與收斂
- [x] 補最小測試或確認既有測試已足夠守住改動
- [x] 跑 Batch 1 最小 gate
- [x] 整理結果與暫存輸出
- **Status:** complete

## 關鍵問題
1. `InkFlowNodeSchema` 現在剩下的內容，是否都屬於 Editor 表單責任？
2. 這一刀要不要順便拆 choice/condition 的顯示 helper，還是先只切一層檔案邊界？
3. 哪一組測試最能證明這次只是切 seam，沒有改壞匯入匯出與 round-trip？

## 已做決策
| 決策 | 原因 |
|------|------|
| 本輪只切 Editor option schema | 這是 `InkFlowChartNodes.cs` 剩下最明顯的混合責任 |
| 先不改 node 類型與 port 語意 | 這一層已在 Batch 1 第一刀切出去，這輪不重複動 |
| 優先沿用既有測試 gate | 這輪是重構，不是功能擴張，先守既有閉環最務實 |

## 錯誤紀錄
| Error | Attempt | Resolution |
|-------|---------|------------|
| 完整 EditMode gate 會順手改動 `Assets/OffMeshLinkScene.unity` 的 scene fileID | 1 | 確認是外部套件測試副作用後，將該 scene restore，只保留本輪真正需要的作者工具變更 |

## 備註
- 若發現 schema 內容仍混到 canonical/current projection 語意，不要硬留在 Editor-only 檔內，應再次切分。
- 本輪實作已將 `InkFlowNodeSchema` 正式改名並抽成 `InkFlowNodeOptionSchema.cs`。
- `Batch1OptionSchemaGateResults.xml` 顯示 `OpsidanosInk.EditModeTests.dll` 為 `37/37 passed`，其中 `InkFlowChartImportTests` 為 `9/9 passed`，`InkFlowChartRoundTripTests` 為 `3/3 passed`。
