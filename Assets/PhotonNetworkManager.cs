using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    public static PhotonNetworkManager Instance { get; private set; }

    private string gameVersion = "1";

    [SerializeField]
    private GameObject rpc_Handler;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(this);

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 60;
    }

    private void Start()
    {
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("포톤 마스터 서버에 연결됨");

        if (!PhotonNetwork.OfflineMode)  // 온라인일 경우에만 로비 접속
        {
            PhotonNetwork.JoinLobby();
            Debug.Log("포톤 로비 접속 시도");
        }
    }

    public void SinglePlay()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 1;

        PhotonNetwork.CreateRoom("싱글플레이", roomOptions);

        Debug.Log("싱글 플레이로 포톤 접속 및 방 생성");
    }

    public void MultuPlay()
    {
        PhotonNetwork.OfflineMode = false; // 온라인 모드
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("빈 방이 없어 새 방을 생성합니다.");
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;

        PhotonNetwork.CreateRoom(null, roomOptions); // 랜덤한 이름으로 방 생성
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Stage");

            rpc_Handler = PhotonNetwork.InstantiateRoomObject("RPC_Handler", Vector3.zero, Quaternion.identity);
            DontDestroyOnLoad(rpc_Handler);

        }
    }

    public override void OnLeftRoom()
    {
        if (rpc_Handler != null)
        {
            Debug.Log("rpc_Handler 제거");
            Destroy(rpc_Handler);
        }
    }

    public void CreateRoom(string roomName, RoomOptions roomOptions)
    {
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void SetNickname(string nickname)
    {
        PhotonNetwork.NickName = nickname;
    }

}
