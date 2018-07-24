using UnityEngine;

public class UIHud : MonoBehaviour
{
    private Camera renderCamera;
    private int hudLayer;

    void Awake()
    {
        hudLayer = LayerMask.NameToLayer("UI_HUD");
        renderCamera = GetComponent<Canvas>().worldCamera;

        UIManager.RegisterHUD(this);
        var widgetComponents = gameObject.GetComponentsInChildren<HUDWidget>(true);
        for (int i = 0; i < widgetComponents.Length; i++)
            widgetComponents[i].RegisterWidget();
    }

    public void ShowHUD()
    {
        renderCamera.cullingMask |= 1 << hudLayer;
    }

    public void HideHUD()
    {
        renderCamera.cullingMask &= ~(1 << hudLayer);
    }

    public bool IsVisible()
    {
        return (renderCamera.cullingMask & (1 << hudLayer)) != 0;
    }

}
