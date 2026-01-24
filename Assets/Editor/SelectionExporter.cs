// Assets/Editor/SelectionExporter.cs
// Compact + Whitelisted Generic Formatters
// - Minified JSON (no pretty print) to reduce size
// - properties: NO empty objectReference objects
// - ObjectReference wiring exported only in component.objectRefs
// - No expansion of composite sub-fields (avoid rotation.x/y/z/w redundancy)
// - Skip Transform defaults (pos/rot/scale)
// - Improve readability for common UI composite structs by "single-line atomic formatting":
//   RectOffset, ColorBlock, Navigation, SpriteState, AnimationTriggers

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine.UI;
using UnityEngine.Events;

public static class SelectionExporter
{
    private const string ExporterVersion = "2025-12-16.compact-v2-genericWhitelist";

    // KEY: keep output minified to reduce size dramatically
    private const bool PRETTY_PRINT = false;

    private const int MAX_STRING_LEN = 512;
    private const int MAX_ARRAY_SCAN_FOR_OBJECTREFS = 256;
    private const int MAX_GENERIC_TYPE_NAME_LEN = 64;

    // Output directory: <ProjectRoot>/GoData/exports/YYYY-MM-DD/
    private static string ExportRoot => Path.GetFullPath(Path.Combine(
        Application.dataPath, "..", "GoData", "exports", DateTime.Now.ToString("yyyy-MM-dd")
    ));

    [MenuItem("Tools/AI/Export Selected GameObject To JSON")]
    public static void ExportSelectedGameObject()
    {
        var go = Selection.activeGameObject;
        if (go == null)
        {
            EditorUtility.DisplayDialog("Export Failed", "請在 Hierarchy（或 Prefab Stage）選擇一個 GameObject。", "OK");
            return;
        }

        ExportAndSave(go, explicitPrefabAssetPath: null);
    }

    [MenuItem("Tools/AI/Export Selected Prefab Asset To JSON")]
    public static void ExportSelectedPrefabAsset()
    {
        var obj = Selection.activeObject;
        if (obj == null)
        {
            EditorUtility.DisplayDialog("Export Failed", "請在 Project 視窗選擇一個 Prefab。", "OK");
            return;
        }

        var path = AssetDatabase.GetAssetPath(obj);
        if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
        {
            EditorUtility.DisplayDialog("Export Failed", "選擇的不是 Prefab Asset（.prefab）。", "OK");
            return;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Export Failed", "無法載入 Prefab。", "OK");
            return;
        }

        ExportAndSave(prefab, explicitPrefabAssetPath: path);
    }

    private static void ExportAndSave(GameObject root, string explicitPrefabAssetPath)
    {
        try { Canvas.ForceUpdateCanvases(); } catch { /* ignore */ }

        var snapshot = new Snapshot
        {
            exporterVersion = ExporterVersion,
            exportTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            unityVersion = Application.unityVersion,
            platform = Application.platform.ToString(),
            projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..")),

            source = BuildSourceInfo(root, explicitPrefabAssetPath),
            root = ExportGameObjectRecursive(root),

            prefabInstances = CollectPrefabInstanceData(root),
            warnings = new List<string>()
        };

        if (snapshot.source != null &&
            snapshot.source.stageKind == "MainStage" &&
            snapshot.source.selectionSceneIsSaved == false)
        {
            snapshot.warnings.Add("Selection is in an unsaved Scene (scene.path is empty). GlobalObjectId may be unavailable; hierarchyKey is used as fallback.");
        }

        var json = JsonUtility.ToJson(snapshot, PRETTY_PRINT);

        Directory.CreateDirectory(ExportRoot);
        var timestamp = DateTime.Now.ToString("HHmmssfff");
        var fileName = $"{SanitizeFileName(root.name)}_{timestamp}.json";
        var outputPath = Path.Combine(ExportRoot, fileName);

        File.WriteAllText(outputPath, json);

        Debug.Log($"[SelectionExporter] Exported to: {outputPath}");
        EditorUtility.RevealInFinder(outputPath);
    }

    #region Data Model

    [Serializable]
    private class Snapshot
    {
        public string exporterVersion;
        public string exportTime;
        public string unityVersion;
        public string platform;
        public string projectPath;

        public SourceInfo source;
        public GOData root;

        public List<PrefabInstanceData> prefabInstances = new List<PrefabInstanceData>();
        public List<string> warnings = new List<string>();
    }

    [Serializable]
    private class SourceInfo
    {
        public string stageKind; // "MainStage" / "PrefabStage"
        public string prefabStagePrefabAssetPath;
        public string prefabStagePrefabGuid;

        public string selectionKind; // "PrefabAsset" / "PrefabAssetInStage" / "PrefabInstanceInScene" / "SceneObject"
        public string selectionHierarchyPath;
        public string selectionHierarchyKey;

        public string selectionSceneName;
        public string selectionScenePath;
        public bool selectionSceneIsSaved;
        public string selectionSceneGuid;

        public string selectionPrefabAssetPath;
        public string selectionPrefabGuid;
        public string selectionPrefabInstanceStatus;

        public ObjectId selectionObjectId;
    }

    [Serializable]
    private class GOData
    {
        public string name;
        public string hierarchyPath;
        public string hierarchyKey;

        public bool activeSelf;

        public string tag;
        public int layer;
        public int siblingIndex;

        public ObjectId objectId;
        public PrefabLink prefab;

        public RectInfo rectInfo;
        public LayoutInfo layoutInfo;

        public List<ComponentData> components = new List<ComponentData>();
        public List<GOData> children = new List<GOData>();
    }

    [Serializable]
    private class ObjectId
    {
        public string stableId;
        public string kind; // "Asset" / "SceneObject" / "UnsavedSceneObject" / "Unknown"

        public string globalObjectId;

        public string assetGuid;
        public string fileId;
        public string assetPath;

        public string scenePath;
        public string sceneName;

        public string hierarchyPath;
        public string hierarchyKey;

        public int instanceId;

        public string assetBundleName;
        public string addressableAddress;
    }

    [Serializable]
    private class PrefabLink
    {
        public bool isPartOfPrefabInstance;
        public bool isPrefabInstanceRoot;
        public bool isAddedGameObjectOverride;

        public string nearestInstanceRootPath;
        public string nearestInstanceRootKey;

        public string prefabAssetPath;
        public string prefabAssetGuid;
        public string prefabInstanceStatus;

        public string assetBundleName;
        public string addressableAddress;
    }

    [Serializable]
    private class RectInfo
    {
        public bool isRectTransform;

        public Vec2 anchorMin;
        public Vec2 anchorMax;
        public Vec2 pivot;
        public Vec2 anchoredPosition;
        public Vec2 sizeDelta;

        public Vec3[] worldCorners;   // 4
        public Vec2[] screenCorners;  // 4
        public Rect2 screenRect;

        public string canvasPath;
        public string canvasRenderMode;
        public string canvasWorldCameraPath;
        public float canvasScaleFactor;
        public Vec2 canvasReferenceResolution;
    }

    [Serializable]
    private class LayoutInfo
    {
        public bool hasLayoutGroup;
        public string layoutGroupType;

        public float preferredWidth;
        public float preferredHeight;

        public List<ChildRectInfo> calculatedChildRects = new List<ChildRectInfo>();
    }

    [Serializable]
    private class ChildRectInfo
    {
        public string childHierarchyPath;
        public string childHierarchyKey;
        public ObjectId childId;

        public Rect2 screenRect;
        public Vec3[] worldCorners;
    }

    [Serializable]
    private class ComponentData
    {
        public string type;

        public bool hasEnabled;
        public bool enabled;

        public bool isMissingScript;
        public bool isAddedComponentOverride;

