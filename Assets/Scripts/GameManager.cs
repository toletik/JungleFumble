using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] int length = 20;
    [SerializeField] int height = 14;
    [SerializeField] Camera cam = null;

    [SerializeField] GameObject map = null;
    [SerializeField] GameObject highlightTileParent = null;
    [SerializeField] GameObject gridParent = null;


    Transform selectedEntity = null;
    List<int> indexHighlightTiles = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        map.transform.localScale = new Vector3(length, height, 1);
        GenerateGrid();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            
            if (Physics.Raycast(ray, out hit))
            {
                //select Allies
                if (hit.transform.CompareTag("Allies"))
                {                   
                    GenerateHighlightTiles(GetTile(hit.point.x, hit.point.y), hit.transform.GetComponent<Character>().mvt, true);
                    selectedEntity = hit.transform;
                }
                //select Enemies
                else if (hit.transform.CompareTag("Enemies"))
                {
                    GenerateHighlightTiles(GetTile(hit.point.x, hit.point.y), hit.transform.GetComponent<Character>().mvt, false);
                }
                //select a Tile
                else
                {
                    int tileIndex = GetTile(hit.point.x, hit.point.y);


                    if (selectedEntity != null && indexHighlightTiles.Contains(tileIndex) )
                    {
                        Vector2 temp = GetPosFromTile(tileIndex);
                        selectedEntity.position = new Vector3(temp.x, temp.y, selectedEntity.position.z);
                    }

                    ClearHighlightTiles();

                }

            }
        }


        if (Input.GetMouseButtonDown(1))
        {
            ClearHighlightTiles();
        }


    }


    void GenerateGrid()
    {
        //clear previous lines
        foreach (Transform child in gridParent.transform)
            Destroy(child.gameObject);

        //Vertical
        float minX = -0.5f * (length - 2);
        for (int l = 0; l < length - 1; l++)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Quad);
            line.transform.SetParent(gridParent.transform);
            line.transform.localScale = new Vector3(0.1f, height, 0.1f);
            line.transform.localPosition = new Vector3(minX + l, 0, 0);
            line.GetComponent<Renderer>().material.color = Color.black;
        }

        //Horizontal
        float minY = -0.5f * (height - 2);
        for (int h = 0; h < height - 1; h++)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Quad);
            line.transform.SetParent(gridParent.transform);
            line.transform.localScale = new Vector3(length, 0.1f, 0.1f);
            line.transform.localPosition = new Vector3(0, minY + h, 0);
            line.GetComponent<Renderer>().material.color = Color.black;
        }


    }
    void GenerateHighlightTiles(int CentralTile, int statMvt, bool isAllies)
    {
        ClearHighlightTiles();

        float minX = -0.5f * (length - 1);
        float minY = -0.5f * (height - 1);

        for (int h = 0; h < height; h++)
            for (int l = 0; l < length; l++)
            {
                int tileIndex = l + h * length;

                if (CentralTile == tileIndex)
                    continue;

                int offsetX = Mathf.Abs(CentralTile % length - tileIndex % length);
                int offsetY = Mathf.Abs((int)(CentralTile / length) - (int)(tileIndex / length));

                if (offsetX + offsetY <= statMvt)
                {
                    indexHighlightTiles.Add(tileIndex);
                    GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    quad.transform.SetParent(highlightTileParent.transform);
                    quad.transform.localPosition = new Vector3(minX + l, minY + h, 0);
                    quad.GetComponent<Renderer>().material.color = isAllies ? Color.blue : Color.red;
                }
            }
    }
    void ClearHighlightTiles()
    {
        //clear previous Highlight
        foreach (Transform child in highlightTileParent.transform)
            Destroy(child.gameObject);

        indexHighlightTiles.Clear();
        selectedEntity = null;
    }

    int GetTile(float x, float y)
    { 
        return Mathf.RoundToInt(x + 0.5f * (length - 1)) + Mathf.RoundToInt(y + 0.5f * (height - 1)) * length; 
    }
    Vector2 GetPosFromTile(int tileIndex)
    {
        return new Vector2(-0.5f * (length - 1) + tileIndex % length, 
                           -0.5f * (height - 1) + (int)(tileIndex / length));
    }

}