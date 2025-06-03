using UnityEngine;
using UnityEngine.UI;

public class UIStartPanel : UIBase
{
    [SerializeField]
    private Button multiPlayButton;
    [SerializeField]
    private Button singlePlayButton;
    [SerializeField]
    private Button quitButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
    }

    public override void Init()
    {
        multiPlayButton.onClick.AddListener(OnClickedMultiPlayButton);
        singlePlayButton.onClick.AddListener(OnClickedSinglePlayButton);
        quitButton.onClick.AddListener(OnClickedQuitButton);
    }

    private void OnClickedMultiPlayButton()
    {

    }

    private void OnClickedSinglePlayButton()
    {
        PhotonNetworkManager.Instance.ConnectPhotonToSinglePlay();
    }

    private void OnClickedQuitButton()
    {
#if UNITY_EDITOR
        // �����Ϳ��� ���� ���� ��� ������ ����
        UnityEditor.EditorApplication.isPlaying = false;
#else
    // ����� ���ӿ����� ����
    Application.Quit();
#endif
    }

}
