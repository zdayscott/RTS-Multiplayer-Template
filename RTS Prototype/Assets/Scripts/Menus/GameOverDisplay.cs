using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class GameOverDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text winnerNameText;
    [SerializeField] private GameObject gameOverDisplay;
    
     void Start()
    {
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }
     private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

     public void LeaveGame()
     {
         if (NetworkServer.active && NetworkClient.isConnected)
         {
             NetworkManager.singleton.StopHost();
         }
         else
         {
             NetworkManager.singleton.StopClient();
         }
     }

    private void ClientHandleGameOver(string winner)
    {
        winnerNameText.text = $"{winner} has won!";
        
        gameOverDisplay.SetActive(true);
    }
}
