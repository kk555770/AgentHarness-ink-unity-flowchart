# 任務計畫：Batch 4 bridge kickoff 實作

## 目標
建立 `canonical graph` 最小控制面骨架，讓 repo 第一次具備可呼叫的 graph document 與 command service，而不是只停在文件規格。

## 當前階段
Phase 1

## 階段

### Phase 1：對齊實作形狀
- [x] 重讀 proposal planning
- [x] 盤點現有 core / tests 風格
- [x] 收斂最小操作集合
- **Status:** complete

### Phase 2：落地 core 骨架
- [x] 新增 graph document / node / edge 記錄
- [x] 新增 operation result / validation result
- [x] 新增 command service
- **Status:** complete

### Phase 3：補測試與驗證
- [x] 新增 EditMode 測試
- [x] 跑 EditMode gate
- [x] 跑 PlayMode gate
- [x] 清理測試暫存與場景噪音
- [x] 整理 findings / progress
- **Status:** complete

## 關鍵問題
1. 第一版 command service 最小應支援哪些操作？
2. 哪些資料欄位先用最小骨架就好，不要一開始打滿？
3. 這批要怎麼做，才能不碰現有 GraphToolkit / runtime 閉環？

## 已做決策
| 決策 | 原因 |
|------|------|
| 第一版只做 `CreateGraph / CreateNode / ConnectPorts / ValidateGraph` | 這四個是文件與 JSON contract 都共同指向的最小控制面 |
| `CanonicalGraphDocument` 走 DTO 風格 | 對齊現有 `CurrentFlow*Models` 的薄型資料載體風格 |
| `OperationResult / ValidationResult` 走 `readonly struct` | 對齊現有 import / export result 的 result 工廠風格 |

## 錯誤紀錄
| Error | Attempt | Resolution |
|-------|---------|------------|

## 備註
- 使用者已同意這一批實作，可以正式改檔。