        // MonoBehaviour identity (flatten)
        public bool isMonoBehaviour;
        public string scriptAssetPath;
        public string scriptGuid;
        public string scriptFileId;
        public string scriptClassFullName;
        public string scriptAssemblyName;

        // Compact properties only (no objectReference nested objects)
        public List<Prop> properties = new List<Prop>();

        // Wiring exported here only
        public List<ObjectRefData> objectRefs = new List<ObjectRefData>();

        // UnityEvent persistent calls
        public List<EventBinding> eventBindings = new List<EventBinding>();

        // Sprite / Atlas info for UI.Image
        public bool hasSprite;
        public string spritePath;
        public string spriteGuid;
        public bool hasSpriteAtlas;
        public string spriteAtlasPath;
        public string spriteAtlasGuid;
    }

    // Compact property entry:
    // p: propertyPath
    // t: short type code
    // v: string value
    // n: arraySize (only meaningful when t == "a")
    [Serializable]
    private class Prop
    {
        public string p;
        public string t;
        public string v;
        public int n;
    }

    [Serializable]
    private class ObjectRefData
    {
        public string propertyPath;

        public string referencedType;
        public string referencedName;

        public string hierarchyPath;
        public string hierarchyKey;

        public ObjectId objectId;
    }

    [Serializable]
    private class EventBinding
    {
        public string eventFieldName;
        public string eventFieldType;

        public string[] eventParameterTypes;

        public int callIndex;

        public ObjectRefData target;
        public string methodName;
        public string resolvedMethodSignature;
        public string argumentType;
    }

    [Serializable]
    private class PrefabInstanceData
    {
        public string instanceRootPath;
        public string instanceRootKey;
        public ObjectId instanceRootId;

        public string prefabAssetPath;
        public string prefabAssetGuid;
        public string prefabInstanceStatus;

        public List<PropertyModData> propertyModifications = new List<PropertyModData>();

        public List<ObjectRefData> addedGameObjects = new List<ObjectRefData>();
        public List<ObjectRefData> addedComponents = new List<ObjectRefData>();
        public List<ObjectRefData> removedGameObjects = new List<ObjectRefData>();
        public List<ObjectRefData> removedComponents = new List<ObjectRefData>();
    }

    [Serializable]
    private class PropertyModData
    {
        public string targetPath;
        public string targetKey;
        public string targetType;
        public ObjectId targetId;

        public string propertyPath;
        public string value;

        public ObjectRefData objectReference;
    }

    [Serializable]
    private struct Vec2
    {
        public float x, y;
        public Vec2(float x, float y) { this.x = x; this.y = y; }
        public static Vec2 From(Vector2 v) => new Vec2(v.x, v.y);
    }

    [Serializable]
    private struct Vec3
    {
        public float x, y, z;
        public Vec3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vec3 From(Vector3 v) => new Vec3(v.x, v.y, v.z);
    }

    [Serializable]
    private struct Rect2
    {
        public float x;
        public float y;
        public float w;
        public float h;
    }

    #endregion

    #region Export Core

    private static GOData ExportGameObjectRecursive(GameObject go)
    {
        var data = new GOData
        {
            name = go.name,
            hierarchyPath = GetHierarchyPath(go.transform),
            hierarchyKey = GetHierarchyKey(go.transform),
            activeSelf = go.activeSelf,

            tag = SafeGetTag(go),
            layer = go.layer,
            siblingIndex = go.transform.GetSiblingIndex(),

            objectId = BuildObjectId(go, null, null),
            prefab = BuildPrefabLink(go),

            rectInfo = BuildRectInfo(go),
            layoutInfo = BuildLayoutInfo(go)
        };

        var comps = go.GetComponents<Component>();
        foreach (var comp in comps)
        {
            var cd = new ComponentData();

            if (comp == null)
            {
                cd.type = "(Missing Script)";
                cd.isMissingScript = true;
                cd.isAddedComponentOverride = false;

                cd.hasEnabled = false;
                cd.enabled = false;

                cd.isMonoBehaviour = false;
                cd.scriptAssetPath = "";
                cd.scriptGuid = "";
                cd.scriptFileId = "";
                cd.scriptClassFullName = "";
                cd.scriptAssemblyName = "";

                cd.properties = new List<Prop>();
                cd.objectRefs = new List<ObjectRefData>();
                cd.eventBindings = new List<EventBinding>();

                cd.hasSprite = false;
                cd.spritePath = "";
                cd.spriteGuid = "";
                cd.hasSpriteAtlas = false;
                cd.spriteAtlasPath = "";
                cd.spriteAtlasGuid = "";

                data.components.Add(cd);
                continue;
            }

            cd.type = comp.GetType().FullName;
            cd.isMissingScript = false;
            cd.isAddedComponentOverride = SafeIsAddedComponentOverride(comp);

            cd.hasEnabled = TryGetEnabled(comp, out bool en);
            cd.enabled = en;

            FillScriptIdentity(comp, cd);

            cd.properties = ExtractPropertiesCompact(comp);
            cd.objectRefs = ExtractObjectReferences(comp);
            cd.eventBindings = ExtractUnityEventBindings(comp);

            FillSpriteAtlasInfo(comp, cd);

            data.components.Add(cd);
        }

        for (int i = 0; i < go.transform.childCount; i++)
        {
            data.children.Add(ExportGameObjectRecursive(go.transform.GetChild(i).gameObject));
        }

        return data;
    }

    #endregion

    #region Source / Stage

    private static SourceInfo BuildSourceInfo(GameObject selected, string explicitPrefabAssetPath)
    {
        var src = new SourceInfo();

        PrefabStage stage = null;
        try { stage = PrefabStageUtility.GetCurrentPrefabStage(); } catch { /* ignore */ }

        if (stage != null)
        {
            src.stageKind = "PrefabStage";
            src.prefabStagePrefabAssetPath = stage.assetPath ?? "";
            src.prefabStagePrefabGuid = string.IsNullOrEmpty(src.prefabStagePrefabAssetPath)
                ? ""
                : AssetDatabase.AssetPathToGUID(src.prefabStagePrefabAssetPath);
        }
        else
        {
            src.stageKind = "MainStage";
            src.prefabStagePrefabAssetPath = "";
            src.prefabStagePrefabGuid = "";
        }

        src.selectionHierarchyPath = GetHierarchyPath(selected.transform);
        src.selectionHierarchyKey = GetHierarchyKey(selected.transform);

        var scene = selected.scene;
        src.selectionSceneName = scene.IsValid() ? scene.name : "";
        src.selectionScenePath = scene.IsValid() ? (scene.path ?? "") : "";
        src.selectionSceneIsSaved = !string.IsNullOrEmpty(src.selectionScenePath);
        src.selectionSceneGuid = src.selectionSceneIsSaved ? AssetDatabase.AssetPathToGUID(src.selectionScenePath) : "";

        if (!string.IsNullOrEmpty(explicitPrefabAssetPath))
        {
            src.selectionKind = "PrefabAsset";
            src.selectionPrefabAssetPath = explicitPrefabAssetPath;
            src.selectionPrefabGuid = AssetDatabase.AssetPathToGUID(explicitPrefabAssetPath);
            src.selectionPrefabInstanceStatus = "PrefabAsset";
        }
        else if (PrefabUtility.IsPartOfPrefabInstance(selected))
        {
            src.selectionKind = "PrefabInstanceInScene";

            var instanceRoot = FindNearestInstanceRootByWalkingParents(selected);
            var p = instanceRoot != null ? PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instanceRoot) : "";
            src.selectionPrefabAssetPath = p ?? "";
            src.selectionPrefabGuid = string.IsNullOrEmpty(src.selectionPrefabAssetPath)
                ? ""
                : AssetDatabase.AssetPathToGUID(src.selectionPrefabAssetPath);

            src.selectionPrefabInstanceStatus = instanceRoot != null
                ? PrefabUtility.GetPrefabInstanceStatus(instanceRoot).ToString()
                : "PrefabInstance(UnknownRoot)";
        }
        else
        {
            var assetPath = AssetDatabase.GetAssetPath(selected);
            if (!string.IsNullOrEmpty(assetPath) && assetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                src.selectionKind = "PrefabAssetInStage";
                src.selectionPrefabAssetPath = assetPath;
                src.selectionPrefabGuid = AssetDatabase.AssetPathToGUID(assetPath);
                src.selectionPrefabInstanceStatus = "PrefabAsset";
            }
            else
            {
                src.selectionKind = "SceneObject";
                src.selectionPrefabAssetPath = "";
                src.selectionPrefabGuid = "";
                src.selectionPrefabInstanceStatus = "NotAPrefab";
            }
        }

