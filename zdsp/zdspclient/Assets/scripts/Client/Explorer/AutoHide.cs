using UnityEngine;

public class AutoHide : MonoBehaviour
{
    private float mLastSetTransparentTime;
    private Shader mOldShader = null;    
    private float mTransparency = 1.0f;
    private const float mTargetTransparancy = 0.4f;
    private const float mFallOff = 0.5f; // returns to 100% in 0.5 sec

    private Renderer mRenderer;
    //private bool m_originalenabled;

    public void BeTransparent(Renderer renderer)
    {
        mRenderer = renderer;        
        if (mOldShader == null)
        {
            // Save the current shader
            mOldShader = mRenderer.material.shader;                
            mRenderer.material.shader = Shader.Find("Mobile/DiffuseAlpha");
            //m_originalenabled = m_renderer.enabled;
            //m_renderer.enabled = false;
        }

        mLastSetTransparentTime = Time.fixedTime;
    }

    void Update()
    {
        float sign = -1;
        if (Time.fixedTime > mLastSetTransparentTime + 0.5f)  //if 0.5sec no betransparent, implies player is no longer blocked by this object
            sign = 1;                

        mTransparency += sign * (1.0f - mTargetTransparancy) * Time.deltaTime / mFallOff; //will always try to go to opaque or transparent in falloff time

        if (mTransparency < mTargetTransparancy)
            mTransparency = mTargetTransparancy;

        if (mTransparency < 1.0f)
        {
            Color color = mRenderer.material.color;
            color.a = mTransparency;
            mRenderer.material.color = color;
        }
        else
        {
            // Reset the shader
            mRenderer.material.shader = mOldShader;
            // And remove this script
            //mRenderer.enabled = mOriginalenabled;
            Destroy(this);
        }

    }
}
