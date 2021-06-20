using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] Hexagon hexagonPrefab;
    [SerializeField] HexagonBomb hexagonBombPrefab;

    public List<Hexagon> hexagons = new List<Hexagon>();
    public List<Hexagon> hexagonsToDestroy = new List<Hexagon>();
    public List<Hexagon> selectedHexagons = new List<Hexagon>();    

    private GameObject hexagonContainer;
    private GameObject hexagonBombContainer;

    private GameManagement gameManager;

    public int row = 9;
    public int column = 8;

    private int bombAmount = 0;
    public int bombAppearThreshold = 1000;
    public int bombEveryScore = 1000;

    bool isRotatingClockwise = true;

    void Awake()
    {
        gameManager = FindObjectOfType<GameManagement>();
        
        //Create Board
        CreateHexagons();
    }

    //Create hexagons in loops by columns and rows
    private void CreateHexagons()
    {
        hexagonContainer = new GameObject("Hexagons");
        hexagonBombContainer = new GameObject("HexagonBombs");

        for (int i = 0; i < column; i++)
        {
            if (i % 2 == 0)
            {
                for (int j = 0; j < row; j++)
                {
                    Hexagon hex = Instantiate(hexagonPrefab, Vector3.zero, Quaternion.identity);

                    hex.positionOnGrid = new Vector2(i + 1, j * 2 + 1);
                    hex.transform.position = hex.GridToWorldPosition();
                    hex.transform.parent = hexagonContainer.transform;
                    
                    hexagons.Add(hex);
                }
            }
            else
            {
                for (int j = 0; j < row; j++)
                {
                    Hexagon hex = Instantiate(hexagonPrefab, Vector3.zero, Quaternion.identity);

                    hex.positionOnGrid = new Vector2(i + 1, j * 2 + 2);
                    hex.transform.position = hex.GridToWorldPosition();
                    hex.transform.parent = hexagonContainer.transform;
                    
                    hexagons.Add(hex);                    
                }
            }
        }
    }

    private IEnumerator Start()
    {
        gameManager.isBusy = true;

        yield return new WaitForSeconds(0.8f);

        StartCoroutine(FindAllMatches());
    }

    //Find all 3 same color hexagons on board
    private IEnumerator FindAllMatches()
    {
        gameManager.isBusy = true;

        //Every hex in board send rays to check same color hexagons nearby
        foreach (var hex in hexagons)
        {
            hex.SendRays();
        }

        //If 3 or more hexagons to destroy, destroy them
        if (hexagonsToDestroy.Count > 2)
        {
            DestroyHexagons();

            yield return new WaitForSeconds(1f);

            //After destroy them, fill the empty spaces
            StartCoroutine(FillTheEmptySpaces());
        }
        else
        {
            //If no possible move left in game, end the game
            if (PossibleMoveLeft())
            {
                gameManager.isBusy = false;                
            }
            else
            {
                yield return new WaitForSeconds(0.3f);

                gameManager.EndGame();
            }
        }

    }

    //Add hexagons we touch on board to 'selectedHexagons' list
    public void SelectHexagons(Vector2[] positions)
    {
        //If there is already selected hexagons, remove them from list
        if (selectedHexagons.Count > 0)
        {
            foreach (var hex in selectedHexagons)
            {
                //Reset color of hexagon
                if (hex != null)
                {
                    hex.SetColor();             
                }
            }

            selectedHexagons.Clear();
        }

        //Add hexagons that found by gridPosition to 'selectedHexagons' list
        foreach (var position in positions)
        {
            Hexagon hexagon = hexagons.Where(hex => hex.positionOnGrid == position).FirstOrDefault();

            selectedHexagons.Add(hexagon);            
        }

        //Make selected hexagons more visible
        foreach (var hex in selectedHexagons)
        {
            hex.MakeHighlighted();
        }

    }

    //Fill empty spaces by shifting down existing ones and creating new ones
    public IEnumerator FillTheEmptySpaces()
    {
        gameManager.isBusy = true;

        /*  Shift down existing ones
            Find how many empty spaces of underneath of existing ones and --
            --decrease their y position of gridPositions amount of empty spaces
         */
        for (int i = 1; i <= column; i++)
        {
            List<Hexagon> hexList = hexagons.Where(hex => hex.positionOnGrid.x == i).ToList();            

            foreach(var hex in hexList)
            {
                List<Vector2> emptyGrids = 
                    hexagonsToDestroy.Where(grid => grid.positionOnGrid.x == i & grid.positionOnGrid.y < hex.positionOnGrid.y)
                                        .Select(grid => grid.positionOnGrid).ToList();

                if (emptyGrids.Count > 0)
                {
                    hex.positionOnGrid = new Vector2(i, hex.positionOnGrid.y - (emptyGrids.Count * 2));
                }
            }     

        }

        
        //Create new hexagons above shifted ones
        for (int i = 1; i <= column; i++)
        {
            List<Hexagon> emptySpacesByColumns = hexagonsToDestroy.Where(hex => hex.positionOnGrid.x == i).ToList();

            if (emptySpacesByColumns.Count > 0)
            {
                int newRow = i % 2 == 0 ? this.row * 2 : (this.row * 2) - 1;

                //If bombAmount is bigger than 0, instantiate 'bomb hexagon' instead of 'normal hexagon'
                foreach (var space in emptySpacesByColumns)
                {
                    Hexagon newHex;

                    if (bombAmount <= 0)
                    {
                        newHex = Instantiate(hexagonPrefab, new Vector3((i - 1) * 1.75f, newRow + 4, 0f), Quaternion.identity);                        
                        newHex.transform.parent = hexagonContainer.transform;
                    }
                    else
                    {
                        newHex = Instantiate(hexagonBombPrefab, new Vector3((i - 1) * 1.75f, newRow + 4, 0f), Quaternion.identity);
                        newHex.transform.parent = hexagonBombContainer.transform;
                        bombAmount--;
                    }

                    newHex.positionOnGrid = new Vector2(i, newRow);

                    hexagons.Add(newHex);

                    newRow -= 2;
                }
            }
                     
        }
        
        hexagonsToDestroy.Clear();

        yield return new WaitForSeconds(1f);

        StartCoroutine(FindAllMatches());
    }

    //Rotate hexagons by changing their gridPositions
    public void RotateSelectedHexagons()
    {
        Vector2 tempGridPos = selectedHexagons[0].positionOnGrid;

        //CLOCKWISE ROTATION
        if (isRotatingClockwise)
        {
            selectedHexagons[0].positionOnGrid = selectedHexagons[1].positionOnGrid;
            selectedHexagons[1].positionOnGrid = selectedHexagons[2].positionOnGrid;
            selectedHexagons[2].positionOnGrid = tempGridPos;            
        }
        //ANTI-CLOCKWISE ROTATION
        else
        {
            selectedHexagons[0].positionOnGrid = selectedHexagons[2].positionOnGrid;
            selectedHexagons[2].positionOnGrid = selectedHexagons[1].positionOnGrid;
            selectedHexagons[1].positionOnGrid = tempGridPos;
        }
    }

    public void DestroyHexagons()
    {
        if (selectedHexagons.Count > 0)
        {
            foreach (var hexagon in selectedHexagons)
            {
                hexagon.SetColor();            
            }

            selectedHexagons.Clear();
        }

        //Earn point for each destroyed hexagons 
        gameManager.EarnPoints(hexagonsToDestroy.Count);

        //If you reach the 'bombAppearThreshold' score, increase 'bombAmount' and 'bombAppearThreshold'
        if (gameManager.totalScore >= bombAppearThreshold)
        {
            bombAmount++;
            bombAppearThreshold += bombEveryScore;
        }
        
        //Remove destroyed ones and show VFX
        foreach (var hex in hexagonsToDestroy)
        {
            hexagons.Remove(hex);

            hex.ShowBreakingVFX();

            Destroy(hex.gameObject, 0.1f);            
        }

    }

    public IEnumerator Rotate()
    {
        gameManager.isBusy = true;

        for (int i = 0; i < 3; i++)
        {
            RotateSelectedHexagons();      
            
            yield return new WaitForSeconds(0.3f);
            
            //Check every rotation if there is 3 or more same color hexagon
            foreach (var hex in selectedHexagons)
            {
                hex.SendRays();
            }                     

            
            //if there is 3 or more same color hexagon
            if(hexagonsToDestroy.Count > 0)
            {                
                yield return new WaitForSeconds(0.7f);

                DestroyHexagons();

                yield return new WaitForSeconds(0.3f);

                //If there is 'hexagon bomb' on board, decrease its countdown
                if (hexagonBombContainer.transform.childCount > 0)
                {
                    foreach (Transform bomb in hexagonBombContainer.transform)
                    {
                        bomb.GetComponent<HexagonBomb>().CountDown();                        
                    }
                }

                StartCoroutine(FillTheEmptySpaces());

                //Destroyed same color hexagons, dont rotate anymore
                break;
            }  
        }      
          
        gameManager.isBusy = false;
    }

    public void RotateClockwise()
    {
        if (selectedHexagons.Count > 0 && !gameManager.isBusy)
        {
            isRotatingClockwise = true;
            StartCoroutine(Rotate());            
        }
    }

    public void RotateAntiClockwise()
    {
        if (selectedHexagons.Count > 0 && !gameManager.isBusy)
        {
            isRotatingClockwise = false;
            StartCoroutine(Rotate());            
        }
    }

    //Find 6 adjacent hexagons by gridPosition
    //Set conditions because hexagons can be on edge of the board
    private Vector2[] FindAdjacentHexagons(Vector2 gridPos)
    {
        Vector2 topHex = gridPos + Vector2.up * 2;
        Vector2 topRightHex = gridPos + Vector2.one;
        Vector2 bottomRightHex = gridPos + Vector2.down + Vector2.right;
        Vector2 bottomHex = gridPos + Vector2.down * 2;
        Vector2 bottomLeftHex = gridPos + Vector2.down + Vector2.left;
        Vector2 topLeftHex = gridPos + Vector2.left + Vector2.up;

        if (gridPos.x == 1)
        {
            if (gridPos.y == 1)
            {
                Vector2[] grids = {
                    topHex,
                    topRightHex,
                };                
                
                return grids;
            }
            else if(gridPos.y == (row * 2) - 1)
            {
                Vector2[] grids = {
                    topRightHex,
                    bottomRightHex,
                    bottomHex,
                };

                return grids;
            }
            else
            {
                Vector2[] grids = {
                    topHex,
                    topRightHex,
                    bottomRightHex,
                    bottomHex,
                };

                return grids;
            }

        }
        else if (gridPos.x == column)
        {
            if (column % 2 == 0 && gridPos.y == 2)
            {
                Vector2[] grids = {
                    topHex,
                    bottomLeftHex,
                    topLeftHex
                };

                return grids;
            }
            else if(column % 2 != 0 && gridPos.y == 1)
            {
                Vector2[] grids = {
                    topHex,
                    topLeftHex
                };

                return grids;
            }
            else if(column % 2 != 0 && gridPos.y == (row * 2) - 1)
            {
                Vector2[] grids = {
                    topLeftHex,
                    bottomLeftHex,
                    bottomHex,
                };

                return grids;
            }
            else if(column % 2 == 0 && gridPos.y == row * 2)
            {
                Vector2[] grids = {
                    bottomLeftHex,
                    bottomHex,
                };

                return grids;
            }
            else
            {
                Vector2[] grids = {
                    topHex,
                    bottomHex,
                    bottomLeftHex,
                    topLeftHex
                };

                return grids;                
            }
        }
        else if (gridPos.y == 1)
        {
            Vector2[] grids = {
                topHex,
                topRightHex,
                topLeftHex
            };

            return grids;
        }
        else if (gridPos.y == 2)
        {
            Vector2[] grids = {
                topHex,
                topRightHex,
                bottomRightHex,
                bottomLeftHex,
                topLeftHex
            };

            return grids;
        }
        else if (gridPos.y == row * 2)
        {
            Vector2[] grids = {
                bottomRightHex,
                bottomHex,
                bottomLeftHex,
            };

            return grids;
        }
        else if (gridPos.y == (row * 2) - 1)
        {
            Vector2[] grids = {
                topRightHex,
                bottomRightHex,
                bottomHex,
                bottomLeftHex,
                topLeftHex
            };

            return grids;
        }
        else
        {
            Vector2[] grids = {
                topHex,
                topRightHex,
                bottomRightHex,
                bottomHex,
                bottomLeftHex,
                topLeftHex
            };
            
            return grids;
        }



    }

    //Find joint hexagons of 2 hexagons
    //Set conditions because hexagons can be on edge of the board
    private List<Vector2> FindSharedHexagons(Vector2 centerHex, Vector2 otherHex)
    {
        List<Vector2> vectors = new List<Vector2>();
        
        if (centerHex.x == otherHex.x)
        {
            if(centerHex.y > otherHex.y)
            {
                Vector2 hex1 = centerHex + Vector2.down + Vector2.right;
                Vector2 hex2 = centerHex + Vector2.down + Vector2.left;

                vectors.Add(hex1);
                vectors.Add(hex2);

                if (hex1.x < 1 || hex1.x > column)
                {
                    vectors.Remove(hex1);
                }
                if (hex2.x < 1 || hex2.x > column)
                {
                    vectors.Remove(hex2);
                }
                
                return vectors;
            }
            else
            {
                Vector2 hex1 = centerHex + Vector2.up + Vector2.right;
                Vector2 hex2 = centerHex + Vector2.up + Vector2.left;

                vectors.Add(hex1);
                vectors.Add(hex2);

                if (hex1.x < 1 || hex1.x > column)
                {
                    vectors.Remove(hex1);
                }
                if (hex2.x < 1 || hex2.x > column)
                {
                    vectors.Remove(hex2);
                }
                
                return vectors;                
            }

        }
        else if(centerHex.x > otherHex.x)
        {
            if (centerHex.y > otherHex.y)
            {
                Vector2 hex1 = centerHex + Vector2.up + Vector2.left;
                Vector2 hex2 = centerHex + Vector2.down * 2;

                vectors.Add(hex1);
                vectors.Add(hex2);

                if (hex1.y < 1 || hex1.y > row * 2)
                {
                    vectors.Remove(hex1);
                }
                if (hex1.y < 1 || hex1.y > row * 2)
                {
                    vectors.Remove(hex2);
                }
                
                return vectors;
            }
            else
            {
                Vector2 hex1 = centerHex + Vector2.down + Vector2.left;
                Vector2 hex2 = centerHex + Vector2.up * 2;

                vectors.Add(hex1);
                vectors.Add(hex2);

                if (hex1.y < 1 || hex1.y > row * 2)
                {
                    vectors.Remove(hex1);
                }
                if (hex1.y < 1 || hex1.y > row * 2)
                {
                    vectors.Remove(hex2);
                }
                
                return vectors;
            }
        }
        else
        {
            if (centerHex.y > otherHex.y)
            {
                Vector2 hex1 = centerHex + Vector2.up + Vector2.right;
                Vector2 hex2 = centerHex + Vector2.down * 2;

                vectors.Add(hex1);
                vectors.Add(hex2);

                if (hex1.y < 1 || hex1.y > row * 2)
                {
                    vectors.Remove(hex1);
                }
                if (hex1.y < 1 || hex1.y > row * 2)
                {
                    vectors.Remove(hex2);
                }
                
                return vectors;
            }
            else
            {
                Vector2 hex1 = centerHex + Vector2.down + Vector2.right;
                Vector2 hex2 = centerHex + Vector2.up * 2;

                vectors.Add(hex1);
                vectors.Add(hex2);

                if (hex1.y < 1 || hex1.y > row * 2)
                {
                    vectors.Remove(hex1);
                }
                if (hex1.y < 1 || hex1.y > row * 2)
                {
                    vectors.Remove(hex2);
                }
                
                return vectors;
            }            
        }
    }

    /*  ->Go to every hexagon on board
            ->Find 6 adjacent hexagons of current hexagon
                ->If there is one in 6 hexagons, with same color as the current hexagon, find joint hexagons of these two
                    ->Find joint hexagons' 6 adjacent hexagons
                        ->If there is same color hexagon in 6 hexagons, as the current hexagon RETURN TRUE       
        -> RETURN FALSE 

    */
    private bool PossibleMoveLeft()
    {
        for (int i = 1; i <= column; i++)
        {
            if (i % 2 == 0)
            {
                for (int j = 2; j <= row * 2; j = j +2)
                {
                    Hexagon centerHexagon = hexagons.Where(hex => hex.positionOnGrid == new Vector2(i,j)).FirstOrDefault();

                    foreach (var grid in FindAdjacentHexagons(new Vector2(i, j)))
                    {
                        Hexagon otherHexagon = hexagons.Where(hex => hex.positionOnGrid == grid).FirstOrDefault();

                        if (otherHexagon.color == centerHexagon.color)
                        {
                            foreach (var sharedHex in 
                                    FindSharedHexagons(centerHexagon.positionOnGrid, otherHexagon.positionOnGrid))
                            {
                                
                                foreach (var hexGrid in FindAdjacentHexagons(sharedHex))
                                {
                                    if (hexGrid == centerHexagon.positionOnGrid || hexGrid == otherHexagon.positionOnGrid)
                                    {
                                        continue;
                                    }

                                    Hexagon hex = hexagons.Where(hex => hex.positionOnGrid == hexGrid).
                                                    FirstOrDefault();

                                    if (hex != null)
                                    {         
                                        if (hex.color == centerHexagon.color)
                                        {
                                            return true;                                            
                                        }

                                    }
                                }

                            }
                        }
                    }                   

                }                
            }
            else
            {
                for (int j = 1; j <= (row * 2) - 1; j = j + 2)
                {
                    
                    Hexagon centerHexagon = hexagons.Where(hex => hex.positionOnGrid == new Vector2(i,j)).FirstOrDefault();
                    List<Hexagon> adjacentHexagons = new List<Hexagon>();

                    foreach (var grid in FindAdjacentHexagons(new Vector2(i, j)))
                    {
                        Hexagon otherHexagon = hexagons.Where(hex => hex.positionOnGrid == grid).FirstOrDefault();

                        if (otherHexagon.color == centerHexagon.color)
                        {
                            foreach (var sharedHex in 
                                    FindSharedHexagons(centerHexagon.positionOnGrid, otherHexagon.positionOnGrid))
                            {
                                
                                foreach (var hexGrid in FindAdjacentHexagons(sharedHex))
                                {
                                    if (hexGrid == centerHexagon.positionOnGrid || hexGrid == otherHexagon.positionOnGrid)
                                    {
                                        continue;
                                    }

                                    Hexagon hex = hexagons.Where(hex => hex.positionOnGrid == hexGrid).
                                                    FirstOrDefault();

                                    if (hex != null)
                                    {         
                                        if (hex.color == centerHexagon.color)
                                        {
                                            return true;                                            
                                        }

                                    }
                                }

                            }
                        }
                    } 
                }                 
            }            
        }
        
        return false;
    }
}
