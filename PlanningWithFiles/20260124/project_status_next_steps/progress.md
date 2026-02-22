# 進度紀錄（ink-unity-integration）

## Session：2026-01-24

### Phase 1：需求與限制盤點
- **狀態：complete**
- **開始時間：** 2026-01-24 17:32:27 CST
- 做了什麼：
  - 確認專案根目錄結構（存在 `Assets/`、`Packages/`、`ProjectSettings/` 等）
  - 確認 `PlanningWithFiles/` 內已有 `20260120/`、`20260122/` 兩次紀錄
  - 建立本次任務的三個規劃檔案（本檔、`task_plan.md`、`findings.md`）
- 影響到的檔案：
  - `PlanningWithFiles/20260124/project_status_next_steps/task_plan.md`（新增）
  - `PlanningWithFiles/20260124/project_status_next_steps/findings.md`（新增）
  - `PlanningWithFiles/20260124/project_status_next_steps/progress.md`（新增）

### Phase 2：讀既有任務紀錄（PlanningWithFiles）
- **狀態：complete**
- 做了什麼：
  - 閱讀並整理：
    - `PlanningWithFiles/20260120/TextAdventureEngine/`（主計畫 v1）
    - `PlanningWithFiles/20260122/OpsidanosInk/`（修錯任務）
    - `PlanningWithFiles/20260122/TextAdventureEngine/`（v1 續作）
  - 把「專案目的、核心管線、已完成與待完成」寫進 `findings.md`
- 影響到的檔案：
  - `PlanningWithFiles/20260124/project_status_next_steps/findings.md`（更新）

### Phase 3：讀專案說明與結構
- **狀態：complete**
- 做了什麼：
  - 閱讀文件：`README.md`、`Packages/com.opsidanos.ink/README.md`、`UIToolkitSpec.md`、`Packages/manifest.json`、`Packages/**/package.json`
  - 盤點玩家模式最小骨架程式與 UI：`InkStoryEngine` / `VNPlayerPresenter` / `VNPlayer.uxml/.uss`
  - 盤點 Sample 的 Tag 範例：`speaker/bg/bgm/se/shake`
- 影響到的檔案：
  - `PlanningWithFiles/20260124/project_status_next_steps/findings.md`（更新）
  - `PlanningWithFiles/20260124/project_status_next_steps/task_plan.md`（更新）

### Phase 4：整理「專案目的／目前進度／下一步」
- **狀態：in_progress**
- **開始時間：** 2026-01-24 17:39:01 CST
- 做了什麼：
  - 準備用「8歲小孩看得懂」的方式，整理目的/進度/下一步，並產出正式提案
- 影響到的檔案：
  - `PlanningWithFiles/20260124/project_status_next_steps/progress.md`（更新）

### Phase 5：提案（已取得同意）
- **狀態：complete**
- 做了什麼：
  - 已透過 AskUserQuestionTool 取得同意：選擇「提案 A（Tag 事件管線）」
- 影響到的檔案：
  - `PlanningWithFiles/20260124/project_status_next_steps/findings.md`（更新：加入提案 A/B 內容）

### Phase 6：實作（提案 A：Tag 事件管線）
- **狀態：in_progress**
- 做了什麼：
  - 新增 `InkTagEventRouter`：把 `StoryOutput.ParsedTags` 轉成事件，並在 Console 輸出 Tag（可開關 Unknown Tag）
  - 更新 `Packages/com.opsidanos.ink/README.md`：加入可選元件 `InkTagEventRouter` 的使用方式
  - 用 Unity MCP 自動設定 `Assets/Scene/Test.unity`：
    - 在 `VNPlayer` 加上 `UIDocument` / `InkStoryEngine` / `VNPlayerPresenter` / `InkTagEventRouter`
    - 指定 `UIDocument` 的 UXML：`Packages/com.opsidanos.ink/Runtime/UI/UXML/VNPlayer.uxml`
    - 建立 Demo Ink：`Assets/OpsidanosInk/Demo/story.ink`，並用 `Assets > Recompile Ink` 產生 `Assets/OpsidanosInk/Demo/story.json`
    - 把 `InkStoryEngine.storyJsonAsset` 指到 `Assets/OpsidanosInk/Demo/story.json`
    - 把 `VNPlayerPresenter` 與 `InkTagEventRouter` 的 `storyEngine/uiDocument` 引用接到同一個 `VNPlayer` 上的元件
