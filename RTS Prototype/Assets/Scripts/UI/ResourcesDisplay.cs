using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class ResourcesDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text resourcesText;

    private RTSPlayer _player;

    private void Start()
    {
        _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        PlayerOnClientOnResourcesUpdated(_player.GetResources());
        _player.ClientOnResourcesUpdated += PlayerOnClientOnResourcesUpdated;
    }

    private void OnDestroy()
    {
        _player.ClientOnResourcesUpdated -= PlayerOnClientOnResourcesUpdated;
    }

    private void PlayerOnClientOnResourcesUpdated(int obj)
    {
        resourcesText.text = $"Resources: {obj}";
    }
}
