# 任務計畫：Flow Chart 分岔節點規範與匯出/匯入閉環

## 目標
在不靠「多行 `->`」的前提下，補齊 Flow Chart（GraphToolkit）對「選項 / 條件 / 結果」分岔的明確規範，並讓 `.inkfc ⇄ (.ink + .flowchart.json)` 能穩定互轉且可驗證。

## 目前階段
Phase 1

## 階段

### Phase 1：需求確認與現況盤點
- [ ] 把「多條輸出線」的語意拆成節點型別（選項 / 條件 / 結果）
- [ ] 盤點目前 GraphToolkit 節點/匯出/匯入的限制與缺口
- [ ] 把關鍵發現寫入 findings.md
- **Status:** in_progress

### Phase 2：規範文件更新（先規範、後實作）
- [ ] 更新 `Documentation/DeveloperModeOutputContract.md` 的 Graph 章節
- [ ] 明確定義每種分岔節點的輸入/輸出線規則、必要欄位、匯出 Ink 對應語法
- **Status:** pending

### Phase 3：GraphToolkit 節點模型調整
- [ ] 新增「選項節點 / 條件節點」並支援可調整輸出埠數量
- [ ] 定義每條輸出線所需的最小資料（例如：選項文字、條件式）
- **Status:** pending

### Phase 4：匯出/匯入工具對齊
- [ ] 匯出：依節點型別輸出正確 Ink（`*` 選項 / 條件 divert）
- [ ] 匯入：依節點型別還原節點、選項/條件資料與連線
- **Status:** pending

### Phase 5：測試鎖定與驗證（Unity MCP）
- [ ] 更新/新增 EditMode 測試覆蓋分岔節點匯出/匯入
- [ ] 使用 Unity MCP 跑 `OpsidanosInk.EditModeTests`
- [ ] 記錄測試結果到 progress.md
- **Status:** pending

### Phase 6：交付
- [ ] 彙整本次規範與程式修改重點
- [ ] 列出尚未覆蓋的分岔類型（例如：小遊戲結果）
- **Status:** pending

## 關鍵問題（需要使用者確認）
1. 條件節點的「其他/否則」分支，是否固定為「最後一個輸出埠」？
2. 條件分支的條件式，是否直接使用 Ink 表達式字串（例如 `favor > 7`）？
3. 小遊戲結果分岔是否先以「條件節點」承載（由外部先把結果寫入 Ink 變數），本次先不新增專用節點？

## 已做決策
| 決策 | 理由 |
|---|---|
| （待補） |  |

## 錯誤紀錄
| 錯誤 | 嘗試 | 解法 |
|---|---:|---|
| （待補） | 1 |  |

