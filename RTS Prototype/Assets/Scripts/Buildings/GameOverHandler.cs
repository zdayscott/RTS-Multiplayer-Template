using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameOverHandler : NetworkBehaviour
{
    private List<UnitBase> _bases = new List<UnitBase>();

    public static event Action ServerOnGameOver;
    public static event Action<string> ClientOnGameOver;

    #region Server

    public override void OnStartServer()
    {
        UnitBase.ServerOnBaseSpawn += ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawn += ServerHandleBaseDespawned;    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnBaseSpawn -= ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawn -= ServerHandleBaseDespawned;
    }

    private void ServerHandleBaseSpawned(UnitBase unitBase)
    {
        _bases.Add(unitBase);
    }

    private void ServerHandleBaseDespawned(UnitBase unitBase)
    {
        _bases.Remove(unitBase);

        if (_bases.Count != 1) return;
        
        // On GameOver
        var winner = _bases[0].connectionToClient.identity.GetComponent<RTSPlayer>().GetDisplayName();
            
        RpcGameOver($"Player {winner}");
        
        ServerOnGameOver?.Invoke();
    }

    #endregion

    #region Client

    [ClientRpc]
    private void RpcGameOver(string winner)
    {
        ClientOnGameOver?.Invoke(winner);
    }

    #endregion
}
