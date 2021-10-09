using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class TitlemapTools : MonoBehaviour
{
    [SerializeField]
    private Tilemap _titleMap = null;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.LogError("On Click");
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int gridPosition = _titleMap.WorldToCell(mousePosition);
            Debug.LogError(gridPosition);
        }
    }
}
