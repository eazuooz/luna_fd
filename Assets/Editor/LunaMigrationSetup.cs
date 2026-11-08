using System;
using System.Collections.Generic;
using System.IO;
using Luna.Gameplay;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

/// <summary>Registers the game's authored assets with the supplied native services. Does not create a legacy runtime.</summary>
public static class LunaMigrationSetup
{
    public const string LoadingScene = "Assets/Scenes/Loading.unity";
    public static readonly string[] NativeScenes = { "Assets/Scenes/Lobby.unity", "Assets/Scenes/Game.unity", "Assets/Scenes/Dungeon.unity" };
    // Original asset identities, retained through prefab migration. Account/PVP intentionally share the existing popup.
    public static readonly Dictionary<UI_TYPE, string> FormGuids = new Dictionary<UI_TYPE, string>
    {
        { (UI_TYPE)1, "ce66bb3eb2201b64c8048832a273e361" },
        { (UI_TYPE)2, "ebcd36fc4adc9d444807b144a55dc227" },
        { (UI_TYPE)3, "b381c751633cd66428c1776a69ea24b6" },
        { (UI_TYPE)4, "31bdcd513348ae341a894582b608acb9" },
        { (UI_TYPE)5, "72a6950a69e93604a9f6a571e7b11202" },
        { (UI_TYPE)6, "d6931dbcc18dd6b4db3c42a354e8432b" },
        { (UI_TYPE)7, "06ba4b203dc79e240bb927575075607f" },
        { (UI_TYPE)8, "c2abbf85de28bb24a8959aff6ad698f5" },
        { (UI_TYPE)9, "b429fa79082bcab4da8eab67c21c91db" },
        { (UI_TYPE)10, "5b63149d460f1e54abbb2829744f2b65" },
        { (UI_TYPE)11, "a55862ad5101aaa48a1a264bc25a98d1" },
        { (UI_TYPE)12, "744f7eed8afe11b4da2a183d383c25c2" },
        { (UI_TYPE)13, "5ce61c6b81341a4438b42c5d32921599" },
        { (UI_TYPE)14, "1569356f86da5584a8cac4842a0a48eb" },
        { (UI_TYPE)15, "a55862ad5101aaa48a1a264bc25a98d1" },
        { (UI_TYPE)18, "a55862ad5101aaa48a1a264bc25a98d1" },
        { (UI_TYPE)19, "357434116061de14fb6883371db5e9c6" },
        { (UI_TYPE)20, "cc361b48a3047f146921a59da439e0f4" },
        { (UI_TYPE)21, "dda52cb72c1883e488c3fe70632a8901" },
        { (UI_TYPE)22, "36ef3daaed4cb2c4787b72620fc98fbd" },
        { (UI_TYPE)23, "806b65e3103382841adf8e41f9f90f94" },
        { (UI_TYPE)24, "db13c73b8ebbdf14a99688d3bce4890b" },
        { (UI_TYPE)25, "053453209ed8eeb4b93e63f503a3cd65" },
        { (UI_TYPE)26, "f6a2a5223489404498ce7e2458cccee2" },
        { (UI_TYPE)27, "958004852460e1240bb78185d84f6123" },
        { (UI_TYPE)28, "42a24763407b10a4d9d2829f26730ffe" },
    };

