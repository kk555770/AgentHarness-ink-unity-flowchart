# 任務計畫：Batch 5 canonical projection bridge 提案

## 任務目標

在 Batch 4 已建立 `CanonicalGraphDocument` 與 `CanonicalGraphCommandService` 最小骨架後，
判斷下一步最值得先做的 bridge seam，
讓 current projection 與 GraphToolkit baseline 開始真正碰到 canonical document，
而不是繼續停在 shell 薄化。

## 任務範圍

- 盤點 Batch 4 後的主要缺口
- 比較幾個候選下一步
- 提出最小可落地的 Batch 5 提案
- 整理建議檔案、驗證方式與風險

## 階段

- [x] 重讀全域與 repo `AGENTS.md`
- [x] 確認目前工作樹狀態
- [x] 盤點 Batch 4 implementation 與相關文件
- [x] 比較下一步候選 seam
- [x] 收斂成單一提案

## 目前判斷

1. `GraphToolkit shell` 已不是最大缺口，現在真正缺的是 `canonical document <-> current projection` 的橋。
2. 若直接做 WebView，會跳過中間最關鍵的可驗證 bridge 層。
3. 若直接讓 GraphToolkit UI 手勢去操作 `CanonicalGraphCommandService`，風險較高，因為目前 GraphToolkit editor internal model 仍大量存在。
4. 最保守、最值回票價的下一步，是先做：
   - `CanonicalGraphDocument -> ExportGraphDto`
   - `ExportGraphDto -> CanonicalGraphDocument`
   的 shared adapter / bridge。

## 預計提案方向

- 先建立 current projection 與 canonical document 的雙向 adapter
- 先補純資料測試與既有 projection / round-trip gate
- 之後再讓 exporter / importer 漸進改接 shared bridge
