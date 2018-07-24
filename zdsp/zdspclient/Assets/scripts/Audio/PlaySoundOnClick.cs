using UnityEngine;
using UnityEngine.UI;

namespace Zealot.Audio
{
    [RequireComponent(typeof(Button))]
    public class PlaySoundOnClick : PlaySoundBase
    {
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(PlayAudioClip);
        }
    }
}