
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using System;
using System.Text.RegularExpressions;

namespace TileMap2Img
{

    public enum ImageFormat
    {
        PNG = 0,
        JPG = 1,
        EXR = 2,
        TGA = 3,
    }
    
    public class Tilemap2Image : EditorWindow
    {
        public static string STRING_WINDOW_NAME = "Tilemap To Image";
        //Action:
        Tilemap _selectedTilemap = null;
        string _fileName = null;


        SpriteRenderer _debugRenderer;
        int _selectedFormat;
        private int minX, maxX, minY, maxY;


        private static readonly Regex sWhitespace = new Regex(@"\s+");

        #region GUI & Action
        [MenuItem("Window/TileMap2Img")]
        public static void ShowWindow()
        {
            GetWindow<Tilemap2Image>(STRING_WINDOW_NAME);
        }

        private void OnGUI()
        {
            GUILayout.Label("Convert Tilemap to Images", EditorStyles.boldLabel);

            EditorGUILayout.Space();
            _selectedTilemap = EditorGUILayout.ObjectField("Tilemap", _selectedTilemap, typeof(Tilemap), true) as Tilemap;
            _selectedFormat = EditorGUILayout.Popup("Format", _selectedFormat, Enum.GetNames(typeof(ImageFormat)));
            //Debug only
            _debugRenderer = EditorGUILayout.ObjectField("Sprite Renderer", _debugRenderer, typeof(SpriteRenderer), true) as SpriteRenderer;

            _fileName = EditorGUILayout.TextField("File Name", _fileName);
            EditorGUILayout.Space();

            // Export Button
            EditorGUI.BeginDisabledGroup(_selectedTilemap == null || _fileName == null || ReplaceWhitespace(_fileName, "").Length == 0);

            if (GUILayout.Button("Export Image"))SaveImage((ImageFormat)_selectedFormat);
            
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Reset Input"))
            {
                _selectedTilemap = null;
                _fileName = null;
            }

            if (GUILayout.Button("Test"))
            {
                Debug.LogError("Cell Bounds:" + _selectedTilemap.cellBounds.ToString());
                Debug.LogError("Cell Bounds (Local):" + _selectedTilemap.localBounds.ToString());
                Debug.LogError("Size:" + _selectedTilemap.size.ToString());

                var tilemap = _selectedTilemap;
                int constValue = 4;
                var firstTile = tilemap.GetTilesBlock(tilemap.cellBounds)[0] as Tile;

                float width = firstTile.sprite.rect.width;
                float height = firstTile.sprite.rect.height;

                Debug.LogError("New width " + width);
                Texture2D tilemapTexture = new Texture2D((int)width*tilemap.size.x*constValue, (int)height * tilemap.size.y*constValue);
             
                Color[] newcolors = new Color[(int)width * (int)height];
                //Sprite tileMapSprite = tilemap.GetSprite(new Vector3Int(0, 1, 0));
                //Texture2D _tx = GetCurrentSprite(tileMapSprite);

                for (int i = 0; i < newcolors.Length; i++)
                {
                    newcolors[i] = Color.red;
                }
                tilemapTexture.SetPixels(
                    0 * (int)width,
                    0 * (int)height,
                    (int)width*constValue,
                    (int)height*constValue,
                    newcolors
                    );
                tilemapTexture.SetPixels(
                    0 * (int)width,
                    3 * (int)height,
                    (int)width*constValue, (int)height*constValue, newcolors
                    );

                tilemapTexture.Apply();
                _debugRenderer.sprite = Sprite.Create(tilemapTexture, new Rect(0,0,tilemapTexture.width, tilemapTexture.height), new Vector2(0.5f, 0.5f));
            }
            //Warning
            if (_selectedTilemap == null) EditorGUILayout.HelpBox("Missing Tilemap", MessageType.Warning);
            if (_fileName == null || ReplaceWhitespace(_fileName, "").Length == 0 )EditorGUILayout.HelpBox("Missing Filename", MessageType.Warning);
        }

        #endregion

