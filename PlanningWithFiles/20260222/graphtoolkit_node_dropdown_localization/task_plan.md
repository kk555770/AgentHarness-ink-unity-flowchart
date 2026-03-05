# Task Plan: GraphToolkit 節點白話化與結構化對話節點

## Goal
在不破壞 Graph v2 匯出/匯入閉環的前提下，完成兩件事：
1. 已完成：節點白話繁中命名 + `actionKind` 下拉化。
2. 下一階段：把「對話節點」升級為**結構化欄位**，讓使用者不用手填 JSON，
   仍可輸出合法 Ink 與可逆 sidecar。

## Current Phase
Phase 16（實作完成，待使用者驗收）

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

### Phase 6: 測試防暴走流程與場景還原
- [x] 盤點「暴走」根因與受影響範圍
- [x] 實作 GraphToolkit 專用安全測試入口（固定篩選）
- [x] 對 Import/RoundTrip 測試加上固定分類與超時
- [x] 還原非目標場景改動（OffMeshLinkScene）
- [x] 重新驗證並回報結果（Import 8/8、RoundTrip 2/2）
- **Status:** complete

### Phase 7: 節點型別中文化與標題修正
- [x] 重新確認節點標題來源（GraphToolkit 以類別名稱為主）
- [x] 將節點類別改為中文命名並保留序列化相容
- [x] 同步更新匯入/匯出/測試與契約文件
- [x] 執行 GraphToolkit 安全範圍測試驗證
- **Status:** complete

### Phase 8: 新需求完整盤點與計畫落地
- [x] 彙整使用者要求（不用手填 JSON、全欄位下拉/數值化、白話節點）
- [x] 盤點現有節點能力與缺口，區分「可直接做」與「需契約先補齊」
- [x] 把完整拆解寫入 `task_plan.md`、`findings.md`、`progress.md`
- **Status:** complete

### Phase 9: 契約補全（DeveloperModeOutputContract）
- [ ] 新增「結構化對話節點」欄位契約（動作清單、觸發時機、換行、角色綁定）
- [ ] 新增「角色來源契約」（本圖可用角色清單、ID/名稱映射規則）
- [ ] 補齊 Ink 對應表：每個欄位如何映射成 tag 與文字段落
- [ ] 明確定義版本升級策略（舊 `actionKind+content` 與新格式共存）
- **Status:** pending

### Phase 10: 節點資料模型重構（GraphToolkit）
- [ ] 建立對話節點結構化資料模型（ActionStep / ActionParam / Trigger）
- [ ] 為「動作類型」建立白名單（出現/移動/跳躍/旋轉/搖晃/縮放/更換立繪/消失）
- [ ] 建立「可用角色集合」資料來源（供下拉選單）
- [ ] 定義 Node Option Key 與 sidecar 欄位鍵名對照
- **Status:** pending

### Phase 11: 編輯器互動實作（不用手填 JSON）
- [ ] 對話節點加入動作清單 UI（新增/刪除動作列）
- [ ] 動作列支援：動作類型下拉、角色下拉、角色 ID 只讀顯示、角色名稱可覆寫
- [ ] 依動作類型動態顯示參數欄位（數值/枚舉/位置）
- [ ] 加入「是否換行」勾選、「動作觸發時機」下拉 + 自訂秒數欄位
- [ ] 驗證節點內容變多時，節點高度可正確自動撐開
- **Status:** pending

### Phase 12: 匯出/匯入與閉環相容
- [ ] 匯出器把結構化欄位映射成 Ink + tag（由契約規則驅動）
- [ ] sidecar 寫入完整結構化欄位，確保可逆
- [ ] 匯入器完整還原節點欄位與動作列順序
- [ ] 舊版 sidecar（只有 `actionKind+content`）匯入相容策略落地
- **Status:** pending

### Phase 13: 測試與防暴走驗證
- [ ] 新增 Import/RoundTrip 測試覆蓋所有動作類型與觸發時機
- [ ] 新增 Editor 互動測試（動作列增減、動態參數切換、角色映射）
- [ ] 維持 `GraphToolkitFlowSafe` 範圍 + 每 fixture `Timeout(60000)`
- [ ] 只用固定篩選流程執行，避免再次觸發 package 測試暴走
- **Status:** pending

### Phase 14: 文件與交付
- [ ] 更新 `DeveloperModeOutputContract.md` 範例（含完整對話節點案例）
- [ ] 更新 GraphToolkit 操作說明（一般使用者步驟）
- [ ] 提供最小可驗證流程圖範本（開始→對話→選項/條件）
- **Status:** pending

