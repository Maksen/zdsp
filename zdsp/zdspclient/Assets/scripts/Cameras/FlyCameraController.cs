using UnityEngine;
using System.Collections;
using System;

public class FlyCameraController : MonoBehaviour
{
    public AnimationCurve lerpCurve;

    private CameraParams origParams;
    private CameraParams targetParams;
    private float timer;
    private float flyTime;
    private bool isFlyingTo = false;
    private bool isFlyingBack = false;
    private Action OnComplete;
    private ZDSPCamera cam;

    void Awake()
    {
        cam = GetComponent<ZDSPCamera>();
    }

    public void FlyTo(CameraParams target, float time, Action callback = null)
    {
        origParams = cam.GetCurrentCameraParams();
        targetParams = target;
        OnComplete = callback;
        timer = 0;
        flyTime = time;
        isFlyingTo = true;
        isFlyingBack = false;
        StartCoroutine(FlyCameraTo());
    }

    public void Reset(float time, Action callback = null)
    {
        OnComplete = callback;
        timer = time;
        flyTime = time;
        isFlyingBack = true;
        isFlyingTo = false;
        StartCoroutine(FlyCameraBack());
    }

    private IEnumerator FlyCameraTo()
    {
        while (!isFlyingBack)
        {
            cam.Distance = Mathf.Lerp(origParams.distance, targetParams.distance, lerpCurve.Evaluate(timer / flyTime));
            cam.Tilt = Mathf.Lerp(origParams.tilt, targetParams.tilt, lerpCurve.Evaluate(timer / flyTime));
            cam.Heading = Mathf.Lerp(origParams.heading, targetParams.heading, lerpCurve.Evaluate(timer / flyTime));
            cam.Height = Mathf.Lerp(origParams.height, targetParams.height, lerpCurve.Evaluate(timer / flyTime));
            cam.FocalLength = Mathf.Lerp(origParams.focalLength, targetParams.focalLength, lerpCurve.Evaluate(timer / flyTime));

            if (lerpCurve.Evaluate(timer / flyTime) >= 1)
                break;

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (isFlyingTo)
        {
            OnStop();
            isFlyingTo = false;
        }
    }

    private IEnumerator FlyCameraBack()
    {
        while (!isFlyingTo)
        {
            cam.Distance = Mathf.Lerp(origParams.distance, targetParams.distance, lerpCurve.Evaluate(timer / flyTime));
            cam.Tilt = Mathf.Lerp(origParams.tilt, targetParams.tilt, lerpCurve.Evaluate(timer / flyTime));
            cam.Heading = Mathf.Lerp(origParams.heading, targetParams.heading, lerpCurve.Evaluate(timer / flyTime));
            cam.Height = Mathf.Lerp(origParams.height, targetParams.height, lerpCurve.Evaluate(timer / flyTime));
            cam.FocalLength = Mathf.Lerp(origParams.focalLength, targetParams.focalLength, lerpCurve.Evaluate(timer / flyTime));

            if (lerpCurve.Evaluate(timer / flyTime) <= 0)
                break;

            timer -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (isFlyingBack)
        {
            OnStop();
            isFlyingBack = false;
        }
    }

    private void OnStop()
    {
        if (OnComplete != null)
        {
            OnComplete();
            OnComplete = null;
        }
    }
}
