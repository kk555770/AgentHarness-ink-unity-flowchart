// ===== 變更開始 =====
// 2026/03/25 Opsidanos (修改原因：讓 compile probe registry 內建可用預設值，避免 Ink 驗證只能靠 Editor bootstrap 才能成立)
// 預期結果：只要 core assembly 可用，registry 就能懶初始化預設 probe；測試仍可覆寫成自訂 probe
namespace OpsidanosInk.CanonicalGraph
{
    public static class CurrentFlowInkCompileProbeRegistry
    {
        private static readonly object SyncRoot = new object();
        private static ICurrentFlowInkCompileProbe s_Probe;

        public static bool HasProbe
        {
            get
            {
                lock (SyncRoot)
                {
                    return ResolveCurrentProbeLocked() != null;
                }
            }
        }

        public static ICurrentFlowInkCompileProbe Current
        {
            get
            {
                lock (SyncRoot)
                {
                    return ResolveCurrentProbeLocked();
                }
            }
            set
            {
                lock (SyncRoot)
                {
                    s_Probe = value;
                }
            }
        }

        public static bool TryCompile(string inkContent, out string errorMessage)
        {
            ICurrentFlowInkCompileProbe probe = Current;

            if (probe == null)
            {
                errorMessage = "Ink compile probe 尚未安裝。";
                return false;
            }

            if (!probe.TryCompile(inkContent, out errorMessage))
            {
                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = "Ink compile probe 回傳失敗。";
                }

                return false;
            }

            return true;
        }

        private static ICurrentFlowInkCompileProbe ResolveCurrentProbeLocked()
        {
            s_Probe ??= new CurrentFlowDefaultInkCompileProbe();
            return s_Probe;
        }
    }
}
// ===== 變更結束 =====
