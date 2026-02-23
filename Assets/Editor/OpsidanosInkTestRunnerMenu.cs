// ===== 變更開始 =====
// 2026/02/05 Opsidanos (修改原因：新增一鍵跑我們自己的測試，避免誤跑到 package 的大量測試卡住)
// 預期結果：在 Unity 上方選單點一下，就只跑指定的 EditMode/PlayMode 測試 Assembly
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace OpsidanosInk.Editor
{
    public static class OpsidanosInkTestRunnerMenu
    {
        private static readonly TestRunnerApi Api = new TestRunnerApi();
        private static RunAllCallback runAllCallback;
        // ===== 變更開始 =====
        // 2026/02/23 Opsidanos (修改原因：建立 GraphToolkit 專用安全測試流程，避免誤跑整包 EditMode 連動 package 場景測試)
        // 預期結果：固定只跑 Import/RoundTrip 兩個 fixture，且可透過 category 再次守門
        private const string GraphToolkitSafeCategory = "GraphToolkitFlowSafe";
        private static readonly string[] GraphToolkitSafeFixtureNames =
        {
            "OpsidanosInk.Tests.EditMode.InkFlowChartImportTests",
            "OpsidanosInk.Tests.EditMode.InkFlowChartRoundTripTests"
        };
        // ===== 變更結束 =====

        // ===== 變更開始 =====
        // 2026/02/07 Opsidanos (修改原因：將測試工具入口統一到 Tools/OpsidanosInk，避免分散在多個選單路徑)
        // 預期結果：測試入口全部集中在 Tools/OpsidanosInk/測試，功能流程不變
        [MenuItem("Tools/OpsidanosInk/測試/跑全部（Edit → Play，只跑我們的）")]
        // ===== 變更結束 =====
        private static void RunAll_EditThenPlay()
        {
            if (runAllCallback != null)
            {
                Debug.LogWarning("[OpsidanosInk] 測試正在跑，請等結束後再按一次。");
                return;
            }

            runAllCallback = new RunAllCallback(Api);
            Api.RegisterCallbacks(runAllCallback);
            runAllCallback.RunEditMode();
        }

        // ===== 變更開始 =====
        // 2026/02/07 Opsidanos (修改原因：將測試工具入口統一到 Tools/OpsidanosInk，避免分散在多個選單路徑)
        // 預期結果：測試入口全部集中在 Tools/OpsidanosInk/測試，功能流程不變
        [MenuItem("Tools/OpsidanosInk/測試/跑 EditMode（只跑我們的）")]
        // ===== 變更結束 =====
        private static void RunEditMode_OnlyOurs()
        {
            RunEditMode();
        }

        // ===== 變更開始 =====
        // 2026/02/07 Opsidanos (修改原因：將測試工具入口統一到 Tools/OpsidanosInk，避免分散在多個選單路徑)
        // 預期結果：測試入口全部集中在 Tools/OpsidanosInk/測試，功能流程不變
        [MenuItem("Tools/OpsidanosInk/測試/跑 PlayMode（只跑我們的）")]
        // ===== 變更結束 =====
        private static void RunPlayMode_OnlyOurs()
        {
            RunPlayMode();
        }

        // ===== 變更開始 =====
        // 2026/02/23 Opsidanos (修改原因：提供 GraphToolkit 最小測試入口，避免使用者手滑啟動整包測試導致 Unity/MCP 暴走)
        // 預期結果：從選單一鍵執行時，只會命中 GraphToolkit Import/RoundTrip，且具固定分類防呆
        [MenuItem("Tools/OpsidanosInk/測試/GraphToolkit/安全跑 Import+RoundTrip（60秒）")]
        // ===== 變更結束 =====
        private static void RunGraphToolkitSafe_EditMode()
        {
            // ===== 變更開始 =====
            // 2026/02/23 Opsidanos (修改原因：將測試範圍鎖死在 assembly + category + fixture，避免篩選條件遺漏)
            // 預期結果：不會誤跑到 package 測試，減少場景切換與編譯風暴風險
            var filter = new Filter
            {
                testMode = TestMode.EditMode,
                assemblyNames = new[] { "OpsidanosInk.EditModeTests" },
                categoryNames = new[] { GraphToolkitSafeCategory },
                testNames = GraphToolkitSafeFixtureNames
            };

            Debug.Log("[OpsidanosInk] GraphToolkit 安全測試啟動：Import + RoundTrip（60 秒逾時由測試層控制）");
            Api.Execute(new ExecutionSettings(filter));
            // ===== 變更結束 =====
        }

        private static void RunEditMode()
        {
            var filter = new Filter
            {
                testMode = TestMode.EditMode,
                assemblyNames = new[] { "OpsidanosInk.EditModeTests" }
            };

            Api.Execute(new ExecutionSettings(filter));
        }

        private static void RunPlayMode()
        {
            var filter = new Filter
            {
                testMode = TestMode.PlayMode,
                assemblyNames = new[] { "OpsidanosInk.PlayModeTests" }
            };

            Api.Execute(new ExecutionSettings(filter));
        }

        private sealed class RunAllCallback : ICallbacks
        {
            private readonly TestRunnerApi api;
            private bool isRunningEditMode = true;

            public RunAllCallback(TestRunnerApi api)
            {
                this.api = api;
            }

            public void RunEditMode()
            {
                isRunningEditMode = true;
                OpsidanosInkTestRunnerMenu.RunEditMode();
            }

            public void RunStarted(ITestAdaptor testsToRun)
            {
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                if (isRunningEditMode)
                {
                    isRunningEditMode = false;
                    OpsidanosInkTestRunnerMenu.RunPlayMode();
                    return;
                }

                api.UnregisterCallbacks(this);
                runAllCallback = null;
            }
        }
    }
}
#endif
// ===== 變更結束 =====
