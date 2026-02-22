# 任務計畫：角色 Tag（char）JSON 狀態化＋轉場（可測試）

## 目標
- 把角色演出從「每個槽位一條指令」改成「角色層一次輸出一份 JSON 狀態」：
  - 角色（actor）與表情（expr）分開
  - 預設 `replace`：沒寫到的 `left/center/right` 視為 `null`（清掉，不殘留）
  - 支援 `patch`（保留未指定槽位）
- 支援轉場規則：
  - 預設時間都是 `0.3` 秒（可用 JSON 控制）
  - 出現（appear）：透明 → 不透明
  - 移動（move）：從原槽位移到新槽位
  - 消失（disappear）：不透明 → 透明，結束後才移除
  - 預設順序（未指定 steps 時）：出現 → 移動 → 消失
  - `move=0` 代表瞬間移動（出現/消失仍照各自秒數）
  - `fade` 已改名為 `disappear`（目前相容，但會印 Error 提醒改名）
  - 支援 `transition.steps`：
    - `steps` 會照順序一個一個跑
    - 同一個 step 內的 `actions` 會同時開始（例如 `["move","disappear"]`）
    - 這一步有動作的角色會在角色層內暫時置頂（不會蓋過對話框/名字框）
    - 可用 `raise/raiseActors` 控制層級：
      - `raise=false`：本步不自動置頂（有動作也不置頂）
      - `raiseActors=["alice"]`：就算沒有動作，也能「只改層級」
    - 整串 steps 結束後會重置回基準層級（不影響下一串）
- 用「專案內現有的圖片資源」完成對照（alice / bs / ss）
- 提供一段「更複雜」的 Demo Ink 測試情境，能目視檢驗：
  - 多角色同時移動
  - 同名角色換表情仍視為同一人
  - steps 的「循序／同時」效果
  - 瞬間移動（0 秒）

## 目前階段
Phase 4

## Phases

### Phase 1：規格與需求整理
- [x] 整理 `char` JSON Tag 規格（replace/patch、actor/expr、transition）
- [x] 整理「出現→移動→消失」分段規則與預設秒數
- [x] 將規格與例子寫入 `findings.md`
- **狀態：complete**

### Phase 2：程式實作（Tag → 狀態 → 轉場）
- [x] 擴充 `InkTagEventRouter`：新增 `char` tag 事件（並保留/整合舊 `char-left/center/right`）
- [x] 新增角色狀態播放器元件（接 `char` JSON，做 replace、appear/move/disappear；先不做 patch）
- [x] 更新文件（README）補上新 Tag 規格與用法
- [x] 支援 `transition.steps`（可循序/可同時），並加入「動作中置頂、結束重置」
- [x] steps 新增 `raise/raiseActors`：支援「不自動置頂」與「只有改層級」
- **狀態：complete**

### Phase 3：資源對照與測試內容
- [x] 用現有貼圖建立 actor/expr 對照（alice / bs / ss）
- [x] 增加更複雜的 Demo Ink 測試段落（含 appear/move/disappear/instant）
- [x] 增加 raise/raiseActors 的 Demo 段落（含 raise=false、無動作只改層級）
- [x] 重新編譯 Ink（產生 `.json`）
- **狀態：complete**

### Phase 4：Unity 驗證
- [x] 進 Play Mode 目視驗證：出現/移動/消失順序正確，且移動是從原位置開始（需要你協助點選「角色狀態測試」）
- [x] 目視驗證：steps 的循序/同時正確，且動作中的角色會暫時置頂（結束後重置）
- [x] 目視驗證：raise=false 會關閉自動置頂；raiseActors 可在無動作時調整層級
- [x] Console 無 error（含 EditMode Test 與 Play Mode 後再讀 Console）
- [x] 把測試結果記到 `progress.md`
- **狀態：complete**

### Phase 5：交付
- [x] 檢查檔案改動範圍與文件是否齊全
- [x] 更新本任務 `task_plan.md` / `progress.md` 狀態
- **狀態：complete**

## Key Questions（待確認/需定義清楚）
1. `char` 預設 `replace` 時，「沒寫到的槽位」是否一律視為 `null`（清掉）？
2. `transition` 是否要支援 `appear/move/disappear` 分開指定？（預設都 0.3 秒）
3. 若同一個 actor 同時出現在兩個槽位，是否一律視為資料錯誤並 `Debug.LogError`？
4. `transition.steps` 是否允許同一個動作出現多次？（目前規則：每個動作最多出現一次，重複會印 Error）

## Decisions Made
| 決策 | 原因 |
|------|------|
| `char` 使用 JSON（而不是 `char-left/right` 分散指令） | Flow Chart 之後會由元件輸出資料，JSON 最容易管線化與擴張 |
| actor 與 expr 分離 | 同名視為同一角色；表情/姿勢可增減，不需把每張圖當成新角色 |
| 預設 replace、缺省槽位視為 null | 避免殘留（你現在遇到的「左+右都出現」問題） |
| 出現→移動→消失三段 | 出現/移動/消失可各自控制秒數，且移動會從原位置開始，方便演出 |

## Errors Encountered
| Error | Attempt | Resolution |
|-------|---------|------------|
| `AskUserQuestionsPlus/ask_user_question_plus` timeout（600s） | 1 | 改用文字提案＋請你直接回覆「同意/不同意」 |
