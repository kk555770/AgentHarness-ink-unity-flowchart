# 進度日誌：Restore 回歸與範圍偏移稽核

## 2026-02-11

### 階段 1：範圍與版本基線
- 確認工作樹只有一個未提交檔案：
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/InkTagCharacterStatePlayer.cs`
- 確認目前 `HEAD`：`dc8a443`
- 搜索關鍵字：`Restore_缺少Appear`、`BuildTransitionSchedule`、`char.transition.steps`

### 階段 2：失敗測試與改動對照
- 讀取測試：`Assets/Tests/PlayMode/OpsidanosInkPlayModeTests.cs:205`
- 讀取 runtime 差異：
  - `git diff -- Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/InkTagCharacterStatePlayer.cs`
- 逐行確認改動內容：
  - Restore 改 `LogError -> Log`
  - `missing Appear` 在 Restore 下改 `Insert(0, ...)`

### 階段 3：歷史追因
- `git log -- <runtime-file>`：只有 `74c26b4/6e16382/b3d9090`
- `git log -- Assets/Tests/PlayMode/OpsidanosInkPlayModeTests.cs`：含 `896d997`
- `git show --name-only 896d997`：有 PlayMode 測試檔，無 runtime presentation 檔
- 找到關鍵記錄：
  - `PlanningWithFiles/20260206/phase5_phase6_unified/progress.md:59`
  - 內容：`已還原 InkTagCharacterStatePlayer.cs`

### 階段 4：當下驗證
- Unity MCP 跑單測：
  - `OpsidanosInk.Tests.OpsidanosInkPlayModeTests.Restore_缺少Appear時_不應產生CharTransitionStepsError`
  - 結果：1/1 pass（job `e98b0a8d1f7d4700860c62d5f3906578`）
- Console 讀取：
  - warning/error：0
  - `char.transition.steps` 只剩一般 Log（非 Error）

### 階段 5：交付整理
- 建立本次任務三檔：`task_plan.md`、`findings.md`、`progress.md`
- 準備對使用者交付：
  - 錯誤點（流程/程式/版本）
  - 改動點（到底修了什麼）
  - 原始對話設計資料鏈
  - 已確認風險與決策分歧

### 階段 6：補查既定對話流程（你指出的缺口）
- 重新掃描 `PlanningWithFiles` 全歷史中與對話流程相關條文（task/findings/progress 三類）。
- 逐檔抽出規格主線：
  - `20260120` 玩家模式總規格與 `PresentationState` 原則
  - `20260125` 兩段式點擊（Busy/ForceComplete/Continue）
  - `20260126` 打字機 + Auto 1 秒
  - `20260130` Save/Load/Rollback
  - `20260201` Restore 層級相反（含 ForceComplete）
  - `20260204` PlayMode 驗收清單
  - `20260205` UI 點擊測試
  - `20260206` 主計畫進程與 `InkTagCharacterStatePlayer.cs` 還原記錄
- 已把這條完整流程回填到 `findings.md` 新增章節（第 7 章）。

### 階段 7：補查場景資料鏈（Unity MCP 實測）
- 讀取 active scene：`Assets/Scene/Test.unity`。
- 透過 `find_gameobjects` + MCP resource 讀 `VNPlayer` 的元件清單與 components 屬性。
- 已確認 Runtime 實掛載鏈：
  - `InkStoryEngine`、`VNPlayerPresenter`、`InkTagEventRouter`、`InkTagCharacterStatePlayer`、`InkSaveSystem`。
- 已確認場景同時存在：
  - `InkTagCharacterPlayer`（舊式）
  - `InkTagCharacterStatePlayer`（新式）
- 這個「雙播放器共存」風險已補寫到 `findings.md` 第 8 章。

## 本輪錯誤紀錄
| 時間 | 錯誤 | 嘗試 | 解法 |
|---|---|---|---|
| 2026-02-11 | 讀錯 `InkTagEventRouter.cs` 路徑（先查到 `Runtime/Scripts/UI`） | 1 | 改用 `rg --files` 找到正確路徑 `Runtime/Scripts/Story/InkTagEventRouter.cs` |
| 2026-02-11 | `git show dc8a443 -- <runtime-file>` 沒內容易誤判 | 1 | 補 `git log -- <file>` 與 `git show <commit>:<file>` 交叉確認 |
| 2026-02-11 | `rg` 使用 `--glob \"*/task_plan.md\"` 沒命中 | 1 | 改成 `--glob \"**/task_plan.md\"` 重新掃描 |
| 2026-02-11 | 誤用 `manage_gameobject action=get_components`（該工具不支援） | 1 | 改用 `find_gameobjects` + `read_mcp_resource(mcpforunity://scene/gameobject/{id}/components)` |
