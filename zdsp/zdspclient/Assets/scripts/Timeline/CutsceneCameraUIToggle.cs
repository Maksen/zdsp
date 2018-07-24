using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneCameraUIToggle : MonoBehaviour
{
    public static Dictionary<string, CutsceneCameraUIToggle> cutscenes = new Dictionary<string, CutsceneCameraUIToggle>();

    private void Start()
    {
        if(cutscenes.ContainsKey(gameObject.name) == false)
            cutscenes.Add(gameObject.name, this);
    }

    private void OnDestroy()
    {
        if (cutscenes.ContainsKey(gameObject.name) == true)
            cutscenes.Remove(gameObject.name);
    }

    public void Toggle()
    {
        if (TimelineManager.uicamera == null)
            return;

        Set(!TimelineManager.uicamera.enabled);
    }

    public void SetCamUIActive()
    {
        Set(true);
    }

    public void SetTimelineActive()
    {
        Set(false);
    }

    void Set(bool camactive)
    {
        if (TimelineManager.uicamera == null)
            return;

        var timeline = gameObject.GetComponent<PlayableDirector>();

        if (timeline == null)
            return;

        if (camactive)
        {
            TimelineManager.uicamera.enabled = true;
            timeline.gameObject.SetActive(false);
        }
        else
        {
            TimelineManager.uicamera.enabled = false;
            timeline.gameObject.SetActive(true);
        }
    }
}
