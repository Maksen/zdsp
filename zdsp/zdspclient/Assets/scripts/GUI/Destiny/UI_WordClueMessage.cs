using UnityEngine;
using UnityEngine.UI;

public class UI_WordClueMessage : MonoBehaviour
{
    [SerializeField]
    Text Message;

    public void Init(string msg)
    {
        Message.text = msg;
    }
}
