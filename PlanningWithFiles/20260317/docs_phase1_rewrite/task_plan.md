# 任務計畫：文件翻修 Phase 1

## 目標
先翻修專案說明文件，明確切開 canonical truth、現況工作流、作者工具策略與 Runtime 使用入口，為後續 Web-first 作者工具重製鋪路。

## 當前階段
Phase 1

## 階段

### Phase 1：建立工作記錄與確認範圍
- [x] 確認使用者同意先翻修說明文件
- [x] 確認本輪只改文件，不改正式腳本
- [x] 建立本輪 PlanningWithFiles 紀錄
- **Status:** complete

### Phase 2：閱讀現有文件入口與規格文件
- [x] 讀根 README
- [x] 讀 `Packages/com.opsidanos.ink/README.md`
- [x] 讀核心規格文件開頭與責任敘述
- [x] 整理需要統一的語言與交叉引用
- **Status:** complete

### Phase 3：實作文件翻修
- [x] 改寫根 README
- [x] 新增 `Documentation/AuthoringToolStrategy.md`
- [x] 新增 `Documentation/DocsIndex.md`
- [x] 統一核心規格文件開頭與交叉引用
- [x] 更新 Runtime README 的定位說明
- **Status:** complete

### Phase 4：一致性檢查與回報
- [x] 檢查關鍵詞與責任邊界是否一致
- [x] 整理本輪成果與後續建議
- [ ] 回報給使用者
- **Status:** in_progress

## 關鍵問題
1. 哪些文件現在最容易讓人誤把現況工作流當成最終架構？
2. 要怎麼把「Web-first 作者工具策略」寫清楚，又不把 Electron/實作細節寫死？
3. 如何讓新讀者能在最短時間找到「方向文件」、「現況入口」與「過渡層」？

## 已做決策
| 決策 | 原因 |
|------|------|
| 先翻修文件，再談 WebView 實作 | 先統一語言與責任邊界，避免後續實作建立在混亂前提上 |
| 新增策略文件與索引文件 | 目前缺的是總圖與作者工具策略，不只是零散補句子 |

## 錯誤紀錄
| Error | Attempt | Resolution |
|-------|---------|------------|
| 無 | 1 | 尚未發生 |

## 備註
- 每完成一批文件改寫就更新 findings 與 progress
- 保留既有正式文件內容，但加強「這份文件在系統裡扮演什麼角色」
