# 任務計畫：打字機效果 + Auto 1 秒規則（不與點擊衝突）

## 目標
- 加入「打字機效果」：對話文字逐字顯示。
- Auto 模式改成：**打字機完成 + 動畫完成** 後，**再等 1 秒** 才會進下一句。
- 點擊與 Auto 不打架：
  - 點擊進下一句：Auto 的等待不會延續到下一句（直接重置）。
  - 點擊把打字機/動畫強制到終點：Auto 立刻開始記 1 秒；若期間沒有點擊才會自動進下一句。
  - 若在 Auto 的 1 秒等待期間點擊：走「點擊推進規則」，這一次 Auto 直接作廢（不會再自動進下一句）。

## 目前階段
完成

## Phases

### Phase 1：規格確認與盤點
- [x] 整理打字機規格（逐字速度、可否關閉、ForceComplete 行為）
- [x] 整理 Auto/Skip/點擊 的優先序與互動
- **狀態：complete**

### Phase 2：實作打字機（Presenter）
- [x] `VNPlayerPresenter`：加入打字機 coroutine（逐字更新 `BodyLabel`）
- [x] 支援 ForceComplete：一鍵顯示完整文字，並讓 Busy 判斷包含打字機
- **狀態：complete**

### Phase 3：Auto 1 秒規則（不與點擊衝突）
- [x] Auto：等「打字機 + 動畫」都完成後，再等 1 秒才推進
- [x] 點擊：等待期間點擊會作廢這次 Auto；點擊進下一句會重置
- [x] 加「同一幀只推進一次」保護，避免 Auto 與點擊同時觸發
- **狀態：complete**

### Phase 4：驗證
- [x] 你在 Play Mode 驗證：逐字顯示、點一下補完、Auto 1 秒邏輯符合規格
- [x] 我讀 Unity Console：Error=0、Warning=0
- **狀態：complete**

### Phase 5：交付
- [x] 更新本任務 `progress.md` 與狀態
- **狀態：complete**

## 決策紀錄
| 決策 | 原因 |
|------|------|
| 打字機先做在 `VNPlayerPresenter` | 先最小可用，之後再抽成獨立元件（若需要） |

## 錯誤紀錄
| 錯誤 | 嘗試 | 解法 |
|------|------|------|
