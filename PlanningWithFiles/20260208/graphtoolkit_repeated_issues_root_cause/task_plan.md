# 任務計畫：Graph Toolkit 反覆問題根因調查

## 目標
- 找出近期 Graph Toolkit 反覆出現問題的共同根因。
- 以「上一個 commit 前後」為界線，比對差異並定位風險變更。
- 交付可驗證證據（commit、檔案、console、歷史任務紀錄）。

## 範圍
- 專案內 Graph Toolkit 相關程式、設定、資產、場景與歷史任務紀錄。
- Git 歷史（至少包含 `HEAD` 與 `HEAD~1` 的差異）。
- Unity Console 現況（若可讀取）。

## 階段
| 階段 | 狀態 | 說明 |
|---|---|---|
| 1. 建立基線 | complete | 盤點目前分支、最近 commit、工作樹狀態 |
| 2. 歷史任務回顧 | complete | 掃描 PlanningWithFiles 全歷史中的 Graph Toolkit 記錄 |
| 3. 程式/設定差異比對 | complete | 比對 `HEAD~1..HEAD` 與近期 Graph Toolkit 相關改動 |
| 4. Console/重現線索 | complete | 抽取目前錯誤訊息與觸發條件 |
| 5. 根因彙整 | complete | 將問題分群並建立「現象→證據→根因→驗證方式」 |

## 已知限制
- 本次以調查為主，不直接修改功能程式。
- 若讀不到 Unity Console，改以 Logs 與既有紀錄補強。

## 錯誤紀錄
| 時間 | 錯誤 | 嘗試 | 結果 |
|---|---|---|---|
