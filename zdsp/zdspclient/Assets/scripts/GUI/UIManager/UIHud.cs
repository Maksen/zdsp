using UnityEngine;
using UnityEngine.UI;

public class UIHud : MonoBehaviour
{
    private Camera renderCamera;
    private GraphicRaycaster raycaster;
    private int hudLayer;

    void Awake()
    {
        hudLayer = LayerMask.NameToLayer("UI_HUD");
        renderCamera = GetComponent<Canvas>().worldCamera;
        raycaster = GetComponent<GraphicRaycaster>();

        UIManager.RegisterHUD(this);
        var widgetComponents = gameObject.GetComponentsInChildren<HUDWidget>(true);
        for (int i = 0; i < widgetComponents.Length; i++)
            widgetComponents[i].RegisterWidget();
    }

    public void ShowHUD()
    {
        renderCamera.cullingMask |= 1 << hudLayer;
        raycaster.enabled = true;
    }

    public void HideHUD()
    {
        renderCamera.cullingMask &= ~(1 << hudLayer);
        raycaster.enabled = false;
    }

    public bool IsVisible()
    {
        return (renderCamera.cullingMask & (1 << hudLayer)) != 0;
    }
}
