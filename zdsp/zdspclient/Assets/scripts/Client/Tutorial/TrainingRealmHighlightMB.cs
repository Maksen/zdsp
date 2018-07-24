using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class TrainingRealmHighlightMB : MonoBehaviour, IPointerDownHandler {

    public GameObject root;
    public GameObject step1;
    public GameObject step2;
    public GameObject step3;
    public GameObject step4;
    public GameObject step5; 
    // Use this for initialization
    
     
    void Awake()
    {
         
    }

    public void OnCanvasClick()
    { 
    }
	 
    public void OnStep(int step)
    {        
        step1.SetActive(false);
        step2.SetActive(false);
        step3.SetActive(false);
        step4.SetActive(false);
        step5.SetActive(false); 
        //hight ui step index different 
        if (step == 2 || step == 3)
        {
            step1.SetActive(true);
        }
        else if(step == 4)
        {
            step2.SetActive(true);
        }
        else if(step == 5)
        {
            step3.SetActive(true);
        }
        else if(step ==6)
        {
            step4.SetActive(true);
        }else if (step == 7)
        {
            step5.SetActive(true);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnCanvasClick();
    }
}
