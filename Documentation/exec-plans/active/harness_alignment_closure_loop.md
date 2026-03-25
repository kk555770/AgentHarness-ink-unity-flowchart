# Harness Alignment Closure Loop

> 文件負責人：harness
> 最後更新：2026/03/25
> 角色：正式進行中計畫 / Active Exec Plan

## 目標

把這輪 harness 對齊工作收斂成可重讀、可重審、可續作的正式閉環，讓 workflow guard、docs guard、Ink compile validation、以及 `ImportProjection` 的系統文件都維持同一份 system-of-record。

## 本輪已落地的基線

- workflow guard
  - `CI.yml` 在缺少 `UNITY_LICENSE` 時改為 fail closed
  - `codex-auto-fix.yml` 在缺少 `UNITY_LICENSE` 時不再假裝修好
  - `docs-garden.yml` 改成優先使用 CI workflow_run provenance
- docs guard
  - `repo_guard` 已擴大 freshness / index / ownership / workflow permissions 檢查
  - `doc_garden` 已把 workflow permissions 與文件分類覆蓋納入報告
  - `Documentation/generated/index.md` 已是正式生成文件入口
- Ink compile validation
  - `ValidateProjection(Ink)` 與 `ProjectGraph(Ink)` 已要求 compile probe
  - 預設 compile probe 已進 canonical core，不再只靠 Editor bootstrap 才有 probe
  - 對應測試已補上 probe 成功 / 失敗 / JSON control plane 的回歸驗證

## 改進項目與順序

1. 正式文件與任務文件同步
   - 目標：把 closure loop 的內容寫回 `PlanningWithFiles` 與 `Documentation`
   - 驗證方式：`task_plan.md`、`findings.md`、`progress.md` 與本文件內容一致
   - 完成定義：所有長期入口都能直接看到目前的改進順序與完成標準

2. workflow guard 收斂
   - 目標：把 CI、auto-fix、docs-garden 的信任邊界寫死
   - 驗證方式：缺少授權時 fail closed、workflow permissions 檢查、docs-garden provenance 可追溯
   - 完成定義：不會再出現缺授權卻綠燈的假驗證

3. docs guard 收斂
   - 目標：讓 docs freshness、owner coverage、index counts、generated docs 彼此一致
   - 驗證方式：`repo_guard` 與 `doc_garden` 報告一致，generated index 可回推來源
   - 完成定義：正式文件與 generated 文件都能被機械式檢查覆蓋

4. `ImportProjection` 落地到 control plane
   - 目標：把 `ImportProjection(flowchart-json)` 的 request / response shape、dispatcher routing、shared import service 與測試一起補齊
   - 驗證方式：EditMode contract 測試、dispatcher 測試、shared service 測試都通過，且 API spec / JSON contract 內容一致
   - 完成定義：canonical JSON control plane 已具備最小 import loop，不再只有 `ValidateProjection / ProjectGraph`

5. 重新審查 projection / import 閉環
   - 目標：把 `ValidateProjection`、`ProjectGraph`、`ImportProjection` 的語意邊界一次對齊
   - 驗證方式：重新閱讀文章與正式文件，確認每個 target 的規範沒有互相打架
   - 完成定義：control plane、contract、現況註記三者一致，且不再把已落地功能寫成待辦

6. 剩餘架構與觀測性缺口
   - 目標：處理 deeper code graph guard、`story-json`、`graphtoolkit-model`、agent observability
   - 驗證方式：每個缺口都有正式文件與可執行驗證入口
   - 完成定義：沒有只存在聊天室、沒有 system-of-record 的關鍵缺口

## 目前尚未完成

- `ImportProjection` 目前只先支援 `flowchart-json`
- `ink` import、legacy mapping warnings 與更多 target 的 round-trip 語意仍待補強
- 雲端 / CI 的完整 Unity feedback loop 仍依賴 `UNITY_LICENSE`
- `story-json` 與 `graphtoolkit-model` 仍是後續 target
- 更深層的 code graph guard 與 observability 仍待補強

## 驗收標準

- `PlanningWithFiles` 的三份文件都能獨立回答「現在在做什麼、為什麼做、下一步是什麼」
- `CurrentMilestones.md`、`QUALITY_SCORE.md`、`RELIABILITY.md`、`tech-debt.md` 與 API / JSON contract 文件一致
- `ImportProjection(flowchart-json)` 已有正式 contract 與 control-plane 實作描述，且未把未支援 target 寫成已完成
- 後續若擴充其他 import target，只需要更新同一批 system-of-record 文件，不需要先補一份新口頭說明
