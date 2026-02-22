# 任務計畫：資源映射 ResourceMap（JSON）管線化

## 目標
- 把所有演出用資源（bg/bgm/se/cg/char/actor+expr）集中到一份 `resource_map.json`。
- 讓各個 Tag Player 都讀同一份 `InkResourceMap`，不要每個元件各自維護 bindings。
- 之後 Flow Chart 單元元件只要匯出同格式 JSON，就能用管線化方式串接到 Runtime。

## 目前階段
Phase 6

## Phases

### Phase 1：需求與規格整理
- [x] 確認資源分類（bg/bgm/se/cg/character/actors）
- [x] 定義 JSON 格式（避免 Dictionary，改用陣列方便 `JsonUtility`）
- **狀態：complete**

### Phase 2：建立 Demo JSON 與 Runtime 讀取器
- [x] 新增 `Assets/OpsidanosInk/Demo/resource_map.json`（用專案內現有資源填入）
- [x] 新增 `InkResourceMap`（讀 TextAsset JSON，提供 TryGet* 查詢）
- **狀態：complete**

### Phase 3：Tag Player 串接 ResourceMap（相容舊 bindings）
- [x] `InkTagBackgroundPlayer` / `InkTagCgPlayer` / `InkTagCharacterPlayer` / `InkTagCharacterStatePlayer` / `InkTagAudioPlayer` 增加 `resourceMap` 欄位
- [x] 有指定 `resourceMap` 時優先讀 ResourceMap；沒指定時沿用舊 bindings（方便漸進移轉）
- **狀態：complete**

### Phase 4：補齊 Unity 資產連結（meta/scene）
- [x] 新增 `Assets/OpsidanosInk/Demo/resource_map.json.meta`（固定 guid）
- [x] 新增 `Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/InkResourceMap.cs.meta`（固定 guid）
- [x] 更新 `Assets/Scene/Test.unity`：
  - `VNPlayer` 加上 `InkResourceMap` 並指定 `resourceMapJson`
  - 各 Tag Player 的 `resourceMap` 指向同一個 `InkResourceMap`
- **狀態：complete**

### Phase 5：測試與文件
- [x] 新增 EditMode Test：ResourceMap JSON 可解析、且能載入對應資源（Editor）
- [x] 更新 `Packages/com.opsidanos.ink/README.md`：新增 ResourceMap 用法與 JSON 範例
- **狀態：complete**

### Phase 6：Unity 驗證與交付
- [x] 你在 Play Mode 驗證：背景/立繪/CG/BGM/SE 全部改由 ResourceMap 驅動後仍正常
- [x] 我檢查 Console（Error=0；Warning 只有 MCP TestJob 清理訊息）
- [x] 更新本任務 `progress.md` 與狀態
- **狀態：complete**

## Key Questions（待確認）
1. Player build 是否要支援 Addressables/Resources？（已決定：Addressables；`resource_map.json` 增加 `address` 欄位，2026-01-27）
2. Flow Chart 匯出 JSON 時，是否要固定 version、或允許擴充欄位？

## Decisions Made
| 決策 | 原因 |
|------|------|
| JSON 用陣列而不是 Dictionary | `JsonUtility` 對 Dictionary 不友善，陣列最穩定 |
| 先支援 Editor 用 assetPath 載入 | 先把流程跑通；Build 再決定 Addressables/Resources |
| Player build 使用 Addressables（address）載入 | `assetPath` 在 Player 無法用；Addressables 可管線化且好擴充 |

## Errors Encountered
| Error | Attempt | Resolution |
|-------|---------|------------|
| Unity MCP 無 instance（no_unity_session） | 1 | 先改用檔案層級接線，Play Mode/Console 驗證再請你開 Unity session |
