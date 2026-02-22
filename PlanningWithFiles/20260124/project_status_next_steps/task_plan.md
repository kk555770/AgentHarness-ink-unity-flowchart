# 任務計畫：重新理解專案現況與下一步（ink-unity-integration）

## 目標
- 在**不修改任何代碼**的前提下，整理：
  - 這個專案「想解決什麼問題」（目的）
  - 目前「做到哪裡了」（進度）
  - 目前「卡在哪裡／有什麼風險」（問題）
  - 下一步「我可以直接做的事情」（提案清單：範圍、現況、預期效果）
- 產出提案後，請專案負責人確認是否同意，再開始動手改代碼。

## 目前階段
- Phase 10（完成：資源映射 ResourceMap）

## 階段拆解（每完成一段就更新狀態）

### Phase 1：需求與限制盤點
- [x] 重新閱讀 `AGENTS.md` 並確認本次任務範圍（只做盤點與提案，不改代碼）
- [x] 確認 `PlanningWithFiles/` 既有紀錄日期與主題（避免漏掉未完成事項）
- [x] 將初步觀察寫入 `findings.md`
- **狀態：complete**

### Phase 2：讀既有任務紀錄（PlanningWithFiles）
- [x] 閱讀 `PlanningWithFiles/20260120/**` 與 `PlanningWithFiles/20260122/**`
- [x] 整理：當時目標、已完成、未完成、後續建議
- [x] 將整理結果寫入 `findings.md`
- **狀態：complete**

### Phase 3：讀專案說明與結構
- [x] 閱讀 `README.md`、`Documentation/`（若有）、`Packages/manifest.json`
- [x] 盤點：主要程式入口、Package 組成、UI（若有）
- [x] 將整理結果寫入 `findings.md`
- **狀態：complete**

### Phase 4：整理「專案目的／目前進度／下一步」
- [x] 用「8歲小孩看得懂」的方式，寫出：目的、目前進度、下一步
- [x] 列出我能做的下一步工作清單（每項含：範圍、現況、預期效果、需要你測試的點）
- **狀態：complete**

### Phase 5：提案（等你同意後才改代碼）
- [x] 把 Phase 4 的清單整理成正式提案（不參雜額外問句）
- [x] 透過 AskUserQuestionTool 請你選擇「同意／不同意／要調整」
- **狀態：complete**

### Phase 6：實作（提案 A：Tag 事件管線）
- [x] 新增 `InkTagEventRouter`（Tag → 事件路由＋除錯輸出）
- [x] 更新 `Packages/com.opsidanos.ink/README.md`（讓使用方式更清楚）
- [x] 用 Unity MCP 自動設定 `Assets/Scene/Test.unity`（加元件、接引用、指定 UXML 與 Story JSON）
- [x] 新增 Demo Ink：`Assets/OpsidanosInk/Demo/story.ink`（含 Tag 範例），並編譯產生 `Assets/OpsidanosInk/Demo/story.json`
- [x] 請你在 Unity Play Mode 驗證：點「下一句」與左右選項，Console 會依序印出 `speaker/bg/bgm/se/shake` 相關 Tag
- [x] 清理 Console 警告：移除 Unity MCP 的 `:last-child`（避免 `Unknown pseudo class "last-child" in StyleSheet Common`）
- **狀態：complete**

### Phase 7：下一步（已完成）
- [x] 改善 Tag log：不要在「故事結束」那一筆重複印上一句的 Tag（已實作，待你驗證）
- [x] 請你在 Unity Play Mode 驗證：跑到結尾時，Tag log 不再多印一次（不會再出現同樣 Tag 的最後一筆）
- **狀態：complete**

### Phase 8：Phase 4（演出）最小綁定（BGM/SE）
- [x] 新增 `InkTagAudioPlayer`（Tag → 播放音樂/音效）
- [x] 用 Unity MCP 更新 `Assets/Scene/Test.unity`：`VNPlayer` 加 `AudioSource` * 2 + `InkTagAudioPlayer` 並接好對照
- [x] 你在 Play Mode 驗證：走到 `bgm:opening` / `se:open` 時，Console 會印出 `[OpsidanosInk][BGM]` / `[OpsidanosInk][SE]`，並且聽得到聲音
- **狀態：complete**

