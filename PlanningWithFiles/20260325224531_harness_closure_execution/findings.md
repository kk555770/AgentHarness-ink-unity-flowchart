# Findings & Decisions

## Requirements

- 把目前已完成的 harness 對齊成果整理成正式任務文件與 system-of-record 入口。
- 把 `ImportProjection` 納入正式 system-of-record 文件。
- 若某些 target 尚未完成，就不能因為 `flowchart-json` 已落地而把全部 import target 寫成已完成。
- 只修改 `Documentation` 與 `PlanningWithFiles`。
- 所有內容都要用繁體中文。

## Research Findings

- 主線已補上 `ImportProjection(flowchart-json)` 的 request / response shape、dispatcher routing、shared import service 與測試。
- 主線後續又把預設 Ink compile probe 拉進 canonical core，`ValidateProjection(Ink)` 不再只靠 Editor bootstrap 才能拿到 probe。
- `Documentation/CurrentMilestones.md`、`QUALITY_SCORE.md`、`RELIABILITY.md`、`tech-debt.md` 需要從「dispatcher 未落地」改成「`flowchart-json` 已落地，但其他 target 未完成」。
- `Documentation/CanonicalGraphJsonContract.md` 需要跟程式碼一致，改用 `projectionJson` / `projectionText` 承載 payload，而不是 `metadata.rawJson`。
- `Documentation/exec-plans/active/index.md`、`Documentation/exec-plans/index.md`、`Documentation/PLANS.md` 已經把新的 closure loop 計畫掛進正式入口，接下來只需要補正文與同步其他 system-of-record 文件。
- 目前已完成的基線包含 workflow guard、docs guard 與 Ink compile validation；這輪文件要把這些成果固定成可重讀的正式內容。

## Technical Decisions

| Decision | Rationale |
|----------|-----------|
| `ImportProjection` 先以 contract 固定邊界，再讓 `flowchart-json` control-plane 對齊同一份文件 | 讓文件與程式可以在同一輪收斂，而不是互相等待 |
| `flowchart-json` 先作為目前最小可守的 import target | 這與現有 importer 路徑最接近，也最能和現況對齊 |
| request payload 改用 `projectionJson / projectionText` | 主線已落地實作，文件應直接對齊真實 API shape |
| 預設 Ink compile probe 放進 core，而不是只留在 Editor bootstrap | 可直接縮小 article re-audit 中「Ink 驗證是 Editor-bound」的缺口 |

## Issues Encountered

| Issue | Resolution |
|-------|------------|
| 文件子代理完成時，主線已額外補上 `ImportProjection` control-plane | 重新複核所有 system-of-record 文件，避免留下「仍待主線落地」的過時描述 |
| 獨立 re-audit 指出 Ink 驗證仍偏 Editor-bound | 主線把預設 compile probe 拉進 core，再重新驗證 |

## Resources

- `Documentation/CanonicalGraphApiSpec.md`
- `Documentation/CanonicalGraphJsonContract.md`
- `Documentation/CurrentMilestones.md`
- `Documentation/QUALITY_SCORE.md`
- `Documentation/RELIABILITY.md`
- `Documentation/exec-plans/active/harness_alignment_closure_loop.md`
- OpenAI harness engineering 文章：<https://openai.com/zh-Hant/index/harness-engineering/>

## Visual/Browser Findings

- 這篇文章的重點是把工程回饋迴路做成可驗證的 harness，而不是只靠人讀信件或聊天室內容。
- 文章強調的是把 truth 寫回 repo，讓 agent 能靠檔案、guard 和測試重複驗證。

---
*這份 findings 會跟著每一輪文件同步更新*
