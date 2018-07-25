using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

[RequireComponent(typeof(Image))]
public class UI_SilderImages : MonoBehaviour
{
    private Image image;
    private int currentIdx = 0;
    private Coroutine autoShowImage;
    public int intervalSec = 5;
    [SerializeField]
    private List<Sprite> imageList;

    [Serializable]
    public class OnNextEventInt : UnityEvent<int>
    {
    }

    /// <summary>
    /// On Next Event.
    /// </summary>
    [SerializeField]
    public OnNextEventInt onNextShowed = new OnNextEventInt();

    public int CurrentIndex
    {
        get { return currentIdx; }
        set
        {
            if (value >= 0 && value < imageList.Count && value != currentIdx)
            {
                currentIdx = value;
                ShowCurrent();
            }
        }
    }

    public int ImageCount
    {
        get
        {
            if (imageList != null)
                return imageList.Count;
            return 0;
        }
    }

    public bool IsFirst { get { return currentIdx == 0; } }
    public bool IsLast { get { return currentIdx + 1 == ImageCount; } }


    void Awake()
    {
        image = gameObject.GetComponent<Image>();
        if (imageList == null)
            imageList = new List<Sprite>();
    }

    void OnDestroy()
    {
        StopPlay();
        image = null;
    }

    void OnEnable()
    {
    }

    //-----------------------------------------------

    public void ShowNext()
    {
        currentIdx++;
        ShowCurrent();
        ResetAutoShowNextTime();
        onNextShowed.Invoke(currentIdx);
    }

    public void ShowPrev()
    {
        if (currentIdx > 0)
            currentIdx--;
        else
            currentIdx = imageList.Count - 1;
        ShowCurrent();
        ResetAutoShowNextTime();
    }

    public void ShowCurrent()
    {
        if (imageList.Count <= 0)
            return;
        currentIdx = currentIdx % imageList.Count;
        if (currentIdx >= 0 && currentIdx < imageList.Count)
            image.sprite = imageList[currentIdx];
    }

    public void StartPlay()
    {
        ShowCurrent();
        ResetAutoShowNextTime();
    }

    public void StopPlay()
    {
        if (autoShowImage != null)
        {
            StopCoroutine(autoShowImage);
            autoShowImage = null;
        }
    }

    //-----------------------------------------------

    public void AddImage(params Sprite[] images)
    {
        for (int i = 0; i < images.Length; i++)
        {
            if(images[i])
                imageList.Add(images[i]);
        }
    }

    public void ClearImages()
    {
        imageList.Clear();
        currentIdx = 0;
    }

    IEnumerator DoNextShow(int intervalSeconds)
    {
        yield return new WaitForSeconds(intervalSeconds);
        ShowNext();
    }

    private void ResetAutoShowNextTime()
    {
        //重置變更時間(先停掉再重設)
        StopPlay();
        if (intervalSec > 0)
            autoShowImage = StartCoroutine(DoNextShow(intervalSec));
    }

    public void AddOnNextCallback(UnityAction<int> callback)
    {
        onNextShowed.AddListener(callback);
    }
}
