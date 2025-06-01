using System.Collections.Generic;
using UnityEngine;

public class PuzzleImageSlicer : MonoBehaviour
{
    public string imageName = "originalImage"; // Resources 폴더에 넣어야 함
    public int rows = 3;
    public int cols = 3;
    public float worldSize = 3f; // 원본 이미지가 차지할 총 월드 크기
    public GameObject piecePrefab;

    void Start()
    {
        Texture2D originalTexture = Resources.Load<Texture2D>("KingSejongtheGreat/" + imageName);
        if (originalTexture == null)
        {
            Debug.LogError("Resources 폴더에 이미지가 없습니다: " + imageName);
            return;
        }

        if (piecePrefab == null)
        {
            piecePrefab = Resources.Load<GameObject>("Prefabs/Piece"); // Resources/Prefabs/Piece.prefab
            if (piecePrefab == null)
            {
                Debug.LogError("Piece 프리팹을 Resources/Prefabs 경로에 넣어주세요.");
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
        originalObj.transform.position = new Vector3(0, 0, 20); // 원본 위치
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
        Vector3 originalImagePos = new Vector3(0, 0, 20); // 원본 위치

        // 조각 간 간격을 고려하여 겹치지 않는 위치 생성
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

                // 올바른 위치 설정
                float correctX = startPos.x + col * pieceWorldWidth;
                float correctZ = startPos.y - row * pieceWorldHeight;
                Vector3 correctPos = new Vector3(correctX, 0, correctZ) + originalImagePos;

                // 겹치지 않는 랜덤 위치 생성 (격자 단위)
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

                // 정답 위치 저장
                Piece pieceScript = piece.GetComponent<Piece>();
                if (pieceScript != null)
                {
                    pieceScript.correctPosition = correctPos;
                }

                // 박스 콜라이더 크기 설정
                BoxCollider box = piece.GetComponent<BoxCollider>();
                if (box != null)
                {
                    // 조각이 x축으로 90도 회전되어 있으므로 y,z가 이미지 가로/세로에 대응됨
                    box.size = new Vector3(pieceWorldWidth, pieceWorldHeight, 0.2f); // 얇게, 가로, 세로
                    box.center = Vector3.zero;
                }
            }
        }
    }
}