- **結束時間：** 2026-01-24 18:10:26 CST
- 影響到的檔案：
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Story/InkTagEventRouter.cs`（新增）
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Story/InkTagEventRouter.cs.meta`（新增）
  - `Packages/com.opsidanos.ink/README.md`（更新）
  - `Assets/OpsidanosInk/Demo/story.ink`（新增/更新）
  - `Assets/OpsidanosInk/Demo/story.json`（新增/更新：Ink 編譯產物）
  - `Assets/Scene/Test.unity`（更新：VNPlayer 設定完成）
  - `PlanningWithFiles/20260124/project_status_next_steps/task_plan.md`（更新）

### Phase 8：實作（最小演出綁定：BGM/SE）
- **狀態：complete**
- 做了什麼：
  - 新增 `InkTagAudioPlayer`：訂閱 `InkTagEventRouter` 的 `bgm` / `se` 事件，播放音樂/音效並輸出 log
  - 更新 `Packages/com.opsidanos.ink/README.md`：補上音訊綁定的接法與欄位說明
  - 用 Unity MCP 更新 `Assets/Scene/Test.unity`：
    - `VNPlayer` 新增 `AudioSource` * 2（第一個當 BGM、第二個當 SE）
    - `VNPlayer` 新增 `InkTagAudioPlayer`，並把引用與對照表接好
    - 預設對照：
      - `bgm:opening` → `Assets/NewResources/Audio/BGM/ukulele_song.mp3`
      - `se:open` → `Assets/NewResources/Audio/Sound/click.wav`
