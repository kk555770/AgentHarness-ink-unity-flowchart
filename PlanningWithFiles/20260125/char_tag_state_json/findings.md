# 發現與紀錄：角色 Tag（char）JSON 狀態化＋轉場

## 需求（使用者明確要求）
- 角色層指令要「一次針對該層刷新」，避免殘留造成左右同時出現。
- Tag 格式要一致、通用、可擴張，之後要能由 Flow Chart 單元元件輸出（所以偏好 JSON）。
- 角色（actor）與表情（expr）要分開：
  - 同名 actor 視為同一個角色
  - expr 是抽象值（文字/代號），再由 mapping 對到圖片
  - expr 可增減，不應把每張圖當成不同角色 id
- 轉場規則要可定義：
  - 預設會移動（move）
  - 預設移動時間 `0.3` 秒
  - 瞬間移動 `0` 秒
  - 出現（appear）預設 `0.3` 秒（可控制）
  - 消失（disappear）預設 `0.3` 秒（可控制）
  - 同一句話的演出順序：**出現 → 移動 → 消失**
  - 只要同一句同時有「移動 + 消失」，若都用預設，總時間 `0.6` 秒（0.3 + 0.3）
- 要用「實際存在的資源」並提供「複雜一點」的測試情境方便驗證。

## 現況（已知問題根因）
- 目前 `char-left/center/right` 是「只改單一槽位」，沒有規格說其他槽位要清掉，所以會殘留。
- 要解：把角色層變成「狀態（state）」指令，一次描述 left/center/right。

## 建議規格（草案，待實作）

### 1) `char` Tag 形式（JSON）
- Tag 例：`# char:{"left":{"actor":"bs"},"center":{"actor":"alice","expr":"normal"}}`
- 預設為 `replace`：
  - 沒寫到的 `left/center/right` 視為 `null`（清掉）
- `mode:"patch"` 先不做（目前會 `Debug.LogError` 提醒），避免「缺省欄位」在 Unity JSON 解析時無法分辨「沒寫」與「寫 null」。

### 2) Slot 物件（actor/expr）
- Slot 值：`{"actor":"alice","expr":"happy"}`
- `expr` 可省略：使用該 actor 的 DefaultExpr（由 mapping 決定）

### 3) 轉場（transition）
- 預設：
  - `appearDuration = 0.3`
  - `moveDuration = 0.3`
  - `disappearDuration = 0.3`
  - 預設會 move（同一個 actor 從上一句 A 槽位到下一句 B 槽位）
- `transition` 例：
  - 移動 1 秒：`"transition":{"move":1}`
  - 出現 0.5 秒：`"transition":{"appear":0.5}`
  - 消失 0.5 秒：`"transition":{"disappear":0.5}`
  - 瞬間移動：`"transition":{"move":0}`（出現/消失仍照各自秒數）
- `transition.steps`（可由 Flow Chart 輸出）：
  - `steps` 會照順序一個一個執行
  - 同一個 step 內的 `actions` 會同時開始（例如 `["move","disappear"]`）
  - 演出時「有動作的角色」會在角色層內暫時置頂，整串結束後會重置回基準層級
  - steps 也支援「層級控制」：
    - `raise`（bool，預設 `true`）：`false` 代表本步不自動置頂（有動作也不置頂）
    - `raiseActors`（string[]）：不管有沒有動作，都先把指定 actor 暫時置頂（可用來「只有改層級」）
    - steps 允許「沒有 actions 但有 raiseActors」；但不能兩個都空（會印 Error）
- 分段規則：
  - Phase A：出現（appear）→ 新出現的角色 opacity 0→1
  - Phase B：移動（move）→ 角色從原槽位移到新槽位
  - Phase C：消失（disappear）→ 要移除的角色 opacity 1→0，結束後才從畫面移除

## 實作選擇（2026/01/25）
- JSON 解析：使用 `JsonUtility.FromJson`（transition 的 `appear/disappear/move` 預設值用 `-1` 判斷「未提供」）
- 演出策略：
  - 角色本體：每個 actor 都有自己的 `VisualElement`（放在 `CharacterLayer` 的 overlay layer 裡）
  - 移動：用 actor 自己的 `VisualElement` 動 `left/top`，所以一定是「從原位置移到新位置」
  - 出現/消失：用 `opacity` 做淡入淡出；消失結束後才把該 actor 從畫面移除

## Ink Tag JSON 注意事項（重要）
- Ink 的 `{ ... }` 在文字內容中代表「inline logic」，所以**不能直接把 JSON 的 `{` `}` 放進 tag**，否則輸出的字串會被編譯器拆解，變成不完整的 JSON。
- 解法：在 `.ink` 裡用反斜線逃逸 `{` `}`，讓它們變成「純文字」：
  - 例：`# char:\{\"left\":\{\"actor\":\"bs\"\}\}`

## 實際可用的圖片資源（目前 repo 內）
- alice：
  - `Assets/NewResources/Image/Characters/alice_normal.png`
  - `Assets/NewResources/Image/Characters/alice_happy.png`
  - `Assets/NewResources/Image/Characters/alice_shy.png`
- bs：`Assets/NewResources/Image/Characters/BS/BS_Stand.png`
- ss：`Assets/NewResources/Image/Characters/SS/SS_Stand.png`

## 測試情境（應涵蓋）
- 同一 actor 連續換位（center→right→left）
- 同一 actor 換 expr（normal→happy）仍視為同一人（移動時使用新 expr）
- 三人同時換位（交換站位）
- 同一句同時有「出現 + 移動」（驗證 appear + move 的順序）
- 同一句同時有「出現 + 移動 + 消失」（驗證三段順序與秒數）
- 同一句同時有「移動 + 消失」（驗證 move + disappear 的順序）
- 同一句指定瞬間（0 秒）
