using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class MemoryIsolationBuilder
{
    [Serializable]
    private sealed class Report
    {
        public string unityVersion, startedUtc, finishedUtc, scene, pipeline, materialShader, graphicsDevice, player, buildResult;
        public string[] commandLine;
        public bool completed;
        public int buildErrors, buildWarnings;
        public int idleUpdates;
        public double drainSeconds;
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
    }

    private static Report report;
    private static string outputDirectory;
    private static double drainStarted;

    public static void Build()
    {
        if (!File.Exists(Path.Combine(Directory.GetParent(Application.dataPath).FullName, ".memory-isolation-project")))
            throw new InvalidOperationException("This method only runs inside its marked standalone project.");
        outputDirectory = Environment.GetEnvironmentVariable("MEMORY_ISOLATION_DIRECTORY");
        if (string.IsNullOrWhiteSpace(outputDirectory) || !Path.IsPathRooted(outputDirectory))
            throw new InvalidOperationException("An absolute isolated evidence directory is required.");
        Directory.CreateDirectory(outputDirectory);
        report = new Report
        {
            unityVersion = Application.unityVersion,
            startedUtc = DateTime.UtcNow.ToString("o"),
            scene = Environment.GetEnvironmentVariable("MEMORY_ISOLATION_SCENE") ?? "empty",
            pipeline = GraphicsSettings.defaultRenderPipeline != null ? GraphicsSettings.defaultRenderPipeline.GetType().FullName : "builtin",
            graphicsDevice = SystemInfo.graphicsDeviceType.ToString(),
            commandLine = Environment.GetCommandLineArgs(),
            player = Environment.GetEnvironmentVariable("MEMORY_ISOLATION_PLAYER")
        };
        Application.logMessageReceived += CaptureLog;
        NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;
        try
        {
            if (string.IsNullOrWhiteSpace(report.player) || !Path.IsPathRooted(report.player))
                throw new InvalidOperationException("An absolute isolated player output is required.");
            if (report.scene != "empty" && report.scene != "builtin-shader")
                throw new InvalidOperationException("Unknown minimal scene variant.");
            PlayerSettings.companyName = "MemoryIsolation";
            PlayerSettings.productName = "MemoryIsolation";
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { GraphicsDeviceType.Direct3D11 });
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (report.scene == "builtin-shader")
            {
                var camera = new GameObject("Minimal camera").AddComponent<Camera>();
                camera.transform.position = new Vector3(0f, 0f, -5f);
                camera.gameObject.AddComponent<AudioListener>();
                // Keep the graphics contrast independent of Physics/Collider modules.
                // The serialized mesh is an ordinary authored asset, not a runtime leak fixture.
                const string meshPath = "Assets/MinimalMesh.asset";
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                if (mesh == null)
                {
                    mesh = new Mesh { name = "Minimal quad" };
                    mesh.vertices = new[] { new Vector3(-1f, -1f, 0f), new Vector3(-1f, 1f, 0f), new Vector3(1f, 1f, 0f), new Vector3(1f, -1f, 0f) };
                    mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
                    mesh.uv = new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right };
                    mesh.RecalculateNormals();
                    mesh.RecalculateBounds();
                    AssetDatabase.CreateAsset(mesh, meshPath);
                }
                var quad = new GameObject("Minimal quad");
                // Spine sprite shaders consume the mesh vertex color. Keep the same
                // valid white input when reusing the quad from the URP-only contrast.
                mesh.colors = new[] { Color.white, Color.white, Color.white, Color.white };
                EditorUtility.SetDirty(mesh);
                quad.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = quad.AddComponent<MeshRenderer>();
                string shaderName = Environment.GetEnvironmentVariable("MEMORY_ISOLATION_SHADER");
                if (string.IsNullOrWhiteSpace(shaderName))
                    shaderName = Environment.GetEnvironmentVariable("MEMORY_ISOLATION_PIPELINE") == "urp" ? "Universal Render Pipeline/Unlit" : "Unlit/Color";
                var shader = Shader.Find(shaderName);
                if (shader == null) throw new InvalidOperationException("Minimal shader is missing: " + shaderName);
                const string materialPath = "Assets/Minimal.mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null)
                {
                    material = new Material(shader);
                    AssetDatabase.CreateAsset(material, materialPath);
                }
                else material.shader = shader;
                material.color = Color.cyan;
                EditorUtility.SetDirty(material);
                report.materialShader = material.shader.name;
                renderer.sharedMaterial = material;
            }
            const string scenePath = "Assets/Minimal.unity";
            if (!EditorSceneManager.SaveScene(scene, scenePath)) throw new IOException("Could not save minimal scene.");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory(Path.GetDirectoryName(report.player));
            var built = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { scenePath }, locationPathName = report.player,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.CleanBuildCache
            });
            report.buildResult = built.summary.result.ToString();
            report.buildErrors = built.summary.totalErrors;
            report.buildWarnings = built.summary.totalWarnings;
            if (built.summary.result != BuildResult.Succeeded) report.errors.Add("Minimal player build failed.");
        }
        catch (Exception error) { report.errors.Add(error.ToString()); }
        drainStarted = EditorApplication.timeSinceStartup;
        EditorApplication.update += Drain;
    }

    private static void CaptureLog(string message, string stack, LogType type)
    {
        if (type == LogType.Warning) report.warnings.Add(message);
        else if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message + "\n" + stack);
    }

    private static void Drain()
    {
        report.idleUpdates = ShaderUtil.anythingCompiling || EditorApplication.isCompiling || EditorApplication.isUpdating ? 0 : report.idleUpdates + 1;
        report.drainSeconds = EditorApplication.timeSinceStartup - drainStarted;
        if (report.idleUpdates < 16 && report.drainSeconds < 60d) return;
        if (report.idleUpdates < 16) report.errors.Add("Editor did not reach 16 idle updates in 60 seconds.");
        EditorApplication.update -= Drain;
        report.completed = report.buildResult == "Succeeded" && report.errors.Count == 0;
        report.finishedUtc = DateTime.UtcNow.ToString("o");
        File.WriteAllText(Path.Combine(outputDirectory, "operation.json"), JsonUtility.ToJson(report, true));
        Debug.Log("Minimal build finished; raw process shutdown diagnostics remain authoritative.");
        EditorApplication.Exit(report.completed ? 0 : 1);
    }
}
