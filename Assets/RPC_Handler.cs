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

    [PunRPC]
    public void RPC_ResetPuzzlePiece(int photonViewID)
    {
        PhotonView piecePV = PhotonView.Find(photonViewID);
        if (piecePV != null)
        {
            Piece piece = piecePV.GetComponent<Piece>();
            if (piece != null)
            {
                piece.ResetToRandomPosition();  // 기존에 구현된 고정 함수 호출
                Debug.Log($"퍼즐 조각 [{piece.row}, {piece.col}] 고정 완료");
            }
            else
            {
                Debug.LogWarning("Piece 컴포넌트가 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning($"viewID {photonViewID}에 해당하는 PhotonView를 찾지 못했습니다.");
        }
    }

    [PunRPC]
    public void RPC_TransferOwnerShip(int pieceViewID, int playerViewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView piecePV = PhotonView.Find(pieceViewID);
        PhotonView playerPV = PhotonView.Find(playerViewID);
        if (piecePV == null || playerPV == null) return;

        piecePV.TransferOwnership(playerPV.Owner);
    }

    [PunRPC]
    public void RPC_OnGameOverText()
    {
        StageManager.Instance.mainPanel.OnGameOverText();
    }

    [PunRPC]
    public void RPC_GameOver()
    {
        StageManager.Instance.GameOver();
    }

    [PunRPC]
    public void RPC_VoteRestart(int actorNumber)
    {
        StageManager.Instance.VoteRestart(actorNumber);
    }
}
