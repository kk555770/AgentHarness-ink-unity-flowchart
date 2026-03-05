# Task Plan: 狀態2 角色節點與登場集束節點

## Goal
在不破壞現有 Graph v2 匯出/匯入閉環前提下，完成狀態2需求：
1. 新增 `character` 節點（角色ID、角色名、可增減表情+圖片列）。
2. 新增 `castBundle` 節點（收束本次可用角色）。
3. `對話/動作` 節點角色下拉只讀 `castBundle` 輸出清單。
4. 拉線/斷線/角色資料變更後可刷新下拉。

## Scope Baseline
- 基準提交：`4412d94`（已重新 commit 的狀態1）。
- 本計劃目前只做規劃，不做程式修改。
- 任何程式修改前都要先提案並等使用者同意。

## Current Phase
Phase 2（契約草案已完成，待使用者確認）

## Phases
### Phase 1: 任務邊界凍結與基準確認
- [x] 重讀最後任務需求（狀態2）
- [x] 確認目前基準提交與工作樹狀態
- [x] 建立新 planning 三檔
- **Status:** complete

### Phase 2: 契約先行（DeveloperModeOutputContract）
- [x] 定義 `character` 節點欄位契約（ID/名稱/表情清單/資源路徑）
- [x] 定義 `castBundle` 節點欄位契約（輸入數量/輸出角色集合）
- [x] 定義 `action`/`stageAction` 的角色下拉來源規則
- [x] 定義刷新時機（接線變更、欄位變更、匯入後）
- [x] 定義匯出 sidecar 欄位與舊資料相容策略
- **Status:** complete

### Phase 3: GraphToolkit 資料模型調整
- [ ] `InkFlowChartNodes.cs` 增加 `character`/`castBundle` 節點
- [ ] `character` 支援列數增減與 `Texture2D` 欄位
- [ ] `castBundle` 支援動態輸入埠（`CharacterIn*`）
- [ ] `action`/`stageAction` 增加 `CastIn` 輸入埠與角色 option key
- **Status:** pending

### Phase 4: 編輯器下拉刷新機制
- [ ] 建立「由 CastIn 收集角色候選」流程
- [ ] 套用角色下拉到 `action` 與 `stageAction`
- [ ] 接線變更時刷新（圖變更事件）
- [ ] 角色節點資料改動時刷新
- **Status:** pending

### Phase 5: 匯出/匯入閉環
- [ ] `InkFlowChartExportModels.cs` 補齊新 DTO 欄位
- [ ] `InkFlowChartExporter.cs` 輸出 `character`/`castBundle` 與連線資訊
- [ ] `InkFlowChartImporter.cs` 還原節點、欄位與連線
- [ ] 保留舊 sidecar 匯入相容
- **Status:** pending

### Phase 6: 測試與驗證（防暴走）
- [ ] 新增 Import 測試（角色節點、集束節點、角色下拉來源）
- [ ] 新增 RoundTrip 測試（欄位與連線不丟失）
- [ ] 測試執行採固定篩選 + 單次 60 秒上限
- [ ] console error 清查
- **Status:** pending

### Phase 7: 文件與交付
- [ ] 更新契約文件與操作說明
- [ ] 更新當日 planning 三檔（做一步、記一步）
- [ ] 提交變更摘要與殘餘風險
- **Status:** pending

## Hard Rules (Session)
- 可並行任務必須使用 multi-agent，不可用單線程連續查找替代。
- 測試任務單次上限 60 秒。
- 調查與修改任務不限時。
- 先提案、等同意、再執行。

## Key Questions
1. 目前無阻塞問題，可進入 Phase 2 契約草案提案。

## Decisions Made (2026-03-03 MCP)
| Decision | Rationale |
|----------|-----------|
| 角色圖片引用採 `Addressable + GUID` | 兼顧可讀性與穩定重定位 |
| 未加入 Addressable 的圖片自動加入固定 Group `Project_Resources` | 減少手動操作，流程一致 |
| 若 `Project_Resources` 不存在則自動建立 | 降低首次使用阻力，避免流程中斷 |
| Addressable Key 最終格式固定 `char/{id}/{expr}` | 可讀且可由節點欄位直接推導 |
| 先拖圖後補欄位時，Key 採即時重算更新 | 對齊實際編輯流程，不要求先填完整資料 |
| 自動加入策略採 A+B（拖圖即加入；改欄位與圖變更時重算） | 同時滿足即時可用與資料一致性 |
| 刷新時機採「圖變更就刷新」 | 避免每幀重算造成暴走，同時維持即時性 |
| 對話/動作的名稱覆寫只影響當下，不回寫角色下拉來源 | 保持角色主資料穩定，避免污染全域 |
| `castBundle` 輸出先做依接線自動增減 | 對齊使用者要的彈性資料流 |
| `castBundle` 多輸出都送同一份角色名單 | 第一版先穩定資料來源語意，避免分組複雜度 |

## Errors Encountered
| Error | Attempt | Resolution |
|-------|---------|------------|
| 尚無 | - | - |
