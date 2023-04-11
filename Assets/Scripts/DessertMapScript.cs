using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System;

public class DessertMapScript : MonoBehaviour
{
    [Header("Manager Scripts")]
    public battleManagerScript1 BMS;
    public gameManagerScript1 GMS;
    [Header("Tiles")]
    public TileTypes[] tileTypes;
    public int[,] tiles;
    [Header("Units on the board")]
    public GameObject unitsOnBoard;
    public GameObject[,] tilesOnMap;
    public GameObject[,] quadOnMap;
    public GameObject[,] quadOnMapForUnitMovementDisplay;
    public GameObject[,] quadOnMapCursor;

    public GameObject mapUI;
    public GameObject mapCursorUI;
    public GameObject mapUnitMovementUI;
    public List<Node> currentPath = null;

    public Node[,] graph;
    [Header("Containers")]
    public GameObject tileContainer;
    public GameObject UIQuadPotentialMovesContainer;
    public GameObject UIQuadCursorContainer;
    public GameObject UIUnitMovementPathContainer;
    [Header("Board Size")]
    public int mapSizeX;
    public int mapSizeY;
    [Header("Selected Unit Info")]
    public GameObject selectedUnit;
    public HashSet<Node> selectedUnitTotalRange;
    public HashSet<Node> selectedUnitMoveRange;
    public bool unitSelected = false;
    public int unitSelectedPreviousX;
    public int unitSelectedPreviousY;
    public GameObject previousOccupiedTile;
    public UnityEvent onGameFinished;


    [Header("Materials")]
    public Material greenUIMat;
    public Material redUIMat;
    public Material blueUIMat;
    private void Start()
    {
        generateMapInfo();
        generatePathFindingGraph();
        generateMapVisuals();
        setIfTileIsOccupied();
    }

    private void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            if (selectedUnit == null)
            {
                mouseClickToSelectUnitV2();

            }

