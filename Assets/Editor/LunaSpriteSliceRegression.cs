#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spine;
using Spine.Unity.Editor;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

/// <summary>Exercises the actual Spine sprite-slicing editor command on disposable assets.</summary>
public static class LunaSpriteSliceRegression
{
    public static void Run()
    {
        string folderName = "__LunaSpriteSliceRegression_" + System.Guid.NewGuid().ToString("N");
        string folder = "Assets/" + folderName;
        var diagnostics = new List<string>();
        AssetDatabase.CreateFolder("Assets", folderName);
        try
        {
            string path = folder + "/atlas.png";
            CreateTexture(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.mipmapEnabled = false;
            var provider = Provider(importer);
            var preserved = new SpriteRect
            {
                name = "preserved", spriteID = GUID.Generate(), rect = new Rect(0, 0, 4, 4),
                alignment = SpriteAlignment.Custom, pivot = new Vector2(0.25f, 0.75f),
                border = Vector4.one
            };
            var untouched = new SpriteRect
            {
                name = "untouched", spriteID = GUID.Generate(), rect = new Rect(8, 8, 4, 4),
                alignment = SpriteAlignment.Center, pivot = Vector2.one * 0.5f
            };
            provider.SetSpriteRects(new[] { preserved, untouched });
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(new[]
            {
                new SpriteNameFileIdPair(preserved.name, preserved.spriteID),
                new SpriteNameFileIdPair(untouched.name, untouched.spriteID)
            });
            provider.Apply();
            importer.SaveAndReimport();
            diagnostics.Add(Snapshot(path, "Authored slices before command"));
            long oldLocalId = LocalId(path, preserved.name);
            long untouchedLocalId = LocalId(path, untouched.name);

            var page = new AtlasPage { name = "atlas.png", width = 16, height = 16 };
            var atlas = new Atlas(new List<AtlasPage> { page }, new List<AtlasRegion>
            {
                new AtlasRegion { page = page, name = preserved.name, x = 1, y = 2, width = 6, height = 5 },
                new AtlasRegion { page = page, name = "added", x = 8, y = 4, width = 3, height = 6, degrees = 90 }
            });
            try
            {
                SpineAtlasAssetInspector.UpdateSpriteSlices(AssetDatabase.LoadAssetAtPath<Texture2D>(path), atlas);
                diagnostics.Add(Snapshot(path, "Authored slices after first command"));
                SpriteRect[] first = Provider((TextureImporter)AssetImporter.GetAtPath(path)).GetSpriteRects();
                Require(first.Length == 3, "Slice update lost or duplicated sprites.");
                var updated = first.Single(s => s.name == preserved.name);
                var added = first.Single(s => s.name == "added");
                Require(updated.spriteID == preserved.spriteID && updated.rect == new Rect(1, 9, 6, 5),
                    "Updating an existing slice must retain its GUID and convert atlas coordinates.");
                Require(updated.pivot == preserved.pivot && updated.border == preserved.border
                    && updated.alignment == preserved.alignment, "Existing pivot/border/alignment changed.");
                Require(LocalId(path, preserved.name) == oldLocalId, "Existing Sprite local fileID changed.");
                Require(LocalId(path, untouched.name) == untouchedLocalId
                    && first.Single(s => s.name == untouched.name).rect == untouched.rect,
                    "An unrelated sprite was changed.");
                Require(added.rect == new Rect(8, 9, 6, 3), "Rotated atlas slice dimensions were not swapped.");
                long addedLocalId = LocalId(path, added.name);

                SpineAtlasAssetInspector.UpdateSpriteSlices(AssetDatabase.LoadAssetAtPath<Texture2D>(path), atlas);
                diagnostics.Add(Snapshot(path, "Authored slices after repeated command"));
                var secondProvider = Provider((TextureImporter)AssetImporter.GetAtPath(path));
                SpriteRect[] second = secondProvider.GetSpriteRects();
                Require(second.Length == 3 && second.Single(s => s.name == added.name).spriteID == added.spriteID,
                    "Repeated slicing created a new identity or a duplicate.");
                Require(LocalId(path, preserved.name) == oldLocalId && LocalId(path, added.name) == addedLocalId,
                    "Repeated slicing changed Sprite local fileIDs.");
                var pairs = secondProvider.GetDataProvider<ISpriteNameFileIdDataProvider>().GetNameFileIdPairs().ToArray();
                Require(pairs.Length == 3 && pairs.Select(p => p.name).Distinct().Count() == 3,
                    "Repeated slicing duplicated name/file-ID mappings.");
            }
            finally { atlas.Dispose(); }

            // A regular atlas texture must also become a usable multiple-sprite asset.
            string plainPath = folder + "/plain.png";
            CreateTexture(plainPath);
            diagnostics.Add(Snapshot(plainPath, "New texture project defaults"));
            var plainImporter = (TextureImporter)AssetImporter.GetAtPath(plainPath);
            plainImporter.textureType = TextureImporterType.Default;
            plainImporter.spriteImportMode = SpriteImportMode.None;
            plainImporter.SaveAndReimport();
            plainImporter = (TextureImporter)AssetImporter.GetAtPath(plainPath);
            diagnostics.Add(Snapshot(plainPath, "Explicit plain texture before command"));
            Require(plainImporter.textureType == TextureImporterType.Default
                && plainImporter.spriteImportMode == SpriteImportMode.None,
                "Plain-texture test precondition was not retained by the importer.");
            var plainPage = new AtlasPage { name = "plain.png", width = 16, height = 16 };
            var plainAtlas = new Atlas(new List<AtlasPage> { plainPage }, new List<AtlasRegion>
            {
                new AtlasRegion { page = plainPage, name = "plain-slice", x = 0, y = 0, width = 8, height = 8 }
            });
            try { SpineAtlasAssetInspector.UpdateSpriteSlices(AssetDatabase.LoadAssetAtPath<Texture2D>(plainPath), plainAtlas); }
            finally { plainAtlas.Dispose(); }
            plainImporter = (TextureImporter)AssetImporter.GetAtPath(plainPath);
            diagnostics.Add(Snapshot(plainPath, "Plain texture after command"));
            var plainRects = Provider(plainImporter).GetSpriteRects();
            Require(plainImporter.textureType == TextureImporterType.Sprite
                && plainImporter.spriteImportMode == SpriteImportMode.Multiple
                && plainRects.Length == 1,
                "A regular atlas texture was not converted into sprite slices: type=" + plainImporter.textureType
                + ", mode=" + plainImporter.spriteImportMode + ", slices=" + plainRects.Length + ".");
            LocalId(plainPath, "plain-slice");
            WriteEvidence(folder, diagnostics, false);
        }
        catch (Exception exception)
        {
            diagnostics.Add(exception.ToString());
            WriteEvidence(folder, diagnostics, true);
            throw;
        }
        finally
        {
            // The randomly named folder is created by this test; no authored assets are touched.
            AssetDatabase.DeleteAsset(folder);
        }
    }

    private static string Snapshot(string path, string label)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        var provider = Provider(importer);
        string rects = string.Join("; ", provider.GetSpriteRects().Select(rect =>
            rect.name + " [" + rect.spriteID + "] " + rect.rect));
        return label + ": type=" + importer.textureType + ", mode=" + importer.spriteImportMode
            + ", providerMode=" + provider.spriteImportMode + ", rects={" + rects + "}";
    }

