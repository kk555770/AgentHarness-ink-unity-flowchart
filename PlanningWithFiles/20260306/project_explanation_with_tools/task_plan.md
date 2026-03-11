# 專案觀察與說明計畫

## 目標
- 透過實際閱讀專案文件、歷史規劃與核心程式碼，整理出這個 Unity + Ink 專案的用途、架構、工作流程與目前成熟度。
- 把「目前觀察到的實作狀態」與「使用者澄清的真正架構北極星」分開記錄，避免把過渡期驗證手段誤判成最終設計。

## 階段
| 階段 | 狀態 | 說明 |
| --- | --- | --- |
| 1 | 已完成 | 建立本次規劃檔案並確認調查範圍 |
| 2 | 已完成 | 閱讀專案文件、規格與歷史 PlanningWithFiles |
| 3 | 已完成 | 抽樣閱讀核心 Runtime / Editor / Tests 程式碼 |
| 4 | 已完成 | 整理成對使用者清楚可讀的說明 |
| 5 | 已完成 | 補記使用者澄清的 Canonical Schema 架構意圖與驗證回路 |
| 6 | 已完成 | 新增正式架構文件到 `Documentation/` |
| 7 | 已完成 | 提出下一步正式文件收斂提案，優先處理架構稿與輸出契約的語意邊界 |
| 8 | 已完成 | 新增 Canonical Graph Schema 文件並對齊架構稿與輸出契約 |
| 9 | 已完成 | 補 Canonical Graph Schema Spec，正式定義 node / port / edge / mapping 規則 |
| 10 | 已完成 | 補 Canonical Graph API Spec，正式定義 AI / 程式控制操作面 |
| 11 | 已完成 | 重新盤點全部設計規範，確認先以 Plain JSON contract 作為第一個正式 wire contract，並保留 JSON-RPC 為後續 adapter 路線 |
| 12 | 待提案 | 把 Plain JSON Contract 再往下補成 graph snapshot / result payload / error details 的細部規格 |
| 13 | 已完成 | 透過既有 PlanningWithFiles 重新完整盤點目前文件、核心模組、測試與專案結構，驗證目前理解是否仍一致 |
| 14 | 已完成 | 已先處理 `dialogue` / legacy `action` 的文件接縫，明確切開 canonical 命名、current sidecar token 與 `stageAction` |
| 15 | 已完成 | 已補 `character / castBundle` 的正式定位，先標成 projection-heavy / authoring data-source nodes，暫不升格 canonical core |
| 16 | 已完成 | 已把 `dialogue <-> legacy action` mapping 收斂到 `InkFlowNodeSchema` 單一 helper / 常數層，並通過 GraphToolkit 安全測試 |
| 17 | 已完成 | 已更新根 README 的 repo 定位與閱讀順序，先把新讀者導到 Architecture / Schema / Contract / Runtime 正確入口 |
| 18 | 待提案 | 評估是否要新增短版 onboarding / 專案導讀文件，讓 README 只負責入口而不是承擔完整導覽 |

## 已知限制
- 這次任務以觀察與解釋為主，不修改產品程式碼。
- 需要優先參考既有 PlanningWithFiles，避免忽略歷史結論。

## 待回答問題
- 專案的核心產品是什麼？
- Runtime、Editor、Graph/FlowChart 分別扮演什麼角色？
- 目前專案完成到什麼程度？有沒有明顯重心？
- 使用者真正想要的「Schema / Projection / Runtime Adapter / Validation Loop」四層關係是什麼？
- 下一步應優先新增哪份正式文件，才能最少改動地收斂目前文件衝突？
- 若先試 JSON-RPC，應把它定義成 canonical control plane 的預設 JSON contract，還是 transport adapter？
- 目前 repo 在 2026-03-06 這個時間點，文件、程式碼、測試、樣板與規格之間是否仍維持同一套敘事？
- 哪些部分已明確成熟，哪些部分仍屬過渡或收斂中？

## 本輪結論
- 到 2026-03-06 為止，repo 的文件、程式碼、測試、樣板與 Demo 資產，整體上仍維持同一套敘事，沒有看出主線翻盤。
- 真正產品主體仍是 `Packages/com.opsidanos.ink` 的 Runtime + 演出 + 存讀檔 + UI 框架；根 README 仍偏上游 `ink-unity-integration` 歷史定位。
- `Assets/Editor/FlowChart/GraphToolkit` 仍是重要作者工具主線，但較準確的定位是 current projection / authoring workflow，不是最終 canonical truth。
- Runtime 與測試目前成熟度最高的主線，仍是：
  - 統一 `StoryOutput`
  - Tag 事件分流
  - presentation snapshot
  - save/load/rollback restore 收斂
  - UI 點擊節奏與 busy/blocker 控制
- 最像過渡中的地方仍是：
  - 現況 Graph v2 / `.flowchart.json` sidecar 與 canonical schema 的雙軌並存
  - `action` 舊命名仍留在 current implementation / sidecar / exporter-importer
  - root README 與真正產品敘事尚未完全同步
- 若要先做第一個低風險高報酬調整，最適合的切入點是：
  - 先把 `dialogue` 與 legacy `action` 的責任邊界寫死
  - 再決定 `character / castBundle` 是否仍維持 projection-only
  - 最後才收斂到程式碼 mapping 與 README 入口
- 目前這一刀已完成文件收斂；下一個最自然的接續工作，就是把 `character / castBundle` 的定位補成正式裁決。
- `character / castBundle` 的定位已完成第二輪文件收斂；若要再往下做，下一個更值得處理的是程式碼中的 `dialogue <-> action` mapping 收斂，而不是先擴張更多未落地節點。
- `dialogue <-> action` 的程式碼 mapping 已完成收斂；接下來若想再提高協作穩定性，最值得做的是 README 入口整理，而不是先擴張更多 schema 節點。
- 根 README 的入口整理也已完成；目前最值得往下想的，會是是否要再補一份更短的 onboarding / 專案導讀，或維持目前由 README 承擔入口責任。
- 若繼續優化文件入口，下一步最自然的是新增一份短版 onboarding / 專案導讀，讓 README 專注於入口與索引。
