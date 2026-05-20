# 目前里程碑

> 文件負責人：harness
> 最後更新：2026/05/20
> 角色：**現況進度入口 / Current Status**  
> 目的：讓新讀者先知道 repo 現在做到哪裡，不必先翻整包 `PlanningWithFiles/`

## 0. 先講結論

- 2026/05/20 起，這個 repo 進入**閒置狀態**。
- GitHub Actions 遠端 workflow 已手動停用；本地 workflow source
  也已拿掉 `push`、`pull_request`、`schedule`、`workflow_run`
  這些背景觸發。
- 閒置期間不得自動產生 docs-garden PR 或 Codex auto-fix PR。
  要恢復 CI/CD，必須由使用者明確要求。
- canonical JSON control plane 已不是只停在文件層。
- Batch 8~10 已把最小可讀可寫回圈落到程式與測試。
- Batch 11 / 11A 已把 `ValidateProjection / ProjectGraph` 先接到 `flowchart-json / ink`，並補上 request / response / dispatcher 護欄。
- `ImportProjection(flowchart-json)` 也已進 control-plane 與正式 system-of-record。
- 目前**還沒有**：
  - `ImportProjection(ink)` 與其他 target 的 import loop
  - `story-json` target
  - `graphtoolkit-model` target
  - WebView / Electron host 成品

## 1. 已交付里程碑

### Batch 8：plain JSON command bridge

已交付最小寫入入口：

- `CreateGraph`
- `CreateNode`
- `ConnectPorts`
- `ValidateGraph`

定位：

- 先把 canonical graph 從「只有文件」變成「可呼叫的最小 JSON control plane」
- 不先做 WebView，不先做一般化動態 payload parser

## Batch 9：GetGraph snapshot bridge

已交付最小讀取入口：

- `GetGraph`

定位：

- 讓 control plane 從只能下指令，進化成最小可讀可寫回圈
- snapshot 直接鏡射 canonical graph，不混成 projection API

## Batch 10：mutation bridge

已交付最小可編輯 mutation：

- `ReplaceNodePayload`
- `DisconnectEdge`
- `RemoveNode`

定位：

- 補齊「能改 / 能拆 / 能刪」的作者工具寫入面
- 仍不先做 Web host

## Batch 11 / 11A：projection-specific control plane

repo 內目前已可見：

- `ValidateProjection`
- `ProjectGraph`

目前先支援 target：

- `flowchart-json`
- `ink`

這一批補上的重點不是前端殼，而是：

- target-based projection service
- JSON dispatcher routing
- request / response contract round-trip 護欄
- unsupported target 的固定錯誤碼

## 2. 目前 focus

現在比較合理的理解是：

- 專案暫停主動推進，先保持現況，不跑背景 CI/CD。
- canonical graph / JSON control plane 已經成形
- current GraphToolkit workflow 仍是 baseline
- projection-specific control plane 正在收穩
- 下一步不應誤讀成「直接跳去做 WebView 成品」

## 3. 不要誤讀的事

1. `Web-first` 是長期方向，不等於 repo 已有 Web host 成品。
2. `PlanningWithFiles/` 很重要，但它是工作記錄，不是長期正式入口。
3. Batch 11 / 11A 的重點是 projection contract 與 host bridge 護欄，不是 UI 外殼。
4. `ImportProjection` 已先完成 `flowchart-json` control-plane 最小閉環；`ink` import、`story-json`、`graphtoolkit-model` 目前仍屬未完成項。