    private static void WriteEvidence(string folder, List<string> diagnostics, bool failed)
    {
        string directory = Path.GetFullPath("Logs/Revival");
        Directory.CreateDirectory(directory);
        var output = new List<string>(diagnostics);
        if (failed)
            foreach (string metadata in Directory.GetFiles(folder, "*.meta"))
                output.Add(metadata + Environment.NewLine + File.ReadAllText(metadata));
        string file = Path.Combine(directory, failed
            ? "sprite-slice-failure-" + Path.GetFileName(folder) + ".txt"
            : "sprite-slice-regression.txt");
        File.WriteAllText(file, string.Join(Environment.NewLine + Environment.NewLine, output));
        Debug.Log("Spine sprite slice diagnostics: " + file);
    }

    private static ISpriteEditorDataProvider Provider(TextureImporter importer)
    {
        var factories = new SpriteDataProviderFactories();
        factories.Init();
        var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
        Require(provider != null, "Sprite data provider is unavailable.");
        provider.InitSpriteEditorDataProvider();
        return provider;
    }

    private static long LocalId(string path, string name)
    {
        var sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Single(s => s.name == name);
        Require(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite, out string _, out long localId)
            && localId != 0, "Imported Sprite has no local fileID.");
        return localId;
    }

    private static void CreateTexture(string path)
    {
        var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        try
        {
            texture.SetPixels(Enumerable.Repeat(Color.white, 256).ToArray());
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
        }
        finally { UnityEngine.Object.DestroyImmediate(texture); }
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Spine sprite slicing regression: " + message);
    }
}
#endif
