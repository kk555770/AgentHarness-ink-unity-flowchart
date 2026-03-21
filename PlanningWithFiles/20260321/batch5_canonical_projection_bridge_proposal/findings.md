# Findings

## 2026/03/21

- Batch 4 已經讓 repo 具備最小 canonical graph 控制面：
  - `CanonicalGraphDocument`
  - `CanonicalGraphCommandService`
  - `CreateGraph / CreateNode / ConnectPorts / ValidateGraph`
- 目前 exporter / importer / round-trip 真正共用的 still-shared contract 仍是 `ExportGraphDto`。
- `CurrentFlowProjectionService` 目前直接吃 `ExportGraphDto`，還沒有 canonical document 版本的 `ProjectGraph`。
- `CurrentFlowGraphToolkitImportAdapter` 目前吃的是 `CurrentFlowImportPlan`，還不是 canonical document。
- 這代表 canonical core 已經出現，但 current working line 還沒有真的踩到它。

## 候選比較

### 候選 A：繼續切 GraphToolkit shell

- 優點：風險低
- 缺點：收益開始變小
- 判斷：不再是最值得先做的下一步

### 候選 B：直接做 WebView / Browser bridge

- 優點：方向感強
- 缺點：中間缺 bridge 層，容易變成用新前端硬接舊 projection
- 判斷：太早

### 候選 C：直接讓 GraphToolkit 建圖過程呼叫 `CanonicalGraphCommandService`

- 優點：看起來最接近「真 control plane」
- 缺點：GraphToolkit 仍高度依賴 editor internal model，第一刀風險偏高
- 判斷：可以做，但不應是 Batch 5 第一刀

### 候選 D：建立 `CanonicalGraphDocument <-> ExportGraphDto` bridge

- 優點：
  - 正好補上 Batch 4 與 Batch 2 之間的斷層
  - 純資料層，風險比直接碰 GraphToolkit UI 低
  - 可先用單元測試守住 deterministic 與 mapping
- 缺點：
  - 這一刀做完後，還需要下一刀把 importer / exporter 改接
- 判斷：最合理的下一步

## 實作後補充

- `CurrentFlowCanonicalGraphAdapter` 已落地。
- exporter 與 importer 都已最小改接：
  - exporter：`ExportGraphDto -> CanonicalGraphDocument -> ExportGraphDto`
  - importer：sidecar 驗證後，先經過 canonical bridge，再回到 normalized DTO
- 這代表 current working line 已經不再完全繞開 canonical core。

## 驗證結論

- `CurrentFlowCanonicalGraphAdapterTests`
  - `4/4 passed`
- `Unity EditMode gate`
  - `276 total / 276 passed / 0 failed / 4 skipped`
  - `OpsidanosInk.EditModeTests.dll`：`62/62 passed`
- `Unity PlayMode gate`
  - `221 total / 130 passed / 0 failed / 91 skipped`
  - `OpsidanosInk.PlayModeTests.dll`：`17/17 passed`

## 剩餘觀察

- 測試會產生 XML 報表暫存。
- `OffMeshLinkScene.unity` 的測試噪音已還原。
- 下一批若再往前走，最合理的是：
  - 把 `CurrentFlowProjectionService` 開始接到 canonical document
  - 或把 importer / exporter 更進一步改成直接吃 shared bridge，而不是只做最小穿越
