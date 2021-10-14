using System.Collections;
using System.Collections.Generic;
using TileMap2Img.Core;
using TileMap2Img.Define;
using UnityEngine;


namespace TileMap2Img
{
    public class T2IRectangle : T2ILayout
    {
        #region Public Attributes
        public TilemapParam TilemapParam = null;
        public Vector3Int Bounds { get => _bounds; set => _bounds = value; }
        #endregion

        #region Private Attributes
        private Vector3Int _bounds;
        private int _rowNum = 0;
        private int _colNum = 0;
        private float _spriteWidth = 0;
        private float _spriteHeight = 0;
        #endregion

        #region override
        public override Texture2D ExportTexture2d()
        {
            if (TilemapParam == null) return null;

            var tilemap = TilemapParam.Tilemap;
            int _minX, _maxX, _minY, _maxY;

            // Get Basic Info
            Bounds = tilemap.cellBounds.size;
            _rowNum = Bounds.y;
            _colNum = Bounds.x;


            // Get Sprite
            Sprite firstSprite = T2IUtils.GetFirstSprite(tilemap);

            if (firstSprite == null) Debug.Log(LabelText.NoSpriteFound);

             _spriteWidth = firstSprite.rect.width;
             _spriteHeight = firstSprite.rect.height;

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
                if (offset_x > _colNum - 1)
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
            areaCrop.x = (_maxX - _minX + 1) * (int)_spriteWidth;
            areaCrop.y = (_maxY - _minY + 1) * (int)_spriteHeight;

            // Loop to cut:
            Texture2D finalTexture = new Texture2D(areaCrop.x, areaCrop.y);

            // Initialize
            InitTexture(finalTexture);

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
                    if (correspondingSprite != null)
                    {
                        for (int i = 0; i < correspondingSprite.width; i++)
                        {
                            for (int j = 0; j < correspondingSprite.height; j++)
                            {
                                finalTexture.SetPixel(
                                    i + offset_x * (int)_spriteWidth - beginPoint.x,
                                    j + offset_y * (int)_spriteHeight - beginPoint.y,
                                    correspondingSprite.GetPixel(i, j));
                                count++;
                            }
                        }
                    }

                }
                offset_x++;
                if (offset_x > _colNum - 1)
                {
                    offset_y++;
                    offset_x = 0;
                }
            }

            finalTexture.filterMode = FilterMode.Point;
            finalTexture.Apply();

            return finalTexture; //Return Texture2D

        }
        #endregion

        #region private
        private void InitTexture(Texture2D texture)
        {
            for (int i = 0; i < texture.width; i++)
            {
                for (int j = 0; j < texture.height; j++)
                {
                    texture.SetPixel(i, j, new Color(0, 0, 0, 0));
                }
            }

            texture.Apply();
        }

        #endregion
    }

}
