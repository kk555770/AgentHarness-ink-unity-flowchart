# 調查發現

## 需求
- 使用者要求開始。
- 本輪正式從文件翻修切到 Batch 0 實作。

## 研究發現
- 本機有 Unity `6000.3.9f1` 可用。
- `Packages/com.opsidanos.ink/Core` 目前不存在。
- `Assets/Editor/Tests/OpsidanosInk.EditModeTests.asmdef` 是最合理的第一批測試掛點。
- Batch 0 應先立：
  - `CanonicalNodeKinds`
  - `CanonicalPortSemantics`
  - `CanonicalGraphInvariant`
- `InkFlowChartNodes.cs` 目前已經有 `start / dialogue / stageAction / comment / choice / condition` 這組命名，可直接作為 Batch 0 最小 core 常數的對位來源。
- 最小 core 測試已可先守：
  - node kind 固定順序
  - Flow / ActionData / ActionIn 命名
  - 單一 Flow 輸出節點判定入口
- Unity batchmode 已成功完成：
  - 新 asmdef 編譯
  - 新測試組譯
  - 新檔案 import 與 `.meta` 生成
- Unity Test Framework `1.6.0` 的 command line test runner 不能搭配 `-quit`；若帶 `-quit`，測試流程不會正確進入 XML 寫檔收尾。
- 改用不帶 `-quit` 的叫法後，CLI 已可穩定輸出正式 XML：
  - `Batch0CoreResults.xml`：7 個 canonical core 測試全部通過
  - `Batch0GateResults.xml`：7 個 canonical core 測試加 1 個 `InkFlowChartGraphSmokeTests` 全部通過
- 目前 Batch 0 最小 gate 已具備可重跑的正式指令形狀：
  - `-assemblyNames OpsidanosInk.EditModeTests`
  - `-testFilter "^OpsidanosInk\\.Tests\\.EditMode\\.Canonical;^OpsidanosInk\\.Tests\\.EditMode\\.InkFlowChartGraphSmokeTests$"`
  - `-runSynchronously`
  - `-testResults <path>`
  - 不加 `-quit`

## 技術判斷
| Decision | Rationale |
|----------|-----------|
| 先做 3 個最小型別 | 足夠建立 core seam，又不會偷跑到 validator / projection |
| 先跑 smoke + core 測試 | 這最符合 Batch 0 gate |
| EditMode 測試 asmdef 直接加新 core reference | 這樣最少改動就能把新測試接進既有測試線 |
| 先解掉 CLI XML 產出問題，再宣告 Batch 0 完成 | Batch 0 是第一個正式 gate，不應只停在 compile/import 成功 |

## 遇到的問題
| Issue | Resolution |
|-------|------------|
| Unity CLI 未在指定位置產出 `testResults` XML | 根因是誤用了 `-quit`；移除 `-quit` 後，Unity test runner 會自行在 `RunFinished` 後存出 XML 並正常結束 |

## 參考資源
- `Documentation/AuthoringPhase1Batch0Kickoff.md`
- `Assets/Editor/Tests/OpsidanosInk.EditModeTests.asmdef`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodes.cs`
- `Packages/com.opsidanos.ink/Core/OpsidanosInk.CanonicalGraph.asmdef`
- `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CanonicalNodeKinds.cs`
- `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CanonicalPortSemantics.cs`
- `Packages/com.opsidanos.ink/Core/Scripts/CanonicalGraph/CanonicalGraphInvariant.cs`
- `Library/PackageCache/com.unity.test-framework@0b7a23ab2e1d/UnityEditor.TestRunner/CommandLineTest/SettingsBuilder.cs`
- `Library/PackageCache/com.unity.test-framework@0b7a23ab2e1d/UnityEditor.TestRunner/CommandLineTest/ResultsSavingCallbacks.cs`
- `Library/PackageCache/com.unity.test-framework@0b7a23ab2e1d/UnityEditor.TestRunner/CommandLineTest/Executer.cs`

## 視覺/瀏覽重點
- 這輪的核心是立骨架與測試掛點，不是搬邏輯。
- CLI gate 的真正坑不是測試壞掉，而是 test runner 結束方式錯了。
