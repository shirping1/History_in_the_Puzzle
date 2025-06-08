
using Photon.Pun;
using UnityEngine;

public class PlayerController : MonoBehaviourPun
{
    public float moveSpeed = 3f;
    [HideInInspector] public Transform holdPoint;  // 인스펙터에 할당되어야 함

    private Animator animator;
    private Rigidbody rb;

    private GameObject nearbyPiece = null;       // 충돌 중인 Piece
    private GameObject currentHeldPiece = null;  // 현재 들고 있는 Piece

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        // 비마스터는 로컬에서 RootMotion을 쓰지 않고
        if (!photonView.IsMine)
        {
            animator.applyRootMotion = false;
        }
    }

    Vector3 moveInput;

    void Update()
    {
        // 비마스터는 입력/이동 로직을 실행하지 않음 (Transform은 PhotonTransformView가 처리)
        if (!photonView.IsMine)
            return;

        if (StageManager.Instance.isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RPC_Handler.Instance.photonView.RPC(nameof(RPC_Handler.Instance.RPC_VoteRestart), RpcTarget.MasterClient, photonView.Owner.ActorNumber);
            }
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(h, 0, v).normalized;

        // 회전
        if (moveInput != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // 애니메이션 Speed 업데이트
        animator.SetFloat("Speed", moveInput.magnitude);

        // G키 입력 (줍기/놓기)
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (currentHeldPiece != null)
            {
                DropPiece();
            }
            else if (nearbyPiece != null)
            {
                PickUpPiece(nearbyPiece);
            }
        }
    }

    private void OnAnimatorMove()
    {
        if (animator != null && photonView.IsMine)
        {
            // RootMotion을 이동/회전에 적용
            transform.position += animator.deltaPosition;
            transform.rotation *= animator.deltaRotation;
        }
    }

    private void FixedUpdate()
    {
        // (선택사항) 물리 이동으로 대체할 경우 주석 해제
        // if (photonView.IsMine)
        //     rb.MovePosition(rb.position + moveInput * Time.fixedDeltaTime * moveSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Piece") && currentHeldPiece == null)
        {
            nearbyPiece = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Piece") && nearbyPiece == other.gameObject)
        {
            nearbyPiece = null;
        }
    }

    private void PickUpPiece(GameObject piece)
    {
        PhotonView piecePV = piece.GetComponent<PhotonView>();
        currentHeldPiece = piece;
        int myID = photonView.ViewID;
        int pieceID = piecePV.ViewID;
        if (piecePV != null && !piecePV.IsMine)
        {
            photonView.RPC(nameof(RPC_TransferOwnerShip), RpcTarget.OthersBuffered, pieceID, myID);
            //piecePV.RequestOwnership();
            Debug.Log("소유권 요청");
        }


        // ① 로컬에선 즉시 부모 붙이고 위치 고정
        _AttachPieceLocally(piece);

        // ② 다른 클라이언트에 동일하게 부모 붙이고 위치 고정하도록 RPC 호출
        photonView.RPC(nameof(RPC_PickUpPiece), RpcTarget.OthersBuffered, pieceID, myID);

        //photonView.RPC(nameof(RPC_TransferOwnerShip), RpcTarget.OthersBuffered, pieceID, myID);
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

    private void _AttachPieceLocally(GameObject piece)
    {
        piece.transform.SetParent(holdPoint);
        piece.transform.position = holdPoint.position;


        // piece.transform.localRotation = Quaternion.identity; ← 이 줄 제거

        Rigidbody rb = piece.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
    }

    [PunRPC]
    private void RPC_PickUpPiece(int pieceViewID, int playerViewID)
    {
        PhotonView piecePV = PhotonView.Find(pieceViewID);
        PhotonView playerPV = PhotonView.Find(playerViewID);
        if (piecePV == null || playerPV == null) return;

        PlayerController pc = playerPV.GetComponent<PlayerController>();
        if (pc == null) return;

        piecePV.transform.SetParent(pc.holdPoint);
        piecePV.transform.position = pc.holdPoint.position;

        // ⚠ 회전값을 변경하지 않고 그대로 둠
        // piecePV.transform.localRotation = Quaternion.identity; ← 제거

        Rigidbody rb = piecePV.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
    }


    private void DropPiece()
    {
        if (currentHeldPiece == null) return;

        int pieceID = currentHeldPiece.GetComponent<PhotonView>().ViewID;

        // ① 로컬에서 부모 해제 + y=0
        _DetachPieceLocally(currentHeldPiece);

        // ② 원격에도 동일하게 부모 해제 + y=0
        photonView.RPC(nameof(RPC_DropPiece), RpcTarget.OthersBuffered, pieceID);

        currentHeldPiece = null;
    }

    private void _DetachPieceLocally(GameObject piece)
    {
        piece.transform.SetParent(null);
        Vector3 dropPos = piece.transform.position;
        dropPos.y = 0f;
        piece.transform.position = dropPos;
        Rigidbody rbPiece = piece.GetComponent<Rigidbody>();
        if (rbPiece) rbPiece.isKinematic = false;
    }

    [PunRPC]
    private void RPC_DropPiece(int pieceViewID)
    {
        PhotonView piecePV = PhotonView.Find(pieceViewID);
        if (piecePV == null) return;

        piecePV.transform.SetParent(null);
        Vector3 dropPos = piecePV.transform.position;
        dropPos.y = 0f;
        piecePV.transform.position = dropPos;
        Rigidbody rbPiece = piecePV.GetComponent<Rigidbody>();
        if (rbPiece) rbPiece.isKinematic = false;
    }
}
