# tech-debt

## 目前主要缺口

- `repo_guard v1` 已補 docs 入口 / asmdef / 檔案大小 gate，但還沒有更深層 freshness / cross-links / ownership 檢查
- `codex-auto-fix.yml` 已改成 Unity-first，但完整 verify 仍依賴 `UNITY_LICENSE`
- Unity 測試在缺少授權時仍可能退化成說明訊息而非固定回饋回圈
