using System;
using System.Collections.Generic;
using Mirror;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class RTSNetworkManager : NetworkManager
{
    [SerializeField] private GameObject unitBasePrefab = null;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab;
    public List<RTSPlayer> players { get; } = new List<RTSPlayer>();

    private bool isGameInProgress = false;

    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;

    #region Server

    public override void OnServerConnect(NetworkConnection conn)
    {
        if(!isGameInProgress) return;
        
        conn.Disconnect();
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);
    
        var player = conn.identity.GetComponent<RTSPlayer>();
            
        players.Add(player);
        player.SetDisplayName($"Player {players.Count}");
            
        player.SetTeamColor(new Color(Random.Range(0f,1f),Random.Range(0f,1f),Random.Range(0f,1f)));
        
        player.SetPartyOwner(players.Count == 1);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        var player = conn.identity.GetComponent<RTSPlayer>();

        players.Remove(player);
        
        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        players.Clear();
        isGameInProgress = false;
    }

    public void StartGame()
    {
        if(players.Count < 2)return;

        isGameInProgress = true;
        
        ServerChangeScene("Scene_Map_01");
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
        {
            var gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);
                
            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

            foreach (var player in players)
            {
                var unitSpawnerInstance = Instantiate(unitBasePrefab, GetStartPosition().position, Quaternion.identity);
        
                NetworkServer.Spawn(unitSpawnerInstance, player.connectionToClient);
            }
        }
    }
    
    

    #endregion

    #region Client

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        ClientOnConnected?.Invoke();
    }
    
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        ClientOnDisconnected?.Invoke();
    }

    public override void OnStopClient()
    {
        players.Clear();
    }

    #endregion




}