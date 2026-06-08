using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Magebase.Spritesynth.Editor
{
    public static class SpritesynthImporter
    {
        private const string RootFolder = "Assets/Spritesynth";
        private const string GenerationsFolder = "Assets/Spritesynth/Generations";

        public static string Import(byte[] pngData, string prompt, GenerationResult result)
        {
            if (pngData == null || pngData.Length == 0)
                throw new ArgumentException("PNG data is empty", nameof(pngData));

            EnsureDirectories();

            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss");
            string sanitizedPrompt = SanitizeFileName(prompt);
            if (string.IsNullOrEmpty(sanitizedPrompt))
                sanitizedPrompt = "untitled";

            string folderPath = GenerationsFolder + "/" + timestamp;
            string fullFolder = Path.Combine(Application.dataPath, "..", folderPath);
            Directory.CreateDirectory(fullFolder);

            string fileName = sanitizedPrompt + ".png";
            string assetPath = folderPath + "/" + fileName;
            string fullPath = Path.Combine(Application.dataPath, "..", assetPath);

            File.WriteAllBytes(fullPath, pngData);

            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.crunchedCompression = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.mipmapEnabled = false;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.SaveAndReimport();
            }

            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }

            EditorUtility.DisplayDialog(
                "SpriteSynth",
                $"Generation complete!\n\nPrompt: {prompt}\nSize: {result.asset.width}x{result.asset.height}\nCredits: {result.credits_cost}\nTime: {result.duration_ms}ms\n\nImported to:\n{assetPath}",
                "OK"
            );

            return assetPath;
        }

        private static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder(RootFolder))
                AssetDatabase.CreateFolder("Assets", "Spritesynth");

            if (!AssetDatabase.IsValidFolder(GenerationsFolder))
                AssetDatabase.CreateFolder(RootFolder, "Generations");
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "untitled";

            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');

            name = name.Replace(' ', '_');

            if (name.Length > 80)
                name = name.Substring(0, 80);

            name = name.Trim('.');
            if (string.IsNullOrEmpty(name))
                name = "untitled";

            return name;
        }
    }
}
