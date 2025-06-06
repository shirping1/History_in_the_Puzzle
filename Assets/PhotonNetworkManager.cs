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
        Debug.Log("���� ������ ������ �����");

        if (!PhotonNetwork.OfflineMode)  // �¶����� ��쿡�� �κ� ����
        {
            PhotonNetwork.JoinLobby();
            Debug.Log("���� �κ� ���� �õ�");
        }
    }

    public void SinglePlay()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 1;

        PhotonNetwork.CreateRoom("�̱��÷���", roomOptions);

        Debug.Log("�̱� �÷��̷� ���� ���� �� �� ����");
    }

    public void MultuPlay()
    {
        PhotonNetwork.OfflineMode = false; // �¶��� ���
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("�� ���� ���� �� ���� �����մϴ�.");
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;

        PhotonNetwork.CreateRoom(null, roomOptions); // ������ �̸����� �� ����
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
            Debug.Log("rpc_Handler ����");
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
