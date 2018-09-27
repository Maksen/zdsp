using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameIcon_BuffDebuff : MonoBehaviour
{
    [SerializeField]
    Image buffIconImage;

    [SerializeField]
    Text cdTimeText;

    [SerializeField]
    Text buffAmtText;

    private float timeLeft;
    private Coroutine countdownTimer;
    private bool lateCountdown;

    public void Init(BuffEnum eff, long timeleft, int amount)
    {
        gameObject.SetActive(true);
        buffIconImage.sprite = LoadBuffDebuffIcon(eff);

        StopCountdown();
        lateCountdown = false;
        if (timeleft > 0)
        {
            timeLeft = (float)timeleft / 1000;
            if (gameObject.activeInHierarchy)
                countdownTimer = StartCoroutine(Countdown());
            else
                lateCountdown = true;  // need to wait for gameobject to become active to start coroutine
        }

        buffAmtText.text = amount > 0 ? amount.ToString() : "";
    }

    void OnEnable()
    {
        if (lateCountdown)
        {
            lateCountdown = false;
            countdownTimer = StartCoroutine(Countdown());
        }
    }

    void OnDisable()
    {
        StopCountdown();
    }

    private IEnumerator Countdown()
    {
        while (timeLeft > 0)
        {
            cdTimeText.text = ((int)Math.Ceiling(timeLeft)).ToString();
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        OnCountdownEnd();
    }

    private void OnCountdownEnd()
    {
        cdTimeText.text = "";
        gameObject.SetActive(false);
    }

    private void StopCountdown()
    {
        if (countdownTimer != null)
            StopCoroutine(countdownTimer);
    }

    private Sprite LoadBuffDebuffIcon(BuffEnum eff)
    {
        string path = "";
        switch (eff)
        {
            case BuffEnum.Buff:
                path = "zzz_DefaultTest.tif";
                break;
            case BuffEnum.Debuff:
                path = "zzz_DefaultTest.tif";
                break;
            default:
                break;
        }

        if (!string.IsNullOrEmpty(path))
            return ClientUtils.LoadIcon(string.Format("UI_ZDSP_Icons/Element_Attacks/{0}", path));

        return null;
    }
}
