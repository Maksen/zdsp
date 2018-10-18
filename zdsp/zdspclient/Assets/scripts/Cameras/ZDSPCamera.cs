using EZCameraShake;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public class CameraParams
{
    public float distance;
    public float tilt;
    public float heading;
    public float height;
    public float focalLength;

    public CameraParams() { }

    public CameraParams(float dist, float angle, float head, float ht, float focalLen)
    {
        distance = dist;
        tilt = angle;
        heading = head;
        height = ht;
        focalLength = focalLen;
    }
}

public class ZDSPCamera : MonoBehaviour
{
    public Camera mainCamera { get; private set; }
    public enum CameraMode { Overhead, Orbit };
    public CameraMode cameraMode { get; private set; }

    public GameObject targetObject;

    [Header(("Initial Parameters"))]
    public CameraParams overheadParameters = new CameraParams(11.8f, 35f, 0f, 1.6f, 30f);
    public CameraParams orbitParameters = new CameraParams(11f, 1.5f, 0f, 1.68f, 50f);

    [Header(("Control Speeds"))]
    public float zoomSpeed = 1f;
    public float rotationSpeed = 3.5f;
    public float touchZoomSensitivity = 0.4f;
    public float touchRotationSensitivity = 0.55f;
    public float flyTime = 1f;

    [Header(("Limits"))]
    public float minDistance = 7f;
    public float maxDistance = 15f;
    public float minTiltAngle = -7.5f;
    public float maxTiltAngle = 80.0f;
    public float minHeight = 1f;
    public float maxHeight = 2f;
    public float minFocalLength = 30f;
    public float maxFocalLength = 135f;

    private float currentFocalLength;
    private float currentDistance;
    private float currentHeight;

    private bool isMovingCamera = false;
    private bool isActive = false;
    protected Vector3 cameraTarget;
    private ZDSPJoystick joystick;

    // Camera fly
    private bool isFlying = false;
    private FlyCameraController cameraController;

    // Camera shake
    private float shakeIntensity;
    private float shakeDecay = 0.02f;
    private Quaternion OriginalRot;
    private bool Shaking = false;

    private void Awake()
    {
        mainCamera = GetComponentInChildren<Camera>();
        cameraController = GetComponent<FlyCameraController>();

        cameraMode = CameraMode.Overhead;
        Distance = overheadParameters.distance;
        Tilt = overheadParameters.tilt;
        Heading = overheadParameters.heading;
        Height = overheadParameters.height;
        FocalLength = overheadParameters.focalLength;

        PointerScreen.OnPointerUpEvent += OnPointerUp;

        isActive = false;
    }

    private void OnDestroy()
    {
        PointerScreen.OnPointerUpEvent -= OnPointerUp;
    }

    public void SetCameraActive(bool val)
    {
        isActive = val;
    }

    public void Init(GameObject target)
    {
        GameObject widget = UIManager.GetWidget(HUDWidgetType.Joystick);
        if (widget != null)
            joystick = widget.GetComponent<ZDSPJoystick>();

        targetObject = target;
        cameraTarget = targetObject.transform.position;
        isActive = true;
    }

    private void OnPointerUp(PointerEventData eventData)
    {
        if (PointerScreen.Instance.PointerCount <= 1)  // this is the last touch on screen
        {
            isMovingCamera = false;
        }
    }

    private void LateUpdate()
    {
        if (!isActive || targetObject == null || joystick == null)
            return;

        if (isFlying)
        {
            // controlled by FlyCameraController
        }
        else // user controlled
        {
            if (!joystick.IsInControl())  // prevent controlling camera with joystick is in control
            {
                if (cameraMode == CameraMode.Orbit)
                    UpdateRotation();

                if (cameraMode == CameraMode.Overhead)
                    UpdateZoom();
            }
        }

        UpdatePosition();
        UpdateFOV();

        //UpdateCameraShake();

#if UNITY_EDITOR || UNITY_STANDALONE  // to reset isMovingCamera after zooming since OnPointerUp won't be called
        if (isMovingCamera && PointerScreen.Instance.PointerCount == 0)
            isMovingCamera = false;
#endif
    }

