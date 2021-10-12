using System;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Minimap : MonoBehaviour, IPointerDownHandler,IDragHandler
{
    [SerializeField] private RectTransform minimapRect;
    [SerializeField] private float mapScale;
    [SerializeField] private float offset = -6;

    private Transform _playerCameraTransform;

    private void Start()
    {
        _playerCameraTransform = NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetCameraTransform();
    }

    private void MoveCamera()
    {
        var mousePos = Mouse.current.position.ReadValue();
        
        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRect, mousePos, null, out var localPoint)) return;

        var rect = minimapRect.rect;
        var lerp = new Vector2((localPoint.x - rect.x)/ rect.width, (localPoint.y - rect.y)/rect.height);

        var newCameraPos = new Vector3(Mathf.Lerp(-mapScale, mapScale, lerp.x), _playerCameraTransform.position.y, Mathf.Lerp(-mapScale, mapScale, lerp.y));

        _playerCameraTransform.position = newCameraPos + new Vector3(0,0, offset);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MoveCamera();
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveCamera();
    }
}
