# 任務計畫：Batch 1 節點語意 seam

## 目標
開始落地 Batch 1：把 `InkFlowChartNodes.cs` 內混在 GraphToolkit 殼裡的節點種類、埠語意與 current projection 命名 helper，先抽成可被 Editor 與後續匯出匯入共用的 seam，同時守住現有 GraphToolkit smoke / export 線。

## 當前階段
Phase 4

## 階段

### Phase 1：確認切分邊界
- [x] 重讀 Batch 1 提案
- [x] 檢查 `InkFlowChartNodes.cs` 現況
- [x] 確認第一刀只切語意，不碰 projection/traversal
- **Status:** complete

### Phase 2：建立新 seam 型別
- [x] 新增 Batch 1 需要的 core/helper 型別
- [x] 決定哪些 helper 留在 Editor 殼
- **Status:** complete

### Phase 3：改接現有 GraphToolkit 腳本
- [x] 更新 `InkFlowChartNodes.cs`
- [x] 更新 `InkFlowChartExporter.cs`
- [x] 更新 `InkFlowChartImporter.cs`
- **Status:** complete

### Phase 4：補測試與驗證
- [x] 新增或更新最小測試
- [x] 跑 Batch 1 最小 gate
- **Status:** complete

## 關鍵問題
1. 哪些語意可以安全移到 core，而不把 `Unity.GraphToolkit` 型別帶進去？
2. `dialogue/action` 的 canonical/current projection 對照，這一批應放在哪一層最不容易再打結？
3. Batch 1 最小可驗證範圍要守到哪裡，才算真的切出 seam 而不是只搬常數？

## 已做決策
| 決策 | 原因 |
|------|------|
| 先從 `InkFlowNodeSchema` 下手 | 這裡是目前責任最混的地方 |
| 先不碰 Runtime 主線 | Batch 1 只處理作者工具的節點語意切分 |
| `INode -> current projection node type` 另立 Editor adapter | 這段會碰到 GraphToolkit 型別，不適合塞回純 core |
| `UnityTest` 類的 round-trip 驗證另跑非同步 gate | `runSynchronously` 只適合先守同步測試與編譯護欄 |

## 錯誤紀錄
| Error | Attempt | Resolution |
|-------|---------|------------|
| `runSynchronously` 那輪沒有真的跑到 `UnityTest` 類 round-trip 方法 | 1 | 保留同步 gate 先守 core/smoke/export，再補一輪不帶 `-runSynchronously` 的 async gate，正式驗證 import 與 round-trip |

## 備註
- 這一輪若發現 helper 依賴 `GraphToolkit` 型別，就不要硬搬進 core，先切成 Editor adapter 與 core semantics 兩層。
- Batch 1 第一刀已完成：
  - `Batch1GateResults.xml`：20/20 passed（core naming + smoke + export）
  - `Batch1AsyncGateResults.xml`：12/12 passed（import + round-trip）
