# 任務計畫：Batch 0 實作

## 目標
真正落地 Batch 0：建立 `OpsidanosInk.CanonicalGraph` 最小骨架、接上 Editor 測試 asmdef、補上最小 core 測試，並嘗試跑最小 EditMode gate。

## 當前階段
Phase 1

## 階段

### Phase 1：建立工作記錄與確認範圍
- [x] 確認使用者要求開始
- [x] 建立本輪 PlanningWithFiles 紀錄
- [x] 確認先做 Batch 0，不跳到 Batch 1
- **Status:** complete

### Phase 2：實作 core 骨架
- [x] 新增 core asmdef
- [x] 新增最小語意型別
- **Status:** complete

### Phase 3：實作測試掛點與最小測試
- [x] 更新 `OpsidanosInk.EditModeTests.asmdef`
- [x] 新增最小 core 測試
- **Status:** complete

### Phase 4：執行 gate 與回報
- [x] 跑最小 EditMode gate
- [x] 整理結果與風險
- **Status:** complete

## 關鍵問題
1. 新 core 型別最小到什麼程度才夠，不會太空也不會偷跑到 Batch 1？
2. 既有 Editor 測試組件直接參考新 core，是否能穩定編譯？
3. Unity 6000.3.9f1 的最小 EditMode gate 能否順利跑通？

## 已做決策
| 決策 | 原因 |
|------|------|
| 先只做 core 骨架與最小測試 | 避免 Batch 0 偷跑進語意搬遷 |
| 新測試先掛既有 EditMode 測試 asmdef | 最務實且風險最低 |

## 錯誤紀錄
| Error | Attempt | Resolution |
|-------|---------|------------|
| Unity CLI 未產出 `-testResults` XML | 1 | 根因已確認是 Unity Test Framework 1.6.0 的 command line test runner 不應搭配 `-quit`；改成讓 test runner 自行結束後，`Batch0CoreResults.xml` 與 `Batch0GateResults.xml` 已正常產出 |

## 備註
- Batch 0 正式 gate 已確認通過：
  - `Batch0CoreResults.xml`：7/7 passed
  - `Batch0GateResults.xml`：8/8 passed
