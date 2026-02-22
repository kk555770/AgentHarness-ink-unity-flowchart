# 任務計畫：文字冒險 / 視覺小說引擎（v1）

## 封存狀態
- 本檔已封存（2026/02/06），後續主線請改看：
  - `PlanningWithFiles/20260206/phase5_phase6_unified/task_plan.md`

## 目標
在這個 Unity 專案（`ink-unity-integration`）中做出一套可重用的「文字冒險 / 視覺小說」引擎（UPM package），分成「開發者模式（Editor）」與「玩家模式（Runtime）」兩個場景。

## 核心管線（新規定）
- 開發者模式（Editor）：用 Flow Chart 製作內容 → **輸出 `.ink`**（可版本控管）
- Ink Unity Integration：把 `.ink` **編譯**成 `.json`
- 玩家模式（Runtime）：**只讀 `.json`** 播放（Runtime 不依賴 Flow Chart 資產）

## 優化原則（同樣需求下，先做這些最省力）
- `.ink` 只放「故事與 Tag」；Flow Chart 編輯資料另外存成 sidecar（例如 `story.ink.flow.json`）
- Tag 規格像字典：格式固定、參數固定，先解析成結構化資料，再由各系統處理
- 玩家模式維護一份 `PresentationState`（背景/立繪/BGM/UI 開關等），Tag 只改它；存檔/倒帶也只存它
- Flow Chart 先做最小節點集合（Ink 節點 + 演出節點 + 註解），其他節點（小遊戲/條件/分支）先留接口
- 出錯用 `Debug.LogError` 指到「作者能修的地方」（哪個 Tag、哪個 id、在哪個 knot），不要在 Play Mode 自動幫忙修資料

## 目前階段
已封存（不再追蹤新待辦）

## Phases

### Phase 1：依新規定重寫計畫（完成）
- [x] 把「玩家模式」與「開發者模式」的責任切清楚
- [x] 把 `.ink`（來源）與 `.json`（播放）管線寫進文件
- [x] 建立本次 planning-with-files 的 `task_plan.md` / `findings.md` / `progress.md`
- **狀態：complete**

### Phase 2：回退後盤點與基準確認（完成）
- [x] 以你回退後的 repo 狀態為準，重新盤點「目前還剩下哪些功能」
- [x] 確認 MVP 先做「玩家模式」還是「開發者模式」優先
- **狀態：complete**

### Phase 3：玩家模式（Runtime）MVP（complete）
- [x] 建立 UPM 套件：`OpsidanosInk`（`Packages/com.opsidanos.ink`）
- [x] 以 Ink `.json` 建立 `Ink.Runtime.Story`
- [x] UI Toolkit：名字、文字、選項（最小可跑）
- [x] 修正 Unity Console：`CS0118` / `DirectoryNotFoundException` / `Samples~` 警告 / `package.json` JSON 錯誤
- [x] 回看（Backlog）
- [x] Auto / Skip
- [x] 隱藏 UI
- [x] Tag → 結構化資料 → 事件（先打通管線，不求全做完）
- [x] 在 Unity 驗證功能（回看 / Auto / Skip / 隱藏 UI）
- **狀態：complete**

### Phase 4：演出與資源綁定（完成）
- [x] 先支援：BGM（`bgm:id`）
- [x] 先支援：SE（`se:id`）
- [x] 先支援：背景（`bg:id`）
- [x] 先支援：立繪（含位置：left/center/right）
- [x] 先支援：CG（`cg:id` / `cg:clear`）
- [x] 先支援：背景特效（`# shake`）
- [x] 資源綁定：改用「ResourceMap JSON」（`resource_map.json`：id→assetPath）集中管理（取代 ScriptableObject）
- [x] Addressables（已接上：Player build 讀 `resource_map.json` 的 `address`，用 Addressables 載入資源）
- **狀態：complete**

### Phase 5：存檔 / 讀檔 / 逐步倒帶（archived）
- [x] 存檔：`inkStateJson` + 引擎狀態 + 畫面狀態（不依賴流程圖/章節系統）
- [x] 倒帶：Rollback buffer（最多 N 步）
- [x] 已移轉：UI：存/讀 槽位（多槽 + Auto 槽）→ 新主計畫項目 1
- **狀態：archived**

