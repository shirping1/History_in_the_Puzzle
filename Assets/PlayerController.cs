using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 3f;
    private Animator animator;

    private GameObject nearbyPiece = null;       // �浹 ���� Piece
    private GameObject currentHeldPiece = null;  // ���� ��� �ִ� Piece
    public Transform holdPoint;                  // �Ӹ� �� ��ġ

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 moveInput = new Vector3(h, 0, v).normalized;
       

        // ȸ��
        if (moveInput != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // �ִϸ��̼� Speed ������Ʈ
        animator.SetFloat("Speed", moveInput.magnitude);

        // GŰ �Է�
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (currentHeldPiece != null)
            {
                DropPiece(); // ��� ������ ����
            }
            else if (nearbyPiece != null)
            {
                PickUpPiece(nearbyPiece); // ��ó�� �ְ� �� ��� ������ �ݱ�
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

        // ��ġ�� �����ϰ�, ȸ���� �״�� ����
        piece.transform.position = holdPoint.position;

        Rigidbody rb = piece.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
    }

    private void DropPiece()
    {
        // 1. �θ� ����
        currentHeldPiece.transform.SetParent(null);

        // 2. ���� ��ġ ��������
        Vector3 dropPosition = currentHeldPiece.transform.position;

        // 3. y���� 0���� ���� (�ٴڿ� ����߸�)
        dropPosition.y = 0f;
        currentHeldPiece.transform.position = dropPosition;

        // 4. ���� Ȱ��ȭ
        Rigidbody rb = currentHeldPiece.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;

        // 5. ���� ��� �ִ� ���� �ʱ�ȭ
        currentHeldPiece = null;
    }


}
