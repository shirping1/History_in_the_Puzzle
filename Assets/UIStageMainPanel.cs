using UnityEngine;
using UnityEngine.UI;

public class UIStageMainPanel : UIBase
{
    [SerializeField]
    private Text playerWaitText;
    [SerializeField]
    private Text timerText;
    [SerializeField]
    private Text clearText;

    private void Start()
    {
        clearText.gameObject.SetActive(false);
    }

    public void ClosePlayerWaitText()
    {
        playerWaitText.gameObject.SetActive(false);
    }

    public void SetTimerText(float time)
    {
        timerText.text = Mathf.CeilToInt(time).ToString();
    }

    public void SetClearText(bool value)
    {
        clearText.gameObject.SetActive(value);
    }
}
