# Findings & Decisions

## Requirements
- 使用者要在 GraphToolkit 節點內用「點擊後下拉」選格式，不靠手動背語法。
- 節點名稱要更白話、繁體中文，且可由程式內集中變數控制。
- 不能破壞既有 Graph v2 規範與閉環（`.inkfc ⇄ .flowchart.json/.ink`）。

## Research Findings
- 目前 `InkFlowActionNode` 只有 `Content` 字串欄位，沒有內容型別下拉。
- 目前 `choice`、`condition` 已有可下拉 enum 欄位（`ChoiceMode`）可作為實作模式。
- 匯出/匯入 DTO 目前沒有 `actionKind`，所以 UI 選擇無法 round-trip 保留。
- 契約文件目前定義節點型別與 sidecar 最小欄位，但未定義 Action 類型下拉。
- `run_tests(mode=EditMode)` 若不加篩選，會連 package 的 Editor 測試一起跑，可能觸發場景建立/切換與大量編譯。
- `Packages/com.unity.ai.navigation/Tests/Editor` 內含會建立/儲存/開啟場景的測試，正是暴走來源之一。
- 目前 `OpsidanosInkTestRunnerMenu.cs` 雖有「只跑我們的」入口，但沒有 GraphToolkit 專用最小範圍入口與硬性 timeout 規則。
- 使用固定篩選（assembly=`OpsidanosInk.EditModeTests` + category=`GraphToolkitFlowSafe` + fixture regex）後，
  可穩定只命中目標 10 個測試，不再觸發 package 場景測試鏈。
- GraphToolkit `UserNodeModelImp.Title` 直接回傳 `m_Node?.GetType().Name`，因此節點標題預設就是類別名。
- 目前 `.inkfc` fixture 仍可見舊英文類別名（`InkFlowStartNode` 等），代表先前的標題覆寫機制未真正改到標題來源。

## Technical Decisions
| Decision | Rationale |
|----------|-----------|
| 新增 `InkFlowActionKind` enum | 直接提供下拉選單，降低使用者手動輸入負擔 |
| 新增 `ExportNodeDto.actionKind` | 確保匯入再匯出後設定不遺失 |
| 既有 node `type` 不改（仍為 start/action/comment/choice/condition） | 避免破壞既有資料與測試 |
| 節點與欄位顯示名集中在同一個設定類別 | 後續改詞彙時可單點維護 |
| GraphToolkit 驗證改成固定 filter（assembly + category + fixture） | 避免下次誤跑整包測試造成 Unity/MCP 失控 |
| Import/RoundTrip 類別加 `[Timeout(60000)]` | 符合測試 60 秒上限，卡住時能快速失敗 |
| 還原 `Assets/OffMeshLinkScene.unity` | 此場景不屬於本次 GraphToolkit 任務，需回復乾淨狀態 |
| 節點中文標題採「中文可見節點型別繼承既有節點基底」 | 不破壞現有匯出/匯入型別判斷，且新建/匯入節點可直接顯示繁中名稱 |

## Issues Encountered
| Issue | Resolution |
|-------|------------|
| GraphToolkit 沒有公開「節點顯示名 attribute」給使用者程式集直接套用 | 改為在節點生命週期以集中名稱設定節點標題，並保持 type 不變 |
| MCP 測試介面當前無 Unity session | 改用 Unity CLI 嘗試執行 EditMode 測試 |
| Unity CLI 測試被「專案已被另一 Unity instance 開啟」阻擋 | 停止重試，保留阻擋訊息並回報 |
| MCP `localhost:8080/mcp` 連線失敗（transport send error） | 先完成流程守門改檔，待 Unity/MCP 恢復後再跑驗證 |
| 使用反射/選項覆寫節點 Title 無效 | 改走「類別命名」路徑，並同步修正所有型別參照 |

## 2026-02-23 補充結果（Phase 7 實作中）
- `InkFlowBaseNode` 已移除 `ApplyNodeTitle` 反射覆寫邏輯。
- 既有英文字節點型別保留為基底（`InkFlowStartNode`...），不再掛 `UseWithGraph`。
- 新增中文可見節點型別（`開始`/`對話`/`註解`/`選項`/`條件`）並掛 `UseWithGraph`。
- 匯入器 `CreateNodeByType` 已改為建立中文可見節點型別，確保匯入圖也是繁中節點名。
- `InkFlowActionKind` 已加上 `InspectorName`（對話/動作/自訂）以修正下拉顯示語言。

