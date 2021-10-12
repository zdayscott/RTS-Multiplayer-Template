using System;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter;
    [SerializeField] private float chaseRange = 5f;

    private Camera mainCamera;

    #region Server

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    private void ServerHandleGameOver()
    {
        agent.ResetPath();
    }

    [ServerCallback]
    private void Update()
    {
        var target = targeter.target;
        if (target != null)
        {
            if ((target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange)
            {
                agent.SetDestination(target.transform.position);
            }
            else if(agent.hasPath)
            {
                agent.ResetPath();
            }
            return;
        }
        
        if(!agent.hasPath) return;
        if (agent.remainingDistance > agent.stoppingDistance) return;
        
        agent.ResetPath();
    }

    [Command]
    public void CmdMove(Vector3 targetPosition)
    {
        ServerMove(targetPosition);
    }

    public void ServerMove(Vector3 targetPosition)
    {
                targeter.CmdClearTarget();
                
                if(!NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 1f, NavMesh.AllAreas)) return;
        
                agent.SetDestination(hit.position);
    }

    #endregion
}