# Findings: 狀態2 角色節點與登場集束節點

## 任務來源（最後任務重讀結論）
- 本輪目標是「狀態2」而非擴張新需求。
- 狀態2核心是：
  1. `character` 節點可維護角色與表情圖資。
  2. `castBundle` 節點聚合本次登場角色。
  3. `action/stageAction` 角色下拉只顯示聚合結果。
  4. 接線與資料變更時可刷新角色候選。

## 現況觀察
- 目前已有 `action`（對話）與 `stageAction`（資料動作）拆分基礎。
- 現有規格檔已包含流程線/資料線分離，但尚未完整定義 `character/castBundle` 契約細節。
- 測試防暴走流程已建立（固定篩選 + timeout），可沿用。

## 這輪先做的事
- 先建立 2026-03-03 新 planning 任務，將狀態2單獨切出。
- 先收斂邊界，避免和舊任務混線。

## 已鎖定規則
- 可並行任務使用 multi-agent。
- 測試任務每次最多 60 秒。
- 調查與修改不限時。
- 先提案、等同意、再執行。

## 2026-03-03 MCP 問答新增結論（角色圖片來源）
- 使用者要求：
  - 在開發介面 FlowChart 節點可直接拖 Project 圖片。
  - 節點資料希望自動讀取 `Addressable + GUID`。
  - 若圖片已有 GUID 但尚未加入 Addressable，希望能自動加入。
- 已確認：
  - Addressable Group 策略：固定群組。
  - 固定群組名稱：`Project_Resources`。
  - 刷新策略：圖變更就刷新（接線/欄位/匯入/開圖）。
  - 角色名稱覆寫：對話/動作可改當下顯示資訊，但不影響角色下拉來源主資料。
  - `castBundle` 輸出策略：先做「依接線自動增減」。

## MCP 問答錯誤紀錄
- `ask_user_question_plus` 曾出現 120 秒 timeout（重試後成功）。

## 2026-03-03 MCP 追加確認（逐筆）
- Q1：`Project_Resources` 不存在時是否自動建立？
  - A：自動建立（`auto_create`）。
- Q2：先拖圖、後填角色ID/表情名稱時，Addressable Key 如何更新？
  - A：欄位變更時即時重算並更新 Key（`recompute_and_rename`）。
- Q3：最終 Key 格式？
  - A：`char/{id}/{expr}`。
- Q4：自動加入 Addressable 與後續改欄位刷新是否分兩段？
  - A：採用 A+B（拖圖即加入；改 ID/表情/cast 變更時重算 Key）。
- Q5：`castBundle` 多輸出的資料語意？
  - A：每個輸出都送同一份角色名單（`same_list_each_output`）。

## 待確認風險
1. 若 `character` 欄位鍵名先定錯，後續 importer/exporter 會反覆改動。
2. 若下拉刷新只綁接線事件，角色內容編輯後可能不更新。
3. 若 sidecar 不保存足夠資訊，round-trip 會丟失角色資料或順序。

## 2026-03-03 契約草案落地結果（Phase 2）
- 已在 `DeveloperModeOutputContract.md` 補上：
  - `character` / `castBundle` 節點型別與中文顯示名。
  - 三種接線型別（Flow / ActionData / CharacterData+CastData）與限制。
  - 角色來源、Addressable、Key 重算、刷新時機規範。
  - `character/castBundle` 不直接輸出 Ink knot 的規範。
  - sidecar 新欄位（`characterId`、`characterName`、`expressions`、`actorId`、`actorName`、`castInputCount` 等）。
  - 匯入相容策略（舊欄位缺失時的 warning 與重算規則）。
