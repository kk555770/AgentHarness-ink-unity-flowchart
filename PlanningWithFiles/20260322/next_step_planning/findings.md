# Findings

## 工作樹現況

- 目前分支仍是 `arcumit/CodexInk`。
- Batch 7 仍未提交，工作樹包含：
  - `InkFlowChartImporter.cs`
  - `CurrentFlowImportService.cs`
  - `CurrentFlowCanonicalImportServiceTests.cs`
  - Batch 7 XML 測試報表
  - `PlanningWithFiles/20260321/batch7_importer_canonical_first/`
  - `PlanningWithFiles/20260321/repo_refresh_docs_goals_next_step/`

## 文件北極星

- 文件方向一致：
  - 真相在 `canonical graph schema / API / JSON contract`
  - `GraphToolkit` 是 current baseline / migration shell
  - 長期作者工具方向是 `Web-first control surface`
- 文件沒有支持「現在直接做 WebView 成品」。
- 文件更像是在要求：
  - 先把 `control plane / bridge / transport` 立穩
  - 再做 Web-first authoring frontend

## 目前程式碼現況

- exporter 已 canonical-first
- importer 也在 Batch 7 收斂成 canonical-first 入口
- `CurrentFlowProjectionService` 與 `CurrentFlowImportService` 兩邊已經對稱很多
- `GraphToolkit shell` 已經不是最大瓶頸
- 最大缺口變成：
  - `CanonicalGraphApiSpec` 和 `CanonicalGraphJsonContract` 還主要停在文件層
  - `CanonicalGraphCommandService` 雖然已可用，但還沒有正式的 machine-callable JSON bridge / authoring bridge

## 候選方向比較

### 候選 A：繼續切 GraphToolkit shell

- 好處：風險最低
- 壞處：價值明顯下降
- 判斷：不應優先

### 候選 B：直接做 WebView / Web authoring prototype

- 好處：很接近最終目標，看起來也最有感
- 壞處：
  - 目前缺正式 bridge / transport 契約
  - 容易讓前端先長出來，但控制面仍未落地
- 判斷：現在做太早

### 候選 C：做 plain JSON command bridge / authoring bridge kickoff

- 好處：
  - 直接把文件中的 `CanonicalGraphApiSpec` / `CanonicalGraphJsonContract` 變成可呼叫程式
  - AI / Web-first / Playwright 之後都有真正的 control plane 可接
  - 不需要先碰 WebView 外殼
- 壞處：
  - 需要先決定最小 request / response / dispatcher 邊界
- 判斷：目前最值得做

## 推薦結論

- 下一步最合理的是：
  - 先提交 Batch 7
  - 接著做 **Batch 8：plain JSON command bridge / authoring bridge kickoff**
- 這一批應優先把：
  - `CreateGraph`
  - `CreateNode`
  - `ConnectPorts`
  - `ValidateGraph`
  這四個 canonical command 包成正式的 JSON contract adapter / dispatcher
- WebView 應放在 Batch 8 之後，不應先跑到 bridge 前面
