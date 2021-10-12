using System;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Unit : NetworkBehaviour
{
    [SerializeField] private int cost = 10;
    [SerializeField] private Health health;
    [SerializeField] private UnitMovement movement;
    [SerializeField] private Targeter targeter;
    
    [SerializeField] private UnityEvent onSelected;
    [SerializeField] private UnityEvent onDeselected;

    public static event Action<Unit> ServerOnUnitSpawn;
    public static event Action<Unit> ServerOnUnitDespawn;
    
    public static event Action<Unit> AuthorityOnUnitSpawn;
    public static event Action<Unit> AuthorityOnUnitDespawn;

    #region Getters

    public UnitMovement GetUnitMovement() => movement;
    public Targeter GetTargeter() => targeter;
    public int GetCost() => cost;

    #endregion

    #region Server

    public override void OnStartServer()
    {
        ServerOnUnitSpawn?.Invoke(this);
        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        ServerOnUnitDespawn?.Invoke(this);  
        
        health.ServerOnDie -= ServerHandleDie;
    }
    
    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }
    #endregion
    
    #region Client

    public override void OnStartAuthority()
    {
        AuthorityOnUnitSpawn?.Invoke(this);
    }
    
    public override void OnStopClient()
    {
        if (!hasAuthority) return;

        AuthorityOnUnitDespawn?.Invoke(this);
    }

    [Client]
    public void Select()
    {
        if(!hasAuthority) return;
        onSelected?.Invoke();
    }
    
    [Client]
    public void Deselect()
    {
        if (!hasAuthority) return;
        onDeselected?.Invoke();
    }
    
 

    #endregion
}