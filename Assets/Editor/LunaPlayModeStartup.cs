using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>Always initialize services/data through Loading when pressing Play in any scene.</summary>
[InitializeOnLoad]
public static class LunaPlayModeStartup
{
    static LunaPlayModeStartup()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorSceneManager.playModeStartScene =
                AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Loading.unity");
        };
    }
}
