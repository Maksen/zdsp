using UnityEngine;
using UnityEngine.Video;
using Zealot.Audio;

public class UI_DialogVideoPlayer : BaseWindowBehaviour
{
    [SerializeField]
    VideoPlayer MoviePlayer;

    public void Init(string path)
    {
        ClientUtils.LoadVideoAsync(path, VideoClipLoaded);
    }

    private void VideoClipLoaded(VideoClip videoclip)
    {
        if (videoclip != null)
        {
            MoviePlayer.clip = videoclip;
            MoviePlayer.loopPointReached += OnVideoFinished;
            MoviePlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            MoviePlayer.SetTargetAudioSource(0, SoundFX.Instance.GetAudioSource());
            MoviePlayer.Play();
        }
        else
            StopVideo();
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
        MoviePlayer.clip = null;
        MoviePlayer.Stop();
        UIManager.CloseDialog(WindowType.DialogVideoPlayer);
    }
}
