using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class GameManager : MonoBehaviour
{
    [SerializeField] int length = 20;
    [SerializeField] int height = 14;
    [SerializeField] float speed = 10;
    [SerializeField] int touchDownLength = 0;
    [SerializeField] int nbOfPointsToWin = 0;
    [SerializeField] GameObject winScreen = null;
    [SerializeField] GameObject loseScreen = null;


    [SerializeField] Transform initialOffset = null;
    [SerializeField] Camera cam = null;
    [SerializeField] GameObject map = null;
    [SerializeField] GameObject highlightTileParent = null;
    [SerializeField] GameObject gridParent = null;
    [SerializeField] Material gridMat = null;


    GameObject selectedEntity = null;
    bool selectedEntityTryToMove = false;
    List<int> indexHighlightTiles = new List<int>();


    bool inPlayMode = false;
    [SerializeField] List<GameObject> allies = new List<GameObject>();
    [SerializeField] List<GameObject> enemies = new List<GameObject>();
    List<GameObject> allCharacters = new List<GameObject>();
    uint scoreAllies = 0;
    uint scoreEnemies = 0;


    [SerializeField] public GameObject characterCard = null;
    [SerializeField] GameObject ball = null;
    [SerializeField] Transform ballPlaymode = null;
    Vector3 ballinitialPos = Vector3.zero;
    Vector3 ballDestination = Vector3.zero;


    [SerializeField] GameObject pauseMenu = null;
    private bool isInPause = false;


    // sound var
    [SerializeField] string comfirmMovementSound = "";
    [SerializeField] string passSound = "";


    // Start is called before the first frame update
    void Start()
    {
        ballinitialPos = ball.transform.position;
        ballDestination = ball.transform.position;
        map.transform.localScale = new Vector3(length, height, 1);
        foreach (GameObject character in allies)
            allCharacters.Add(character);
        foreach (GameObject character in enemies)
            allCharacters.Add(character);
        GenerateGrid();
    }

    // Update is called once per frame
    void Update()
    {
        if (inPlayMode)
            PlayMode();
        else
            TacticalMode();

        if (Input.GetKeyDown(KeyCode.Escape))
            PauseMenu();




    }


    void PlayMode()
    {
        ResolveCharacterColision();
        MoveCharacters();
        MoveBall();
        
        //check if reach touchdown for ball reception while in touchdown zone
        foreach (GameObject character in allCharacters)
            if (character.GetComponent<Character>().hasBall)
                if (HasReachTouchDown(character))
                    TouchDown(character);

                if (!isThereStillWaypoints() && (!ball.activeSelf || ballDestination == ball.transform.position))
            QuitPlayMode();
    }
    void TacticalMode()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ClearHighlightTiles();
            StartPlayMode();
        }


        if (Input.GetMouseButtonDown(0))
            OnLeftClick();
        if (Input.GetMouseButtonDown(1))
            OnRightClick();
        if (Input.GetMouseButtonDown(2))
            OnMiddleClick();

    }




    //Playmode
    void StartPlayMode()
    {
        inPlayMode = true;
        SetAIWaypoints();
        if (ball.activeSelf && ball.transform.position != ballDestination)
            RuntimeManager.PlayOneShot(passSound);

    }
    GameObject GetClosestCharacterToTile(List<GameObject> characters, int tileIndex)
    {
        GameObject toReturn = null;
        int offsetMax = int.MaxValue;

        foreach (GameObject character in characters)
        {
            int possibleNewOffset = GetOffsetAllBetweenTiles(GetTile(character.transform.position.x, character.transform.position.y), tileIndex);

            if (possibleNewOffset < offsetMax)
            {
                toReturn = character;
                offsetMax = possibleNewOffset;
            }
        }

        return toReturn;
    }
    GameObject GetFarterCharacterToTile(List<GameObject> characters, int tileIndex)
    {
        GameObject toReturn = null;
        int offsetMax = 0;

        foreach (GameObject character in characters)
        {
            int possibleNewOffset = GetOffsetAllBetweenTiles(GetTile(character.transform.position.x, character.transform.position.y), tileIndex);

            if (possibleNewOffset > offsetMax)
            {
                toReturn = character;
                offsetMax = possibleNewOffset;
            }
        }

        return toReturn;
    }
    GameObject GetCharacterWithBall(List<GameObject> characters)
    {
        foreach (GameObject character in characters)
        {
            if (character.GetComponent<Character>().hasBall)
                return character;
        }

        return null;
    }
    void SetAIWaypoints()
    {
        List<GameObject> tempAllies = new List<GameObject>(allies);
        List<GameObject> tempEnemies = new List<GameObject>(enemies);

        GameObject allyWithBall = GetCharacterWithBall(tempAllies);
        GameObject enemyWithBall = GetCharacterWithBall(tempEnemies);

        //Rush to Ball
        if (ball.activeSelf)
        {
            int tileGoal = GetTile(ball.transform.position.x, ball.transform.position.y);
            GameObject closestEnemy = GetClosestCharacterToTile(tempEnemies, tileGoal);
            SetWaypoints(closestEnemy, tileGoal);

            tempAllies.Remove(GetFarterCharacterToTile(tempAllies, tileGoal));
            tempEnemies.Remove(closestEnemy);
        }
        //Rush to Ally with Ball
        else if (allyWithBall != null)
        {
            int tileGoal = GetTile(allyWithBall.transform.position.x, allyWithBall.transform.position.y);
            GameObject closestEnemy = GetClosestCharacterToTile(tempEnemies, tileGoal);
            SetWaypoints(closestEnemy, tileGoal);

            tempAllies.Remove(allyWithBall);
            tempEnemies.Remove(closestEnemy);
        }
        //Rush to TouchDown Zone
        else if (enemyWithBall != null)
        {
            //cast as int to maintain same row but column 0
            int tileGoal = (int)(GetTile(enemyWithBall.transform.position.x, enemyWithBall.transform.position.y) / length) * length;
            SetWaypoints(enemyWithBall, tileGoal);

            tempAllies.Remove(GetFarterCharacterToTile(tempAllies, tileGoal));
            tempEnemies.Remove(enemyWithBall);
        }
        else
            Debug.Log("AI dont find a Primary Goal");


        //Try to block 3 remaining allies
        foreach (GameObject ally in tempAllies)
        {
            int allyTileIndex = GetTile(ally.transform.position.x, ally.transform.position.y);
            GameObject closestEnemy = GetClosestCharacterToTile(tempEnemies, allyTileIndex);
            SetWaypoints(closestEnemy, allyTileIndex);
            tempEnemies.Remove(closestEnemy);
        }


    }
    void ResolveCharacterColision()
    {
        List<int> tempAllTilesIndex = new List<int>();

        //Check if two character want to access same tile or a character want to move to a tile of a static character
        foreach (GameObject character in allCharacters)
        {
            Character currentCharacterScript = character.GetComponent<Character>();
            int currentCharacterTile = 0;
            bool currentCharacterIsStatic = false;

            //get reference tile
            if (currentCharacterScript.queueTileIndex.Count == 0)
            {
                currentCharacterIsStatic = true;
                currentCharacterTile = GetTile(character.transform.position.x, character.transform.position.y);
            }
            else
                currentCharacterTile = currentCharacterScript.queueTileIndex[0];


            if (tempAllTilesIndex.Contains(currentCharacterTile))
            {
                Character otherCharacterScript = allCharacters[tempAllTilesIndex.IndexOf(currentCharacterTile)].GetComponent<Character>();

                //resolve colision
                {
                    //current is static so other stop path
                    if (currentCharacterIsStatic) 
                    {
                        ClearPath(allCharacters[tempAllTilesIndex.IndexOf(currentCharacterTile)]);
                        RuntimeManager.PlayOneShot(otherCharacterScript.blocSound);
                    }
                    //other is static so current stop path
                    else if (otherCharacterScript.queueTileIndex.Count == 0)
                    {
                        ClearPath(character);
                        RuntimeManager.PlayOneShot(currentCharacterScript.blocSound);
                    }
                    //if current is stronger he get the tile
                    else if (currentCharacterScript.strength > otherCharacterScript.strength) 
                    {
                        ClearPathAfterFirst(currentCharacterScript);
                        ClearPath(allCharacters[tempAllTilesIndex.IndexOf(currentCharacterTile)]);
                        RuntimeManager.PlayOneShot(currentCharacterScript.blocSound);
                    }
                    //if other is stronger he get the tile
                    else // other
                    {
                        ClearPathAfterFirst(otherCharacterScript);
                        ClearPath(character);
                        RuntimeManager.PlayOneShot(otherCharacterScript.blocSound);
                    }
                }

                //resolve ball
                if (currentCharacterScript.hasBall || otherCharacterScript.hasBall)
                {
                    bool isCurrentStronger = false;

                    if (currentCharacterScript.strength > otherCharacterScript.strength)
                        isCurrentStronger = true;
                    else
                        isCurrentStronger = false;

                    currentCharacterScript.hasBall = isCurrentStronger;
                    currentCharacterScript.ballIcon.SetActive(isCurrentStronger);
                    otherCharacterScript.hasBall = !isCurrentStronger;
                    otherCharacterScript.ballIcon.SetActive(!isCurrentStronger);
                }
                
            }

            tempAllTilesIndex.Add(currentCharacterTile);

        }
    }
    void MoveCharacters()
    { 

        foreach (GameObject character in allCharacters)
        {
            Character characterScript = character.GetComponent<Character>();

            if (characterScript.queueTileIndex.Count > 0)
            {
                //Apply Mvt to Tactical
                Vector2 tilePos = GetPosFromTile(characterScript.queueTileIndex[0]);
                Vector3 direction = new Vector3(tilePos.x, tilePos.y, character.transform.position.z) - character.transform.position;
                character.transform.position += direction.normalized * (speed * Time.deltaTime);

                //Apply Mvt to Playmode 
                if (characterScript.charactePlaymode != null)
                {
                    Vector3 directionPlaymode = new Vector3(direction.x, direction.z, direction.y);
                    characterScript.charactePlaymode.position += directionPlaymode.normalized * (speed * Time.deltaTime);
                }

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

            }
        }
    }
    void MoveBall()
    {
        if (ball.activeSelf && ballDestination != ball.transform.position)
        {
            //Apply to Tactical
            Vector3 direction = ballDestination - ball.transform.position;
            ball.transform.position += direction.normalized * (speed * Time.deltaTime);

            //Apply Mvt to Playmode 
            if (ballPlaymode != null)
            {
                Vector3 directionPlaymode = new Vector3(direction.x, direction.z, direction.y);
                ballPlaymode.position += directionPlaymode.normalized * (speed * Time.deltaTime);
            }


            //if you pass the waypoint remove it
            if (Vector3.Dot(direction, ballDestination - ball.transform.position) < 0)
                ballDestination = ball.transform.position;
            else
                ball.GetComponent<LineRenderer>().SetPosition(0, ball.transform.position);
        }
    }
    bool isThereStillWaypoints()
    {
        foreach (GameObject character in allCharacters)
            if (character.GetComponent<Character>().queueTileIndex.Count > 0)
                return true;

        return false;
    }
    void QuitPlayMode()
    {
        inPlayMode = false;

        foreach (GameObject character in allCharacters)
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

                characterCard.SetActive(true);
                characterCard.GetComponent<Renderer>().material = characterScript.characterCardMat;

                //if not throwing already
                if (characterScript.canPickUpBall)
                {
                    //to check if display highlight tiles from character or last waypoint
                    selectedEntity = hit.transform.gameObject;
                    selectedEntityTryToMove = true;
                    GenerateHighlightTiles(characterScript.queueTileIndex.Count == 0 ? GetTile(hit.point.x, hit.point.y) : characterScript.queueTileIndex[characterScript.queueTileIndex.Count - 1], characterScript.mvt - characterScript.queueTileIndex.Count, Color.blue);
                }
            }
            //select Enemies
            else if (hit.transform.CompareTag("Enemies"))
            {
                Character characterScript = hit.transform.GetComponent<Character>();
                characterCard.SetActive(true);
                characterCard.GetComponent<Renderer>().material = characterScript.characterCardMat;

                GenerateHighlightTiles(GetTile(hit.point.x, hit.point.y), hit.transform.GetComponent<Character>().mvt, Color.red);
                selectedEntity = null;
            }
            //select a Tile
            else
            {
                int tileIndex = GetTile(hit.point.x, hit.point.y);

                if (selectedEntity != null && indexHighlightTiles.Contains(tileIndex))
                {
                    RuntimeManager.PlayOneShot(comfirmMovementSound);

                    if (selectedEntityTryToMove)
                        TileSelectMove(selectedEntity, tileIndex);
                    else
                        TileSelectThrow(tileIndex);
                }
                else
                {
                    characterCard.SetActive(false);
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
                {
                    ClearPath(hit.transform.gameObject);
                    selectedEntity = null;
                }
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
                    selectedEntity = hit.transform.gameObject;
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
            line.transform.localScale = new Vector3(0.05f, height, 0.05f);
            line.transform.localPosition = new Vector3(minX + l, 0, 0);
            line.GetComponent<Renderer>().material = gridMat;
        }

        //Horizontal
        float minY = -0.5f * (height - 2);
        for (int h = 0; h < height - 1; h++)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Quad);
            line.transform.SetParent(gridParent.transform);
            line.transform.localScale = new Vector3(length, 0.05f, 0.05f);
            line.transform.localPosition = new Vector3(0, minY + h, 0);
            line.GetComponent<Renderer>().material = gridMat;
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
        x -= initialOffset.position.x;
        y -= initialOffset.position.y;

        return Mathf.RoundToInt(x + 0.5f * (length - 1)) + Mathf.RoundToInt(y + 0.5f * (height - 1)) * length;
    }
    Vector2 GetPosFromTile(int tileIndex)
    {
        return new Vector2(-0.5f * (length - 1) + tileIndex % length,
                           -0.5f * (height - 1) + (int)(tileIndex / length))
             + new Vector2(initialOffset.position.x, initialOffset.position.y);
    }
    int GetOffsetAllBetweenTiles(int tileReference, int tile2) { return Mathf.Abs(GetOffsetXBetweenTiles(tileReference, tile2)) + Mathf.Abs(GetOffsetYBetweenTiles(tileReference, tile2)); }
    int GetOffsetXBetweenTiles(int tileReference, int tile2) { return tile2 % length - tileReference % length;  }
    int GetOffsetYBetweenTiles(int tileReference, int tile2) { return (int)(tile2 / length) - (int)(tileReference / length); }
    void SetWaypoints(GameObject character, int tileDestination)
    {
        Character characterScript = character.transform.GetComponent<Character>();

        //check if move from Character or last Waypoint
        int tileReference = characterScript.queueTileIndex.Count == 0 ? GetTile(character.transform.position.x, character.transform.position.y) : characterScript.queueTileIndex[characterScript.queueTileIndex.Count - 1];

        int offsetX = GetOffsetXBetweenTiles(tileReference, tileDestination);
        int offsetY = GetOffsetYBetweenTiles(tileReference, tileDestination);

        for (int i = 1; i < Mathf.Abs(offsetY) + 1; ++i)
        {
            //security so enemies cant exceed their Mvt stat
            if (characterScript.queueTileIndex.Count >= characterScript.mvt)
                return;

            characterScript.queueTileIndex.Add(tileReference + length * ((offsetY > 0) ? i : -i));
        }

        //Reset reference tile because tiles maybe have been add
        tileReference = characterScript.queueTileIndex.Count == 0 ? GetTile(character.transform.position.x, character.transform.position.y) : characterScript.queueTileIndex[characterScript.queueTileIndex.Count - 1];

        for (int i = 1; i < Mathf.Abs(offsetX) + 1; ++i)
        {
            //security so enemies cant exceed their Mvt stat
            if (characterScript.queueTileIndex.Count >= characterScript.mvt)
                return;

            characterScript.queueTileIndex.Add(tileReference + ((offsetX > 0) ? i : -i));
        }
    
    }
    void TileSelectMove(GameObject character, int tileDestination)
    {
        SetWaypoints(character, tileDestination);

        Character characterScript = character.transform.GetComponent<Character>();
        GenerateHighlightTiles(characterScript.queueTileIndex.Count == 0 ? GetTile(character.transform.position.x, character.transform.position.y) : characterScript.queueTileIndex[characterScript.queueTileIndex.Count - 1], characterScript.mvt - characterScript.queueTileIndex.Count, Color.blue);
        UpdateTrailPath(character.transform.gameObject);
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
    void ClearPath(GameObject character)
    {
        Character characterScript = character.GetComponent<Character>();
        characterScript.queueTileIndex.Clear();

        LineRenderer lr = character.GetComponent<LineRenderer>();
        if (lr != null)
            lr.positionCount = 0;

    }
    void ClearPathAfterFirst(Character characterScript)
    {
        characterScript.queueTileIndex.RemoveRange(1, characterScript.queueTileIndex.Count - 1);

        LineRenderer lr = characterScript.gameObject.GetComponent<LineRenderer>();
        if (lr != null)
            lr.positionCount = 1;

    }
    //TouchDown
    bool HasReachTouchDown(GameObject character)
    {
        Character characterScript = character.GetComponent<Character>();
        float tileXValue = GetTile(character.transform.position.x, character.transform.position.y) % length;

        if ((tileXValue < touchDownLength && character.CompareTag("Enemies")) ||
            (tileXValue >= length - touchDownLength && character.CompareTag("Allies")) )
            return true;

        return false;
    }
    void TouchDown(GameObject character)
    {
        //update score
        if (character.CompareTag("Allies"))
            scoreAllies++;
        else
            scoreEnemies++;


        //Finish or reset pos
        if (scoreAllies >= nbOfPointsToWin)
            winScreen.SetActive(true);
        else if (scoreEnemies >= nbOfPointsToWin)
            loseScreen.SetActive(true);
        else
        {
            QuitPlayMode();

            Character characterScript = character.GetComponent<Character>();
            characterScript.hasBall = false;
            characterScript.ballIcon.SetActive(false);


            foreach (GameObject chara in allCharacters)
            {
                Character charaScript = chara.GetComponent<Character>();
                ClearPath(chara);
                charaScript.queueTileIndex.Clear();
                chara.transform.position = charaScript.initialPos;
            }

            ball.transform.position = ballinitialPos + new Vector3(character.CompareTag("Allies")? 1 : -1, 0, 0);
            ballDestination = ball.transform.position;
            ball.SetActive(true);
        }

    }

    public void PauseMenu()
	{
        if (!isInPause)
		{
            pauseMenu.SetActive(true);
            isInPause = true;
		}
        else
		{
            pauseMenu.SetActive(false);
            isInPause = false;
        }
            
	}


}