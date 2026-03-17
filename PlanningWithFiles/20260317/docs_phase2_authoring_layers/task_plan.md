# 任務計畫：文件翻修 Phase 2（作者工具分層）

## 目標
把 current GraphToolkit 工作流、current projection contract 與 future Web-first authoring strategy 三層切得更清楚，避免後續實作提案建立在混讀前提上。

## 當前階段
Phase 1

## 階段

### Phase 1：建立工作記錄與確認範圍
- [x] 確認使用者同意進入文件翻修 Phase 2
- [x] 確認本輪仍只改文件，不改正式腳本
- [x] 建立本輪 PlanningWithFiles 紀錄
- **Status:** complete

### Phase 2：盤點作者工具相關文件
- [x] 閱讀 `DocsIndex`、`AuthoringToolStrategy`、`DeveloperModeOutputContract`
- [x] 閱讀 `README` 與 `NarrativeGraphArchitecture` 相關段落
- [x] 找出 current tooling / current projection / future strategy 的混讀點
- **Status:** complete

### Phase 3：實作文件翻修
- [x] 新增 current authoring workflow 文件
- [x] 更新 `DocsIndex.md`
- [x] 更新 `README.md`
- [x] 更新 `AuthoringToolStrategy.md`
- [x] 視需要補強 `NarrativeGraphArchitecture.md` 與 `DeveloperModeOutputContract.md`
- **Status:** complete

### Phase 4：一致性檢查與回報
- [x] 搜尋關鍵詞與交叉引用
- [x] 確認分層敘事一致
- [ ] 回報給使用者
- **Status:** in_progress

## 關鍵問題
1. 哪份文件最適合承接「current GraphToolkit 工作流」的細節，而不污染架構北極星？
2. 要怎麼讓讀者一眼分出「current authoring workflow」和「future authoring strategy」？
3. 如何避免 `DeveloperModeOutputContract.md` 被誤讀成作者工具總規格？

## 已做決策
| 決策 | 原因 |
|------|------|
| Phase 2 聚焦作者工具三層分界 | 這是 Phase 1 完成後最自然、也最容易混讀的一塊 |
| 新增一份 current authoring workflow 文件 | 把現況工作流集中，不讓策略文件承擔太多現況細節 |

## 錯誤紀錄
| Error | Attempt | Resolution |
|-------|---------|------------|
| 無 | 1 | 尚未發生 |

## 備註
- 這輪不談前端框架細節，只切文件責任邊界
