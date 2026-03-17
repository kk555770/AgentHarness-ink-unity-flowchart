# 調查發現

## 需求
- 使用者已同意繼續下一輪文件翻修。
- 本輪持續只改文件，不改正式腳本。
- 目標是把第一階段實作計畫，整理成逐批次可執行提案。

## 研究發現
- `Documentation/AuthoringPhase1ImplementationPlan.md` 已經整理出第一階段的目標、優先檔案、暫不動清單與測試群。
- 現在缺的不是更多方向，而是：
  - 每一批先改哪些檔
  - 每一批交付什麼結果
  - 每一批跑哪些測試當 gate
- `Assets/Editor/Tests/InkFlowChartExportTests.cs` 已明確守住：
  - `.inkfc -> .ink + .flowchart.json`
  - 匯出錯誤訊息
  - Ink 編譯合法性
- 這代表逐批次提案可以直接把現有測試當 gate，而不是只寫「之後再測」。
- `Assets/Editor/Tests/InkFlowChartImportTests.cs` 已守住 current projection 回建 `.inkfc` 的成功與失敗分支，適合作為 Batch 2 之後的主要 gate。
- `Assets/Editor/Tests/InkFlowChartRoundTripTests.cs` 已守住 Graph v2 的可逆閉環，適合作為 Batch 2 的 stop line。
- `Assets/Tests/PlayMode/OpsidanosInkPlayModeTests.cs` 已守住 Save / Load / Rollback 與 Restore 行為，適合作為 Batch 3 的最後 gate。

## 技術判斷
| Decision | Rationale |
|----------|-----------|
| 新增逐批次 implementation proposal 文件 | 讓第一階段從「清單」變成「可執行順序」 |
| 每批都要有 gate 測試 | 避免一次改太大，卻不知道是哪一批把閉環打壞 |
| 批次從 Batch 0 到 Batch 3 | 先立骨架，再抽語意，再抽 validator/projection，最後才薄化 shell |
| Batch 3 才跑完整 PlayMode gate | 避免每批都把 Runtime 測試成本扛滿，但在第一階段收尾前仍把主閉環驗完 |

## 遇到的問題
| Issue | Resolution |
|-------|------------|
| 第一階段計畫仍偏大顆粒 | 再往下切成批次與 gate，讓真正實作時不會失速 |
| 容易把 implementation plan 與 batch proposal 混成同一份文件 | 讓前者負責範圍，後者負責批次順序與 stop line |

## 參考資源
- `Documentation/AuthoringPhase1ImplementationPlan.md`
- `Documentation/AuthoringRefactorBoundaries.md`
- `Assets/Editor/Tests/InkFlowChartExportTests.cs`
- `Assets/Editor/Tests/InkFlowChartImportTests.cs`
- `Assets/Editor/Tests/InkFlowChartRoundTripTests.cs`
- `Assets/Tests/PlayMode/OpsidanosInkPlayModeTests.cs`

## 視覺/瀏覽重點
- 這輪要交付的是「施工順序表」，不是新的藍圖。
- 這輪最重要的收斂是：每一批都要有 gate，沒過就不要往下一批。
