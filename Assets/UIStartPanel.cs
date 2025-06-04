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
        multiPlayButton.interactable = false;
        singlePlayButton.interactable = false;

        PhotonNetworkManager.Instance.MultuPlay();
    }

    private void OnClickedSinglePlayButton()
    {
        multiPlayButton.interactable = false;
        singlePlayButton.interactable = false;

        PhotonNetworkManager.Instance.SinglePlay();
    }

    private void OnClickedQuitButton()
    {
#if UNITY_EDITOR
        // 에디터에서 실행 중일 경우 에디터 정지
        UnityEditor.EditorApplication.isPlaying = false;
#else
    // 빌드된 게임에서는 종료
    Application.Quit();
#endif
    }

}
