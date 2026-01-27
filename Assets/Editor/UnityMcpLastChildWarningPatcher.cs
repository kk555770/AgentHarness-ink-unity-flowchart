using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace OpsidanosInk.Editor
{
    public static class UnityMcpLastChildWarningPatcher
    {
        private const string MenuPath = "Window/MCP For Unity/修補 last-child 警告（Common.uss）";
        private const string PackageName = "com.coplaydev.unity-mcp";
        private const string CommonUssAssetPath = "Packages/com.coplaydev.unity-mcp/Editor/Windows/Components/Common.uss";
        private const string CommonUssRelativePathInCache = "Editor/Windows/Components/Common.uss";

        [MenuItem(MenuPath, priority = 50)]
        private static void Patch()
        {
            // ===== 變更開始 =====
            // 2026/01/27 Opsidanos (修改原因：提供手動修補工具，讓 Unity MCP 使用 git 來源也能移除 :last-child 警告)
            // 預期結果：點選選單後，會修改 Library/PackageCache 內的 Common.uss，Console 不再出現 last-child 警告
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string packageCacheRoot = Path.Combine(projectRoot, "Library", "PackageCache");

            if (!Directory.Exists(packageCacheRoot))
            {
                Debug.LogError($"[UnityMCP][USS] 找不到 Library/PackageCache：{packageCacheRoot}");
                return;
            }

            string[] candidatePackageFolders = Directory.GetDirectories(packageCacheRoot, $"{PackageName}@*");
            if (candidatePackageFolders.Length == 0)
            {
                Debug.LogError($"[UnityMCP][USS] 找不到 {PackageName} 的 PackageCache 目錄，請先確認已安裝套件。");
                return;
            }

            string commonUssPath = candidatePackageFolders
                .OrderByDescending(Directory.GetLastWriteTimeUtc)
                .Select(folder => Path.Combine(folder, CommonUssRelativePathInCache))
                .FirstOrDefault(File.Exists);

            if (string.IsNullOrEmpty(commonUssPath))
            {
                Debug.LogError($"[UnityMCP][USS] 找不到 Common.uss：{CommonUssRelativePathInCache}");
                return;
            }

            string original = File.ReadAllText(commonUssPath, Encoding.UTF8);
            if (original.IndexOf(":last-child", StringComparison.Ordinal) < 0)
            {
                Debug.Log($"<color=#2EA3FF>[UnityMCP][USS]</color> 已經沒有 :last-child，不需要修補：{commonUssPath}");
                return;
            }

            string patched = PatchLastChildBlock(original);
            if (string.Equals(patched, original, StringComparison.Ordinal))
            {
                Debug.LogError($"[UnityMCP][USS] 偵測到 :last-child，但找不到可移除的區塊，請手動檢查：{commonUssPath}");
                return;
            }

            File.WriteAllText(commonUssPath, patched, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            AssetDatabase.ImportAsset(CommonUssAssetPath, ImportAssetOptions.ForceUpdate);

            Debug.Log($"<color=#2EA3FF>[UnityMCP][USS]</color> 已修補 Common.uss（移除 :last-child）：{commonUssPath}");
            // ===== 變更結束 =====
        }

        private static string PatchLastChildBlock(string content)
        {
            const string pattern = @"(?s)\s*\.section-stack\s*>\s*\.section:last-child\s*\{.*?\}\s*";
            return Regex.Replace(content, pattern, "\n", RegexOptions.CultureInvariant);
        }
    }
}
