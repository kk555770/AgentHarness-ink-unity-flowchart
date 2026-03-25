# 文件所有權

> 文件負責人：harness
> 最後更新：2026/03/25

## 目的

把 `Documentation/` 的責任切清楚，讓每份文件都有固定 owner。

## owner 分類

- `architecture`：架構、schema、控制面、分層與核心設計
- `authoring`：作者工具工作流、重製邊界、實作計畫
- `projection`：JSON / Ink / 輸出契約 / projection 規格
- `runtime`：玩家模式、播放、除錯與執行時行為
- `harness`：計畫、品質、可靠性、工作規則、generated docs
- `security`：安全、權限、祕密、外部服務風險
- `reference`：參考資料、工具規格、索引、查詢型文件

## 更新責任

- 每份 `Documentation/**/*.md` 都要標示 `> 文件負責人：...`
- 新增或大改文件時，先選主責 owner，再同步對應索引
- 文件若跨分類，以主要解決的問題決定 owner，不要平均分配
- 文件主題如果已漂移，就直接改 owner，不要硬撐舊分類

## 實務判準

- 能回答「架構怎麼分」就歸 `architecture`
- 能回答「作者怎麼改」就歸 `authoring`
- 能回答「輸出長什麼樣」就歸 `projection`
- 能回答「執行時怎麼動」就歸 `runtime`
- 能回答「怎麼維持可靠」就歸 `harness`
- 能回答「怎麼避免風險」就歸 `security`
- 能回答「只是查資料」就歸 `reference`
