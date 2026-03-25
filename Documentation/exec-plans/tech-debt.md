# tech-debt

> 文件負責人：harness
> 最後更新：2026/03/25

## 目前主要缺口

- `repo_guard v4` 已開始補 docs freshness / index 內容量 / cross-links / active exec plan / ownership / workflow permissions，但還沒有更深層 code graph guard
- `ImportProjection` 雖已補上 `flowchart-json` control-plane 閉環，但 `ink` import 與其他 target 仍未完成
- `codex-auto-fix.yml` 已改成 Unity-first，而且缺少完整 Unity 驗證 secrets 時會 fail closed，但完整 verify 仍依賴 GitHub 上有有效 secrets 組合
- 需要至少一個新版 CI run 產出 Unity 測試 artifact，generated test truth 才會從 bootstrap 狀態進入固定更新
- `story-json` 與 `graphtoolkit-model` 仍是後續 target
