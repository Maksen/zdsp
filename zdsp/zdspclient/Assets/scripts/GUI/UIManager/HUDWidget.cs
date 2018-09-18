using UnityEngine;

public class HUDWidget : MonoBehaviour
{
    public HUDWidgetType widgetType;
    public bool ActiveOnStartup = true;

    private BaseWidgetBehaviour widgetBehaviour;

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
    }

    public bool IsActive()
    {
        return gameObject.activeInHierarchy;
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
        if (ActiveOnStartup)
            OnActivate();
        else
            OnDeactivate();
    }
}