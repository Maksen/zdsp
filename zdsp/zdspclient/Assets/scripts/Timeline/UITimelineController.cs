using UnityEngine;

public class UITimelineController : MonoBehaviour
{
    public delegate void TimelineDelegate();
    public event TimelineDelegate OnBegin = delegate { };
    public event TimelineDelegate OnEnd = delegate { };

    private GameObject timelineObj;
    private string path;

    public void Play(string timelinePath)
    {
        if (timelineObj != null && path != timelinePath)
        {
            CleanUp();
        }

        if (timelineObj == null && !string.IsNullOrEmpty(timelinePath))
        {
            OnBegin();
            AssetLoader.Instance.LoadAsync<GameObject>(timelinePath, OnTimelineLoaded, true);
            path = timelinePath;
        }
    }

    private void OnTimelineLoaded(GameObject obj)
    {
        if (obj != null)
        {
            timelineObj = Instantiate(obj);
            UITimeline uiTimeline = timelineObj.GetComponent<UITimeline>();
            if (uiTimeline != null)
                uiTimeline.SetCloseCallback(OnEndCutscene);
            timelineObj.transform.SetParent(transform, false);
        }
        else
        {
            OnEndCutscene();
        }
    }

    public void OnEndCutscene()
    {
        CleanUp();
        OnEnd();
    }

    public void CleanUp()
    {
        if (timelineObj != null)
            Destroy(timelineObj);
        timelineObj = null;
        path = "";
    }
}