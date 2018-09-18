using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Zealot.Audio;

public class UI_VideoClueMessage : MonoBehaviour
{
    [SerializeField]
    VideoPlayer MoviePlayer;

    [SerializeField]
    Image Photo;

    [SerializeField]
    RawImage MovieScreen;

    [SerializeField]
    GameObject PlayButton;

    [SerializeField]
    GameObject SkipButton;

    private VideoClip mVideoClip;
    private AudioSource mAudioSource;
    private Texture2D mVideoFrame;

    public void Init(string videopath, string photopath, AudioSource audioSource)
    {
        mAudioSource = audioSource;
        ClientUtils.LoadVideoAsync(videopath, OnVideoClipLoaded);
    }

    public void Init(string videopath, AudioSource audioSource)
    {
        mAudioSource = audioSource;
        mAudioSource.enabled = true;
        ClientUtils.LoadVideoAsync(videopath, OnVideoClipLoaded);
    }

    public void OnVideoClipLoaded(VideoClip videoClip)
    {
        mVideoClip = videoClip;
        if (mVideoClip != null)
        {
            MoviePlayer.clip = mVideoClip;
            MoviePlayer.waitForFirstFrame = true;
            MoviePlayer.prepareCompleted += OnClipReady;
            MoviePlayer.loopPointReached += OnVideoFinished;
            MoviePlayer.Prepare();
            MoviePlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            MoviePlayer.EnableAudioTrack(0, true);
            MoviePlayer.SetTargetAudioSource(0, mAudioSource);
            MoviePlayer.controlledAudioTrackCount = 1;
            mAudioSource.volume = 1.0f;
        }
    }

    private void OnClipReady(VideoPlayer source)
    {
        RenderTexture renderTexture = source.texture as RenderTexture;
        mVideoFrame = new Texture2D(renderTexture.width, renderTexture.height);
        RenderTexture.active = renderTexture;
        mVideoFrame.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        mVideoFrame.Apply();
        RenderTexture.active = null;

        Photo.sprite = Sprite.Create(mVideoFrame, new Rect(0, 0, 654, 332), new Vector2(0.5f, 0.5f));
        
        MoviePlayer.prepareCompleted -= OnClipReady;
        MoviePlayer.Stop();
    }

    public void OnClickPlay()
    {
        PlayVideo();
    }

    private void PlayVideo()
    {
        if (mVideoClip != null)
        {
            SkipButton.SetActive(true);
            PlayButton.SetActive(false);
            MovieScreen.enabled = true;
            SoundFX.Instance.MuteSound(true);
            Photo.enabled = false;
            MoviePlayer.Play();
            mAudioSource.Play();
        }
    }

    private void OnVideoFinished(VideoPlayer videoplayer)
    {
        StopVideo();
    }

    public void OnClickSkip()
    {
        StopVideo();
    }

    private void StopVideo()
    {
        SkipButton.SetActive(false);
        PlayButton.SetActive(true);
        MovieScreen.enabled = false;
        SoundFX.Instance.MuteSound(false);
        Photo.enabled = true;
        MoviePlayer.Stop();
    }

    public void Clean()
    {
        StopVideo();
        MoviePlayer.clip = null;
    }
}
