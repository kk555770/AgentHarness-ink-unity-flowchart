# 文件索引

> 最後更新：2026/03/17  
> 目的：給新讀者一張地圖，快速分辨哪份文件在講北極星、哪份在講現況工作流、哪份在講控制面、哪份只是過渡層。

## 0. 先講結論

如果你只想用最短時間搞懂這個 repo，請照這個順序讀：

1. `Documentation/NarrativeGraphArchitecture.md`
2. `Documentation/AuthoringToolStrategy.md`
3. `Documentation/CurrentAuthoringWorkflow.md`
4. `Documentation/AuthoringRefactorBoundaries.md`
5. `Documentation/AuthoringPhase1ImplementationPlan.md`
6. `Documentation/AuthoringPhase1ImplementationProposal.md`
7. `Documentation/AuthoringPhase1Batch0Kickoff.md`
8. `Documentation/CanonicalGraphSchema.md`
9. `Documentation/CanonicalGraphSchemaSpec.md`
10. `Documentation/CanonicalGraphApiSpec.md`
11. `Documentation/CanonicalGraphJsonContract.md`
12. `Documentation/DeveloperModeOutputContract.md`
13. `Packages/com.opsidanos.ink/README.md`

你可以把這 13 份文件想成：

- 前 2 份：告訴你**為什麼**這樣設計
- 第 3 份：告訴你**目前作者工具工作流**怎麼運作
- 第 4 份：告訴你**之後重製要先切哪一刀**
- 第 5 份：告訴你**第一階段實際怎麼動手**
- 第 6 份：告訴你**第一階段要分幾批施工**
- 第 7 份：告訴你**第一批先怎麼開工**
- 中間 4 份：告訴你**真相與控制面**長什麼樣
- 後 2 份：告訴你**目前投影契約與 Unity Runtime**怎麼落地

## 1. 北極星文件

### `Documentation/NarrativeGraphArchitecture.md`

這份文件回答：

- 這個專案真正想做什麼
- canonical schema、projection、runtime adapter、validation loop 怎麼分層
- 為什麼現在 repo 看起來像「到處都有真相」

如果你只能先看一份，先看這份。

### `Documentation/AuthoringToolStrategy.md`

這份文件回答：

- 為什麼作者工具策略正在往 Web-first 收斂
- GraphToolkit 的現況定位是什麼
- WebView / Browser / Electron 各自扮演什麼角色
- 為什麼 AI 接入比較適合 web control surface

### `Documentation/CurrentAuthoringWorkflow.md`

這份文件回答：

- 目前 GraphToolkit / `.inkfc` / `.flowchart.json` / `.ink` / `story.json` 這條線實際怎麼跑
- 為什麼它很重要，但不等於 canonical truth
- 為什麼它更像 current working baseline

### `Documentation/AuthoringRefactorBoundaries.md`

這份文件回答：

- 目前 GraphToolkit 腳本裡哪些責任混在一起
- 哪些要留在 current tooling shell
- 哪些要抽成 canonical core / projection adapter / future bridge
- 重製最值得先切哪一刀

### `Documentation/AuthoringPhase1ImplementationPlan.md`

這份文件回答：

- 第一階段到底先做什麼，不做什麼
- 先改哪些檔，先不動哪些檔
- 要跑哪些測試來守住現有閉環
- 為什麼第一階段應先抽 core seam，而不是先換前端殼

### `Documentation/AuthoringPhase1ImplementationProposal.md`

這份文件回答：

- 第一階段應拆成哪幾批
- 每一批先改哪些檔
- 每一批要跑哪些測試當 gate
- 什麼情況下可以往下一批走

### `Documentation/AuthoringPhase1Batch0Kickoff.md`

這份文件回答：

- Batch 0 第一批先新增哪些檔
- 哪個 asmdef 一定要先改
- 最小 core 測試應先補哪幾支
- Batch 0 完成後，什麼情況才可以進 Batch 1

## 2. 真相層文件

