using UnityEngine;
using UnityEngine.UI;

public class HUDWidget : MonoBehaviour
{
    public HUDWidgetType widgetType;
    public bool ActiveOnStartup = true;
    public Toggle toggle;

    private BaseWidgetBehaviour widgetBehaviour;
    private bool defaultToggleIsOn;

    public void RegisterWidget()
    {
        if (widgetType != HUDWidgetType.None)
        {
#if !ZEALOT_DEVELOPMENT
            if (widgetType == HUDWidgetType.ConsoleCommand)
            {
                gameObject.SetActive(false);
                return;
            }
#endif
            if (toggle != null)
                defaultToggleIsOn = toggle.isOn;
            widgetBehaviour = gameObject.GetComponent<BaseWidgetBehaviour>();
            if (ActiveOnStartup)
                OnActivate();
            else
                OnDeactivate();
            UIManager.RegisterWidget(this);
        }
    }

    public void OnActivate()
    {        
        gameObject.SetActive(true);

        if (widgetBehaviour != null)
            widgetBehaviour.hasBeenActive = true;
    }

    public void OnDeactivate()
    {
        gameObject.SetActive(false);

        if (toggle != null && toggle.isOn != defaultToggleIsOn)
            toggle.isOn = defaultToggleIsOn;
    }

    public bool IsActived()
    {
        return gameObject.activeSelf;
    }

    public void OnLevelChanged()
    {
        if (widgetBehaviour != null && widgetBehaviour.hasBeenActive)
        {
            widgetBehaviour.OnLevelChanged();
            widgetBehaviour.hasBeenActive = false;
        }
    }

    /// <summary>
    /// Use this to setup BaseWidgetBehaviour if the widget is not under the HUD hierarchy
    /// </summary>
    public void SetWidgetBehaviour()
    {
        widgetBehaviour = gameObject.GetComponent<BaseWidgetBehaviour>();
        if (toggle != null)
            defaultToggleIsOn = toggle.isOn;
        if (ActiveOnStartup)
            OnActivate();
        else
            OnDeactivate();
    }
}
