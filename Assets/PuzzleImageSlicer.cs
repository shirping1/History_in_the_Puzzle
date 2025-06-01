using System.Collections.Generic;
using UnityEngine;

public class PuzzleImageSlicer : MonoBehaviour
{
    public string imageName = "originalImage"; // Resources ������ �־�� ��
    public int rows = 3;
    public int cols = 3;
    public float worldSize = 3f; // ���� �̹����� ������ �� ���� ũ��
    public GameObject piecePrefab;

    void Start()
    {
        Texture2D originalTexture = Resources.Load<Texture2D>("KingSejongtheGreat/" + imageName);
        if (originalTexture == null)
        {
            Debug.LogError("Resources ������ �̹����� �����ϴ�: " + imageName);
            return;
        }

        if (piecePrefab == null)
        {
            piecePrefab = Resources.Load<GameObject>("Prefabs/Piece"); // Resources/Prefabs/Piece.prefab
            if (piecePrefab == null)
            {
                Debug.LogError("Piece �������� Resources/Prefabs ��ο� �־��ּ���.");
                return;
            }
        }

        CreateFullImageObject(originalTexture);
        CreatePuzzlePieces(originalTexture);
    }

    void CreateFullImageObject(Texture2D texture)
    {
        GameObject originalObj = new GameObject("OriginalImage");
        SpriteRenderer renderer = originalObj.AddComponent<SpriteRenderer>();

        float pixelsPerUnit = texture.width / worldSize;

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit
        );

        renderer.sprite = sprite;
        renderer.color = new Color(1,1,1,0.5f) ;
        originalObj.transform.position = new Vector3(0, 0, 20); // ���� ��ġ
        originalObj.transform.rotation = Quaternion.Euler(90f, 0, 0);
    }

    void CreatePuzzlePieces(Texture2D texture)
    {
        float pixelsPerUnit = texture.width / worldSize;

        float piecePixelWidth = (float)texture.width / cols;
        float piecePixelHeight = (float)texture.height / rows;

        float pieceWorldWidth = worldSize / cols;
        float pieceWorldHeight = (worldSize * texture.height / texture.width) / rows;

        Vector2 startPos = new Vector2(-pieceWorldWidth * (cols - 1) / 2f, pieceWorldHeight * (rows - 1) / 2f);
        Vector3 originalImagePos = new Vector3(0, 0, 20); // ���� ��ġ

        // ���� �� ������ ����Ͽ� ��ġ�� �ʴ� ��ġ ����
        HashSet<Vector2Int> usedPositions = new HashSet<Vector2Int>();

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int invertedRow = rows - 1 - row;

                Rect rect = new Rect(
                    col * piecePixelWidth,
                    invertedRow * piecePixelHeight,
                    piecePixelWidth,
                    piecePixelHeight
                );

                Sprite pieceSprite = Sprite.Create(
                    texture,
                    rect,
                    new Vector2(0.5f, 0.5f),
                    pixelsPerUnit
                );

                GameObject piece = Instantiate(piecePrefab);
                piece.name = $"Piece_{row}_{col}";

                SpriteRenderer renderer = piece.GetComponent<SpriteRenderer>();
                if (renderer != null)
                    renderer.sprite = pieceSprite;

                // �ùٸ� ��ġ ����
                float correctX = startPos.x + col * pieceWorldWidth;
                float correctZ = startPos.y - row * pieceWorldHeight;
                Vector3 correctPos = new Vector3(correctX, 0, correctZ) + originalImagePos;

                // ��ġ�� �ʴ� ���� ��ġ ���� (���� ����)
                Vector3 randomPos = Vector3.zero;
                while (true)
                {
                    int randX = Random.Range(-20, 21);
                    int randZ = Random.Range(-21, 0);
                    Vector2Int gridPos = new Vector2Int(randX, randZ);
                    if (!usedPositions.Contains(gridPos))
                    {
                        usedPositions.Add(gridPos);
                        randomPos = new Vector3(randX, 0, randZ);
                        break;
                    }
                }

                piece.transform.position = randomPos;
                piece.transform.rotation = Quaternion.Euler(90f, 0, 0);

                // ���� ��ġ ����
                Piece pieceScript = piece.GetComponent<Piece>();
                if (pieceScript != null)
                {
                    pieceScript.correctPosition = correctPos;
                }

                // �ڽ� �ݶ��̴� ũ�� ����
                BoxCollider box = piece.GetComponent<BoxCollider>();
                if (box != null)
                {
                    // ������ x������ 90�� ȸ���Ǿ� �����Ƿ� y,z�� �̹��� ����/���ο� ������
                    box.size = new Vector3(pieceWorldWidth, pieceWorldHeight, 0.2f); // ���, ����, ����
                    box.center = Vector3.zero;
                }
            }
        }
    }
}
