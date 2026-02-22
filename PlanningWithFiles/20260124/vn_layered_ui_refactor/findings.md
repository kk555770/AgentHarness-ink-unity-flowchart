# 盤點與發現：VN UI 分層改造

## 依賴清單（避免改壞）
`VNPlayerPresenter` 會用 `root.Q(...)` 找這些 name：
- `VNRoot`
- `SpeakerLabel`
- `BodyLabel`
- `ContinueButton`
- `ChoicesContainer`
- `BacklogPanel`
- `BacklogScrollView`
- `BacklogButton`
- `BacklogCloseButton`
- `AutoButton`
- `SkipButton`
- `HideButton`
- `ShowUIButton`

只要這些 name 還在，且 class（`vn-hidden` / `vn-ui-hidden`）規則維持，現有功能就不容易壞。

## UI Toolkit 分層關鍵規則
- 不支援 `z-index`：同一層級的疊放靠 UXML 順序。
- 點擊遮擋靠 `picking-mode`：
  - 視覺層（背景/立繪/特效圖層）應設定 `Ignore`
  - 需要互動（選項/系統按鈕/小遊戲）保持預設 `Position`

## 建議的 VN 分層（對照需求 1~13）
- 1 `base-color-layer`：滿版黑底（避免 Unity 預設背景露出）
- 2 `background-layer/background`：背景圖
- 3 `character-layer`：立繪容器（left/center/right）
- 4 `cg-layer/cg-image`：CG（預設隱藏）
- 5 `background-effect-layer`：背景特效（預設隱藏）
- 6 `speaker-name-box/speaker-name`：名字框
- 7 `dialogue-panel/dialogue-text`：對話框
- 8 `story-effect-layer`：劇情特效（可覆蓋對話框）
- 9 `click-area`：透明點擊捕捉層（避免底層干擾）
- 10 `choice-container`：選項層（要比 click-area 高）
- 11 `system-ui-layer`：系統按鈕列（要比 click-area 高，且不要低於選項）
- 12 `click-effect-layer`：點擊特效（預設隱藏、且不擋點擊）
- 13 `minigame-layer`：小遊戲層（顯示時蓋住底下所有 VN）

## 與現有 UI 的對應策略（降低改動風險）
- `ContinueButton` 建議改作 `click-area`（仍保留 name，讓 C# 不用改）
- `TopBar` 會搬進 `system-ui-layer`（保留 Backlog/Auto/Skip/Hide 按鈕 name）
- `ChoicesContainer` 會搬進 `choice-container`（保留 name）
- `BacklogPanel` 建議放在 `system-ui-layer` 之上（打開時蓋住其他層，避免點擊穿透）

## 測試回報（2026/01/24）
- 現象：主畫面上方出現遮擋區塊，讓對話被遮住一部分；但不影響選項；點一下會消失。
- 推測原因：`ContinueButton`（class `click-area`）是滿版 Button，卻被 `.vn-root Button` 這類較高優先度的按鈕樣式套到背景色/hover 背景，導致它蓋在 `dialogue-ui-layer` 上方時變成「半透明遮罩」。
- 對策：在 `VNPlayer.uss` 用更高優先度 selector（例如 `.vn-root Button.click-area` / `:hover` / `:active`）把背景/背景圖/tint 清掉，讓 click-area 永遠透明。

## 修正結果（2026/01/24）
- 已修改 `Packages/com.opsidanos.ink/Runtime/UI/USS/VNPlayer.uss`：把 `.click-area` / `:hover` / `:active` selector 改成 `.vn-root Button.click-area` / `:hover` / `:active`，確保會覆蓋 `.vn-root Button` 的背景樣式。
- 已用 Unity MCP 進 Play Mode 讀 Console：未看到 error/warning。

## MVP 主題樣式對應（2026/01/24）
- 來源參考檔：`/Users/arcumit/Documents/GitHub/hgame_dev/Assets/UI/UI_MVP/USS/MainTheme_MVP.uss`
- 先做 4 個對應：`.speaker-name-box`、`.dialogue-panel`、`.choice-container`、`.choice-button`
- 本專案選項按鈕 class 是 `vn-choice-button`（由 `VNPlayerPresenter` 加上），所以要用 `.vn-root Button.vn-choice-button`（含 `:hover/:active`）才能確實覆蓋 `.vn-root Button` 的共用按鈕樣式。
- 參考檔的 `.choice-container { display: none; }` 不能照搬，否則選項會永遠顯示不出；本專案用 `vn-hidden` 控制顯示/隱藏。
