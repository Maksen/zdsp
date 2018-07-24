using CnControls;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class ZDSPJoystick : SimpleJoystick
{
    [SerializeField] RectTransform Blocker;
    [SerializeField] float fadeTime = 0.2f;

    private CanvasGroup canvasGroup;
    private int controllingPointerId = -4;
    private bool isInTouchArea = false;
    private bool hasStartedDragging = false;
    private bool isActive = true;
    private bool isVisible;
    private Coroutine fadeCoroutine;
    private float fadeTimer = 0;
    private bool useBlocker = false;
    private GameObject raycastExcludeObj;
    private List<RaycastResult> raycastResult = new List<RaycastResult>();

    protected override void Awake()
    {
        HUDWidget widget = GetComponent<HUDWidget>();
        widget.SetWidgetBehaviour();
        UIManager.RegisterJoystick(widget);
        raycastExcludeObj = PointerScreen.Instance.gameObject;
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        PointerScreen.OnPointerUpEvent += OnPointerUp;
        PointerScreen.OnPointerDownEvent += OnPointerDown;
        PointerScreen.OnDragEvent += OnDrag;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        PointerScreen.OnPointerUpEvent -= OnPointerUp;
        PointerScreen.OnPointerDownEvent -= OnPointerDown;
        PointerScreen.OnDragEvent -= OnDrag;
    }

    public void SetTouchZoneFullScreen(bool value)
    {
        useBlocker = !value;
    }

    public void SetActive(bool value)
    {
        isActive = value;
        if (!isActive)
            ResetState();
    }

    public bool IsInControl()
    {
        return isVisible || CnInputManager.GetAxis("Horizontal") != 0 || CnInputManager.GetAxis("Vertical") != 0;
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (!isActive)
            return;

#if UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > 1 && !isVisible) // cannot have more than 1 touch on screen if not started
            return;
#endif
        if (PointerScreen.Instance.PointerCount > 1 && !isVisible) // cannot have more than 1 valid touch if not started
            return;

        if (isInTouchArea)  // on press must be in valid touch area
        {
            if (!isVisible)  // has passed drag threshold and not started yet
            {
                // check whether current position is over any UI
                if (!IsPointerOverUI(eventData))
                {
                    base.OnPointerDown(eventData);  // show joystick at current position, will set isVisible to true
                    controllingPointerId = eventData.pointerId;  // set the pointer that start the joystick as controlling
                }
            }
            else  // joystick is visible so can drag
            {
                if (eventData.pointerId != controllingPointerId && controllingPointerId > -4)  // ignore drag from non-controlling pointers
                    return;

                if (controllingPointerId == -4) // previous controlling pointer has lifted, take over as controlling pointer
                    controllingPointerId = eventData.pointerId;

                if (!hasStartedDragging)  // has not started dragging, need to drag a little more to start movement
                {
                    float magnitude = (eventData.position - eventData.pressPosition).magnitude;
                    if (magnitude >= EventSystem.current.pixelDragThreshold * 2)
                    {
                        hasStartedDragging = true;
                        base.OnDrag(eventData);
                    }
                }
                else
                {
                    base.OnDrag(eventData);
                }
            }
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
#if UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > 1)  // cannot have more than 1 touch on screen even if over UI object
            return;
#endif
        if (PointerScreen.Instance.PointerCount > 1)  // cannot have more than 1 valid touch
            return;

        isInTouchArea = useBlocker ? IsPointerInBlockerArea(eventData) : true;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (PointerScreen.Instance.PointerCount <= 1)  // this is the last touch on screen
        {
            isInTouchArea = false;
            hasStartedDragging = false;
            if (isVisible)
                Reset();  // will hide joystick
        }

        if (eventData.pointerId == controllingPointerId)
            controllingPointerId = -4;
    }

    protected override void Hide(bool isHidden)
    {
        isVisible = !isHidden;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        float targetAlpha = isHidden ? 0f : 1f;
        float duration = Mathf.Abs((targetAlpha - canvasGroup.alpha) * fadeTime);
        fadeTimer = 0;

        if (isHidden && canvasGroup.alpha != targetAlpha)  // fade alpha to 0 if not already 0
        {
            fadeCoroutine = StartCoroutine(Fade(canvasGroup.alpha, targetAlpha, duration));
        }
        else if (!isHidden && canvasGroup.alpha != targetAlpha) // fade alpha to 1 if not already 1
        {
            base.Hide(false);  // turn on joystick first before fading
            fadeCoroutine = StartCoroutine(Fade(canvasGroup.alpha, targetAlpha, duration));
        }
        else  // already at target alpha, so just turn joystick on/ff
        {
            base.Hide(isHidden);
        }
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        while (fadeTimer < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, fadeTimer / duration);
            fadeTimer += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = endAlpha;

        if (endAlpha == 0f)  // hiding
            base.Hide(true); // turn off joystick
    }

    public void Reset()
    {
        HorizintalAxis.Value = VerticalAxis.Value = 0f;
        Hide(true);
    }

    public void ResetState()
    {
        controllingPointerId = -1;
        isInTouchArea = false;
        hasStartedDragging = false;
        canvasGroup.alpha = 0;
        Reset();
    }

    public bool IsPointerInBlockerArea(PointerEventData eventData)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(Blocker, eventData.pressPosition, eventData.pressEventCamera);
    }

    private bool IsPointerOverUI(PointerEventData eventData)
    {
        bool result = false;
        EventSystem.current.RaycastAll(eventData, raycastResult);
        for (int i = 0; i < raycastResult.Count; i++)
        {
            if (raycastResult[i].gameObject == null || raycastResult[i].gameObject == raycastExcludeObj)
                continue;
            result = true;
        }
        raycastResult.Clear();
        return result;
    }
}