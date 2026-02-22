# Task Plan: GraphToolkit 節點白話命名與下拉選單

## Goal
在不破壞 Graph v2 匯出/匯入閉環的前提下，補上節點白話名稱設定與
可下拉選的節點格式欄位，並同步更新 sidecar、測試與契約文件。

## Current Phase
Phase 5（完成）

## Phases
### Phase 1: 規範對齊與現況確認
- [x] 確認 GraphToolkit 節點現況（目前多為手動輸入）
- [x] 確認契約文件對節點型別與 sidecar 欄位定義
- [x] 確認可修改檔案與測試覆蓋點
- **Status:** complete

### Phase 2: 節點與欄位設計
- [x] 建立節點顯示名稱集中常數
- [x] 補 Action 節點「內容類型」下拉欄位
- [x] 讓節點標題可由集中常數控制
- **Status:** complete

### Phase 3: 匯出匯入與 sidecar 同步
- [x] sidecar 增加 `actionKind`
- [x] 匯出寫入 `actionKind`
- [x] 匯入讀回 `actionKind`（含舊資料預設值）
- **Status:** complete

### Phase 4: 測試與文件更新
- [x] 更新 Import/RoundTrip 測試
- [x] 更新 DeveloperModeOutputContract.md
- [x] 檢查測試是否反映新欄位行為
- **Status:** complete

### Phase 5: 驗證與交付
- [x] 執行目標測試（已嘗試，受 Unity 執行個體限制）
- [x] 彙整變更與風險
- [x] 回報使用者
- **Status:** complete

## Key Questions
1. 在不新增新節點型別下，如何讓一般使用者先選格式再填內容？
2. 新增 `actionKind` 後，如何保證舊版 sidecar 仍可匯入？

## Decisions Made
| Decision | Rationale |
|----------|-----------|
| 先在既有 `action` 節點新增下拉，不先拆新節點 | 變更最小，對既有圖與匯出流程衝擊最小 |
| `actionKind` 要進 sidecar | 避免匯入再匯出遺失 UI 選擇，維持可逆閉環 |
| 節點與欄位中文顯示名集中管理 | 後續改名只改一處，避免字串分散 |
| 測試先嘗試自動執行，再回報環境阻擋 | 不假裝通過；環境被佔用時要明確揭露 |

## Errors Encountered
| Error | Attempt | Resolution |
|-------|---------|------------|
| MCP `run_tests` 回傳 `no_unity_session` | 1 | 改走 Unity CLI 測試 |
| Unity CLI 測試被阻擋（專案已被另一個 Unity 開啟） | 1 | 停止重試並回報使用者 |
