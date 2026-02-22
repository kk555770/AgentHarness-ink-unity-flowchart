# 進度紀錄：角色 Tag（char）JSON 狀態化＋轉場

## Session：2026-01-25

### Phase 1：規格與需求整理
- **狀態：complete**
- **開始時間：** 2026-01-25
- 做了什麼：
  - 建立本次任務的 planning 檔案（task_plan / findings / progress）
  - 整理使用者要求：char JSON、actor/expr 分離、replace/patch、出現/移動/消失分段與預設秒數
- 影響到的檔案：
  - `PlanningWithFiles/20260125/char_tag_state_json/task_plan.md`
  - `PlanningWithFiles/20260125/char_tag_state_json/findings.md`
  - `PlanningWithFiles/20260125/char_tag_state_json/progress.md`

### Phase 2：程式實作（Tag → 狀態 → 轉場）
- **狀態：complete**
- 做了什麼：
  - 擴充 `InkTagEventRouter`：新增 `char` tag 事件（`CharacterTagReceived`）
  - 新增 `InkTagCharacterStatePlayer`：接 `char:<json>`，做 `replace`＋「出現→移動→消失」三段式轉場
  - 修正移位起點：每個 actor 都有自己的 VisualElement（overlay layer），移動時從原槽位移到新槽位（不會先跳到畫面左邊）
  - 支援 `transition.steps`：可循序/可同時，且「有動作的角色」會在角色層內暫時置頂，整串結束後重置回基準層級
  - 相容舊欄位：`transition.fade` 仍可用，但會 `Debug.LogError` 提醒改名為 `disappear`
  - 更新 `VNPlayer.uss`：改成 `character-actors-layer/character-actor`（overlay layer + actor 本體）
  - 更新文件：`Packages/com.opsidanos.ink/README.md` 補上 char JSON 規格、steps 規格、Ink `{}` 逃逸注意事項
  - 更新 Demo story：加入 steps 範例（循序/同時）
  - 新增/更新 EditMode Test：驗證 Demo story 的 `char` tag 值可被 JSON 解析（含 steps）
  - 發現 Ink 的 `{}` 需要在 `.ink` 內用 `\{ \}` 逃逸，否則輸出不是完整 JSON；已修正 Demo story
- 影響到的檔案：
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Story/InkTagEventRouter.cs`
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/InkTagCharacterStatePlayer.cs`
  - `Packages/com.opsidanos.ink/README.md`
  - `Packages/com.opsidanos.ink/Runtime/UI/USS/VNPlayer.uss`
  - `Assets/OpsidanosInk/Demo/story.ink`
  - `Assets/Editor/Tests/CharTagJsonTests.cs`

### Phase 3：重新編譯 Ink 與驗證
- **狀態：complete**
- 做了什麼：
  - 重新編譯 `Assets/OpsidanosInk/Demo/story.ink` → `Assets/OpsidanosInk/Demo/story.json`
  - 跑 EditMode Test：`OpsidanosInk.Tests.CharTagJsonTests.CharTagJson_DemoStory_SequenceIsParsable`
  - 讀 Unity Console：確認沒有 error/warning

### Phase 3 補充：steps 層級控制（raise/raiseActors）
- **狀態：complete**
- 做了什麼：
  - steps 新增 `raise` 與 `raiseActors`：
    - `raise=false`：本步不自動置頂（有動作也不置頂）
    - `raiseActors=["alice"]`：就算本步沒有動作，也能「只有改層級」
  - 更新 Demo story：加入 `raise=false` 與「無動作只改層級」的段落
  - 更新文件：`Packages/com.opsidanos.ink/README.md` 補上新欄位說明與例子
  - 更新 EditMode Test：驗證 Demo story 的 steps 內容包含 `raise/raiseActors`
  - 重新編譯 Ink，並再次跑 EditMode Test（通過）

## 測試結果
| 測試 | 輸入 | 預期 | 實際 | 狀態 |
|------|------|------|------|------|
| EditMode Test：CharTagJson_DemoStory_SequenceIsParsable | `Assets/OpsidanosInk/Demo/story.json` | `char` tag 值可被 JSON 解析，且關鍵欄位符合 Demo 預期（含 steps） | 已通過 | ✅ |

### Play Mode 驗證結果（你已操作，我再讀 Console）
- Unity Console（Error）：0 條
- Unity Console（Warning）：1 條（MCP 自己清理過期 TestJob，與 char 演出無關）

## 錯誤紀錄
| 時間 | 錯誤 | 嘗試次數 | 解法 |
|------|------|----------|------|
| 2026-01-25 | `AskUserQuestionsPlus/ask_user_question_plus` 逾時（600s） | 1 | 改用文字提案＋請你直接回覆「同意/不同意」 |
