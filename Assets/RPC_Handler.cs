using Photon.Pun;
using UnityEngine;

public class RPC_Handler : MonoBehaviourPun
{
    public static RPC_Handler Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(this);
    }

    [PunRPC]
    public void RPC_ClosePlayerWaitText()
    {
        StageManager.Instance.ClosePlayerWaitTextInMainPanel();
    }

    [PunRPC]
    public void RPC_SyncTimer(float time)
    {
        StageManager.Instance.mainPanel.SetTimerText(time);
    }
}
