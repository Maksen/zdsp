using UnityEngine;
using UnityEngine.EventSystems;
using Zealot.Common.Actions;
using Zealot.Client.Actions;
using Zealot.Client.Entities;
using Zealot.Common.Entities;
using CnControls;
using System.Collections.Generic;
using System;

public class PlayerInput : MonoBehaviour
{
    private PlayerGhost mPlayerGhost;

    private ZDSPJoystick joystick;
    private Vector3 moveDirection;
    private Vector3 moveStartPos;
    const float bufferDist = 4;
    const float sqrBufferDist = (bufferDist - 1) * (bufferDist - 1);

    //private HUD_Skills SkillButtons;

    private int PickableLayerMask;
    private GameObject moveIndicator = null;
    private int pointerId;
    private ZDSPCamera mainCam;
    private RayHitComparer rayHitComparer;

    void Awake()
    {
        PointerScreen.OnPointerDownEvent += OnPointerDown;
        PointerScreen.OnPointerClickEvent += OnPointerClick;
        rayHitComparer = new RayHitComparer();
    }

    private void OnDestroy()
    {
        PointerScreen.OnPointerDownEvent -= OnPointerDown;
        PointerScreen.OnPointerClickEvent -= OnPointerClick;
    }

    public void Init(PlayerGhost p)
    {
        mPlayerGhost = p;
        if (mainCam == null)
        {
            mainCam = GameInfo.gCombat.PlayerCamera.GetComponent<ZDSPCamera>();
            joystick = UIManager.GetWidget(HUDWidgetType.Joystick).GetComponent<ZDSPJoystick>();

            //SkillButtons = UIManager.GetWidget(HUDWidgetType.SkillButtons).GetComponent<HUD_Skills>();
            //SkillButtons.Init();

            PickableLayerMask = LayerMask.GetMask("Entities", "Destructible", "Nav_Walkable", "Boss");
        }
    }

    void Reset()
    {
        moveDirection = Vector3.zero;
        moveStartPos = Vector3.zero;
    }

    public bool IsControlling()
    {
        return CnInputManager.GetAxis("Horizontal") != 0 || CnInputManager.GetAxis("Vertical") != 0
            || GameInfo.gBasicAttackState.mBasicAttackButtonDown;  // last one still need?
    }

    private bool CanKeyboardMove()
    {
        var chatWindow = UIManager.GetWidget(HUDWidgetType.Chatroom);
        if ((chatWindow != null && chatWindow.activeInHierarchy) || UIManager.IsAnyWindowOpened() || UIManager.IsAnyDialogOpened())
            return false;
        else
            return true;
    }

    public void SetMoveIndicator(Vector3 position)
    {
        if (moveIndicator == null)
            moveIndicator = ObjPoolMgr.Instance.GetObject(OBJTYPE.MODEL, "Effects_ZDSP_Indicators_prefab/moveIndicator.prefab", true);

        if (position == Vector3.zero)
        {
            moveIndicator.SetActive(false);
        }
        else
        {
            moveIndicator.transform.position = position;
            moveIndicator.SetActive(false);
            moveIndicator.SetActive(true);
        }
    }

    private void OnPointerDown(PointerEventData eventData)
    {
        if (GUIUtility.hotControl != 0)  // to be removed later after remove GUI buttons
        {
            pointerId = -1;
            return;
        }

        if (PointerScreen.Instance.PointerCount == 1) // first pointer on screen
            pointerId = -4;  // reset
        else
            pointerId = eventData.pointerId;
    }

    private void OnPointerClick(PointerEventData eventData)
    {
        if (mainCam == null || mainCam.cameraMode == ZDSPCamera.CameraMode.Orbit)
            return;

#if UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > 1)  // cannot have more than 1 touch on screen even if over UI object
            return;
#endif
        if (PointerScreen.Instance.PointerCount == 0) // no more pointer on screen (last pointer is removed when pointer up)
        {
            // this pointer is same as the first pointer on screen and has not moved past drag threshold
            if (pointerId == -4 && !eventData.dragging)
            {
                Ray ray = mainCam.GetComponent<Camera>().ScreenPointToRay(eventData.position);
                RaycastHit[] hits;
                hits = Physics.RaycastAll(ray, 100f, PickableLayerMask);
                Array.Sort(hits, rayHitComparer);  // sort the hits by distance
                for (int i = 0; i < hits.Length; i++)
                {
                    GameObjectToEntityRef entityRef = hits[i].collider.GetComponent<GameObjectToEntityRef>();
                    if (entityRef)
                    {                        
                        //Debug.Log("click on entity :" + entityRef.ToString());
                        BaseClientEntity entity = entityRef.mParentEntity;
                        if (entity.CanSelect)
                        {
                            if (isWaitingForTargetPos)
                            {
                                _selectCallback(entity.Position);
                            }
                            else
                            {
                                entity.Interact();
                            }

                            //if (entity.IsActor())
                            //    Debug.Log("click on entity :" + ((ActorGhost)entity).GetPersistentID());

                            GameInfo.gCombat.OnSelectEntity(entity);
                            break;  // can select entity, so ignore remaining hits, else continue to check other hits
                        }
                    }
                    else  // click on ground
                    {
                        if (isWaitingForTargetPos)
                        {
                            _selectCallback(hits[i].point);
                        }
                        else if (mPlayerGhost.CanMove())
                        {
                            SetMoveIndicator(hits[i].point);
                            mPlayerGhost.Bot.StopBot();
                            mPlayerGhost.ActionInterupted();
                            if (mPlayerGhost.IsInParty())
                                mPlayerGhost.PartyStats.PauseAutoFollow();
                            mPlayerGhost.PathFindToTarget(hits[i].point, -1, 0, false, false, () =>
                            {
                                SetMoveIndicator(Vector3.zero);
                                if (mPlayerGhost.IsInParty())
                                    mPlayerGhost.PartyStats.ResumeAutoFollow();
                            });
                        }
                        break;
                    }
                }
            }
        }
    }

