using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpsidanosInk.Editor
{
    [InitializeOnLoad]
    public static class InkFlowChartGraphStyleBootstrap
    {
        // ===== 變更開始 =====
        // 2026/02/25 Opsidanos (修改原因：GraphToolkit 節點文字欄位過短，需在不改資料語意下只調整編輯器顯示寬度)
        // 預期結果：開啟 .inkfc 圖時自動套用欄位寬度樣式，對話/動作/選項/條件文字欄位可讀性提升
        private const string StyleSheetAssetPath = "Assets/Editor/FlowChart/GraphToolkit/InkFlowChartNodeFields.uss";
        private const string GraphWindowTypeFullName = "Unity.GraphToolkit.Editor.GraphViewEditorWindowImp";
        private const string GraphWindowBaseTypeFullName = "Unity.GraphToolkit.Editor.GraphViewEditorWindow";
        private const string RootScopeClassName = "ink-flowchart-wide-fields";
        private static readonly string GraphAssetExtension = $".{InkFlowChartGraph.AssetExtension}";
        private static double s_LastApplyTime;
        private static bool s_HasLoggedMissingStyleSheet;
        private static StyleSheet s_StyleSheet;

        static InkFlowChartGraphStyleBootstrap()
        {
            EditorApplication.update += ApplyStyleToInkFlowChartWindows;
        }

        private static void ApplyStyleToInkFlowChartWindows()
        {
            if (EditorApplication.timeSinceStartup - s_LastApplyTime < 0.5d)
            {
                return;
            }

            s_LastApplyTime = EditorApplication.timeSinceStartup;

            if (!TryLoadStyleSheet(out StyleSheet styleSheet))
            {
                return;
            }

            EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            for (int index = 0; index < windows.Length; index++)
            {
                EditorWindow window = windows[index];
                if (window == null || !IsGraphToolkitWindow(window))
                {
                    continue;
                }

                if (!IsInkFlowChartWindow(window))
                {
                    continue;
                }

                VisualElement root = window.rootVisualElement;
                if (root == null)
                {
                    continue;
                }

                if (!root.ClassListContains(RootScopeClassName))
                {
                    root.AddToClassList(RootScopeClassName);
                }

                if (!root.styleSheets.Contains(styleSheet))
                {
                    root.styleSheets.Add(styleSheet);
                }
            }
        }

        private static bool TryLoadStyleSheet(out StyleSheet styleSheet)
        {
            if (s_StyleSheet == null)
            {
                s_StyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetAssetPath);
            }

            styleSheet = s_StyleSheet;
            if (styleSheet != null)
            {
                return true;
            }

            if (!s_HasLoggedMissingStyleSheet)
            {
                s_HasLoggedMissingStyleSheet = true;
                Debug.LogError($"[InkFlowChart] 找不到樣式檔：{StyleSheetAssetPath}");
            }

            return false;
        }

        private static bool IsGraphToolkitWindow(EditorWindow window)
        {
            Type windowType = window.GetType();
            if (windowType.FullName == GraphWindowTypeFullName)
            {
                return true;
            }

            Type current = windowType.BaseType;
            while (current != null)
            {
                if (current.FullName == GraphWindowBaseTypeFullName)
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }

        private static bool IsInkFlowChartWindow(EditorWindow window)
        {
            object graphTool = GetPropertyValue(window, "GraphTool");
            object toolState = GetPropertyValue(graphTool, "ToolState");
            object graphModel = GetPropertyValue(toolState, "GraphModel");
            object graphObject = GetPropertyValue(graphModel, "GraphObject");
            object filePathObject = GetPropertyValue(graphObject, "FilePath");
            string filePath = filePathObject as string;

            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            return filePath.EndsWith(GraphAssetExtension, StringComparison.OrdinalIgnoreCase);
        }

        private static object GetPropertyValue(object target, string propertyName)
        {
            if (target == null || string.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return property?.GetValue(target);
        }
        // ===== 變更結束 =====
    }
}
