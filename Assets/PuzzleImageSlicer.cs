using System.Collections.Generic;
using UnityEngine;

public class PuzzleImageSlicer : MonoBehaviour
{
    public string imageName = "originalImage"; // Resources ������ �־�� ��
    public int rows = 3;
    public int cols = 3;
    public float worldSize = 3f; // ���� �̹����� ������ �� ���� ũ��

    public GameObject piecePrefab;
    public GameObject puzzleBackgroundPrefab;

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
        GameObject bgObj = Instantiate(puzzleBackgroundPrefab);
        bgObj.transform.position = new Vector3(0, 0, 20);
        bgObj.transform.rotation = Quaternion.Euler(90f, 0, 0);

        PuzzleBackground background = bgObj.GetComponent<PuzzleBackground>();
        background.Init(texture, worldSize, rows, cols);
    }

    void CreatePuzzlePieces(Texture2D texture)
    {
        float pixelsPerUnit = texture.width / worldSize;

        float piecePixelWidth = (float)texture.width / cols;
        float piecePixelHeight = (float)texture.height / rows;

        float imageAspect = (float)texture.height / texture.width;
        float worldHeight = worldSize * imageAspect;

        float pieceWorldWidth = worldSize / cols;
        float pieceWorldHeight = worldHeight / rows;

        Vector3 originalImagePos = new Vector3(0, 0, 20); // ���� ��� ��ġ
        Vector3 topLeft = originalImagePos - new Vector3(worldSize / 2f, 0, worldHeight / 2f);

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

                // �ùٸ� ��ġ ���
                Vector3 correctPos = topLeft + new Vector3(
                    col * pieceWorldWidth + pieceWorldWidth / 2f,
                    0,
                    row * pieceWorldHeight + pieceWorldHeight / 2f
                );

                // ��ġ�� �ʴ� ���� ��ġ ����
                Vector3 randomPos = Vector3.zero;
                while (true)
                {
                    int randX = Random.Range(-20, 21);
                    int randZ = Random.Range(-20, 0);
                    Vector2Int gridPos = new Vector2Int(randX, randZ);
                    if (!usedPositions.Contains(gridPos))
                    {
                        usedPositions.Add(gridPos);
                        randomPos = new Vector3(randX, 0, randZ);
                        break;
                    }
                }

                piece.transform.position = randomPos;
                piece.transform.rotation = Quaternion.Euler(90f, 0, 0); // 2D Sprite�� ��鿡 ����

                // ���� ��ġ ����
                Piece pieceScript = piece.GetComponent<Piece>();
                if (pieceScript != null)
                {
                    pieceScript.correctPosition = correctPos;
                    pieceScript.row = row;
                    pieceScript.col = col;
                }

                // �ݶ��̴� ����
                BoxCollider box = piece.GetComponent<BoxCollider>();
                if (box != null)
                {
                    // ������ x������ 90�� ȸ���Ǿ� �����Ƿ� y,z�� �̹��� ����/���ο� ������
                    box.size = new Vector3(pieceWorldWidth, pieceWorldHeight, 0.2f);
                    box.center = Vector3.zero;
                }
            }
        }
    }

}
