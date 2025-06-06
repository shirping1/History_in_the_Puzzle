using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class StageManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject playerPerfab;
    [SerializeField]
    private CinemachineCamera cinemachineCamera;
    [SerializeField]
    private PuzzleImageSlicer imageSlicer; // ���� ������ ����/�ʱ�ȭ�� �ִ� ������Ʈ

    // �� ó�� ���� ���� ��, �׸��� ���� ���� �ε� �ÿ� ���
    private bool isGameStarted = false;
    public static StageManager Instance;

    public Person person; // ���� �ε�� ���� �ش��ϴ� 'Person' enum

    // **��ü ���� ���� ���� ���, ���ݱ��� ���� ������ ����**
    private int totalPieces = 0;
    private int placedPieces = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }

    private void Start()
    {
        StartCoroutine(InitWhenJoinedRoom());
    }

    // �濡 ���� ��(PhotonNetwork.InRoom�� true�� ��) ���� �÷��̾ �����ϰ� ������ ����
    private IEnumerator InitWhenJoinedRoom()
    {
        // �� ���� ������ ���
        while (!PhotonNetwork.InRoom)
            yield return null;

        UIManager.Instance.CloseAllUI();

        // �÷��̾� ������Ʈ ����
        GameObject player = PhotonNetwork.Instantiate(playerPerfab.name, Vector3.zero, Quaternion.identity);
        cinemachineCamera.Follow = player.transform;
        cinemachineCamera.LookAt = player.transform;

        // ������ Ŭ���̾�Ʈ�� ���, ��ٷ� ���� �ε� �õ�
        TryStartGame();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // ������ Ŭ���̾�Ʈ�� �ƴϰų� �̹� ���� ���������� ����
        TryStartGame();
    }

    /// <summary>
    /// ������ Ŭ���̾�Ʈ�� ���ִ� �ο��� �� á�� ���� ���۵��� �ʾҴٸ顱 �� ���� ������ �ε��� �ش�.
    /// </summary>
    private void TryStartGame()
    {
        if (!PhotonNetwork.IsMasterClient || isGameStarted)
            return;

        // �濡 �ʿ��� �ο��� ��� ä������
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            isGameStarted = true;
            Debug.Log("��� �÷��̾� ���� �Ϸ�. ���� ����!");
            // �������� Person(���� �̹��� ���� ����)
            person = (Person)Random.Range(0, (int)Person.MAX);

            // ���� �ʱ�ȭ
            imageSlicer.Init(person);

            // ���� ������ ������ ���� �� ������ ��� ��, totalPieces�� ����
            StartCoroutine(SetTotalPiecesCoroutine());
        }
    }

    /// <summary>
    /// �� ������ ��� �� �� ���� �����ϴ� Piece ������ ���� totalPieces�� ����
    /// �� placedPieces�� 0���� �ʱ�ȭ
    /// </summary>
    private IEnumerator SetTotalPiecesCoroutine()
    {
        yield return null; // �� ������ ��ٸ� ��
        // Scene �󿡼� ��Piece�� ������Ʈ�� ���� ��� ������Ʈ�� ã�Ƽ� ���� �ľ�
        Piece[] pieces = FindObjectsOfType<Piece>();
        totalPieces = pieces.Length;
        placedPieces = 0;
        Debug.Log($"[StageManager] ���� ���� �� ����: {totalPieces}");
    }

    /// <summary>
    /// ������ �������� ���� ������ ȣ��Ǵ� RPC ������
    /// �� RpcTarget.MasterClient �θ� ���ŵ�
    /// </summary>
    [PunRPC]
    public void OnPiecePlacedRPC()
    {
        OnPiecePlacedLocal();
    }

    /// <summary>
    /// MasterClient ������ local�ϰ� ���� ���� ���� ������Ű��, 
    /// ��� ���� ������ ���� ���� �ε� ��ƾ�� ����
    /// </summary>
    private void OnPiecePlacedLocal()
    {
        placedPieces++;
        Debug.Log($"[StageManager] ���� ���� ���� ����: {placedPieces}/{totalPieces}");

        if (placedPieces >= totalPieces)
        {
            Debug.Log("[StageManager] ��� ���� ������ ����! ���� ���� �ε�...");
            // �ڷ�ƾ���� ���� ������ ���� �ε��� ����
            StartCoroutine(LoadNextPuzzleCoroutine());
        }
    }

    /// <summary>
    /// ��� ���� ������ ������ �� ����� �ڷ�ƾ.
    /// 1) ���� ���� ������ ���� ����� ��Ʈ��ũ���� �����ϰ� ��� ����(PUN Destroy)
    /// 2) ���� Person(���� �̹��� ����)�� �������� ��� imageSlicer.Init(person) ȣ��
    /// 3) �� ������ ��ٸ� �� totalPieces ���� ���
    /// </summary>
    private IEnumerator LoadNextPuzzleCoroutine()
    {
        // (1) ���� ���� ������ ����� ��� �ı�
        //    �� PUN ������Ʈ�� ��� PhotonNetwork.Destroy, �Ϲ� ������Ʈ�� Destroy
        //    �� Piece.cs���� �̹� PhotonView�� �پ� �ִٰ� ����
        Piece[] oldPieces = FindObjectsOfType<Piece>();
        foreach (Piece p in oldPieces)
        {
            PhotonView pv = p.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                PhotonNetwork.Destroy(p.gameObject);
            }
            else if (pv == null)
            {
                Destroy(p.gameObject);
            }
        }

        // PuzzleBackground ��ü�� ���� ���� ��� ��
        PuzzleBackground[] oldBackgrounds = FindObjectsOfType<PuzzleBackground>();
        foreach (PuzzleBackground bg in oldBackgrounds)
        {
            PhotonView pv = bg.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                PhotonNetwork.Destroy(bg.gameObject);
            }
            else if (pv == null)
            {
                Destroy(bg.gameObject);
            }
        }

        yield return null; // �ı��� Ȯ���� �ݿ��� �� �ֵ��� �� ������ ��

        // (2) �� ���� �̹���(Person) ���� (������ ���� �̹����� ����)
        Person newPerson;
        do
        {
            newPerson = (Person)Random.Range(0, (int)Person.MAX);
        } while (newPerson == person);

        person = newPerson;
        imageSlicer.Init(person);

        // (3) �� ������ ��ٷ��� ���� ���Ӱ� ������ Piece�� ī��Ʈ
        yield return null;
        Piece[] newPieces = FindObjectsOfType<Piece>();
        totalPieces = newPieces.Length;
        placedPieces = 0;
        Debug.Log($"[StageManager] �� ���� �ε��. �� ���� ����: {totalPieces}");
    }
}