### `Documentation/CanonicalGraphSchema.md`

這份文件回答：

- canonical truth 應該包含什麼
- 哪些東西不應混進 canonical schema

### `Documentation/CanonicalGraphSchemaSpec.md`

這份文件回答：

- graph / node / port / edge 的具體形狀
- `dialogue` / `stageAction` / `choice` / `condition` 等規格
- 目前 sidecar / GraphToolkit mapping 怎麼對照

## 3. 控制面文件

### `Documentation/CanonicalGraphApiSpec.md`

這份文件回答：

- AI / 程式要怎麼操作 canonical graph
- 哪些操作要 deterministic、idempotent

### `Documentation/CanonicalGraphJsonContract.md`

這份文件回答：

- 上面的語意 API 如果包成第一版 Plain JSON，形狀長什麼樣
- 為什麼這不是 `.flowchart.json`
- 為什麼這也不是 JSON-RPC 本體

## 4. 現況工作流文件

### `Documentation/CurrentAuthoringWorkflow.md`

這份文件回答：

- 目前 current GraphToolkit workflow 實際怎麼運作
- `.inkfc`、`.flowchart.json`、`.ink`、`story.json` 各自扮演什麼角色

重要提醒：

- 這份文件講的是 **current authoring workflow**
- 它不等於 canonical truth
- 它也不取代輸出契約

### `Documentation/DeveloperModeOutputContract.md`

這份文件回答：

- 目前 current Flow projection 輸出到 Ink / Runtime 時，哪些輸出才合法
- `char` JSON、`transition.steps`、Restore / Rollback 等 current contract

重要提醒：

- 這份文件很重要
- 但它講的是 **current projection contract**
- 不要把它誤讀成 canonical truth 本體

### `Packages/com.opsidanos.ink/README.md`

這份文件回答：

- 目前 Unity Runtime 要怎麼接
- `InkStoryEngine`、`VNPlayerPresenter`、`InkSaveSystem`、`InkResourceMap` 怎麼掛

重要提醒：

- 這份文件是 **Runtime 使用手冊**
- 不等於整個 repo 的架構總覽

## 5. API / 工具規範護欄

### `GraphToolkitSpec.md`

這份文件回答：

- GraphToolkit API 與 Editor 工具開發規範

### `UIToolkitSpec.md`

這份文件回答：

- UI Toolkit API 與 UI 結構規範

這兩份文件比較像：

- 「你要動手做時，不要踩 API 邊界」

而不是：

- 「這個專案的最終真相是什麼」

## 6. 最容易混淆的 9 件事

1. `.flowchart.json` 很重要，但它是 **current sidecar / projection format**，不是 canonical truth。
2. `Packages/com.opsidanos.ink/README.md` 很實用，但它講的是 **目前 Unity Runtime 用法**，不是作者工具策略。
3. `GraphToolkit` 現在仍是重要工作流，但它是 **現況工具外殼**，不是未來唯一長期平台。
4. `CanonicalGraphJsonContract.md` 是 **控制面 wire contract**，不是 `.flowchart.json` sidecar。
5. `CurrentAuthoringWorkflow.md` 講的是 **現在怎麼工作**；`AuthoringToolStrategy.md` 講的是 **未來要往哪裡去**。
6. `AuthoringRefactorBoundaries.md` 講的是 **從現在走到未來時，先切哪一刀**。
7. `AuthoringPhase1ImplementationPlan.md` 講的是 **第一階段實際要怎麼動手**，不是新的方向文件。
8. `AuthoringPhase1ImplementationProposal.md` 講的是 **第一階段分批怎麼施工**，不是另一份 architecture。
9. `AuthoringPhase1Batch0Kickoff.md` 講的是 **第一批現在就先做什麼**，不是開始進 Batch 1。

## 7. 一句總結

這個 repo 最重要的閱讀原則是：

> **先分清楚北極星、真相層、控制面、現況投影與 Runtime 手冊，再開始看程式碼。**
