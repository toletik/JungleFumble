using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{    
    [SerializeField] int   length = 20;
    [SerializeField] int   height = 14;
    [SerializeField] float speed = 10;
    [SerializeField] int   touchDownLength = 0;

    [SerializeField] Camera cam = null;
    [SerializeField] GameObject map = null;
    [SerializeField] GameObject highlightTileParent = null;
    [SerializeField] GameObject gridParent = null;


    Transform selectedEntity = null;
    bool selectedEntityTryToMove = false;
    List<int> indexHighlightTiles = new List<int>();


    bool inPlayMode = false;
    [SerializeField] List<GameObject> characters = new List<GameObject>();
    uint scoreAllies = 0;
    uint scoreEnemies = 0;


    [SerializeField] GameObject ball = null;
    Vector3 ballinitialPos = Vector3.zero;
    Vector3 ballDestination = Vector3.zero;


    // Start is called before the first frame update
    void Start()
    {
        ballinitialPos = ball.transform.position;
        ballDestination = ball.transform.position;
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
        MoveCharacters();
        MoveBall();

        if (!isThereStillWaypoints() && (!ball.activeSelf || ballDestination == ball.transform.position))
            QuitPlayMode();
    }
    void TacticalMode()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ClearHighlightTiles();
            inPlayMode = true;
        }


        if (Input.GetMouseButtonDown(0))
            OnLeftClick();
        if (Input.GetMouseButtonDown(1))
            OnRightClick();
        if (Input.GetMouseButtonDown(2))
            OnMiddleClick();

    }




    //Playmode
    void MoveCharacters()
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
                    //Check if is in touchDown Zone
                    if (characterScript.hasBall && HasReachTouchDown(character))
                        TouchDown(character);
                    else
                    {
                        characterScript.queueTileIndex.RemoveAt(0);
                        UpdateTrailPath(character);
                    }
                }
                else
                    character.GetComponent<LineRenderer>().SetPosition(0, character.transform.position);

            }
        }
    }
    void MoveBall()
    {
        if (ball.activeSelf && ballDestination != ball.transform.position)
        {
            Vector3 direction = ballDestination - ball.transform.position;
            ball.transform.position += direction.normalized * (speed * Time.deltaTime);

            //if you pass the waypoint remove it
            if (Vector3.Dot(direction, ballDestination - ball.transform.position) < 0)
                ballDestination = ball.transform.position;
            else
                ball.GetComponent<LineRenderer>().SetPosition(0, ball.transform.position);
        }
    }
    bool isThereStillWaypoints()
    {
        foreach (GameObject character in characters)
            if (character.GetComponent<Character>().queueTileIndex.Count > 0)
                return true;

        return false;
    }
    void QuitPlayMode()
    {
        inPlayMode = false;

        foreach (GameObject character in characters)
            character.GetComponent<Character>().canPickUpBall = true;
    }
    //TacticalMode
    void OnLeftClick()
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);


        if (Physics.Raycast(ray, out hit))
        {
            //select Allies
            if (hit.transform.CompareTag("Allies"))
            {
                Character characterScript = hit.transform.GetComponent<Character>();

                //if not throwing already
                if (characterScript.canPickUpBall)
                {
                    //to check if display highlight tiles from character or last waypoint
                    selectedEntity = hit.transform;
                    selectedEntityTryToMove = true;
                    GenerateHighlightTiles(characterScript.queueTileIndex.Count == 0 ? GetTile(hit.point.x, hit.point.y) : characterScript.queueTileIndex[characterScript.queueTileIndex.Count - 1], characterScript.mvt - characterScript.queueTileIndex.Count, Color.blue);
                }
            }
            //select Enemies
            else if (hit.transform.CompareTag("Enemies"))
            {
                GenerateHighlightTiles(GetTile(hit.point.x, hit.point.y), hit.transform.GetComponent<Character>().mvt, Color.red);
                selectedEntity = null;
            }
            //select a Tile
            else
            {
                int tileIndex = GetTile(hit.point.x, hit.point.y);

                if (selectedEntity != null && indexHighlightTiles.Contains(tileIndex))
                {
                    if (selectedEntityTryToMove)
                        TileSelectMove(tileIndex);
                    else
                        TileSelectThrow(tileIndex);
                }
                else
                {
                    ClearHighlightTiles();
                    selectedEntity = null;
                }

            }

        }
    }
    void OnRightClick()
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);


        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.CompareTag("Allies"))
            {
                Character characterScript = hit.transform.GetComponent<Character>();

                //cancel the throw
                if (!characterScript.canPickUpBall)
                    CancelThrow(characterScript);
                //cancel Mvt
                else
                    ClearTrailPath(hit.transform.gameObject);
            }
        }

        ClearHighlightTiles();
    }
    void OnMiddleClick()
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);


        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.CompareTag("Allies"))
            {
                Character characterScript = hit.transform.GetComponent<Character>();

                //can Throw only if have ball AND dont already move
                if (characterScript.hasBall && characterScript.queueTileIndex.Count == 0)
                {
                    selectedEntity = hit.transform;
                    selectedEntityTryToMove = false;
                    GenerateHighlightTiles(GetTile(hit.transform.position.x, hit.transform.position.y), hit.transform.GetComponent<Character>().range, Color.yellow);
                }
            }
        }
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
    void GenerateHighlightTiles(int CentralTile, int stat, Color color)
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

                if (GetOffsetAllBetweenTiles(CentralTile, tileIndex) <= stat)
                {
                    indexHighlightTiles.Add(tileIndex);
                    GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    quad.transform.SetParent(highlightTileParent.transform);
                    quad.transform.localPosition = new Vector3(minX + l, minY + h, 0);
                    quad.GetComponent<Renderer>().material.color = color;
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
    void TileSelectMove(int tileIndex)
    {
        Character characterScript = selectedEntity.GetComponent<Character>();
        //check if move from Character or last Waypoint
        int referenceTile = characterScript.queueTileIndex.Count == 0 ? GetTile(selectedEntity.position.x, selectedEntity.position.y) : characterScript.queueTileIndex[characterScript.queueTileIndex.Count - 1];

        int offsetX = GetOffsetXBetweenTiles(referenceTile, tileIndex);
        int offsetY = GetOffsetYBetweenTiles(referenceTile, tileIndex);

        for (int i = 1; i < Mathf.Abs(offsetX) + 1; ++i)
        {
            characterScript.queueTileIndex.Add(referenceTile + ((offsetX > 0) ? i : -i));
        }

        //Reset reference tile because tiles maybe have been add
        referenceTile = characterScript.queueTileIndex.Count == 0 ? GetTile(selectedEntity.position.x, selectedEntity.position.y) : characterScript.queueTileIndex[characterScript.queueTileIndex.Count - 1];

        for (int i = 1; i < Mathf.Abs(offsetY) + 1; ++i)
        {
            characterScript.queueTileIndex.Add(referenceTile + length * ((offsetY > 0) ? i : -i));
        }

        GenerateHighlightTiles(characterScript.queueTileIndex.Count == 0 ? GetTile(selectedEntity.position.x, selectedEntity.position.y) : characterScript.queueTileIndex[characterScript.queueTileIndex.Count - 1], characterScript.mvt - characterScript.queueTileIndex.Count, Color.blue);
        UpdateTrailPath(selectedEntity.gameObject);
    }
    void TileSelectThrow(int tileIndex)
    {
        Vector2 tilePos = GetPosFromTile(tileIndex);
        ballDestination = new Vector3(tilePos.x, tilePos.y, ballinitialPos.z);

        Character characterScript = selectedEntity.GetComponent<Character>();
        characterScript.hasBall = false;
        characterScript.ballIcon.SetActive(false);
        characterScript.canPickUpBall = false;

        ball.SetActive(true);
        ball.transform.position = new Vector3(selectedEntity.transform.position.x, selectedEntity.transform.position.y, ballinitialPos.z);
        LineRenderer ballLR = ball.GetComponent<LineRenderer>();
        ballLR.positionCount = 2;
        ballLR.SetPosition(0, ball.transform.position);
        ballLR.SetPosition(1, ballDestination);

        ClearHighlightTiles();
    }
    //Cancel
    void CancelThrow(Character characterScript)
    {
        characterScript.hasBall = true;
        characterScript.ballIcon.SetActive(true);
        characterScript.canPickUpBall = true;

        ball.SetActive(false);
        ball.GetComponent<LineRenderer>().positionCount = 0;
    }
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
    //TouchDown
    bool HasReachTouchDown(GameObject character)
    {
        Character characterScript = character.GetComponent<Character>();
        float tileXValue = characterScript.queueTileIndex[0] % length;

        if ((tileXValue < touchDownLength && character.CompareTag("Enemies")) ||
            (tileXValue >= length - touchDownLength && character.CompareTag("Allies")) )
            return true;

        return false;
    }
    void TouchDown(GameObject character)
    {
        QuitPlayMode();

        Character characterScript = character.GetComponent<Character>();
        characterScript.hasBall = false;
        characterScript.ballIcon.SetActive(false);

        if (character.CompareTag("Allies"))
            scoreAllies++;
        else
            scoreEnemies++;

        Debug.Log("TOUCHDOWN !");
        Debug.Log("Score Allies : " + scoreAllies);
        Debug.Log("Score Enemies : " + scoreEnemies);

        foreach (GameObject chara in characters)
        {
            Character charaScript = chara.GetComponent<Character>();
            ClearTrailPath(chara);
            charaScript.queueTileIndex.Clear();
            chara.transform.position = charaScript.initialPos;
        }

        ball.transform.position = ballinitialPos;
        ballDestination = ball.transform.position;
        ball.SetActive(true);
    }


}