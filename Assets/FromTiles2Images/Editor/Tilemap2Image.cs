
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using System;
using System.Text.RegularExpressions;
using UnityEngine.UI;

namespace TileMap2Img
{
    struct RectangleTilemapData
    {
        public Vector2Int TopLeft;
        public Vector2Int TopRight;
        public Vector2Int BottomLeft;
        public Vector2Int BottomRight;
    }
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



        Image _testImg = null;

        SpriteRenderer _debugRenderer;

        SpriteRenderer _debugRenderer2;


        int _selectedFormat;
        private int minX, maxX, minY, maxY;

        //For prototype & testing
        Sprite _sampleSprite = null;
        int UnityUnitPerPixel;


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

            _debugRenderer2 = EditorGUILayout.ObjectField("Sprite Renderer 2", _debugRenderer2, typeof(SpriteRenderer), true) as SpriteRenderer;

            _sampleSprite = EditorGUILayout.ObjectField("Sprite 4 Test", _sampleSprite, typeof(Sprite), true) as Sprite;

            UnityUnitPerPixel = EditorGUILayout.IntField("Unit per pixel", UnityUnitPerPixel);

            _testImg = EditorGUILayout.ObjectField("Image 4 Test", _testImg, typeof(Image), true) as Image;
            EditorGUILayout.Space();


            // End Debug 
            _fileName = EditorGUILayout.TextField("File Name", _fileName);
            EditorGUILayout.Space();


            // Export Button
            EditorGUI.BeginDisabledGroup(_selectedTilemap == null || _fileName == null || ReplaceWhitespace(_fileName, "").Length == 0);

            if (GUILayout.Button("Export Image"))SaveImage((ImageFormat)_selectedFormat);
            
            EditorGUI.EndDisabledGroup();

            //Warning
            if (_selectedTilemap == null) EditorGUILayout.HelpBox("Missing Tilemap", MessageType.Warning);
            if (_fileName == null || ReplaceWhitespace(_fileName, "").Length == 0) EditorGUILayout.HelpBox("Missing Filename", MessageType.Warning);

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

