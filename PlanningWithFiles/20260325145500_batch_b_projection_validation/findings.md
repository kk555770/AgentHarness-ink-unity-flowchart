# Findings：batch_b_projection_validation

## 初始結論

- `ValidateProjection(Ink)` 原本只是在組出 Ink 字串，沒有真正編譯驗證。
- `ProjectGraph(Ink)` 原本也只是輸出字串，沒有 compile-level regression harness。
- 不能把 `Ink.Compiler` 直接拉進 canonical core，否則會破壞 core/editor 分層。

## 採用方案

- core 只提供 `ICurrentFlowInkCompileProbe` 與 registry。
- editor-only bootstrap 負責把真實 `Ink.Compiler` 包成 probe。
- service 層在 `InkTarget` 路徑先 build，再透過 probe 編譯。
- tests 用 recording/failing probe 同時覆蓋成功與失敗語意。

## 最後確認

- regression harness 已經能直接鎖住 `ProjectGraph(Ink)` 的 compile-level 行為。
- `ValidateProjection(Ink)` 失敗時會回報 `PROJECTION_COMPILE_FAILED` 或 `PROJECTION_VALIDATION_UNAVAILABLE`。
- `ProjectGraph(Ink)` 失敗時不會吐出假成功的 projection text。
