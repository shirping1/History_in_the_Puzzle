using UnityEngine;
using UnityEngine.UI;

public class UIStageMainPanel : UIBase
{
    [SerializeField]
    private Text playerWaitText;
    [SerializeField]
    private Text timerText;

    public void ClosePlayerWaitText()
    {
        playerWaitText.gameObject.SetActive(false);
    }

    public void SetTimerText(float time)
    {
        timerText.text = Mathf.CeilToInt(time).ToString();
    }
}
