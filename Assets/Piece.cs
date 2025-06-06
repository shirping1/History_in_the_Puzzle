using Photon.Pun;
using UnityEditor;
using UnityEngine;

public class Piece : MonoBehaviourPun
{
    public int row, col;
    public Vector3 correctPosition;
    private bool isLocked = false;

    private Vector3 pos;
    private Quaternion quaternion;

    public void SetTargetTransform(Vector3 pos, Quaternion quaternion)
    {
        this.pos = pos;
        this.quaternion = quaternion;
    }

    public void LockPiece()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RequestOwnership();
        }

        transform.position = pos;
        transform.rotation = quaternion;

        isLocked = true;
        GetComponent<Collider>().enabled = false;
        photonView.RPC(nameof(RPC_LockPiece), RpcTarget.Others);
    }

    [PunRPC]
    public void RPC_LockPiece()
    {
        isLocked = true;
        GetComponent<Collider>().enabled = false;
    }

    public void ResetToRandomPosition()
    {
        if (isLocked) return;
        if (!PhotonNetwork.IsMasterClient) return;

        float x = Random.Range(-20f, 20f);
        float z = Random.Range(-20f, 0f);
        transform.position = new Vector3(x, 0, z);
    }

    [PunRPC]
    public void InitPiece(int row, int col, Vector3 correctPos, Vector3 randomPos, float x, float y, float rectWidth, float rectHeight, float pixelsPerUnit, string textureName)
    {
        this.row = row;
        this.col = col;
        this.correctPosition = correctPos;
        transform.position = randomPos;
        transform.rotation = Quaternion.Euler(90f, 0, 0);

        Rect spriteRect = new Rect(x, y, rectWidth, rectHeight);

        // ���� �ؽ�ó �ε�
        Texture2D texture = Resources.Load<Texture2D>($"Images/{textureName}");
        if (texture == null)
        {
            Debug.LogError($"Texture '{textureName}' not found in Resources/Images/");
            return;
        }

        // ��������Ʈ ����
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sprite = Sprite.Create(
                texture,
                spriteRect,
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit
            );
        }

        // �ݶ��̴� ����
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            float width = spriteRect.width / pixelsPerUnit;
            float height = spriteRect.height / pixelsPerUnit;
            box.size = new Vector3(width, height, 0.2f);
            box.center = Vector3.zero;
        }
    }
}
