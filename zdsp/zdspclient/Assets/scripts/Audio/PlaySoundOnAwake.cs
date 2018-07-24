namespace Zealot.Audio
{
    public class PlaySoundOnAwake : PlaySoundBase
    {
        private void OnEnable()
        {
            PlayAudioClip();
        }
    }

}