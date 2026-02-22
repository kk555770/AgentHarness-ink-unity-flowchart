# 發現與紀錄：資源映射 ResourceMap（JSON）管線化

## 使用者需求（重要）
- 資源映射未來要給 Flow Chart 單元元件輸出，所以需要「資料格式明確、可擴張、可管線化」。
- 不要用 ScriptableObject 做映射；改成可由工具輸出的資料（本次先用 JSON）。
- 既有 Tag（bg/bgm/se/cg/char/char JSON）要能接到同一份資源映射，不要每個元件各自填 Inspector。

## JSON 格式（Demo）
- 檔案：`Assets/OpsidanosInk/Demo/resource_map.json`
- 每筆資源欄位：
  - `assetPath`：Editor（Play Mode）用
  - `address`：Player build 用（Addressables）
- 分類：
  - `bg`：背景 Texture2D
  - `bgm`：音樂 AudioClip
  - `se`：音效 AudioClip
  - `cg`：CG Texture2D
  - `character`：舊 `char-left/center/right:<id>` 對照用（Texture2D）
  - `actors`：新 `char` JSON（actor/expr）對照用（Texture2D）

## Runtime 讀取策略
- `InkResourceMap` 讀 `TextAsset` 的 JSON（`JsonUtility`）。
- Editor 內（含 Play Mode）用 `AssetDatabase.LoadAssetAtPath` 依 `assetPath` 載入資源。
- Player build 用 Addressables 依 `address` 載入資源；若 `address` 為空會 `Debug.LogError` 指出需要補欄位與設定 Addressable。

## Unity 連結注意事項
- 新增的 `.json` 與 `.cs` 都需要 `.meta` 固定 guid，才能安全被 scene 引用。
