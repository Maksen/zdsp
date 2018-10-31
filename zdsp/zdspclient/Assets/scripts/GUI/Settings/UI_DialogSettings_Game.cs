using UnityEngine;

public class UI_DialogSettings_Game : MonoBehaviour
{
    [SerializeField]
    DefaultToggleInGroup defaultToggleingrpSound = null;
    [SerializeField]
    DefaultToggleInGroup defaultToggleingrpMusic = null;

    public void Init()
    {
        defaultToggleingrpSound.GoToPage(GameSettings.SoundFXEnabled ? 0 : 1);
        defaultToggleingrpMusic.GoToPage(GameSettings.MusicEnabled ? 0 : 1);
    }

    public void OnValueChangedToggleSound(bool isOn)
    {
        if (GameSettings.SoundFXEnabled == isOn)
            return;

        GameSettings.SoundFXEnabled = isOn;
        GameSettings.SerializeClient();
    }

    public void OnValueChangedToggleMusic(bool isOn)
    {
        if (GameSettings.MusicEnabled == isOn)
            return;

        GameSettings.MusicEnabled = isOn;
        GameSettings.SerializeClient();
    }
}
