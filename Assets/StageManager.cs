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
    private PuzzleImageSlicer imageSlicer; // 퍼즐 조각을 생성/초기화해 주는 컴포넌트

    // → 처음 게임 시작 시, 그리고 다음 퍼즐 로딩 시에 사용
    private bool isGameStarted = false;
    public static StageManager Instance;

    public Person person; // 현재 로드된 퍼즐에 해당하는 'Person' enum

    // **전체 퍼즐 조각 개수 대비, 지금까지 맞춘 개수를 추적**
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

    // 방에 들어온 뒤(PhotonNetwork.InRoom이 true일 때) 실제 플레이어를 스폰하고 게임을 시작
    private IEnumerator InitWhenJoinedRoom()
    {
        // 룸 입장 전까지 대기
        while (!PhotonNetwork.InRoom)
            yield return null;

        UIManager.Instance.CloseAllUI();

        // 플레이어 오브젝트 스폰
        GameObject player = PhotonNetwork.Instantiate(playerPerfab.name, Vector3.zero, Quaternion.identity);
        cinemachineCamera.Follow = player.transform;
        cinemachineCamera.LookAt = player.transform;

        // 마스터 클라이언트일 경우, 곧바로 퍼즐 로딩 시도
        TryStartGame();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // 마스터 클라이언트가 아니거나 이미 게임 시작했으면 무시
        TryStartGame();
    }

    /// <summary>
    /// 마스터 클라이언트가 “최대 인원이 다 찼고 아직 시작되지 않았다면” 한 번만 퍼즐을 로딩해 준다.
    /// </summary>
    private void TryStartGame()
    {
        if (!PhotonNetwork.IsMasterClient || isGameStarted)
            return;

        // 방에 필요한 인원이 모두 채워지면
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            isGameStarted = true;
            Debug.Log("모든 플레이어 접속 완료. 게임 시작!");
            // 랜덤으로 Person(퍼즐 이미지 종류 결정)
            person = (Person)Random.Range(0, (int)Person.MAX);

            // 퍼즐 초기화
            imageSlicer.Init(person);

            // 퍼즐 조각이 생성된 직후 한 프레임 대기 후, totalPieces를 세팅
            StartCoroutine(SetTotalPiecesCoroutine());
        }
    }

    /// <summary>
    /// 한 프레임 대기 후 씬 내에 존재하는 Piece 개수를 세서 totalPieces에 저장
    /// → placedPieces는 0으로 초기화
    /// </summary>
    private IEnumerator SetTotalPiecesCoroutine()
    {
        yield return null; // 한 프레임 기다린 뒤
        // Scene 상에서 ‘Piece’ 컴포넌트가 붙은 모든 오브젝트를 찾아서 개수 파악
        Piece[] pieces = FindObjectsOfType<Piece>();
        totalPieces = pieces.Length;
        placedPieces = 0;
        Debug.Log($"[StageManager] 퍼즐 조각 총 개수: {totalPieces}");
    }

    /// <summary>
    /// ‘맞춘 조각’이 생길 때마다 호출되는 RPC 진입점
    /// → RpcTarget.MasterClient 로만 수신됨
    /// </summary>
    [PunRPC]
    public void OnPiecePlacedRPC()
    {
        OnPiecePlacedLocal();
    }

    /// <summary>
    /// MasterClient 내에서 local하게 맞춘 조각 수를 증가시키고, 
    /// 모두 맞춘 시점에 다음 퍼즐 로딩 루틴을 실행
    /// </summary>
    private void OnPiecePlacedLocal()
    {
        placedPieces++;
        Debug.Log($"[StageManager] 현재 맞춘 퍼즐 조각: {placedPieces}/{totalPieces}");

        if (placedPieces >= totalPieces)
        {
            Debug.Log("[StageManager] 모든 퍼즐 조각을 맞춤! 다음 퍼즐 로딩...");
            // 코루틴으로 실제 “다음 퍼즐 로딩” 수행
            StartCoroutine(LoadNextPuzzleCoroutine());
        }
    }

    /// <summary>
    /// 모든 퍼즐 조각이 맞춰진 뒤 실행될 코루틴.
    /// 1) 기존 퍼즐 조각과 퍼즐 배경을 네트워크에서 안전하게 모두 제거(PUN Destroy)
    /// 2) 다음 Person(퍼즐 이미지 종류)를 랜덤으로 골라서 imageSlicer.Init(person) 호출
    /// 3) 한 프레임 기다린 뒤 totalPieces 새로 얻기
    /// </summary>
    private IEnumerator LoadNextPuzzleCoroutine()
    {
        // (1) 기존 퍼즐 조각과 배경을 모두 파괴
        //    → PUN 오브젝트일 경우 PhotonNetwork.Destroy, 일반 오브젝트면 Destroy
        //    → Piece.cs에는 이미 PhotonView가 붙어 있다고 가정
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

        // PuzzleBackground 객체도 같이 지워 줘야 함
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

        yield return null; // 파괴가 확실히 반영될 수 있도록 한 프레임 쉼

        // (2) 새 퍼즐 이미지(Person) 선택 (이전과 같은 이미지는 제외)
        Person newPerson;
        do
        {
            newPerson = (Person)Random.Range(0, (int)Person.MAX);
        } while (newPerson == person);

        person = newPerson;
        imageSlicer.Init(person);

        // (3) 한 프레임 기다려서 씬에 새롭게 생성된 Piece를 카운트
        yield return null;
        Piece[] newPieces = FindObjectsOfType<Piece>();
        totalPieces = newPieces.Length;
        placedPieces = 0;
        Debug.Log($"[StageManager] 새 퍼즐 로드됨. 총 조각 개수: {totalPieces}");
    }
}
