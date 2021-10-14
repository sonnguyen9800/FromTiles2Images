using System.Collections;
using System.Collections.Generic;
using TileMap2Img.Define;
using UnityEngine;

namespace TileMap2Img.Core
{
    public abstract class T2ILayout
    {

        // Start is called before the first frame update
        public abstract Texture2D ExportTexture2d();
    }
}

