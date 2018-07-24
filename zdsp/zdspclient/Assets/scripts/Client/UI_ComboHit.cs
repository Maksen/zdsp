using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_ComboHit : MonoBehaviour
{
    //Modified version of FPS.cs
    //Right Click on Hierarchy, ZDGUI/FPS
    //This will add the FPS prefab, sorting layer in very very high, should be always on top.
     [SerializeField]
    UnityEngine.UI.Text hitnumberText; 
    Animator theanimator;
    // Use this for initialization
    void Start()
    {
        //hitnumberText = GetComponentInChildren<UnityEngine.UI.Text>();
        theanimator = GetComponent<Animator>();
    }
    Coroutine lastCoroutine;
    public void PlayNumber(int count)
    {
        gameObject.SetActive(true);
        if (lastCoroutine != null)
            StopCoroutine(lastCoroutine);
        lastCoroutine = StartCoroutine(AnimateValue(count));
    }

    public IEnumerator AnimateValue(int val)
    {
        if (animatingCombValue >= 999)
            animatingCombValue = 998;
        int totalpops = val - animatingCombValue; 
        float speed = totalpops; 
        if (speed > 1.5f) speed = 1.5f;//current animation speed too fast, can not fast it. 
        if (theanimator == null)
        {
            theanimator = GetComponent<Animator>();
        }
        theanimator.speed = speed;
        float intval = 0.3f / speed;
        while (totalpops > 0)
        {
            totalpops--;
            
            animatingCombValue ++;
            if (hitnumberText != null)
                hitnumberText.text = animatingCombValue.ToString();
            if (theanimator != null)
            {
                theanimator.Play("ComboHits_ComboHits", -1, 0f);
            } 
            yield return new WaitForSeconds(intval);
        }
        yield return null;
    } 
    protected int animatingCombValue = 0; 

    public void ResetNumberOnTimeout()
    {
        animatingCombValue = 0;
    }
}
