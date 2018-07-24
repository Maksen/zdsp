using UnityEngine;

/// <summary>
/// Change a material float property from one value to another over time
/// </summary>
public class MaterialFloatPropertyChanger : MonoBehaviour
{
    public string PropertyName = "";
    public float StartValue;
    public float EndValue;
    public float CompletionTime;
    public int PlayCount = 1;

    private Material mMaterial;
    private float mTimeElapsed;
    private int mCurrPlayCount;

    
    void OnEnable()
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        mMaterial = renderer.material;
        mCurrPlayCount = 0;
        Reset();               
    }

    private void Reset()
    {
        mMaterial.SetFloat(PropertyName, StartValue);
        mTimeElapsed = 0;
    }

    void Update()
    {
        if (mTimeElapsed >= CompletionTime)
        {
            mCurrPlayCount++;
            if (mCurrPlayCount >= PlayCount)
            {
                enabled = false;
                return;
            }
            else
            {
                Reset();
                return;
            }
        }
        
        mTimeElapsed += Time.deltaTime;
        mTimeElapsed = Mathf.Min(mTimeElapsed, CompletionTime);
        float curr = Mathf.Lerp(StartValue, EndValue, mTimeElapsed / CompletionTime);
        mMaterial.SetFloat(PropertyName, curr);
    }

    void OnDestroy()
    {
        if (mMaterial != null)
            Destroy(mMaterial);
    }
}