
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using System;
using TileMap2Img.Define;
namespace TileMap2Img
{

    public class T2IEditor : EditorWindow
    {
        #region Fields
        private Tilemap _selectedTilemap = null;
        private string _fileName = null;
        private int _selectedFormat;

        public T2IEditor()
        {
        }
        #endregion

        #region GUI & Action
        [MenuItem("Window/TileMap2Img")]
        public static void ShowWindow()
        {
            GetWindow<T2IEditor>(LabelText.WindowName);
        }

        private void OnGUI()
        {
            SetupGUI();

            // Disable Group
            EditorGUI.BeginDisabledGroup(_selectedTilemap == null);
           
            if (GUILayout.Button(LabelText.ExportImage)) ExportTilemap();   
            
            EditorGUI.EndDisabledGroup();
            
            if (GUILayout.Button(LabelText.ResetInput)) ResetInput();

            DisplayWarning();
        }

        #endregion

        #region UI
        private void SetupGUI()
        {
            GUILayout.Label(LabelText.Title, EditorStyles.boldLabel);

            EditorGUILayout.Space();

            _selectedTilemap = EditorGUILayout.ObjectField(LabelText.TileMap, _selectedTilemap, typeof(Tilemap), true) as Tilemap;
            _selectedFormat = EditorGUILayout.Popup(LabelText.ImageFormat, _selectedFormat, Enum.GetNames(typeof(ImageFormat)));

            EditorGUILayout.Space();
            _fileName = EditorGUILayout.TextField(LabelText.ImageFileName, _fileName);
            EditorGUILayout.Space();

        }

        private void DisplayWarning()
        {
            //Warning
            if (_selectedTilemap == null) EditorGUILayout.HelpBox(LabelText.MissingTileMap, MessageType.Warning);
            if (_fileName == null || T2IUtils.ReplaceWhitespace(_fileName, "").Length == 0) EditorGUILayout.HelpBox(LabelText.MissingFileName, MessageType.Warning);
        }
        #endregion

        #region Button
        private void ExportTilemap()
        {
            var CellLayout = T2IUtils.GetGridLayout(_selectedTilemap);

            TilemapParam tileMapParam = new TilemapParam
            {
                Tilemap = _selectedTilemap,
                Layout = CellLayout,
                ImageFormat = (ImageFormat)_selectedFormat
            };

            T2ICore.Process(tileMapParam, _fileName);
        }

        private void ResetInput()
        {
            _selectedTilemap = null;
            _fileName = null;
        }
        #endregion
    }

}