        src.selectionObjectId = BuildObjectId(selected, src.selectionHierarchyPath, src.selectionHierarchyKey);
        return src;
    }

    #endregion

    #region Prefab Info / Overrides

    private static PrefabLink BuildPrefabLink(GameObject go)
    {
        var link = new PrefabLink();

        if (PrefabUtility.IsPartOfPrefabInstance(go))
        {
            link.isPartOfPrefabInstance = true;
            link.isAddedGameObjectOverride = PrefabUtility.IsAddedGameObjectOverride(go);

            var instanceRoot = FindNearestInstanceRootByWalkingParents(go);
            link.isPrefabInstanceRoot = (instanceRoot != null && instanceRoot == go);

            link.nearestInstanceRootPath = instanceRoot != null ? GetHierarchyPath(instanceRoot.transform) : "";
            link.nearestInstanceRootKey = instanceRoot != null ? GetHierarchyKey(instanceRoot.transform) : "";

            link.prefabAssetPath = instanceRoot != null
                ? PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instanceRoot)
                : null;

            link.prefabAssetGuid = string.IsNullOrEmpty(link.prefabAssetPath)
                ? ""
                : AssetDatabase.AssetPathToGUID(link.prefabAssetPath);

            link.prefabInstanceStatus = instanceRoot != null
                ? PrefabUtility.GetPrefabInstanceStatus(instanceRoot).ToString()
                : "PrefabInstance(UnknownRoot)";

            FillAssetPackagingInfo(link.prefabAssetPath, out link.assetBundleName, out link.addressableAddress);
            return link;
        }

        var assetPath = AssetDatabase.GetAssetPath(go);
        if (!string.IsNullOrEmpty(assetPath) && assetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
        {
            link.isPartOfPrefabInstance = false;
            link.isPrefabInstanceRoot = false;
            link.isAddedGameObjectOverride = false;

            link.nearestInstanceRootPath = "";
            link.nearestInstanceRootKey = "";

            link.prefabAssetPath = assetPath;
            link.prefabAssetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            link.prefabInstanceStatus = "PrefabAsset";

            FillAssetPackagingInfo(link.prefabAssetPath, out link.assetBundleName, out link.addressableAddress);
            return link;
        }

        link.isPartOfPrefabInstance = false;
        link.isPrefabInstanceRoot = false;
        link.isAddedGameObjectOverride = false;
        link.nearestInstanceRootPath = "";
        link.nearestInstanceRootKey = "";
        link.prefabAssetPath = null;
        link.prefabAssetGuid = "";
        link.prefabInstanceStatus = "NotAPrefab";
        link.assetBundleName = "";
        link.addressableAddress = "";
        return link;
    }

    private static List<PrefabInstanceData> CollectPrefabInstanceData(GameObject selectionRoot)
    {
        var roots = new HashSet<GameObject>();
        foreach (var t in selectionRoot.GetComponentsInChildren<Transform>(true))
        {
            var r = FindNearestInstanceRootByWalkingParents(t.gameObject);
            if (r != null) roots.Add(r);
        }

        var list = new List<PrefabInstanceData>();

        foreach (var root in roots)
        {
            var pid = new PrefabInstanceData
            {
                instanceRootPath = GetHierarchyPath(root.transform),
                instanceRootKey = GetHierarchyKey(root.transform),
                instanceRootId = BuildObjectId(root, null, null),

                prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root),
                prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(root).ToString(),
                prefabAssetGuid = ""
            };

            if (!string.IsNullOrEmpty(pid.prefabAssetPath))
                pid.prefabAssetGuid = AssetDatabase.AssetPathToGUID(pid.prefabAssetPath);

            var mods = PrefabUtility.GetPropertyModifications(root);
            if (mods != null)
            {
                foreach (var m in mods)
                {
                    if (m == null || m.target == null) continue;

                    pid.propertyModifications.Add(new PropertyModData
                    {
                        targetPath = GetObjectPath(m.target),
                        targetKey = GetObjectKey(m.target),
                        targetType = m.target.GetType().FullName,
                        targetId = BuildObjectId(m.target, null, null),

                        propertyPath = m.propertyPath,
                        value = m.value ?? "",

                        objectReference = m.objectReference ? BuildObjectRefData(m.objectReference, null) : null
                    });
                }
            }

            pid.addedGameObjects = CollectAddedGameObjectsUnderInstanceRoot(root);
            pid.addedComponents = CollectAddedComponentsUnderInstanceRoot(root);

            pid.removedGameObjects = CollectRemovedGameObjects(root);
            pid.removedComponents = CollectRemovedComponents(root);

            list.Add(pid);
        }

        return list;
    }

    private static List<ObjectRefData> CollectAddedGameObjectsUnderInstanceRoot(GameObject instanceRoot)
    {
        var list = new List<ObjectRefData>();
        foreach (var t in instanceRoot.GetComponentsInChildren<Transform>(true))
        {
            var go = t.gameObject;
            try
            {
                if (PrefabUtility.IsAddedGameObjectOverride(go))
                    list.Add(BuildObjectRefData(go, null));
            }
            catch { /* ignore */ }
        }
        return list;
    }

    private static List<ObjectRefData> CollectAddedComponentsUnderInstanceRoot(GameObject instanceRoot)
    {
        var list = new List<ObjectRefData>();
        foreach (var t in instanceRoot.GetComponentsInChildren<Transform>(true))
        {
            var comps = t.GetComponents<Component>();
            foreach (var c in comps)
            {
                if (c == null) continue;
                if (SafeIsAddedComponentOverride(c))
                    list.Add(BuildObjectRefData(c, null));
            }
        }
        return list;
    }

    private static List<ObjectRefData> CollectRemovedGameObjects(GameObject instanceRoot)
    {
        var list = new List<ObjectRefData>();
        try
        {
            var mi = typeof(PrefabUtility).GetMethod(
                "GetRemovedGameObjects",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(GameObject) },
                null
            );
            if (mi == null) return list;

            var result = mi.Invoke(null, new object[] { instanceRoot });
            if (result is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is GameObject go && go != null)
                        list.Add(BuildObjectRefData(go, null));
                }
            }
        }
        catch { /* ignore */ }
        return list;
    }

    private static List<ObjectRefData> CollectRemovedComponents(GameObject instanceRoot)
    {
        var list = new List<ObjectRefData>();
        try
        {
            var mi = typeof(PrefabUtility).GetMethod(
                "GetRemovedComponents",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(GameObject) },
                null
            );
            if (mi == null) return list;

            var result = mi.Invoke(null, new object[] { instanceRoot });
            if (result is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is Component c && c != null)
                        list.Add(BuildObjectRefData(c, null));
                }
            }
        }
        catch { /* ignore */ }
        return list;
    }

    #endregion

    #region Properties (Compact) + ObjectRefs

    private static List<Prop> ExtractPropertiesCompact(Component comp)
    {
        var list = new List<Prop>();
        if (comp == null) return list;

        SerializedObject so;
        try { so = new SerializedObject(comp); }
        catch { return list; }

        var it = so.GetIterator();
        bool isTransform = comp is Transform;
        bool enterChildren = true;

        while (true)
        {
            bool moved;
            try { moved = it.NextVisible(enterChildren); }
            catch { break; }

            if (!moved) break;

            // CORE: never expand children to avoid redundancy (rotation.x/y/z/w etc.)
            enterChildren = false;

            var path = it.propertyPath;

            // m_Script already exported via script identity
            if (path == "m_Script") continue;

            // ObjectReference is exported only in objectRefs list (avoid empty objects in properties)
            if (it.propertyType == SerializedPropertyType.ObjectReference) continue;

            // Skip Transform defaults
            if (isTransform)
            {
                if (path == "m_LocalPosition" && it.propertyType == SerializedPropertyType.Vector3)
                {
                    var v = it.vector3Value;
                    if (Approximately(v, Vector3.zero)) continue;
                }
                if (path == "m_LocalRotation" && it.propertyType == SerializedPropertyType.Quaternion)
                {
                    var q = it.quaternionValue;
                    if (Approximately(q, Quaternion.identity)) continue;
                }
                if (path == "m_LocalScale" && it.propertyType == SerializedPropertyType.Vector3)
                {
                    var v = it.vector3Value;
                    if (Approximately(v, Vector3.one)) continue;
                }
            }

            // Arrays: summary only
            if (it.isArray && it.propertyType != SerializedPropertyType.String)
            {
                list.Add(new Prop
                {
                    p = path,
                    t = "a",
                    v = "Array",
                    n = SafeArraySize(it)
                });
                continue;
            }

            // ManagedReference: summary only
            if (it.propertyType == SerializedPropertyType.ManagedReference)
            {
                var tn = "";
                try { tn = it.managedReferenceFullTypename ?? ""; } catch { tn = ""; }
                list.Add(new Prop { p = path, t = "g", v = string.IsNullOrEmpty(tn) ? "(ManagedRef)" : tn, n = 0 });
                continue;
            }

            // Generic: whitelist formatter (RectOffset, ColorBlock, Navigation, SpriteState, AnimationTriggers)
            if (it.propertyType == SerializedPropertyType.Generic)
            {
                if (TryFormatKnownGeneric(it, out var tc, out var val))
                {
                    list.Add(new Prop { p = path, t = tc, v = val, n = 0 });
                }
                else
                {
                    var tname = it.type ?? "";
                    if (tname.Length > MAX_GENERIC_TYPE_NAME_LEN) tname = tname.Substring(0, MAX_GENERIC_TYPE_NAME_LEN);
                    list.Add(new Prop { p = path, t = "g", v = string.IsNullOrEmpty(tname) ? "(Generic)" : $"(Generic:{tname})", n = 0 });
                }
                continue;
            }

            try
            {
                switch (it.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        list.Add(new Prop { p = path, t = "i", v = it.intValue.ToString(CultureInfo.InvariantCulture), n = 0 });
                        break;

                    case SerializedPropertyType.Boolean:
                        list.Add(new Prop { p = path, t = "b", v = it.boolValue ? "true" : "false", n = 0 });
                        break;

                    case SerializedPropertyType.Float:
                        list.Add(new Prop { p = path, t = "f", v = FormatFloat(it.floatValue), n = 0 });
                        break;

                    case SerializedPropertyType.String:
                        list.Add(new Prop { p = path, t = "s", v = Trunc(it.stringValue ?? "", MAX_STRING_LEN), n = 0 });
                        break;

                    case SerializedPropertyType.Enum:
                        list.Add(new Prop { p = path, t = "e", v = $"{it.enumValueIndex}:{SafeEnumName(it)}", n = 0 });
                        break;

                    case SerializedPropertyType.Vector2:
                        {
                            var v = it.vector2Value;
                            list.Add(new Prop { p = path, t = "v2", v = $"({FormatFloat(v.x)},{FormatFloat(v.y)})", n = 0 });
                            break;
                        }

                    case SerializedPropertyType.Vector3:
                        {
                            var v = it.vector3Value;
                            list.Add(new Prop { p = path, t = "v3", v = $"({FormatFloat(v.x)},{FormatFloat(v.y)},{FormatFloat(v.z)})", n = 0 });
                            break;
                        }

                    case SerializedPropertyType.Vector4:
                        {
                            var v = it.vector4Value;
                            list.Add(new Prop { p = path, t = "v4", v = $"({FormatFloat(v.x)},{FormatFloat(v.y)},{FormatFloat(v.z)},{FormatFloat(v.w)})", n = 0 });
                            break;
                        }

                    case SerializedPropertyType.Quaternion:
                        {
                            var q = it.quaternionValue;
                            list.Add(new Prop { p = path, t = "q", v = $"({FormatFloat(q.x)},{FormatFloat(q.y)},{FormatFloat(q.z)},{FormatFloat(q.w)})", n = 0 });
                            break;
                        }

                    case SerializedPropertyType.Color:
                        {
                            var c = it.colorValue;
                            list.Add(new Prop { p = path, t = "c", v = $"rgba({FormatFloat(c.r)},{FormatFloat(c.g)},{FormatFloat(c.b)},{FormatFloat(c.a)})", n = 0 });
                            break;
                        }

                    case SerializedPropertyType.Rect:
                        {
                            var r = it.rectValue;
                            list.Add(new Prop { p = path, t = "r", v = $"Rect({FormatFloat(r.x)},{FormatFloat(r.y)},{FormatFloat(r.width)},{FormatFloat(r.height)})", n = 0 });
                            break;
                        }

                    case SerializedPropertyType.Bounds:
                        {
                            var b = it.boundsValue;
                            var c = b.center;
                            var s = b.size;
                            list.Add(new Prop
                            {
                                p = path,
                                t = "bd",
                                v = $"Bounds(c=({FormatFloat(c.x)},{FormatFloat(c.y)},{FormatFloat(c.z)}),s=({FormatFloat(s.x)},{FormatFloat(s.y)},{FormatFloat(s.z)}))",
                                n = 0
                            });
                            break;
                        }

                    case SerializedPropertyType.LayerMask:
                    case SerializedPropertyType.Character:
                        list.Add(new Prop { p = path, t = "i", v = it.intValue.ToString(CultureInfo.InvariantCulture), n = 0 });
                        break;

                    case SerializedPropertyType.AnimationCurve:
                        list.Add(new Prop { p = path, t = "g", v = "(AnimationCurve)", n = 0 });
                        break;

                    case SerializedPropertyType.Gradient:
                        list.Add(new Prop { p = path, t = "g", v = "(Gradient)", n = 0 });
                        break;

                    case SerializedPropertyType.ExposedReference:
                        list.Add(new Prop { p = path, t = "g", v = "(ExposedReference)", n = 0 });
                        break;

                    default:
                        list.Add(new Prop { p = path, t = "?", v = "", n = 0 });
                        break;
                }
            }
            catch
            {
                list.Add(new Prop { p = path, t = "?", v = "(ReadFailed)", n = 0 });
            }
        }

        return list;
    }

    private static List<ObjectRefData> ExtractObjectReferences(Component comp)
    {
        var list = new List<ObjectRefData>();
        if (comp == null) return list;

        SerializedObject so;
        try { so = new SerializedObject(comp); }
        catch { return list; }

        var it = so.GetIterator();
        bool enterChildren = true;

        while (true)
        {
            bool moved;
            try { moved = it.NextVisible(enterChildren); }
            catch { break; }

            if (!moved) break;

            enterChildren = true;

            // Guard: do not deep-scan huge arrays
            if (it.isArray && it.propertyType != SerializedPropertyType.String)
            {
                int sz = SafeArraySize(it);
                if (sz > MAX_ARRAY_SCAN_FOR_OBJECTREFS)
                    enterChildren = false;
            }

            if (it.propertyType != SerializedPropertyType.ObjectReference) continue;

            UnityEngine.Object obj;
            try { obj = it.objectReferenceValue; }
            catch { continue; }

            if (obj == null) continue;

            list.Add(BuildObjectRefData(obj, it.propertyPath));
        }

        return list;
    }

    private static ObjectRefData BuildObjectRefData(UnityEngine.Object obj, string propertyPath)
    {
        if (obj == null) return null;

        return new ObjectRefData
        {
            propertyPath = propertyPath ?? "",
            referencedType = obj.GetType().FullName,
            referencedName = obj.name,
            hierarchyPath = GetObjectPath(obj),
            hierarchyKey = GetObjectKey(obj),
            objectId = BuildObjectId(obj, null, null)
        };
    }

    #endregion

    #region Generic whitelist formatters (RectOffset / ColorBlock / Navigation / SpriteState / AnimationTriggers)

    private static bool TryFormatKnownGeneric(SerializedProperty p, out string typeCode, out string value)
    {
        typeCode = "g";
        value = "";

        if (p == null || p.propertyType != SerializedPropertyType.Generic)
            return false;

        if (TryFormatRectOffset(p, out value)) { typeCode = "ro"; return true; }
        if (TryFormatColorBlock(p, out value)) { typeCode = "cb"; return true; }
        if (TryFormatNavigation(p, out value)) { typeCode = "nav"; return true; }
        if (TryFormatSpriteState(p, out value)) { typeCode = "ss"; return true; }
        if (TryFormatAnimationTriggers(p, out value)) { typeCode = "trg"; return true; }

        return false;
    }

    private static bool TypeIs(SerializedProperty p, string simpleTypeName)
    {
        if (p == null) return false;
        var t = p.type;
        if (string.IsNullOrEmpty(t)) return false;

        if (string.Equals(t, simpleTypeName, StringComparison.OrdinalIgnoreCase))
            return true;

        return t.EndsWith("." + simpleTypeName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryFormatRectOffset(SerializedProperty p, out string formatted)
    {
        formatted = "";
        if (!TypeIs(p, "RectOffset")) return false;

        int left = ReadIntRelative(p, "m_Left", "left");
        int right = ReadIntRelative(p, "m_Right", "right");
        int top = ReadIntRelative(p, "m_Top", "top");
        int bottom = ReadIntRelative(p, "m_Bottom", "bottom");

        formatted = $"RectOffset(l={left},r={right},t={top},b={bottom})";
        return true;
    }

    private static bool TryFormatColorBlock(SerializedProperty p, out string formatted)
    {
        formatted = "";
        if (!TypeIs(p, "ColorBlock")) return false;

        var normal = ReadColorRelative(p, "m_NormalColor", "normalColor");
        var highlighted = ReadColorRelative(p, "m_HighlightedColor", "highlightedColor");
        var pressed = ReadColorRelative(p, "m_PressedColor", "pressedColor");
        var selected = ReadColorRelative(p, "m_SelectedColor", "selectedColor");
        var disabled = ReadColorRelative(p, "m_DisabledColor", "disabledColor");

        float mult = ReadFloatRelative(p, "m_ColorMultiplier", "colorMultiplier");
        float fade = ReadFloatRelative(p, "m_FadeDuration", "fadeDuration");

        formatted = $"ColorBlock(n={FmtColor(normal)},h={FmtColor(highlighted)},p={FmtColor(pressed)},s={FmtColor(selected)},d={FmtColor(disabled)},mult={FormatFloat(mult)},fade={FormatFloat(fade)})";
        return true;
    }

    private static bool TryFormatNavigation(SerializedProperty p, out string formatted)
    {
        formatted = "";
        if (!TypeIs(p, "Navigation")) return false;

        string mode = ReadEnumRelative(p, "m_Mode", "mode");

        // Summary only; the real wiring is in component.objectRefs (stable IDs)
        string up = ReadObjNameRelative(p, "m_SelectOnUp", "selectOnUp");
        string down = ReadObjNameRelative(p, "m_SelectOnDown", "selectOnDown");
        string left = ReadObjNameRelative(p, "m_SelectOnLeft", "selectOnLeft");
        string right = ReadObjNameRelative(p, "m_SelectOnRight", "selectOnRight");

        formatted = $"Navigation(mode={mode},up={up},down={down},left={left},right={right})";
        return true;
    }

    private static bool TryFormatSpriteState(SerializedProperty p, out string formatted)
    {
        formatted = "";
        if (!TypeIs(p, "SpriteState")) return false;

        string h = ReadObjNameRelative(p, "m_HighlightedSprite", "highlightedSprite");
        string pr = ReadObjNameRelative(p, "m_PressedSprite", "pressedSprite");
        string sel = ReadObjNameRelative(p, "m_SelectedSprite", "selectedSprite");
        string dis = ReadObjNameRelative(p, "m_DisabledSprite", "disabledSprite");

        formatted = $"SpriteState(h={h},p={pr},s={sel},d={dis})";
        return true;
    }

    private static bool TryFormatAnimationTriggers(SerializedProperty p, out string formatted)
    {
        formatted = "";
        if (!TypeIs(p, "AnimationTriggers")) return false;

        string n = ReadStringRelative(p, "m_NormalTrigger", "normalTrigger");
        string h = ReadStringRelative(p, "m_HighlightedTrigger", "highlightedTrigger");
        string pr = ReadStringRelative(p, "m_PressedTrigger", "pressedTrigger");
        string sel = ReadStringRelative(p, "m_SelectedTrigger", "selectedTrigger");
        string dis = ReadStringRelative(p, "m_DisabledTrigger", "disabledTrigger");

        formatted = $"AnimationTriggers(n={Q(n)},h={Q(h)},p={Q(pr)},s={Q(sel)},d={Q(dis)})";
        return true;
    }

    private static int ReadIntRelative(SerializedProperty parent, params string[] names)
    {
        var child = FindAnyRelative(parent, names);
        if (child == null) return 0;
        try { if (child.propertyType == SerializedPropertyType.Integer) return child.intValue; }
        catch { }
        return 0;
    }

    private static float ReadFloatRelative(SerializedProperty parent, params string[] names)
    {
        var child = FindAnyRelative(parent, names);
        if (child == null) return 0f;
        try
        {
            if (child.propertyType == SerializedPropertyType.Float) return child.floatValue;
            if (child.propertyType == SerializedPropertyType.Integer) return child.intValue;
        }
        catch { }
        return 0f;
    }

    private static string ReadStringRelative(SerializedProperty parent, params string[] names)
    {
        var child = FindAnyRelative(parent, names);
        if (child == null) return "";
        try { if (child.propertyType == SerializedPropertyType.String) return child.stringValue ?? ""; }
        catch { }
        return "";
    }

    private static string ReadEnumRelative(SerializedProperty parent, params string[] names)
    {
        var child = FindAnyRelative(parent, names);
        if (child == null) return "Unknown";
        try
        {
            if (child.propertyType == SerializedPropertyType.Enum)
            {
                int idx = child.enumValueIndex;
                var dn = child.enumDisplayNames;
                if (dn != null && idx >= 0 && idx < dn.Length)
                    return $"{dn[idx]}({idx})";
                return idx.ToString(CultureInfo.InvariantCulture);
            }
            if (child.propertyType == SerializedPropertyType.Integer)
                return child.intValue.ToString(CultureInfo.InvariantCulture);
        }
        catch { }
        return "Unknown";
    }

    private static Color ReadColorRelative(SerializedProperty parent, params string[] names)
    {
        var child = FindAnyRelative(parent, names);
        if (child == null) return default;
        try { if (child.propertyType == SerializedPropertyType.Color) return child.colorValue; }
        catch { }
        return default;
    }

    private static string ReadObjNameRelative(SerializedProperty parent, params string[] names)
    {
        var child = FindAnyRelative(parent, names);
        if (child == null) return "null";
        try
        {
            if (child.propertyType == SerializedPropertyType.ObjectReference)
            {
                var o = child.objectReferenceValue;
                return o ? o.name : "null";
            }
        }
        catch { }
        return "null";
    }

    private static SerializedProperty FindAnyRelative(SerializedProperty parent, params string[] names)
    {
        if (parent == null || names == null) return null;

        for (int i = 0; i < names.Length; i++)
        {
            try
            {
                var c = parent.FindPropertyRelative(names[i]);
                if (c != null) return c;
            }
            catch { /* ignore */ }
        }
        return null;
    }

    private static string FmtColor(Color c)
    {
        return $"rgba({FormatFloat(c.r)},{FormatFloat(c.g)},{FormatFloat(c.b)},{FormatFloat(c.a)})";
    }

    private static string Q(string s)
    {
        if (string.IsNullOrEmpty(s)) return "\"\"";
        if (s.Length > 128) s = s.Substring(0, 128) + "...";
        return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }

    #endregion

    #region UnityEvent Persistent Bindings

    private static List<EventBinding> ExtractUnityEventBindings(Component comp)
    {
        var list = new List<EventBinding>();
        if (comp == null) return list;

        var type = comp.GetType();

        for (var t = type; t != null && t != typeof(Component); t = t.BaseType)
        {
            FieldInfo[] fields;
            try { fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly); }
            catch { continue; }

            foreach (var f in fields)
            {
                if (f == null) continue;
                if (!typeof(UnityEventBase).IsAssignableFrom(f.FieldType)) continue;

                UnityEventBase ev;
                try { ev = f.GetValue(comp) as UnityEventBase; }
                catch { continue; }

                if (ev == null) continue;

                int count = 0;
                try { count = ev.GetPersistentEventCount(); } catch { }

                var paramTypes = GetUnityEventParameterTypes(f.FieldType);
                var paramTypeNames = new string[paramTypes.Count];
                for (int i = 0; i < paramTypes.Count; i++)
                    paramTypeNames[i] = paramTypes[i] != null ? paramTypes[i].FullName : "Unknown";

                for (int i = 0; i < count; i++)
                {
                    UnityEngine.Object target = null;
                    string methodName = "";

                    try
                    {
                        target = ev.GetPersistentTarget(i);
                        methodName = ev.GetPersistentMethodName(i) ?? "";
                    }
                    catch { }

                    var binding = new EventBinding
                    {
                        eventFieldName = f.Name,
                        eventFieldType = f.FieldType.FullName,
                        eventParameterTypes = paramTypeNames,
                        callIndex = i,

                        target = target ? BuildObjectRefData(target, null) : null,
                        methodName = methodName,
                        resolvedMethodSignature = ResolvePersistentMethodSignature(target, methodName, paramTypes),
                        argumentType = (paramTypes.Count == 0) ? "None" : string.Join(",", BuildGenericArgLabels(paramTypes.Count))
                    };

                    if ((binding.target != null) || !string.IsNullOrEmpty(binding.methodName))
                        list.Add(binding);
                }
            }
        }

        return list;
    }

    private static List<Type> GetUnityEventParameterTypes(Type eventFieldType)
    {
        var list = new List<Type>();

        for (var t = eventFieldType; t != null; t = t.BaseType)
        {
            if (!t.IsGenericType) continue;

            var def = t.GetGenericTypeDefinition();
            if (def == typeof(UnityEvent<>)
                || def == typeof(UnityEvent<,>)
                || def == typeof(UnityEvent<,,>)
                || def == typeof(UnityEvent<,,,>))
            {
                list.AddRange(t.GetGenericArguments());
                break;
            }
        }

        return list;
    }

    private static string ResolvePersistentMethodSignature(UnityEngine.Object target, string methodName, List<Type> eventParamTypes)
    {
        if (target == null || string.IsNullOrEmpty(methodName))
            return "";

        try
        {
            var tt = target.GetType();
            var methods = tt.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var m in methods)
            {
                if (m.Name != methodName) continue;

                var ps = m.GetParameters();
                if (ps.Length != eventParamTypes.Count) continue;

                bool ok = true;
                for (int i = 0; i < ps.Length; i++)
                {
                    var want = eventParamTypes[i];
                    if (want == null) continue;

                    if (!ps[i].ParameterType.IsAssignableFrom(want) && !want.IsAssignableFrom(ps[i].ParameterType))
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok)
                    return $"{tt.FullName}.{m.Name}({string.Join(", ", ParamTypeNames(ps))})";
            }

            foreach (var m in methods)
            {
                if (m.Name != methodName) continue;
                var ps = m.GetParameters();
                return $"{tt.FullName}.{m.Name}({string.Join(", ", ParamTypeNames(ps))})";
            }
        }
        catch { }

        return "";
    }

    private static IEnumerable<string> ParamTypeNames(ParameterInfo[] ps)
    {
        for (int i = 0; i < ps.Length; i++)
            yield return ps[i].ParameterType != null ? ps[i].ParameterType.FullName : "Unknown";
    }

    private static string[] BuildGenericArgLabels(int n)
    {
        var arr = new string[n];
        for (int i = 0; i < n; i++) arr[i] = $"T{i}";
        return arr;
    }

    #endregion

    #region Rect / Layout

    private static RectInfo BuildRectInfo(GameObject go)
    {
        var rt = go.transform as RectTransform;
        if (rt == null) return new RectInfo { isRectTransform = false };

        var info = new RectInfo
        {
            isRectTransform = true,
            anchorMin = Vec2.From(rt.anchorMin),
            anchorMax = Vec2.From(rt.anchorMax),
            pivot = Vec2.From(rt.pivot),
            anchoredPosition = Vec2.From(rt.anchoredPosition),
            sizeDelta = Vec2.From(rt.sizeDelta),

            worldCorners = null,
            screenCorners = null,
            screenRect = new Rect2 { x = 0, y = 0, w = 0, h = 0 },

            canvasPath = "",
            canvasRenderMode = "",
            canvasWorldCameraPath = "",
            canvasScaleFactor = 1f,
            canvasReferenceResolution = new Vec2(0, 0)
        };

        var canvas = go.GetComponentInParent<Canvas>();
        Camera cam = null;
        if (canvas != null)
        {
            info.canvasPath = GetHierarchyPath(canvas.transform);
            info.canvasRenderMode = canvas.renderMode.ToString();
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                cam = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;

            info.canvasWorldCameraPath = (cam != null) ? GetHierarchyPath(cam.transform) : "";
            info.canvasScaleFactor = canvas.scaleFactor;

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
                info.canvasReferenceResolution = Vec2.From(scaler.referenceResolution);
        }

        try
        {
            var wc = new Vector3[4];
            rt.GetWorldCorners(wc);
            info.worldCorners = new Vec3[4]
            {
                Vec3.From(wc[0]), Vec3.From(wc[1]), Vec3.From(wc[2]), Vec3.From(wc[3])
            };

            var sc = new Vector2[4];
            for (int i = 0; i < 4; i++)
                sc[i] = RectTransformUtility.WorldToScreenPoint(cam, wc[i]);

            info.screenCorners = new Vec2[4]
            {
                Vec2.From(sc[0]), Vec2.From(sc[1]), Vec2.From(sc[2]), Vec2.From(sc[3])
            };

            float minX = sc[0].x, minY = sc[0].y, maxX = sc[0].x, maxY = sc[0].y;
            for (int i = 1; i < 4; i++)
            {
                minX = Mathf.Min(minX, sc[i].x);
                minY = Mathf.Min(minY, sc[i].y);
                maxX = Mathf.Max(maxX, sc[i].x);
                maxY = Mathf.Max(maxY, sc[i].y);
            }

            info.screenRect = new Rect2 { x = minX, y = minY, w = (maxX - minX), h = (maxY - minY) };
        }
        catch { }

        return info;
    }

    private static LayoutInfo BuildLayoutInfo(GameObject go)
    {
        var rt = go.transform as RectTransform;
        var lg = go.GetComponent<LayoutGroup>();
        if (rt == null || lg == null)
        {
            return new LayoutInfo
            {
                hasLayoutGroup = false,
                layoutGroupType = "",
                preferredWidth = 0,
                preferredHeight = 0,
                calculatedChildRects = new List<ChildRectInfo>()
            };
        }

        var info = new LayoutInfo
        {
            hasLayoutGroup = true,
            layoutGroupType = lg.GetType().FullName,
            preferredWidth = 0,
            preferredHeight = 0,
            calculatedChildRects = new List<ChildRectInfo>()
        };

        try
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            Canvas.ForceUpdateCanvases();
        }
        catch { }

        try
        {
            info.preferredWidth = LayoutUtility.GetPreferredWidth(rt);
            info.preferredHeight = LayoutUtility.GetPreferredHeight(rt);
        }
        catch { }

        for (int i = 0; i < rt.childCount; i++)
        {
            var child = rt.GetChild(i) as RectTransform;
            if (child == null) continue;

            var ri = BuildRectInfo(child.gameObject);

            info.calculatedChildRects.Add(new ChildRectInfo
            {
                childHierarchyPath = GetHierarchyPath(child),
                childHierarchyKey = GetHierarchyKey(child),
                childId = BuildObjectId(child.gameObject, null, null),
                screenRect = ri != null ? ri.screenRect : new Rect2(),
                worldCorners = ri != null ? ri.worldCorners : null
            });
        }

        return info;
    }

    #endregion

    #region Script Identity / SpriteAtlas / Enabled

    private static void FillScriptIdentity(Component comp, ComponentData cd)
    {
        cd.isMonoBehaviour = false;
        cd.scriptAssetPath = "";
        cd.scriptGuid = "";
        cd.scriptFileId = "";
        cd.scriptClassFullName = comp != null ? comp.GetType().FullName : "";
        cd.scriptAssemblyName = comp != null ? comp.GetType().Assembly.GetName().Name : "";

        if (comp is MonoBehaviour mb)
        {
            cd.isMonoBehaviour = true;
            try
            {
                var ms = MonoScript.FromMonoBehaviour(mb);
                if (ms != null)
                {
                    var path = AssetDatabase.GetAssetPath(ms) ?? "";
                    cd.scriptAssetPath = path;

                    if (TryGetGuidAndFileId(ms, out string guid, out string fileId, out string _))
                    {
                        cd.scriptGuid = guid;
                        cd.scriptFileId = fileId;
                    }
                    else if (!string.IsNullOrEmpty(path))
                    {
                        cd.scriptGuid = AssetDatabase.AssetPathToGUID(path);
                        cd.scriptFileId = "";
                    }
                }
            }
            catch { }
        }
    }

    private static void FillSpriteAtlasInfo(Component comp, ComponentData cd)
    {
        cd.hasSprite = false;
        cd.spritePath = "";
        cd.spriteGuid = "";
        cd.hasSpriteAtlas = false;
        cd.spriteAtlasPath = "";
        cd.spriteAtlasGuid = "";

        if (comp == null) return;
        if (comp.GetType().FullName != "UnityEngine.UI.Image") return;

        Sprite sprite = null;
        try
        {
            var pi = comp.GetType().GetProperty("sprite", BindingFlags.Instance | BindingFlags.Public);
            if (pi != null)
                sprite = pi.GetValue(comp, null) as Sprite;
        }
        catch { }

        if (sprite == null) return;

        cd.hasSprite = true;

        var sp = AssetDatabase.GetAssetPath(sprite) ?? "";
        cd.spritePath = sp;
        cd.spriteGuid = string.IsNullOrEmpty(sp) ? "" : AssetDatabase.AssetPathToGUID(sp);

        try
        {
            var t = Type.GetType("UnityEditor.U2D.SpriteAtlasUtility, UnityEditor");
            if (t == null) return;

            var mi = t.GetMethod("GetSpriteAtlas", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Sprite) }, null);
            if (mi == null) return;

            var atlasObj = mi.Invoke(null, new object[] { sprite }) as UnityEngine.Object;
            if (atlasObj == null) return;

            var ap = AssetDatabase.GetAssetPath(atlasObj) ?? "";
            if (string.IsNullOrEmpty(ap)) return;

            cd.hasSpriteAtlas = true;
            cd.spriteAtlasPath = ap;
            cd.spriteAtlasGuid = AssetDatabase.AssetPathToGUID(ap);
        }
        catch { }
    }

    private static bool TryGetEnabled(Component comp, out bool enabled)
    {
        enabled = false;
        if (comp == null) return false;

        try
        {
            if (comp is Behaviour b) { enabled = b.enabled; return true; }
            if (comp is Renderer r) { enabled = r.enabled; return true; }
            if (comp is Collider c) { enabled = c.enabled; return true; }

            var pi = comp.GetType().GetProperty("enabled", BindingFlags.Instance | BindingFlags.Public);
            if (pi != null && pi.PropertyType == typeof(bool))
            {
                enabled = (bool)pi.GetValue(comp, null);
                return true;
            }
        }
        catch { }

        return false;
    }

    #endregion

    #region Identity / Packaging / Addressables

    private static ObjectId BuildObjectId(UnityEngine.Object obj, string dataHierarchyPath, string dataHierarchyKey)
    {
        var id = new ObjectId
        {
            stableId = "",
            kind = "Unknown",

            globalObjectId = "",

            assetGuid = "",
            fileId = "",
            assetPath = "",

            scenePath = "",
            sceneName = "",

            hierarchyPath = dataHierarchyPath ?? GetObjectPath(obj),
            hierarchyKey = dataHierarchyKey ?? GetObjectKey(obj),

            instanceId = obj != null ? obj.GetInstanceID() : 0,

            assetBundleName = "",
            addressableAddress = ""
        };

        if (obj == null) return id;

        if (TryGetGuidAndFileId(obj, out string guid, out string fileId, out string assetPath))
        {
            id.kind = "Asset";
            id.assetGuid = guid;
            id.fileId = fileId;
            id.assetPath = assetPath;

            FillAssetPackagingInfo(id.assetPath, out id.assetBundleName, out id.addressableAddress);

            id.stableId = $"{guid}:{fileId}";
            return id;
        }

        var goid = SafeGetGlobalObjectId(obj);
        if (!IsNullLikeGlobalObjectId(goid))
        {
            id.kind = "SceneObject";
            id.globalObjectId = goid;

            var sc = ExtractScene(obj);
            if (sc.HasValue)
            {
                id.sceneName = sc.Value.name ?? "";
                id.scenePath = sc.Value.path ?? "";
            }

            id.stableId = goid;
            return id;
        }

        var scene = ExtractScene(obj);
        if (scene.HasValue)
        {
            id.sceneName = scene.Value.name ?? "";
            id.scenePath = scene.Value.path ?? "";
            if (string.IsNullOrEmpty(id.scenePath))
            {
                id.kind = "UnsavedSceneObject";
                id.stableId = $"{id.sceneName}:{id.hierarchyKey}";
                return id;
            }
        }

        id.kind = "Unknown";
        id.stableId = id.hierarchyKey;
        return id;
    }

    private static void FillAssetPackagingInfo(string assetPath, out string assetBundleName, out string addressableAddress)
    {
        assetBundleName = "";
        addressableAddress = "";

        if (string.IsNullOrEmpty(assetPath)) return;

        try
        {
            var importer = AssetImporter.GetAtPath(assetPath);
            if (importer != null)
                assetBundleName = importer.assetBundleName ?? "";
        }
        catch { }

        try
        {
            addressableAddress = TryGetAddressableAddressByAssetPath(assetPath) ?? "";
        }
        catch { }
    }

    private static bool _addrInit;
    private static object _addrSettings;
    private static MethodInfo _findAssetEntry;
    private static PropertyInfo _entryAddressProp;

    private static string TryGetAddressableAddressByAssetPath(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath)) return null;
        var guid = AssetDatabase.AssetPathToGUID(assetPath);
        if (string.IsNullOrEmpty(guid)) return null;
        return TryGetAddressableAddressByGuid(guid);
    }

    private static string TryGetAddressableAddressByGuid(string guid)
    {
        if (string.IsNullOrEmpty(guid)) return null;

        if (!_addrInit) InitAddressablesReflection();
        if (_addrSettings == null || _findAssetEntry == null || _entryAddressProp == null) return null;

        object entry = null;
        try { entry = _findAssetEntry.Invoke(_addrSettings, new object[] { guid }); }
        catch { return null; }

        if (entry == null) return null;

        try { return _entryAddressProp.GetValue(entry, null) as string; }
        catch { return null; }
    }

    private static void InitAddressablesReflection()
    {
        _addrInit = true;

        try
        {
            var defaultObjType =
                Type.GetType("UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject, Unity.Addressables.Editor");

            if (defaultObjType == null) return;

            object settings = null;
            var propSettings = defaultObjType.GetProperty("Settings", BindingFlags.Public | BindingFlags.Static);
            if (propSettings != null)
            {
                settings = propSettings.GetValue(null, null);
            }
            else
            {
                var miGetSettings = defaultObjType.GetMethod("GetSettings", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(bool) }, null);
                if (miGetSettings != null)
                    settings = miGetSettings.Invoke(null, new object[] { false });
            }

            if (settings == null) return;

            _addrSettings = settings;

            _findAssetEntry = settings.GetType().GetMethod("FindAssetEntry", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(string) }, null);
            if (_findAssetEntry == null) return;

            var entryType = _findAssetEntry.ReturnType;
            _entryAddressProp = entryType.GetProperty("address", BindingFlags.Public | BindingFlags.Instance)
                               ?? entryType.GetProperty("Address", BindingFlags.Public | BindingFlags.Instance);
        }
        catch
        {
            _addrSettings = null;
            _findAssetEntry = null;
            _entryAddressProp = null;
        }
    }

    #endregion

    #region Helpers

    private static GameObject FindNearestInstanceRootByWalkingParents(GameObject go)
    {
        var tr = go != null ? go.transform : null;
        while (tr != null)
        {
            GameObject root = null;
            try { root = PrefabUtility.GetNearestPrefabInstanceRoot(tr.gameObject); }
            catch { root = null; }

            if (root != null) return root;
            tr = tr.parent;
        }
        return null;
    }

    private static bool SafeIsAddedComponentOverride(Component c)
    {
        try { return PrefabUtility.IsAddedComponentOverride(c); }
        catch { return false; }
    }

    private static string GetHierarchyPath(Transform t)
    {
        var parts = new List<string>();
        while (t != null)
        {
            parts.Add(t.name);
            t = t.parent;
        }
        parts.Reverse();
        return string.Join("/", parts);
    }

    private static string GetHierarchyKey(Transform t)
    {
        var parts = new List<string>();
        while (t != null)
        {
            parts.Add($"{t.name}[{t.GetSiblingIndex()}]");
            t = t.parent;
        }
        parts.Reverse();
        return string.Join("/", parts);
    }

    private static string GetObjectPath(UnityEngine.Object obj)
    {
        if (obj == null) return "";
        if (obj is Component c) return GetHierarchyPath(c.transform);
        if (obj is GameObject g) return GetHierarchyPath(g.transform);
        return obj.name;
    }

    private static string GetObjectKey(UnityEngine.Object obj)
    {
        if (obj == null) return "";
        if (obj is Component c) return GetHierarchyKey(c.transform);
        if (obj is GameObject g) return GetHierarchyKey(g.transform);
        return obj.name;
    }

    private static Scene? ExtractScene(UnityEngine.Object obj)
    {
        try
        {
            if (obj is GameObject go) return go.scene;
            if (obj is Component c) return c.gameObject.scene;
        }
        catch { }
        return null;
    }

    private static string SafeGetGlobalObjectId(UnityEngine.Object obj)
    {
        try { return GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString(); }
        catch { return ""; }
    }

    private static bool IsNullLikeGlobalObjectId(string gid)
    {
        if (string.IsNullOrEmpty(gid)) return true;
        return gid.Contains("00000000000000000000000000000000");
    }

    private static bool TryGetGuidAndFileId(UnityEngine.Object obj, out string guid, out string fileId, out string assetPath)
    {
        guid = "";
        fileId = "";
        assetPath = "";

        try
        {
#if UNITY_2020_2_OR_NEWER
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out guid, out long localId))
            {
                fileId = localId.ToString(CultureInfo.InvariantCulture);
                assetPath = AssetDatabase.GUIDToAssetPath(guid);
                return true;
            }
