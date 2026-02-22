# 任務計畫：倒退（Rollback/Load）時 char 層級規則相反

日期：2026/02/01  
任務資料夾：`PlanningWithFiles/20260201/rollback_char_layer_reverse/`

## 目標（8歲也看得懂）
- 正常推進：後做的動作在上面（維持現在行為）
- 倒退/讀檔：先做的動作在上面（規則相反，且「動作中」與「動作結束後重排」都要一致）

## 現況（已知）
- `InkTagCharacterStatePlayer`：
  - 動作中：用 `BringToFront()` 讓動作中的角色暫時置頂
  - 動作結束：用固定順序重排（左→中→右）
- `InkSaveSystem`：
  - 倒退/讀檔會用 `EmitExternalOutput()` 丟出同一句輸出，讓 UI 與 Tag 重新跑一遍
- 目前沒有區分「正常輸出」與「倒退輸出」，所以倒退時也會走同一套置頂規則

## 主要做法（我要怎麼做）
1. 在 `StoryOutput` 加上來源（Normal / Restore）
2. `InkStoryEngine` 產生的輸出標成 Normal
3. `InkSaveSystem` 送出的倒退/讀檔輸出標成 Restore
4. `InkTagCharacterStatePlayer` 依輸出來源切換層級規則：
   - Normal：維持原本 `BringToFront()`
   - Restore：改成「把後做的塞到前面那疊的下面」（先做的在上面）
   - 動作結束後固定重排也要反過來
   - `ForceComplete` 也要套用同樣判定（避免漏規則）

## Phase（調查/實作一步、記錄一步）
| Phase | 內容 | 狀態 |
| --- | --- | --- |
| 1 | 調查：目前 char 層級相關程式碼位置與呼叫點 | 完成 |
| 2 | 實作：StoryOutput 來源標記（Normal/Restore） | 完成 |
| 3 | 實作：倒退時動作中置頂規則相反（含 anchor/插隊邏輯） | 完成 |
| 4 | 實作：倒退時動作結束後固定重排相反（含 ForceComplete） | 完成 |
| 5 | 驗證：用 Unity 場景測試幾個 char transition case | 完成 |
| 6 | 文件：補齊 findings/progress 與規則說明 | 完成 |

## 驗收方式（Play Mode 肉眼驗收清單）
> 目標：讓「正常推進」與「倒退/讀檔」的層級規則都完全一致，不會因為 ForceComplete 或固定重排而漏掉。

### A. 正常推進（Normal）
- 進入 Play Mode：`Assets/Scene/Test.unity`
- 找一段會出現「多個角色重疊」或「分步動作（steps）」的句子
- 確認規則：**後做的動作在上面**
  - 例：先動 A 再動 B → B 應該蓋在 A 上面

### B. 倒退/讀檔（Restore）
- 先往前走幾句，確保有至少 2 次以上的角色動作（避免只看一個角色）
- 按「倒帶」或「讀檔」回到前一句/前幾句
- 確認規則：**先做的動作在上面**
  - 例：原本是先動 A 再動 B → 倒退回放時 A 應該在最上面

### C. ForceComplete（點擊強制完成）
- 在角色正在動的時候連點（觸發 `ForceComplete`）
- 確認：
  - Normal 時仍是「後做的在上」
  - Restore 時仍是「先做的在上」

### D. 如果看起來不對（排查順序）
- 先確認是不是「倒退輸出」：倒退/讀檔應該標記為 `StoryOutputSource.Restore`
- 再確認是不是同時有舊式角色（`InkTagCharacterPlayer`）與新式角色（`InkTagCharacterStatePlayer`）在互相影響判斷

## 錯誤紀錄
| 時間 | 內容 | 解法 |
| --- | --- | --- |
|  |  |  |
