// ===== 變更開始 =====
// 2026/02/06 Opsidanos (修改原因：修正 smoke test 固定路徑重建造成的 warning，改為每次建立唯一 .inkfc 路徑)
// 預期結果：測試可建立/載入圖資產且不再出現同一路徑重建 warning
using NUnit.Framework;
using OpsidanosInk.Editor;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace OpsidanosInk.Tests.EditMode
{
    public sealed class InkFlowChartGraphSmokeTests
    {
        private const string TempFolderPath = "Assets/TmpGraphToolkitTests";

        [Test]
        public void InkFlowChartGraph_可建立並載入()
        {
            string tempGraphPath = string.Empty;
            try
            {
                EnsureTempFolder();
                tempGraphPath = BuildUniqueTempGraphPath();
                // ===== 變更開始 =====
                // 2026/02/06 Opsidanos (修改原因：`GraphDatabase.CreateGraph` 會輸出固定 warning，改為在建圖片段暫時關閉 logger 避免噪音)
                // 預期結果：測試仍可建立/載入 Graph，且 Console 不再出現該 warning
                bool originalLogEnabled = Debug.unityLogger.logEnabled;
                Debug.unityLogger.logEnabled = false;
                InkFlowChartGraph createdGraph;
                try
                {
                    createdGraph = GraphDatabase.CreateGraph<InkFlowChartGraph>(tempGraphPath);
                }
                finally
                {
                    Debug.unityLogger.logEnabled = originalLogEnabled;
                }
                // ===== 變更結束 =====
                Assert.NotNull(createdGraph, "建立 .inkfc 圖資產後，回傳圖物件不可為 null。");

                InkFlowChartGraph loadedGraph = GraphDatabase.LoadGraph<InkFlowChartGraph>(tempGraphPath);
                Assert.NotNull(loadedGraph, "建立後重新載入 .inkfc 圖資產不可為 null。");
            }
            finally
            {
                DeleteTempGraphIfExists(tempGraphPath);
                DeleteTempFolderIfExists();
                AssetDatabase.Refresh();
            }
        }

        private static void EnsureTempFolder()
        {
            if (!AssetDatabase.IsValidFolder(TempFolderPath))
            {
                AssetDatabase.CreateFolder("Assets", "TmpGraphToolkitTests");
            }
        }

        private static string BuildUniqueTempGraphPath()
        {
            // ===== 變更開始 =====
            // 2026/02/06 Opsidanos (修改原因：`GenerateUniqueAssetPath` 在固定檔名下仍可能回傳同一路徑，改成 GUID 檔名避免重複)
            // 預期結果：每次 smoke test 都使用不同資產路徑，不再觸發同路徑重建 warning
            string uniqueFileName = $"Smoke_{System.Guid.NewGuid():N}.inkfc";
            string basePath = $"{TempFolderPath}/{uniqueFileName}";
            // ===== 變更結束 =====
            return AssetDatabase.GenerateUniqueAssetPath(basePath);
        }

        private static void DeleteTempGraphIfExists(string graphPath)
        {
            if (string.IsNullOrEmpty(graphPath))
            {
                return;
            }

            Object existing = AssetDatabase.LoadAssetAtPath<Object>(graphPath);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(graphPath);
            }
        }

        private static void DeleteTempFolderIfExists()
        {
            if (AssetDatabase.IsValidFolder(TempFolderPath))
            {
                AssetDatabase.DeleteAsset(TempFolderPath);
            }
        }
    }
}
// ===== 變更結束 =====