#else
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out guid, out int localIdInt))
            {
                fileId = localIdInt.ToString(CultureInfo.InvariantCulture);
                assetPath = AssetDatabase.GUIDToAssetPath(guid);
                return true;
            }
#endif
        }
        catch { }

        return false;
    }

    private static string SafeGetTag(GameObject go)
    {
        try { return go.tag; }
        catch { return ""; }
    }

    private static string SafeEnumName(SerializedProperty p)
    {
        try
        {
            if (p.enumDisplayNames != null && p.enumValueIndex >= 0 && p.enumValueIndex < p.enumDisplayNames.Length)
                return p.enumDisplayNames[p.enumValueIndex];
        }
        catch { }
        return "";
    }

    private static int SafeArraySize(SerializedProperty p)
    {
        try { return p.arraySize; }
        catch { return 0; }
    }

    private static string Trunc(string s, int maxLen)
    {
        if (string.IsNullOrEmpty(s)) return "";
        if (s.Length <= maxLen) return s;
        return s.Substring(0, maxLen) + "...(truncated)";
    }

    private static string FormatFloat(float f)
    {
        return f.ToString("G9", CultureInfo.InvariantCulture);
    }

    private static bool Approximately(Vector3 a, Vector3 b, float eps = 1e-6f)
    {
        return Mathf.Abs(a.x - b.x) < eps && Mathf.Abs(a.y - b.y) < eps && Mathf.Abs(a.z - b.z) < eps;
    }

    private static bool Approximately(Quaternion a, Quaternion b, float eps = 1e-6f)
    {
        return Mathf.Abs(a.x - b.x) < eps && Mathf.Abs(a.y - b.y) < eps &&
               Mathf.Abs(a.z - b.z) < eps && Mathf.Abs(a.w - b.w) < eps;
    }

    private static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }

    #endregion
}
