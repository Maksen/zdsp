using UnityEngine;
using System.Collections;
using System;

public class Hud_ProgressBar : MonoBehaviour
{
    [SerializeField]
    UI_ProgressBarC ProgressBar;

    private Action mCallback;
    private float mTotalTime;
    private float mCurrentTime;
    private long mMaxValue;
    private bool bStart;

    public void InitTimeBar(float timeinsec, Action callback)
    {
        mCurrentTime = 0;
        mTotalTime = timeinsec;
        mCallback = callback;

        ProgressBar.Max = mMaxValue = 100;
        ProgressBar.Value = 0;
        ProgressBar.Type = ProgressbarCTypes.NoText;
        bStart = true;
    }

    private void Update()
    {
        if (bStart)
        {
            mCurrentTime += Time.deltaTime;
            float v = (mCurrentTime / mTotalTime) * mMaxValue;
            ProgressBar.Value = (long)v;

            if (v >= mMaxValue)
            {
                bStart = false;
                TimerEnd();
            }
        }
    }

    private void TimerEnd()
    {
        if (mCallback != null)
        {
            mCallback();
        }
    }

    public void ForceEnd()
    {
        bStart = false;
        Hud_QuestAction questAction = UIManager.GetWidget(HUDWidgetType.QuestAction).GetComponent<Hud_QuestAction>();
        questAction.SetButtonStatus(true);
    }
}
