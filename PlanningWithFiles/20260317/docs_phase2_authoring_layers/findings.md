# 調查發現

## 需求
- 使用者已同意進入文件翻修 Phase 2。
- 本輪持續只改文件，不改正式腳本。
- 目標是切開 current GraphToolkit 工作流、current projection contract 與 future Web-first authoring strategy。

## 研究發現
- `AuthoringToolStrategy.md` 已把方向講清楚，但還沒有一份專門承接「現在實際怎麼用 GraphToolkit / sidecar / Ink round-trip」的現況文件。
- `DeveloperModeOutputContract.md` 已明講自己是 current projection contract，但內容仍非常厚，讀者若想找「目前作者工具工作流長什麼樣」還是會直接掉進大契約。
- `README.md` 現在已經有方向導覽，但還少一個專門給 current authoring workflow 的落點可連。
- `NarrativeGraphArchitecture.md` 已能講清楚 why 與四層，但不適合塞太多 current tooling 細節。
- 本輪已新增 `Documentation/CurrentAuthoringWorkflow.md`，把 `.inkfc -> .flowchart.json + .ink -> story.json -> Unity Runtime` 這條 current working line 集中描述。
- `CurrentAuthoringWorkflow.md` 現在把 4 個主要載體切開：
  - `.inkfc`
  - `.flowchart.json`
  - `.ink`
  - `story.json`
- `CurrentAuthoringWorkflow.md` 也明確把自己和 `DeveloperModeOutputContract.md` 的差異切開：
  - 前者是工作流描述
  - 後者是輸出合法性契約
- `DocsIndex.md` 現在把 `CurrentAuthoringWorkflow.md` 插在策略文件與真相層文件之間，讓讀者先理解「現在怎麼工作」，再讀「規則長什麼樣」。
- `AuthoringToolStrategy.md` 現在不再負責 current GraphToolkit workflow 的逐步細節，而是專注在方向與責任切分。
- `NarrativeGraphArchitecture.md` 已補上 `CurrentAuthoringWorkflow.md` 作為文件關係中的現況工作流文件。

## 技術判斷
| Decision | Rationale |
|----------|-----------|
| 新增 `CurrentAuthoringWorkflow.md` | 把 current GraphToolkit / sidecar / ink round-trip 集中收納 |
| 讓 `AuthoringToolStrategy.md` 保持偏方向層 | 避免策略文件又被現況細節拖回去 |
| 讓 `DeveloperModeOutputContract.md` 持續只講 current projection contract | 不再替作者工具總覽背鍋 |
| 把 `CurrentAuthoringWorkflow.md` 掛進 README、索引、策略與契約 | 讓讀者從不同入口進來都能找到正確落點 |

## 遇到的問題
| Issue | Resolution |
|-------|------------|
| 尚未遇到阻塞 | 持續實作 |

## 參考資源
- `Documentation/DocsIndex.md`
- `Documentation/AuthoringToolStrategy.md`
- `Documentation/DeveloperModeOutputContract.md`
- `Documentation/NarrativeGraphArchitecture.md`
- `README.md`

## 視覺/瀏覽重點
- 現在缺的不是新的大方向，而是一份「current authoring workflow」中繼文件。
- 這個缺口已補上，現在三層已能明確分開：
  - `AuthoringToolStrategy.md`：未來方向
  - `CurrentAuthoringWorkflow.md`：現在怎麼工作
  - `DeveloperModeOutputContract.md`：現在輸出怎樣才合法
