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
    private PuzzleImageSlicer imageSlicer;

    bool isGameStarted = false;

    public static StageManager Instance;

    public UIStageMainPanel mainPanel;

    public Person person;

    private Coroutine timerCoroutine;

    private int maxPiece = 9;
    private int lockPiece = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
            Destroy(this);
    }

    void Start()
    {
        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        while (!PhotonNetwork.InRoom)
            yield return null;

        UIManager.Instance.CloseAllUI();
        mainPanel = UIManager.Instance.OpenPanel<UIStageMainPanel>();

        GameObject player = PhotonNetwork.Instantiate(playerPerfab.name, Vector3.zero, Quaternion.identity);
        cinemachineCamera.Follow = player.transform;
        cinemachineCamera.LookAt = player.transform;

        TryStartGame();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TryStartGame();
    }

    private void TryStartGame()
    {
        if (!PhotonNetwork.IsMasterClient || isGameStarted)
            return;

        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            isGameStarted = true;
            Debug.Log("모든 플레이어 접속 완료. 게임 시작!");
            person = (Person)Random.Range(0, (int)Person.MAX);// 0 이상 max 미만

            RPC_Handler.Instance.photonView.RPC(nameof(RPC_Handler.Instance.RPC_ClosePlayerWaitText), RpcTarget.AllBuffered);

            if(PhotonNetwork.CurrentRoom.MaxPlayers == 1)
            {
                timerCoroutine = StartCoroutine(GameTimer(90f));
            }
            else if(PhotonNetwork.CurrentRoom.MaxPlayers == 2)
            {
                timerCoroutine = StartCoroutine(GameTimer(60f));
            }

            imageSlicer.Init(person);
        }
    }

    public void ClosePlayerWaitTextInMainPanel()
    {
        mainPanel.ClosePlayerWaitText();
    }

    IEnumerator GameTimer(float time)
    {
        while (time >= 0 && isGameStarted)
        {
            RPC_Handler.Instance.photonView.RPC(nameof(RPC_Handler.Instance.RPC_SyncTimer), RpcTarget.AllBuffered, time);
            yield return new WaitForSeconds(1f);
            time -= 1f;
            Debug.Log("게임 시간" + time);
        }
    }

    public void LockPieceCount()
    {
        lockPiece++;

        if(lockPiece == maxPiece)
        {
            isGameStarted = false;
            StopCoroutine(timerCoroutine);
            RPC_Handler.Instance.photonView.RPC(nameof(RPC_Handler.Instance.RPC_ShowClearText), RpcTarget.AllBuffered);
        }
    }
}
