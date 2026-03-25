# SECURITY

> 文件負責人：security
> 最後更新：2026/03/25

## 現況

- 目前 repo 內沒有成形的安全入口文件
- 也還沒有清楚的 threat model 或 secret-handling 規範文件

## 目前最少共識

- 不要把敏感資料寫進文件、測試輸出或工作記錄
- 修改 CI / workflow / 外部 API 呼叫時，要把權限與風險說清楚

## 缺口

- 缺少正式安全規格
- 缺少針對 workflow / secrets / external service 的機械式檢查
