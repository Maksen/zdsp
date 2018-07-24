using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SoftMasking;

public class HUD_TickerTape : MonoBehaviour {

    public Text mAnnoucementText;
    public RectTransform mAnnoucementTrans;
    public float mMoveSpeed;

    Vector2 mStartingPos;
    Vector2 mParentAnnoucementTextPos;
    bool mIsShowing;

    static Vector2 mEndingPos;
    static bool mIsAnoucementExist = false;
    static Queue<string> mAnnoucementQueue = new Queue<string>();
    static string mCurrentText = "";
    static Vector3 mCurrentPos = Vector3.zero;
    // Use this for initialization
    void Awake () {
        mIsShowing = false;
        StartCoroutine(DelayInit());
    }

    IEnumerator DelayInit()
    {
        yield return new WaitForSeconds(0.5f);
        mStartingPos = mAnnoucementText.GetComponent<RectTransform>().anchoredPosition;
        mParentAnnoucementTextPos = mAnnoucementTrans.parent.GetComponent<RectTransform>().sizeDelta;
        if (string.IsNullOrEmpty(mCurrentText) == false)
        {
            mAnnoucementText.text = mCurrentText;
            mAnnoucementText.transform.localPosition = mCurrentPos;
            mIsShowing = true;
        }
        else
        {
            gameObject.SetActive(false);
            mIsShowing = false;
        }
           
    }

    void OnDestroy()
    {
        if (gameObject != null)
        {
            SoftMask softmask = gameObject.GetComponent<SoftMask>();
            //if (softmask != null)
            //    softmask.Clear();
        }
    }

	// Update is called once per frame
	void Update () {

        if (mIsShowing == true)
        {
            if (mAnnoucementTrans.anchoredPosition.x < -mEndingPos.x)//ended
            {

                if (mAnnoucementQueue.Count > 0)
                {
                    mAnnoucementText.text = mAnnoucementQueue.Dequeue();
                    mCurrentText = mAnnoucementText.text;
                    mIsShowing = false;
                    mAnnoucementText.gameObject.SetActive(false);
                    StartCoroutine(Delay());
                }
                else
                {
                    mCurrentText = "";
                    mCurrentPos = Vector3.zero;
                    mIsShowing = false;
                    mIsAnoucementExist = false;
                    gameObject.SetActive(false);
                }
            }
            else
            {
                mAnnoucementText.transform.Translate(Vector3.left * Time.deltaTime * mMoveSpeed);
                mCurrentPos = mAnnoucementText.transform.localPosition;
            }
        }

    }

    public void SetAnnoucementText(string text)
    {
        if (mIsShowing == false && mIsAnoucementExist == false)
        {
            gameObject.SetActive(true);
            mAnnoucementText.gameObject.SetActive(false);
            mAnnoucementText.text = text;
            mCurrentText = text;
            mIsAnoucementExist = true;
            StartCoroutine(Delay());
        }
        else//ongoing annoucement
        {
            mAnnoucementQueue.Enqueue(text);
        }
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(0.5f);
        Vector2 offset = mAnnoucementTrans.sizeDelta / 2;
        mAnnoucementTrans.anchoredPosition = new Vector2(mStartingPos.x + offset.x, mAnnoucementTrans.anchoredPosition.y);
        mEndingPos = mParentAnnoucementTextPos + offset;
        mAnnoucementText.gameObject.SetActive(true);
        mIsShowing = true;
    }

    
}
