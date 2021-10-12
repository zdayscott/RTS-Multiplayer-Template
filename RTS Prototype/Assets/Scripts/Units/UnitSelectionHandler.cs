using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionHandler : MonoBehaviour
{
    private Camera mainCamera;
    public List<Unit> selectedUnits { get; } = new List<Unit>();

    [SerializeField] private RectTransform unitSelectionArea = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();

    private Vector2 startPostion;
    private RTSPlayer _player;

    private void Start()
    {
        mainCamera = Camera.main;
        _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        Unit.AuthorityOnUnitDespawn += AuthorityHandleDespawn;

        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }



    private void OnDestroy()
    {
        Unit.AuthorityOnUnitDespawn -= AuthorityHandleDespawn;
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
        else if (Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea();
        }
        
    }

    private void StartSelectionArea()
    {
        if(!Keyboard.current.leftShiftKey.isPressed)
        {
            // Start Selection area    
            foreach (var selectedUnit in selectedUnits)
            {
                selectedUnit.Deselect();
            }

            selectedUnits.Clear();
        }
        
        unitSelectionArea.gameObject.SetActive(true);

        startPostion = Mouse.current.position.ReadValue();
        
        UpdateSelectionArea();
    }

    private void UpdateSelectionArea()
    {
        var mousePosition = Mouse.current.position.ReadValue();

        var areaWidth = mousePosition.x - startPostion.x;
        var areaHeight = mousePosition.y - startPostion.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition = startPostion + new Vector2(areaWidth / 2, areaHeight / 2);
    }

    private void ClearSelectionArea()
    {
        unitSelectionArea.gameObject.SetActive(false);

        if (unitSelectionArea.sizeDelta.magnitude == 0)
        {
            var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                    
            if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) return;
                    
            if(!hit.collider.TryGetComponent<Unit>(out Unit unit)) return;

            if (!selectedUnits.Contains(unit))
            {                
                selectedUnits.Add(unit);
                unit.Select();
            }

        }
        else
        {
            var min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
            var max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

            foreach (var unit in from unit in _player.GetMyUnits() 
                let screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position) 
                where !selectedUnits.Contains(unit) &&
                      screenPosition.x >= min.x && 
                      screenPosition.x <= max.x && 
                      screenPosition.y >= min.y &&
                      screenPosition.y <= max.y 
                select unit)
            {
                selectedUnits.Add(unit);
                unit.Select();
            }
        }
    }
    
    private void AuthorityHandleDespawn(Unit obj)
    {
        selectedUnits.Remove(obj);
    }

    private void ClientHandleGameOver(string _)
    {
        enabled = false;
    }
}