            else if (selectedUnit.GetComponent<UnitScript1>().unitMoveState == selectedUnit.GetComponent<UnitScript1>().getMovementStateEnum(1) && selectedUnit.GetComponent<UnitScript1>().movementQueue.Count == 0)
            {


                if (selectTileToMoveTo())
                {
                    Debug.Log("movement path has been located");
                    unitSelectedPreviousX = selectedUnit.GetComponent<UnitScript1>().x;
                    unitSelectedPreviousY = selectedUnit.GetComponent<UnitScript1>().y;
                    previousOccupiedTile = selectedUnit.GetComponent<UnitScript1>().tileBeingOccupied;
                    moveUnit();

                    StartCoroutine(moveUnitAndFinalize());

                }

            }
            else if (selectedUnit.GetComponent<UnitScript1>().unitMoveState == selectedUnit.GetComponent<UnitScript1>().getMovementStateEnum(2))
            {
                finalizeOption();
            }

        }
        if (Input.GetMouseButtonDown(1))
        {
            if (selectedUnit != null)
            {
                if (selectedUnit.GetComponent<UnitScript1>().movementQueue.Count == 0 && selectedUnit.GetComponent<UnitScript1>().combatQueue.Count == 0)
                {
                    if (selectedUnit.GetComponent<UnitScript1>().unitMoveState != selectedUnit.GetComponent<UnitScript1>().getMovementStateEnum(3))
                    {
                        deselectUnit();
                    }
                }
                else if (selectedUnit.GetComponent<UnitScript1>().movementQueue.Count == 1)
                {
                    selectedUnit.GetComponent<UnitScript1>().visualMovementSpeed = 0.5f;
                }
            }
        }


    }

    public void generateMapInfo()
    {
        tiles = new int[mapSizeX, mapSizeY];
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                tiles[x, y] = 0;
            }
        }
    }

    public void generatePathFindingGraph()
    {
        graph = new Node[mapSizeX, mapSizeY];

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                graph[x, y] = new Node();
                graph[x, y].x = x;
                graph[x, y].y = y;
            }
        }
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                if (x > 0)
                {
                    graph[x, y].neighbours.Add(graph[x - 1, y]);
                }
                if (x < mapSizeX - 1)
                {
                    graph[x, y].neighbours.Add(graph[x + 1, y]);
                }
                if (y > 0)
                {
                    graph[x, y].neighbours.Add(graph[x, y - 1]);
                }
                if (y < mapSizeY - 1)
                {
                    graph[x, y].neighbours.Add(graph[x, y + 1]);
                }


            }
        }
    }

    public void generateMapVisuals()
    {
        tilesOnMap = new GameObject[mapSizeX, mapSizeY];
        quadOnMap = new GameObject[mapSizeX, mapSizeY];
        quadOnMapForUnitMovementDisplay = new GameObject[mapSizeX, mapSizeY];
        quadOnMapCursor = new GameObject[mapSizeX, mapSizeY];
        int index;
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                index = tiles[x, y];
                GameObject newTile = Instantiate(tileTypes[index].tileVisualPrefab, new Vector3(x, 0, y), Quaternion.identity);
                newTile.GetComponent<ClickableTileScript1>().tileX = x;
                newTile.GetComponent<ClickableTileScript1>().tileY = y;
                newTile.GetComponent<ClickableTileScript1>().map = this;
                newTile.transform.SetParent(tileContainer.transform);
                tilesOnMap[x, y] = newTile;


                GameObject gridUI = Instantiate(mapUI, new Vector3(x, 0.501f, y), Quaternion.Euler(90f, 0, 0));
                gridUI.transform.SetParent(UIQuadPotentialMovesContainer.transform);
                quadOnMap[x, y] = gridUI;

                GameObject gridUIForPathfindingDisplay = Instantiate(mapUnitMovementUI, new Vector3(x, 0.502f, y), Quaternion.Euler(90f, 0, 0));
                gridUIForPathfindingDisplay.transform.SetParent(UIUnitMovementPathContainer.transform);
                quadOnMapForUnitMovementDisplay[x, y] = gridUIForPathfindingDisplay;

                GameObject gridUICursor = Instantiate(mapCursorUI, new Vector3(x, 0.503f, y), Quaternion.Euler(90f, 0, 0));
                gridUICursor.transform.SetParent(UIQuadCursorContainer.transform);
                quadOnMapCursor[x, y] = gridUICursor;

            }
        }
    }

    public void moveUnit()
    {
        if (selectedUnit != null)
        {
            selectedUnit.GetComponent<UnitScript1>().MoveNextTile();
        }
    }

    public Vector3 tileCoordToWorldCoord(int x, int y)
    {
        return new Vector3(x, 0.75f, y);
    }

    public void setIfTileIsOccupied()
    {
        foreach (Transform team in unitsOnBoard.transform)
        {
            foreach (Transform unitOnTeam in team)
            {
                int unitX = unitOnTeam.GetComponent<UnitScript1>().x;
                int unitY = unitOnTeam.GetComponent<UnitScript1>().y;
                unitOnTeam.GetComponent<UnitScript1>().tileBeingOccupied = tilesOnMap[unitX, unitY];
                tilesOnMap[unitX, unitY].GetComponent<ClickableTileScript1>().unitOnTile = unitOnTeam.gameObject;
            }

        }
    }

    public void generatePathTo(int x, int y)
    {

        if (selectedUnit.GetComponent<UnitScript1>().x == x && selectedUnit.GetComponent<UnitScript1>().y == y)
        {
            currentPath = new List<Node>();
            selectedUnit.GetComponent<UnitScript1>().path = currentPath;

            return;
        }
        if (unitCanEnterTile(x, y) == false)
        {
            return;
        }

        selectedUnit.GetComponent<UnitScript1>().path = null;
        currentPath = null;
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();
        Node source = graph[selectedUnit.GetComponent<UnitScript1>().x, selectedUnit.GetComponent<UnitScript1>().y];
        Node target = graph[x, y];
        dist[source] = 0;
        prev[source] = null;
        List<Node> unvisited = new List<Node>();

        foreach (Node n in graph)
        {

            if (n != source)
            {
                dist[n] = Mathf.Infinity;
                prev[n] = null;
            }
            unvisited.Add(n);
        }
        while (unvisited.Count > 0)
        {
            Node u = null;
            foreach (Node possibleU in unvisited)
            {
                if (u == null || dist[possibleU] < dist[u])
                {
                    u = possibleU;
                }
            }


            if (u == target)
            {
                break;
            }

            unvisited.Remove(u);

            foreach (Node n in u.neighbours)
            {

                float alt = dist[u] + costToEnterTile(n.x, n.y);
                if (alt < dist[n])
                {
                    dist[n] = alt;
                    prev[n] = u;
                }
            }
        }
        if (prev[target] == null)
        {
            return;
        }
        currentPath = new List<Node>();
        Node curr = target;
        while (curr != null)
        {
            currentPath.Add(curr);
            curr = prev[curr];
        }
        currentPath.Reverse();

        selectedUnit.GetComponent<UnitScript1>().path = currentPath;

    }

    public float costToEnterTile(int x, int y)
    {

        if (unitCanEnterTile(x, y) == false)
        {
            return Mathf.Infinity;

        }

        TileTypes t = tileTypes[tiles[x, y]];
        float dist = t.movementCost;

        return dist;
    }

    public bool unitCanEnterTile(int x, int y)
    {
        if (tilesOnMap[x, y].GetComponent<ClickableTileScript1>().unitOnTile != null)
        {
            if (tilesOnMap[x, y].GetComponent<ClickableTileScript1>().unitOnTile.GetComponent<UnitScript1>().teamNum != selectedUnit.GetComponent<UnitScript1>().teamNum)
            {
                return false;
            }
        }
        return tileTypes[tiles[x, y]].isWalkable;
    }

    public void mouseClickToSelectUnit()
    {
        GameObject tempSelectedUnit;

        RaycastHit hit;
        Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);



        if (Physics.Raycast(ray, out hit))
        {
            if (unitSelected == false)
            {

                if (hit.transform.gameObject.CompareTag("Tile"))
                {
                    if (hit.transform.GetComponent<ClickableTileScript1>().unitOnTile != null)
                    {


                        tempSelectedUnit = hit.transform.GetComponent<ClickableTileScript1>().unitOnTile;
                        if (tempSelectedUnit.GetComponent<UnitScript1>().unitMoveState == tempSelectedUnit.GetComponent<UnitScript1>().getMovementStateEnum(0)
                            && tempSelectedUnit.GetComponent<UnitScript1>().teamNum == GMS.currentTeam
                            )
                        {
                            disableHighlightUnitRange();
                            selectedUnit = tempSelectedUnit;
                            selectedUnit.GetComponent<UnitScript1>().map = this;
                            selectedUnit.GetComponent<UnitScript1>().setMovementState(1);
                            unitSelected = true;

                            highlightUnitRange();
                        }
                    }
                }

                else if (hit.transform.parent != null && hit.transform.parent.gameObject.CompareTag("Unit"))
                {

                    tempSelectedUnit = hit.transform.parent.gameObject;
                    if (tempSelectedUnit.GetComponent<UnitScript1>().unitMoveState == tempSelectedUnit.GetComponent<UnitScript1>().getMovementStateEnum(0)
                          && tempSelectedUnit.GetComponent<UnitScript1>().teamNum == GMS.currentTeam
                        )
                    {

                        disableHighlightUnitRange();
                        selectedUnit = tempSelectedUnit;
                        selectedUnit.GetComponent<UnitScript1>().setMovementState(1);
                        selectedUnit.GetComponent<UnitScript1>().map = this;
                        unitSelected = true;

                        highlightUnitRange();
                    }
                }
            }

        }
    }


    public void finalizeMovementPosition()
    {
        tilesOnMap[selectedUnit.GetComponent<UnitScript1>().x, selectedUnit.GetComponent<UnitScript1>().y].GetComponent<ClickableTileScript1>().unitOnTile = selectedUnit;
        selectedUnit.GetComponent<UnitScript1>().setMovementState(2);

        highlightUnitAttackOptionsFromPosition();
        highlightTileUnitIsOccupying();
    }

    public void mouseClickToSelectUnitV2()
    {

        if (unitSelected == false && GMS.tileBeingDisplayed != null)
        {

            if (GMS.tileBeingDisplayed.GetComponent<ClickableTileScript1>().unitOnTile != null)
            {
                GameObject tempSelectedUnit = GMS.tileBeingDisplayed.GetComponent<ClickableTileScript1>().unitOnTile;
                if (tempSelectedUnit.GetComponent<UnitScript1>().unitMoveState == tempSelectedUnit.GetComponent<UnitScript1>().getMovementStateEnum(0)
                               && tempSelectedUnit.GetComponent<UnitScript1>().teamNum == GMS.currentTeam
                               )
                {
                    disableHighlightUnitRange();
                    selectedUnit = tempSelectedUnit;
                    selectedUnit.GetComponent<UnitScript1>().map = this;
                    selectedUnit.GetComponent<UnitScript1>().setMovementState(1);
                    unitSelected = true;
                    highlightUnitRange();

                }
            }
        }

    }

    public void finalizeOption()
    {

        RaycastHit hit;
        Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
        HashSet<Node> attackableTiles = getUnitAttackOptionsFromPosition();

        if (Physics.Raycast(ray, out hit))
        {

            if (hit.transform.gameObject.CompareTag("Tile"))
            {
                if (hit.transform.GetComponent<ClickableTileScript1>().unitOnTile != null)
                {
                    GameObject unitOnTile = hit.transform.GetComponent<ClickableTileScript1>().unitOnTile;
                    int unitX = unitOnTile.GetComponent<UnitScript1>().x;
                    int unitY = unitOnTile.GetComponent<UnitScript1>().y;

                    if (unitOnTile == selectedUnit)
                    {
                        disableHighlightUnitRange();
                        selectedUnit.GetComponent<UnitScript1>().wait();
                        selectedUnit.GetComponent<UnitScript1>().setMovementState(3);
                        deselectUnit();


                    }
                    else if (unitOnTile.GetComponent<UnitScript1>().teamNum != selectedUnit.GetComponent<UnitScript1>().teamNum && attackableTiles.Contains(graph[unitX, unitY]))
                    {
                        if (unitOnTile.GetComponent<UnitScript1>().currentHealthPoints > 0)
                        {
                            Debug.Log(selectedUnit.GetComponent<UnitScript1>().currentHealthPoints);
                            StartCoroutine(BMS.Attack(selectedUnit, unitOnTile));


                            StartCoroutine(deselectAfterMovements(selectedUnit, unitOnTile));
                        }
                    }
                }
            }
            else if (hit.transform.parent != null && hit.transform.parent.gameObject.CompareTag("Unit"))
            {
                GameObject unitClicked = hit.transform.parent.gameObject;
                int unitX = unitClicked.GetComponent<UnitScript1>().x;
                int unitY = unitClicked.GetComponent<UnitScript1>().y;

                if (unitClicked == selectedUnit)
                {
                    disableHighlightUnitRange();
                    selectedUnit.GetComponent<UnitScript1>().wait();
                    selectedUnit.GetComponent<UnitScript1>().setMovementState(3);
                    deselectUnit();

                }
                else if (unitClicked.GetComponent<UnitScript1>().teamNum != selectedUnit.GetComponent<UnitScript1>().teamNum && attackableTiles.Contains(graph[unitX, unitY]))
                {
                    if (unitClicked.GetComponent<UnitScript1>().currentHealthPoints > 0)
                    {
                        StartCoroutine(BMS.Attack(selectedUnit, unitClicked));
                        StartCoroutine(deselectAfterMovements(selectedUnit, unitClicked));
                    }
                }

            }
        }

    }

    public void deselectUnit()
    {

        if (selectedUnit != null)
        {
            if (selectedUnit.GetComponent<UnitScript1>().unitMoveState == selectedUnit.GetComponent<UnitScript1>().getMovementStateEnum(1))
            {
                disableHighlightUnitRange();
                disableUnitUIRoute();
                selectedUnit.GetComponent<UnitScript1>().setMovementState(0);


                selectedUnit = null;
                unitSelected = false;
            }
            else if (selectedUnit.GetComponent<UnitScript1>().unitMoveState == selectedUnit.GetComponent<UnitScript1>().getMovementStateEnum(3))
            {
                disableHighlightUnitRange();
                disableUnitUIRoute();
                unitSelected = false;
                selectedUnit = null;
            }
            else
            {
                disableHighlightUnitRange();
                disableUnitUIRoute();
                tilesOnMap[selectedUnit.GetComponent<UnitScript1>().x, selectedUnit.GetComponent<UnitScript1>().y].GetComponent<ClickableTileScript1>().unitOnTile = null;
                tilesOnMap[unitSelectedPreviousX, unitSelectedPreviousY].GetComponent<ClickableTileScript1>().unitOnTile = selectedUnit;

                selectedUnit.GetComponent<UnitScript1>().x = unitSelectedPreviousX;
                selectedUnit.GetComponent<UnitScript1>().y = unitSelectedPreviousY;
                selectedUnit.GetComponent<UnitScript1>().tileBeingOccupied = previousOccupiedTile;
                selectedUnit.transform.position = tileCoordToWorldCoord(unitSelectedPreviousX, unitSelectedPreviousY);
                selectedUnit.GetComponent<UnitScript1>().setMovementState(0);
                selectedUnit = null;
                unitSelected = false;
            }
        }
    }

    public void highlightUnitRange()
    {


        HashSet<Node> finalMovementHighlight = new HashSet<Node>();
        HashSet<Node> totalAttackableTiles = new HashSet<Node>();
        HashSet<Node> finalEnemyUnitsInMovementRange = new HashSet<Node>();

        int attRange = selectedUnit.GetComponent<UnitScript1>().attackRange;
        int moveSpeed = selectedUnit.GetComponent<UnitScript1>().moveSpeed;


        Node unitInitialNode = graph[selectedUnit.GetComponent<UnitScript1>().x, selectedUnit.GetComponent<UnitScript1>().y];
        finalMovementHighlight = getUnitMovementOptions();
        totalAttackableTiles = getUnitTotalAttackableTiles(finalMovementHighlight, attRange, unitInitialNode);

        foreach (Node n in totalAttackableTiles)
        {

            if (tilesOnMap[n.x, n.y].GetComponent<ClickableTileScript1>().unitOnTile != null)
            {
                GameObject unitOnCurrentlySelectedTile = tilesOnMap[n.x, n.y].GetComponent<ClickableTileScript1>().unitOnTile;
                if (unitOnCurrentlySelectedTile.GetComponent<UnitScript1>().teamNum != selectedUnit.GetComponent<UnitScript1>().teamNum)
                {
                    finalEnemyUnitsInMovementRange.Add(n);
                }
            }
        }


        highlightEnemiesInRange(totalAttackableTiles);
        highlightMovementRange(finalMovementHighlight);
        selectedUnitMoveRange = finalMovementHighlight;
        selectedUnitTotalRange = getUnitTotalRange(finalMovementHighlight, totalAttackableTiles);

    }


    public void disableUnitUIRoute()
    {
        foreach (GameObject quad in quadOnMapForUnitMovementDisplay)
        {
            if (quad.GetComponent<Renderer>().enabled == true)
            {

                quad.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    public HashSet<Node> getUnitMovementOptions()
    {
        float[,] cost = new float[mapSizeX, mapSizeY];
        HashSet<Node> UIHighlight = new HashSet<Node>();
        HashSet<Node> tempUIHighlight = new HashSet<Node>();
        HashSet<Node> finalMovementHighlight = new HashSet<Node>();
        int moveSpeed = selectedUnit.GetComponent<UnitScript1>().moveSpeed;
        Node unitInitialNode = graph[selectedUnit.GetComponent<UnitScript1>().x, selectedUnit.GetComponent<UnitScript1>().y];

        finalMovementHighlight.Add(unitInitialNode);
        foreach (Node n in unitInitialNode.neighbours)
        {
            cost[n.x, n.y] = costToEnterTile(n.x, n.y);
            if (moveSpeed - cost[n.x, n.y] >= 0)
            {
                UIHighlight.Add(n);
            }
        }

        finalMovementHighlight.UnionWith(UIHighlight);

        while (UIHighlight.Count != 0)
        {
            foreach (Node n in UIHighlight)
            {
                foreach (Node neighbour in n.neighbours)
                {
                    if (!finalMovementHighlight.Contains(neighbour))
                    {
                        cost[neighbour.x, neighbour.y] = costToEnterTile(neighbour.x, neighbour.y) + cost[n.x, n.y];
                        if (moveSpeed - cost[neighbour.x, neighbour.y] >= 0)
                        {
                            tempUIHighlight.Add(neighbour);
                        }
                    }
                }

            }

            UIHighlight = tempUIHighlight;
            finalMovementHighlight.UnionWith(UIHighlight);
            tempUIHighlight = new HashSet<Node>();

        }
        return finalMovementHighlight;
    }

    public HashSet<Node> getUnitTotalRange(HashSet<Node> finalMovementHighlight, HashSet<Node> totalAttackableTiles)
    {
        HashSet<Node> unionTiles = new HashSet<Node>();
        unionTiles.UnionWith(finalMovementHighlight);
        unionTiles.UnionWith(totalAttackableTiles);
        return unionTiles;
    }

    public HashSet<Node> getUnitTotalAttackableTiles(HashSet<Node> finalMovementHighlight, int attRange, Node unitInitialNode)
    {
        HashSet<Node> tempNeighbourHash = new HashSet<Node>();
        HashSet<Node> neighbourHash = new HashSet<Node>();
        HashSet<Node> seenNodes = new HashSet<Node>();
        HashSet<Node> totalAttackableTiles = new HashSet<Node>();
        foreach (Node n in finalMovementHighlight)
        {
            neighbourHash = new HashSet<Node>();
            neighbourHash.Add(n);
            for (int i = 0; i < attRange; i++)
            {
                foreach (Node t in neighbourHash)
                {
                    foreach (Node tn in t.neighbours)
                    {
                        tempNeighbourHash.Add(tn);
                    }
                }

                neighbourHash = tempNeighbourHash;
                tempNeighbourHash = new HashSet<Node>();
                seenNodes.UnionWith(neighbourHash);
            }
            totalAttackableTiles.UnionWith(seenNodes);
        }
        totalAttackableTiles.Add(unitInitialNode);

        return totalAttackableTiles;
    }



    public HashSet<Node> getUnitAttackOptionsFromPosition()
    {
        Node initialNode = graph[selectedUnit.GetComponent<UnitScript1>().x, selectedUnit.GetComponent<UnitScript1>().y];
        int attRange = selectedUnit.GetComponent<UnitScript1>().attackRange;

        HashSet<Node> attackableTiles = getUnitTotalAttackableTiles(new HashSet<Node> { initialNode }, attRange, initialNode);
        attackableTiles.Remove(initialNode);

        return attackableTiles;
    }

    public HashSet<Node> getTileUnitIsOccupying()
    {

        int x = selectedUnit.GetComponent<UnitScript1>().x;
        int y = selectedUnit.GetComponent<UnitScript1>().y;
        HashSet<Node> singleTile = new HashSet<Node>();
        singleTile.Add(graph[x, y]);
        return singleTile;

    }

    public void highlightTileUnitIsOccupying()
    {
        if (selectedUnit != null)
        {
            highlightMovementRange(getTileUnitIsOccupying());
        }
    }

    public void highlightUnitAttackOptionsFromPosition()
    {
        if (selectedUnit != null)
        {
            highlightEnemiesInRange(getUnitAttackOptionsFromPosition());
        }
    }

    public void highlightMovementRange(HashSet<Node> movementToHighlight)
    {
        foreach (Node n in movementToHighlight)
        {
            quadOnMap[n.x, n.y].GetComponent<Renderer>().material = blueUIMat;
            quadOnMap[n.x, n.y].GetComponent<MeshRenderer>().enabled = true;
        }
    }


    public void highlightEnemiesInRange(HashSet<Node> enemiesToHighlight)
    {
        foreach (Node n in enemiesToHighlight)
        {
            quadOnMap[n.x, n.y].GetComponent<Renderer>().material = redUIMat;
            quadOnMap[n.x, n.y].GetComponent<MeshRenderer>().enabled = true;
        }
    }


    public void disableHighlightUnitRange()
    {
        foreach (GameObject quad in quadOnMap)
        {
            if (quad.GetComponent<Renderer>().enabled == true)
            {
                quad.GetComponent<Renderer>().enabled = false;
            }
        }
    }


    public IEnumerator moveUnitAndFinalize()
    {
        disableHighlightUnitRange();
        disableUnitUIRoute();
        while (selectedUnit.GetComponent<UnitScript1>().movementQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }
        finalizeMovementPosition();
    }

    public IEnumerator deselectAfterMovements(GameObject unit, GameObject enemy)
    {

        selectedUnit.GetComponent<UnitScript1>().setMovementState(3);
        disableHighlightUnitRange();
        disableUnitUIRoute();

        yield return new WaitForSeconds(.25f);
        while (unit.GetComponent<UnitScript1>().combatQueue.Count > 0)
        {
            yield return new WaitForEndOfFrame();
        }
        while (enemy.GetComponent<UnitScript1>().combatQueue.Count > 0)
        {
            yield return new WaitForEndOfFrame();

        }
        deselectUnit();
    }

    public bool selectTileToMoveTo()
    {
        RaycastHit hit;
        Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {

            if (hit.transform.gameObject.CompareTag("Tile"))
            {

                int clickedTileX = hit.transform.GetComponent<ClickableTileScript1>().tileX;
                int clickedTileY = hit.transform.GetComponent<ClickableTileScript1>().tileY;
                Node nodeToCheck = graph[clickedTileX, clickedTileY];


                if (selectedUnitMoveRange.Contains(nodeToCheck))
                {
                    if ((hit.transform.gameObject.GetComponent<ClickableTileScript1>().unitOnTile == null || hit.transform.gameObject.GetComponent<ClickableTileScript1>().unitOnTile == selectedUnit) && (selectedUnitMoveRange.Contains(nodeToCheck)))
                    {
                        generatePathTo(clickedTileX, clickedTileY);
                        return true;
                    }
                }
            }

        }
        return false;
    }

    public static implicit operator tileMapScript(DessertMapScript v)
    {
        throw new NotImplementedException();
    }
}
