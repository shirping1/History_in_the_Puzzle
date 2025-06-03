using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    public static PhotonNetworkManager Instance { get; private set; }

    private string gameVersion = "1";

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

    public override void OnConnectedToMaster()
    {
        Debug.Log("���� ������ ������ �����");

        if (!PhotonNetwork.OfflineMode)  // �¶����� ��쿡�� �κ� ����
        {
            PhotonNetwork.JoinLobby();
            Debug.Log("���� �κ� ���� �õ�");
        }
    }

    public void ConnectPhotonToSinglePlay()
    {
        PhotonNetwork.OfflineMode = true;
        PhotonNetwork.GameVersion = gameVersion;

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 1;

        PhotonNetwork.CreateRoom("�̱��÷���", roomOptions);

        Debug.Log("�̱� �÷��� (�������� ���)�� ���� ���� �� �� ����");
    }

    public void ConnectPhoton()
    {
        PhotonNetwork.OfflineMode = false; // �¶��� ���
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();

        Debug.Log("���漭�� ���� �õ� (�¶���)");
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.OfflineMode)
        {
            PhotonNetwork.LoadLevel("Stage");
        }

        //if (!PhotonNetwork.OfflineMode)
        //{
        //    //UIManager.Instance.OpenPanelInOverlayCanvas<UIRoomPanel>();
        //}
        //if (PhotonNetwork.IsMasterClient)
        //{
        //    //photonNetworkManager = PhotonNetwork.InstantiateRoomObject("PhotonNetworkManager", Vector3.zero, Quaternion.identity);
        //    //DontDestroyOnLoad(photonNetworkManager);
        //}
    }

    public override void OnLeftRoom()
    {
        //if (photonNetworkManager != null)
        //{
        //    Debug.Log("photonNetworkManager ����");
        //    Destroy(photonNetworkManager);
        //}
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
