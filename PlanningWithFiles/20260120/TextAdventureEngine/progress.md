# 進度紀錄（v1）

## Session：2026-01-20 ～ 2026-01-22

### Phase 1：依新規定重寫計畫
- **Status:** complete
- **Started:** 2026-01-20
- Actions taken:
  - 你要求「依新規定」重寫計畫，並表示會把先前新增內容回退
  - 重新建立 planning-with-files 的三個檔案（本檔案所在資料夾）
  - 把核心管線改成：Flow Chart（Editor）→ 輸出 `.ink` → Ink Unity 編譯 `.json` → 玩家模式（Runtime）讀 `.json`
  - 依你的要求把「同樣需求下的優化原則」補進 v1 計畫（sidecar、Tag 字典、`PresentationState`、最小節點集合、只提示不自動修）
- Files created/modified:
  - `PlanningWithFiles/20260120/TextAdventureEngine/task_plan.md`（created）
  - `PlanningWithFiles/20260120/TextAdventureEngine/findings.md`（created）
  - `PlanningWithFiles/20260120/TextAdventureEngine/progress.md`（created）

### Phase 2：回退後盤點與基準確認
- **Status:** complete
- **Started:** 2026-01-21
- Actions taken:
  - 你已把「舊的代碼」回退到最原始版本（以你回退後的 repo 狀態為準重新盤點）
  - 我在本機看到 `Packages/` 只剩 `Ink/`（我們自家的 UPM package 需要重新建立才會出現）
- Files created/modified:
  - `PlanningWithFiles/20260120/TextAdventureEngine/task_plan.md`（updated：切到 Phase 2）
  - `PlanningWithFiles/20260120/TextAdventureEngine/progress.md`（updated：新增 Phase 2 記錄）

### Phase 3：玩家模式（Runtime）MVP
- **Status:** in_progress
- **Started:** 2026-01-21
- Actions taken:
  - 依你同意的方案，開始重建 UPM package（`OpsidanosInk` / `com.opsidanos.ink`）的最小骨架
  - 新增 Runtime：`InkStoryEngine`（讀 `.json` 建立 `Ink.Runtime.Story`，吐出 `StoryOutput`）
  - 新增 UI Toolkit：`VNPlayerPresenter` + `VNPlayer.uxml/.uss`（顯示名字/文字/選項）
  - 補齊玩家模式操作：回看（Backlog）/ Auto / Skip / 隱藏 UI
  - Tag 管線：新增 `InkTag` / `InkTagParser`，並把解析後資料放進 `StoryOutput.ParsedTags`（UI 不再用字串判斷結束）
  - 修正 Unity Console：
    - `CS0118: 'Story' is a namespace but is used like a type`：`InkStoryEngine` 改用 `InkRuntimeStory = Ink.Runtime.Story` alias 避免命名衝突
    - `DirectoryNotFoundException`：修正 `InkEditorUtils.IsInkFile`，遇到資料夾路徑先回傳 false，避免把 `Packages/com.opsidanos.ink` 資料夾誤判成 `.ink` 檔
    - `Samples~` 警告：調整 `.gitignore` 讓 `Packages/**/Samples~` 不再被忽略；移除 `Samples~.meta`；補最小 Sample（`story.ink`）
    - `package.json` JSON 錯誤：修正 `samples` 結尾括號 `}` → `]`
- Files created/modified:
  - `PlanningWithFiles/20260120/TextAdventureEngine/task_plan.md`（updated：切到 Phase 3）
  - `PlanningWithFiles/20260120/TextAdventureEngine/progress.md`（updated：新增 Phase 3 記錄）
  - `Packages/com.opsidanos.ink/package.json`（created）
  - `Packages/com.opsidanos.ink/Runtime/OpsidanosInk.Runtime.asmdef`（created）
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Story/InkStoryEngine.cs`（created → updated：新增 HasEnded/ParsedTags）
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Story/StoryOutput.cs`（created → updated：新增 HasEnded/ParsedTags）
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Story/InkTag.cs`（created）
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Story/InkTagParser.cs`（created）
  - `Packages/com.opsidanos.ink/Runtime/Scripts/UI/VNPlayerPresenter.cs`（created → updated：回看/Auto/Skip/隱藏 UI）
  - `Packages/com.opsidanos.ink/Runtime/UI/UXML/VNPlayer.uxml`（created → updated：TopBar / BacklogPanel / ShowUIButton）
  - `Packages/com.opsidanos.ink/Runtime/UI/USS/VNPlayer.uss`（created → updated：回看/隱藏 UI 樣式）
  - `Packages/com.opsidanos.ink/Samples~/PlayerModeSample/story.ink`（updated：補 Tag 範例）
  - `Packages/Ink/Editor/Core/InkEditorUtils.cs`（updated：避免把資料夾誤判成 .ink）
  - `Packages/com.opsidanos.ink/Samples~.meta`（deleted）
  - `Packages/packages-lock.json`（updated）

