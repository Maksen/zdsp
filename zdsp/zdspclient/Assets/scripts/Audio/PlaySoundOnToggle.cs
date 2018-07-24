using UnityEngine;
using UnityEngine.UI;

namespace Zealot.Audio
{
    [RequireComponent(typeof(Toggle))]
    public class PlaySoundOnToggle : PlaySoundBase
    {
        private Toggle toggle;
        private bool isLastToggled = false;

        private void Awake()
        {
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(PlaySound);
        }

        private void PlaySound(bool toggled)
        {
            if (toggled && !isLastToggled)
            {
                isLastToggled = true;
                PlayAudioClip();
            }
            else if (!toggled && isLastToggled)
            {
                isLastToggled = false;
            }
        }
    }
}