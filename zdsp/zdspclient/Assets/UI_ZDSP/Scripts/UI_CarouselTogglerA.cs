using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(ScrollRect))]
public class UI_CarouselTogglerA : MonoBehaviour, IInitializePotentialDragHandler, IEndDragHandler
{
    public bool snap = true;
    public bool nonStick = false;
    [Range(0.001f, 2f)]
    public float stickyFactor = 1;
    public bool reveal = false;
    public bool centerOnStartup = true;
    public float snapSpeed = 10;
    public bool inertia = false;
    public float inertialDamping = 1F;
    public float inertiaStopThreshold = 0.1F;
    public bool horizontalWrap = false;
    public bool verticalWrap = false;

    public Button rewardButton;

    Vector2 wrapCount = new Vector2(0, 0);
    RectTransform contentRectTransform;
    Vector2 targetPosition;
    Vector2 startingPosition;
    Vector2 lastPos = new Vector2(0, 0);
    Vector2 secondLastPos = new Vector2(0, 0);
    Vector2 inertiaSpeed = new Vector2(0, 0);
    Vector2 moveToInertiaSpeed = new Vector2(0, 0);
    float currentProgress = 0;
    Button targetButton;
    bool movingToPosition = false;
    bool sliding = false;
    bool movingToReward = false;
    public bool moving
    {
        get { return sliding || movingToPosition || movingToReward; }
        private set { }
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        movingToPosition = false;
        sliding = false;
        wrapCount = new Vector2(0, 0);
        if (reveal)
        {
            Mask mask = gameObject.GetComponent<Mask>();
            mask.enabled = false;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (reveal)
        {
            Mask mask = gameObject.GetComponent<Mask>();
            mask.enabled = true;
        }
        if (inertia)
        {
            sliding = true;
        }
        else
        {
            if (snap)
            {
                //SnapToClosest();
            }
        }
    }

    void Awake()
    {
        ScrollRect scrollRect = gameObject.GetComponent<ScrollRect>();
        contentRectTransform = scrollRect.content;
    }

    void OnEnable()
    {
        if (centerOnStartup)
        {
            //CenterOnToggled();
            //currentProgress = 1; // jump to destination
        }
    }

    void LateUpdate()
    {
        ScrollRect scrollRect = gameObject.GetComponent<ScrollRect>();

        //if(movingToReward == false)
        //{
            inertiaSpeed = (lastPos - secondLastPos + wrapCount);
            inertiaSpeed /= Time.deltaTime;
        //}

        if (sliding)
        {

            if (inertiaSpeed.magnitude < inertiaStopThreshold && movingToReward == false)
            {
                moveToInertiaSpeed = inertiaSpeed;
                sliding = false;
                if (snap)
                {
                    SnapToClosest();
                }
            }
            else if (inertiaSpeed.magnitude < inertiaStopThreshold && movingToReward == true)
            {
                scrollRect.normalizedPosition += moveToInertiaSpeed * Time.deltaTime;
                if (snap)
                {
                    SnapToClosest();
                }
            }
            else if (inertiaSpeed.magnitude >= inertiaStopThreshold && movingToReward == false)
            {
                float updatedMagnitude = inertiaSpeed.magnitude - (Time.deltaTime * inertialDamping * inertialDamping);
                if (updatedMagnitude < 0)
                {

                    updatedMagnitude = 0;
                }
                inertiaSpeed = inertiaSpeed.normalized * updatedMagnitude;
                scrollRect.normalizedPosition += inertiaSpeed * Time.deltaTime;
            }
        }
        else
        {
            if (movingToPosition)
            {
                float motionPercent = snapSpeed * (1.1f - currentProgress); //1.1 -> non asymptotic approach
                motionPercent *= Time.deltaTime;
                currentProgress += motionPercent;
                if (currentProgress > 1)
                {
                    currentProgress = 1; // catch rounding and timing errors
                }
                contentRectTransform.anchoredPosition =
                    startingPosition + (currentProgress * (targetPosition - startingPosition));
                if (currentProgress == 1)
                {
                    movingToPosition = false;
                    //targetToggle.isOn = true;
                }
            }
        }

        if (horizontalWrap || verticalWrap)
        {
            if (horizontalWrap)
            {
                wrapCount.x = 0;
                if (scrollRect.horizontalNormalizedPosition < 0)
                {
                    wrapCount.x = -1 + (int)scrollRect.horizontalNormalizedPosition;
                }

                if (scrollRect.horizontalNormalizedPosition >= 1)
                {
                    wrapCount.x = (int)scrollRect.horizontalNormalizedPosition;
                }
            }
            if (verticalWrap)
            {
                wrapCount.y = 0;
                if (scrollRect.verticalNormalizedPosition < 0)
                {
                    wrapCount.y = -1 + (int)scrollRect.verticalNormalizedPosition;
                }

                if (scrollRect.verticalNormalizedPosition >= 1)
                {
                    wrapCount.y = (int)scrollRect.verticalNormalizedPosition;
                }
            }
            scrollRect.normalizedPosition = scrollRect.normalizedPosition - wrapCount;
        }
        secondLastPos = lastPos + wrapCount;
        lastPos = scrollRect.normalizedPosition;
    }

    public void SnapToClosest()
    {
        Button[] buttons = contentRectTransform.GetComponentsInChildren<Button>();
        if (buttons.Length > 0)
        {
            Button buttonClosest = findClosestButton(buttons);
            if(buttonClosest == rewardButton)
            {
                CenterButtonOnRect(buttonClosest);
                movingToReward = false;
            }
            else
            {
                sliding = true;
                movingToReward = true;
            }
        }
    }

    //public void CenterOnToggled()
    //{
    //    Toggle[] toggles = contentRectTransform.GetComponentsInChildren<Toggle>();
    //    Toggle onToggle = null;
    //    for (int i = 0; !onToggle && (i < toggles.Length); ++i)
    //    {
    //        if (toggles[i].isOn)
    //        {
    //            onToggle = toggles[i];
    //        }
    //    }
    //    if (onToggle)
    //    { // at least one item is already toggled, focus it
    //        CenterToggleOnRect(onToggle);
    //    }
    //}

    //public void CenterOnToggledIndex(int index)
    //{
    //    Toggle[] toggles = contentRectTransform.GetComponentsInChildren<Toggle>();
    //    Toggle onToggle = null;
    //    if (toggles.Length > 0)
    //    {
    //        onToggle = toggles[index];
    //    }
    //    if (onToggle)
    //    { // at least one item is already toggled, focus it
    //        CenterToggleOnRect(onToggle);
    //    }
    //}

    Button findClosestButton(Button[] buttons)
    { // closest to scrollRect center
        RectTransform buttonRectTransform;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Button buttonClosest = buttons[0];
        Vector3 diff, minDiff;
        float distance, minDistance;
        buttonRectTransform = buttonClosest.GetComponent<RectTransform>();
        minDiff = rectTransform.position - buttonRectTransform.position;
        minDistance = minDiff.magnitude;
        if (buttonClosest.IsActive())
        {
            minDistance /= stickyFactor;
            if (nonStick && buttons.Length > 2)
            {
                buttonClosest = buttons[1];
            }
        }
        foreach (Button button in buttons)
        {
            buttonRectTransform = button.GetComponent<RectTransform>();
            diff = rectTransform.position - buttonRectTransform.position;
            distance = diff.magnitude;
            if (button.IsActive())
            {
                distance /= stickyFactor;
            }
            if (distance < minDistance)
            {
                if (!(nonStick && button.IsActive()))
                { // if nonStick and is current, skip.
                    minDiff = diff;
                    minDistance = distance;
                    buttonClosest = button;
                }
            }
        }
        return buttonClosest;
    }

    void CenterButtonOnRect(Button button)
    {
        RectTransform buttonRectTransform = button.GetComponent<RectTransform>();
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        UI_CarouselTogglerA carousel = gameObject.GetComponent<UI_CarouselTogglerA>();
        ScrollRect scrollRect = gameObject.GetComponent<ScrollRect>();
        Vector3 diff = rectTransform.position - buttonRectTransform.position;
        diff = contentRectTransform.InverseTransformVector(diff);
        if (carousel.horizontalWrap && scrollRect.horizontal)
        {
            if (Mathf.Abs(diff[0]) > Mathf.Abs(scrollRect.content.rect.height - Mathf.Abs(diff[0]) - rectTransform.rect.height))
            {
                diff[0] = -1 * Mathf.Sign(diff[0]) * Mathf.Abs(scrollRect.content.rect.height - Mathf.Abs(diff[0]) - rectTransform.rect.height);
            }
        }
        if (carousel.verticalWrap && scrollRect.vertical)
        {
            if (Mathf.Abs(diff[1]) > Mathf.Abs(scrollRect.content.rect.height - Mathf.Abs(diff[1]) - rectTransform.rect.height))
            {
                diff[1] = -1 * Mathf.Sign(diff[1]) * Mathf.Abs(scrollRect.content.rect.height - Mathf.Abs(diff[1]) - rectTransform.rect.height);
            }
        }
        Vector2 currentPosition = contentRectTransform.anchoredPosition;
        targetPosition.Set(
            scrollRect.horizontal ? currentPosition.x + (diff[0]) : currentPosition.x,
            scrollRect.vertical ? currentPosition.y + (diff[1]) : currentPosition.y
            );
        //targetPosition.Set(currentPosition.x, targetPosition.y - 35.0f);
        startingPosition.Set(currentPosition.x, currentPosition.y);
        movingToPosition = true;
        currentProgress = 0;
        targetButton = button;
    }

    public void AddImpulse(Vector2 impulse)
    {
        if (inertia)
        {
            inertiaSpeed += impulse; // will be overwritten at update
            secondLastPos = lastPos - (inertiaSpeed * Time.deltaTime);
            sliding = true;
        }
    }

    public void Stop()
    {
        sliding = false;
    }
}