## Test Results
| Test | Input | Expected | Actual | Status |
|------|-------|----------|--------|--------|
| JSON 格式檢查 | `python3` 解析 `Packages/com.opsidanos.ink/package.json` / `OpsidanosInk.Runtime.asmdef` | 可被解析 | 可被解析 | ✓ |

## Error Log
| Timestamp | Error | Attempt | Resolution |
|-----------|-------|---------|------------|
| 2026-01-22 | `git restore` 出現 `.git/index.lock` 權限錯誤 | 1 | 改用可寫入 `.git/` 的方式重試後成功 |
| 2026-01-22 | `CS0118: 'Story' is a namespace but is used like a type` | 1 | `InkStoryEngine` 用 alias（`InkRuntimeStory`）避免命名衝突 |
| 2026-01-22 | Ink 自動編譯器把資料夾 `Packages/com.opsidanos.ink` 誤判成 `.ink`，造成 `DirectoryNotFoundException` | 1 | 修正 `InkEditorUtils.IsInkFile`：遇到資料夾路徑先回傳 false |
| 2026-01-22 | `Samples~` 相關警告（`.gitignore` 的 `*~` 造成 Samples~ 沒被版控） | 1 | 調整 `.gitignore` 允許 `Packages/**/Samples~`；移除 `Samples~.meta`；補最小 Sample |
| 2026-01-22 | `package.json is not valid JSON` | 1 | 修正 `samples` 結尾括號：`}` → `]` |

## 5 個問題自我檢查
| 問題 | 答案 |
|------|------|
| 我在哪裡？ | Phase 3（玩家模式（Runtime）MVP） |
| 我要去哪裡？ | Phase 3~7（玩家模式、演出、存檔倒帶、開發者模式、示範驗收） |
| 目標是什麼？ | 做出可重用的 VN 引擎（UPM package），含開發者模式與玩家模式 |
| 我學到什麼？ | 看 `PlanningWithFiles/20260120/TextAdventureEngine/findings.md` |
| 我做了什麼？ | 看本檔上方 |

## Session：2026-01-27
- 完成 Phase 4 的 Addressables（Player build 資源載入）：
  - `InkResourceMap` 在 Player build 改用 Addressables 依 `address` 載入（Editor 仍用 `assetPath`）
  - Demo：`Assets/OpsidanosInk/Demo/resource_map.json` 補齊 `address`，並更新 `version=2`
  - 更新套件依賴與 asmdef：`Packages/com.opsidanos.ink/package.json` / `Packages/com.opsidanos.ink/Runtime/OpsidanosInk.Runtime.asmdef`
  - 更新文件與測試：`Packages/com.opsidanos.ink/README.md`、`Assets/Editor/Tests/InkResourceMapAddressablesTests.cs`

## Session：2026-02-06（歷史總計畫狀態同步）
- 依後續任務實際完成情況，回填 `task_plan.md` 狀態，避免舊總計畫誤導：
  - Phase 3（玩家模式 MVP）同步為 `complete`
  - Phase 5（存讀檔/倒帶）曾同步為 `in_progress`（後續已封存並遷移至新主計畫）
  - Phase 7（示範與驗收）曾同步為 `in_progress`（後續已封存並遷移至新主計畫）
- 對應驗收來源：
  - `PlanningWithFiles/20260204/playmode_acceptance_checklist/`（人工驗收與自動測試）
  - `PlanningWithFiles/20260205/playmode_ui_click_tests/`（UI 點擊與一鍵測試）

## Session：2026-02-06（封存與主計畫遷移）
- 本檔已封存，不再追蹤新待辦。
- 剩餘 7 項已完整遷移到：
  - `PlanningWithFiles/20260206/phase5_phase6_unified/task_plan.md`
