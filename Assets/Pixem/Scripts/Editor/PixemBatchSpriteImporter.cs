using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public static class PixemBatchSpriteImporter
{
    private static int width = 32;
    private static int height = 32;
    private static Vector2 pivot = new Vector2(0.5f, 0.5f);

    [MenuItem("Tools/Batch Sprite Importer")]
    public static void ProcessTextures()
    {
        Texture2D[] textures = Resources.LoadAll<Texture2D>("Items");

        foreach (Texture2D texture in textures)
        {
            string assetPath = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (importer == null)
                continue;

            importer.spritePixelsPerUnit = 32;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);

            int spriteWidth = assetPath.Contains("LeftHandWeapon") ? width * 2 : width;
            int spriteHeight = assetPath.Contains("LeftHandWeapon") ? height * 2 : height;

            int rows = texture.height / spriteHeight;
            int columns = texture.width / spriteWidth;

            List<SpriteMetaData> metas = importer.spritesheet?.ToList() ?? new List<SpriteMetaData>();

            HashSet<string> existingNames = new HashSet<string>(metas.Select(m => m.name));

            int index = 0;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    string name = $"{texture.name}_{index++}";

                    if (existingNames.Contains(name))
                        continue;

                    SpriteMetaData meta = new SpriteMetaData
                    {
                        rect = new Rect(
                            x * spriteWidth,
                            texture.height - ((y + 1) * spriteHeight),
                            spriteWidth,
                            spriteHeight
                        ),
                        name = name,
                        pivot = pivot,
                        alignment = (int)SpriteAlignment.Center
                    };
                    metas.Add(meta);
                }
            }

            importer.spritesheet = metas.ToArray();
            importer.SaveAndReimport();
        }

        AssetDatabase.Refresh();
        Debug.Log("All sprites have been sliced successfully!");
    }
}
