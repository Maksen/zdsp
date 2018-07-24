using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(EventTrigger))]
public class HUD_BasicAttackBtn : MonoBehaviour
{
    private Button button;
    private EventTrigger trigger;
    private Image buttonImage;

    void Awake()
    {
        button = GetComponent<Button>();
        trigger = GetComponent<EventTrigger>();
        buttonImage = GetComponent<Image>();        
    }

    public void Init()
    {
        EventTrigger.Entry downEntry = new EventTrigger.Entry();
        downEntry.eventID = EventTriggerType.PointerDown;
        downEntry.callback.AddListener(OnBasicAttackButtonDown);
        trigger.triggers.Add(downEntry);

        EventTrigger.Entry upEntry = new EventTrigger.Entry();
        upEntry.eventID = EventTriggerType.PointerUp;
        upEntry.callback.AddListener(OnBasicAttackButtonUp);
        trigger.triggers.Add(upEntry);
    }

    public void SetButtonImage(Sprite sprite)
    {
        buttonImage.sprite = sprite;
    }

    private void OnBasicAttackButtonDown(BaseEventData bed)
    {
        GameInfo.gBasicAttackState.mBasicAttackButtonDown = true;
        //if (GameInfo.gLocalPlayer.QuestStats.isTraining)
        //{
        //    TrainingRealmContoller.Instance.HideHighlightDialog(Zealot.Common.Trainingstep.KillMonster);
        //}
    }

    private void OnBasicAttackButtonUp(BaseEventData bed)
    {
        GameInfo.gBasicAttackState.mBasicAttackButtonDown = false;
    }
}
