using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UI_ToggleSound : MonoBehaviour {
    bool isMute;

    public void Mute()
    {
        isMute = !isMute;
        AudioListener.volume = isMute ? 0 : 1;
    }
}
