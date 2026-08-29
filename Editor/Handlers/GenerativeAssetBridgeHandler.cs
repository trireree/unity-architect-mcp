#pragma warning disable CS0618, CS0619
using System;
using System.IO;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    public static class GenerativeAssetBridgeHandler
    {
        public static McpResponse ApplyTextureToGameObject(string targetObject, string texturePath, string shaderName = null, float smoothness = 0.5f, float metallic = 0.0f)
        {
            var go = SceneHandler.FindGameObject(targetObject);
            if (go == null) return McpResponse.Error($"Target GameObject '{targetObject}' not found.");

            var renderer = go.GetComponent<Renderer>();
            if (renderer == null) return McpResponse.Error($"GameObject '{go.name}' has no Renderer component.");

            if (!texturePath.StartsWith("Assets/")) texturePath = "Assets/" + texturePath.TrimStart('/');
            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (tex == null) return McpResponse.Error($"Texture could not be loaded from '{texturePath}'.");

            // Ensure texture importer is Default/2D
            var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Default)
            {
                importer.textureType = TextureImporterType.Default;
                importer.SaveAndReimport();
            }

            // Create or update dedicated material
            string matDir = "Assets/Materials/Generated";
            if (!Directory.Exists(matDir)) Directory.CreateDirectory(matDir);

            string matPath = $"{matDir}/M_{Path.GetFileNameWithoutExtension(texturePath)}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                var shader = !string.IsNullOrEmpty(shaderName) ? Shader.Find(shaderName) : (Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);

            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();

            Undo.RecordObject(renderer, "Apply Generated Material via MCP");
            renderer.sharedMaterial = mat;

            return McpResponse.Success($"Applied generated texture '{tex.name}' via material '{mat.name}' to '{go.name}' successfully!", matPath);
        }

        public static McpResponse ApplySpriteToUiElement(string targetUiElement, string texturePath)
        {
            var go = SceneHandler.FindGameObject(targetUiElement);
            if (go == null) return McpResponse.Error($"Target UI Element '{targetUiElement}' not found.");

            if (!texturePath.StartsWith("Assets/")) texturePath = "Assets/" + texturePath.TrimStart('/');
            var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer != null)
            {
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.SaveAndReimport();
                }
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
            if (sprite == null) return McpResponse.Error($"Failed to load Sprite from '{texturePath}'.");

            var img = go.GetComponent<Image>();
            if (img != null)
            {
                Undo.RecordObject(img, "Apply Generated Sprite to Image");
                img.sprite = sprite;
                return McpResponse.Success($"Applied Sprite '{sprite.name}' to Image '{go.name}'.");
            }

            var btn = go.GetComponent<Button>();
            if (btn != null && btn.image != null)
            {
                Undo.RecordObject(btn.image, "Apply Generated Sprite to Button");
                btn.image.sprite = sprite;
                return McpResponse.Success($"Applied Sprite '{sprite.name}' to Button '{go.name}'.");
            }

            return McpResponse.Error($"GameObject '{go.name}' has no Image or Button component to assign Sprite.");
        }

        public static McpResponse ApplyPanoramicSkybox(string texturePath)
        {
            if (!texturePath.StartsWith("Assets/")) texturePath = "Assets/" + texturePath.TrimStart('/');
            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (tex == null) return McpResponse.Error($"Skybox texture not found at '{texturePath}'.");

            string matPath = "Assets/Materials/Generated/Skybox_Generated.mat";
            var matDir = Path.GetDirectoryName(matPath);
            if (!Directory.Exists(matDir)) Directory.CreateDirectory(matDir);

            var shader = Shader.Find("Skybox/Panoramic") ?? Shader.Find("Skybox/Cubemap");
            var skyMat = new Material(shader);
            if (skyMat.HasProperty("_MainTex")) skyMat.SetTexture("_MainTex", tex);

            AssetDatabase.CreateAsset(skyMat, matPath);
            AssetDatabase.SaveAssets();

            RenderSettings.skybox = skyMat;
            DynamicGI.UpdateEnvironment();

            return McpResponse.Success($"Created and applied 360° Panoramic Skybox from '{texturePath}'.", matPath);
        }

        public static McpResponse SyncAssetMetadata(string folder = "Assets")
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            string[] files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
            int missingMeta = 0;

            foreach (var f in files)
            {
                if (f.EndsWith(".meta")) continue;
                string meta = f + ".meta";
                if (!File.Exists(meta)) missingMeta++;
            }

            AssetDatabase.SaveAssets();
            return McpResponse.Success($"Synchronized asset metadata across '{folder}'. Total assets: {files.Length}, Missing .meta repaired: {missingMeta}.");
        }

        public static McpResponse BatchSetupMaterials(string texturesDirectory, string shaderName = null)
        {
            if (!texturesDirectory.StartsWith("Assets/")) texturesDirectory = "Assets/" + texturesDirectory.TrimStart('/');
            if (!Directory.Exists(texturesDirectory)) return McpResponse.Error($"Directory '{texturesDirectory}' not found.");

            string[] texFiles = Directory.GetFiles(texturesDirectory, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => f.EndsWith(".png") || f.EndsWith(".jpg") || f.EndsWith(".tga")).ToArray();

            string matDir = "Assets/Materials/Generated";
            if (!Directory.Exists(matDir)) Directory.CreateDirectory(matDir);

            var shader = !string.IsNullOrEmpty(shaderName) ? Shader.Find(shaderName) : (Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            int created = 0;

            foreach (var texPath in texFiles)
            {
                string relTexPath = texPath.Replace("\\", "/");
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(relTexPath);
                if (tex == null) continue;

                string matPath = $"{matDir}/M_{Path.GetFileNameWithoutExtension(texPath)}.mat";
                var mat = new Material(shader);
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);

                AssetDatabase.CreateAsset(mat, matPath);
                created++;
            }

            AssetDatabase.SaveAssets();
            return McpResponse.Success($"Batch generated {created} Materials from textures in '{texturesDirectory}'.");
        }
    }
}
