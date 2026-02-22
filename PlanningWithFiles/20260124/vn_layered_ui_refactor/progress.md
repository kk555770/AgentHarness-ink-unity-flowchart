# 進度紀錄：VN UI 分層改造

## 2026/01/24
- 建立規劃檔（本檔 / `task_plan.md` / `findings.md`）
- 已盤點 `VNPlayer` 現況與 `VNPlayerPresenter` 依賴的 element name
- 已確認 `Packages/com.opsidanos.ink/Runtime/UI/UXML/VNPlayer.uxml` 目前已是 1~13 分層骨架（保留必要 name）
- 已確認 `Packages/com.opsidanos.ink/Runtime/UI/USS/VNPlayer.uss` 仍是舊樣式（需要改成對應新分層的 USS）
- 已用 Unity MCP 讀 Console：目前未看到 error/warning（只有 Tag/BGM/SE log）
- 已修改 `Packages/com.opsidanos.ink/Runtime/UI/USS/VNPlayer.uss`：改成對應 1~13 分層（base/background/character/cg/effect/dialogue/click/choice/system/backlog/minigame）
- 已用 Unity MCP 進 Play Mode 再讀 Console：未看到 USS 相關 warning/error
- 下一步：你在 Game View（1920x1080）目視確認分層與點擊行為是否符合預期
- 你回報：主畫面有遮擋區塊蓋住對話，但不影響選項，點一下會消失（推測是 click-area 套到一般 Button 的背景樣式）
- 已修正遮擋：提高 click-area selector 優先度（改成 `.vn-root Button.click-area`，含 `:hover/:active`）
- 已用 Unity MCP 再次進 Play Mode 讀 Console：未看到 error/warning
- 已依你同意：套用 MVP 主題的 4 個樣式對應到 `Packages/com.opsidanos.ink/Runtime/UI/USS/VNPlayer.uss`（名字框/對話框/選項容器/選項按鈕）
- 已用 Unity MCP 進 Play Mode + 讀 Console：未看到新增 warning/error
- 下一步：請你在 Game View（1920x1080）目視確認外觀（名字框/對話框/選項）
- 你回報：基本上沒有問題（Phase 5 完成）
