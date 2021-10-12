using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [FormerlySerializedAs("_building")] [SerializeField] private Building building;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private LayerMask floorMask;

    private Camera _mainCamera;
    private RTSPlayer _player;
    private BoxCollider _buildingCollider;
    private GameObject _buildingPreviewInstance;
    private Renderer _buildingRenderer;

    private void Start()
    {
        _mainCamera = Camera.main;

        iconImage.sprite = building.GetSprite();
        priceText.text = building.GetPrice().ToString();
        _buildingCollider = building.GetComponent<BoxCollider>();
        _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    }

    private void Update()
    {
        if(_buildingPreviewInstance == null) return;

        UpdateBuildingPreview();
    }

    private void UpdateBuildingPreview()
    {
        var ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, floorMask)) return;

        _buildingPreviewInstance.transform.position = hit.point;

        if (!_buildingPreviewInstance.activeSelf)
        {
            _buildingPreviewInstance.SetActive(true);
        }

        var color = _player.CanPlaceBuilding(_buildingCollider, hit.point) ? Color.green : Color.red;
        
        _buildingRenderer.material.SetColor("_BaseColor", color);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) return;
        
        if(_player.GetResources() < building.GetPrice()) return;

        _buildingPreviewInstance = Instantiate(building.GetPreview());
        _buildingRenderer = _buildingPreviewInstance.GetComponentInChildren<Renderer>();
        
        _buildingPreviewInstance.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(_buildingPreviewInstance == null) return;

        var ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, floorMask))
        {
            // Place building
            _player.CmdTryPlaceBuilding(building.GetId(), hit.point);
        }
        
        Destroy(_buildingPreviewInstance);
    }
}
