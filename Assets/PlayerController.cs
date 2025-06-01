using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 3f;
    private Animator animator;

    private GameObject nearbyPiece = null;       // 충돌 중인 Piece
    private GameObject currentHeldPiece = null;  // 현재 들고 있는 Piece
    public Transform holdPoint;                  // 머리 위 위치

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 moveInput = new Vector3(h, 0, v).normalized;
       

        // 회전
        if (moveInput != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // 애니메이션 Speed 업데이트
        animator.SetFloat("Speed", moveInput.magnitude);

        // G키 입력
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (currentHeldPiece != null)
            {
                DropPiece(); // 들고 있으면 놓기
            }
            else if (nearbyPiece != null)
            {
                PickUpPiece(nearbyPiece); // 근처에 있고 안 들고 있으면 줍기
            }
        }
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
        currentHeldPiece = piece;
        piece.transform.SetParent(holdPoint);

        // 위치만 조정하고, 회전은 그대로 유지
        piece.transform.position = holdPoint.position;

        Rigidbody rb = piece.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
    }

    private void DropPiece()
    {
        // 1. 부모 해제
        currentHeldPiece.transform.SetParent(null);

        // 2. 현재 위치 가져오기
        Vector3 dropPosition = currentHeldPiece.transform.position;

        // 3. y값만 0으로 설정 (바닥에 떨어뜨림)
        dropPosition.y = 0f;
        currentHeldPiece.transform.position = dropPosition;

        // 4. 물리 활성화
        Rigidbody rb = currentHeldPiece.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;

        // 5. 현재 들고 있는 조각 초기화
        currentHeldPiece = null;
    }


}
