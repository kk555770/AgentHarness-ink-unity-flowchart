# 任務計畫：OpsidanosInk（Unity 套件）

## 目標
- Unity Console 不再出現 `CS0118`、`DirectoryNotFoundException`、`Samples~` 相關警告
- 套件內不放本名，只使用代號：`Opsidanos`（黑曜石）

## 範圍
- 建立/維護規劃文件：`PlanningWithFiles/20260122/OpsidanosInk/`
- 修正 `Packages/com.opsidanos.ink/Runtime/Scripts/Story/InkStoryEngine.cs`（`Story` 命名衝突）
- 修正 `Packages/Ink/Editor/Core/InkEditorUtils.cs`（避免把資料夾誤判成 `.ink`）
- 清理 `Packages/com.opsidanos.ink` 的 `Samples~` 殘留與 `package.json` samples 設定

## 任務來源
- 你貼出的 Unity Console 錯誤/警告：
  - `CS0118: 'Story' is a namespace but is used like a type`
  - `DirectoryNotFoundException`（Ink 自動編譯器誤判路徑）
  - `Samples~` 相關警告
  - `package.json is not valid JSON`
- 本任務的結束條件：你在 Unity 端驗證「Console 乾淨」（對應本檔 Phase 5）

## Phase
- [x] Phase 1：建立規劃文件
- [x] Phase 2：修正 `InkStoryEngine` 編譯錯誤（`Story` 衝突）
- [x] Phase 3：修正 Ink 自動編譯器誤判資料夾（`IsInkFile`）
- [x] Phase 4：清理 `Samples~` 與 `package.json` samples
- [x] Phase 5：在 Unity 驗證 Console（由專案負責人操作）

## 目前狀態
- **狀態：complete**

## 錯誤紀錄
| 錯誤 | 嘗試 | 結果/解法 |
| --- | --- | --- |
| `CS0118: 'Story' is a namespace but is used like a type` | 1 | `InkStoryEngine.cs` 改用 `Ink.Runtime.Story` alias |
| `DirectoryNotFoundException`（Ink 編譯器讀到 `Assets/s/...`） | 1 | `InkEditorUtils.IsInkFile` 排除資料夾路徑 |
| `Samples~` 資料夾/`.meta` 不一致警告 | 1 | 補回 `Samples~/PlayerModeSample` 與 `.meta`，並調整 `.gitignore` 讓 `Samples~` 不再被忽略 |
| `package.json is not valid JSON` | 1 | `samples` 結尾括號修正：`}` → `]` |
| `Samples~.meta exists but folder can't be found` 警告 | 1 | 刪除 `Samples~.meta`（`Samples~` 不應該有資料夾 `.meta`） |
