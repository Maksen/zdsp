using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using Zealot.Audio;

public class MoviePlayback : MonoBehaviour
{
    public bool startnow;
#if UNITY_EDITOR || UNITY_STANDALONE
    public MovieTexture mt;
#endif

    public string movieFileName;
    public Color backgroundColor = Color.black;

    public float SkipDuration = 3.0f;
    public GameObject SkipButton;

    private Action OnFinishedCallback;
    private float mRemainTime;


#if UNITY_ANDROID || UNITY_IPHONE
    public FullScreenMovieControlMode controlMod = FullScreenMovieControlMode.CancelOnInput;
    public FullScreenMovieScalingMode scalingMod = FullScreenMovieScalingMode.AspectFit;
#endif

    void Start()
    {                
        if (startnow)
        {
            StartPlay(OnFinished);
        }
    }

    void LateUpdate()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (mt != null && mt.isPlaying)
        {
            if (mRemainTime > 0)
            {
                mRemainTime -= Time.deltaTime;
                if (mRemainTime <= 0)
                {
                    SkipButton.SetActive(true);
                }
            }
        }
#endif
    }

    void OnFinished()
    {
        Debug.Log("Finished playing movie");
    }

    void OnDestroy()
    {
        GetComponent<RawImage>().texture = null;
        GetComponent<AudioSource>().clip = null;
#if UNITY_EDITOR || UNITY_STANDALONE
        mt = null;
#endif
    }

    public void StartPlay(Action callback, float skipduration = 3.0f)
    {        
        OnFinishedCallback = callback;
        SkipDuration = skipduration;
        mRemainTime = SkipDuration;
        //String c = GameInfo.mChar;

#if UNITY_EDITOR || UNITY_STANDALONE
        PlayEditorMode();
#elif UNITY_ANDROID || UNITY_IPHONE
        Debug.Log("UNIT_ANDROID PlayMobileMode");
        PlayMobileMode();
#endif
    }

    public void PlayEditorMode()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        Debug.Log("PlayEditorMode");
        if (mt == null)
        {
            Debug.Log("mt == null");
            OnSkip();
            return;
        }

        GetComponent<RawImage>().texture = mt;
        AudioSource aud = GetComponent<AudioSource>();
        aud.mute = !GameSettings.MusicEnabled;
        aud.clip = mt.audioClip;
        transform.parent.gameObject.SetActive(true);
        Music.Instance.StopMusic();
        mt.Stop();
        mt.Play();
        aud.Play();
        StartCoroutine(FindEnd());
#endif
    }

    public void PlayMobileMode()
    {
        string videopath = GetFullVideoPath();
        if (string.IsNullOrEmpty(videopath))
        {
            Debug.Log("Movie Path is undefined");
            return;
        }

        Debug.LogFormat("Movie Path: {0}", videopath);
        StartCoroutine(PlayVideoCoroutine(videopath));       
    }

    protected IEnumerator PlayVideoCoroutine(string videoPath)
    {
#if UNITY_ANDROID || UNITY_IPHONE
        Handheld.PlayFullScreenMovie(videoPath, backgroundColor, controlMod, scalingMod);
#endif
        yield return new WaitForEndOfFrame();

        // end of playing
        Debug.LogFormat("End of playing: {0}", videoPath);
        EndVideo();        
    }

public void OnSkip()
    {
        Debug.Log("OnSkip");
        StopPlay();
    }

    public void StopPlay()
    {
#if UNITY_EDITOR || UNITY_STANDALONE      
        if (mt != null)
            mt.Stop();
#endif
        transform.parent.gameObject.SetActive(false);
        if (OnFinishedCallback != null)
            OnFinishedCallback();

        OnFinishedCallback = null;
    }

    private IEnumerator FindEnd()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        while (mt.isPlaying)
        {
            yield return 0;
        }
#endif
        EndVideo();
        yield break;
    }

    void EndVideo()
    {
        GetComponent<RawImage>().texture = null;
        transform.parent.gameObject.SetActive(false);
        if (OnFinishedCallback != null)
            OnFinishedCallback();
    }

    string GetFullVideoPath()
    {
        //#if UNITY_IPHONE
        //     playVideoFile = "file://" + videoFile;
        //# endif

#if USE_ASSETBUNDLE
        //play from persistent path
        return  Application.persistentDataPath + "/" + movieFileName;
#else
        //play from streaming assets
        return movieFileName;
#endif
    }
}

