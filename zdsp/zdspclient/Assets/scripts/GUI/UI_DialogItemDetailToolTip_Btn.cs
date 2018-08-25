using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UI_DialogItemDetailToolTip_Btn : MonoBehaviour
{
    public Text Name;
    public Image Image;
    public Button Button;

    public void Init(string name, Sprite icon, UnityAction callback)
    {
        Name.text = name;
        Image.sprite = icon;
        if (callback != null)
            Button.onClick.AddListener(callback);
        Button.onClick.AddListener(OnClicked);
    }

    public void OnClicked()
    {
        UIManager.CloseDialog(WindowType.DialogItemDetail);
    }
}