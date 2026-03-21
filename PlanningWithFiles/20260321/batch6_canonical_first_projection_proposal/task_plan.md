# 任務計畫：Batch 6 canonical-first projection

## 任務目標

在 Batch 5 已經建立 `CanonicalGraphDocument <-> ExportGraphDto` bridge 後，
讓 current projection 主線正式改成 canonical-first，
並完成最小實作與驗證。

## 任務範圍

- 確認目前工作樹真相與 Batch 5 未提交狀態
- 盤點 `CurrentFlowProjectionService` 與 `InkFlowChartExporter` 的剩餘 DTO-first 耦合
- 比較下一步候選 seam，收斂成單一提案
- 完成最小實作與測試驗證
- 記錄子代理回覆狀態與是否可採用

## 階段

- [x] 重讀全域與 repo `AGENTS.md`
- [x] 確認目前工作樹狀態
- [x] 重讀 `GraphToolkitSpec.md`
- [x] 盤點 `CurrentFlowProjectionService`
- [x] 盤點 `InkFlowChartExporter`
- [x] 嘗試平行調度子代理讀碼
- [x] 收斂 Batch 6 提案
- [x] 實作 canonical-first projection service
- [x] 改接 exporter 走 canonical-first projection
- [x] 跑最小 projection 測試
- [x] 跑完整 EditMode / PlayMode gate
- [x] 還原測試噪音

## 目前判斷

1. Batch 5 已讓 importer / exporter 最小穿過 canonical bridge，但 `.ink` projection 主線仍是 `ExportGraphDto -> BuildInkContent`。
2. `CurrentFlowProjectionService` 目前沒有 `CanonicalGraphDocument -> Ink` 的 public / internal 入口。
3. 下一步若再繼續切 `GraphToolkit shell`，收益已小於讓 projection 主線正式搬家。
4. 最保守且高收益的下一步，是先讓 exporter 改成 canonical-first；importer 先不動。

## 最終實作方向

- 讓 `CurrentFlowProjectionService` 新增 canonical-first 入口
- 讓 exporter 改成：
  - `GraphToolkit graph -> ExportGraphDto`
  - `ExportGraphDto -> CanonicalGraphDocument`
  - `CanonicalGraphDocument -> projection service -> Ink`
- 新增 canonical-first projection 測試
