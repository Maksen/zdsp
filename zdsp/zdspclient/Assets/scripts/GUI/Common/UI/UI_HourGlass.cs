using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class UI_HourGlass : MonoBehaviour
{
    public Text message;

    private Coroutine countdownTimer;
    private Action mTimeoutCallback;

    private void Awake()
    {
        message.transform.parent.gameObject.SetActive(false);
    }

    public void Show(float duration, string msg, Action timeoutCallback)
    {
        if (countdownTimer != null)
            StopCoroutine(countdownTimer);
      
        gameObject.SetActive(true);

        if (!string.IsNullOrEmpty(msg))
        {
            message.text = msg;
            message.transform.parent.gameObject.SetActive(true);
        }

        mTimeoutCallback = timeoutCallback;

        if (duration > 0)
            countdownTimer = StartCoroutine(Countdown(duration));
    }

    public void Stop()
    {
        if (countdownTimer != null)
            StopCoroutine(countdownTimer);
  
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (countdownTimer != null)
            StopCoroutine(countdownTimer);

        message.transform.parent.gameObject.SetActive(false);
    }

    private IEnumerator Countdown(float duration)
    {
        yield return new WaitForSeconds(duration);

        Stop();

        if (mTimeoutCallback != null)
            mTimeoutCallback();
    }
}

