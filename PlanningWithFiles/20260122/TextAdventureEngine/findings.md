# Findings：文字冒險 / 視覺小說引擎（v1，2026-01-22 續作）

## 這次最重要的事
- 未完成任務其實都已經寫在 `PlanningWithFiles/20260120/TextAdventureEngine/task_plan.md`：
  - Phase 3 還剩 4 個勾勾（回看 / AutoSkip / 隱藏 UI / Tag 管線）
  - Phase 4~7 都還是 pending

## 現況（你已經做完/驗證）
- `OpsidanosInk` 套件已可被 Unity 正常解析，且 Console 已乾淨（你已驗證）

## 這次新增的坑（避免下次再踩）
- UI Toolkit 的 `ScrollView`：
  - `scrollView.childCount` 不是你塞進去的項目數量（它包含 ScrollView 自己的內部元素）
  - 要拿你塞進去的項目，請用 `scrollView.contentContainer.childCount`
- Unity 的 Start 順序：
  - `InkStoryEngine` 在 `Start()` 會先吐第一句
  - Presenter 如果在 `Start()` 才抓 UXML 元件，可能會錯過第一句
  - 所以 Presenter 改成在 `Awake()` 就先抓元件
- UI Toolkit USS 疊層規則：
  - USS 不支援 `z-index`，使用會在 Console 出現警告
  - 疊層要靠「UXML 順序」（後面的元素畫在上面）＋ `position: absolute`
