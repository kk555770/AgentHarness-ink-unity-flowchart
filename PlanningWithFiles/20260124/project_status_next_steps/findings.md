# 盤點與發現（ink-unity-integration）

## 需求（從使用者請求整理）
- 使用 `planning-with-files` 建立任務檔（`task_plan.md` / `findings.md` / `progress.md`）。
- 重新理解：專案目的、目前進度、下一步。
- 先提案，等你同意後才開始改代碼。

## 初步觀察（已確認的事實）
- 這是一個 Unity 專案（有 `Assets/`、`Packages/`、`ProjectSettings/`）。
- 專案根目錄存在 `PlanningWithFiles/`，目前有 `20260120/` 與 `20260122/` 兩次紀錄。
- 目前 Git 工作區是髒的（有多個已修改/新增/刪除檔案），之後若要動代碼要特別小心避免混到不相關變更。

## 研究與閱讀發現（持續補）
- 已讀：`PlanningWithFiles/20260122/OpsidanosInk/task_plan.md`、`PlanningWithFiles/20260122/OpsidanosInk/findings.md`
  - 當時任務目標：讓 Unity Console 不再出現 `CS0118`、`DirectoryNotFoundException`、`Samples~` 相關警告，並且套件內只用代號 `Opsidanos`。
  - 當時修的重點檔案（計畫上）：`Packages/com.opsidanos.ink/Runtime/Scripts/Story/InkStoryEngine.cs`、`Packages/Ink/Editor/Core/InkEditorUtils.cs`，以及 `Samples~` 與 `package.json` samples 設定。
  - 重要發現：
    - `Packages/com.opsidanos.ink` 這個資料夾路徑會以 `.ink` 結尾，可能被第三方工具誤判成 Ink 檔。
    - `Story` 容易在命名空間/型別之間撞名（例如 `Ink.Runtime.Story`）。
    - `.gitignore` 的 `*~` 會把 UPM 的 `Samples~` 忽略掉，容易造成 Unity 警告（`.meta` 與資料夾不同步）。
    - 不建議放 `Samples~.meta`，否則 Unity 會檢查資料夾一致性而噴警告。
  - 決策：
    - 套件內作者/註解人名只用代號 `Opsidanos`。
    - 提供最小 `Samples~/PlayerModeSample`（只有 `story.ink`）讓玩家模式可快速匯入使用。
- 待補：閱讀 `PlanningWithFiles/20260120/**`、`PlanningWithFiles/20260122/**` 其他主題後更新。
- 已讀：`PlanningWithFiles/20260122/TextAdventureEngine/task_plan.md`、`PlanningWithFiles/20260122/TextAdventureEngine/findings.md`
  - 這是承接 `20260120/TextAdventureEngine` 的續作。
  - 當時目標（2026-01-22）：先把玩家模式（Runtime）MVP Phase 3 的 4 件事做完：回看（Backlog）、Auto/Skip、隱藏 UI、Tag → 結構化資料 → 事件管線。
  - Phase 4～7（演出資源綁定 / 存讀檔 / Editor Flow Chart / 示範驗收）當時仍是 pending。
  - 當時「需要避免再踩的坑」：
    - UI Toolkit `ScrollView`：要用 `scrollView.contentContainer.childCount` 才是你塞的項目數
    - Unity `Start()` 順序：Presenter 在 `Awake()` 抓 UXML，避免漏掉 `InkStoryEngine` 在 `Start()` 先吐的第一句
    - UI Toolkit USS：不支援 `z-index`，疊層靠 UXML 順序＋`position: absolute`
