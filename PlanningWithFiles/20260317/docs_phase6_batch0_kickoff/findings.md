# 調查發現

## 需求
- 使用者已同意繼續。
- 本輪持續只改文件，不改正式腳本。
- 目標是把 Batch 0 收成真正可開工的清單。

## 研究發現
- `Documentation/AuthoringPhase1ImplementationProposal.md` 已定義 Batch 0 要做「core 骨架 + gate」，但還沒細到檔案與 asmdef 層級。
- 現有 Editor 測試組件是 `Assets/Editor/Tests/OpsidanosInk.EditModeTests.asmdef`。
- 現有 Graph editor 組件是 `Assets/Editor/FlowChart/OpsidanosInk.FlowChartEditor.asmdef`。
- `OpsidanosInk.EditModeTests.asmdef` 目前已參考：
  - `OpsidanosInk.Runtime`
  - `OpsidanosInk.FlowChartEditor`
  - `Ink-Libraries`
  - `Unity.GraphToolkit.*`
- 這代表如果 Batch 0 要新增 core 測試，最務實的第一步很可能是：
  - 新增 `OpsidanosInk.CanonicalGraph.asmdef`
  - 更新 `OpsidanosInk.EditModeTests.asmdef` 讓它能直接參考新 core
- `OpsidanosInk.Runtime.asmdef` 目前只有 Runtime 相關參考，Batch 0 不應碰它。
- Batch 0 最值得先立的 placeholder，不是 validator / projection service，而是：
  - `CanonicalNodeKinds`
  - `CanonicalPortSemantics`
  - `CanonicalGraphInvariant`
- 這代表 Batch 0 文件需要把「先立哪些型別」與「先不要立哪些型別」一起寫清楚。

## 技術判斷
| Decision | Rationale |
|----------|-----------|
| Batch 0 應明寫 asmdef 修改點 | 這是第一批最容易漏掉、但又最影響是否能開工的地方 |
| Batch 0 測試先掛既有 Editor 測試組件 | 比另開一套測試組件更省風險，也更符合「先立 gate」目的 |
| Batch 0 先立小而真的 placeholder | 比先立一堆空殼大類別更能避免責任漂移 |

## 遇到的問題
| Issue | Resolution |
|-------|------------|
| 批次提案還不夠細到 asmdef 與測試檔層級 | 在 Batch 0 開工清單裡補齊實際檔案與最小測試名單 |
| 容易把 Batch 0 誤解成已經開始抽語意 | 在新文件裡明確寫「先立骨架與 gate，不搬邏輯」 |

## 參考資源
- `Documentation/AuthoringPhase1ImplementationProposal.md`
- `Documentation/AuthoringPhase1Batch0Kickoff.md`
- `Assets/Editor/Tests/OpsidanosInk.EditModeTests.asmdef`
- `Assets/Editor/FlowChart/OpsidanosInk.FlowChartEditor.asmdef`
- `Packages/com.opsidanos.ink/Runtime/OpsidanosInk.Runtime.asmdef`

## 視覺/瀏覽重點
- Batch 0 真正重要的不是搬邏輯，而是先把 asmdef 與測試掛點鋪好。
- 這輪的核心交付是：把第一批從「概念」收成「現在就能照著開工的文件」。
