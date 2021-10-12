using System.Collections;
using System.Collections.Generic;
using System.IO;
using TileMap2Img.Core;
using TileMap2Img.Define;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.GridLayout;

namespace TileMap2Img
{
    public static class T2ICore
    {
        public static void Process(TilemapParam param, string fileName)
        {
            Texture2D _texture = null;
            switch (param.Layout)
            {
                case CellLayout.Rectangle:
                    T2ILayout t2ILayout = new T2IRectangle
                    {
                        TilemapParam = param
                    };
                    _texture = t2ILayout.ExportTexture2d();
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

            SaveImage(param, fileName, _texture);
        }

        private static void SaveImage(TilemapParam param, string fileName, Texture2D _texture)
        {
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
                    File.WriteAllBytes(path + "/" + fileName + "." + param.ImageFormat.ToString().ToLower(), byteCodedata);
                    EditorUtility.DisplayDialog(LabelText.SuccessConvert, string.Format("The file {0}.{1} has been exported", fileName, param.ImageFormat.ToString().ToLower()), "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog(LabelText.CannotConvert, LabelText.ProblemLog, LabelText.Cancel);
                }

                // Just in case we are saving to the asset folder, tell Unity to scan for modified or new assets
                AssetDatabase.Refresh();
            }
        }
    }

}