- 待補：閱讀 `README.md`、`Documentation/`、`Packages/manifest.json` 後更新。
- 已讀：`PlanningWithFiles/20260120/TextAdventureEngine/task_plan.md`、`PlanningWithFiles/20260120/TextAdventureEngine/findings.md`
  - 這是目前「整個專案最核心的計畫」：在 Unity 專案 `ink-unity-integration` 中，做出可重用的「文字冒險/視覺小說」引擎（UPM package），分成：
    - 開發者模式（Editor）：用 Flow Chart 做內容 → 輸出 `.ink`（可版本控管）
    - Ink Unity Integration：把 `.ink` 編譯成 `.json`
    - 玩家模式（Runtime）：只讀 `.json` 播放（Runtime 不依賴 Flow Chart 資產）
  - v1 需求重點（當時已明確）：
    - 平台：PC + WebGL（第一版）
    - 互動：只有選項（Choice）
    - UI：Runtime + Editor 都用 UI Toolkit
    - 演出：用 Tag 驅動（背景/立繪/BGM/SE/UI/文字效果/畫面特效/自訂事件）
    - 玩家模式功能：文字/選項/回看/Auto/Skip/存讀檔/倒帶/設定/隱藏 UI
  - 優化原則（為了更省力且好維護）：
    - `.ink` 只放故事與 Tag；Flow Chart 編輯資料另外存 sidecar（例如 `story.ink.flow.json`）
    - Tag 先解析成結構化資料再交給各系統處理
    - Runtime 維護 `PresentationState`（背景/立繪/BGM/UI 開關等），Tag 只改它；存檔/倒帶也只存它
    - 出錯用 `Debug.LogError` 指到作者能修的地方，不在 Play Mode 自動修資料
  - 當時進度（2026-01-20 文件記錄）：
    - Phase 3（玩家模式 MVP）多數已完成，但「在 Unity 驗證功能」仍待你操作確認
    - Phase 4～7（演出資源綁定 / 存讀檔/倒帶 / Editor Flow Chart / 示範驗收）仍是 pending
- 已讀：`PlanningWithFiles/20260120/TextAdventureEngine/progress.md`、`PlanningWithFiles/20260122/TextAdventureEngine/progress.md`
  - Phase 3（玩家模式 MVP）已做的內容（文件記錄）：
    - UPM 套件：`Packages/com.opsidanos.ink`（顯示名 `OpsidanosInk`）
    - Runtime 播放：`InkStoryEngine` 讀 `.json` 建立 `Ink.Runtime.Story`，輸出 `StoryOutput`
    - UI Toolkit：`VNPlayerPresenter` + `VNPlayer.uxml/.uss`（名字/文字/選項）
    - 操作：回看（Backlog）/ Auto / Skip / 隱藏 UI
    - Tag 管線：`InkTag` / `InkTagParser`，解析後放進 `StoryOutput.ParsedTags`
  - Sample：
    - `Packages/com.opsidanos.ink/Samples~/PlayerModeSample/story.ink` 有加入 `bg/bg m/se/shake` 等 Tag 範例（先打通管線，尚未做資源綁定）
  - 明確待辦（需要你在 Unity 操作驗證）：
    - 回看 / Auto / Skip / 隱藏 UI 在 Play Mode 的體感與正確性
- 已讀：`PlanningWithFiles/20260122/OpsidanosInk/progress.md`
  - 記錄確認：`Samples~`、`.gitignore`、`package.json`、`Samples~.meta` 等問題已處理，且你已在 Unity 驗證 Console 乾淨。
- 已讀：`README.md`
  - 這個 repo 的基底是 ink 官方的 Unity 整合套件：提供 Ink 編譯（`.ink` → `.json`）、Ink Player Window（Editor 播放/除錯）、自動編譯、Inspector 工具等功能。
- 已讀：`Packages/com.opsidanos.ink/README.md`
  - `com.opsidanos.ink` 是我們加在這個專案上的 UPM 套件：用 Ink + UI Toolkit 做可重用的「文字冒險/視覺小說」框架。
  - 文件寫明「核心管線（新規定）」與 `TextAdventureEngine` 計畫一致：Editor Flow Chart → `.ink` → 編譯 `.json` → Runtime 只讀 `.json`
  - 文件提供「玩家模式快速開始」：在場景放 `UIDocument` + `InkStoryEngine` + `VNPlayerPresenter`，並指定 `VNPlayer.uxml` 與 `story.json`
  - 文件列的「目前功能（最小骨架）」：讀 `.json`、UI 顯示名字/文字/選項
- 已讀：`Packages/Ink/README.md`（前 200 行）
  - 內容與根目錄 `README.md` 同方向：說明 Ink Unity Integration 的安裝與功能概覽。
- 已讀：`Packages/manifest.json`
  - 專案有安裝 `com.coplaydev.unity-mcp`（從 GitHub URL），以及一些 Unity 內建/官方套件（InputSystem、Test Framework 等）。
- 已讀：`Packages/com.opsidanos.ink/package.json`
  - `com.opsidanos.ink` 版本 `0.1.0`，目標 Unity `6000.3`
  - 依賴：`com.inkle.ink-unity-integration` `1.2.1`
  - Samples：`玩家模式快速開始`（`Samples~/PlayerModeSample`）
- 已讀：`Packages/Ink/package.json`
  - 這個 repo 內的 `Packages/Ink` 就是 `com.inkle.ink-unity-integration`（版本 `1.2.1`），符合 `com.opsidanos.ink` 的依賴版本。
