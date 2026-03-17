# 任務計畫：文件翻修 Phase 6（Batch 0 開工清單）

## 目標
把 Phase 5 的逐批次提案，往下收成 Batch 0 的真正開工清單，讓後續開始實作時知道第一批要新增哪些檔、要調整哪些 asmdef、要先補哪些最小測試。

## 當前階段
Phase 4

## 階段

### Phase 1：建立工作記錄與確認範圍
- [x] 確認使用者同意繼續
- [x] 建立本輪 PlanningWithFiles 紀錄
- [x] 確認本輪仍只改文件，不改正式腳本
- **Status:** complete

### Phase 2：盤點 Batch 0 的實際落點
- [x] 盤點 Batch 0 相關文件內容
- [x] 盤點現有 asmdef 與測試組織
- [x] 收斂 Batch 0 應新增與應修改的檔案清單
- **Status:** complete

### Phase 3：實作文件翻修
- [x] 新增 Batch 0 開工清單文件
- [x] 更新 `DocsIndex.md`
- [x] 更新 `AuthoringPhase1ImplementationProposal.md`
- [x] 視需要更新 `README.md`
- **Status:** complete

### Phase 4：一致性檢查與回報
- [x] 檢查交叉引用與角色邊界
- [x] 確認文件之間沒有重複搶角色
- [x] 回報給使用者
- **Status:** complete

## 關鍵問題
1. Batch 0 除了新增 core asmdef，還要不要調整現有測試 asmdef？
2. Batch 0 的最小測試應先掛在既有 Editor 測試組件，還是另外開新測試組件？
3. 哪些 placeholder 型別值得先立，哪些則不該在 Batch 0 先做太多？

## 已做決策
| 決策 | 原因 |
|------|------|
| 先把 Batch 0 收成開工清單，再開始真正改碼 | 開工前先把第一批文件化，降低走偏風險 |
| Batch 0 聚焦骨架、asmdef、最小測試 | 這一批的目的不是搬邏輯，而是把地板鋪好 |

## 錯誤紀錄
| Error | Attempt | Resolution |
|-------|---------|------------|
| 無 | 1 | 本輪僅文件與 planning 修改，未出現阻塞 |

## 備註
- 這輪只把 Batch 0 寫清楚，不提前進 Batch 1