    // Comparer for checking distances in raycast hits
    public class RayHitComparer : IComparer<RaycastHit>
    {
        public int Compare(RaycastHit x, RaycastHit y)
        {
            return x.distance.CompareTo(y.distance);
        }
    }

    void Update()
    {
#if UNITY_ANDROID || UNITY_EDITOR || UNITY_STANDALONE
        CheckBackButtonCloseWindow();
#endif

        if (mPlayerGhost == null || !mPlayerGhost.IsAlive() || mPlayerGhost.IsStun())
            return;

        ActionCommand currCommand = mPlayerGhost.GetAction().mdbCommand;

        float moveX = CnInputManager.GetAxis("Horizontal");
        float moveY = CnInputManager.GetAxis("Vertical");
#if UNITY_EDITOR || UNITY_STANDALONE
        if (!CanKeyboardMove())  // prevent moving player when typing in chat or any window
            moveX = moveY = 0;
#endif
        if (moveX != 0.0f || moveY != 0.0f)
        {
            SetMoveIndicator(Vector3.zero);
            Vector3 dir = new Vector3(moveX, 0, moveY);
            dir = GameInfo.gCombat.PlayerCamera.transform.TransformDirection(dir);
            dir.y = 0;
            dir.Normalize();

            if (currCommand.GetActionType() == ACTIONTYPE.IDLE || !dir.Equals(moveDirection)
                || Vector3.SqrMagnitude(moveStartPos - mPlayerGhost.Position) >= sqrBufferDist)
            {
                if (mPlayerGhost.CanMove())
                {
                    Vector3 targetPos = mPlayerGhost.Position + dir * bufferDist;
                    WalkActionCommand cmd = new WalkActionCommand();
                    cmd.targetPos = targetPos;
                    ClientAuthoACWalk walkAction = new ClientAuthoACWalk(mPlayerGhost, cmd);
                    mPlayerGhost.PerformAction(walkAction);
                    moveDirection = dir;
                    moveStartPos = mPlayerGhost.Position;
                    mPlayerGhost.ActionInterupted();
                }
            }
        }
        else  // not basic attack/moving joystick
        {
            Reset();

            if (currCommand.GetActionType() == ACTIONTYPE.WALK)
            {
                mPlayerGhost.Idle();
                mPlayerGhost.Bot.ClearRouter();
                if (mPlayerGhost.IsInParty())
                    mPlayerGhost.PartyStats.ResumeAutoFollow();
            }
        }

        //CheckSelectedEntityRelevance();
    }

    private void CheckSelectedEntityRelevance()
    {
        Entity entity = GameInfo.gSelectedEntity;
        if (entity == null)
            return;

        PlayerGhost localplayer = GameInfo.gLocalPlayer;
        if (localplayer == null)
            return;

        if ((entity.Position - localplayer.Position).sqrMagnitude > 196 + entity.Radius * entity.Radius) //14m + entity radius
            GameInfo.gCombat.OnSelectEntity(null);
    }

    public void ResetJoystick()
    {
        joystick.ResetState();
        Reset();
        SetMoveIndicator(Vector3.zero);
    }

    private void CheckBackButtonCloseWindow()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (UIManager.IsAnyDialogOpened())
            {
                UIManager.CloseTopDialog();
            }
            else if (UIManager.IsAnyWindowOpened())
            {
                UIManager.CloseTopWindow();
            }
            else
            {
#if (UNITY_ANDROID || UNITY_STANDALONE) && !UNITY_EDITOR
                //GameObject obj = UIManager.GetWindowGameObject(WindowType.DialogSetting);
                //if (obj != null)
                //{
                //    UI_Settings uiSettings = obj.GetComponent<UI_Settings>();
                //    uiSettings.OnClickQuitGame();
                //}
                Application.Quit();
#endif
            }
        }
    }


    #region skill target handling
    private bool isWaitingForTargetPos = false;

    public delegate void OnTargetSelected(Vector3 pos);

    private OnTargetSelected _selectCallback;
    public void ListenForPos(OnTargetSelected selectCallback, bool needpos)
    {
        isWaitingForTargetPos = true;
        _selectCallback = selectCallback;
        _selectCallback += (Vector3 pos) =>
        {
            isWaitingForTargetPos = false;
        };
    }
    #endregion
}
