using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class MemoryIsolationUrpBuilder
{
    public static void Build()
    {
        if (!File.Exists(Path.Combine(Directory.GetParent(Application.dataPath).FullName, ".memory-isolation-project")))
            throw new InvalidOperationException("URP isolation setup requires the marked standalone project.");
        const string rendererPath = "Assets/MinimalUniversalRenderer.asset";
        const string pipelinePath = "Assets/MinimalUniversalPipeline.asset";
        var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
        if (renderer == null)
        {
            renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(renderer, rendererPath);
        }
        var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
        if (pipeline == null)
        {
            pipeline = UniversalRenderPipelineAsset.Create(renderer);
            AssetDatabase.CreateAsset(pipeline, pipelinePath);
        }
        GraphicsSettings.defaultRenderPipeline = pipeline;
        QualitySettings.renderPipeline = pipeline;
        AssetDatabase.SaveAssets();
        MemoryIsolationBuilder.Build();
    }
}
