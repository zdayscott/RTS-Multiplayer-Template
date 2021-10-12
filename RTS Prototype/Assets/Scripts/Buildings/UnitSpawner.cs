using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Health health;
    [SerializeField] private Unit unitPrefab = null;
    [SerializeField] private Transform unitSpawnpoint = null;
    [SerializeField] private TMP_Text remainingUnitsText;
    [SerializeField] private Image unitProcessingImage;
    [SerializeField] private int maxUnitQueue = 5;
    [SerializeField] private float spawnMoveRange = 7;
    [SerializeField] private float unitSpawnDuration = 5;

    [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdate))]
    private int queuedUnits;

    [SyncVar]
    private float unitTimer;

    private RTSPlayer _player;
    private float progressImageVelocity;

    private void Update()
    {
        if (isServer)
        {
            ProduceUnits();
        }

        if (isClient)
        {
            UpdateTimerDisplay();
        }
    }

    #region Server

    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleDie;
    }



    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
    }
    
    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }
    [Command]
    private void CmdSpawnUnit()
    {
        
        if(queuedUnits == maxUnitQueue) return;

        _player = connectionToClient.identity.GetComponent<RTSPlayer>();
        
        if(_player.GetResources() < unitPrefab.GetCost()) return;

        queuedUnits++;
        
        _player.SetResources(_player.GetResources() - unitPrefab.GetCost());
    }

    [Server]
    private void ProduceUnits()
    {
        if (queuedUnits == 0) return;

        unitTimer += Time.deltaTime;
        if(unitTimer < unitSpawnDuration) return;
        
        var unitInstance = Instantiate(unitPrefab, unitSpawnpoint.position, unitSpawnpoint.rotation);
        NetworkServer.Spawn(unitInstance.gameObject, connectionToClient);

        var spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = unitSpawnpoint.position.y;

        var unitMovement = unitInstance.GetUnitMovement();
        unitMovement.ServerMove(unitSpawnpoint.position + spawnOffset);

        queuedUnits--;
        unitTimer = 0;
    }

    #endregion

    #region Client

        public void OnPointerClick(PointerEventData eventData)
        {
            if(!hasAuthority) return;

            if (eventData.button != PointerEventData.InputButton.Left) return;

            CmdSpawnUnit();
        }

        public void ClientHandleQueuedUnitsUpdate(int old, int newVal)
        {
            remainingUnitsText.text = newVal.ToString();
        }

        private void UpdateTimerDisplay()
        {
            var newProgress = unitTimer / unitSpawnDuration;

            if (newProgress < unitProcessingImage.fillAmount)
            {
                unitProcessingImage.fillAmount = newProgress;
            }
            else
            {
                unitProcessingImage.fillAmount = Mathf.SmoothDamp(unitProcessingImage.fillAmount, newProgress,
                    ref progressImageVelocity, 0.1f);
            }
        }

    #endregion


}
