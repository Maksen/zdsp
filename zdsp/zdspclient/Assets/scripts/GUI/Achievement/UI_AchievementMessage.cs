using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_AchievementMessage : MonoBehaviour
{
    [SerializeField] Text messageText;

    private Queue<string> buffer = new Queue<string>();

    public void ShowMessage(string message)
    {
        buffer.Enqueue(message);
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        if (buffer.Count > 0)
            messageText.text = buffer.Dequeue();
    }

    // Called by animation on last frame
    public void OnFinished()
    {
        if (buffer.Count > 0)
            gameObject.SetActive(true);
    }
}
