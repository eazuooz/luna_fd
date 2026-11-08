using System;
using System.Collections.Generic;
using System.IO;
using EverydayDevup.Framework;
using UnityEditor;
using UnityEngine;

/// <summary>Rebuilds obsolete UI lookup caches from the current, GUID-preserved prefabs.</summary>
public static class LunaAssetRepair
{
    public static void RepairAndAudit()
    {
        RepairReferences();
        UpgradeLegacyEffectSerialization();
        LunaValidation.Audit();
    }

    private static void UpgradeLegacyEffectSerialization()
    {
        string[] paths =
        {
            "Assets/JMO Assets/Cartoon FX/Materials/Stretched/CFX_RayRounded.mat",
            "Assets/JMO Assets/Cartoon FX/Materials/Animated/CFX_Anim_Electricity_Add.mat",
            "Assets/JMO Assets/Cartoon FX/Materials/Electric/CFX_Sparkles_Add.mat",
            "Assets/JMO Assets/Cartoon FX/Materials/Electric/CFX_ElectricBolt_Add.mat",
            "Assets/JMO Assets/Cartoon FX/Mesh/CFX_Mesh_Circular1.5.asset"
        };
        foreach (string path in paths)
            if (AssetDatabase.LoadMainAssetAtPath(path) == null)
                throw new InvalidDataException("Cannot safely upgrade unreadable legacy effect asset: " + path);
        // Unity upgrades the serialized object schema while retaining its actual mesh/material contents.
        AssetDatabase.ForceReserializeAssets(paths, ForceReserializeAssetsOptions.ReserializeAssets);
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Luna/Validation/Rebuild UI Reference Assets")]
    public static void RepairReferences()
    {
        int repaired = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:UIReference", new[] { "Assets/Resources/UIReference" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var reference = AssetDatabase.LoadAssetAtPath<UIReference>(path);
            if (reference == null) continue;
            GameObject target = reference.uiTarget;
            if (reference.name == "UIQuitPopup")
                target = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources_moved/Prefabs/UIQuitPopup.prefab");
            if (target == null) throw new InvalidDataException("No verified prefab target for " + path);

            reference.uiTarget = target;
            reference.dic.Clear();
            foreach (Transform child in target.GetComponentsInChildren<Transform>(true))
            {
                string hierarchyPath = HierarchyPath(child);
                string key = reference.dic.ContainsKey(child.name) ? hierarchyPath : child.name;
                string baseKey = key;
                int duplicate = 1;
                while (reference.dic.ContainsKey(key)) key = baseKey + "#" + duplicate++;
                reference.dic.Add(key, new UIReferenceData { name = child.name, path = hierarchyPath, obj = child.gameObject });
            }
            EditorUtility.SetDirty(reference);
            repaired++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Rebuilt " + repaired + " UI reference assets from current prefab object IDs.");
    }

    private static string HierarchyPath(Transform child)
    {
        var names = new List<string>();
        for (Transform current = child; current != null; current = current.parent) names.Add(current.name);
        names.Reverse();
        return "/" + string.Join("/", names);
    }
}
