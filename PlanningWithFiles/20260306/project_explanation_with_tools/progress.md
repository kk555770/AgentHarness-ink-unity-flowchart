# 進度紀錄

## 2026-03-06
- 建立本次 `project_explanation_with_tools` 規劃資料夾。
- 已確認需要同時閱讀文件、歷史規劃與核心程式碼，不能只靠專案名稱推測。
- 已完成第一輪文件閱讀：根 README、`com.opsidanos.ink` README、`DeveloperModeOutputContract.md`、`project_status_assessment/findings.md`。
- 第一輪結論：此倉庫表面上是 `ink-unity-integration`，但目前主產品其實是建在其上的 `OpsidanosInk` 視覺小說框架與 Flow Chart 編輯流程。
- 已完成第二輪規格閱讀：`UIToolkitSpec.md`、`GraphToolkitSpec.md`、`package.json`、`Runtime` 檔案樹。
- 第二輪結論：專案把 UI Toolkit 與 Graph Toolkit 視為正式工作流的一部分，不只是零散實驗；`Runtime` 也已按 Story / Presentation / Save / UI 分層。
- 已抽樣閱讀核心程式：`InkStoryEngine`、`InkTagEventRouter`、`InkSaveSystem`、`VNPlayer.uxml`、`InkFlowChartGraph`、`InkFlowChartExporter`、`InkFlowChartImporter`。
- 第三輪結論：專案目前最成熟的是 Runtime 播放鏈，Editor GraphToolkit 已具備 authoring / 匯出 / 匯入骨架，並強調 round-trip 與 restore/rollback 閉環。
- 已收斂三條平行調查結果：Runtime、Editor/GraphToolkit、測試與封裝邊界。
- 已用本地命令再次確認 asmdef 邊界與測試數量（EditMode 24、PlayMode 17、合計 41）。
- 最終判斷：這是以 `OpsidanosInk` 為主體的 VN 框架專案；Runtime 已可實際遊玩，GraphToolkit 則是正在收斂中的可視化作者工具主線。
- 使用者進一步澄清了真正北極星：
  - `Graph JSON / Schema / API` 才是唯一語意真相
  - `GraphToolkit` 必須完整表現真相，但不是架構中心
  - 最終 VN 執行不應依賴 Unity 才能成立
  - 現在看起來多處都有「真相」，其實是分段開發時為了視覺檢驗與自動化回歸所建立的工作投影
- 已把上述澄清整理成 `Canonical Schema / Projection / Runtime Adapter / Validation Loop` 四層描述，補記到本次 `findings.md` 與 `task_plan.md`。
- 已確認正式文件現況：`Documentation/` 沒有架構總覽文件，新增一份架構說明文件是合理方案。
- 使用者已同意提案。
- 已新增正式架構文件：`Documentation/NarrativeGraphArchitecture.md`。
- 新文件已整理：
  - canonical schema 是唯一真相
  - GraphToolkit / Ink / Unity 都是投影或 adapter
  - 為什麼 repo 現在看起來像多重真相
  - 為什麼 AI 正確的控制面應該是 schema / API，而不是 Editor 手勢
