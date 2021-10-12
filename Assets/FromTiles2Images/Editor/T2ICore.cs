using System.Collections;
using System.Collections.Generic;
using System.IO;
using TileMap2Img.Define;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.GridLayout;

namespace TileMap2Img
{
    public static class T2ICore
    {
        public static void Process(TileMapParam param, string fileName)
        {
            Texture2D _texture = null;
            switch (param.Layout)
            {
                case CellLayout.Rectangle:
                    ProcesTilemapRectangle(param.Tilemap);
                    break;

                case CellLayout.Hexagon:
                    EditorUtility.DisplayDialog(LabelText.FeatureUnsupported, string.Format("Cannot export layout {0}", param.Layout.ToString()), LabelText.Cancel);
                    break;

                case CellLayout.Isometric:
                    EditorUtility.DisplayDialog(LabelText.FeatureUnsupported, string.Format("Cannot export layout {0}", param.Layout.ToString()), LabelText.Cancel);
                    break;

                case CellLayout.IsometricZAsY:
                    EditorUtility.DisplayDialog(LabelText.FeatureUnsupported, string.Format("Cannot export layout {0}", param.Layout.ToString()), LabelText.Cancel);
                    break;
                default:
                    EditorUtility.DisplayDialog(LabelText.FeatureUnsupported, string.Format("Cannot export layout {0}", param.Layout.ToString()), LabelText.Cancel);
                    break;
            }


            if (_texture == null)
            {
                EditorUtility.DisplayDialog(LabelText.Yes, LabelText.CannotConvert, LabelText.Cancel);
                return;
            }

            string path = EditorUtility.SaveFolderPanel(LabelText.ImageSave, "", fileName);
            if (path.Length != 0)
            {
                byte[] byteCodedata;
                if (param.ImageFormat == ImageFormat.PNG)
                {
                    byteCodedata = _texture.EncodeToPNG();
                }
                else if (param.ImageFormat == ImageFormat.JPG)
                {
                    byteCodedata = _texture.EncodeToJPG();
                }
                else if (param.ImageFormat == ImageFormat.EXR)
                {
                    byteCodedata = _texture.EncodeToEXR();
                }
                else if (param.ImageFormat == ImageFormat.TGA)
                {
                    byteCodedata = _texture.EncodeToTGA();
                }
                else
                {
                    byteCodedata = _texture.EncodeToPNG();
                }

                if (byteCodedata != null)
                {
                    File.WriteAllBytes(path + "/" + fileName + "." + param.ToString().ToLower(), byteCodedata);
                    EditorUtility.DisplayDialog(LabelText.SuccessConvert, string.Format("The file {0}.{1} has been exported", fileName, param.ToString()), "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog(LabelText.CannotConvert, LabelText.ProblemLog, LabelText.Cancel);
                }

                // Just in case we are saving to the asset folder, tell Unity to scan for modified or new assets
                AssetDatabase.Refresh();
            }
        }

        static Texture2D ProcesTilemapRectangle(Tilemap tilemap)
        {
            int _minX, _maxX, _minY, _maxY;

            // Get Basic Info
            var bounds = tilemap.cellBounds.size;
            int rowNumber = bounds.y;
            int colNumber = bounds.x;


            // Get Sprite
            Sprite firstSprite = T2IUtils.GetFirstSprite(tilemap);

            if (firstSprite == null) Debug.Log(LabelText.NoSpriteFound);

            float spriteWith = firstSprite.rect.width;
            float spriteHeight = firstSprite.rect.height;

            // Main Loop

            int offset_y = 0;
            int offset_x = 0;

            // Bounder points:
            _minX = _minY = int.MaxValue;
            _maxX = _maxY = int.MinValue;

            foreach (var pos in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.HasTile(pos))
                {
                    //Check if the point can be new Bottom Left
                    if (_minX > offset_x) _minX = offset_x;

                    if (_maxX < offset_x) _maxX = offset_x;

                    if (_minY > offset_y) _minY = offset_y;

                    if (_maxY < offset_y) _maxY = offset_y;
                }

                offset_x++;
                if (offset_x > colNumber - 1)
                {
                    offset_y++;
                    offset_x = 0;
                }
            }
            RectangleTilemapData rectangleTilemapData = new RectangleTilemapData
            {
                BottomLeft = new Vector2Int(_minX, _minY),
                BottomRight = new Vector2Int(_maxX, _minY),
                TopLeft = new Vector2Int(_minX, _maxY),
                TopRight = new Vector2Int(_maxX, _maxY)
            };

            // Prepare before crop:
            Vector2Int beginPoint = rectangleTilemapData.BottomLeft;

            Vector2Int areaCrop = new Vector2Int();
            areaCrop.x = (_maxX - _minX + 1) * (int)spriteWith;
            areaCrop.y = (_maxY - _minY + 1) * (int)spriteHeight;

            // Loop to cut:
            Texture2D finalTexture = new Texture2D(areaCrop.x, areaCrop.y);

            // Initialize
            for (int i = 0; i < finalTexture.width; i++)
            {
                for (int j = 0; j < finalTexture.height; j++)
                {
                    finalTexture.SetPixel(i, j, new Color(0, 0, 0, 0));
                }
            }

            finalTexture.Apply();

            //Reset offset
            offset_y = 0;
            offset_x = 0;


            //Vector difference:
            int count = 0;
            foreach (var position in tilemap.cellBounds.allPositionsWithin)
            {

                if (tilemap.HasTile(position))
                {
                    var correspondingSprite = T2IUtils.GetCurrentSprite(tilemap.GetSprite(position));

                    for (int i = 0; i < correspondingSprite.width; i++)
                    {
                        for (int j = 0; j < correspondingSprite.height; j++)
                        {
                            finalTexture.SetPixel(
                                i + offset_x * (int)spriteWith - beginPoint.x,
                                j + offset_y * (int)spriteHeight - beginPoint.y,
                                correspondingSprite.GetPixel(i, j));
                            count++;
                        }
                    }
                }
                offset_x++;
                if (offset_x > colNumber - 1)
                {
                    offset_y++;
                    offset_x = 0;
                }
            }

            finalTexture.filterMode = FilterMode.Point;
            finalTexture.Apply();

            return finalTexture; //Return Texture2D

        }
    }

}
