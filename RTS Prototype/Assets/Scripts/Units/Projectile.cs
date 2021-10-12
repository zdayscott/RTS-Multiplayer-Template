using System;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : NetworkBehaviour
{
    
    [SerializeField] private Rigidbody rb;
    [SerializeField] private int damage = 20;
    [SerializeField] private float destroyAfterSeconds = 5;
    [SerializeField] private float launchForce = 20f;

    private void Start()
    {
        rb.velocity = transform.forward * launchForce;
    }

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), destroyAfterSeconds);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out NetworkIdentity id))
        {
            if(id.connectionToClient == connectionToClient) return;
        }
        else
        {
            return;
        }
        
        if(!other.TryGetComponent(out Health health)) return;
        
        health.DealDamage(damage);
        DestroySelf();
    }

    [Server]
    private void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}
