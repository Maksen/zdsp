using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimelineManager : MonoBehaviour
{
    public static Camera uicamera = null;

    void init()
    {
        uicamera = gameObject.GetComponent<Camera>();
    }

	// Use this for initialization
	void Start ()
    {
        init(); 
    }

    private void OnEnable()
    {
        init();
    }

    public void PlayCutscene(string name)
    {
        if (CutsceneCameraUIToggle.cutscenes != null)
        {
            if(CutsceneCameraUIToggle.cutscenes.ContainsKey(name))
                CutsceneCameraUIToggle.cutscenes[name].SetTimelineActive();
        }
    }
	
	public void SkipCutscene(string name)
    {
        if (CutsceneCameraUIToggle.cutscenes != null)
        {
            if(CutsceneCameraUIToggle.cutscenes.ContainsKey(name))
                CutsceneCameraUIToggle.cutscenes[name].SetCamUIActive();
        }
    }
}
