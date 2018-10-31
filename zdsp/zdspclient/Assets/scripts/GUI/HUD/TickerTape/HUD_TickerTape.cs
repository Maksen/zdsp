using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD_TickerTape : MonoBehaviour
{
    [SerializeField] Text messageText;
    [SerializeField] float translateSpeed = 1;

    private Transform messageTransform;
    private RectTransform messageRectTransform;
    private Vector3 originalPosition;
    private float parentWidth;
    private Queue<string> buffer = new Queue<string>();
    private Coroutine coroutine;
    private Color textColor;

    private void Awake()
    {
        messageRectTransform = messageText.GetComponent<RectTransform>();
        messageTransform = messageText.transform;
        originalPosition = messageTransform.localPosition;
        parentWidth = messageText.transform.parent.GetComponent<RectTransform>().sizeDelta.x;
        textColor = messageText.color;
    }

    public void AddTickerTapeMessage(string message)
    {
        buffer.Enqueue(message);

        if (coroutine == null)
            ShowMessage();
    }

    public void ShowMessage()
    {
        messageText.color = Color.clear;
        messageText.text = buffer.Dequeue();
        UIManager.SetWidgetActive(HUDWidgetType.TickerTape, true);
        StartCoroutine(WaitForGUIUpdate());
    }

    private IEnumerator WaitForGUIUpdate()
    {
        yield return null;
        yield return null;
        float endPosX = -parentWidth - messageRectTransform.sizeDelta.x - originalPosition.x;
        messageText.color = textColor;
        coroutine = StartCoroutine(Translate(endPosX));
    }

    private IEnumerator Translate(float endPosX)
    {
        while (messageTransform.localPosition.x > endPosX)
        {
            messageTransform.Translate(Vector3.left * translateSpeed * Time.deltaTime);
            yield return null;
        }

        messageTransform.localPosition = originalPosition;

        if (buffer.Count > 0)
            ShowMessage();
        else
            UIManager.SetWidgetActive(HUDWidgetType.TickerTape, false);
    }

    private void OnDisable()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        buffer.Clear();
    }
}