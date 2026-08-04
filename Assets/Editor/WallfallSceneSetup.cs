using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Wallfall.EditorTools
{
    public static class WallfallSceneSetup
    {
        /// <summary>Pixel art must be point-filtered and uncompressed or it renders blurry. Run once.</summary>
        [MenuItem("WALLFALL/Fix itchio Import Settings")]
        public static void FixImportSettings()
        {
            int fixedCount = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/itchio" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                if (importer.filterMode == FilterMode.Point &&
                    importer.textureCompression == TextureImporterCompression.Uncompressed &&
                    !importer.mipmapEnabled) continue;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
                fixedCount++;
            }
            Debug.Log($"WALLFALL: fixed import settings on {fixedCount} textures (point filter, uncompressed).");
        }

        [MenuItem("WALLFALL/Setup Scene")]
        public static void SetupScene()
        {
            if (Object.FindFirstObjectByType<GameBootstrap>() != null)
            {
                Debug.Log("WALLFALL: scene already has a GameBootstrap.");
                return;
            }
            var go = new GameObject("WALLFALL");
            go.AddComponent<GameBootstrap>();
            Undo.RegisterCreatedObjectUndo(go, "WALLFALL Setup");
            EditorSceneManager.MarkSceneDirty(go.scene);
            Debug.Log("WALLFALL: bootstrap added. Press Play.");
        }
    }
}
