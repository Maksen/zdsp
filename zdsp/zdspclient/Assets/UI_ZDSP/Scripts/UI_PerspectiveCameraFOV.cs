using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UI_PerspectiveCameraFOV : MonoBehaviour
{

    //Change FOV to fit screen ratio. Dev assume is 16:9 Ratio, Height is 576. Default FOV 16 for 16:9
    //ScreenHeight * defaultFOV / 576

    public float defaultFOV; //16
    private float startAspect;
    private float currentAspect;
    public Camera Cam;

    void Start()
    {
        Cam.fieldOfView = 1.77778F * defaultFOV / Cam.aspect;
        startAspect = (float)System.Math.Round(Cam.aspect, 2);
        currentAspect = startAspect;
    }

    void Update()
    {
        UpdateAspectRatio();
    }

    float time_elapsed = 0.0f;
    float update_period = 1.0f;
    void UpdateAspectRatio()
    {
        time_elapsed += Time.deltaTime;

        if (time_elapsed < update_period)
            return;

        time_elapsed = 0;

        currentAspect = (float)System.Math.Round(Cam.aspect, 2);
        //Debug.LogError("Current: " + currentAspect);

        if (startAspect != currentAspect)
        {
            Cam.fieldOfView = 1.77778F * defaultFOV / Cam.aspect;
            Debug.LogError("Aspect Changed");
            startAspect = currentAspect;
        }
    }
}
