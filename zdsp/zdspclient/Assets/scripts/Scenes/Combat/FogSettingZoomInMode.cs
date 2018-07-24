using UnityEngine;

public class FogSettingZoomInMode : MonoBehaviour
{
    public bool fog;
    public Color fogColor;
    public float fogDensity;
    public float fogEndDistance;
    public FogMode fogMode;
    public float fogStartDistance;

    private bool mFog;
    private Color mFogColor;
    private float mFogDensity;
    private float mFogEndDistance;
    private FogMode mFogMode;
    private float mFogStartDistance;

    void Awake()
    {
        mFog = RenderSettings.fog;
        mFogColor = RenderSettings.fogColor;
        mFogDensity = RenderSettings.fogDensity;
        mFogEndDistance = RenderSettings.fogEndDistance;
        mFogMode = RenderSettings.fogMode;
        mFogStartDistance = RenderSettings.fogStartDistance;
    }

    public void EnableZoomInMode(bool enable)
    {
        if (enable)
        {
            RenderSettings.fog = fog;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogEndDistance = fogEndDistance;
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogStartDistance = fogStartDistance;
        }
        else
        {
            RenderSettings.fog = mFog;
            RenderSettings.fogColor = mFogColor;
            RenderSettings.fogDensity = mFogDensity;
            RenderSettings.fogEndDistance = mFogEndDistance;
            RenderSettings.fogMode = mFogMode;
            RenderSettings.fogStartDistance = mFogStartDistance;
        }
    }
}