using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

[System.Obsolete("Obsolete for ZDSP, replaced by ZDSPCamera")]
public class CameraManager : MonoBehaviour
{
    public static CameraManager SP;

    public Transform targetPlayer;
    public bool followPlayer = true;

    [Header(("Camera Settings"))]
    public float minZoomDistance = 8.0f;
    public float maxZoomDistance = 20.0f;
    public float minTiltAngle = 5.0f;
    public float maxTiltAngle = 36.0f;
    public float currentDistance = 20.0f;
    public float shiftRatio = 2.2f;
    public float heightRatio = 1.35f;
    private float panRatio = 0;
    private float origShiftRatio;
    private float origHeightRatio;
    private float origDistance;
    private float origTiltAngle;

    [Header(("Control Speeds"))]
    public float panSpeed = 5.0f;
    public float rotationSpeed = 75.0f;
    public float zoomSpeed = 40.0f;
    public float tiltSpeed = 90.0f;
    public float mouseScrollMultiplier = 15f;
    public float mouseZoomMultiplier = 0.2f;
    public float mouseTiltMultiplier = 0.23f;

    [Space(10)]
    public bool useKeyboardInput = true;
    public bool useMouseInput = true;
    public bool separateTiltAndZoom = false;

    private float mousePanMultiplier = 0.1f;
    private bool increaseSpeedWhenZoomedOut = true;
    private bool smoothing = true;
    private float smoothingFactor = 0.4f;

    [Space(10)]
    public Vector3 cameraTarget;
    public Vector3 shiftedTarget;

    private Vector3 lastMousePos;
    private Vector3 lastPanSpeed = Vector3.zero;
    private bool isResettingHeading = false;
    private bool isMouseOverUI = false;

    private Camera mainCam;

    void Awake()
    {
        mainCam = GetComponent<Camera>();
        SP = this;
        InitEventSystem();
    }

    void InitEventSystem()
    {
        if (EventSystem.current == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }

    void Start()
    {
        if (targetPlayer != null)
            cameraTarget = targetPlayer.position;

        lastMousePos = Vector3.zero;

        origShiftRatio = shiftRatio;
        origHeightRatio = heightRatio;
        origDistance = currentDistance;
        origTiltAngle = transform.localEulerAngles.x;
    }

    public void setTarget(Transform tr)
    {
        targetPlayer = tr;
        cameraTarget = targetPlayer.position;
    }

    public void SetFollowPlayer(bool val)
    {
        followPlayer = val;
    }

    void LateUpdate()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && EventSystem.current.IsPointerOverGameObject())
            isMouseOverUI = true;

        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            isMouseOverUI = false;

        UpdatePanning();
        UpdateRotation();
        UpdateZooming();
        UpdatePosition();

        lastMousePos = Input.mousePosition;

