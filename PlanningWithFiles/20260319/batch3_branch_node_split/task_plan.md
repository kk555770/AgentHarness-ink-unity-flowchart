# 任務計畫：Batch 3 branch node 拆檔

## 目標
在 `InkFlowChartEditorCommands` 這刀完成後，繼續薄化 `GraphToolkit shell`，把 `InkFlowChartNodes.cs` 內剩餘的支援型別與分支節點拆到獨立檔，讓主節點檔更接近「主線節點本體集合」。

## 當前階段
Phase 1

## 階段

### Phase 1：確認拆檔邊界
- [x] 重讀目前 `InkFlowChartNodes.cs` 與引用點
- [x] 確認這刀不碰 `Exporter / Importer / StyleBootstrap`
- [x] 決定新檔案切法
- **Status:** complete

### Phase 2：落地拆檔
- [x] 新增 `InkFlowActionPayload.cs`
- [x] 新增 `InkFlowChartBranchNodes.cs`
- [x] 更新 `InkFlowChartNodes.cs`
- **Status:** complete

### Phase 3：完整驗證與整理
- [x] 跑 EditMode gate
- [x] 跑 PlayMode gate
- [x] 清理測試暫存與場景噪音
- [x] 整理 findings / progress
- **Status:** complete

## 關鍵問題
1. `InkFlowChartNodes.cs` 現在還有哪些責任最適合拆成獨立檔？
2. 這刀是否能只做檔案邊界整理，而不改任何 node 行為？
3. 哪些既有測試最能證明這次拆檔沒有破壞 `GraphToolkit` 與玩家閉環？

## 已做決策
| 決策 | 原因 |
|------|------|
| 另開新的 `PlanningWithFiles` 記錄 | 這是 `EditorCommands` 之後的新一刀，不和前一輪混在一起 |
| 先拆 `InkFlowActionPayload` 與 branch node | 這是 `InkFlowChartNodes.cs` 裡剩餘最像「還混在一起的小盒子」的部分 |
| 不碰 `InkFlowChartGraphStyleBootstrap.cs` | 前面 planning 已寫明這批先不要動樣式與 UI 視覺殼 |

## 錯誤紀錄
| Error | Attempt | Resolution |
|-------|---------|------------|

## 備註
- 使用者已同意這一刀，可以正式改檔。
- 這輪要遵守 repo `AGENTS.md`：修改 `.cs` 必須補上 `// ===== 變更開始 =====` 註解區塊。
