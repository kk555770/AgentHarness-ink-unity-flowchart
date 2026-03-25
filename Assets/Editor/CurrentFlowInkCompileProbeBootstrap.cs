// ===== 變更開始 =====
// 2026/03/25 Opsidanos (修改原因：Editor bootstrap 改成只負責明確安裝 core 預設 probe，避免 Editor 與 core 各自維護一份編譯邏輯)
// 預期結果：Editor 仍會在載入時安裝 probe，但真正的編譯邏輯只保留在 canonical core 一份
using OpsidanosInk.CanonicalGraph;
using UnityEditor;

namespace OpsidanosInk.Editor
{
    [InitializeOnLoad]
    public static class CurrentFlowInkCompileProbeBootstrap
    {
        static CurrentFlowInkCompileProbeBootstrap()
        {
            CurrentFlowInkCompileProbeRegistry.Current = new CurrentFlowDefaultInkCompileProbe();
        }
    }
}
// ===== 變更結束 =====
