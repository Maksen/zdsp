using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Button))]
public class OpenWindowButton : MonoBehaviour
{
    public WindowType openWindowType = WindowType.None;
    public LinkUIType LinkUIType = LinkUIType.None;

    private Button button;

    void Awake()
    {

#if !ZEALOT_DEVELOPMENT
        if (openWindowType == WindowType.ConsoleCommand)
        {
            this.gameObject.SetActive(false);
            return;
        }
#endif
        button = this.gameObject.GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnOpenClick);
    }

    public virtual void OnOpenClick()
    {
        if (openWindowType != WindowType.None)
        {
            //if (LinkUIType == LinkUIType.None)
                UIManager.OpenWindow(openWindowType);
            //else
            //    ClientUtils.OpenUIWindowByLinkUI(LinkUIType);
        }
    }

    protected virtual void ValidateWindowType()
    {
#if UNITY_EDITOR
        if (openWindowType >= WindowType.DialogStart && openWindowType <= WindowType.DialogEnd)
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