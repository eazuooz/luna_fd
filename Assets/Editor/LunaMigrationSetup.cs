using System;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

/// <summary>Idempotent project asset migration for the Game Framework services.</summary>
public static class LunaMigrationSetup
{
    [MenuItem("Luna/Prepare Game Framework Assets")]
    public static void Prepare()
    {
        const string source = "Assets/Resources/Sounds/Escape from hell (extract).mp3";
        const string destination = "Assets/Audio/LunaBattle.mp3";
        if (!AssetDatabase.IsValidFolder("Assets/Audio")) AssetDatabase.CreateFolder("Assets", "Audio");
        if (AssetDatabase.LoadAssetAtPath<AudioClip>(source) != null)
        {
            if (AssetDatabase.LoadAssetAtPath<AudioClip>(destination) != null)
                throw new InvalidOperationException("Both original and migrated battle music exist; preserve and inspect them before continuing.");
            string error = AssetDatabase.MoveAsset(source, destination);
            if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
        }
        if (AssetDatabase.LoadAssetAtPath<AudioClip>(destination) == null)
            throw new InvalidOperationException("Battle music asset is missing.");
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null || settings.DefaultGroup == null)
            throw new InvalidOperationException("Addressables settings/default group are missing.");
        var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(destination), settings.DefaultGroup);
        entry.address = "Luna/Audio/Battle";
        EditorUtility.SetDirty(settings);
        EditorUtility.SetDirty(settings.DefaultGroup);
        AssetDatabase.SaveAssets();
        Debug.Log("Luna Game Framework assets prepared.");
    }

    public static void PrepareAndAudit()
    {
        Prepare();
        LunaValidation.Audit();
    }

    public static void PrepareBuild()
    {
        Prepare();
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.BuildWithPlayer;
        foreach (var group in settings.groups)
        {
            if (group == null) continue;
            var schema = group.GetSchema<BundledAssetGroupSchema>();
            if (schema == null) continue;
            // The legacy Addressables project kept profile variables but lost schema bindings.
            if (string.IsNullOrEmpty(schema.BuildPath.Id)) schema.BuildPath.SetVariableByName(settings, "LocalBuildPath");
            if (string.IsNullOrEmpty(schema.LoadPath.Id)) schema.LoadPath.SetVariableByName(settings, "LocalLoadPath");
            var serialized = new SerializedObject(schema);
            serialized.FindProperty("m_Group").objectReferenceValue = group;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(schema);
        }
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }
}
