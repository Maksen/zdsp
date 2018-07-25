using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;



public class UI_ToggleDOTween : MonoBehaviour 
{

    public Toggle MyToggle;
    public DOTweenAnimation MyTween;


    void Start()
    {
        MyToggle.onValueChanged.AddListener(OnValueChanged);
    }


    void OnValueChanged(bool check)
    {

        if (check)
        {

            MyTween.DOPlayForward();

        }
        else
        {
            MyTween.DOPlayBackwards();

        }

    }       
    

    
}

   