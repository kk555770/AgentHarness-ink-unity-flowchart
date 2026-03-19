# 任務計畫：Batch 3 下一個 seam 掃描

## 目標
在 Batch 3 已完成兩刀薄化後，繼續找出 GraphToolkit shell 還剩下哪一塊最值得切，優先選擇低風險、可用既有 gate 守住的下一刀；若切點夠清楚，直接落地。

## 當前階段
Phase 1

## 階段

### Phase 1：盤點剩餘責任
- [x] 重讀 Batch 3 文件與既有 planning 記錄
- [x] 盤點 GraphToolkit shell 剩餘責任
- [x] 決定下一刀的最小切點
- **Status:** complete

### Phase 2：實作下一刀
- [x] 落地低風險 seam
- [x] 補必要測試或沿用既有 gate
- **Status:** complete

### Phase 3：完整驗證與整理
- [x] 跑完整 gate
- [x] 清理暫存輸出
- [x] 整理 findings / progress
- **Status:** complete

## 關鍵問題
1. Batch 3 在做完 graph shell 與 node shell 薄化後，還剩哪一塊責任最像 GraphToolkit baseline，而不是核心真相？
2. 下一刀要避免碰哪些已經在 Batch 1 / 2 收斂好的 seam？
3. 這一刀做完後，是否真的會讓後續 Web-first bridge 更容易接上？

## 已做決策
| 決策 | 原因 |
|------|------|
| 今天這輪另開新的 PlanningWithFiles | 這是新的掃描與可能的實作，不和 2026-03-18 的 Batch 3 記錄混在一起 |
| 先平行做文件分析與碼面盤點 | 一邊看北極星，一邊看真實殘留責任，能避免過度憑印象決定 |
| 下一刀先薄化 `InkFlowChartEditorCommands.cs` | 這個檔還同時握有 MenuItem 與資產路徑 helper，切掉 helper 後會更像純 command 入口，風險也低 |
| 繼續沿用完整 EditMode / PlayMode gate | 這刀碰的是 Editor command shell，沒有直接專屬測試，仍以既有整合 gate 守回歸 |

## 錯誤紀錄
| Error | Attempt | Resolution |
|-------|---------|------------|

## 備註
- 使用者已明確授權我主動使用 multi-agent / sub-agent；只要子任務沒有寫入衝突，就優先平行拆解。
