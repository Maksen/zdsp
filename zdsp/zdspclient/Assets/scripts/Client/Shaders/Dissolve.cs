using UnityEngine;

public class Dissolve : MonoBehaviour
{   
    private static readonly string NoiseTexPath = "Textures_Dissolve/sf_noise_clouds_01.png";

    private static Shader mDissolveShader;

    private Material mMaterial;
    private float mDissolveAmt;

    public void Init(SkinnedMeshRenderer renderer)
    {
        mMaterial = renderer.material;        
        Texture diffuse = (Texture) mMaterial.mainTexture;

        if (mDissolveShader == null)        
            mDissolveShader = Shader.Find("FXModel/Dissolve");
        
        mMaterial.shader = mDissolveShader;

        Texture noise = (Texture) ObjPoolMgr.Instance.GetTexture(NoiseTexPath);        
        mMaterial.SetTexture("_Noise", noise);        
        mMaterial.SetTexture("_Diffuse", diffuse);        
        mDissolveAmt = 0;
        mMaterial.SetFloat("_Dissolveamount", mDissolveAmt);
    }

    void Update()
    {        
        mDissolveAmt += Time.deltaTime; //dissolve within 1 sec
        mMaterial.SetFloat("_Dissolveamount", mDissolveAmt);
    }

    void OnDestroy()
    {
        if (mMaterial != null)
            Destroy(mMaterial);
    }
}