### Phase 15: 流程線/資料線分離（動作節點拆分）
- [x] 新增 `stageAction` 節點型別與中文節點「動作」
- [x] 對話節點新增可調 `ActionIn*` 資料輸入埠
- [x] 動作節點改為 `ActionData` typed 輸出埠
- [x] 匯出 sidecar 新增 `toPortName`，保存目標輸入埠
- [x] 匯入改為依 `toPortName` 還原連線（Flow / ActionIn*）
- [x] 匯出驗證新增 stageAction 規則（必須接到對話 ActionIn*）
- [x] 匯出 Ink 時把 stageAction 內容併入對話 knot，stageAction 不單獨輸出 knot
- [x] Import/RoundTrip 測試新增 stageAction 資料線案例
- [x] 契約文件補上流程線/資料線定義與 sidecar 欄位
- **Status:** complete

### Phase 16: 文字欄位寬度調整（先契約、後實作）
- [x] 先更新 `DeveloperModeOutputContract.md`，補上「節點文字欄位可讀寬度」規範
- [x] 新增 GraphToolkit 樣式檔，拉長節點內文字欄位顯示寬度
- [x] 新增編輯器 bootstrap，僅在 `.inkfc` 圖視窗套用樣式
- [ ] 待使用者實機確認欄位可讀性是否符合需求
- **Status:** in_progress

### Phase 17: 執行規則更新（multi-agent 與時限）
- [x] 明確鎖定：可並行任務必須走 multi-agent
- [x] 明確鎖定：multi-agent 的測試任務每次最多 60 秒
- [x] 明確鎖定：調查任務與修改任務不受 60 秒上限限制
- [x] 記錄「先提案、等同意、再執行」為當前會話硬規則
- **Status:** complete

## Key Questions
1. 角色清單來源要放哪裡（Graph 資產內、專案設定資產、或場景資料）？
2. 動作參數是否統一為「鍵值對」模型，還是每個動作獨立強型別類別？
3. 觸發時機的「自訂」時間單位固定秒數嗎？是否允許負值？
4. 節點寬度目前不提供手拉，是否先以自動高度 + 折疊區塊交付第一版？

## Decisions Made
| Decision | Rationale |
|----------|-----------|
| 先在既有 `action` 節點新增下拉，不先拆新節點 | 變更最小，對既有圖與匯出流程衝擊最小 |
| `actionKind` 要進 sidecar | 避免匯入再匯出遺失 UI 選擇，維持可逆閉環 |
| 節點與欄位中文顯示名集中管理 | 後續改名只改一處，避免字串分散 |
| 測試先嘗試自動執行，再回報環境阻擋 | 不假裝通過；環境被佔用時要明確揭露 |
| 測試防暴走改為「流程守門」而非只靠口頭提醒 | 防止再次誤跑整包 EditMode/Package 測試，避免 Unity/MCP 被拖垮 |
| 節點標題改為「類別名即中文」 | GraphToolkit Title 取類別名稱，直接改類別名才會穩定生效 |
| 對話節點不允許一般使用者手填 JSON | 降低輸入錯誤，改由欄位化資料產生合法輸出 |
| 圖（Flow Chart）是流程權威，玩家模式只解讀輸出文本 | 維持流程閉環與可逆，不讓流程藏進自由文字 |
| 規範必須先對齊 Ink 基準層，不可只貼現有進度 | 避免規範被現況綁死，確保後續可擴充且可檢驗 |
| 多分支不只 choice，condition/小遊戲等節點都要可調輸出數量 | 符合文字遊戲實際流程設計，避免單一路線假分支 |
| 對話節點與動作節點拆分，並用 typed data port 連線 | 用線路語意防止誤接，讓「同句多角色動作」可視覺化且可逆 |
| 先只做「欄位變寬 + 契約更新」，不提前做選項化重構 | 依使用者當前指令，先解決編輯可讀性阻塞點 |
| 欄位寬度用專案樣式覆寫，不改匯出/匯入語意 | 避免 UI 微調影響 Graph v2 閉環資料契約 |
| 可並行工作必須使用 multi-agent；單線程多次查找不算並行 | 對齊全域規範，降低阻塞與重工 |
| multi-agent 測試任務上限 60 秒；調查/修改不限時 | 避免測試任務卡死，同時保留分析與實作完整度 |
| 先提案、等同意、再執行 | 防止未授權操作，確保每次變更可追溯 |

## Errors Encountered
| Error | Attempt | Resolution |
|-------|---------|------------|
| MCP `run_tests` 回傳 `no_unity_session` | 1 | 改走 Unity CLI 測試 |
| Unity CLI 測試被阻擋（專案已被另一個 Unity 開啟） | 1 | 停止重試並回報使用者 |
| 直接跑整包 EditMode 造成 Unity 場景測試鏈暴走 | 1 | 以 assembly+category+fixture 固定篩選，並加入 60 秒超時 |
| MCP transport error (`localhost:8080/mcp`) | 1 | 等使用者恢復後，改用固定篩選重跑並驗證通過 |
| 對話節點需求遠超 `actionKind + content` 現有結構 | 1 | 先把完整需求拆解成 Phase 9-14，再進入實作 |
