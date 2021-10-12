using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security;
using Mirror;
using UnityEngine;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Building[] _buildings = new Building[0];
    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] private float buildingRangeLimit = 5f;

    [SyncVar(hook = nameof(ClientHandleResourceUpdated))] private int resources = 500;

    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
    private bool isPartyOwner = false;

    [SyncVar(hook = nameof(ClientHandleUpdateDisplayName))] private string displayName;

    public int GetResources() => resources;
    public bool GetIsPartyOwner() => isPartyOwner;
    public string GetDisplayName() => displayName;

    public event Action<int> ClientOnResourcesUpdated;
    public static event Action<bool> AuthorityOnPartyOwnerStatUpdated;
    public static event Action ClientOnInfoUpdated;

    private Color teamColor = new Color();
    private List<Unit> myUnits = new List<Unit>();
    private List<Building> myBuildings = new List<Building>();


    public List<Unit> GetMyUnits() => myUnits;
    public Color GetTeamColor() => teamColor;
    public Transform GetCameraTransform() => cameraTransform;

    public bool CanPlaceBuilding(BoxCollider boxCollider, Vector3 point)
    {
        if (Physics.CheckBox(point + boxCollider.center, boxCollider.size / 2, Quaternion.identity,
            buildingBlockLayer))
        {
            return false;
        }

        return myBuildings.Any(building => (point - building.transform.position).sqrMagnitude <= buildingRangeLimit * buildingRangeLimit);
    }

    #region Server

        public override void OnStartServer()
        {
            Unit.ServerOnUnitSpawn += ServerHandleUnitSpawned;
            Unit.ServerOnUnitDespawn += ServerHandleUnitDespawned;
            Building.ServerOnBuildingSpawn += ServerHandleBuildingSpawned;
            Building.ServerOnBuildingDespawn += ServerHandleBuildingDespawned;
            
            DontDestroyOnLoad(gameObject);
        }
    
        public override void OnStopServer()
        {
            Unit.ServerOnUnitSpawn -= ServerHandleUnitSpawned;
            Unit.ServerOnUnitDespawn -= ServerHandleUnitDespawned;   
            Building.ServerOnBuildingSpawn -= ServerHandleBuildingSpawned;
            Building.ServerOnBuildingDespawn -= ServerHandleBuildingDespawned;
        }
        
        [Server]
        public void SetResources(int newResources)
        {
            resources = newResources;
        }

        [Server]
        public void SetTeamColor(Color newTeamColor)
        {
            teamColor = newTeamColor;
        }

        [Server]
        public void SetPartyOwner(bool state)
        {
            isPartyOwner = state;
        }

        [Server]
        public void SetDisplayName(string newName)
        {
            displayName = newName;
        }
    
        private void ServerHandleUnitSpawned(Unit unit)
        {
            if (unit.connectionToClient.connectionId != connectionToClient.connectionId) return;
            
            myUnits.Add(unit);
        }
        
        private void ServerHandleUnitDespawned(Unit unit)
        {
            if (unit.connectionToClient.connectionId != connectionToClient.connectionId) return;
            
            myUnits.Remove(unit);
        }
        
        private void ServerHandleBuildingSpawned(Building building)
        {
            if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;
            
            myBuildings.Add(building);
        }
        
        private void ServerHandleBuildingDespawned(Building building)
        {
            if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;
            
            myBuildings.Remove(building);
        }

        [Command]
        public void CmdTryPlaceBuilding(int buildingId, Vector3 point)
        {
            // Validate and place
            var buildingToPlace = _buildings.FirstOrDefault(building => building.GetId() == buildingId);

            if (buildingToPlace is null) return;

            if (resources < buildingToPlace.GetPrice())  return;

            var buildingCollider = buildingToPlace.GetComponent<BoxCollider>();

            if(!CanPlaceBuilding(buildingCollider, point)) return;

            var buildingInstance = Instantiate(buildingToPlace.gameObject, point, buildingToPlace.transform.rotation);
            NetworkServer.Spawn(buildingInstance, connectionToClient);
            
            SetResources(resources - buildingToPlace.GetPrice());
        }

        [Command]
        public void CmdStartGame()
        {
            if(!isPartyOwner) return;
            
            ((RTSNetworkManager) NetworkManager.singleton).StartGame();
        }

        #endregion

    #region Client

    public override void OnStartClient()
    {
        if(NetworkServer.active)return;
        
        DontDestroyOnLoad(gameObject);
        
        ((RTSNetworkManager)NetworkManager.singleton).players.Add(this);
    }

    public override void OnStartAuthority()
    {
        if(NetworkServer.active) return;

        Unit.AuthorityOnUnitSpawn += AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawn += AuthorityHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawn += AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawn += AuthorityHandleBuildingDespawned;
    }

    public override void OnStopClient()
    {
        ClientOnInfoUpdated?.Invoke();

        if(!isClientOnly) return;   

        ((RTSNetworkManager)NetworkManager.singleton).players.Add(this);

        if(!hasAuthority) return;
        
        Unit.AuthorityOnUnitSpawn -= AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawn -= AuthorityHandleUnitDespawned;  
        Building.AuthorityOnBuildingSpawn -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawn -= AuthorityHandleBuildingDespawned;
    }
    
    private void AuthorityHandleUnitSpawned(Unit unit)
    {
        myUnits.Add(unit);
    }
        
    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        myUnits.Remove(unit);
    }
    
    private void AuthorityHandleBuildingSpawned(Building building)
    {
        myBuildings.Add(building);
    }
        
    private void AuthorityHandleBuildingDespawned(Building building)
    {
        myBuildings.Remove(building);
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool old, bool newState)
    {
        if(!hasAuthority) return;

        AuthorityOnPartyOwnerStatUpdated?.Invoke(newState);
    }

    public void ClientHandleResourceUpdated(int oldVal, int newVal)
    {
        ClientOnResourcesUpdated?.Invoke(newVal);
    }

    public void ClientHandleUpdateDisplayName(string old, string newName)
    {
        ClientOnInfoUpdated?.Invoke();
    }

    #endregion

}
