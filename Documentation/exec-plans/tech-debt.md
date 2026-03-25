# tech-debt

> 文件負責人：harness
> 最後更新：2026/03/25

## 目前主要缺口

- `repo_guard v4` 已開始補 docs freshness / index 內容量 / cross-links / active exec plan / ownership，但還沒有更深層 code graph guard
- `codex-auto-fix.yml` 已改成 Unity-first，但完整 verify 仍依賴 `UNITY_LICENSE`
- Unity 測試在缺少授權時仍可能退化成說明訊息而非固定回饋回圈
- 需要至少一個新版 CI run 產出 Unity 測試 artifact，generated test truth 才會從 bootstrap 狀態進入固定更新
