using Mirror;
using UnityEngine;

public class Targeter : NetworkBehaviour
{
    public Targetable target { get; private set; }

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
        CmdClearTarget();
    }

    #region Server

        [Command]
        public void CmdSetTarget(GameObject targetGO)
        {
            if(!targetGO.TryGetComponent<Targetable>(out var targetable)) return;
    
            target = targetable;
        }
    
        [Command]
        public void CmdClearTarget()
        {
            target = null;
        }

    #endregion
    
}
