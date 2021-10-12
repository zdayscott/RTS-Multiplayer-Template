using System;
using Mirror;
using UnityEngine;

public class Building : NetworkBehaviour
{
    [SerializeField] private GameObject buildingPreview;
    [SerializeField] private Sprite icon = null;
    [SerializeField] private int id = -1;
    [SerializeField] private int price = 100;

    public GameObject GetPreview() => buildingPreview;
    public Sprite GetSprite() => icon;
    public int GetId() => id;
    public int GetPrice() => price;

    public static event Action<Building> ServerOnBuildingSpawn;
    public static event Action<Building> ServerOnBuildingDespawn;
    
    public static event Action<Building> AuthorityOnBuildingSpawn;
    public static event Action<Building> AuthorityOnBuildingDespawn;

    #region Server

    public override void OnStartServer()
    {
        ServerOnBuildingSpawn?.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerOnBuildingDespawn?.Invoke(this);  
        
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        AuthorityOnBuildingSpawn?.Invoke(this);
    }
    
    public override void OnStopClient()
    {
        if (!hasAuthority) return;

        AuthorityOnBuildingDespawn?.Invoke(this);
    }

    #endregion
}
