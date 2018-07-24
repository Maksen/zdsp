using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class ActivateOnCutsceneEnd : MonoBehaviour {

    public Camera UiCamera;
    public PlayableDirector Timeline;

    private void Update()
    {
        if (UiCamera != null && Timeline != null)
        {
            if (Timeline.state != PlayState.Playing)
            {
                UiCamera.enabled = true;
            }
            else
                UiCamera.enabled = false;
        }
    }
}
