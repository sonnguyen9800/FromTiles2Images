using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.GridLayout;

namespace TileMap2Img
{
    public static class T2IUtils 
    {
        public static readonly Regex Whitespace = new Regex(@"\s+");
        public static string ReplaceWhitespace(string input, string replacement)
        {
            return Whitespace.Replace(input, replacement);
        }


        public static bool HandleTextureUnreadable(Texture2D texture2D)
        {
            if (null == texture2D) return false;
            string assetPath = AssetDatabase.GetAssetPath(texture2D);
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (tImporter != null)
            {
                if (tImporter.isReadable)
                {
                    return true;
                }
                else
                {
                    var choice = EditorUtility.DisplayDialog("Edit Assest file",
                        string.Format("The texture cannot be read. Do you want to set {0} to be readable/writable", texture2D.name),
                        "Ok", "Cancel");

                    if (choice)
                    {
                        tImporter.isReadable = true;

                        AssetDatabase.ImportAsset(assetPath);
                        AssetDatabase.Refresh();
                        return true;
                    }
                }
            }
            return false;
        }

        public static Texture2D GetCurrentSprite(Sprite sprite)
        {
            var texture = sprite.texture;
            var result = HandleTextureUnreadable(texture);

            if (!result)
            {
                return null;
            }

            var pixels = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                             (int)sprite.textureRect.y,
                                             (int)sprite.textureRect.width,
                                             (int)sprite.textureRect.height);

            Texture2D textura = new Texture2D((int)sprite.textureRect.width,
                                             (int)sprite.textureRect.height)
            {
                filterMode = FilterMode.Point
            };
            textura.SetPixels(pixels);
            textura.Apply();
            return textura;
        }

        public static Sprite GetFirstSprite(Tilemap tilemap)

        {
            foreach (var pos in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.HasTile(pos))
                {
                    return tilemap.GetSprite(pos);
                }
            }
            return null;
        }

        public static CellLayout GetGridLayout(Tilemap tilemaps)
        {
            Grid grid = tilemaps.gameObject.GetComponentInParent<Grid>();
            return grid.cellLayout;
        }
    }

}
