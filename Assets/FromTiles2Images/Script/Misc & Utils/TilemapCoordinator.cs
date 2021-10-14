using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class TilemapCoordinator : MonoBehaviour
{

    public Grid grid;


    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector3Int position = Vector3Int.FloorToInt(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin);
            Vector3Int cellPos = grid.WorldToCell(position);

            Debug.Log(cellPos);
        }
    }




}
