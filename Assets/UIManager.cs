using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private Stack<UIBase> uiStack = new Stack<UIBase>();

    [SerializeField]
    private Canvas canvas;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        OpenPanel<UIStartPanel>();
    }

    public T OpenPanel<T>(bool additive = false) where T : UIBase
    {
        string prefabName = typeof(T).Name;
        GameObject prefab = Resources.Load<GameObject>($"UI/{prefabName}");

        GameObject panelInstance = Instantiate(prefab, canvas.transform, false);
        T panel = panelInstance.GetComponent<T>();

        if (!additive) ClosePeekUI();

        uiStack.Push(panel);
        return panel;
    }

    public void ClosePeekUI()
    {
        if (uiStack.Count > 0)
        {
            Destroy(uiStack.Pop().gameObject);
        }

        if (uiStack.Count > 0)
        {
            uiStack.Peek().gameObject.GetComponent<CanvasGroup>().interactable = true;
        }
    }

    public void CloseAllUI()
    {
        while (uiStack.Count > 0)
        {
            UIBase ui = uiStack.Pop();

            if (ui != null && ui.gameObject != null)
            {
                Destroy(ui.gameObject);
            }
        }
    }

    public T OpenPopupPanel<T>() where T : UIBase
    {
        if (uiStack.Count > 0)
            uiStack.Peek().gameObject.GetComponent<CanvasGroup>().interactable = false;

        string prefabName = typeof(T).Name;
        GameObject prefab = Resources.Load<GameObject>($"UI/{prefabName}");

        GameObject panelInstance = Instantiate(prefab);
        panelInstance.transform.SetParent(canvas.transform, false);

        T popup = panelInstance.GetComponent<T>();
        uiStack.Push(popup);

        return popup;
    }

    public UIBase ReturnPeekUI()
    {
        if (uiStack.Count > 0)
            return uiStack.Peek();
        else
            return null;
    }

    public void CloseAllAndOpen<T>() where T : UIBase
    {
        CloseAllUI();
        OpenPanel<T>();
    }
}