            if (GUILayout.Button("Draw on Texture"))
            {
                // Get Basic Info
                var bounds = _selectedTilemap.cellBounds.size;
                Debug.Log(bounds.ToString());

                int rowNumber = bounds.y;
                int colNumber = bounds.x;


                Debug.Log(string.Format("Number of Rows: {0} Columns: {1}", rowNumber, colNumber));

                // Get Sprite
                Sprite firstSprite = GetFirstSprite(_selectedTilemap);

                if (firstSprite == null) Debug.Log("No Title with sprite attached could be found");
                float spriteWith = firstSprite.rect.width;
                float spriteHeight = firstSprite.rect.height;


                Debug.Log(string.Format("Sprite With {0} & Height: {1}", spriteWith.ToString(), spriteHeight));

                // Create Texture (big)
                //int unitPerPixel = UnityUnitPerPixel;
                int unitPerPixel = 1;

                Texture2D texture2D = new Texture2D((int)spriteWith* colNumber *unitPerPixel,
                    (int) spriteHeight * rowNumber*unitPerPixel);
                
                // Init Texture with all red color
                for(int i = 0; i < (int)spriteWith* colNumber*unitPerPixel; i++)
                {
                    for (int j = 0; j < (int)spriteHeight * rowNumber*unitPerPixel; j++)
                    {
                        texture2D.SetPixel(i, j, Color.red);
                    }
                }

                texture2D.Apply();


                Debug.Log(string.Format("Size of texture: width: {0}/ height: {1}",
                    texture2D.width, texture2D.height));
                
                // Main Loop
                int offset_x = 0, offset_y = 0;

                foreach(var position in _selectedTilemap.cellBounds.allPositionsWithin)
                {
                    if (_selectedTilemap.HasTile(position))
                    {
                        //var correspondingSprite = _selectedTilemap.GetSprite(position);

                        var correspondingSprite = GetCurrentSprite(_selectedTilemap.GetSprite(position));

                        for (int i = 0; i < correspondingSprite.width; i++)
                        {
                            for (int j = 0; j < correspondingSprite.height; j++)
                            {
                                texture2D.SetPixel(
                                    i + offset_x * (int)spriteWith,
                                    j + offset_y * (int)spriteHeight,
                                    correspondingSprite.GetPixel(i,j)); 

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
                texture2D.filterMode = FilterMode.Point;

                texture2D.Apply();


                // Create Sprite to test
                Sprite testSprite = Sprite.Create(texture2D, new Rect(0.0f, 0.0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));


                if (_testImg != null)
                {
                    _testImg.sprite = testSprite;
                    _testImg.SetNativeSize();
                }

                if (_debugRenderer != null)
                {
                    _debugRenderer.sprite = testSprite;

                }
                return;

                // Cropping Texture
                


            }

            if (GUILayout.Button("Hardcode Sprite"))
            {
                // Get Basic Info
                var bounds = _selectedTilemap.cellBounds.size;
                Debug.Log(bounds.ToString());

                int rowNumber = bounds.y;
                int colNumber = bounds.x;


                Debug.Log(string.Format("Number of Rows: {0} Columns: {1}", rowNumber, colNumber));

                // Get Sprite
                Sprite firstSprite = GetFirstSprite(_selectedTilemap);

                if (firstSprite == null) Debug.Log("No Title with sprite attached could be found");
                float spriteWith = firstSprite.rect.width;
                float spriteHeight = firstSprite.rect.height;


                Debug.Log(string.Format("Sprite With {0} & Height: {1}", spriteWith.ToString(), spriteHeight));

                // Create Texture (big)
                //int unitPerPixel = UnityUnitPerPixel;
                int unitPerPixel = 1;

                Texture2D texture2D = new Texture2D((int)spriteWith * colNumber * unitPerPixel,
                    (int)spriteHeight * rowNumber * unitPerPixel);

                // Init Texture with all red color
                for (int i = 0; i < (int)spriteWith * colNumber * unitPerPixel; i++)
                {
                    for (int j = 0; j < (int)spriteHeight * rowNumber * unitPerPixel; j++)
                    {
                        texture2D.SetPixel(i, j, Color.red);
                    }
                }

                texture2D.Apply();


                Debug.Log(string.Format("Size of texture: width: {0}/ height: {1}",
                    texture2D.width, texture2D.height));

                // Main Loop
                int offset_x = 0, offset_y = 0;

                foreach (var position in _selectedTilemap.cellBounds.allPositionsWithin)
                {
                    if (_selectedTilemap.HasTile(position))
                    {
                        var correspondingSprite = GetCurrentSprite(_selectedTilemap.GetSprite(position));

                        for (int i = 0; i < correspondingSprite.width; i++)
                        {
                            for (int j = 0; j < correspondingSprite.height; j++)
                            {
                                texture2D.SetPixel(
                                    i + offset_x * (int)spriteWith,
                                    j + offset_y * (int)spriteHeight,
                                    correspondingSprite.GetPixel(i, j));

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
                texture2D.filterMode = FilterMode.Point;
                texture2D.Apply();


                // Create Sprite to test
                Sprite testSprite = Sprite.Create(texture2D, new Rect(0.0f, 0.0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
                if (_testImg != null)
                {
                    _testImg.sprite = testSprite;
                    _testImg.SetNativeSize();
                }

                if (_debugRenderer != null)
                {
                    _debugRenderer.sprite = testSprite;

                }

                //Reset offset
                offset_y = 0;
                offset_x = 0;

                // Bounder points:
                int minX = minY = int.MaxValue; int maxX = maxY = int.MinValue;

                foreach (var pos in _selectedTilemap.cellBounds.allPositionsWithin)
                {
                    if (_selectedTilemap.HasTile(pos))
                    {
                        //Check if the point can be new Bottom Left
                        if (minX > offset_x) minX = offset_x;

                        if (maxX < offset_x) maxX = offset_x;

                        if (minY > offset_y) minY = offset_y;

                        if (maxY < offset_y) maxY = offset_y;
                    }


                    offset_x++;
                    if (offset_x > colNumber - 1)
                    {
                        offset_y++;
                        offset_x = 0;
                    }
                }
                RectangleTilemapData rectangleTilemapData = new RectangleTilemapData();

                rectangleTilemapData.BottomLeft = new Vector2Int(minX, minY);
                rectangleTilemapData.BottomRight = new Vector2Int(maxX, minY);
                rectangleTilemapData.TopLeft = new Vector2Int(minX, maxY);
                rectangleTilemapData.TopRight = new Vector2Int(maxX, maxY);

                // Prepare before crop:
                Vector2Int beginPoint = rectangleTilemapData.BottomLeft;

                Vector2Int areaCrop = new Vector2Int();
                areaCrop.x = (maxX - minX + 1) * (int)spriteWith;
                areaCrop.y = (maxY - minY + 1) * (int)spriteHeight;

                // Loop to cut:
                Texture2D finalTexture = new Texture2D(areaCrop.x, areaCrop.y);


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
                foreach (var position in _selectedTilemap.cellBounds.allPositionsWithin)
                {


                    if (_selectedTilemap.HasTile(position))
                    {
                        var correspondingSprite = GetCurrentSprite(_selectedTilemap.GetSprite(position));

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

                Debug.LogError(string.Format("Data Count: {0}", count));
                finalTexture.filterMode = FilterMode.Point;
                finalTexture.Apply();

                Debug.LogError(string.Format("Get color: {0}", finalTexture.GetPixel(0,0)));



                Sprite testSprite2 = Sprite.Create(finalTexture, new Rect(0.0f, 0.0f, finalTexture.width, finalTexture.height), new Vector2(0.5f, 0.5f));

                if (_debugRenderer2 != null)
                {
                    _debugRenderer2.sprite = testSprite2;
                }
                Debug.LogError("End");

                
            }

        }


        private bool CheckOffsetExist(int x, int y, int maxX, int minX, int maxY, int minY)
        {
            if (x < minX || x > maxX) return false;
            if (y < minY || y > maxY) return false;

            return true;
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

        //Get first sprite
        private Sprite GetFirstSprite(Tilemap tilemap)
            
        {
            foreach(var pos in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.HasTile(pos))
                {
                    return tilemap.GetSprite(pos);
                }
            }
            return null;
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