## 2026-02-24 新需求完整盤點（結構化對話節點）

### 使用者需求（不可省略）
- 一般使用者不可手填 JSON；欄位要以下拉/勾選/數值輸入完成。
- 對話節點需支持：
  - 動作類型：出現/移動/跳躍/旋轉/搖晃/縮放/更換立繪/消失
  - 人物下拉（依「可用角色集合」與流程上下文決定）
  - 人物 ID 自動顯示（只讀）
  - 人物名稱預設自動帶入，但可手動覆寫顯示名
  - 文字內容（自然語言）
  - 動態動作參數（依動作類型變化，可增減）
  - 是否換行（勾選）
  - 動作觸發時機（文本開始/文本結束/自訂）
- 多分支節點不是只有 `choice`；`condition`、小遊戲結果等都要支持多輸出埠。
- 流程權威是圖；輸出檔要同時可被玩家模式使用、且可逆還原圖結構。

### 現況缺口（程式層）
- `InkFlowActionNode` 目前只有 `ActionKind` + `Content` 兩個欄位，尚無動作列模型。
- sidecar 目前只新增了 `actionKind`，不足以保存動作參數/觸發時機/角色映射。
- 尚未有「角色資料來源」契約，無法穩定提供人物下拉名單。
- 動態欄位與參數面板尚未建置，仍需手動在 `Content` 寫自由文字。

### 現況缺口（契約層）
- `DeveloperModeOutputContract.md` 已有 Ink 基準與 Graph v2 子集合，但尚未定義：
  - 結構化對話節點欄位契約（動作列資料結構）
  - 動作參數映射規則（UI 欄位 -> Ink tag）
  - 角色來源與 ID/名稱同步規則
  - 舊版 `actionKind+content` 到新版結構化資料的相容策略

### 技術風險與可行策略
- 風險 A：若直接把所有內容壓回單一字串，會再次依賴手填語法，違反需求。
  - 策略：新增結構化 sidecar 欄位，匯出器由資料模型產生文字。
- 風險 B：節點欄位變多後，畫面可讀性下降。
  - 策略：以「動作列」+「依類型顯示參數」控制密度，先確保自動高度可用。
- 風險 C：測試若誤跑整包 EditMode，會再次暴走。
  - 策略：維持 `GraphToolkitFlowSafe` + fixture 固定篩選 + `Timeout(60000)`。

### 建議資料模型（草案，待契約定稿）
- `DialogueEntry`
  - `speakerActorId`（string，可空）
  - `speakerDisplayName`（string，可覆寫）
  - `text`（string）
  - `isLineBreak`（bool）
  - `actions[]`
- `DialogueAction`
  - `actionType`（enum）
  - `targetActorId`（string）
  - `trigger`（Start/End/Custom + `customTimeSec`）
  - `params[]`（key/type/value）

### 驗證目標（下一階段）
- 匯入後再匯出，`actions` 順序與參數不可改變。
- 相同 sidecar 在不同機器開啟，節點 UI 顯示結果一致。
- 輸出 `.ink` 可編譯成 `story.json`，且玩家模式流程符合圖上接線。

## 2026-02-25 實作結果（流程線/資料線分離）

### 已落地內容
- 節點拆分：
  - `action` 保留為對話節點（主流程）
  - 新增 `stageAction`（資料節點）
- 對話節點新增 `ActionInputCount` option，動態建立 `ActionIn*` typed input。
- 動作節點新增 `ActionData` typed output。
- sidecar `outputs[*]` 新增 `toPortName`，可逆保存「接到哪個輸入埠」。
- 匯入連線邏輯改為：
  - 優先使用 `toPortName`
  - 缺值時 fallback 到 `Flow`（相容舊 sidecar）
- 匯出驗證新增 stageAction 規則：
  - 必須且只能 1 條資料線
  - 目標必須是對話節點 `ActionIn*`
- 匯出 Ink 規則調整：
  - stageAction 不單獨輸出 knot
  - 內容按 `ActionIn` 序插入對話 knot（同序號再以節點 id 排序）

### 實作取捨
- 沒有直接硬控 wire 顏色 API（GraphToolkitSpec 已註記 public API 不提供這類控制）。
- 改用 typed data port 與 flow port 分離，讓編輯器以「線型別」區分流程線/資料線。

### 新增測試覆蓋
- Import 新增：`Dialogue + 2 個 stageAction` 可還原資料線與 `ActionInputCount=2`。
- RoundTrip 新增：
  - `toPortName` 不丟失（`ActionIn0/ActionIn1`）
  - 匯出 `.ink` 包含 stageAction 內容
  - 匯出 `.ink` 不產生 stageAction 專屬 knot

