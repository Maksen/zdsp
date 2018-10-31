using UnityEngine;

public class EnvironmentController
{
    private Color mAmbientLight;
    private float mAmbientIntensity;
    private FogSettingZoomInMode mFogSettingZoomInMode;

    public EnvironmentController()
    {
        mAmbientLight = RenderSettings.ambientLight;
        mAmbientIntensity = RenderSettings.ambientIntensity;
        mFogSettingZoomInMode = GameObject.FindObjectOfType<FogSettingZoomInMode>();
    }

    public void SetUIAmbientLight(bool enable, Color lightColor, float lightIntensity)
    {
        if (enable)
        {
            RenderSettings.ambientLight = lightColor;
            RenderSettings.ambientIntensity = lightIntensity;
        }
        else
        {
            RenderSettings.ambientLight = mAmbientLight;
            RenderSettings.ambientIntensity = mAmbientIntensity;
        }
    }

    public void EnableZoomInMode(bool enable)
    {
        if (mFogSettingZoomInMode)
            mFogSettingZoomInMode.EnableZoomInMode(enable);
    }
}