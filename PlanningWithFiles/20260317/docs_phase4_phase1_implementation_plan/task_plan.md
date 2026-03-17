# 任務計畫：文件翻修 Phase 4（第一階段實作計畫）

## 目標
把前面整理出的作者工具重製方向與責任邊界，收斂成一份可執行的第一階段實作計畫，讓後續真正動手時知道先改哪些檔、先不動哪些檔、要怎麼驗證。

## 當前階段
Phase 4

## 階段

### Phase 1：建立工作記錄與確認範圍
- [x] 確認使用者同意繼續下一輪文件翻修
- [x] 建立本輪 PlanningWithFiles 紀錄
- [x] 確認本輪仍只改文件，不改正式腳本
- **Status:** complete

### Phase 2：盤點第一階段實作的共識基礎
- [x] 盤點現有文件裡已經提到的第一階段說法
- [x] 盤點 current GraphToolkit 線與現有測試護欄
- [x] 收斂哪些內容適合進 implementation plan
- **Status:** complete

### Phase 3：實作文件翻修
- [x] 新增第一階段實作計畫文件
- [x] 更新 `DocsIndex.md`
- [x] 更新 `AuthoringToolStrategy.md`
- [x] 更新 `AuthoringRefactorBoundaries.md`
- [x] 視需要更新 `README.md`
- **Status:** complete

### Phase 4：一致性檢查與回報
- [x] 檢查交叉引用與名詞一致性
- [x] 確認文件之間沒有重複搶角色
- [x] 回報給使用者
- **Status:** complete

## 關鍵問題
1. 第一階段到底是「先做 WebView」還是「先切 canonical core 邊界」？
2. 哪些 current 檔案必須先保留，才能讓 round-trip 與 Runtime 驗證繼續存在？
3. 驗證段要直接對應 repo 裡哪些現有測試，才不會變成空話？

## 已做決策
| 決策 | 原因 |
|------|------|
| 先做 implementation plan，再談真正實作 | 先把工程切法和驗證護欄寫清楚，後面動手才不容易失焦 |
| 第一階段聚焦 canonical core 與 adapter 邊界 | 這比先換前端殼更能降低重製風險 |

## 錯誤紀錄
| Error | Attempt | Resolution |
|-------|---------|------------|
| 無 | 1 | 本輪僅文件與 planning 修改，未出現阻塞 |

## 備註
- 這輪仍不決定前端框架與最終桌面殼，只把第一階段工程切法寫清楚
