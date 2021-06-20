using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagement : MonoBehaviour
{
    [SerializeField] Text scoreText;
    [SerializeField] GameObject gameOverUI;
    [SerializeField] public HexagonColor[] colors;                                                                            


    private Camera cam;
    private BoardManager boardManager;

    public int totalScore = 0;
    private int hexagonScore = 5;

    public bool isBusy;

    private void Awake()
    {
        boardManager = FindObjectOfType<BoardManager>();    
        cam = Camera.main;
    }

    private void Start() 
    {
        //Set camera position and size
        float camXPos = ((boardManager.column - 1) * 1.75f) / 2;
        float camYPos = ((boardManager.row * 2) - 1) / 2;

        cam.transform.position = new Vector3(camXPos, camYPos, cam.transform.position.z);
        cam.orthographicSize = boardManager.row >= boardManager.column ? camYPos + 8 : camXPos * 2;
    }

    //Earn points for each destroyed blocks and show it on UI text
    public void EarnPoints(int amountOfBlocks)
    {
        totalScore += amountOfBlocks * hexagonScore;

        scoreText.text = totalScore.ToString();
    }

    //Show game over UI canvas
    public void EndGame()
    {
        isBusy = true;

        gameOverUI.SetActive(true);
    }

    //Reload game scene
    public void RestartGame()
    {
        StartCoroutine(LoadAsync());
    }

    //Load game scene async
    private IEnumerator LoadAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(0);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }        
    }


}
