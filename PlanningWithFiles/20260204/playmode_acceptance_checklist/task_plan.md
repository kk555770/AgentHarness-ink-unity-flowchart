# 任務計畫：Play Mode 驗收清單（存/讀/倒帶 + 倒退角色層級）

日期：2026/02/04  
任務資料夾：`PlanningWithFiles/20260204/playmode_acceptance_checklist/`

## 目標（8歲也看得懂）
- 我想用一張清單，確認 `Assets/Scene/Test.unity` 在 Play Mode 真的「能玩、能存、能倒帶」
- 我想把結果記下來：哪一項通過、哪一項失敗、下一步要修哪裡

## 範圍與限制
- 只做「驗收/記錄」與「git 乾淨化（忽略 `.DS_Store`）」；不新增新功能
- 驗收場景固定：`Assets/Scene/Test.unity`
- 驗收重點：Save / Load / Rollback、ForceComplete（連點）、倒退時 `char` 層級規則
- 需要看 Console / 場景接線時，優先用 Unity MCP 讀取（避免用猜的）

## 目前已知（從先前任務整理）
- 存檔/讀檔/倒帶：最小版已完成，但需要 Play Mode 肉眼驗收
- 倒退角色層級：`PlanningWithFiles/20260201/rollback_char_layer_reverse/` 已做到 Phase 5（待驗收）
- ResourceMap：Editor 用 `assetPath`；Player 用 Addressables `address`（本次先驗收 Editor Play Mode）

## 交付物
- `findings.md`：驗收結果表（每項 ✓/✗ + 備註）
- `progress.md`：時間序工作紀錄
- `git status` 乾淨（不再出現 `Samples~/.DS_Store`）

## Phase（調查/實作一步、記錄一步）
| Phase | 內容 | 狀態 |
| --- | --- | --- |
| 1 | 整理驗收項目與步驟（清單化） | 完成 |
| 2 | 檢查場景接線與基本環境（必要時用 Unity MCP） | 完成 |
| 3 | 進 Play Mode 逐項驗收（人工/或能自動就自動） | 完成 |
| 4 | 總結：通過/失敗原因 + 下一步修正提案清單 | 完成 |
| 5 | 小清理：更新 `.gitignore` 並移除 `.DS_Store` | 完成 |
| 6 | 新增 PlayMode 自動測試（Save/Load/Rollback + char 層級 + ForceComplete） | 完成 |
| 7 | 用 Unity Test Runner 跑測試並記錄結果（用 Unity MCP） | 完成 |

## 驗收清單（總覽）
### A. 基本播放
- [x] 文字能顯示、選項能點
- [x] Backlog（回看）能開/關、內容正確
- [x] Auto 能自動前進
- [x] Skip 能快速跳
- [x] 隱藏 UI 能切換（不影響故事狀態）
- [x] 打字機：文字會逐字出現；點擊可強制完成（ForceComplete）

### B. Tag 演出（至少各看一次）
- [x] `bg` 背景切換
- [x] `cg` 顯示/清除
- [x] `bgm` 播放/切換
- [x] `se` 播放
- [x] `char`（包含 steps 轉場）能出現/移動/消失
- [x] `shake` 震動效果

### C. Save / Load
- [x] 先推進幾句 → 按「存檔」
- [x] 再推進幾句（讓畫面明顯變）→ 按「讀檔」
- [x] 驗收：文字/選項/背景/CG/BGM/角色/震動狀態都回到存檔那一句

### D. Rollback（倒帶）
- [x] 連按倒帶 3～5 次
- [x] 驗收：每次都回到上一句，且畫面狀態也跟著回去

### E. 倒退角色層級規則（最重要）
- [x] Normal（正常推進）：後做的在上面
- [x] Restore（倒退/讀檔）：先做的在上面
- [x] ForceComplete 連點也要符合上面兩條規則

## 關鍵問題
1. 目前「存/讀/倒帶」是否足夠穩定？（有沒有漏掉某些 Tag 狀態）
2. 倒退角色層級是否真的修好？（含動作中與動作結束後重排、含 ForceComplete）
3. 如果失敗，最小修正點在哪裡？（要改哪個類別/哪個規則）

## 決策紀錄
| 決策 | 原因 |
| --- | --- |
| 先做 Play Mode 驗收再進 Phase 6（Editor Flow Chart） | 先把 Runtime 站穩，後面才不會一直回頭修 |

## 錯誤紀錄
| Error | Attempt | Resolution |
| --- | --- | --- |
|  | 1 |  |
