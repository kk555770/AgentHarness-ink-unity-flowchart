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

        [MenuItem("OpsidanosInk/測試/跑全部（Edit → Play，只跑我們的）")]
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

        [MenuItem("OpsidanosInk/測試/跑 EditMode（只跑我們的）")]
        private static void RunEditMode_OnlyOurs()
        {
            RunEditMode();
        }

        [MenuItem("OpsidanosInk/測試/跑 PlayMode（只跑我們的）")]
        private static void RunPlayMode_OnlyOurs()
        {
            RunPlayMode();
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
