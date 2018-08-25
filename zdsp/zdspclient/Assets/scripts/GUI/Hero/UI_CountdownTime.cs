using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using Zealot.Repository;

public class UI_CountdownTime : MonoBehaviour
{
    [SerializeField] Text timeText;

    private double timeLeft;
    private Coroutine countdownTimer;
    private Action timeUpCallback;

    public void SetTime(double seconds)
    {
        if (countdownTimer != null)
            StopCoroutine(countdownTimer);

        timeText.text = GUILocalizationRepo.GetShortLocalizedTimeString(seconds);
        timeLeft = seconds;
    }

    public void Show(bool val)
    {
        gameObject.SetActive(val);
    }

    public void StartCountdown(Action callback = null)
    {
        timeUpCallback = callback;

        if (countdownTimer != null)
            StopCoroutine(countdownTimer);

        countdownTimer = StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        while (timeLeft > 0)
        {
            timeText.text = GUILocalizationRepo.GetShortLocalizedTimeString(timeLeft);

            timeLeft -= Time.deltaTime;
            yield return null;
        }

        timeText.text = GUILocalizationRepo.GetShortLocalizedTimeString(0);
        OnTimeUp();
    }

    private void OnTimeUp()
    {
        if (timeUpCallback != null)
        {
            timeUpCallback();
            timeUpCallback = null;
        }
    }

    public void StopCountdown()
    {
        if (countdownTimer != null)
            StopCoroutine(countdownTimer);
    }

    private void OnDisable()
    {
        if (countdownTimer != null)
            StopCoroutine(countdownTimer);
    }
}
