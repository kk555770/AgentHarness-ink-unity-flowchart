# 調查發現

## 需求
- 使用者已同意先翻修各項說明文件。
- 本輪先處理文件，不改正式腳本。
- 目標是幫後續 Web-first 作者工具與 AI 接入鋪路。

## 研究發現
- 根 README 已經有「repo 不只是上游鏡像」的正確提醒，但還缺一份正式的作者工具策略文件。
- `Packages/com.opsidanos.ink/README.md` 很像 Runtime 使用手冊，但目前沒把自己明確標成「玩家模式 / 現況 Runtime 入口」。
- `NarrativeGraphArchitecture.md`、`CanonicalGraphSchema*.md`、`CanonicalGraphApiSpec.md`、`CanonicalGraphJsonContract.md` 的規範層已相對清楚，但都還沒正面回答「為什麼作者工具策略要轉向 Web-first」。
- `DeveloperModeOutputContract.md` 很完整地定義了 current projection 到 Runtime 的合法輸出，但讀者若沒先看架構文件，仍可能把它誤讀成整個專案的主規格。
- 這輪翻修後，文件群已分成 5 層：
  - 北極星：`NarrativeGraphArchitecture.md`
  - 作者工具策略：`AuthoringToolStrategy.md`
  - 真相層：`CanonicalGraphSchema.md`、`CanonicalGraphSchemaSpec.md`
  - 控制面：`CanonicalGraphApiSpec.md`、`CanonicalGraphJsonContract.md`
  - 現況投影 / Runtime：`DeveloperModeOutputContract.md`、`Packages/com.opsidanos.ink/README.md`
- 根 README 現在明確說出：GraphToolkit 是 current tooling baseline，作者工具長期策略傾向 Web-first。
- `Packages/com.opsidanos.ink/README.md` 現在明確標示自己是 Runtime 使用手冊，而不是整個 repo 的架構總覽。
- `DeveloperModeOutputContract.md` 現在明確標示自己是 current projection contract，不是 canonical truth，也不是作者工具策略文件。
- `CanonicalGraphApiSpec.md` 與 `CanonicalGraphJsonContract.md` 現在更直接把 AI、Playwright、自動化與未來 Web 作者工具拉進同一個控制面敘事裡。

## 技術判斷
| Decision | Rationale |
|----------|-----------|
| 新增 `AuthoringToolStrategy.md` | 補上現在缺的一塊：作者工具策略與 Web-first 方向 |
| 新增 `DocsIndex.md` | 讓新讀者可快速分辨哪份是北極星、哪份是現況、哪份是過渡層 |
| 統一各文件開頭的「責任定位」 | 避免 canonical / projection / runtime / current tooling 被混讀 |
| 不在這輪把 Electron 寫死 | 目前更重要的是先確立 Web-first control surface 與文件分層，桌面殼可後決 |

## 遇到的問題
| Issue | Resolution |
|-------|------------|
| 尚未遇到阻塞 | 持續實作 |

## 參考資源
- `README.md`
- `Packages/com.opsidanos.ink/README.md`
- `Documentation/NarrativeGraphArchitecture.md`
- `Documentation/CanonicalGraphSchema.md`
- `Documentation/CanonicalGraphSchemaSpec.md`
- `Documentation/CanonicalGraphApiSpec.md`
- `Documentation/CanonicalGraphJsonContract.md`
- `Documentation/DeveloperModeOutputContract.md`

## 視覺/瀏覽重點
- 現有文件已經能看出清楚的 schema-first 北極星。
- 缺口主要在「作者工具策略」與「文件索引導覽」。
- 本輪已補上這兩塊缺口，並把 root README 與 Runtime README 從「單點入口」整理成「分層入口」。
