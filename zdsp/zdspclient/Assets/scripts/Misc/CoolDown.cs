using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class CoolDown : MonoBehaviour {

    public Text mDisplayText;
    public Image mDisplayBar;
	// Use this for initialization
	void Start () {


    }
	
    public void SetDisplayText(string text)
    {
        mDisplayText.text = text;
    }

    public void SetDisplayBarFillAmount(float amount)
    {
        mDisplayBar.fillAmount = amount;
    }

    public string GetDisplayText()
    {
        return mDisplayText.text;
    }

    public float GetDisplayBarFillAmount()
    {
        return mDisplayBar.fillAmount;
    }

    public void StartCoolDown(int duration, Action cooldowncallback)
    {
        mDisplayBar.fillAmount = 0;
        mDisplayText.text = duration.ToString();
        GameInfo.gCombat.StartCoroutine(Cooldown(duration, cooldowncallback));
    }

    IEnumerator Cooldown(int duration, Action cooldowncallback)
    {
        //fill amount to increase 1 percent of 1
        //wait for second to wait for 1 percent of the duration
        float percent = 1.0f * duration / 100.0f;
        float textpercent = 1.0f / duration;
        float fillpercent = 0.01f;
        float totalcount = 0;
        int tempduration = duration;

        while (true)
        {
            yield return new WaitForSeconds(percent);
            mDisplayBar.fillAmount += fillpercent;
            totalcount += fillpercent;
            if (totalcount >= textpercent)
            {
                tempduration--;
                mDisplayText.text = tempduration.ToString();
                totalcount = 0;
            }

            if (tempduration <= 0)
                break;
        }

        cooldowncallback();
    }
}
