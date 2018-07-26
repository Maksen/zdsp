using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UITimeline : MonoBehaviour
{
    [SerializeField] Button closeButton;

    public void SetCloseCallback(UnityAction callback)
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(callback);
    }
}
