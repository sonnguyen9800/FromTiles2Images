using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TitlemapOriginDetector : MonoBehaviour
{
    Tilemap tilemap;

    void OnValidate()
    {
        if (tilemap == null)
            tilemap = GetComponent<Tilemap>();
    }

    void OnDrawGizmos()
    {
        Draw();
    }

    void Draw()
    {
        if (tilemap == null)
            return;

        // tilemap position
        var tp = tilemap.transform.position;

        // bounds + offset
        var tBounds = tilemap.cellBounds;

        // corner points
        var c0 = new Vector3(tBounds.min.x, tBounds.min.y) + tp;
        var c1 = new Vector3(tBounds.min.x, tBounds.max.y) + tp;
        var c2 = new Vector3(tBounds.max.x, tBounds.max.y) + tp;
        var c3 = new Vector3(tBounds.max.x, tBounds.min.y) + tp;

        // draw borders
        Debug.DrawLine(c0, c1, Color.red);
        Debug.DrawLine(c1, c2, Color.red);
        Debug.DrawLine(c2, c3, Color.red);
        Debug.DrawLine(c3, c0, Color.red);

        // draw origin cross
        Debug.DrawLine(new Vector3(tp.x, tBounds.min.y + tp.y), new Vector3(tp.x, tBounds.max.y + tp.y), Color.green);
        Debug.DrawLine(new Vector3(tBounds.min.x + tp.x, tp.y), new Vector3(tBounds.max.x + tp.x, tp.y), Color.green);
    }
}