using Mirror;
using UnityEngine;

public class Targetable : NetworkBehaviour
{
    [SerializeField] private Transform aimAtPoint = null;

    public Transform GetAimAtPoint() => aimAtPoint;
    
    
}