- 影響到的檔案：
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Presentation.meta`（新增）
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/InkTagAudioPlayer.cs`（新增/更新）
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/InkTagAudioPlayer.cs.meta`（新增）
  - `Packages/com.opsidanos.ink/README.md`（更新）
  - `Assets/Scene/Test.unity`（更新：VNPlayer 加入音訊元件並接好欄位）

### Phase 9：實作（UI 1920x1080）
- **狀態：complete（你已驗證畫面）**
- 做了什麼：
  - 更新 `VNPlayer.uss`：把 Label / Button 的基準字體與按鈕 padding 加大，並讓 Speaker / Body / Backlog 更好讀
  - 用 Unity MCP 進 Play Mode 檢查：Console 沒有 warning/error（沒有任何 UI Toolkit 的不支援屬性或 selector 警告）
- 影響到的檔案：
  - `Packages/com.opsidanos.ink/Runtime/UI/USS/VNPlayer.uss`（更新）

### Phase 10：實作（背景切換 bg）
- **狀態：in_progress**
- 做了什麼：
  - 盤點你新增的圖片資源（背景/按鈕/標題），記錄到 `findings.md`
  - 新增 `InkTagBackgroundPlayer`：訂閱 `InkTagEventRouter.BackgroundTagReceived`，把 `bg:id` 對到 `Texture2D` 並切換背景圖（UI Toolkit backgroundImage）
  - 擴充 `InkTagEventRouter`：加入 `char-left/char-center/char-right/cg` Tag 事件路由
  - 新增 `InkTagCharacterPlayer`：接 `char-left/center/right:<id>`，切換 `CharacterLeft/Center/Right` 的立繪
  - 新增 `InkTagCgPlayer`：接 `cg:<id>` / `cg:clear`，顯示/隱藏 `CgLayer` 的 CG
  - 修正圖片比例：讓 BG／立繪／CG（以及未來所有圖像）預設維持原圖比例（並保留可用 `scale/transform` 做橫向/直向縮放動畫的空間）
    - `VNPlayer.uxml`：`Background`/`CgImage` 改用 `VisualElement`（統一用 `backgroundImage` 顯示貼圖）
    - `VNPlayer.uss`：新增 `vn-image`/`vn-image-bottom` class，預設 `scale-to-fit`，並設定 `transform-origin`
  - 更新 Demo/Sample story：加入 `char-*` / `cg` Tag，並執行 `Assets/Recompile Ink` 重新編譯
  - 更新 `Packages/com.opsidanos.ink/README.md`：補上「Tag 演出（最小：背景）」使用方式
  - 用 Unity MCP 更新 `Assets/Scene/Test.unity`：
    - `VNPlayer` 加上 `InkTagBackgroundPlayer`
    - 設定對照：`room_01` → `Assets/NewResources/Image/BG/room.png`、`outside` → `Assets/NewResources/Image/BG/street.png`
    - `VNPlayer` 加上 `InkTagCharacterPlayer` / `InkTagCgPlayer`，並設定 id→貼圖對照
  - 用 Unity MCP 進 Play Mode：Console 沒有新增 error/warning（背景 Tag 需要你點到下一句才會觸發）
  - 圖片「仍然變形」的根因確認：
    - 檢查 `.png.meta` 發現 BG／角色／CG／title 多數是 `nPOTScale: 1`（匯入時會把圖片改成最近的 2 次方尺寸，導致寬高比例被改掉）
    - 這會讓貼圖本身被擠壓，所以 UI 端怎麼顯示都會看起來變形
    - 下一步要改：把這些貼圖的 `nPOTScale` 改成 `0`（None）後再驗證
  - 已完成修正：把 `Assets/NewResources/Image/BG/**`、`Assets/NewResources/Image/Characters/**`、`Assets/NewResources/Image/CG/**`、`Assets/NewResources/Image/title/**` 這批 `nPOTScale: 1` 的 `.png.meta` 全部改成 `nPOTScale: 0`
  - 已用 Unity MCP 觸發 assets refresh：Console 沒有新增 error/warning
  - 新增 `InkTagShakePlayer`：訂閱 `InkTagEventRouter.ShakeTagReceived`，讓純 `# shake` 會讓背景抖動（不影響對話框/選項）
  - 更新 Demo story：新增 `shake_test` 分支（畫面抖動測試），並重新編譯 Ink
  - 更新 `Packages/com.opsidanos.ink/README.md`：補上「Tag 演出（最小：畫面抖動）」接法與欄位說明
  - 用 Unity MCP 更新 `Assets/Scene/Test.unity`：
    - `VNPlayer` 加上 `InkTagShakePlayer`，並把 `tagEventRouter/uiDocument` 引用接好
  - 跑 EditMode Test：`OpsidanosInk.Tests.CharTagJsonTests.CharTagJson_DemoStory_SequenceIsParsable`（通過）
- 影響到的檔案：
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/InkTagBackgroundPlayer.cs`（新增/更新：Sprite → Texture2D）
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/InkTagCharacterPlayer.cs`（新增）
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/InkTagCgPlayer.cs`（新增）
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/InkTagShakePlayer.cs`（新增）
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/InkTagShakePlayer.cs.meta`（新增）
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Story/InkTagEventRouter.cs`（更新：char/cg 路由）
  - `Packages/com.opsidanos.ink/Runtime/UI/UXML/VNPlayer.uxml`（更新：圖片顯示 class 與元素型別）
  - `Packages/com.opsidanos.ink/Runtime/UI/USS/VNPlayer.uss`（更新：角色槽顯示規則）
  - `Assets/OpsidanosInk/Demo/story.ink`（更新：新增 char/cg Tag + shake_test）
  - `Assets/OpsidanosInk/Demo/story.json`（更新：Ink 編譯產物）
  - `Packages/com.opsidanos.ink/Samples~/PlayerModeSample/story.ink`（更新：新增 char/cg Tag）
  - `Packages/com.opsidanos.ink/README.md`（更新）
  - `PlanningWithFiles/20260124/project_status_next_steps/findings.md`（更新：新增資源盤點）
  - `Assets/Scene/Test.unity`（更新：新增背景綁定元件與對照 + shake 綁定）

## 測試結果
| 測試 | 輸入 | 預期 | 實際 | 狀態 |
|------|------|------|------|------|
|      |      |      |      |      |

## Unity 驗證紀錄
| 測試 | 輸入 | 預期 | 實際 | 狀態 |
|------|------|------|------|------|
| Tag Router 起始輸出 | Play Mode（Test 場景） | Console 出現 `[OpsidanosInk][Tag]` | 已看到 `OutputId=1 Tags=speaker:旁白` | ✓ |
| Tag Router 推進/分支輸出 | 點「下一句」＋選擇分支 | 依故事 Tag 依序出現 | 看到：`OutputId=1 speaker`、`OutputId=2 speaker+bg+bgm`、`OutputId=4 speaker+bg+shake`（本次走右邊分支） | ✓ |
| Console 警告檢查（修正前） | Play Mode | 沒有紅字錯誤 | 只有 1 筆警告：`Unknown pseudo class \"last-child\" in StyleSheet Common` | △ |
| Console 警告檢查（修正後） | Play Mode | Console 乾淨 | 警告/錯誤：0 | ✓ |
| Tag log 結尾不重複 | Play Mode 跑到 END | 結尾不會再重複印上一句 Tag | 本次走左邊：最後一筆 Tag log 是 `OutputId=4 speaker+se`（故事結束那筆不再印 Tag） | ✓ |
| 音訊綁定起始 | Play Mode（Test 場景） | Console 沒有 `[OpsidanosInk] ... 尚未指定` 的錯誤 | 已進入 Play Mode，沒有看到相關錯誤 | ✓ |
| 音訊綁定（BGM） | 點「下一句」到出現 `bgm:opening` | 會看到 `[OpsidanosInk][BGM] opening -> ...` 並聽到音樂 | Console：`[OpsidanosInk][BGM] opening -> ukulele_song`，你已確認聽得到音樂 | ✓ |
| 音訊綁定（SE） | 走到出現 `se:open` 的那句 | 會看到 `[OpsidanosInk][SE] open -> ...` 並聽到音效 | Console：`[OpsidanosInk][SE] open -> click`，你已確認聽得到音效 | ✓ |
| UI 字體大小（1920x1080） | Play Mode（GameView=1920x1080） | 名字/內文/按鈕文字大小更舒服 | 你回報：基本上沒有問題 | ✓ |
| 背景切換（BG） | 推進到有 `bg:room_01` / `bg:outside` 的那句 | 背景會換，Console 會印 `[OpsidanosInk][BG] ...` | Console：`[OpsidanosInk][BG] room_01 -> room`、`[OpsidanosInk][BG] outside -> street`；你回報：沒有異常 | ✓ |
| 立繪/CG Tag 起始 | Play Mode（Test 場景起始句） | Console 出現 `char-*` / `cg` 的 log | Console：`[OpsidanosInk][Char] left alice_normal -> alice_normal`、`[OpsidanosInk][CG] clear`、`[OpsidanosInk][Tag] OutputId=1 ... char-left ... cg:clear` | ✓ |
| 圖片比例（nPOTScale） | 修改 `.png.meta` 後再進 Play Mode | BG／立繪／CG 不變形（保持原圖比例） | 你回報：已確認不變形 | ✓ |

### 補記：VN UI 分層與主題樣式（另見 `PlanningWithFiles/20260124/vn_layered_ui_refactor/`）
- 已把 `VNPlayer.uxml` 改成 1~13 分層骨架（保留 Presenter 依賴的 name）。
- 已把 `VNPlayer.uss` 改成對應分層，並套用 MVP 主題的 4 個樣式對應（名字框/對話框/選項容器/選項按鈕）。
- 你回報：基本上沒有問題。

### 補記：新增資源（2026-01-24）
- 圖片資源新增在 `Assets/NewResources/Image/`（背景/按鈕/標題）。
- 之後做 `bg:id` 的背景切換，可以直接用這批背景圖做第一版驗證（細節清單見 `findings.md`）。

### 補記：新增資源（2026-01-25 重新查找）
- 用 Unity MCP `manage_asset` 重新查找：
  - BG：23 張（`Assets/NewResources/Image/BG/`）
  - btn：10 張（`Assets/NewResources/Image/btn/`）
  - CG：1 張（`Assets/NewResources/Image/CG/`）
  - title：1 張（`Assets/NewResources/Image/title/`）
  - Characters：多張（`Assets/NewResources/Image/Characters/`，含子資料夾）
- 發現：BG／角色／CG 目前都是 `Texture2D`（`.meta` 是 `textureType: 0`），所以 `InkTagBackgroundPlayer` 若用 `Sprite` 綁定會一直是 `null`（背景不會真的換圖）。
- 已完成：背景綁定型別改成 `Texture2D`，並重新接 `room_01/outside` 對照到現有背景圖（避免 `0.png/1.png` 已刪除造成空參考）。

### Phase 10 補充：資源映射 ResourceMap（JSON）（2026-01-25）
- 做了什麼：
  - 新增集中式 JSON：`Assets/OpsidanosInk/Demo/resource_map.json`（用現有資源填入 id→assetPath）
  - 新增 Runtime 讀取器：`InkResourceMap`（讀 TextAsset JSON，提供 TryGet* 查詢）
  - 更新各 Tag Player：增加 `resourceMap` 欄位（有指定時優先使用 ResourceMap）
  - 補齊 `.meta` 固定 guid，並更新 `Assets/Scene/Test.unity`：
    - `VNPlayer` 加上 `InkResourceMap` 並指定 `resourceMapJson`
    - 各 Tag Player 的 `resourceMap` 指向同一個 `InkResourceMap`
  - 新增 EditMode Test：`Assets/Editor/Tests/InkResourceMapTests.cs`
  - 更新文件：`Packages/com.opsidanos.ink/README.md` 新增「資源映射（推薦：ResourceMap JSON）」段落
- 目前狀態：
  - 你已在 Play Mode 目視驗證：背景/立繪/CG/BGM/SE 改由 ResourceMap 驅動後仍正常
  - 我已透過 Unity MCP 讀 Console：Error=0；Warning 只有 MCP 自己清理過期 TestJob（與演出無關）

## 記錄：last-child 警告修正
- 時間：2026-01-24 18:22:47 CST
- 根因：`Library/PackageCache/com.coplaydev.unity-mcp@6e9594da7766/Editor/Windows/Components/Common.uss` 使用 `:last-child`
- 解法：移除該 selector（注意：清 `Library/` 或套件重新解析，可能被覆蓋回去）

## 記錄：Tag log 結尾重複修正
- 時間：2026-01-24 18:25:21 CST
- 根因：故事結束時，Ink 的 `currentTags` 仍停留在上一句，導致 `HasEnded=true` 那一筆輸出沿用上一句 Tag → Console Tag log 會重複
- 解法：`InkStoryEngine` 在 `outputHasEnded==true` 時改成送出空的 Tag 列表
- 已驗證：Play Mode 推進到結尾，Tag log 不會再多印一次

## 錯誤紀錄
| 時間 | 錯誤 | 嘗試次數 | 解法 |
|------|------|----------|------|
|      |      | 1        |      |

## 5 個重啟檢查（之後補）
| 問題 | 答案 |
|------|------|
| 我在哪裡？ | Phase 7（已完成） |
| 我要去哪裡？ | Phase 10（Phase 4 視覺演出綁定） |
| 目標是什麼？ | 整理專案目的/進度/下一步，先提案再動代碼 |
| 我學到什麼？ | 看 `findings.md` |
| 我做了什麼？ | 看本檔上方紀錄 |

## 狀態同步（2026-02-06）
- 本任務已全階段完成，文件留存的是當時逐步實作快照。
- 先前段落出現的 `in_progress` 屬於歷史時間點狀態，不代表現在仍未完成。
