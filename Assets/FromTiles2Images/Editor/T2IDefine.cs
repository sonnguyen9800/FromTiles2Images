using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.GridLayout;

namespace TileMap2Img.Define
{
    public enum ImageFormat
    {
        PNG = 0,
        JPG = 1,
        EXR = 2,
        TGA = 3,
    }

    public class TileMapParam
    {
        public Tilemap Tilemap;
        public Grid.CellLayout Layout = Grid.CellLayout.Rectangle;
        public ImageFormat ImageFormat = ImageFormat.PNG;
    }
    public static class LabelText
    {
        public static string WindowName = "Tilemap To Image";

        public static string Title = "Convert Tilemap to Images";

        public static string TileMap = "Tilemaps";
        public static string ImageFormat = "Format";
        public static string ImageFileName = "File Name";
        public static string ImageSave = "Save Image";

        // Button
        public static string ExportImage = "Export Image";
        public static string ResetInput = "Reset Input";

        // Dialog
        public static string CannotConvert = "Cannot Convert";
        public static string SuccessConvert = "Convert Success";

        public static string Yes = "Yes";
        public static string Cancel = "Cancel";

        // Log Error
        public static string ProblemLog = "Unknown Problem occurs";
        public static string NoSpriteFound = "No Title with sprite attached could be found";
        public static string MissingTileMap = "Missing Tilemap";
        public static string MissingFileName = "Missing FileName";
        public static string FeatureUnsupported = "Unsupported Layout";

    }

    struct RectangleTilemapData
    {
        public Vector2Int TopLeft;
        public Vector2Int TopRight;
        public Vector2Int BottomLeft;
        public Vector2Int BottomRight;
    }
}

