#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIDialog : UIWindow
{
    public override void OnCloseClicked()
    {
        if (windowType != WindowType.None)
            UIManager.OnCloseDialog(this);
    }

    public override void OpenWindow()
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);

            UIManager.OnDialogOpened(windowType);

            if (windowBehaviour != null)
                windowBehaviour.OnOpenWindow();
        }
    }

    public override void OnClosing()
    {
        UIManager.OnDialogClosing(windowType);

        if (!HasCloseAnimation)
            OnCloseClicked();

        if (windowBehaviour != null)
            windowBehaviour.OnCloseWindow();
    }

    protected override void ValidateWindowType()
    {
#if UNITY_EDITOR
        if ((windowType < WindowType.DialogStart || windowType > WindowType.DialogEnd) && windowType != WindowType.None)
        {
            EditorUtility.DisplayDialog("Window Type Warning", "Please double check[" + gameObject.name + "] windowtype is of dialog type", "ok");
        }
#endif
    }
}
