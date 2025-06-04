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

    public Person person;

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

        TryStartGame();
    }

    IEnumerator Init()
    {
        while(!PhotonNetwork.InRoom)
            yield return null;

        UIManager.Instance.CloseAllUI();

        GameObject player = PhotonNetwork.Instantiate(playerPerfab.name, Vector3.zero, Quaternion.identity);
        cinemachineCamera.Follow = player.transform;
        cinemachineCamera.LookAt = player.transform;
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

            imageSlicer.Init(person);
        }
    }
}
