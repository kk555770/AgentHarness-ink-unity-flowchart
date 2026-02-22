# 進度紀錄：資源映射 ResourceMap（JSON）管線化

## Session：2026-01-25

### 已完成
- 新增 `Assets/OpsidanosInk/Demo/resource_map.json`（用現有資源填入）
- 新增 `Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/InkResourceMap.cs`
- 更新各 Tag Player 支援 `resourceMap`（有指定時優先使用）
- 新增 `.meta`：
  - `Assets/OpsidanosInk/Demo/resource_map.json.meta`
  - `Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/InkResourceMap.cs.meta`
- 更新 `Assets/Scene/Test.unity`：
  - `VNPlayer` 加上 `InkResourceMap`，並指定 `resourceMapJson`
  - 各 Tag Player 的 `resourceMap` 指向同一個 `InkResourceMap`
- 新增 EditMode Test：`Assets/Editor/Tests/InkResourceMapTests.cs`
- 更新 `Packages/com.opsidanos.ink/README.md`：新增「資源映射（推薦：ResourceMap JSON）」段落

### （歷史）進行中
- 等你在 Play Mode 目視驗證：背景/立繪/CG/BGM/SE 都改由 ResourceMap 驅動後仍正常
- 等 Unity MCP session 可用後，我再讀 Console 做最後確認

### 已確認（你已操作 Play Mode，我已讀 Console）
- Unity Console（Error）：0 條
- Unity Console（Warning）：1 條（MCP 自己清理過期 TestJob，與 ResourceMap 無關）

### （歷史）待做
- （已完成於 2026-01-27）Player build 改用 Addressables 載入（見下方 Session）

## Session：2026-01-27

### 已完成
- `InkResourceMap` 支援 Player build：改用 Addressables 依 `address` 載入資源（Editor 仍用 `assetPath`）
- Demo：`Assets/OpsidanosInk/Demo/resource_map.json` 補齊 `address` 欄位，並更新 `version=2`
- 更新 Runtime asmdef：`Packages/com.opsidanos.ink/Runtime/OpsidanosInk.Runtime.asmdef` 加入 `Unity.Addressables` / `Unity.ResourceManager`
- 更新套件依賴：`Packages/com.opsidanos.ink/package.json` 加入 `com.unity.addressables`
- 更新文件：`Packages/com.opsidanos.ink/README.md` 補上 Player build 的設定步驟
- 新增 EditMode Test：`Assets/Editor/Tests/InkResourceMapAddressablesTests.cs`（檢查 Demo JSON 的 address 完整性）
- 新增 Editor 工具：一鍵把 Demo 內用到的資源設成 Addressable（`Packages/com.opsidanos.ink/Editor/InkResourceMapAddressablesSetup.cs`）

### （歷史）待你驗證
- 你做一次 Player build，進 `Assets/Scene/Test.unity`，確認 BG/立繪/CG/BGM/SE 都正常，且 Console 沒有 `[OpsidanosInk] InkResourceMap ...` 的錯誤

## 狀態同步（2026-02-06）
- 本任務已完成並結案；上方「進行中/待做/待你驗證」為 2026-01-25~2026-01-27 的當時記錄。
- 後續主線改由總計畫追蹤：`20260120/TextAdventureEngine`（目前待辦：Phase 5 多槽 + Auto 槽 UI、Phase 6 Flow Chart）。
