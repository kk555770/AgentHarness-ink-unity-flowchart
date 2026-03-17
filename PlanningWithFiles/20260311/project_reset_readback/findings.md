# 調查發現

## 需求
- 使用者要我先把「文件定下的新方向」與「真實腳本現況」都讀清楚，再做回報。
- 本輪不修改正式腳本，只做調查、盤點、整理。
- 腳本雖然尚未依新方向調整，但仍要整體閱讀一遍。

## 研究發現
- 專案根目錄已有 `AGENTS.md`，其中明確規定未經同意前僅能調查、讀取、搜尋、測試，以及更新 `PlanningWithFiles` 例外文件。
- 專案中已有一組先前的 `PlanningWithFiles/20260306/project_explanation_with_tools/` 文件，可作為前次討論脈絡的入口。
- 專案另有 `UIToolkitSpec.md`、`GraphToolkitSpec.md` 與 `Documentation/DeveloperModeOutputContract.md`，後續若涉及 UI Toolkit、Graph Toolkit 或開發者模式輸出，就需要一起參照。
- `README.md` 已明確說明：這個 repo 到 2026/03/11 為止，已不是單純上游套件鏡像，而是「`OpsidanosInk` Runtime + Graph 作者工具 + 輸出契約 + 驗證閉環」的工作倉庫。
- `Documentation/NarrativeGraphArchitecture.md` 把北極星寫得很硬：唯一語意真相應該放在 `Graph JSON / Schema / API`，GraphToolkit 是視覺化作者外殼，Unity 是 Runtime Adapter 與驗證環境，不應被當成真相本體。
- `Documentation/CanonicalGraphSchemaSpec.md` 明確把 canonical 與現況實作分開：文件優先於舊 script，canonical node type 已正式採用 `dialogue`，而不是 legacy `action`；`stageAction` 也已被獨立出來。
- `Documentation/CanonicalGraphJsonContract.md` 顯示未來 AI／程式控制面會走 machine-callable 的 plain JSON contract，而不是直接靠 Unity 視窗操作或直接把 sidecar 當 API。
- `Documentation/DeveloperModeOutputContract.md` 的重心不是「怎麼匯出就好」，而是保證輸出可重播、可 Restore、可 Rollback，而且 Runtime 不應靠補洞或紅字 Error 來維持正常流程。
- 舊的 `PlanningWithFiles/20260306/project_explanation_with_tools/` 與今天的正式文件群方向一致，核心都指向：canonical schema 才是北極星，Unity / GraphToolkit / sidecar 都是投影或 adapter。
- 目前程式真正成熟的不是 canonical API，而是兩條工作閉環：
  - Runtime 閉環：`story.json -> StoryOutput -> Tag Router -> Presentation / UI -> Save / Load / Rollback`
  - Authoring 閉環：`.inkfc -> .flowchart.json + .ink -> 匯回 .inkfc`
- `Packages/com.opsidanos.ink/Runtime/Scripts/Story/` 很集中，核心只有 `InkStoryEngine`、`StoryOutput`、`InkTagParser`、`InkTagEventRouter` 幾個檔案，說明故事主線目前是清楚且偏實用導向的。
- `VNPlayerPresenter.cs` 已不是最小 Demo，而是把 backlog、auto、skip、hide UI、打字機、busy/force-complete、多槽存讀、倒帶按鈕都串進同一個 Presenter。
- `InkSaveSystem.cs` 目前已能保存 `Ink state json`、當句輸出與 presentation snapshot，支援 rollback buffer、手動 1~3 槽與 auto 槽；但看起來仍是執行期記憶體層級，不是完整持久化方案。
- `InkTagCharacterStatePlayer.cs` 代表新式 `char:{json}` 狀態化演出已落地，且支援 transition/steps/raise/restore；但舊的 `char-left/center/right` 路線也還在，顯示角色系統仍處於搬遷過渡期。
- `Assets/Editor/FlowChart/GraphToolkit/` 目前只有 6 個核心檔，主線非常明確：節點定義、graph 資產、匯出、匯入、sidecar DTO、style bootstrap。
- `InkFlowChartNodes.cs` 內已出現 `CanonicalNodeTypeDialogue` 與 `LegacySidecarDialogueNodeType` 這種集中 mapping，表示文件裁決已開始往程式碼下沉，但程式世界仍保留 legacy 相容層。
- `ExportGraphDto` 仍以 `nodes[*].outputs[]` 為主要 sidecar 結構，`.flowchart.json` 仍是當前 round-trip 的工作核心；這和 canonical schema 規劃中的獨立 edge 一級語意仍有距離。
- 測試分布也支持同一判讀：EditMode 偏 Graph 匯入匯出、round-trip、SaveData、CharTag JSON；PlayMode 偏 UI 點擊節奏、播放流程、槽位操作。也就是說，現在測的仍是過渡期工作閉環，而不是 canonical control plane。

## 技術判斷
| Decision | Rationale |
|----------|-----------|
| 先分兩條線閱讀：規劃文件、實際腳本 | 這樣最後比對落差會更清楚 |
| 先盤點資料夾與文件，再深入核心腳本 | 先看地圖，再看房間，較不容易迷路 |
| 以文件定義的新架構當「目標真相」，再拿腳本去比對 | 使用者已明說腳本尚未跟上，這樣讀才不會把現況誤當目標 |
| 把 Runtime 閉環與 Authoring 閉環分開描述 | 目前專案的成熟度就是分別長在這兩條線上 |

## 遇到的問題
| Issue | Resolution |
|-------|------------|
| 尚未遇到阻塞 | 持續調查 |

## 參考資源
- `AGENTS.md`
- `PlanningWithFiles/20260306/project_explanation_with_tools/`
- `UIToolkitSpec.md`
- `GraphToolkitSpec.md`
- `Documentation/DeveloperModeOutputContract.md`
- `Documentation/NarrativeGraphArchitecture.md`
- `Documentation/CanonicalGraphSchema.md`
- `Documentation/CanonicalGraphSchemaSpec.md`
- `Documentation/CanonicalGraphJsonContract.md`
- `README.md`

## 視覺/瀏覽重點
- 目前 `PlanningWithFiles` 底下只有一組 `20260306/project_explanation_with_tools` 舊紀錄。
- 專案主要結構集中在 `Assets/`、`Packages/`、`Documentation/` 與既有規格文件。
- 文件群已經形成一條很明確的閱讀鏈：架構總覽 → canonical schema → API / JSON contract → 開發者輸出契約 → runtime 與作者工具實作。
- 腳本群也形成兩條明顯主線：
  - `Packages/com.opsidanos.ink/Runtime/Scripts/`：玩家模式與演出
  - `Assets/Editor/FlowChart/GraphToolkit/`：作者工具與 sidecar round-trip
