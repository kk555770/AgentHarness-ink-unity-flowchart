# Findings：OpsidanosInk

## 重要發現
- `Packages/com.opsidanos.ink` 這個「資料夾路徑」本身會以 `.ink` 結尾，所以可能被第三方工具當成「Ink 檔」。
- `InkStoryEngine.cs` 放在 `.../Scripts/Story/`，常見會用 `*.Story` 當命名空間，會和 `Ink.Runtime.Story` 撞名。
- 專案的 `.gitignore` 有 `*~`，會把 `Samples~` 整個資料夾忽略掉，導致「有 `.meta` 但資料夾不在版本控制」→ Unity 警告。
- UPM 套件的 `Samples~` 建議不要放 `Samples~.meta`；Unity 會把它當成「應該存在的資料夾資產」去檢查，容易噴警告。

## 根因（用最短句子講清楚）
- `CS0118`：`Story` 在專案裡是「命名空間」，但程式把它當「型別」用。
- `DirectoryNotFoundException`：Ink 的 Editor 工具把「資料夾」當成 `.ink` 檔案去讀。
- `Samples~`：`Samples~.meta` 存在但資料夾不存在（或不同步），Unity 會噴警告並自動補空資料夾。

## 決策
- 套件內的作者/註解人名只用代號：`Opsidanos`（黑曜石），不放本名。
- 提供最小 `Samples~/PlayerModeSample`（只放 `story.ink`），讓「玩家模式快速開始」可直接 Import 使用。
