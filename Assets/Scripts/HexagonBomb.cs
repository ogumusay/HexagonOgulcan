using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexagonBomb : Hexagon
{
    [SerializeField] Text countdownText;
    public int countdown = 7;

    new void Start()
    {
        countdownText.text = countdown.ToString();
        base.Start();
    }

    //If countdown hits 0, end game
    public void CountDown()
    {
        countdown--;
        countdownText.text = countdown.ToString();
        if (countdown <= 0)
        {
            gameManager.EndGame();
        }
    }

}