- 已盤點：`Packages/com.opsidanos.ink/Runtime/` 目前內容（最小骨架）
  - Story：`InkStoryEngine.cs`、`StoryOutput.cs`、`InkTag.cs`、`InkTagParser.cs`
  - UI：`VNPlayerPresenter.cs`、`VNPlayer.uxml`、`VNPlayer.uss`
- 已讀：`InkStoryEngine.cs`、`VNPlayerPresenter.cs`、`StoryOutput.cs`、`InkTag.cs`、`InkTagParser.cs`
  - Runtime 故事推進：
    - `InkStoryEngine` 在 `Start()` 用 `TextAsset storyJsonAsset.text` 建 `Ink.Runtime.Story`，並立刻吐第一個 `StoryOutput`
    - 透過事件 `OutputGenerated` 把每次輸出丟給 UI（含：speaker、lineText、choices、tags、parsedTags、hasEnded）
  - UI（玩家模式 Presenter）：
    - `VNPlayerPresenter` 在 `Awake()` 就抓 UXML 元件並綁按鈕事件，避免漏掉第一句
    - 目前已包含：回看面板（ScrollView）、Auto/Skip 協程推進、隱藏 UI（加/移除 USS class）
  - Tag 結構化：
    - `InkTagParser` 以 `key:value` 解析 raw tags，解析錯誤用 `Debug.LogError` 指出
- 已讀：`Packages/com.opsidanos.ink/Runtime/UI/UXML/VNPlayer.uxml`、`Packages/com.opsidanos.ink/Runtime/UI/USS/VNPlayer.uss`
  - UXML 結構：TopBar（回看/自動/快轉/隱藏）、Main（對話面板＋ChoicesContainer）、BacklogPanel（absolute）、ShowUIButton（absolute）
  - USS 重點：
    - `.vn-hidden` 用 `display: none` 隱藏元素（切換選項/下一句）
    - `.vn-ui-hidden` 會把 TopBar/Main/Backlog 都隱藏，並顯示右下角的 `.vn-show-ui` 按鈕

## 技術決策（目前先不做）
| 決策 | 原因 |
|------|------|
|      |      |

## 問題與卡點（持續補）
| 問題 | 目前狀態 |
|------|----------|
| 工作區有未提交變更 | 先盤點清楚，再決定下一步是否要在這個狀態下繼續 |

## 資源（之後補文件與重要檔案路徑）
- `README.md`
- `Documentation/`
- `Packages/manifest.json`
- `PlanningWithFiles/20260120/`
- `PlanningWithFiles/20260122/`
 - `UIToolkitSpec.md`
 - `Packages/com.opsidanos.ink/README.md`
- `Packages/com.opsidanos.ink/CHANGELOG.md`
- `Packages/com.opsidanos.ink/Samples~/PlayerModeSample/story.ink`
- `Documentation/InkPlayerWindow.md`
 - `Assets/Scene/Test.unity`
 - `Assets/OpsidanosInk/Demo/story.ink`
 - `Assets/OpsidanosInk/Demo/story.json`

## UI Toolkit 規格提醒（本專案規定）
- 目前專案版本目標：Unity 6000.3.x（文件寫 6000.3.2f1）
- USS 不支援 `z-index`；若用到要改成用「UXML 順序＋position」處理疊層。

## Sample Tag 現況（方便做 Phase 4）
- `Packages/com.opsidanos.ink/Samples~/PlayerModeSample/story.ink` 目前 Tag 範例：
  - `speaker:旁白`
  - `bg:room_01`
  - `bgm:opening`
  - `se:open`
  - `shake`（只有 key，沒有 value）
- 注意：若故事是用 `=== start ===` 開頭，最上面要加一行 `-> start`，否則故事可能會從「空的 root」開始而立刻結束。

## 本次驗證結果（Unity Console）
- Tag Router（`InkTagEventRouter`）已在 Play Mode 驗證可正常輸出：
  - `OutputId=1`：`speaker:旁白`
  - `OutputId=2`：`speaker:旁白, bg:room_01, bgm:opening`
  - 後續分支：
    - 走左邊可看到：`se:open`
    - 走右邊可看到：`bg:outside, shake`
- Tag log 結尾已修正：故事結束那一筆不再重複印上一句的 Tag
- Console 警告/錯誤：0

## Phase 8：音訊綁定（BGM/SE）
- 已新增 `InkTagAudioPlayer`（接 `bgm` / `se` tag 播放音樂/音效，並輸出 `[OpsidanosInk][BGM]` / `[OpsidanosInk][SE]` log）
- 已把 `Assets/Scene/Test.unity` 的 `VNPlayer` 接好預設對照：
  - `bgm:opening` → `Assets/NewResources/Audio/BGM/ukulele_song.mp3`
  - `se:open` → `Assets/NewResources/Audio/Sound/click.wav`
