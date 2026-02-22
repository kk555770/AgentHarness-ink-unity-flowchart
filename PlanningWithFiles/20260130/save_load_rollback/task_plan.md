# 任務計畫：Phase 5 存檔 / 讀檔 / 倒帶（Rollback）

日期：2026/01/30  
任務資料夾：`PlanningWithFiles/20260130/save_load_rollback/`

## 目標（8歲也看得懂）
- 我按一下「存檔」，遊戲會記住我現在走到哪一句
- 我按一下「讀檔」，遊戲會回到那一句，而且背景/立繪/CG/BGM 也會回到對的狀態
- 我按一下「倒帶」，就回到上一句（最多回 N 次）

## 現況（已知）
- 專案已能播放 Ink `.json`（`InkStoryEngine` + `VNPlayerPresenter`）
- 演出已能跑（bg/bgm/se/cg/char/shake）且已有 `InkResourceMap` + Addressables（Player build）
- 目前程式碼尚未有存檔/讀檔/倒帶相關實作（先前搜尋確認）

## 主要做法（我預計怎麼做）
1. `InkStoryEngine` 新增 API：拿到/載入 Ink state（`ToJson` / `LoadJson`）
2. 做一個 `InkSaveSystem`（MonoBehaviour）：
   - 每次產生 `StoryOutput` 後，記錄一份「快照」
   - 快照內容：Ink state + 畫面狀態（bg/cg/角色/bgm）+ 當句文字/選項（用來還原 UI）
3. UI 先做最小按鈕：`存檔` / `讀檔` / `倒帶`
4. 加 EditMode Tests：至少驗證「存檔資料 JSON」能正確序列化/反序列化

## 不做的事（先不碰）
- 多槽位 UI（先 1 槽）
- Editor Flow Chart（Phase 6）

## Phase（調查/實作一步、記錄一步）
| Phase | 內容 | 狀態 |
| --- | --- | --- |
| 1 | 調查 Ink runtime 存檔 API（ToJson/LoadJson） | 完成 |
| 2 | 修改 `InkStoryEngine`：提供 state 存取 API | 完成 |
| 3 | 建立 `InkSaveData`（序列化格式） | 完成 |
| 4 | 建立 `InkSaveSystem`（存/讀/倒帶） | 完成 |
| 5 | 加 UI 按鈕並接線（UI Toolkit） | 完成 |
| 6 | 加測試（EditMode） | 完成 |
| 7 | 整理文件與驗證步驟 | 完成 |

## 決策紀錄
| 決策 | 原因 |
| --- | --- |
| 先做 1 槽 + Rollback | 最快驗證存讀檔是否可用 |
|  |  |

## 錯誤紀錄
| 時間 | 內容 | 解法 |
| --- | --- | --- |
| 2026/01/30 | Unity MCP 顯示 `instances=0`（沒有 Unity Editor 連線），無法自動修改場景 | 先啟動 Unity 專案，再用 MCP 進行接線與存檔 |

## 目前狀態
- **狀態：complete（最小版存讀檔/倒帶任務，已結案）**

## 狀態同步（2026-02-06）
- [x] 本任務的最小目標已完成（1 槽 + rollback buffer + UI 按鈕 + 基礎測試）
- [x] 後續已完成延伸：讀檔後可倒帶回存檔前、快速連按 rollback 排隊、高壓測試通過
- [x] 尚未完成的延伸需求回主計畫追蹤：多槽 + Auto 槽 UI
