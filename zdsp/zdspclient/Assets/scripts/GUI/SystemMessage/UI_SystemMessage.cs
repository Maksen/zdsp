using UnityEngine;
using UnityEngine.UI;

public class UI_SystemMessage : MonoBehaviour
{
    [SerializeField] Text messageText;

    public void ShowMessage(string message)
    {
        messageText.text = message;
        gameObject.SetActive(true);
    }

    public void OnFinished()
    {
        gameObject.SetActive(false);
    }
}
