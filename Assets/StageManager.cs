using Photon.Pun;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class StageManager : MonoBehaviourPun
{
    [SerializeField]
    private GameObject playerPerfab;
    [SerializeField]
    private CinemachineCamera cinemachineCamera;
    void Start()
    {
        StartCoroutine(Init());
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
}
