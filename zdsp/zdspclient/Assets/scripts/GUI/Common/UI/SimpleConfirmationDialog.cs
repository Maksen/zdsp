using System;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public delegate void OnClickButtonCallBackDelegate();

[Obsolete("Please use UI_YesNoDialog instead")]
public class SimpleConfirmationDialog : MonoBehaviour
{
    public Text Message;
    public Button ButtonYes;
    [NonSerialized]
    public GameObject mSubscriber;
    [NonSerialized]
    public bool hideOnAwake = true;
    
    private OnClickButtonCallBackDelegate mOnClickYesCallBack;

    private bool mbKeepOpen; //in case callback will need this dialog again.
    private string mMesssage = "";
    private int mCountDown = 0;
    private float mCountDownStart = 0.0f;
    private int mElapsed = -1;

    void Awake()
    {
        if (hideOnAwake)
            gameObject.SetActive(false);
    }

    void OnDestroy()
    {        
        Message = null;
        mSubscriber = null;
        mOnClickYesCallBack = null;
        ButtonYes = null;
    }
    
    public void InitOk(GameObject subscriber, string message, OnClickButtonCallBackDelegate okcallback, string title="")
    {
        mMesssage = message;
        mbKeepOpen = true;
        mSubscriber = subscriber;
        Message.text = message;
        mOnClickYesCallBack = okcallback;
        if(ButtonYes != null)
            ButtonYes.gameObject.SetActive(true);
        enabled = false;
        gameObject.SetActive(true);
    }

    public void OnClicked_Yes()
    {
        mbKeepOpen = false;
        if (mOnClickYesCallBack != null)
            mOnClickYesCallBack();
        if (!mbKeepOpen)
            gameObject.SetActive(false);
    }
    
    void OnDisable()
    {
        mSubscriber = null;
        mOnClickYesCallBack = null;
    }

    void Update()
    {
        int elapsed = (int)(Time.time - mCountDownStart);
        if (mElapsed != elapsed)
        {
            mElapsed = elapsed;
            int left = mCountDown - mElapsed;
            if (left > 0)
                Message.text = mMesssage + "\n" + GUILocalizationRepo.GetLocalizedString("com_Confirmation_Countdown").Replace("{countdown}", left.ToString());
        }
    }
}
