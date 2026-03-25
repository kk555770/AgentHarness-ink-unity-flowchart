# RELIABILITY

> 最後更新：2026/03/25

## 現有可靠性入口

- `.github/workflows/CI.yml`
- `Tools/repo_guard.py`
- `Tools/run_tests.sh`
- `Assets/Editor/Tests/`
- `Assets/Tests/PlayMode/`
- `Documentation/DeveloperModeOutputContract.md`

## 現況

- `repo_guard v1` 已先守 docs 入口、asmdef 依賴方向、`.cs` 檔案大小
- `repo_guard v2` 已開始守 docs freshness / cross-links / active exec plan
- EditMode / PlayMode / contract 測試已是主要回饋迴路
- current projection contract 已被文件化
- `codex-auto-fix.yml` 已改成 Unity-first verify flow
- 目前活躍收斂計畫在 `Documentation/exec-plans/active/harness_system_of_record_v2.md`

## 主要缺口

- 仍缺更深層的 docs ownership 檢查
- 缺少更完整的 code graph / asmdef 以外架構 gate
- 缺少不依賴 `UNITY_LICENSE` 的完整 Unity 驗證回饋
