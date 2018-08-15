using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIWindow : MonoBehaviour
{
    public WindowType windowType = WindowType.None;
    public Button[] closeButton;
    public Canvas[] windowCanvas;
    public GameObject[] objectsToHide;
    public bool HasCloseAnimation = false;

    protected BaseWindowBehaviour windowBehaviour;

    public BaseWindowBehaviour GetWindowBehaviour()
    {
        return windowBehaviour;
    }

    /// <summary>
    /// added as an event handler to GUIAnimEvent OnFinished event
    /// </summary>
    public virtual void OnCloseClicked()
    {
        if (windowType != WindowType.None)
            UIManager.OnCloseWindow(this);
    }

    #region UIManager
    public virtual void RegisterWindow()
    {
        if (windowType != WindowType.None)
        {
#if !ZEALOT_DEVELOPMENT
            if (windowType == WindowType.ConsoleCommand)
                return;
#endif
            for (int i = 0; i < closeButton.Length; i++)
            {
                if (closeButton[i] != null)
                    closeButton[i].onClick.AddListener(OnClosing);
            }               

            windowBehaviour = gameObject.GetComponent<BaseWindowBehaviour>();
            UIManager.RegisterWindow(this);
            if (windowBehaviour != null)
                windowBehaviour.OnRegister();
        }
    }

    public virtual void OpenWindow()
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);  // Show window

            var topWindow = UIManager.GetTopWindowBehaviour();  // any previous window
            if (topWindow != null)
                topWindow.OnHideWindow();

            UIManager.OnWindowOpened(windowType);  // Push into window queue

            if (windowBehaviour != null)  // Window to be opened
            {
                windowBehaviour.OnOpenWindow();
                windowBehaviour.OnShowWindow();
                windowBehaviour.hasBeenActive = true;
            }
        }
    }

    public void CloseWindow()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Call this to close window by code
    /// </summary>
    public void ClickClose()
    {
        if (closeButton.Length > 0 && closeButton[0] != null)
            closeButton[0].onClick.Invoke();
    }

    public void ShowCanvas(bool show)
    {
        for (int i = 0; i < windowCanvas.Length; i++)
        {
            if (windowCanvas[i] != null)
                windowCanvas[i].enabled = show;
        }

        for (int i = 0; i < objectsToHide.Length; i++)
        {
            if (objectsToHide[i] != null)
                objectsToHide[i].SetActive(show);
        }
    }
    #endregion

    public virtual void OnClosing()  // called by close button
    {
        UIManager.OnWindowClosing(windowType);  // remove window from queue and show previous window

        if (!HasCloseAnimation)
            OnCloseClicked();   // set window inactive, no need to wait for close animation

        if (windowBehaviour != null)
        {
            windowBehaviour.OnCloseWindow();
            windowBehaviour.OnHideWindow();
        }

        var topWindow = UIManager.GetTopWindowBehaviour();
        if (topWindow != null)
            topWindow.OnShowWindow();
    }

    public void OnLevelChanged()
    {
        if (windowBehaviour != null && windowBehaviour.hasBeenActive)
        {
            windowBehaviour.OnLevelChanged();
            windowBehaviour.hasBeenActive = false;
        }
    }

    protected virtual void ValidateWindowType()
    {
#if UNITY_EDITOR
        if (windowType >= WindowType.DialogStart && windowType <= WindowType.DialogEnd && windowType != WindowType.None)
        {
            EditorUtility.DisplayDialog("Window Type Warning", "Please double check [" + gameObject.name + "] windowtype is NOT of dialog type", "ok");
        }
#endif
    }

    void OnValidate()
    {
        ValidateWindowType();
    }
}