    [MenuItem("Luna/Prepare Native Framework Assets")]
    public static void Prepare()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null || settings.DefaultGroup == null) throw new InvalidDataException("Addressables settings/default group are missing.");
        LunaUI.EnsureDefinitions();
        foreach (var definition in LunaUI.Definitions)
        {
            if (!FormGuids.TryGetValue(definition.Type, out string guid)) throw new InvalidDataException("No UI prefab identity for " + definition.Type);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            if (prefab == null || prefab.GetComponent<UILuna>() == null) throw new InvalidDataException("Native UI prefab is missing: " + definition.Type);
            if (definition.Type == UI_TYPE.ACCOUNTSHOP || definition.Type == UI_TYPE.PVP)
            {
                var entry = Register(settings, guid, "Luna/UI/POPUP");
                settings.AddLabel(definition.AssetName, false);
                entry.SetLabel(definition.AssetName, true, false, false);
            }
            else Register(settings, guid, definition.AssetName);
        }
        foreach (string path in NativeScenes)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) throw new InvalidDataException("Native scene is missing: " + path);
            Register(settings, AssetDatabase.AssetPathToGUID(path), path);
        }
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(LoadingScene) == null) throw new InvalidDataException("Loading scene is missing.");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(LoadingScene, true) };

        const string originalMusic = "Assets/Resources/Sounds/Escape from hell (extract).mp3";
        const string music = "Assets/Audio/LunaBattle.mp3";
        if (AssetDatabase.LoadAssetAtPath<AudioClip>(originalMusic) != null)
        {
            if (AssetDatabase.LoadAssetAtPath<AudioClip>(music) != null) throw new InvalidDataException("Both source and migrated battle tracks exist.");
            if (!AssetDatabase.IsValidFolder("Assets/Audio")) AssetDatabase.CreateFolder("Assets", "Audio");
            string error = AssetDatabase.MoveAsset(originalMusic, music);
            if (!string.IsNullOrEmpty(error)) throw new InvalidDataException(error);
        }
        if (AssetDatabase.LoadAssetAtPath<AudioClip>(music) == null) throw new InvalidDataException("Battle music is missing.");
        Register(settings, AssetDatabase.AssetPathToGUID(music), "Luna/Audio/Battle");

        var catalog = AssetDatabase.LoadAssetAtPath<LunaEntityCatalog>("Assets/Resources/Luna/EntityCatalog.asset");
        if (catalog == null) throw new InvalidDataException("Native entity catalog has not been generated.");
        foreach (var entity in catalog.Entries)
        {
            string path = AssetDatabase.GetAssetPath(entity.Prefab);
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(entity.Address)) throw new InvalidDataException("Empty native entity catalog binding.");
            if (entity.Address != path) throw new InvalidDataException("Entity catalog must use its exact Resources asset path: " + path);
            LunaResourcesProvider.ResourcePath(entity.Address);
            // Resources prefabs load through the game Addressables provider, not duplicate bundles.
            var bundled = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(path));
            if (bundled != null) settings.RemoveAssetEntry(bundled.guid, false);
        }
        ConfigureGroupSchemas(settings);
        EditorUtility.SetDirty(settings); AssetDatabase.SaveAssets();
        Debug.Log("Native Luna assets configured: 26 UI keys, 3 native scenes, " + catalog.Entries.Length + " entity bindings, Loading-only build entry.");
    }
    private static AddressableAssetEntry Register(AddressableAssetSettings settings, string guid, string address)
    {
        var entry = settings.FindAssetEntry(guid) ?? settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
        if (entry == null) throw new InvalidDataException("Could not register address " + address);
        entry.address = address; EditorUtility.SetDirty(entry.parentGroup); return entry;
    }
    private static void ConfigureGroupSchemas(AddressableAssetSettings settings)
    {
        foreach (var group in settings.groups)
        {
            if (group == null) continue;
            var schema = group.GetSchema<BundledAssetGroupSchema>();
            if (schema == null) continue;
            if (string.IsNullOrEmpty(schema.BuildPath.Id)) schema.BuildPath.SetVariableByName(settings, "LocalBuildPath");
            if (string.IsNullOrEmpty(schema.LoadPath.Id)) schema.LoadPath.SetVariableByName(settings, "LocalLoadPath");
            var serialized = new SerializedObject(schema);
            var owner = serialized.FindProperty("m_Group");
            if (owner == null) throw new InvalidDataException("Installed Addressables schema no longer exposes its group binding.");
            if (owner.objectReferenceValue != group) { owner.objectReferenceValue = group; serialized.ApplyModifiedPropertiesWithoutUndo(); }
            EditorUtility.SetDirty(schema);
        }
    }
    public static void PrepareAndAudit() { Prepare(); LunaValidation.Audit(); }
    public static void PrepareBuild()
    {
        Prepare();
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.BuildWithPlayer;
        // Content update state is a build artifact, not authored Unity source. Preserve
        // the historical files in Assets and keep final-review source fingerprints stable.
        settings.ContentStateBuildPath = "Builds/Revival/AddressablesState/" + EditorUserBuildSettings.activeBuildTarget;
        EditorUtility.SetDirty(settings); AssetDatabase.SaveAssets();
    }
}
