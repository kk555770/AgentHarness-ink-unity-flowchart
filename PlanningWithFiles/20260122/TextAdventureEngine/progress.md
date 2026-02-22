# Progress：文字冒險 / 視覺小說引擎（v1，2026-01-22 續作）

## 2026-01-22
- 盤點 `PlanningWithFiles/`：
  - `PlanningWithFiles/20260120/TextAdventureEngine/`：主計畫，Phase 3~7 尚未全數完成
  - `PlanningWithFiles/20260122/OpsidanosInk/`：修錯任務，Phase 5（Unity 驗證）已完成
- 建立本次續作計畫資料夾：`PlanningWithFiles/20260122/TextAdventureEngine/`
- 已取得同意：開始實作 Phase 3 剩餘 4 項（回看 / AutoSkip / 隱藏 UI / Tag 管線）
- 完成 Phase C（實作 Phase 3 剩餘 4 項）：
  - 回看（Backlog）：加入回看面板（ScrollView）與歷史文字累積
  - Auto / Skip：加入 coroutine 自動推進（遇到選項/結束會自動停）
  - 隱藏 UI：用 `.vn-ui-hidden` 切換（保留「顯示 UI」按鈕可恢復）
  - Tag → 結構化資料 → 事件：新增 `InkTag` / `InkTagParser`，並把解析後資料放進 `StoryOutput.ParsedTags`
- 更新 Sample：`story.ink` 加入 `bg/bg m/se/shake` 等 Tag 範例（目前先不做資源綁定）
- 修正 USS 規範：移除 `VNPlayer.uss` 的 `z-index`（UI Toolkit USS 不支援），避免 Console 警告
- 已驗證：你已在 Unity Play Mode 測過（回看 / Auto / Skip / 隱藏 UI）

## 2026-02-06（狀態同步）
- 已把本檔「承接任務」狀態同步到最新：
  - Phase 4：完成
  - Phase 5：核心完成，剩多槽 + Auto 槽 UI
  - Phase 6：尚未開始
  - Phase 7：Runtime 驗收完成，開發者模式 demo 待完成
- 備註：本任務本身已結案，後續待辦追蹤改看 `20260120/TextAdventureEngine/task_plan.md`