## 2026-02-25 實作結果（節點文字欄位寬度）

### 使用者最新約束（本輪）
- 只做兩件事：
  1. 先把節點文字欄位拉長
  2. 先更新契約
- 本輪不提前做「選項化重構」。

### 調查結論
- 現況 `action/stageAction/comment/choice/condition` 的主要內容欄位仍是 `string` option。
- GraphToolkit 預設 `Field.uss` 對 label 有較大最小寬度（`min-width: 110px`），會壓縮輸入區。
- 目前專案沒有自訂 GraphToolkit 視窗樣式注入點，需要補 bootstrap 來套用專案 USS。

### 技術決策
- 在契約新增「6.2.1-2 節點文字欄位可讀寬度」條文，明確限制這次改動只屬於顯示層。
- 新增 `InkFlowChartNodeFields.uss` 做欄位寬度覆寫。
- 新增 `InkFlowChartGraphStyleBootstrap.cs`，只在 `.inkfc` 視窗套用樣式，避免影響非目標 Graph 視窗。

## 2026-02-28 新需求盤點（角色節點 + 登場角色集束）

### 需求重點（這輪）
- 新增 `character` 節點：角色 ID + 角色名稱 + 可增減「表情名稱 + 圖片（Texture2D）」欄位。
- 新增 `castBundle` 節點：收束本段可用角色。
- 只有有連到 `castBundle` 的角色，才可在 `action/stageAction` 角色下拉被選到。
- 接線或角色資料變更時，需要刷新角色下拉候選。

### 技術調查結論
- GraphToolkit `Node Option` 原生只有 `AddOption<T>`；若要 `string` 顯示成下拉，需附加 `EnumAttribute`。
- `EnumAttribute` 在套件內是 internal；可透過反射掛到 option builder 的 attribute list。
- `Texture2D` 可作為 option 型別，會呈現可拖拽 `ObjectField`，符合「從 project 拖圖」需求。
- `character/castBundle` 若未同步到 exporter/importer，會在 `GetNodeType/CreateNodeByType` 直接報「不支援節點型別」。

### 這輪契約決策
- `character` / `castBundle` 定位為 sidecar 權威資料節點，不單獨輸出 Ink knot。
- sidecar 需新增：
  - `action/stageAction`：`actorId`、`actorName`
  - `character`：`characterId`、`characterName`、`expressions[*].name`、`expressions[*].textureAssetPath`
  - `castBundle`：`castInputCount`
- 資料線新增兩種：
  - `CharacterData`：`character -> castBundle(CharacterIn*)`
  - `CastData`：`castBundle -> action/stageAction(CastIn)`

## Verification Outcome
- Import fixture：8/8 Passed
- RoundTrip fixture：2/2 Passed
- Unity Console `error`：0 筆
- Import fixture（Phase 15）：9/9 Passed（含 stageAction 資料線案例）
- RoundTrip fixture（Phase 15）：3/3 Passed（含 stageAction round-trip 案例）
- Unity Console `error`（Phase 15）：0 筆

## Resources
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodes.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExportModels.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartExporter.cs`
- `Assets/Editor/FlowChart/GraphToolkit/InkFlowChartImporter.cs`
- `Assets/Editor/Tests/InkFlowChartImportTests.cs`
- `Assets/Editor/Tests/InkFlowChartRoundTripTests.cs`
- `Documentation/DeveloperModeOutputContract.md`
- `Assets/Editor/OpsidanosInkTestRunnerMenu.cs`
- `Packages/com.unity.ai.navigation/Tests/Editor/NavMeshModifierVolumeInPrefabTests.cs`
- `Packages/com.unity.ai.navigation/Tests/Editor/Converter/OffMeshLinkConverterTests.cs`

## 2026-03-03 規則更新記錄（執行流程）

### 使用者新增硬規則（本次確認）
- 可並行工作必須使用 multi-agent，不可用單線程連續查找替代。
- multi-agent 的測試任務最長 60 秒。
- 調查任務與修改任務不受 60 秒上限限制。
- 每次執行維持「先提案、等同意、再執行」。

### 對任務流程的直接影響
- 測試規劃會拆成「可在 60 秒內完成」的小批次，逐批回報。
- 若是調查/修改階段，可使用較長流程，但仍需每階段回報與記錄。
- 並行階段與串行階段會明確切開，避免同檔案寫入衝突。
