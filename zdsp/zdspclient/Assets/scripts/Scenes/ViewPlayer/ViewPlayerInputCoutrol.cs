using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ViewPlayer
{
    enum ViewPlayerControlDragType
    {
        None,
        VPCTMove,
        VPCTRotate
    };

    enum ViewPlayerControlCameraMode
    {
        None,
        Main,
        Follow1,
        Follow2
    }
    public class ViewPlayerInputCoutrol : MonoBehaviour
    {
        public GameObject mainObject;
        public GameObject freeCamera;
        public GameObject followCamera;
        public GameObject dragArea;
        public Toggle toggleMain, toggleTarget;
        public float rotateSpeed = 20.0f;
        public float moveSpeed = 0.1f;

        private ViewPlayerControlDragType controlType;
        private bool isPressed;
        private Vector2 startPos;
        private Vector2 delta;
        private float rotateDif = 5.0f;
        private float moveDif = 5.0f;
        private float xDeg;
        private float yDeg;
        private float zDeg;
        private Quaternion fromRotation;
        private Quaternion toRotation;
        private int helfScreenWith = 4;
        private int helfScreenHeight = 4;
        private float scroll = 0.0f;
        private GameObject controlModel;
        private ViewPlayerControlCameraMode cameraMode;
        private Camera defaultMainCamera;
        private CameraMan cameraMainScript;


        void Start()
        {
            controlType = ViewPlayerControlDragType.VPCTRotate;
            //helfScreenWith = Screen.width / 2;
            //helfScreenHeight = Screen.height / 2;

            EventTrigger trigger = dragArea.GetComponent<EventTrigger>();
            EventTrigger.Entry entry1 = new EventTrigger.Entry();
            entry1.eventID = EventTriggerType.Drag;
            entry1.callback.AddListener((data) => { OnDrag((PointerEventData)data); });
            trigger.triggers.Add(entry1);
            EventTrigger.Entry entry2 = new EventTrigger.Entry();
            entry2.eventID = EventTriggerType.Scroll;
            entry2.callback.AddListener((data) => { OnScroll((PointerEventData)data); });
            trigger.triggers.Add(entry2);

            defaultMainCamera = Camera.main;
            cameraMainScript = defaultMainCamera.GetComponent<CameraMan>();

            if (freeCamera)
            {
                defaultMainCamera.gameObject.SetActive(false);
                CameraManager cm = freeCamera.GetComponent<CameraManager>();
                if (cm)
                {
                    cm.SetFollowPlayer(false);
                }
            }
            else
            {
                freeCamera = defaultMainCamera.gameObject;
                Debug.Log("No FreeCamera and camera need script comopnent 'CameraManager'!");
            }

            if (followCamera)
            {
                ZDSPCamera fc = followCamera.GetComponent<ZDSPCamera>();
                if (!fc)
                {
                    followCamera = null;
                    Debug.Log("No FollowCamera need script comopnent 'FollowCamera'!");
                    ChangeFreeCameraMode();
                }
                else
                {
                    CloseAllCamera();
                    followCamera.SetActive(true);
                }
            }
            else
            {
                Debug.Log("No FollowCamera and camera need script comopnent 'FollowCamera'!");
                ChangeFreeCameraMode();
            }

            gameObject.SetActive(false);

            if (toggleTarget)
            {
                toggleTarget.onValueChanged.AddListener(SelectTargetModel);
                toggleTarget.isOn = false;
                toggleTarget.enabled = false;
            }

            if (toggleMain)
            {
                toggleMain.onValueChanged.AddListener(SelectMainModel);
                toggleMain.isOn = true;
                OnChangeSelectModel(true);
            }
        }

        void Update()
        {


            return;
            /*
    #if UNITY_EDITOR
            if (isPressed)
            {
                delta.y = startPos.y - Input.mousePosition.y;
                delta.x = startPos.x - Input.mousePosition.x;
                startPos = Input.mousePosition;
                DoAvatarRotate();
                DoAvatarMove();
            }
            if (Input.GetMouseButtonDown(0))
            {
                isPressed = true;
                startPos = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(0))
            {
                isPressed = false;
            }

            scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                DoZoom();
            }
    #elif UNITY_ANDROID || UNITY_IOS
           if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                delta.y = Input.GetTouch(0).deltaPosition.y;
                delta.x = Input.GetTouch(0).deltaPosition.x;
                DoAvatarRotate();
                DoAvatarMove();
            }
    #endif*/
        }

        public void OnDrag(PointerEventData eventData)
        {
            this.delta = eventData.delta;
            DoAvatarRotate();
            DoAvatarMove();
        }

        public void OnScroll(PointerEventData eventData)
        {
            scroll = eventData.scrollDelta.y;
            DoZoom();
        }

        bool HaveAvatar()
        {
            if (!controlModel)
            {
                controlModel = ViewPlayerModelControl.Instance.GetCurrectSelectModel();
            }

            return (controlModel) ? true : false;
        }

        void DoAvatarRotate()
        {
            if (controlType != ViewPlayerControlDragType.VPCTRotate)
                return;

            if (cameraMode != ViewPlayerControlCameraMode.None)
                return;

            if (/*isPressed && */HaveAvatar())
            {
                xDeg = controlModel.transform.rotation.eulerAngles.x;
                if (delta.y > rotateDif) delta.y = 1;
                else if (delta.y < -rotateDif) delta.y = -1;
                else delta.y = 0;
                xDeg -= delta.y * rotateSpeed;

                yDeg = controlModel.transform.rotation.eulerAngles.y;
                if (delta.x > rotateDif) delta.x = 1;
                else if (delta.x < -rotateDif) delta.x = -1;
                else delta.x = 0;
                yDeg -= delta.x * rotateSpeed;
                //Debug.Log("delta.x = " + delta.x + ", delta.y = " + delta.y);
                fromRotation = controlModel.transform.rotation;
                toRotation = Quaternion.Euler(xDeg, yDeg, 0);
                controlModel.transform.rotation = Quaternion.Lerp(fromRotation, toRotation, Time.deltaTime * rotateSpeed);
            }
        }

        void DoAvatarMove()
        {
            if (controlType != ViewPlayerControlDragType.VPCTMove)
                return;

            if (cameraMode != ViewPlayerControlCameraMode.None)
                return;

            if (/*isPressed && */HaveAvatar())
            {
                xDeg = controlModel.transform.position.x;
                if (delta.x > moveDif) delta.x = 1;
                else if (delta.x < -moveDif) delta.x = -1;
                else delta.x = 0;
                xDeg += delta.x * moveSpeed;
                xDeg = Mathf.Clamp(xDeg, -helfScreenWith, helfScreenWith);

                yDeg = controlModel.transform.position.y;
                if (delta.y > moveDif) delta.y = 1;
                else if (delta.y < -moveDif) delta.y = -1;
                else delta.y = 0;
                yDeg += delta.y * moveSpeed;
                yDeg = Mathf.Clamp(yDeg, -helfScreenHeight, helfScreenHeight);
                //Debug.Log("xDeg = " + xDeg + ", yDeg = " + yDeg);
                if (xDeg != 0 || yDeg != 0)
                {
                    controlModel.transform.position = new Vector3(xDeg, yDeg, controlModel.transform.position.z);
                }
            }
        }

        void DoZoom()
        {
            if (cameraMode != ViewPlayerControlCameraMode.None)
                return;

            if (HaveAvatar())
            {
                zDeg = controlModel.transform.position.z;
                zDeg += scroll * moveSpeed;
                zDeg = Mathf.Clamp(zDeg, -9, 0);
                controlModel.transform.position = new Vector3(controlModel.transform.position.x, controlModel.transform.position.y, zDeg);
            }
        }

        public void OnModelLoaded()
        {
            controlModel = null;
            ChangeFollowCameraMode();
            SetCameraTarget();

            if (toggleTarget.enabled == false)
                toggleTarget.enabled = true;
        }

        public void OnChangeCameraTarget()
        {
            controlModel = null;
            SetCameraTarget();
        }

        void SetCameraTarget()
        {
            if (HaveAvatar()) {
                var cm = freeCamera.GetComponent<CameraManager>();
                if (cm)
                {
                    cm.setTarget(controlModel.transform);
                }
                followCamera.GetComponent<ZDSPCamera>().Init(controlModel);
            }
        }

        public void SelectMainModel(bool check)
        {
            if (check)
            {
                OnChangeSelectModel(true);
            }
        }

        public void SelectTargetModel(bool check)
        {
            if (check)
            {
                OnChangeSelectModel(false);
            }
        }

        void OnChangeSelectModel(bool is_main)
        {
            // 先換資料
            ViewPlayerModelControl.Instance.ChangeSelectModel(is_main);
            // 再廣播
            mainObject.BroadcastMessage("ChangeSelectModel", is_main, SendMessageOptions.DontRequireReceiver);
            OnChangeCameraTarget();
        }

        public void OnChangeDragMode(int mode)
        {
            controlType = (ViewPlayerControlDragType)mode;
            ChangeDragModelMode();
        }

        public void ChangeDragModelMode()
        {
            cameraMode = ViewPlayerControlCameraMode.None;
            dragArea.SetActive(true);
            if (cameraMainScript)
                cameraMainScript.enabled = false;
        }

        public void ChangeMainCameraMode()
        {
            cameraMode = ViewPlayerControlCameraMode.Main;
            dragArea.SetActive(false);
            CloseAllCamera();
            defaultMainCamera.gameObject.SetActive(true);
            if (cameraMainScript)
                cameraMainScript.enabled = true;
        }

        public void ChangeFreeCameraMode()
        {
            if (HaveAvatar())
            {
                cameraMode = ViewPlayerControlCameraMode.Follow1;
                dragArea.SetActive(false);
                CloseAllCamera();
                freeCamera.SetActive(true);
                //var cm = freeCamera.GetComponent<CameraManager>();
                //if (cm)
                //{
                //    cm.setTarget(controlModel.transform);
                //}
            }
        }

        public void ChangeFollowCameraMode()
        {
            if (followCamera && HaveAvatar())
            {
                cameraMode = ViewPlayerControlCameraMode.Follow2;
                dragArea.SetActive(false);
                CloseAllCamera();
                followCamera.SetActive(true);
                //followCamera.GetComponent<FollowCamera>().Init(controlModel);
            }
        }

        void CloseAllCamera()
        {
            if (defaultMainCamera.gameObject && defaultMainCamera.gameObject.GetActive())
                defaultMainCamera.gameObject.SetActive(false);
            if (freeCamera && freeCamera.GetActive())
                freeCamera.SetActive(false);
            if (followCamera && followCamera.GetActive())
                followCamera.SetActive(false);
        }

        void OnDestory()
        {
            mainObject = null;
            freeCamera = null;
            followCamera = null;
            dragArea = null;
            controlModel = null;
            defaultMainCamera = null;
        }
    }
}