### Phase 9：UI（1920x1080）
- [x] 調整 `VNPlayer.uss`：把文字與按鈕做成 1920x1080 更好讀的尺寸
- [x] 你在 Unity Play Mode 驗證：1920x1080 下，名字/內文/按鈕文字大小都已經舒服
- **狀態：complete**

### Phase 10：Phase 4（演出）視覺綁定（背景/立繪/CG/特效）
- [x] 背景：新增 `InkTagBackgroundPlayer`（訂閱 `bg:id` → 切換背景圖）
- [x] 修正背景綁定：目前 BG 素材是 `Texture2D`（不是 `Sprite`），所以需要先把背景綁定型別改對（否則會一直是 `null`）
- [x] Unity 接線：`Assets/Scene/Test.unity` 的 `VNPlayer` 設定 id→背景圖片 對照（例如 `room_01` / `outside`）
- [x] 你驗證：Play Mode 看到背景真的會換，Console 會印 `[OpsidanosInk][BG] ...`
- [x] 立繪：新增 `char-left/char-center/char-right:<id>` → 角色層（left/center/right）顯示/切換立繪（`InkTagCharacterPlayer`）
- [x] 你驗證：Play Mode 看到立繪會出現/切換（至少看到 `alice_normal` / `alice_happy` / `alice_shy`）
- [x] CG：新增 `cg:<id>` / `cg:clear` → 全畫面覆蓋（平常隱藏）（`InkTagCgPlayer`）
- [x] 你驗證：Play Mode 走到 `cg:alice_happy_cg` 時 CG 會出現，走到 `cg:clear` 時 CG 會消失
- [x] 修正圖片比例：把會變形的貼圖 `nPOTScale` 改成 `0`（None），避免 Unity 匯入時把圖片擠壓成 2 次方尺寸而改掉比例（仍可用 `scale/transform` 做動畫縮放）
- [x] 你驗證：Play Mode 看到 BG／立繪／CG 都沒有「變形」（保持原圖比例）
- [x] 特效：新增 `InkTagShakePlayer`，讓 `# shake` 真的會抖（預設只抖背景，不影響對話框/選項）
- [x] 資源對照（暫時）：先用「Scene 元件欄位」綁定（id → Texture2D/AudioClip），讓 Play Mode 先能驗證
- [x] 資源對照（之後-第 1 步）：新增 `resource_map.json` + `InkResourceMap`（集中式 JSON 映射，方便 Flow Chart 管線化匯出）
- [x] 資源對照（之後-第 2 步）：各 Tag Player 支援 `resourceMap`（有指定時優先使用，沒指定則相容舊 bindings）
- [x] 資源對照（之後-第 3 步）：Unity 接線：Test 場景改用 ResourceMap 驅動（並補齊 `.meta` 固定 guid）
- [x] 資源對照（之後-第 4 步）：新增可驗證的測試 + 更新 README（讓使用方式更清楚）
- **狀態：complete**

## 需要回答的重點問題（做盤點用）
1. 這個專案最核心要解決什麼問題？
2. 目前有哪些功能已經可以用？哪些還不能用？
3. 目前最可能卡住的點是什麼（技術／流程／資料）？
4. 下一步最值得先做的是什麼（影響最大、最省時間、最能驗證方向）？

## 已做的決策（之後若要實作再補）
| 決策 | 原因 |
|------|------|
|      |      |

## 錯誤紀錄
| 錯誤 | 嘗試次數 | 解法 |
|------|----------|------|
|      | 1        |      |

## 狀態同步（2026-02-06）
- [x] 本任務定位維持「歷史盤點 + 當時提案 + 落地紀錄」，全階段完成
- [x] 原文中「待你選擇」屬於當時流程語境，已完成同步標記
