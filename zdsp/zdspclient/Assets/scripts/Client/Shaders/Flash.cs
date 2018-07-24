using UnityEngine;
using System;

public class Flash : MonoBehaviour
{ 
    private static Shader mFlashShader;
    private static readonly float MaxFlashAmt = 1.0f;
    private static float FlashDuration = 0.2f;

    private Material mMaterial;
    private float mFlashAmt;
    private Shader originshader; 

    public void Init(SkinnedMeshRenderer renderer)
    {
        mMaterial = renderer.material;
        Texture diffuse = (Texture)mMaterial.mainTexture;
        originshader =  mMaterial.shader;
        //if (mMaterial.shader.name == "Mobile/Diffuse")
        {
            if (mFlashShader == null)
                mFlashShader = Shader.Find("Mobile/Diffuse_Flash");

            mMaterial.shader = mFlashShader;
            mMaterial.SetTexture("_MainTex", diffuse);
            mMaterial.SetVector("_FlashColor", new Vector4(1, 1, 1, 1));
        }
    }

    public void SetDuration(float newDuration)
    {
        FlashDuration = newDuration;
    }

    public void Revert()
    {
        if(mFlashShader != null)
        {
            mMaterial.shader = originshader;
            mFlashShader = null;
        }
    }

    private float ComputeIntensity()
    {
        //0 to 1
        float ratio =  Math.Min(mFlashAmt / FlashDuration, 1.0f);

        if (ratio <= 0.5f)
            return ratio * 2 * MaxFlashAmt;
        else                   
            return (1.0f - ratio) * 2 * MaxFlashAmt;
    }

    void Update()
    {       
        mFlashAmt += Time.deltaTime; //animate within 1 sec (0 to 1sec)        

        float intensity = ComputeIntensity();
        mMaterial.SetFloat("_FlashIntensity", intensity);

        if (mFlashAmt >= FlashDuration)                    
            enabled = false;
    }

    private void Reset()
    {
        mFlashAmt = 0;
        float intensity = ComputeIntensity();
        mMaterial.SetFloat("_FlashIntensity", intensity);
    }

    public void Restart()
    {
        Reset();        
        enabled = true;
    }

    public bool IsFlashing()
    {
        return enabled;
    }

    void OnDestroy()
    {
        if (mMaterial != null)
            Destroy(mMaterial);
    }
}