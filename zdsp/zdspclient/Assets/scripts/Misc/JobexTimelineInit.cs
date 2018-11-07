using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class JobexTimelineInit : MonoBehaviour {

	// Use this for initialization
	void Start () {
        init();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    Dictionary<GameObject, TransformReset> resetters = new Dictionary<GameObject, TransformReset>();
    private void init()
    {
        var timelines = GetComponentsInChildren<PlayableDirector>(true);

        foreach (var timeline in timelines)
        {
            var onenable = timeline.gameObject.GetComponent<OnEnableScript>();

            if (onenable == null) onenable = timeline.gameObject.AddComponent<OnEnableScript>();

            onenable.onEnabled = null;
            onenable.onEnabled += ResetTimeline;
        }

        var cameras = GetComponentsInChildren<Camera>(true);

        foreach (var camera in cameras)
        {
            var onenable = camera.gameObject.GetComponent<OnEnableScript>();

            if (onenable == null) onenable = camera.gameObject.AddComponent<OnEnableScript>();

            resetters.Add(camera.gameObject, new TransformReset(camera.gameObject.transform));

            onenable.onEnabled = null;
            onenable.onEnabled += ResetPosition;
        }
    }

    void ResetTimeline(GameObject obj)
    {
        var timeline = obj.GetComponent<PlayableDirector>();

        if (timeline != null)
        {
            timeline.time = 0;
            timeline.Evaluate();
            timeline.Play();
        }
    }

    void ResetPosition(GameObject obj)
    {
        if(resetters.ContainsKey(obj))
        {
            resetters[obj].Set(obj.transform);
        }
    }
}

public class TransformReset
{
    public TransformReset(Transform t)
    {
        position = t.localPosition;
        rotation = t.localRotation;
        scale = t.localScale;
    }
    public void Set(Transform t)
    {
        t.localPosition = position;
        t.localRotation = rotation;
        t.localScale = scale;
    }

    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}
