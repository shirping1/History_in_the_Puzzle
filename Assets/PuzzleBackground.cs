using Photon.Pun;
using UnityEngine;

public class PuzzleBackground : MonoBehaviourPun
{
    private float worldSize;
    private float textureAspect; // height / width
    private float pieceWorldWidth;
    private float pieceWorldHeight;
    private Texture2D texture;

    [SerializeField]
    private float threshold = 2f;

    int rows, cols;
    private Person person;

    public void Init(string textureName, float worldSize, int rows, int cols, Person person)
    {
        photonView.RPC("RPC_Init", RpcTarget.OthersBuffered, textureName, worldSize, rows, cols, (int)person);

        Texture2D texture = Resources.Load<Texture2D>("Images/" + textureName);
        if (texture == null)
        {
            Debug.Log("Resources ������ �̹����� �����ϴ�: " + textureName);
        }

        this.texture = texture;
        this.worldSize = worldSize;
        this.textureAspect = (float)texture.height / texture.width;
        this.rows = rows;
        this.cols = cols;
        this.person = person;

        // ���� ������ ũ�� ���
        pieceWorldWidth = worldSize / cols;
        pieceWorldHeight = (worldSize * textureAspect) / rows;

        // ��������Ʈ ����
        float pixelsPerUnit = texture.width / worldSize;
        Sprite sprite = Sprite.Create(texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit
        );
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = new Color(1f, 1f, 1f, 0.5f);

        // �ݶ��̴� ũ�� ����
        BoxCollider collider = GetComponent<BoxCollider>();
        collider.size = new Vector3(worldSize, worldSize * textureAspect, 0.2f);
        collider.center = Vector3.zero;
    }

    [PunRPC]
    public void RPC_Init(string textureName, float worldSize, int rows, int cols, int person)
    {
        Init(textureName, worldSize, rows, cols, (Person)person);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (texture == null || rows <= 0 || cols <= 0) return;

        Gizmos.color = Color.red;

        float pieceWidth = worldSize / cols;
        float pieceHeight = (worldSize * textureAspect) / rows;

        Vector3 topLeft = transform.position + new Vector3(-worldSize / 2f, 0, (worldSize * textureAspect) / 2f);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector3 targetPos = topLeft + new Vector3(
                    col * pieceWidth + pieceWidth / 2f,
                    0,
                    -(row * pieceHeight + pieceHeight / 2f)
                );
                Gizmos.DrawSphere(targetPos, 0.1f);
            }
        }
    }
#endif

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            // ������ Ŭ���̾�Ʈ�� ������ üũ�� ������ �������� ��
            return;
        }

        Piece piece = other.GetComponent<Piece>();
        if (piece != null)
        {
            // �»�� ������ ���
            Vector3 topLeft = transform.position + new Vector3(-worldSize / 2f, 0, (worldSize * textureAspect) / 2f);

            // �� ���� ũ�� ���
            float pieceWidth = worldSize / cols;
            float pieceHeight = (worldSize * textureAspect) / rows;

            // �ش� ������ �־�� �� ��ǥ ��ġ ���
            Vector3 targetPos = topLeft + new Vector3(
                piece.col * pieceWidth + pieceWidth / 2f,
                0,
                -(piece.row * pieceHeight + pieceHeight / 2f)
            );

            if (Vector3.Distance(piece.transform.position, targetPos) <= threshold)
            {
                Debug.Log($"[Snap] {piece.row},{piece.col} ��ġ�� ����");
                piece.SetTargetTransform(targetPos, Quaternion.Euler(90f, 0, 0));

                // (1) ���� ������ ���� ���� �г� ����
                photonView.RPC(nameof(RPC_PopUpQuizPanel), piece.photonView.Owner, piece.photonView.ViewID, (int)person);

                // (2) ���� ���� ī��Ʈ ������ ���� StageManager(������)�� RPC ȣ��
                //    �� ��� ���� ������ ���������� �˻��ϴ� ó���� StageManager �ʿ��� ���
                if (StageManager.Instance != null)
                {
                    StageManager.Instance.photonView.RPC(
                        nameof(StageManager.OnPiecePlacedRPC),
                        RpcTarget.MasterClient
                    );
                }
            }
            else
            {
                Debug.Log($"[Miss] {piece.row},{piece.col} ��ġ �ʹ� �ִ�. �ٽ� ����");
                piece.ResetToRandomPosition();
            }
        }
    }

    [PunRPC]
    public void RPC_PopUpQuizPanel(int viewID, int person)
    {
        UIQuizPanel uIQuizPanel = UIManager.Instance.OpenPopupPanel<UIQuizPanel>();
        uIQuizPanel.Init(viewID, (Person)person);
    }
}