    private void UpdateFOV()
    {
#if UNITY_EDITOR // for use in editor only when don't have UI slider to change focal length
        if (cameraMode == CameraMode.Orbit && !isFlying)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            float deltaZoom = scroll * 2000f;

            if (deltaZoom != 0)
            {
                float newFocalLength = currentFocalLength + deltaZoom * Time.unscaledDeltaTime * zoomSpeed;
                FocalLength = Mathf.Clamp(newFocalLength, minFocalLength, maxFocalLength);
            }
        }
#endif
    }

    private void UpdateRotation()
    {
        float deltaAngleH = 0.0f;
        float deltaAngleV = 0.0f;

        if (PointerScreen.Instance.PointerCount >= 1)  // at least one pointer on screen
        {
            var ptr = PointerScreen.Instance.GetPointer(0);
            if (ptr.dragging && (!joystick.IsPointerInBlockerArea(ptr) || isMovingCamera))
            {
                deltaAngleH += ptr.delta.x;
                deltaAngleV -= ptr.delta.y;
            }
        }

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        deltaAngleH *= touchRotationSensitivity;
        deltaAngleV *= touchRotationSensitivity;
#endif

        if (deltaAngleH != 0 || deltaAngleV != 0)
        {
            SuspendShake();  // cancel any camera shake
            isMovingCamera = true;

            float yAngle = transform.localEulerAngles.y + deltaAngleH * Time.unscaledDeltaTime * rotationSpeed;
            float xAngle = transform.localEulerAngles.x + deltaAngleV * Time.unscaledDeltaTime * rotationSpeed;
            xAngle = Mathf.Clamp(ZDSPCameraUtilities.SignedAngle(xAngle), minTiltAngle, maxTiltAngle);
            transform.localEulerAngles = new Vector3(xAngle, yAngle, transform.localEulerAngles.z);

            // move camera distance closer if tilt is less than 5.5deg
            float thresholdAngle = -5.5f;
            if (!(currentDistance == orbitParameters.distance && xAngle > thresholdAngle))
            {
                float offset = (xAngle - thresholdAngle) * 1.5f;
                currentDistance = orbitParameters.distance + offset;
                currentDistance = Mathf.Clamp(currentDistance, 8f, orbitParameters.distance);
            }
        }
    }

    private void UpdateZoom()
    {
        float deltaZoom = 0f;

#if UNITY_EDITOR || UNITY_STANDALONE

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        deltaZoom -= scroll * 300f;

#elif UNITY_IOS || UNITY_ANDROID

        if (PointerScreen.Instance.PointerCount >= 2) // at least 2 pointers on screen
        {
            var ptr1 = PointerScreen.Instance.GetPointer(0);
            var ptr2 = PointerScreen.Instance.GetPointer(1);
            if ((!joystick.IsPointerInBlockerArea(ptr1) && !joystick.IsPointerInBlockerArea(ptr2)) || isMovingCamera)
            {
                // Find the position in the previous frame of each touch.
                Vector2 primaryPrevPos = ptr1.position - ptr1.delta;
                Vector2 secondaryPrevPos = ptr2.position - ptr2.delta;

                // Find the magnitude of the vector (the distance) between the touches in each frame.
                float prevTouchDeltaMag = (primaryPrevPos - secondaryPrevPos).magnitude;
                float touchDeltaMag = (ptr1.position - ptr2.position).magnitude;

                // Find the difference in the distances between each frame.
                deltaZoom = (prevTouchDeltaMag - touchDeltaMag) * touchZoomSensitivity;
            }
        }

#endif

        if (deltaZoom != 0)
        {
            SuspendShake();
            isMovingCamera = true;

            currentDistance += deltaZoom * Time.unscaledDeltaTime * zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

            // lower height when distance is closer, higher when further
            float t = Mathf.InverseLerp(minDistance, maxDistance, currentDistance);
            currentHeight = Mathf.Lerp(minHeight, maxHeight, t);
        }
    }

    private void UpdatePosition()
    {
        cameraTarget = targetObject.transform.position;
        transform.position = cameraTarget + Vector3.up * currentHeight;
        transform.Translate(Vector3.back * currentDistance);
    }

    public void ToggleCameraMode()
    {
        SuspendShake();  // if camera is shaking, cancel the shake
        cameraMode ^= CameraMode.Orbit;  // toggle camera mode
        joystick.SetTouchZoneFullScreen(cameraMode == CameraMode.Overhead);
        joystick.SetActive(false);  // disable joystick while flying
        if (cameraMode == CameraMode.Orbit)
        {
            FlyCameraToPlayer(flyTime, true);
            if (GameInfo.gCombat != null)
                GameInfo.gCombat.EnableZoomInMode(true);
            else
            {
                SingleRPGControl rpgCtrl = GetComponent<SingleRPGControl>();
                if (rpgCtrl != null)
                    rpgCtrl.EnableZoomInMode(true);
            }
        }
        else
        {
            FlyCameraToInitial(flyTime, () =>
            {
                if (GameInfo.gCombat != null)
                    GameInfo.gCombat.EnableZoomInMode(false);
                else
                {
                    SingleRPGControl rpgCtrl = GetComponent<SingleRPGControl>();
                    if (rpgCtrl != null)
                        rpgCtrl.EnableZoomInMode(false);
                }
            });
        }
    }

    #region CameraFly

    public void FlyCameraToPlayer(float duration, bool facePlayer = true, Action callback = null)
    {
        Vector3 facing = facePlayer ? -targetObject.transform.forward : transform.forward;
        Quaternion cameraTargetRotation = Quaternion.LookRotation(facing);
        float heading = ZDSPCameraUtilities.SignedAngle(cameraTargetRotation.eulerAngles.y);
        CameraParams target = new CameraParams(orbitParameters.distance, orbitParameters.tilt, heading, orbitParameters.height, orbitParameters.focalLength);
        FlyTo(target, duration, callback);
    }

    // Used to fly camera back to starting parameters in overhead mode
    public void FlyCameraToInitial(float duration, Action callback = null)
    {
        FlyTo(overheadParameters, duration, callback);
    }

    public void FlyTo(CameraParams parameters, float duration, Action callback = null)
    {
        isFlying = true;
        cameraController.FlyTo(parameters, duration, OnFlyEnd + callback);
    }

    // Used to return camera back to previous position before a fly to
    public void ReturnCamera(float duration, Action callback = null)
    {
        cameraController.Reset(duration, OnFlyEnd + callback);
    }

    private void OnFlyEnd()
    {
        isFlying = false;
        joystick.SetActive(true);  // enable back joystick
    }

    public bool IsInControl()
    {
        return !isFlying;
    }

    #endregion CameraFly

    #region CameraParams

    public CameraParams GetCurrentCameraParams()
    {
        return new CameraParams(Distance, Tilt, Heading, Height, FocalLength);
    }

    public float Distance
    {
        get { return currentDistance; }
        set { currentDistance = value; }
    }

    public float Tilt
    {
        get { return ZDSPCameraUtilities.SignedAngle(transform.localEulerAngles.x); }
        set { transform.localEulerAngles = new Vector3(value, transform.localEulerAngles.y, transform.localEulerAngles.z); }
    }

    public float Heading
    {
        get { return ZDSPCameraUtilities.SignedAngle(transform.localEulerAngles.y); }
        set { transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, value, transform.localEulerAngles.z); }
    }

    public float Height
    {
        get { return currentHeight; }
        set { currentHeight = value; }
    }

    public float FocalLength
    {
        get { return currentFocalLength; }
        set
        {
            currentFocalLength = value;
            mainCamera.fieldOfView = ZDSPCameraUtilities.FocalLengthToFOV(currentFocalLength);
        }
    }
    #endregion CameraParams

    #region Camera shake
    private void UpdateCameraShake()
    {
        if (shakeIntensity > 0)
        {
            transform.position += UnityEngine.Random.insideUnitSphere * shakeIntensity;
            transform.rotation = new Quaternion(OriginalRot.x + UnityEngine.Random.Range(-shakeIntensity, shakeIntensity) * .2f,
                                      OriginalRot.y + UnityEngine.Random.Range(-shakeIntensity, shakeIntensity) * .2f,
                                      OriginalRot.z + UnityEngine.Random.Range(-shakeIntensity, shakeIntensity) * .2f,
                                      OriginalRot.w + UnityEngine.Random.Range(-shakeIntensity, shakeIntensity) * .2f);

            shakeIntensity -= shakeDecay * Time.unscaledDeltaTime;
        }
        else if (Shaking)
        {
            Shaking = false;
            transform.rotation = OriginalRot;
        }
    }

    public void DoShake(float intensity, float decay)
    {
        if (isMovingCamera || isFlying)  // don't shake if camera is moving
            return;
        if (Shaking) // let the current shaking finish
            return;
        OriginalRot = transform.rotation;
        shakeIntensity = intensity;
        shakeDecay = decay;
        Shaking = true;
    }

    private void SuspendShake()
    {
        if (Shaking)
        {
            Shaking = false;
            shakeIntensity = 0;
            transform.rotation = OriginalRot;
        }
    }

    #endregion Camera shake

    private void OnGUI()
    {
        if (GameInfo.gCombat != null && UIManager.LoadingScreen != null && !UIManager.LoadingScreen.gameObject.activeInHierarchy)
        {
            if (!UIManager.UIHud.IsVisible())
                return;

            GUIStyle style = new GUIStyle();
            int fontsize = Mathf.FloorToInt(15 * ((float)Screen.height / 540f));
            style.fontSize = fontsize;
            style.normal.textColor = Color.black;

            int buttonWidth = Screen.width / 10;
            int buttonHeight = Screen.height / 12;
            GUIStyle buttonStyle = new GUIStyle("button");
            buttonStyle.fontSize = fontsize;

            if (GUI.Button(new Rect(Screen.width/2 + 10, 20, buttonWidth, buttonHeight), cameraMode.ToString(), buttonStyle))
                ToggleCameraMode();
        }
    }
}

public static class ZDSPCameraUtilities
{
    public static float FocalLengthToFOV(float focalLength)
    {
        // Formula from https://en.wikipedia.org/wiki/Angle_of_view
        // Using 35mm film sensor (36mm w x 24mm h)
        int height = 24;  // Unity uses vertical FOV so use film height
        double fov = Mathf.Rad2Deg * 2.0 * Math.Atan(height / (2.0 * focalLength));
        return (float)fov;
    }

    public static float SignedAngle(float angle)  // make angle between -180 to 180
    {
        return (angle <= 180f) ? angle : -(360f - angle);
    }
}