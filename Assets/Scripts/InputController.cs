using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{

    private Vector3 startPosition;
    private Camera cam;

    GameManagement gameManager;
    BoardManager boardManager;

    void Start()
    {
        cam = Camera.main;
        gameManager = FindObjectOfType<GameManagement>();
        boardManager = FindObjectOfType<BoardManager>();
    }

    void Update()
    {
        if (!gameManager.isBusy && boardManager.selectedHexagons.Count > 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                startPosition = cam.ScreenToWorldPoint(Input.mousePosition);    
            }

            if (Input.GetMouseButton(0))
            {
                if (startPosition.x - cam.ScreenToWorldPoint(Input.mousePosition).x > 2)
                {
                    boardManager.RotateAntiClockwise();
                }
                if (startPosition.x - cam.ScreenToWorldPoint(Input.mousePosition).x < -2)
                {
                    boardManager.RotateClockwise();
                }
            }            
        }        
    }
}
