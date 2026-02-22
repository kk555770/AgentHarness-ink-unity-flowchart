# 任務計畫：VN UI 分層改造（文字遊戲）

## 目標
- 把 `VNPlayer` 的 UI Toolkit 結構改成「明確分層」，讓未來要插入背景/立繪/CG/特效/小遊戲時，有固定的掛點。
- 必須符合 `UIToolkitSpec.md`（不使用 `z-index`、不使用不支援的 selector/屬性）。
- 改動前先提案，取得同意後才開始修改代碼/資產。

## 現況
- 目前 `VNPlayer.uxml` 已改成 1~13 分層骨架（保留 `VNPlayerPresenter` 依賴的 element name）。
- 目前 `VNPlayer.uss` 已對應新分層 class，並修正 click-area 透明問題（避免出現半透明遮罩蓋住對話）。
- `PanelSettings.asset` 已設定 `Scale With Screen Size`，ReferenceResolution=1920x1080（解析度變大 UI 會跟著放大）。

## 規則與限制
- UI Toolkit 不支援 `z-index`：疊放順序用 UXML 兄弟順序控制（越後面越上層）。
- 互動遮擋用 `picking-mode` 控制：
  - 視覺層：`Ignore`
  - 需要接收點擊的層：`Position`

## 階段
### Phase 1：盤點與對照
- [x] 重新閱讀 `AGENTS.md`
- [x] 重新閱讀 `UIToolkitSpec.md`
- [x] 盤點 `VNPlayer.uxml/.uss` 與 `VNPlayerPresenter` 依賴的 element name
- **狀態：complete**

### Phase 2：提案（等你同意後才改）
- [x] 提案：補齊 `VNPlayer.uss`（對應 1~13 層），並保留現有功能不壞
- [x] 取得你同意後再開始修改
- **狀態：complete**

### Phase 3：實作（取得同意後）
- [x] 改 `VNPlayer.uxml`：新增/重排分層結構與 placeholder（已完成）
- [x] 改 `VNPlayer.uss`：建立分層定位、隱藏規則、互動遮擋規則（已完成）
- [x] 用 Unity MCP 進 Play Mode 檢查：Console 無 warning/error（已完成）
- **狀態：complete**

### Phase 4：你驗證（取得同意後）
- [x] 你在 Game View（1920x1080）檢查：UI 分層與點擊行為符合預期（遮擋已修正）
- **狀態：complete**

### Phase 5：MVP 主題樣式四項對應（取得同意後）
- [x] 參考 `/Users/arcumit/Documents/GitHub/hgame_dev/Assets/UI/UI_MVP/USS/MainTheme_MVP.uss`，把 4 個樣式對應到本專案（`speaker-name-box` / `dialogue-panel` / `choice-container` / `vn-choice-button`）
- [x] 用 Unity MCP 進 Play Mode 檢查：Console 無 warning/error
- [x] 你目視確認（1920x1080）：名字框/對話框/選項外觀符合 MVP 主題期待（你回報：基本上沒有問題）
- **狀態：complete**

## 決策紀錄
| 決策 | 原因 |
|------|------|
|      |      |

## 錯誤紀錄
| 錯誤 | 嘗試次數 | 解法 |
|------|----------|------|
| click-area 出現半透明遮罩蓋住對話 | 1 | 提高 selector 優先度：把 `.click-area` 改成 `.vn-root Button.click-area`（含 `:hover/:active`） |
