using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Delay in seconds before starting the zoom.")]
    float startDelay = 0f;

    [SerializeField]
    [Tooltip("Amount of the zoom.")]
    [Range(1, 4f)]
    float zoom = 1.2f;

    [SerializeField]
    [Tooltip("Time in seconds for camera to reach target zoom.")]
    float fadeInTime = 0.5f;

    [SerializeField]
    [Tooltip("Time in seconds to hold camera at target zoom.")]
    float holdTime = 0f;

    [SerializeField]
    [Tooltip("Time in seconds for camera zoom to return to original.")]
    float fadeOutTime = 0.5f;

    private ZDSPCamera mainCam;
    private float elapsedTime;
    private float currentFadeTime;
    private bool sustain;
    private float targetFocalLength;
    private bool isFadingIn;
    private float currentHoldTime;
    private AnimationCurve smoothCurve;
    private GameObject parentObject;

    private void Init()
    {
        enabled = false;
        mainCam = FindObjectOfType(typeof(ZDSPCamera)) as ZDSPCamera;
        smoothCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }

    public void StartZoom()
    {
        if (!CanZoom())
            return;

        if (mainCam == null)
            Init();

        if (mainCam != null && mainCam.cameraMode == ZDSPCamera.CameraMode.Overhead && mainCam.IsInControl())
        {
            targetFocalLength = zoom * mainCam.overheadParameters.focalLength;
            sustain = false;
            currentHoldTime = 0;
            elapsedTime = 0;

            if (fadeInTime > 0)
            {
                isFadingIn = true;
                currentFadeTime = 0;
            }
            else
            {
                mainCam.FocalLength = targetFocalLength;
                isFadingIn = false;
                currentFadeTime = 1;
                if (holdTime > 0)
                    sustain = true;
            }
            enabled = true;
        }
    }

    private void Update()
    {
        if (CanUpdate())
        {
            if (isFadingIn)  // fading in
            {
                if (currentFadeTime < 1)
                {
                    currentFadeTime += Time.deltaTime / fadeInTime;
                    currentFadeTime = Mathf.Min(currentFadeTime, 1f);  // clamp to max 1
                }
                else  // fade in completed
                {
                    isFadingIn = false;
                    if (holdTime > 0)
                        sustain = true;
                }
            }
            else if (sustain)  // holding zoom
            {
                if (currentHoldTime < holdTime)
                    currentHoldTime += Time.deltaTime;
                else
                    sustain = false;
            }
            else if (fadeOutTime > 0) // not fading in or holding, so fade out
            {
                if (currentFadeTime > 0)
                {
                    currentFadeTime -= Time.deltaTime / fadeOutTime;
                    currentFadeTime = Mathf.Max(0, currentFadeTime);  // clamp at min 0
                }
                else  // fade out completed
                {
                    enabled = false;
                }
            }
            else  // no fading out immediate go back to original
            {
                currentFadeTime = 0;
                enabled = false;
            }
            mainCam.FocalLength = Mathf.Lerp(mainCam.overheadParameters.focalLength, targetFocalLength, smoothCurve.Evaluate(currentFadeTime));
        }
        else
            elapsedTime += Time.deltaTime;
    }

    private bool CanUpdate()
    {
        return mainCam != null && elapsedTime >= startDelay;
    }

    private bool CanZoom()
    {
        if (fadeInTime == 0 && holdTime == 0 && fadeOutTime == 0)
            return false;

        if (parentObject == null)
        {
            CharacterController cctrl = GetComponentInParent<CharacterController>();
            if (cctrl != null)
                parentObject = cctrl.gameObject;
        }

        if (parentObject != null)
        {
#if UNITY_EDITOR
            if (GameInfo.gCombat == null)  // for non-ingame in editor
                return true;
#endif
            return parentObject.CompareTag("LocalPlayer");
        }
        return true;
    }
}