- 已驗證：Play Mode 走到 Tag 出現時，Console 會印出 `[BGM]` / `[SE]`，並且聽得到聲音
  - Console：`[OpsidanosInk][BGM] opening -> ukulele_song`
  - Console：`[OpsidanosInk][SE] open -> click`

## Unity MCP 操作發現（音訊）
- 同一個 GameObject 上有 2 個 `AudioSource` 時，`manage_components` 設值只會改第一個（第二個要用別的方式或拆到不同 GameObject）
- 若要用 Unity MCP 直接塞 `List<SerializableClass>`，資料類型用 public fields 會比較穩定（本次 `ClipBinding` 改成 `Id/Clip` 公開欄位後才成功寫入）
- `AudioClip` 參考可以直接用資源路徑字串（例：`Assets/NewResources/Audio/BGM/ukulele_song.mp3`）

## Phase 9：UI（1920x1080）
- UI Toolkit 的 USS 長度單位主要是 `px` / `%`，要「解析度變大 UI 跟著變大」主要靠 `PanelSettings` 的 `Scale With Screen Size`
- 本專案目前 `PanelSettings.asset` 已是 `Scale With Screen Size` 且 `ReferenceResolution=1920x1080`
- 本次處理方式：把 `VNPlayer.uss` 的字體與按鈕 padding 加大，讓 1920x1080 看起來舒服；解析度更大時也會跟著放大

## 已處理：last-child 警告
- 警告來源：Unity MCP 套件（`com.coplaydev.unity-mcp`）的 `Common.uss` 使用 `:last-child`
- 已移除 `:last-child` selector，Console 已不再出現該警告
- 注意：這個修正在 `Library/PackageCache/`，如果你清掉 `Library/` 或 Unity 重新解析套件，可能會被覆蓋回去

## 新增資源盤點（2026/01/25 重新掃描）
- 圖片（背景）：`Assets/NewResources/Image/BG/`（共 23 張，且都有 `.meta`）
  - 例：`room.png`、`street.png`、`default.png`、`neighborhood_day.png`、`office.png`...
- 圖片（角色）：`Assets/NewResources/Image/Characters/`（多張，含子資料夾）
  - 例：`alice_happy.png`、`alice_normal.png`、`BS/BS_Stand.png`...
- 圖片（CG）：`Assets/NewResources/Image/CG/`（共 1 張）
  - 檔名：`alice_happy_cg.png`
- 圖片（UI 按鈕）：`Assets/NewResources/Image/btn/`（共 10 張）
  - 檔名：`btn_auto`、`btn_auto_on1`、`btn_auto_on2`、`btn_auto_on3`、`btn_back`、`btn_hide`、`btn_index`、`btn_lobby`、`btn_setting`、`btn_skip`
- 圖片（標題）：`Assets/NewResources/Image/title/`（共 1 張）
  - 檔名：`title_zh.png`
- 重要發現：這批 BG／角色／CG 的 `.png.meta` 目前是 `textureType: 0`（預設 `Texture2D`，不是 `Sprite`），所以**任何 `Sprite` 欄位**都會拖不進去，最後會是 `null`。
- Unity MCP 查資產小抄：`manage_asset` 的 `search_pattern` 要用像 `t:Texture2D` 這種 FindAssets filter；用 `*.png` 會找不到。
- 注意：目前故事 Tag 範例有 `bg:room_01` / `bg:outside`，已在 `Assets/Scene/Test.unity` 重新設定對照（避免引用到已刪除的 `0.png/1.png`）。

## 下一步提案（待你同意後才會開始改代碼）

## 8 歲版本摘要（用很簡單的話說）
- 你寫故事用 `.ink`，Unity 會把它「編譯」成 `.json`（這樣才可以在遊戲裡播放）。
- 這個專案有兩層：
  - `Packages/Ink`：官方的 Ink Unity 工具（負責編譯、除錯工具等）
  - `Packages/com.opsidanos.ink`：我們自己做的「文字冒險/視覺小說播放器」（用 UI Toolkit）
- 目前玩家模式已經能做：
  - 顯示名字/文字/選項
  - 回看、自動、快轉、隱藏 UI
  - 把 Tag 解析成結構化資料（例如 `bg:room_01`）
- 目前還沒做（後面的大任務）：
  - Tag 真的去控制背景/立繪/BGM/SE 等演出
  - 存檔/讀檔/倒帶
  - 開發者模式（Editor）Flow Chart 工具

