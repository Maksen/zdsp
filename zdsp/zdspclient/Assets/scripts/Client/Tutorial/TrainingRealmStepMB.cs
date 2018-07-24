using UnityEngine;

public class TrainingRealmStepMB : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    private HUDWidget hudwidget;
    void Awake()
    {
        hudwidget = gameObject.GetComponent<HUDWidget>();
    }

    public void OnStep(int step)
    {
        if (hudwidget == null)
            return;
        //if (hudwidget.widgetType ==HUDWidgetType.PlayerLabel
        //    || hudwidget.widgetType == HUDWidgetType.ComboHit
        //    || hudwidget.widgetType == HUDWidgetType.DamageLabel)
        //{
        //    gameObject.SetActive(true);
        //    return;
        //}
        //if ((step >= 1 && hudwidget.widgetType == HUDWidgetType.Portrait)
        //    ||(step >= 2 && hudwidget.widgetType == HUDWidgetType.QuestList)
        //    ||(step >= 3 && hudwidget.widgetType == HUDWidgetType.Joystick)
        //    || (step >= 4 && hudwidget.widgetType == HUDWidgetType.SkillButtons)
        //    )
        //{
        //    gameObject.SetActive(true);
        //    if (hudwidget.widgetType == HUDWidgetType.SkillButtons)
        //    {
        //        TrainingSkillButtonStepMB mb =hudwidget.GetComponent<TrainingSkillButtonStepMB>();
        //        mb.OnStep(step);
        //    }
        //}else
        //{
        //    gameObject.SetActive(false);
        //}
    }
}
