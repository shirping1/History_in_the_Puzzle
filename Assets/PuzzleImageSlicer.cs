using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleImageSlicer : MonoBehaviour
{
    public string imageName = "originalImage"; // Resources 폴더에 넣어야 함
    public int rows = 3;
    public int cols = 3;
    public float worldSize = 3f; // 원본 이미지가 차지할 총 월드 크기

    public GameObject piecePrefab;
    public GameObject puzzleBackgroundPrefab;

    void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

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

        CreateFullImageObject(imageName);
        CreatePuzzlePieces(originalTexture, imageName);
    }

    void CreateFullImageObject(string textureName)
    {
        GameObject bgObj = PhotonNetwork.Instantiate(puzzleBackgroundPrefab.name, new Vector3(0, 0, 20), Quaternion.Euler(90f, 0, 0));

        PuzzleBackground background = bgObj.GetComponent<PuzzleBackground>();
        background.Init(textureName, worldSize, rows, cols);
    }

    void CreatePuzzlePieces(Texture2D texture, string textureName)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        float pixelsPerUnit = texture.width / worldSize;

        float piecePixelWidth = (float)texture.width / cols;
        float piecePixelHeight = (float)texture.height / rows;

        float imageAspect = (float)texture.height / texture.width;
        float worldHeight = worldSize * imageAspect;

        float pieceWorldWidth = worldSize / cols;
        float pieceWorldHeight = worldHeight / rows;

        Vector3 originalImagePos = new Vector3(0, 0, 20);
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

                GameObject piece = PhotonNetwork.Instantiate(piecePrefab.name, Vector3.zero, Quaternion.identity);
                piece.name = $"Piece_{row}_{col}";

                Vector3 correctPos = topLeft + new Vector3(
                    col * pieceWorldWidth + pieceWorldWidth / 2f,
                    0,
                    row * pieceWorldHeight + pieceWorldHeight / 2f
                );

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

                Piece pieceScript = piece.GetComponent<Piece>();
                pieceScript.photonView.RPC(
                    nameof(pieceScript.InitPiece),
                    RpcTarget.AllBuffered,
                    row, col, correctPos, randomPos, rect.x, rect.y, rect.width, rect.height, pixelsPerUnit, textureName
                );
            }
        }
    }

}
