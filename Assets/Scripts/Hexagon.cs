using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HexagonColor
{
    Red, Blue, Yellow, Green, Purple, Cyan, White
}

public class Hexagon : MonoBehaviour
{
    [SerializeField] ParticleSystem destroyVFX;
    
    public HexagonColor color;
    public Vector2 positionOnGrid;
    
    protected GameManagement gameManager;
    private BoardManager boardManager;
    private SpriteRenderer spriteRenderer;
    private float colorAlpha = 0.75f;
    private float speed = 10f;
    private bool isOnPosition;

    private const float COLUMN_STEP = 1.75f;
    private const float ROW_STEP = 1;

    private void Awake() 
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameManager = FindObjectOfType<GameManagement>();
        RandomColor();
    }

    public void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
    }

    private void Update()
    {
        // Checking 'world position' if it's on right position according to 'grid position'
        if (!IsOnPosition())
        {
            // If it is not, move it to right 'world position'
            MoveToPosition();
        }
    }

    //Set random color to hexagon
    public void RandomColor()
    {
        int randomNum = Random.Range(0, gameManager.colors.Length);
        color = gameManager.colors[randomNum];

        SetColor();
    }

    //Get RGB color from HexagonColor enum
    public Color GetColor()
    {
        Color color;

        switch (this.color)
            {
                case HexagonColor.Red:
                    color = new Color(1f, 0f, 0f, colorAlpha);
                    break;
                case HexagonColor.Blue:
                    color = new Color(0f, 0f, 1f, colorAlpha);
                    break;
                case HexagonColor.Yellow:
                    color = new Color(0.74f, 0.75f, 0.15f, colorAlpha);
                    break;
                case HexagonColor.Green:
                    color = new Color(0.14f, 0.52f, 0.11f, colorAlpha);
                    break;
                case HexagonColor.Purple:
                    color = new Color(1f, 0.1f, 1f, colorAlpha);
                    break;
                case HexagonColor.Cyan:
                    color = new Color(0f, 0.89f, 1f, colorAlpha);
                    break;
                case HexagonColor.White:
                    color = new Color(0.8f, 0.8f, 0.8f, colorAlpha);
                    break;
                default:
                    color = Color.white;
                    break;
            }    

        return color;
    }

    //Set RGB color
    public void SetColor()
    {
        spriteRenderer.color = GetColor();
    }

    //Convert 'grid position' to 'world position'
    public Vector3 GridToWorldPosition()
    {
        float yPos = positionOnGrid.y - ROW_STEP;
        float xPos = (positionOnGrid.x - 1) * COLUMN_STEP;
        
        return new Vector3(xPos, yPos, 0f);
    }

    //If it's available, pick nearest 3 hexagon (including itself)
    private void OnMouseUp()
    {
        if(!gameManager.isBusy)
        {
            Pick3Hexagons();
        }
    }

    //Pick nearest 3 hexagons by using angle
    private void Pick3Hexagons()
    {
        //Finding angle between 'center of hexagon' and 'mouse position'
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mousePos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0f);
        Vector3 vectorBetweenMouseAndHex = mousePos - transform.position;

        float angle = Vector3.SignedAngle(transform.up, vectorBetweenMouseAndHex, Vector3.forward);

        //
        //RIGHT CORNER
        //
        if (angle > -120f && angle < -60f &&
                positionOnGrid.y > 1 && positionOnGrid.x < boardManager.column && positionOnGrid.y < boardManager.row * 2)
        {
            Vector2[] adjacentHexagons = {new Vector2(positionOnGrid.x + 1, positionOnGrid.y + 1),
                                            new Vector2(positionOnGrid.x + 1, positionOnGrid.y - 1),
                                                positionOnGrid};

            boardManager.SelectHexagons(adjacentHexagons);
        }
        //
        //BOTTOM RIGHT CORNER
        //
        else if (angle > -180f && angle < -120f &&
                    positionOnGrid.y > 2 && positionOnGrid.x < boardManager.column)
        {
            Vector2[] adjacentHexagons = {new Vector2(positionOnGrid.x + 1, positionOnGrid.y - 1),
                                            new Vector2(positionOnGrid.x, positionOnGrid.y - 2),
                                                positionOnGrid};

            boardManager.SelectHexagons(adjacentHexagons);
        }
        //
        //BOTTOM LEFT CORNER
        //
        else if (angle < 180f && angle > 120f &&
                    positionOnGrid.x > 1 && positionOnGrid.y > 2)
        {
            Vector2[] adjacentHexagons = {new Vector2(positionOnGrid.x, positionOnGrid.y - 2),
                                            new Vector2(positionOnGrid.x - 1, positionOnGrid.y - 1),
                                                positionOnGrid};

            boardManager.SelectHexagons(adjacentHexagons);
        }
        //
        //LEFT CORNER
        //     
        else if (angle < 120f && angle > 60f &&
                    positionOnGrid.x > 1 && positionOnGrid.y > 1 && positionOnGrid.y < boardManager.row * 2)
        {
            Vector2[] adjacentHexagons = {new Vector2(positionOnGrid.x - 1, positionOnGrid.y - 1),
                                            new Vector2(positionOnGrid.x - 1, positionOnGrid.y + 1),
                                                positionOnGrid};

            boardManager.SelectHexagons(adjacentHexagons);
        }
        //
        //TOP LEFT CORNER
        //         
        else if (angle < 60f && angle > 0f &&
                    positionOnGrid.x > 1 && positionOnGrid.y + 1 < boardManager.row * 2)
        {
            Vector2[] adjacentHexagons = {new Vector2(positionOnGrid.x - 1, positionOnGrid.y + 1),
                                            new Vector2(positionOnGrid.x, positionOnGrid.y + 2),
                                                positionOnGrid};

            boardManager.SelectHexagons(adjacentHexagons);
        }
        //
        //TOP RIGHT CORNER
        // 
        else if (angle < 0f && angle > -60f &&
                    positionOnGrid.x < boardManager.column && positionOnGrid.y + 1 < boardManager.row * 2)
        {
            Vector2[] adjacentHexagons = {new Vector2(positionOnGrid.x, positionOnGrid.y + 2),
                                            new Vector2(positionOnGrid.x + 1, positionOnGrid.y + 1),
                                                positionOnGrid};

            boardManager.SelectHexagons(adjacentHexagons);
        }
    }

    //Send rays to 6 directions
    public void SendRays()
    {        
        Vector3[,] corners =    {/* TOP RIGHT CORNER */
                                {Vector3.up, (Vector3.right * 1.73f + Vector3.up).normalized},
                                /* RIGHT CORNER */
                                {(Vector3.right * 1.73f + Vector3.up).normalized, (Vector3.right * 1.73f + Vector3.down).normalized},
                                /* BOTTOM RIGHT CORNER */
                                {(Vector3.right * 1.73f + Vector3.down).normalized, Vector3.down},
                                /* BOTTOM LEFT CORNER */
                                {Vector3.down, (Vector3.left * 1.73f + Vector3.down).normalized},
                                /* LEFT CORNER */
                                {(Vector3.left * 1.73f + Vector3.down).normalized, (Vector3.left * 1.73f + Vector3.up).normalized},
                                /* TOP LEFT CORNER */
                                {(Vector3.left * 1.73f + Vector3.up).normalized, Vector3.up},
                                };
        

        List<Hexagon> sameColorHexagons = new List<Hexagon>();

        //Send 2 rays to 2 hexagons that is neighbors of each corner
        for (int i = 0; i < 6; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position + corners[i,0] * 2f, corners[i,0], 0.1f);            
            RaycastHit2D hit2 = Physics2D.Raycast(transform.position + corners[i,1] * 2f, corners[i,1], 0.1f);

            //If corner has 2 neighbor hexagons
            if (hit.collider != null && hit2.collider != null)
            {
                Hexagon hex = hit.transform.GetComponent<Hexagon>();
                Hexagon hex2 = hit2.transform.GetComponent<Hexagon>();

                //If these 2 neighbor hexagons have same color with main hexagon, add them to 'sameColorHexagons' list (including main)
                if (hex.color == hex2.color && color == hex2.color)
                {
                    if (!sameColorHexagons.Contains(hex))
                    {
                        sameColorHexagons.Add(hex);                            
                    }
                    if (!sameColorHexagons.Contains(hex2))
                    {
                        sameColorHexagons.Add(hex2);                            
                    }
                    if (!sameColorHexagons.Contains(this))
                    {
                        sameColorHexagons.Add(this);                                            
                    }
                }
            }            
        }

        //If there are 3 or more same color hexagons, add them to 'hexagonsToDestroy' list
        if (sameColorHexagons.Count > 2)
        {
            foreach (var hex in sameColorHexagons)
            {         
                if (!boardManager.hexagonsToDestroy.Contains(hex))
                {
                    boardManager.hexagonsToDestroy.Add(hex);                    
                }
            }
        }

    }

    //Move hexagon to true world position 
    public void MoveToPosition()
    {        
        Vector3 targetPosition = GridToWorldPosition();
        
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);
        
        //If distance is smaller than 0.1f, snap it to targetPosition
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            transform.position = targetPosition;
        }

    }

    //Check if it's true world position
    public bool IsOnPosition()
    {
        Vector3 targetPosition = GridToWorldPosition();
        
        if(transform.position == targetPosition)
        {
            return true;
        }    

        return false;    
    }

    //Instantiates VFX and sets it's color hexagon's color
    public void ShowBreakingVFX()
    {
        ParticleSystem vfx = Instantiate(destroyVFX, transform.position, Quaternion.identity);       
        ParticleSystem.MainModule settings = vfx.main;
        settings.startColor = GetColor();

        Destroy(vfx.gameObject, 1f);
    }

    //When hexagons is selected, make it highlighted
    public void MakeHighlighted()
    {
        spriteRenderer.color += new Color(0.2f, 0.2f, 0.2f, 1f);
    }
}
