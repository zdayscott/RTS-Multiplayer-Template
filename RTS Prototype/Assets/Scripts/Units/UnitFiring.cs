using Mirror;
using UnityEngine;

public class UnitFiring : NetworkBehaviour
{
    [SerializeField] private Targeter targeter;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnpoint;
    [SerializeField] private float fireRange = 4f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float rotationSpeed = 20f;

    private float lastFireTime;

    [ServerCallback]
    private void Update()
    {
        if(targeter.target == null) return;
        
        if (!CanFireAtTarget()) return;

        var targetRotation = Quaternion.LookRotation(targeter.target.transform.position - transform.position);

        transform.rotation =
            Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (Time.time > (1 / fireRate) + lastFireTime)
        {
            var projectileRotation =
                Quaternion.LookRotation(targeter.target.GetAimAtPoint().position - projectileSpawnpoint.position);
            var projectileInstance = Instantiate(projectilePrefab, projectileSpawnpoint.position, projectileRotation);
            
            NetworkServer.Spawn(projectileInstance, connectionToClient);
            
            lastFireTime = Time.time;
        }
    }

    [Server]
    private bool CanFireAtTarget()
    {
        return (targeter.target.transform.position - transform.position).sqrMagnitude <= fireRange * fireRange;
    }
}
