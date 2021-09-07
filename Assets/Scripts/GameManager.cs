using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] int   length = 20;
    [SerializeField] int   height = 14;
    [SerializeField] float speed = 10;

    [SerializeField] Camera cam = null;
    [SerializeField] GameObject map = null;
    [SerializeField] GameObject highlightTileParent = null;
    [SerializeField] GameObject gridParent = null;


    Transform selectedEntity = null;
    List<int> indexHighlightTiles = new List<int>();


    bool inPlayMode = false;
    [SerializeField] List<GameObject> characters = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        map.transform.localScale = new Vector3(length, height, 1);
        GenerateGrid();

    }

    // Update is called once per frame
    void Update()
    {
        if (inPlayMode)
            PlayMode();
        else
            TacticalMode();

    }


    void PlayMode()
    {
        foreach (GameObject character in characters)
        {
            Character characterScript = character.GetComponent<Character>();

            if (characterScript.queueTileIndex.Count > 0)
            {
                Vector2 tilePos = GetPosFromTile(characterScript.queueTileIndex[0]);
                Vector3 direction = new Vector3(tilePos.x, tilePos.y, character.transform.position.z) - character.transform.position;


                character.transform.position += direction.normalized * (speed * Time.deltaTime);

                //if you pass the waypoint remove it
                if (Vector3.Dot(direction, new Vector3(tilePos.x, tilePos.y, character.transform.position.z) - character.transform.position) < 0)
                {
                    characterScript.queueTileIndex.RemoveAt(0);
                    UpdateTrailPath(character);
                }
                else
                    character.GetComponent<LineRenderer>().SetPosition(0, character.transform.position);

            }
        }

        if (!isThereStillWaypoints())
            inPlayMode = false;
    }
    void TacticalMode()
    {

        if (Input.GetKeyDown(KeyCode.Space))
            inPlayMode = true;


        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);


            if (Physics.Raycast(ray, out hit))
            {


                //select Allies
                if (hit.transform.CompareTag("Allies"))
                {
                    //to check if display highlight tiles from character or last waypoint
                    selectedEntity = hit.transform;
                    Character character = hit.transform.GetComponent<Character>();
                    GenerateHighlightTiles(character.queueTileIndex.Count == 0 ? GetTile(hit.point.x, hit.point.y) : character.queueTileIndex[character.queueTileIndex.Count - 1], character.mvt, true);

                }
                //select Enemies
                else if (hit.transform.CompareTag("Enemies"))
                {
                    GenerateHighlightTiles(GetTile(hit.point.x, hit.point.y), hit.transform.GetComponent<Character>().mvt, false);
                    selectedEntity = null;
                }
                //select a Tile
                else
                {
                    int tileIndex = GetTile(hit.point.x, hit.point.y);

                    if (selectedEntity != null && indexHighlightTiles.Contains(tileIndex))
                    {
                        Character character = selectedEntity.GetComponent<Character>();
                        int referenceTile = character.queueTileIndex.Count == 0 ? GetTile(selectedEntity.position.x, selectedEntity.position.y) : character.queueTileIndex[character.queueTileIndex.Count - 1];

                        int offsetX = GetOffsetXBetweenTiles(referenceTile, tileIndex);
                        int offsetY = GetOffsetYBetweenTiles(referenceTile, tileIndex);

                        for (int i = 1; i < Mathf.Abs(offsetX) + 1; ++i)
                        {
                            character.queueTileIndex.Add(referenceTile + ((offsetX > 0) ? i : -i));
                        }

                        //Reset reference tile because tiles maybe have been add
                        referenceTile = character.queueTileIndex.Count == 0 ? GetTile(selectedEntity.position.x, selectedEntity.position.y) : character.queueTileIndex[character.queueTileIndex.Count - 1];

                        for (int i = 1; i < Mathf.Abs(offsetY) + 1; ++i)
                        {
                            character.queueTileIndex.Add(referenceTile + length * ((offsetY > 0) ? i : -i));
                        }

                        GenerateHighlightTiles(character.queueTileIndex.Count == 0 ? GetTile(selectedEntity.position.x, selectedEntity.position.y) : character.queueTileIndex[character.queueTileIndex.Count - 1], character.mvt, true);
                        UpdateTrailPath(selectedEntity.gameObject);
                    }
                    else
                    {
                        ClearHighlightTiles();
                        selectedEntity = null;
                    }

                }

            }
        }


        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);


            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag("Allies"))
                {
                    selectedEntity = hit.transform;
                    ClearTrailPath(hit.transform.gameObject);
                }
                else
                    ClearHighlightTiles();
            }
            else
                ClearHighlightTiles();
        }

    }




    //Playmode
    bool isThereStillWaypoints()
    {
        foreach (GameObject character in characters)
            if (character.GetComponent<Character>().queueTileIndex.Count > 0)
                return true;

        return false;
    }


    //Grid
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
    //Highlight
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

                if (GetOffsetAllBetweenTiles(CentralTile, tileIndex) <= statMvt - (isAllies ? selectedEntity.GetComponent<Character>().queueTileIndex.Count : 0))
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
    }
    //Tiles
    int GetTile(float x, float y)
    {
        return Mathf.RoundToInt(x + 0.5f * (length - 1)) + Mathf.RoundToInt(y + 0.5f * (height - 1)) * length;
    }
    Vector2 GetPosFromTile(int tileIndex)
    {
        return new Vector2(-0.5f * (length - 1) + tileIndex % length,
                           -0.5f * (height - 1) + (int)(tileIndex / length));
    }
    int GetOffsetAllBetweenTiles(int tile1, int tile2) { return Mathf.Abs(GetOffsetXBetweenTiles(tile1, tile2)) + Mathf.Abs(GetOffsetYBetweenTiles(tile1, tile2)); }
    int GetOffsetXBetweenTiles(int tile1, int tile2) { return tile2 % length - tile1 % length;  }
    int GetOffsetYBetweenTiles(int tile1, int tile2) { return (int)(tile2 / length) - (int)(tile1 / length); }
    //Trail
    void UpdateTrailPath(GameObject character)
    {
        Character characterScript = character.GetComponent<Character>();
        LineRenderer lr = character.GetComponent<LineRenderer>();

        //set to 0 to reset all
        lr.positionCount = 0;
        lr.positionCount = characterScript.queueTileIndex.Count + 1;
        lr.SetPosition(0, character.transform.position);

        for (int i = 0; i < characterScript.queueTileIndex.Count; ++i)
        {
            Vector2 linePos = GetPosFromTile(characterScript.queueTileIndex[i]);
            lr.SetPosition(i + 1, new Vector3(linePos.x, linePos.y, -0.53f));
        }

    }
    void ClearTrailPath(GameObject character)
    {
        Character characterScript = character.GetComponent<Character>();
        LineRenderer lr = character.GetComponent<LineRenderer>();

        characterScript.queueTileIndex.Clear();
        lr.positionCount = 0;

        selectedEntity = null;
    }

}