        if (Input.GetKey(KeyCode.Z))
            ResetCamera();
    }

    void UpdatePanning()
    {
        Vector3 moveVector = new Vector3(0, 0, 0);

        if (useKeyboardInput)
        {
            if (Input.GetKey(KeyCode.A))
            {
                moveVector += new Vector3(-1, 0, 0);
            }
            if (Input.GetKey(KeyCode.S))
            {
                moveVector += new Vector3(0, 0, -1);
            }
            if (Input.GetKey(KeyCode.D))
            {
                moveVector += new Vector3(1, 0, 0);
            }
            if (Input.GetKey(KeyCode.W))
            {
                moveVector += new Vector3(0, 0, 1);
            }
        }

        if (useMouseInput)
        {
            if (Input.GetMouseButton(2))
            {
                Vector3 deltaMousePos = Input.mousePosition - lastMousePos;
                moveVector += new Vector3(-deltaMousePos.x, 0, -deltaMousePos.y) * mousePanMultiplier;
            }
        }

        if (moveVector != Vector3.zero)
        {
            followPlayer = false;
        }

        Vector3 effectivePanSpeed = moveVector;
        if (smoothing)
        {
            effectivePanSpeed = Vector3.Lerp(lastPanSpeed, moveVector, smoothingFactor);
            lastPanSpeed = effectivePanSpeed;
        }

        float oldRotation = transform.localEulerAngles.x;
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, transform.localEulerAngles.z);

        float panMultiplier = increaseSpeedWhenZoomedOut ? (Mathf.Sqrt(currentDistance)) : 1.0f;
        cameraTarget = cameraTarget + transform.TransformDirection(effectivePanSpeed) * panSpeed * panMultiplier * Time.unscaledDeltaTime;
        transform.localEulerAngles = new Vector3(oldRotation, transform.localEulerAngles.y, transform.localEulerAngles.z);
    }

    void UpdateRotation()
    {
        float deltaAngleH = 0.0f;
        float deltaAngleV = 0.0f;
        float deltaYAngle = 0.0f;

        if (useKeyboardInput)
        {
            if (Input.GetKey(KeyCode.Q))
            {
                deltaAngleH = 1.0f;
            }
            if (Input.GetKey(KeyCode.E))
            {
                deltaAngleH = -1.0f;
            }
        }

        if (useMouseInput && !isMouseOverUI)
        {
            // rotate
            if (Input.GetMouseButton(0))
            {
                deltaYAngle = AngleSigned(lastMousePos, Input.mousePosition);
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y - deltaYAngle, transform.localEulerAngles.z);
            }

            // tilt
            if (Input.GetMouseButton(1))
            {
                Vector3 deltaMousePos = Input.mousePosition - lastMousePos;
                deltaAngleV -= deltaMousePos.y * mouseTiltMultiplier;
            }
        }

        float yAngle = transform.localEulerAngles.y;
        float xAngle = transform.localEulerAngles.x;

        if (isResettingHeading)  // for resetting heading
        {
            deltaAngleH = transform.localEulerAngles.y < 180 ? -1.0f : 1.0f;
            yAngle = transform.localEulerAngles.y + deltaAngleH * Time.deltaTime * 350f;

            if (yAngle > 180 && deltaAngleH > 0)
            {
                yAngle = 180;
                isResettingHeading = false;
            }
            if (yAngle < 180 && deltaAngleH < 0)
            {
                yAngle = 180;
                isResettingHeading = false;
            }
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, yAngle, transform.localEulerAngles.z);
        }
        else // manual control
        {
            yAngle = transform.localEulerAngles.y + deltaAngleH * Time.deltaTime * rotationSpeed;
            xAngle = transform.localEulerAngles.x + deltaAngleV * Time.deltaTime * tiltSpeed;
            xAngle = Mathf.Clamp(xAngle, minTiltAngle, maxTiltAngle);
            transform.localEulerAngles = new Vector3(xAngle, yAngle, transform.localEulerAngles.z);
        }
    }

    void UpdateZooming()
    {
        float deltaZoom = 0.0f;

        if (useKeyboardInput)
        {
            if (Input.GetKey(KeyCode.F))
            {
                deltaZoom = 1.0f;
            }
            if (Input.GetKey(KeyCode.R))
            {
                deltaZoom = -1.0f;
            }
        }
        if (useMouseInput && !isMouseOverUI)
        {
            if (separateTiltAndZoom)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                deltaZoom -= scroll * mouseScrollMultiplier;
            }
            else
            {
                if (Input.GetMouseButton(1))
                {
                    Vector3 deltaMousePos = Input.mousePosition - lastMousePos;
                    deltaZoom -= deltaMousePos.y * mouseZoomMultiplier;
                }
            }
        }

        currentDistance = currentDistance + deltaZoom * Time.deltaTime * zoomSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minZoomDistance, maxZoomDistance);
    }

    void UpdatePosition()
    {
        if (followPlayer && targetPlayer != null)
        {
            cameraTarget = targetPlayer.position;
        }

        Vector3 shiftVector = new Vector3(0, 0, 1) * shiftRatio;    // shift camera target up so player is in lower half of camera view
        Vector3 panVector = new Vector3(1, 0, 0) * panRatio;    // pan camera to side (not used when following player)
        Vector3 heightVector = new Vector3(0, 1, 0) * heightRatio;
        Vector3 effectiveMoveVector = shiftVector + panVector + heightVector;

        shiftedTarget = GetShiftedTarget(cameraTarget, effectiveMoveVector);
        transform.position = shiftedTarget;
        transform.Translate(Vector3.back * currentDistance);
    }

    public void ResetCamera()
    {
        shiftRatio = origShiftRatio;
        heightRatio = origHeightRatio;
        currentDistance = origDistance;
        transform.localEulerAngles = new Vector3(origTiltAngle, transform.localEulerAngles.y, transform.localEulerAngles.z);
        isResettingHeading = true;

        if (targetPlayer != null)
        {
            followPlayer = true;
            cameraTarget = targetPlayer.position;
        }

    }


    /********************** Helper functions *****************************/
    Vector3 GetShiftedTarget(Vector3 originalTarget, Vector3 moveVector)
    {
        float oldRotation = transform.localEulerAngles.x;
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, transform.localEulerAngles.z);
        Vector3 target = originalTarget + transform.TransformDirection(moveVector);
        transform.localEulerAngles = new Vector3(oldRotation, transform.localEulerAngles.y, transform.localEulerAngles.z);
        return target;
    }

    float AngleSigned(Vector2 from, Vector2 to)  // clockwise +ve, anti-clockwise -ve
    {
        Vector3 targetScreenPos;
        if (followPlayer)
            targetScreenPos = mainCam.WorldToScreenPoint(targetPlayer.position);
        else
            targetScreenPos = mainCam.WorldToScreenPoint(cameraTarget);
        Vector2 origin = new Vector2(targetScreenPos.x, targetScreenPos.y);
        Vector2 fromVector = from - origin;
        Vector2 toVector = to - origin;
        float angle = Vector2.Angle(fromVector, toVector);
        Vector3 cross = Vector3.Cross(fromVector, toVector);
        if (cross.z > 0)
            angle = -angle;

        return angle;
    }
}
