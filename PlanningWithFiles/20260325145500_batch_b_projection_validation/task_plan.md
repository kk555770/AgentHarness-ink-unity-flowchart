# 任務計畫：batch_b_projection_validation

## 目標

- 針對 `ValidateProjection(Ink)` 與 `ProjectGraph(Ink)` 補齊 compile-level 驗證。
- 保持 canonical core 不直接依賴 editor-only `Ink.Compiler`。
- 讓 JSON control plane 與 editor 測試都能重用同一條 probe 路徑。

## 執行順序

1. 在 `Packages/com.opsidanos.ink/Core/Scripts/Projection/CurrentFlow/` 建立可注入的 compile probe registry。
2. 在 editor-only 程式碼加入 bootstrap，於 Editor domain 自動掛上真實 `Ink.Compiler` probe。
3. 修改 `CurrentFlowProjectionService`，讓 `InkTarget` 的 validate / project 都先經過 probe 編譯。
4. 增加 regression harness，鎖住：
   - validation 真的有走 probe
   - project 真的會回傳可編譯的 Ink
   - 失敗時會回報 compile failure
5. 重新跑 repo guard 與 Unity tests，確認沒有破壞 canonical/editor 分層。

## 狀態

| 階段 | 狀態 | 說明 |
| --- | --- | --- |
| 1 | 已完成 | 盤點 current-flow projection service 與現有測試 |
| 2 | 已完成 | 決定以 core registry + editor bootstrap 的方式接線 |
| 3 | 已完成 | 實作 probe registry、service 驗證、editor bootstrap、regression tests |
| 4 | 已完成 | 驗證 `repo_guard`、`git diff --check`、`Tools/run_tests.sh` |

## 交付物

- `Packages/com.opsidanos.ink/Core/Scripts/Projection/CurrentFlow/ICurrentFlowInkCompileProbe.cs`
- `Packages/com.opsidanos.ink/Core/Scripts/Projection/CurrentFlow/CurrentFlowInkCompileProbeRegistry.cs`
- `Packages/com.opsidanos.ink/Core/Scripts/Projection/CurrentFlow/CurrentFlowProjectionService.cs`
- `Assets/Editor/CurrentFlowInkCompileProbeBootstrap.cs`
- `Assets/Editor/Tests/CurrentFlowInkProjectionCompileHarnessTests.cs`

## 驗證

- `python3 Tools/repo_guard.py`
- `git diff --check`
- `Tools/run_tests.sh`
