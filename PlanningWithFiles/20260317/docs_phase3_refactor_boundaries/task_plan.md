# 任務計畫：文件翻修 Phase 3（重製邊界）

## 目標
把目前 GraphToolkit 腳本責任、future canonical core、以及 authoring bridge 的責任邊界整理成第一版文件，讓後續重製不會變成亂拆。

## 當前階段
Phase 4

## 階段

### Phase 1：建立工作記錄與確認範圍
- [x] 確認使用者同意繼續文件翻修
- [x] 確認本輪仍只改文件，不改正式腳本
- [x] 建立本輪 PlanningWithFiles 紀錄
- **Status:** complete

### Phase 2：盤點目前責任切分現況
- [x] 盤點 GraphToolkit 相關腳本與 asmdef
- [x] 盤點現有文件中對 current tooling / future strategy 的描述
- [x] 整理可保留、可抽離、可降格的責任
- **Status:** complete

### Phase 3：實作文件翻修
- [x] 新增重製邊界文件
- [x] 更新 `DocsIndex.md`
- [x] 更新 `AuthoringToolStrategy.md`
- [x] 視需要更新 `CurrentAuthoringWorkflow.md` 與 `README.md`
- **Status:** complete

### Phase 4：一致性檢查與回報
- [x] 檢查交叉引用與責任名詞
- [x] 確認文件之間沒有互相搶角色
- [x] 回報給使用者
- **Status:** complete

## 關鍵問題
1. 目前 GraphToolkit 腳本裡哪些責任其實應該抽成 canonical core？
2. 哪些責任應該留在 current GraphToolkit workflow，當 migration baseline？
3. 未來 Web-first 作者工具和 Unity 之間，中間 bridge 應該負責什麼、不負責什麼？

## 已做決策
| 決策 | 原因 |
|------|------|
| Phase 3 聚焦責任邊界，不碰實作細節 | 先把拆分線畫清楚，避免後續重構直接踩雷 |
| 新增一份重製邊界文件 | 目前文件已有 why、what、workflow、strategy，但還缺 refactor boundary |

## 錯誤紀錄
| Error | Attempt | Resolution |
|-------|---------|------------|
| 無 | 1 | 本輪僅文件修改與一致性檢查，未出現阻塞 |

## 備註
- 這輪不決定類別名或前端框架，只切 responsibility boundary