        #region Utilities
        public static string ReplaceWhitespace(string input, string replacement)
        {
            return sWhitespace.Replace(input, replacement);
        }
        Texture2D GetCurrentSprite(Sprite sprite) //metodo para obtener el sprite recortado tal y como lo ponemos
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
                                             (int)sprite.textureRect.height);
            textura.filterMode = FilterMode.Point;
            textura.SetPixels(pixels);
            textura.Apply();
            return textura;
        }
        private bool HandleTextureUnreadable(Texture2D texture2D)
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
        #endregion

        #region Core
        private void SaveImage(ImageFormat imageFormat)
        {
            var _texture = ProcesTilemap();
            if (_texture == null)
            {
                EditorUtility.DisplayDialog("Cannot Convert", "Input texture has no sprite or unknown problem occurs", "Cancel");
                return;
            }

            string path = EditorUtility.SaveFolderPanel("Save Image", "", _fileName);
            if (path.Length != 0)
            {

                byte[] byteCodedata;
                if (imageFormat == ImageFormat.PNG)
                {
                    byteCodedata = _texture.EncodeToPNG();
                } else if (imageFormat == ImageFormat.JPG)
                {
                    byteCodedata = _texture.EncodeToJPG(); 
                }
                else if (imageFormat == ImageFormat.EXR)
                {
                    byteCodedata = _texture.EncodeToEXR();
                }
                else if (imageFormat == ImageFormat.TGA)
                {
                    byteCodedata = _texture.EncodeToTGA();
                } else
                {
                    byteCodedata = _texture.EncodeToPNG();
                }

                if (byteCodedata != null)
                {
                    File.WriteAllBytes(path + "/" + _fileName +"."+ imageFormat.ToString().ToLower(), byteCodedata);
                    EditorUtility.DisplayDialog("Convert Success", string.Format("The file {0}.{1} has been exported",_fileName ,imageFormat.ToString()), "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Cannot Convert", "Unknown Problem occurs", "Cancel");
                }

                // Just in case we are saving to the asset folder, tell Unity to scan for modified or new assets
                AssetDatabase.Refresh();
            }
        }
        private Texture2D ProcesTilemap()
        {
            Tilemap tilemap = _selectedTilemap;

            Sprite CurSprite = null;
            bool anySpriteExist = false;
            for (int x = 0; x < tilemap.size.x; x++){
                for (int y = 0; y < tilemap.size.y; y++) {
                    Vector3Int pos = new Vector3Int(-x, -y, 0);
                    if (tilemap.GetSprite(pos) != null)
                    {
                        anySpriteExist = true;
                        CurSprite = tilemap.GetSprite(pos); 
                        if (minX > pos.x)
                        {
                            minX = pos.x;
                        }
                        if (minY > pos.y)
                        {
                            minY = pos.y;
                        }
                    }

                    pos = new Vector3Int(x, y, 0);
                    if (tilemap.GetSprite(pos) != null)
                    {
                        anySpriteExist = true;

                        if (maxX < pos.x)
                        {
                            maxX = pos.x;
                        }
                        if (maxY < pos.y)
                        {
                            maxY = pos.y;
                        }
                    }
                }
            }

            if (!anySpriteExist) return null;



            //Width
            float width = CurSprite.rect.width;
            float height = CurSprite.rect.height;



            //
            Texture2D tilemapTexture = new Texture2D((int)width * tilemap.size.x, (int)height * tilemap.size.y);
            //Debug.Log(string.Format("TM size X:  w:{0} h:{1}", tilemap.size.x, tilemap.size.y));

            //Debug.Log(string.Format("Important X: {0} {1}, values Y: {2}, {3}, width: {4} , height {5}", minX, maxX, minY, maxY, width, height));
            //Debug.Log(string.Format("Tilemap Texture {0} {1}", tilemapTexture.width, tilemapTexture.height));

            //
            Color[] invisible = new Color[tilemapTexture.width * tilemapTexture.height];
            for (int i = 0; i < invisible.Length; i++)
            {
                invisible[i] = new Color(0f, 0f, 0f, 0f);
            }

            tilemapTexture.filterMode = FilterMode.Point;
            tilemapTexture.SetPixels(0, 0, tilemapTexture.width, tilemapTexture.height, invisible);

            //
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (tilemap.GetSprite(new Vector3Int(x, y, 0)) != null)
                    {
                        
                        Texture2D _tx = GetCurrentSprite(tilemap.GetSprite(new Vector3Int(x, y, 0)));

                        _tx.filterMode = FilterMode.Point;
                        if (_tx == null) return null;
                        tilemapTexture.SetPixels(
                            (x - minX) * (int)width, 
                            (y - minY) * (int)height,
                            (int)width, (int)height,
                            _tx.GetPixels());
                    }
                }
            }
            tilemapTexture.Apply();

            return tilemapTexture; //Return Texture2D

        }
        #endregion
    }

}
