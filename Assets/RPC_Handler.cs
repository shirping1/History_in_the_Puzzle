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

    [PunRPC]
    public void RPC_ShowClearText()
    {
        StageManager.Instance.mainPanel.SetClearText(true);
    }

    [PunRPC]
    public void RPC_LockPuzzlePiece(int photonViewID)
    {
        PhotonView piecePV = PhotonView.Find(photonViewID);
        if (piecePV != null)
        {
            Piece piece = piecePV.GetComponent<Piece>();
            if (piece != null)
            {
                piece.LockPiece();  // 기존에 구현된 고정 함수 호출
                Debug.Log($"퍼즐 조각 [{piece.row}, {piece.col}] 고정 완료");
            }
        }
    }
}
