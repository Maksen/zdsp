using UnityEngine;

public class TimelineWeaponAssist : MonoBehaviour
{
    public TimelineAssist timelineAssist;

    void OnEnable()
    {
        if (timelineAssist != null)
            timelineAssist.HideWeapon(false);
    }

    void OnDisable()
    {
        if (timelineAssist != null)
            timelineAssist.HideWeapon(true);
    }
}

