#if UNITY_EDITOR
using UnityEditor;
#endif

public class OpenDialogButton : OpenWindowButton
{
    public override void OnOpenClick()
    {
        if (openWindowType != WindowType.None)
            UIManager.OpenDialog(openWindowType);
    }

    protected override void ValidateWindowType()
    {
#if UNITY_EDITOR
        if (openWindowType < WindowType.DialogStart || openWindowType > WindowType.DialogEnd)
        {
            EditorUtility.DisplayDialog("Window Type Warning", "Please double check[" + gameObject.name + "] windowtype is of dialog type", "ok");
        }
#endif
    }
}