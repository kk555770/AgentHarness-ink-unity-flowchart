# 發現與決定：文字冒險 / 視覺小說引擎（v1）

## 需求（你已說清楚的方向）
- 目標：做成「可重用的框架」（UPM package），不是只做一款遊戲
- 平台：PC + WebGL（第一版）
- 互動：只有選項（Choice）
- UI：遊戲內（玩家模式）+ Unity Editor 工具（開發者模式）都用 UI Toolkit
- 開發者模式：圖形化 Flow Chart 編輯，能輸出 `.ink`，也能讀 `.ink` 還原等效 Flow Chart
- 玩家模式：播放 Ink Unity Integration 編譯出的 `.json`，顯示文字/選項/回看/Auto/Skip/存讀檔/倒帶/設定/隱藏 UI
- 演出：用 Tag 驅動（背景/立繪/BGM/SE/UI/文字效果/畫面特效/自訂事件）
- 存檔：多槽 + Auto，並支援逐步倒帶，且要有版本相容策略

## 研究發現（已用專案內檔案確認）
- Ink Unity Integration 會把 `.ink` 編譯成 `.json`（可自動編譯或手動編譯）：
  - `README.md`
  - `Packages/Ink/README.md`
- Ink runtime 播放故事的核心建構式是吃 JSON 字串：
  - `Packages/Ink/InkLibs/InkRuntime/Story.cs` 內有 `public Story(string jsonString)`

## 技術決策
| Decision | Rationale |
|----------|-----------|
| 開發者模式輸出 `.ink`；玩家模式只讀 `.json` | `.ink` 是人要維護的來源；`.json` 是播放用產物 |
| 兩個模式分不同場景 | 開發者（製作）與玩家（播放）不用互相干擾 |
| Flow Chart 匯入 `.ink` 只要求「等效」 | 允許丟失版面與部分編輯資訊，降低難度 |
| UI 全用 UI Toolkit | 同一套技術做 Runtime 與 Editor |
| 套件顯示名稱用 `OpsidanosInk`；套件代號用 `com.opsidanos.ink` | Unity 套件代號需要全小寫；顯示名稱可以用大寫好讀 |
| `.ink` 保持乾淨；Flow Chart 編輯資料用 sidecar（`story.ink.flow.json`） | 版面/群組/備註不該影響 Ink；也避免把編輯資料塞進 `.ink` 變難讀 |
| Tag 規格像字典，先解析成結構化資料再處理 | 不把字串判斷散在各處，之後加新 Tag 比較不會壞掉 |
| 玩家模式只維護一份 `PresentationState` | 背景/立繪/BGM/UI 開關都集中管理，存檔/倒帶也只要存這份資料 |
| Flow Chart 先做最小節點集合 | 先讓匯出 `.ink` 跑通，避免一開始就做太大做不完 |
| 出錯只提示不自動修 | 讓 Unity Editor 所見即所得；作者知道哪裡寫錯就能修 |
| 套件與程式碼註解只使用代號 `Opsidanos`（黑曜石） | 避免在套件內留下本名 |

## 遇到的狀況
| Issue | Resolution |
|-------|------------|
| 舊文件把「流程圖/章節系統」當成 Runtime 責任 | 本次以 v1 文件重新整理，改成：Flow Chart 是 Editor 工具，Runtime 只吃 `.json` |
| Ink Unity Integration 把資料夾 `Packages/com.opsidanos.ink` 誤判成 `.ink` 檔，導致 `DirectoryNotFoundException` | 修正 `Packages/Ink/Editor/Core/InkEditorUtils.cs` 的 `IsInkFile`：先排除資料夾路徑，避免把資料夾當成 Ink 檔 |
| `Samples~` 被 `.gitignore` 的 `*~` 忽略，造成 Unity 警告 | 調整 `.gitignore` 讓 `Packages/**/Samples~` 不再被忽略；並提供最小 Sample（`story.ink`）與移除 `Samples~.meta` |
| `package.json` JSON 格式錯誤 | 修正 `samples` 陣列結尾括號：`}` → `]` |

## 參考資源（本 repo 內）
- `README.md`
- `Packages/Ink/README.md`
- `Packages/Ink/InkLibs/InkRuntime/Story.cs`
- `Packages/Ink/Editor/Tools/Player Window/InkPlayerWindow.cs`（之後做開發者模式可參考）
