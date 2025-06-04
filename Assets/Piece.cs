using Photon.Pun;
using UnityEngine;

public class Piece : MonoBehaviourPun
{
    public int row, col;
    public Vector3 correctPosition;
    private bool isLocked = false;

    public void LockPiece()
    {
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
    public void InitPiece(int row, int col, Vector3 correctPos, Vector3 randomPos, Rect spriteRect, float pixelsPerUnit, string textureName)
    {
        this.row = row;
        this.col = col;
        this.correctPosition = correctPos;
        transform.position = randomPos;
        transform.rotation = Quaternion.Euler(90f, 0, 0);

        // 퍼즐 텍스처 로드
        Texture2D texture = Resources.Load<Texture2D>($"Puzzles/{textureName}");
        if (texture == null)
        {
            Debug.LogError($"Texture '{textureName}' not found in Resources/Puzzles/");
            return;
        }

        // 스프라이트 생성
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

        // 콜라이더 설정
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