### Phase 6：開發者模式（Editor）Flow Chart（archived）
- [x] 已移轉：UI Toolkit EditorWindow：Flow Chart 編輯（拖拉、連線、節點屬性）→ 新主計畫項目 2
- [x] 已移轉：匯出：Flow Chart → `.ink` + sidecar（例如 `story.ink.flow.json`）→ 新主計畫項目 3
- [x] 已移轉：匯入：`.ink` + sidecar → 還原等效 Flow Chart（不要求跟匯出前一模一樣）→ 新主計畫項目 4
- [x] 已移轉：Flow Chart 節點最小集合：Ink 節點（knot/stitch）、演出節點（產生 Tag）、註解 → 新主計畫項目 5
- [x] 已移轉：Tag 字典（規格表）：每個 Tag 的格式、參數、例子、錯誤訊息規則 → 新主計畫項目 6
- **狀態：archived**

### Phase 7：示範與驗收（archived）
- [x] Demo：玩家模式播放一段 `.json`（含 Tag 演出）
- [x] 已移轉：Demo：開發者模式匯出 `.ink`、再由 Ink Unity 編譯 `.json`、玩家模式可播放 → 新主計畫項目 7
- [x] 驗收清單：能跑、能看、能選、能存讀、能倒帶
- **狀態：archived**

## 關鍵問題（歷史待確認）
1. 內容規模：先做「單一 `.ink`」還是「多檔 include / 多章節」？
2. Flow Chart 節點最小集合：只做「文字/選項/演出」還是要先含「小遊戲節點」？
3. 資源綁定：作者在 Flow Chart 要「直接選 Sprite/Audio」還是「只填 id」？
4. `.ink` 匯入還原：允許「只還原結構與 Tag，版面自動排」嗎？

## 決策紀錄
| 決策 | 原因 |
|------|------|
| 開發者模式輸出 `.ink`；玩家模式只吃編譯後 `.json` | `.ink` 好版本控管；`.json` 是 Ink runtime 直接可播放格式 |
| 開發者模式與玩家模式分成不同場景 | 玩法（播放）與製作（編輯）責任清楚 |
| UI 全用 UI Toolkit（含 Editor 工具） | 專案方向一致，不混用 uGUI |
| Flow Chart 編輯資訊用 sidecar 保存（不塞進 `.ink`） | `.ink` 保持乾淨好讀；版面/備註/群組不影響 Ink 編譯 |
| Tag 先解析成結構化資料再處理 | 可測試、可除錯、可擴充，不把字串判斷散落到 UI |
| 玩家模式用 `PresentationState` 統一管理畫面 | 存檔/倒帶只要存 Ink state + `PresentationState`，不會越做越亂 |
| Flow Chart 先做最小節點集合 | 先把匯出 `.ink` 跑通，再逐步加節點種類 |
| 錯誤只提示不自動修 | 符合所見即所得，避免一次性修好但下次又壞 |

## 錯誤紀錄
| Error | Attempt | Resolution |
|-------|---------|------------|
| `CS0118: 'Story' is a namespace but is used like a type` | 1 | 在 `InkStoryEngine` 使用 `InkRuntimeStory = Ink.Runtime.Story` alias 解決命名衝突 |
| Ink 自動編譯器把資料夾 `Packages/com.opsidanos.ink` 誤判成 `.ink`，造成 `DirectoryNotFoundException` | 1 | 修正 `InkEditorUtils.IsInkFile`：遇到資料夾路徑先回傳 false，避免把資料夾當成 Ink 檔 |
| `Samples~` 相關警告（`.gitignore` 的 `*~` 造成 Samples~ 沒被版控） | 1 | 調整 `.gitignore` 允許 `Packages/**/Samples~`；移除 `Samples~.meta`；補最小 Sample 內容（`story.ink`） |
| `package.json is not valid JSON` | 1 | 修正 `samples` 結尾括號：`}` → `]` |

## 封存備註（2026-02-06）
- 後續請只追新主計畫，不再在本檔新增待辦。
