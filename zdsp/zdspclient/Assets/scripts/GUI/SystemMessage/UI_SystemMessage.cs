using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_SystemMessage : MonoBehaviour
{
    [SerializeField] Text messageText;
    [SerializeField] float translateDuration = 0.15f;

    private float rectTransformHeight;
    private Vector3 originalPosition;
    private bool hasStopped;
    private int translateQueue;
    private Coroutine coroutine;
    private float timer;

    private void Awake()
    {
        rectTransformHeight = GetComponent<RectTransform>().rect.height;
        originalPosition = transform.localPosition;
    }

    public void ShowMessage(string message)
    {
        messageText.text = message;
        gameObject.SetActive(true);
    }

    public void OnFinished()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        transform.localPosition = originalPosition;
        translateQueue = 0;
        hasStopped = false;
        gameObject.SetActive(false);
    }

    public void OnStop()
    {
        hasStopped = true;
        if (translateQueue > 0)
            StartTranslate();
    }

    public void AddTranslateQueue()
    {
        translateQueue++;
        if (hasStopped && coroutine == null)
            StartTranslate();
    }

    private void StartTranslate()
    {
        float startPosY = transform.localPosition.y;
        float endPosY = startPosY + rectTransformHeight;
        timer = 0;
        coroutine = StartCoroutine(TranslateUp(startPosY, endPosY, translateDuration));
    }

    private IEnumerator TranslateUp(float startPosY, float endPosY, float duration)
    {
        while (timer < duration)
        {
            float newPosY = Mathf.Lerp(startPosY, endPosY, timer / duration);
            transform.localPosition = new Vector3(transform.localPosition.x, newPosY, transform.localPosition.z);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = new Vector3(transform.localPosition.x, endPosY, transform.localPosition.z);
        translateQueue--;

        if (translateQueue > 0)
            StartTranslate();
        else
            coroutine = null;
    }
}
