using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class gameManagerScript : MonoBehaviour
{
   

    public TMP_Text currentTeamUI;
    public Canvas displayWinnerUI;
    public TMP_Text skillCooldownUI;

    public TMP_Text UIunitCurrentHealth;
    public TMP_Text UIunitAttackDamage;
    public TMP_Text UIunitAttackRange;
    public TMP_Text UIunitMoveSpeed;
    public TMP_Text UIunitName;
    public UnityEngine.UI.Image UIunitSprite;

    public Canvas UIunitCanvas;
    public GameObject playerPhaseBlock;
    private Animator playerPhaseAnim;
    private TMP_Text playerPhaseText;
   

    private Ray ray;
    private RaycastHit hit;
   

    public int numberOfTeams = 2;
    public int currentTeam;
    public GameObject unitsOnBoard;

    public GameObject team1;
    public GameObject team2;

    public GameObject unitBeingDisplayed;
    public GameObject tileBeingDisplayed;
    public bool displayingUnitInfo;

    public tileMapScript TMS;
    public DessertMapScript TMS1;

    //Cursor Info for tileMapScript
    public int cursorX;
    public int cursorY;
    public int selectedXTile;
    public int selectedYTile;

    //Variables for unitPotentialMovementRoute
    List<Node> currentPathForUnitRoute;
    List<Node> unitPathToCursor;

    public bool unitPathExists;
    public Material UICursor;

    public SkillButton SB;
    public int turn;
    public int turncount;

    public int routeToX;
    public int routeToY;


    public GameObject quadThatIsOneAwayFromUnit;

   
    public void Start()
    {
        currentTeam = 0;
        setCurrentTeamUI();
        teamHealthbarColorUpdate();
        displayingUnitInfo = false;
        playerPhaseAnim = playerPhaseBlock.GetComponent<Animator>();
        playerPhaseText = playerPhaseBlock.GetComponentInChildren<TextMeshProUGUI>();
        unitPathToCursor = new List<Node>();
        unitPathExists = false;       
      
        TMS = GetComponent<tileMapScript>();
        turn = 1;
        turncount = 0;


    }

    public void Update()
    {
        // Get mouse position
        ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            cursorUIUpdate();
            unitUIUpdate();
            SB.HideButton();

            if (TMS.selectedUnit != null && TMS.selectedUnit.GetComponent<UnitScript>().getMovementStateEnum(1) == TMS.selectedUnit.GetComponent<UnitScript>().unitMoveState)
            {
                SB.GetUnitData(TMS.selectedUnit.GetComponent<UnitScript>());
                setSkillCooldownUI(SB.GetSkillCooldown(turn));
                SB.SendCurrentTurn(turn);
                SB.RevealButton();

                if (TMS.selectedUnitMoveRange.Contains(TMS.graph[cursorX, cursorY]))
                {
                    if (cursorX != TMS.selectedUnit.GetComponent<UnitScript>().x || cursorY != TMS.selectedUnit.GetComponent<UnitScript>().y)
                    {
                        if (!unitPathExists && TMS.selectedUnit.GetComponent<UnitScript>().movementQueue.Count == 0)
                        {
                            unitPathToCursor = generateCursorRouteTo(cursorX, cursorY);
                            routeToX = cursorX;
                            routeToY = cursorY;

                            if (unitPathToCursor.Count != 0)
                            {
                                for (int i = 0; i < unitPathToCursor.Count; i++)
                                {
                                    int nodeX = unitPathToCursor[i].x;
                                    int nodeY = unitPathToCursor[i].y;

                                    GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
                                    quadToUpdate.GetComponent<Renderer>().enabled = true;

                                    if (i == 0)
                                    {
                                        quadToUpdate.GetComponent<Renderer>().material = UICursor;
                                    }
                                    else if (i != 0 && (i + 1) != unitPathToCursor.Count)
                                    {
                                        setCorrectRouteWithInputAndOutput(nodeX, nodeY, i);
                                    }
                                    else if (i == unitPathToCursor.Count - 1)
                                    {
                                        setCorrectRouteFinalTile(nodeX, nodeY, i);
                                    }
                                }
                            }
                            unitPathExists = true;
                        }
                        else if (routeToX != cursorX || routeToY != cursorY)
                        {
                            TMS.disableUnitUIRoute();
                            unitPathExists = false;
                        }
                    }
                    else if (cursorX == TMS.selectedUnit.GetComponent<UnitScript>().x && cursorY == TMS.selectedUnit.GetComponent<UnitScript>().y)
                    {
                        TMS.disableUnitUIRoute();
                        unitPathExists = false;
                    }
                }
            }
        }
    }

    public void setSkillCooldownUI(int skillcd)
    {
        skillCooldownUI.SetText((skillcd).ToString());
    }

    public void setCurrentTeamUI()
    {
        currentTeamUI.SetText("Team: " + (currentTeam+1).ToString());
    }


    public void switchCurrentPlayer()
    {
        resetUnitsMovements(returnTeam(currentTeam));
        currentTeam++;
        if (currentTeam == numberOfTeams)
        {
            currentTeam = 0;
        }

    }


    public GameObject returnTeam(int i)
    {
        GameObject teamToReturn = null;
        if (i == 0)
        {
            teamToReturn = team1;
        }
        else if (i == 1)
        {
            teamToReturn = team2;
        }
        return teamToReturn;
    }


    public void resetUnitsMovements(GameObject teamToReset)
    {
        foreach (Transform unit in teamToReset.transform)
        {
            unit.GetComponent<UnitScript>().moveAgain();
        }
    }


    public void endTurn()
    {
        if (TMS.selectedUnit == null)
        {
            switchCurrentPlayer();
            teamHealthbarColorUpdate();
            setCurrentTeamUI();
        }

        if (turncount == 0)
        {
            turncount++;
        }
        else if (turncount == 1)
        {
            turn++;
            turncount = 0;
            Debug.Log(turn);
        }
        //update units skill cooldown
        TMS.getUnitsAlive(turn);
    }


    public void checkIfUnitsRemain(GameObject unit, GameObject enemy)
    {

        StartCoroutine(checkIfUnitsRemainCoroutine(unit,enemy));
    }



    public void cursorUIUpdate()
    {
        if (hit.transform.CompareTag("Tile"))
        {
            updateCursorForTile(hit.transform.gameObject);
        }
        else if (hit.transform.CompareTag("Unit"))
        {
            updateCursorForUnit(hit.transform.parent.gameObject);
        }
        else
        {
            TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;
        }
    }

    private void updateCursorForTile(GameObject tile)
    {
        int tileX = tile.GetComponent<ClickableTileScript>().tileX;
        int tileY = tile.GetComponent<ClickableTileScript>().tileY;

        if (tileBeingDisplayed == null || tileBeingDisplayed != tile)
        {
            TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;

            selectedXTile = tileX;
            selectedYTile = tileY;
            cursorX = selectedXTile;
            cursorY = selectedYTile;
            TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
            tileBeingDisplayed = tile;
        }
    }

    private void updateCursorForUnit(GameObject unit)
    {
        int unitX = unit.GetComponent<UnitScript>().x;
        int unitY = unit.GetComponent<UnitScript>().y;
        GameObject occupiedTile = unit.GetComponent<UnitScript>().tileBeingOccupied;

        if (tileBeingDisplayed == null || tileBeingDisplayed != occupiedTile)
        {
            if (unit.GetComponent<UnitScript>().movementQueue.Count == 0)
            {
                TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;

                selectedXTile = unitX;
                selectedYTile = unitY;
                cursorX = selectedXTile;
                cursorY = selectedYTile;
                TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                tileBeingDisplayed = occupiedTile;
            }
        }
    }


    public void unitUIUpdate()
    {
        if (hit.transform.CompareTag("Unit"))
        {
            unitBeingDisplayed = hit.transform.parent.gameObject;
        }
        else if (hit.transform.CompareTag("Tile"))
        {
            unitBeingDisplayed = hit.transform.GetComponent<ClickableTileScript>().unitOnTile;
        }

        if (unitBeingDisplayed == null)
        {
            UIunitCanvas.enabled = false;
            displayingUnitInfo = false;
            return;
        }
        else
        {
            UIunitCanvas.enabled = true;
            displayingUnitInfo = true;
            var highlightedUnitScript = unitBeingDisplayed.GetComponent<UnitScript>();

            UIunitCurrentHealth.SetText(highlightedUnitScript.currentHealthPoints.ToString());
            UIunitAttackDamage.SetText(highlightedUnitScript.attackDamage.ToString());
            UIunitAttackRange.SetText(highlightedUnitScript.attackRange.ToString());
            UIunitMoveSpeed.SetText(highlightedUnitScript.moveSpeed.ToString());
            UIunitName.SetText(highlightedUnitScript.unitName);
            UIunitSprite.sprite = highlightedUnitScript.unitSprite;
        }
    }

    public void teamHealthbarColorUpdate()
    {
        for(int i = 0; i < numberOfTeams; i++)
        {
            GameObject team = returnTeam(i);
            if(team == returnTeam(currentTeam))
            {
                foreach (Transform unit in team.transform)
                {
                    unit.GetComponent<UnitScript>().changeHealthBarColour(0);
                }
            }
            else
            {
                foreach (Transform unit in team.transform)
                {
                    unit.GetComponent<UnitScript>().changeHealthBarColour(1);
                }
            }
        }
       
        
    }

    public List<Node> generateCursorRouteTo(int x, int y)
    {

        if (TMS.selectedUnit.GetComponent<UnitScript>().x == x && TMS.selectedUnit.GetComponent<UnitScript>().y == y)
        {
            currentPathForUnitRoute = new List<Node>();
            

            return currentPathForUnitRoute;
        }
        if (TMS.unitCanEnterTile(x, y) == false)
        {
            return null;
        }


        currentPathForUnitRoute = null;
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();
        Node source = TMS.graph[TMS.selectedUnit.GetComponent<UnitScript>().x, TMS.selectedUnit.GetComponent<UnitScript>().y];
        Node target = TMS.graph[x, y];
        dist[source] = 0;
        prev[source] = null;
        List<Node> unvisited = new List<Node>();


        foreach (Node n in TMS.graph)
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
                float alt = dist[u] + TMS.costToEnterTile(n.x, n.y);
                if (alt < dist[n])
                {
                    dist[n] = alt;
                    prev[n] = u;
                }
            }
        }
        if (prev[target] == null)
        {
            return null;
        }
        currentPathForUnitRoute = new List<Node>();
        Node curr = target;
        while (curr != null)
        {
            currentPathForUnitRoute.Add(curr);
            curr = prev[curr];
        }
        currentPathForUnitRoute.Reverse();

        return currentPathForUnitRoute;
    }

    public void resetQuad(GameObject quadToReset)
    {
        quadToReset.GetComponent<Renderer>().material = UICursor;
        quadToReset.transform.eulerAngles = new Vector3(90, 0, 0);
        
    }

    public Vector2 directionBetween(Vector2 currentVector, Vector2 nextVector)
    {

        
        Vector2 vectorDirection = (nextVector - currentVector).normalized;
       
        if (vectorDirection == Vector2.right)
        {
            return Vector2.right;
        }
        else if (vectorDirection == Vector2.left)
        {
            return Vector2.left;
        }
        else if (vectorDirection == Vector2.up)
        {
            return Vector2.up;
        }
        else if (vectorDirection == Vector2.down)
        {
            return Vector2.down;
        }
        else
        {
            Vector2 vectorToReturn = new Vector2();
            return vectorToReturn;
        }
    }
    
    public void setCorrectRouteWithInputAndOutput(int nodeX,int nodeY,int i)
    {
        Vector2 previousTile = new Vector2(unitPathToCursor[i - 1].x + 1, unitPathToCursor[i - 1].y + 1);
        Vector2 currentTile = new Vector2(unitPathToCursor[i].x + 1, unitPathToCursor[i].y + 1);
        Vector2 nextTile = new Vector2(unitPathToCursor[i + 1].x + 1, unitPathToCursor[i + 1].y + 1);

        Vector2 backToCurrentVector = directionBetween(previousTile, currentTile);
        Vector2 currentToFrontVector = directionBetween(currentTile, nextTile);


        if (backToCurrentVector == Vector2.right && currentToFrontVector == Vector2.right)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.right && currentToFrontVector == Vector2.up)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 180);
            quadToUpdate.GetComponent<Renderer>().enabled = true;

        }
        else if (backToCurrentVector == Vector2.right && currentToFrontVector == Vector2.down)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }

        else if (backToCurrentVector == Vector2.left && currentToFrontVector == Vector2.left)
        {

            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.left && currentToFrontVector == Vector2.up)
        {

            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.left && currentToFrontVector == Vector2.down)
        {

            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }

        else if (backToCurrentVector == Vector2.up && currentToFrontVector == Vector2.up)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.up && currentToFrontVector == Vector2.right)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.up && currentToFrontVector == Vector2.left)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.down && currentToFrontVector == Vector2.down)
        {

            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.down && currentToFrontVector == Vector2.right)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.down && currentToFrontVector == Vector2.left)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 180);
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
    }

    public void setCorrectRouteFinalTile(int nodeX,int nodeY,int i)
    {
        Vector2 previousTile = new Vector2(unitPathToCursor[i - 1].x + 1, unitPathToCursor[i - 1].y + 1);
        Vector2 currentTile = new Vector2(unitPathToCursor[i].x + 1, unitPathToCursor[i].y + 1);
        Vector2 backToCurrentVector = directionBetween(previousTile, currentTile);

        if (backToCurrentVector == Vector2.right)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.left)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            quadToUpdate.GetComponent<Renderer>().enabled = true;

        }
        else if (backToCurrentVector == Vector2.up)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.down)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 180);
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
    }
    


    public IEnumerator checkIfUnitsRemainCoroutine(GameObject unit, GameObject enemy)
    {
        while (unit.GetComponent<UnitScript>().combatQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }
        
        while (enemy.GetComponent<UnitScript>().combatQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }
        if (team1.transform.childCount == 0)
        {
            displayWinnerUI.enabled = true;
            displayWinnerUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Player 2 has won!");
           
            
        }
        else if (team2.transform.childCount == 0)
        {
            displayWinnerUI.enabled = true;
            displayWinnerUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Player 1 has won!");

          
        }
    }
    
    public void win()
    {
        displayWinnerUI.enabled = true;
        displayWinnerUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Winner!");

    }

  
   
}