### 提案 A（推薦）：Tag 事件管線（先只做路由＋除錯輸出，不做資源綁定）
- 修改範圍：
  - 新增 `com.opsidanos.ink` Runtime 腳本：把 `StoryOutput.ParsedTags` 轉成「明確的事件」與清楚的 `Debug.Log`
  - 不改動 UXML/USS（UI 外觀不變）
- 現在狀況：
  - Tag 已經被解析成 `ParsedTags`，但目前只有 `speaker` 被使用；`bg/bgm/se/shake` 還沒有任何行為
- 預期效果：
  - Play Mode 時，Console 能看到「哪一行觸發了哪些 Tag」的清楚訊息
  - 後續要做 Phase 4（背景/立繪/BGM/SE）時，可以直接接這些事件，不用再到處判斷字串
- 需要你測試：
  - 進 Play Mode 播放 Sample，確認 Console 會依照 `story.ink` 的 Tag 正確輸出（遇到選項、結束也要正常）

### 提案 B：演出最小綁定（bg/bgm/se/shake → 真的會動作）
- 狀態：已完成
- 已做內容（實作結果）：
  - `bg`：背景切換已可動（改用 `Texture2D`，並已接好 `room_01/outside`）
  - `bgm` / `se`：音樂/音效已可動
  - `shake`：目前只有 Tag log，尚未做視覺特效

### 提案 C：立繪＋CG 最小綁定（char/cg → 真的會動作）
- 狀態：已完成（最小綁定）
- 修改範圍：
  - 擴充 `InkTagEventRouter`：新增 `char-left/char-center/char-right/cg` 的事件路由
  - 新增 Runtime 元件：
    - `InkTagCharacterPlayer`：接 `char-left/center/right:<id>`，把立繪顯示在 `CharacterLayer`（left/center/right）
    - `InkTagCgPlayer`：接 `cg:<id>` / `cg:clear`，把 CG 顯示在 `CgLayer`
  - 更新 `VNPlayer.uss`：補齊角色槽的顯示規則（用 `Texture2D`，不使用 `z-index`）
  - 更新 Demo story：加上 `char-*` / `cg` Tag，讓你一跑就看得到效果
  - 用 Unity MCP 更新 `Assets/Scene/Test.unity`：把元件掛上並接好對照（id → Texture2D）
- 現在狀況：
  - UI 分層已經有 `CharacterLayer` / `CgLayer`，目前已接上 Tag，可用 Demo story 直接驗證立繪/CG 的顯示與切換
- 預期效果：
  - Play Mode 時，走到 Tag 就會看到立繪/CG 出現或切換，Console 會印出對應 log

## 圖片比例修正（2026/01/25）
- 問題：CG／立繪（以及可能的 BG）在 UI 上看起來有「變形」，沒有維持原圖比例。
- 目前觀察（根因）：
  - 這批「會變形」的貼圖（BG／角色／CG／title）在 `.png.meta` 裡是 `nPOTScale: 1`。
  - 這代表 Unity 會在匯入時把「不是 2 的次方大小」的圖片，改成「最近的 2 的次方」大小。
  - 例子（實際檔案尺寸）：
    - `alice_happy_cg.png` 原本是 `1536x1024`，若被改成 `1024x1024` 或 `2048x1024`，圖片就會被擠壓變形。
    - `alice_normal.png` 原本是 `672x1568`，若被改成 `512x2048`，寬高比例就會被改掉。
  - 所以就算 UI 用 `scale-to-fit`，貼圖本身已經被 Unity 匯入時改形狀了，畫面看起來還是會變形。
- 作法（下一步要做的修正）：
  - 把這些貼圖的 `nPOTScale` 改成 `0`（None），讓 Unity 不要在匯入時改變貼圖尺寸比例。
  - 範圍：`Assets/NewResources/Image/BG/**`、`Assets/NewResources/Image/Characters/**`、`Assets/NewResources/Image/CG/**`、`Assets/NewResources/Image/title/**` 裡目前是 `nPOTScale: 1` 的那些 `.png.meta`。
- 已完成：上述範圍內 `nPOTScale: 1` 的 `.png.meta` 已全部改成 `nPOTScale: 0`，等待你在 Unity 重新驗證畫面比例是否恢復正常。
- 預期結果：
  - 貼圖匯入後保持原圖比例，BG／立繪／CG 預設都不會「被拉伸」。
  - 之後要做演出動畫（橫向/直向縮放）仍然可以用 UI Toolkit 的 `scale/transform` 去做。
