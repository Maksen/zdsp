using UnityEngine;
using UnityEngine.UI;

public class UI_EventNotification : MonoBehaviour
{
    [SerializeField] Text messageText;

    public void ShowMessage(string message)
    {
        gameObject.SetActive(false);
        messageText.text = message;
        gameObject.SetActive(true);
    }

    public void OnFinished()
    {
        gameObject.SetActive(false);
    }
}
